using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoColorBouncer : MonoBehaviour {

    public float intervalTime = 0.6f;
    public float timeToBounceSize = 0.1f;
    public float timeToNormalSize = 0.2f;
    public Color colorToBounceTo = Color.red;
    public bool shouldAlsoModifyEmissionColor = false;

    protected Timer timer;

    void Start() {
        timer = new Timer(intervalTime);
    }

    void Update() {
        if(timer.IsOver()) {
            StartCoroutine(CBounce());
            timer.Reset();
        }
    }

    protected IEnumerator CBounce() {
        Timer toBounceTimer = new Timer(timeToBounceSize);
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null) {
            Material material = renderer.material;
            Color startColor = material.color;
            Color startEmissionColor = Color.black;
            if(shouldAlsoModifyEmissionColor)
                startEmissionColor = material.GetColor("_EmissionColor");
            while (!toBounceTimer.IsOver()) {
                float avancement = toBounceTimer.GetAvancement();
                Color currentColor = startColor;
                currentColor.r = startColor.r * (1 - avancement) + colorToBounceTo.r * avancement;
                currentColor.g = startColor.g * (1 - avancement) + colorToBounceTo.g * avancement;
                currentColor.b = startColor.b * (1 - avancement) + colorToBounceTo.b * avancement;
                material.color = currentColor;
                if (shouldAlsoModifyEmissionColor) {
                    Color currentEmissionColor = startEmissionColor;
                    currentEmissionColor.r = startEmissionColor.r * (1 - avancement) + colorToBounceTo.r * avancement;
                    currentEmissionColor.g = startEmissionColor.g * (1 - avancement) + colorToBounceTo.g * avancement;
                    currentEmissionColor.b = startEmissionColor.b * (1 - avancement) + colorToBounceTo.b * avancement;
                    material.SetColor("_EmissionColor", currentEmissionColor);
                }
                yield return null;
            }
            Timer toNormalTimer = new Timer(timeToNormalSize);
            while (!toNormalTimer.IsOver()) {
                float avancement = toNormalTimer.GetAvancement();
                Color currentColor = startColor;
                currentColor.r = startColor.r * avancement + colorToBounceTo.r * (1 - avancement);
                currentColor.g = startColor.g * avancement + colorToBounceTo.g * (1 - avancement);
                currentColor.b = startColor.b * avancement + colorToBounceTo.b * (1 - avancement);
                material.color = currentColor;
                if (shouldAlsoModifyEmissionColor) {
                    Color currentEmissionColor = startEmissionColor;
                    currentEmissionColor.r = startEmissionColor.r * avancement + colorToBounceTo.r * (1 - avancement);
                    currentEmissionColor.g = startEmissionColor.g * avancement + colorToBounceTo.g * (1 - avancement);
                    currentEmissionColor.b = startEmissionColor.b * avancement + colorToBounceTo.b * (1 - avancement);
                    material.SetColor("_EmissionColor", currentEmissionColor);
                }
                yield return null;
            }
            material.color = startColor;
        } else {
            Debug.LogWarning("Un GameObject avec un AutColorBouncer doit avoir un Renderer ! :)");
        }
    }
}
