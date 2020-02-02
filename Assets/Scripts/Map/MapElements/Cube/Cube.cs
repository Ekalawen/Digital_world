﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour {

    public enum CubeType { NORMAL, DEATH, INDESTRUCTIBLE };

    public CubeType type;
    public GameObject explosionParticlesPrefab;

    [HideInInspector] public bool bIsRegular = true;
    protected GameManager gm;

    protected virtual void Start() {
        gm = GameManager.Instance;
    }

    public virtual void RegisterCubeToColorSources() {
        ColorManager colorManager = FindObjectOfType<ColorManager>();
        foreach(ColorSource colorSource in colorManager.sources) {
            if(Vector3.Distance(transform.position, colorSource.transform.position) <= colorSource.range)
                colorSource.AddCube(this);
        }
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
        float H, S, V;
        Color.RGBToHSV(color, out H, out S, out V);
        return V;
    }

    public void Explode() {
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
        gm.map.DeleteCube(this);
    }
}
