using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour {

    public enum CubeType { NORMAL, DEATH, INDESTRUCTIBLE, SPECIAL, BRISABLE, BOUNCY, TRANSPARENT };

    public CubeType type;
    public GameObject explosionParticlesPrefab;
    public bool bIsDestructible = true;
    public bool shouldRegisterToColorSources = false;

    [HideInInspector] public bool bIsRegular = true;
    protected GameManager gm;
    protected Material material;
    protected float dissolveTimeToUse = -1;

    public virtual void Start() {
        gm = GameManager.Instance;
        if (shouldRegisterToColorSources) {
            RegisterCubeToColorSources();
        }
        SetDissolveOnStart();
    }

    public void SetDissolveTime(float dissolveTime) {
        dissolveTimeToUse = dissolveTime;
    }

    protected void SetDissolveOnStart() {
        if (gm.map == null)
            return;

        if(gm.timerManager.GetElapsedTime() >= 1.0f) {
            float dissolveTime = dissolveTimeToUse != -1 ? dissolveTimeToUse : gm.postProcessManager.dissolveInGameTime;
            GetMaterial().SetFloat("_DissolveTime", dissolveTime);
            GetMaterial().SetFloat("_PlayerProximityCoef", gm.postProcessManager.dissolveInGamePlayerProximityCoef);
            StartDissolveEffect();
            return;
        }

        if (gm.map.dissolveEffectType == DissolveEffectType.REGULAR_MAP) {
            GetMaterial().SetFloat("_DissolveTime", gm.postProcessManager.dissolveRegularTime);
            GetMaterial().SetFloat("_PlayerProximityCoef", gm.postProcessManager.dissolveRegularPlayerProximityCoef);
            if (gm.map.IsInInsidedRegularMap(transform.position)) {
                StartDissolveEffect();
            } else {
                StopDissolveEffect();
            }
        } else if (gm.map.dissolveEffectType == DissolveEffectType.INFINITE_MAP) {
            GetMaterial().SetFloat("_DissolveTime", gm.postProcessManager.dissolveInfiniteTime);
            GetMaterial().SetFloat("_PlayerProximityCoef", gm.postProcessManager.dissolveInfinitePlayerProximityCoef);
            GetMaterial().SetFloat("_DissolveStartingTime", Time.time - gm.postProcessManager.dissolveInfiniteTime);
        }
    }

    protected void StartDissolveEffect() {
        GetMaterial().SetFloat("_DissolveStartingTime", Time.time);
        GetMaterial().SetVector("_PlayerPosition", gm.player.transform.position);
    }

    protected void StopDissolveEffect() {
        float dissolveTime = GetMaterial().GetFloat("_DissolveTime");
        GetMaterial().SetFloat("_DissolveStartingTime", Time.time - dissolveTime);
    }

    protected void StartDecomposeEffect(float duree) {
        GetMaterial().SetFloat("_DecomposeTime", duree);
        GetMaterial().SetFloat("_DecomposeStartingTime", Time.time);
    }

    public void SetMaterial(Material newMaterial) {
        GetComponent<MeshRenderer>().material = newMaterial;
        material = GetComponent<MeshRenderer>().material;
    }

    protected virtual void RegisterCubeToColorSources() {
        ColorManager colorManager = gm.colorManager;
        foreach(ColorSource colorSource in colorManager.sources) {
            if(Vector3.Distance(transform.position, colorSource.transform.position) <= colorSource.range)
                colorSource.AddCube(this);
        }
    }

    public void ShouldRegisterToColorSources() {
        shouldRegisterToColorSources = true;
    }

    public Color GetColor() {
        return GetMaterial().color;
    }

    public virtual void AddColor(Color addedColor) {
        GetMaterial().color += addedColor;
    }

    public virtual void SetColor(Color newColor) {
        GetMaterial().color = newColor;
    }

    public float GetLuminance() {
        return ColorManager.GetLuminance(GetMaterial().color);
    }

    public void Explode() {
        gm = GameManager.Instance; // Sinon peut y avoir un bug si on essaie de le détruire dans la même frame que lorsqu'il est crée ! (Le Start a pas le temps de s'éxécuter :'()
        GameObject go = Instantiate(explosionParticlesPrefab, transform.position, Quaternion.identity);
        go.transform.up = gm.gravityManager.Up();
        ParticleSystem particle = go.GetComponent<ParticleSystem>();
        ParticleSystemRenderer psr = go.GetComponent<ParticleSystemRenderer>();
        Material mat = psr.material;
        Material newMaterial = new Material(mat);
        newMaterial.color = GetColor();
        psr.material = newMaterial;
        float particuleTime = particle.main.duration;
        Destroy(go, particuleTime);

        Destroy();
    }

    public void Decompose(float duree) {
        gm = GameManager.Instance; // Sinon peut y avoir un bug si on essaie de le détruire dans la même frame que lorsqu'il est crée ! (Le Start a pas le temps de s'éxécuter :'()
        StartDecomposeEffect(duree);
        DestroyIn(duree);
    }

    public bool IsDecomposing() {
        float decomposeStartingTime = GetMaterial().GetFloat("_DecomposeStartingTime");
        return Time.time >= decomposeStartingTime;
    }

    public void DecomposeIn(float dureeDecompose, float timeBeforeDecompose) {
        StartCoroutine(CDecomposeIn(dureeDecompose, timeBeforeDecompose));
    }

    protected IEnumerator CDecomposeIn(float dureeDecompose, float timeBeforeDecompose) {
        yield return new WaitForSeconds(timeBeforeDecompose);
        Decompose(dureeDecompose);
    }

    protected void DestroyIn(float duree) {
        StartCoroutine(CDestroyIn(duree));
    }

    protected IEnumerator CDestroyIn(float duree) {
        yield return new WaitForSeconds(duree);
        Destroy();
    }

    public float GetExplosionParticuleTimeDuration() {
        return explosionParticlesPrefab.GetComponent<ParticleSystem>().main.duration;
    }

    public void ExplodeIn(float seconds) {
        if(gameObject.activeSelf) {
            StartCoroutine(CExplodeIn(seconds));
        } else {
            gameObject.SetActive(true);
            Destroy();
        }
    }
    public IEnumerator CExplodeIn(float seconds) {
        yield return new WaitForSeconds(seconds);
        Explode();
    }

    public void Destroy() {
        gm.map.DeleteCube(this);
    }

    public bool IsDestructible() {
        return bIsDestructible;
    }

    public virtual void InteractWithPlayer() {
    }

    public void SetOpacity(float alpha) {
        Color currentColor = GetColor();
        currentColor.a = alpha;
        SetColor(currentColor);
    }

    public Material GetMaterial() {
        if(material == null) {
            material = GetComponent<MeshRenderer>().material;
        }
        return material;
    }
}
