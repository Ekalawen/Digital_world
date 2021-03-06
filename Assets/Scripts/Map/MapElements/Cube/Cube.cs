﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour {

    public enum CubeType { NORMAL, DEATH, INDESTRUCTIBLE, SPECIAL, BRISABLE, BOUNCY, TRANSPARENT };

    public CubeType type;
    public bool bIsDestructible = true;
    public bool shouldRegisterToColorSources = false;
    public bool startAsLinky = false;
    public Material opaqueMaterial;

    protected bool bIsRegular = true;
    protected GameManager gm;
    protected Material currentMaterial;
    protected float dissolveTimeToUse = -1;
    protected LinkyCubeComponent linkyCube = null;
    protected Coroutine enableDisableCoroutine = null;
    protected Material transparentMaterial;
    protected Coroutine changeMaterialCoroutine = null;

    // Done before player initialization
    public virtual void Initialize() {
        gm = GameManager.Instance;
        InitializeMaterials();
        if (shouldRegisterToColorSources || gm.timerManager.HasGameStarted()) {
            RegisterCubeToColorSources();
        }
        if (startAsLinky) {
            SetLinky();
        }
        SetDissolveOnInitialization();
    }

    protected void InitializeMaterials() {
        transparentMaterial = GetComponent<MeshRenderer>().material;
        if (opaqueMaterial != null) {
            GetComponent<MeshRenderer>().material = opaqueMaterial;
            opaqueMaterial = GetComponent<MeshRenderer>().material;
        } else {
            Debug.LogWarning($"Le Cube {name} n'a pas de materialOpaque renseigné ! ;)");
        }
    }

    public void SetDissolveTime(float dissolveTime) {
        dissolveTimeToUse = dissolveTime;
    }

    public void SetDissolveOnInitialization() {
        if (gm.map == null)
            return;
        if(!gm.IsInitializationOver())
            return;

        if(gm.timerManager.GetElapsedTime() >= 1.0f) {
            float dissolveTime = dissolveTimeToUse != -1 ? dissolveTimeToUse : gm.postProcessManager.dissolveInGameTime;
            StartDissolveEffect(dissolveTime, gm.postProcessManager.dissolveInGamePlayerProximityCoef);
            return;
        }

        if (gm.map.dissolveEffectType == DissolveEffectType.REGULAR_MAP) {
            if (gm.map.IsInInsidedRegularMap(transform.position)) {
                StartDissolveEffect(gm.postProcessManager.dissolveRegularTime, gm.postProcessManager.dissolveRegularPlayerProximityCoef);
            } else {
                StopDissolveEffect();
            }
        } else if (gm.map.dissolveEffectType == DissolveEffectType.INFINITE_MAP) {
            StartDissolveEffect(gm.postProcessManager.dissolveInfiniteTime, gm.postProcessManager.dissolveInfinitePlayerProximityCoef);
            GetMaterial().SetFloat("_DissolveStartingTime", Time.time - gm.postProcessManager.dissolveInfiniteTime);
        }
    }

    public virtual void StartDissolveEffect(float dissolveTime, float playerProximityCoef = 0.0f) {
        SetTransparentMaterial();
        transparentMaterial.SetFloat("_DissolveTime", dissolveTime);
        transparentMaterial.SetFloat("_PlayerProximityCoef", playerProximityCoef);
        transparentMaterial.SetFloat("_DissolveStartingTime", Time.time);
        Vector3 playerPosition = gm.player.transform.position;
        transparentMaterial.SetVector("_PlayerPosition", playerPosition);
        transparentMaterial.SetFloat("_DecomposeStartingTime", 999999f); // Reinitialise Décompose Effect
        SetOpaqueAfterDissolve(dissolveTime, playerProximityCoef);
    }

    protected void SetOpaqueAfterDissolve(float dissolveTime, float playerProximityCoef) {
        float dissolveThickness = 0; // materialTransparent.GetFloat("_DissolveThickness"); // No need en fait, et puis ça évite d'avoir le problème du start ...
        float dissolveStartingTime = Time.timeSinceLevelLoad;
        float distanceCubePlayer = Vector3.Distance(gm.player.transform.position, transform.position);
        // On cherche time tq finalTime = 1
        //float finalTime = distanceCubePlayer * playerProximityCoef + Mathf.Max(time - dissolveStartingTime, 0) / dissolveTime - dissolveThickness;
        //1 = distanceCubePlayer * playerProximityCoef + Mathf.Max(time - dissolveStartingTime, 0) / dissolveTime - dissolveThickness;
        //1 = distanceCubePlayer * playerProximityCoef + (time - dissolveStartingTime) / dissolveTime - dissolveThickness;
        //1 - distanceCubePlayer * playerProximityCoef = (time - dissolveStartingTime) / dissolveTime - dissolveThickness;
        //1 - distanceCubePlayer * playerProximityCoef + dissolveThickness = (time - dissolveStartingTime) / dissolveTime;
        //(1 - distanceCubePlayer * playerProximityCoef + dissolveThickness) * dissolveTime = time - dissolveStartingTime;
        //time = (1 - distanceCubePlayer * playerProximityCoef + dissolveThickness) * dissolveTime + dissolveStartingTime;
        float timeToWait = (1 - distanceCubePlayer * playerProximityCoef + dissolveThickness) * dissolveTime + dissolveStartingTime;
        changeMaterialCoroutine = StartCoroutine(CSetOpaqueMaterialIn(timeToWait));
    }

    protected IEnumerator CSetOpaqueMaterialIn(float duree) {
        yield return new WaitForSeconds(duree);
        SetOpaqueMaterial();
    }

    protected void StopDissolveEffect() {
        SetOpaqueMaterial();
        float dissolveTime = transparentMaterial.GetFloat("_DissolveTime");
        GetMaterial().SetFloat("_DissolveStartingTime", Time.time - dissolveTime);
    }

    protected void StartDecomposeEffect(float duree) {
        SetTransparentMaterial();
        GetMaterial().SetFloat("_DecomposeTime", duree);
        GetMaterial().SetFloat("_DecomposeStartingTime", Time.time);
    }

    private void ChainAnotherDecomposeEffect(float newDuree) {
        float oldDuree = transparentMaterial.GetFloat("_DecomposeTime");
        float oldStartingTime = transparentMaterial.GetFloat("_DecomposeStartingTime");

        float time = Time.time - oldStartingTime;
        float oldPente = 1 / oldDuree;
        float decomposeAvancement = time * oldPente;
        float newPente = (1 - decomposeAvancement) / newDuree;
        float newOffset = 1 - newPente * (time + newDuree);
        float newStartingTime = oldStartingTime - newOffset / newPente;
        float newDureeSinceNewStartingTime = Time.time - newStartingTime + newDuree;

        transparentMaterial.SetFloat("_DecomposeTime", newDureeSinceNewStartingTime);
        transparentMaterial.SetFloat("_DecomposeStartingTime", newStartingTime);
    }

    public void FinishDecomposeEffect() {
        SetOpaqueMaterial();
        float decomposeTime = transparentMaterial.GetFloat("_DecomposeTime");
        GetMaterial().SetFloat("_DecomposeStartingTime", Time.time + decomposeTime);
    }

    public void SetMaterial(Material newMaterial) {
        if(changeMaterialCoroutine != null) {
            StopCoroutine(changeMaterialCoroutine);
        }
        GetComponent<MeshRenderer>().material = newMaterial;
        currentMaterial = GetComponent<MeshRenderer>().material;
    }

    public void SetTexture(Texture newTexture) {
        BothMaterialsSetTexture("_ColorTexture", newTexture);
    }

    protected void SetOpaqueMaterial() {
        if (opaqueMaterial != null) {
            SetMaterial(opaqueMaterial);
        }
    }

    protected void SetTransparentMaterial() {
        SetMaterial(transparentMaterial);
    }

    public virtual void RegisterCubeToColorSources() {
        ColorManager colorManager = gm.colorManager;
        foreach(ColorSource colorSource in colorManager.sources) {
            if (Vector3.Distance(transform.position, colorSource.transform.position) <= colorSource.range) {
                colorSource.AddCube(this);
            }
        }
    }

    public Color GetColor() {
        return GetMaterial().color;
    }

    public virtual void AddColor(Color addedColor) {
        SetColor(GetColor() + addedColor);
    }

    public virtual void SetColor(Color newColor) {
        BothMaterialsSetColor("_Color", newColor);
        BothMaterialsSetColor("_LinkyCubeColor1", newColor); // Attention ! Dans le shader on a "color * 2" à cause d'un bug, ce pourquoi ici c'est plus sombre que la vrai couleur ! ;)
    }

    public virtual void ResetColor() {
        BothMaterialsSetColor("_Color", Color.black);
        BothMaterialsSetColor("_LinkyCubeColor1", Color.black); // Attention ! Dans le shader on a "color * 2" à cause d'un bug, ce pourquoi ici c'est plus sombre que la vrai couleur ! ;)
    }

    public void ResetBeforeStoring() {
        UnSetLinky();
        ResetColor();
        startAsLinky = false;
        if (transparentMaterial != null) {
            SetMaterial(transparentMaterial);
        }
    }

    public float GetLuminance() {
        return ColorManager.GetLuminance(GetMaterial().color);
    }

    public void Explode() {
        if(IsLinky()) {
            linkyCube.LinkyExplode();
        } else {
            RealExplode();
        }
    }

    public void RealExplode() {
        gm = GameManager.Instance; // Sinon peut y avoir un bug si on essaie de le détruire dans la même frame que lorsqu'il est crée ! (Le Start a pas le temps de s'éxécuter :'()
        GameObject go = Instantiate(GetExplosionParticlesPrefab(), transform.position, Quaternion.identity);
        go.transform.up = gm.gravityManager.Up();
        ParticleSystem particle = go.GetComponent<ParticleSystem>();
        ParticleSystemRenderer psr = go.GetComponent<ParticleSystemRenderer>();
        Material mat = psr.material;
        Material newMaterial = new Material(mat);
        if (type != CubeType.BOUNCY) {
            newMaterial.color = GetColor();
        } else {
            newMaterial.color = GetMaterial().GetColor("_BounceColor");
        }
        psr.material = newMaterial;
        float particuleTime = particle.main.duration;
        Destroy(go, particuleTime);

        Destroy();
    }

    public void Decompose(float duree) {
        if(IsLinky()) {
            linkyCube.LinkyDecompose(duree);
        } else {
            RealDecompose(duree);
        }
    }

    public void RealDecompose(float duree) {
        bool isDecomposing = IsDecomposing();
        float dureeDecomposeRemaining = GetDureeDecomposeRemaining();
        if (isDecomposing && dureeDecomposeRemaining <= duree) {
            return;
        }
        gm = GameManager.Instance; // Sinon peut y avoir un bug si on essaie de le détruire dans la même frame que lorsqu'il est crée ! (Le Start a pas le temps de s'éxécuter :'()
        if (isDecomposing) {
            ChainAnotherDecomposeEffect(duree);
        } else {
            StartDecomposeEffect(duree);
        }
        DestroyIn(duree);
    }

    public bool IsDecomposing() {
        float decomposeStartingTime = transparentMaterial.GetFloat("_DecomposeStartingTime");
        return Time.time >= decomposeStartingTime;
    }

    public float GetDureeDecomposeRemaining() {
        float dureeAlreadyDecomposed = Time.time - transparentMaterial.GetFloat("_DecomposeStartingTime");
        float decomposeTime = transparentMaterial.GetFloat("_DecomposeTime");
        return decomposeTime - dureeAlreadyDecomposed;
    }


    public void DecomposeIn(float dureeDecompose, float timeBeforeDecompose) {
        StartCoroutine(CDecomposeIn(dureeDecompose, timeBeforeDecompose));
    }

    protected IEnumerator CDecomposeIn(float dureeDecompose, float timeBeforeDecompose) {
        yield return new WaitForSeconds(timeBeforeDecompose);
        Decompose(dureeDecompose);
    }

    public void RealDecomposeIn(float dureeDecompose, float timeBeforeDecompose) {
        StartCoroutine(CRealDecomposeIn(dureeDecompose, timeBeforeDecompose));
    }

    protected IEnumerator CRealDecomposeIn(float dureeDecompose, float timeBeforeDecompose) {
        yield return new WaitForSeconds(timeBeforeDecompose);
        RealDecompose(dureeDecompose);
    }

    protected void DestroyIn(float duree) {
        StartCoroutine(CDestroyIn(duree));
    }

    protected virtual IEnumerator CDestroyIn(float duree) {
        yield return new WaitForSeconds(duree);
        Destroy();
    }

    public GameObject GetExplosionParticlesPrefab() {
        return gm.postProcessManager.explosionParticlesPrefab;
    }

    public float GetExplosionParticuleTimeDuration() {
        return GetExplosionParticlesPrefab().GetComponent<ParticleSystem>().main.duration;
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

    public void RealExplodeIn(float seconds) {
        if(gameObject.activeSelf) {
            StartCoroutine(CRealExplodeIn(seconds));
        } else {
            gameObject.SetActive(true);
            Destroy();
        }
    }

    public IEnumerator CRealExplodeIn(float seconds) {
        yield return new WaitForSeconds(seconds);
        RealExplode();
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
        if(alpha == 1) {
            SetOpaqueMaterial();
        } else {
            SetTransparentMaterial();
        }
        Color currentColor = GetColor();
        currentColor.a = alpha;
        SetColor(currentColor);
    }

    public Material GetMaterial() {
        if(currentMaterial == null) {
            currentMaterial = GetComponent<MeshRenderer>().material;
        }
        return currentMaterial;
    }

    public void BothMaterialsSetFloat(string key, float value) {
        if (opaqueMaterial != null) {
            opaqueMaterial.SetFloat(key, value);
        }
        if (transparentMaterial != null) {
            transparentMaterial.SetFloat(key, value);
        }
    }

    public void BothMaterialsSetVector(string key, Vector3 vector) {
        if (opaqueMaterial != null) {
            opaqueMaterial.SetVector(key, vector);
        }
        if (transparentMaterial != null) {
            transparentMaterial.SetVector(key, vector);
        }
    }

    public void BothMaterialsSetTexture(string key, Texture texture) {
        if (opaqueMaterial != null) {
            opaqueMaterial.SetTexture(key, texture);
        }
        if (transparentMaterial != null) {
            transparentMaterial.SetTexture(key, texture);
        }
    }

    public void BothMaterialsSetColor(string key, Color color) {
        if (opaqueMaterial != null) {
            opaqueMaterial.SetColor(key, color);
        }
        if (transparentMaterial != null) {
            transparentMaterial.SetColor(key, color);
        }
    }

    public List<Vector3> GetCornerPositions() {
        Vector3 pos = transform.position;
        List<Vector3> corners = new List<Vector3>();
        corners.Add(pos + new Vector3(-0.5f, -0.5f, -0.5f));
        corners.Add(pos + new Vector3(-0.5f, -0.5f, +0.5f));
        corners.Add(pos + new Vector3(+0.5f, -0.5f, -0.5f));
        corners.Add(pos + new Vector3(+0.5f, -0.5f, +0.5f));
        corners.Add(pos + new Vector3(-0.5f, +0.5f, -0.5f));
        corners.Add(pos + new Vector3(-0.5f, +0.5f, +0.5f));
        corners.Add(pos + new Vector3(+0.5f, +0.5f, -0.5f));
        corners.Add(pos + new Vector3(+0.5f, +0.5f, +0.5f));
        return corners;
    }

    public bool IsLinky() {
        return linkyCube != null;
    }

    public bool IsSwappy() {
        return linkyCube != null && linkyCube.IsSwappy();
    }

    public void SetSwappy() {
        if (linkyCube != null) {
            linkyCube.SetSwappy();
        } else {
            Debug.LogError("Tentative de SetSwappy un cube non linky ! Grrr :p");
        }
    }

    public LinkyCubeComponent GetLinkyCubeComponent() {
        return linkyCube;
    }

    public void SetLinky(Texture2D linkyTexture = null) {
        LinkyCubeComponent linkyCubeComponent = gameObject.AddComponent<LinkyCubeComponent>();
        linkyCube = linkyCubeComponent;
        linkyCube.Initialize(this);
        BothMaterialsSetFloat("_IsLinky", 1f);
        if(linkyTexture != null) {
            BothMaterialsSetTexture("_LinkyCubeTexture", linkyTexture);
        }
    }

    public void UnSetLinky() {
        BothMaterialsSetFloat("_IsLinky", 0f);
        Destroy(linkyCube);
    }

    public void SetLinkyValue(bool value) {
        if(value) {
            SetLinky();
        } else {
            UnSetLinky();
        }
    }

    public bool IsRegular() {
        return bIsRegular;
    }

    public void SetRegularValue(bool value) {
        bIsRegular = value;
    }

    public void Disable() {
        if(IsLinky()) {
            linkyCube.LinkyDisable();
        } else {
            RealDisable();
        }
    }

    public void RealDisable() {
        SetTransparentMaterial();
        Collider collider = GetComponent<Collider>();
        collider.enabled = false;
        BothMaterialsSetFloat("_IsDisabled", 1.0f);
    }

    public bool IsEnabled() {
        return GetComponent<Collider>().enabled;
    }

    public void Enable() {
        if(IsLinky()) {
            linkyCube.LinkyEnable();
        } else {
            RealEnable();
        }
    }

    public void RealEnable() {
        SetOpaqueMaterial();
        Collider collider = GetComponent<Collider>();
        collider.enabled = true;
        BothMaterialsSetFloat("_IsDisabled", 0.0f);
        if(gm.player.DoubleCheckInteractWithCube(this)) {
            InteractWithPlayer();
        }
    }

    public void SetEnableValue(bool value) {
        if(value) {
            Enable();
        } else {
            Disable();
        }
    }

    public void RealSetEnableValue(bool value) {
        if(value) {
            RealEnable();
        } else {
            RealDisable();
        }
    }

    public void SetEnableValueIn(bool value, float duration, Vector3 impactPoint) {
        if (IsLinky()) {
            linkyCube.LinkySetEnableValueIn(value, duration, impactPoint);
        } else {
            RealSetEnableValueIn(value, duration, impactPoint);
        }
    }

    public void RealSetEnableValueIn(bool value, float duration, Vector3 impactPoint) {
        if(value == IsEnabled()) {
            return;
        }
        if(enableDisableCoroutine != null) {
            StopCoroutine(enableDisableCoroutine);
        }
        enableDisableCoroutine = StartCoroutine(CSetEnableValueIn(value, duration, impactPoint));
    }

    protected IEnumerator CSetEnableValueIn(bool value, float duration, Vector3 impactPoint) {
        float impactRadius = IsLinky() ? Vector3.Distance(impactPoint, linkyCube.GetFarestCornerFromPoint(impactPoint)) : Mathf.Sqrt(3) / 2;
        StartImpact(impactPoint, impactRadius, duration);
        yield return new WaitForSeconds(duration);
        if (this != null) {
            StopImpact();
            RealSetEnableValue(value);
        }
    }

    public void StartImpact(Vector3 impactPoint, float impactRadius, float impactDuration) {
        BothMaterialsSetFloat("_IsImpacting", 1.0f);
        BothMaterialsSetVector("_ImpactPoint", impactPoint);
        BothMaterialsSetFloat("_ImpactTime", Time.time);
        float impactSpeed = impactRadius / impactDuration;
        BothMaterialsSetFloat("_ImpactPropagationSpeed", impactSpeed);
    }

    public void StopImpact() {
        BothMaterialsSetFloat("_IsImpacting", 0.0f);
    }

}
