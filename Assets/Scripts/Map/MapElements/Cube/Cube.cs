using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour {

    public enum CubeType { NORMAL, DEATH, INDESTRUCTIBLE, SPECIAL };

    public CubeType type;
    public GameObject explosionParticlesPrefab;
    public bool bIsDestructible = true;
    public bool shouldRegisterToColorSources = false;

    [HideInInspector] public bool bIsRegular = true;
    protected GameManager gm;

    protected virtual void Start() {
        gm = GameManager.Instance;
        if (shouldRegisterToColorSources)
            RegisterCubeToColorSources();
    }

    public virtual void RegisterCubeToColorSources() {
        ColorManager colorManager = gm.colorManager;
        SetColor(Color.black);
        foreach(ColorSource colorSource in colorManager.sources) {
            if(Vector3.Distance(transform.position, colorSource.transform.position) <= colorSource.range)
                colorSource.AddCube(this);
        }
    }

    public void ShouldRegisterToColorSources() {
        shouldRegisterToColorSources = true;
    }

    public Color GetColor() {
        return GetComponent<MeshRenderer>().material.color;
    }

    public virtual void AddColor(Color addedColor) {
        GetComponent<MeshRenderer>().material.color += addedColor;
    }

    public virtual void SetColor(Color newColor) {
        GetComponent<MeshRenderer>().material.color = newColor;
    }

    public float GetLuminosity() {
        Color color = GetComponent<MeshRenderer>().material.color;
        return ColorManager.GetLuminosity(color);
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

    public void ExplodeIn(float seconds) {
        StartCoroutine(CExplodeIn(seconds));
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
}
