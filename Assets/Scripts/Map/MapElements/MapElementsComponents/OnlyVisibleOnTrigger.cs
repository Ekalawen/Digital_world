using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MeshRendererColor {
    public MeshRenderer mesh;
    public Color initialColor;
    public Color startColor;

    public MeshRendererColor(MeshRenderer mesh, Color color) : this() {
        this.mesh = mesh;
        this.initialColor = color;
    }
}
public struct ParticleSystemColor {
    public ParticleSystem particleSystem;
    public float initialAlpha;
    public float startAlpha;

    public ParticleSystemColor(ParticleSystem particleSystem, float alpha) : this() {
        this.particleSystem = particleSystem;
        this.initialAlpha = alpha;
    }
}

public class OnlyVisibleOnTrigger : MonoBehaviour {

    // /!\ Il est très important que les meshs soient en Rendering Mode = Fade sinon ça n'aura aucun effet ! :D /!\

    public float dureeVisibilite = 1.0f;
    public float dureeFadeIn = 0.2f;
    public float dureeFadeOut = 0.2f;
    public List<MeshRenderer> initialMeshs; // Les mesh à rendre invisible
    public List<ParticleSystem> initialParticleSystems; // Les systèmes de particules à rendre invisible

    protected List<MeshRendererColor> meshs;
    protected List<ParticleSystemColor> particleSystems;
    protected Coroutine coroutineActivate = null;
    protected Coroutine coroutineFadeIn = null;
    protected Coroutine coroutineFadeOut = null;

    void Start() {
        meshs = new List<MeshRendererColor>();
        foreach (MeshRenderer mesh in initialMeshs) {
            meshs.Add(new MeshRendererColor(mesh, mesh.material.color));
        }
        particleSystems = new List<ParticleSystemColor>();
        foreach (ParticleSystem ps in initialParticleSystems) {
            particleSystems.Add(new ParticleSystemColor(ps, ps.startColor.a));
        }

        SetEverythingInvisible();
    }

    protected void SetEverythingInvisible() {
        foreach (MeshRendererColor mesh in meshs) {
            Color color = mesh.mesh.material.color;
            color.a = 0.0f;
            mesh.mesh.material.color = color;
            mesh.mesh.enabled = false;
        }
        for(int i = 0; i < particleSystems.Count; i++) {
            ParticleSystemColor psc = particleSystems[i];
            psc.particleSystem.Stop();
            for (int j = 0; j < particleSystems.Count; j++) {
                ParticleSystemColor ps = particleSystems[j];
                Color color = ps.particleSystem.startColor;
                color.a = 0.0f;
                ps.particleSystem.startColor = color;
                particleSystems[j] = ps;
            }
            particleSystems[i] = psc;
        }
    }

    protected void SetEverythingVisible() {
        foreach (MeshRendererColor mesh in meshs) {
            mesh.mesh.enabled = true;
        }
        foreach (ParticleSystemColor psc in particleSystems) {
            psc.particleSystem.Play();
        }
    }

    public void Activate() {
        if (coroutineActivate != null)
            StopCoroutine(coroutineActivate);
        if (coroutineFadeIn != null)
            StopCoroutine(coroutineFadeIn);
        if (coroutineFadeOut != null)
            StopCoroutine(coroutineFadeOut);
        coroutineActivate = StartCoroutine(CActivate());
    }

    protected IEnumerator CActivate() {
        SetEverythingVisible();

        coroutineFadeIn = StartCoroutine(CFadeInEverything());
        yield return new WaitForSeconds(dureeVisibilite - dureeFadeOut);
        coroutineFadeOut = StartCoroutine(CFadeOutEverything());
        yield return new WaitForSeconds(dureeFadeOut);

        SetEverythingInvisible();
    }

    protected void SaveCurrentColor() {
        for(int i = 0; i < meshs.Count; i++) {
            MeshRendererColor mesh = meshs[i];
            mesh.startColor = mesh.mesh.material.color;
            meshs[i] = mesh;
        }
        for(int i = 0; i < particleSystems.Count; i++) {
            ParticleSystemColor ps = particleSystems[i];
            ps.startAlpha = ps.particleSystem.startColor.a;
            particleSystems[i] = ps;
        }
    }

    protected IEnumerator CFadeInEverything() {
        Timer timer = new Timer(dureeFadeIn);
        SaveCurrentColor();
        while(!timer.IsOver()) {
            for(int i = 0; i < meshs.Count; i++) {
                MeshRendererColor mesh = meshs[i];
                Color color = mesh.mesh.material.color;
                color.a = Mathf.Lerp(mesh.startColor.a, mesh.initialColor.a, timer.GetAvancement());
                mesh.mesh.material.color = color;
            }
            for(int i = 0; i < particleSystems.Count; i++) {
                ParticleSystemColor ps = particleSystems[i];
                Color color = ps.particleSystem.startColor;
                color.a = Mathf.Lerp(ps.startAlpha, ps.initialAlpha, timer.GetAvancement());
                ps.particleSystem.startColor = color;
                particleSystems[i] = ps;
            }
            yield return null;
        }
    }

    protected IEnumerator CFadeOutEverything() {
        Timer timer = new Timer(dureeFadeIn);
        while (!timer.IsOver()) {
            for (int i = 0; i < meshs.Count; i++) {
                MeshRendererColor mesh = meshs[i];
                Color color = mesh.mesh.material.color;
                color.a = Mathf.Lerp(mesh.initialColor.a, 0.0f, timer.GetAvancement());
                mesh.mesh.material.color = color;
            }
            for (int i = 0; i < particleSystems.Count; i++) {
                ParticleSystemColor ps = particleSystems[i];
                Color color = ps.particleSystem.startColor;
                color.a = Mathf.Lerp(ps.initialAlpha, 0.0f, timer.GetAvancement());
                ps.particleSystem.startColor = color;
                particleSystems[i] = ps;
            }
            yield return null;
        }
    }
}
