using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoColorBouncer : MonoBehaviour {

    public float intervalTime = 0.6f;
    public float timeToBounceSize = 0.1f;
    public float inBounceTime = 0.0f;
    public float timeToNormalSize = 0.2f;
    public float startingTime = 0.0f;
    [ColorUsage(true, true)]
    public Color colorToBounceTo = Color.red;
    public bool shouldAlsoModifyEmissionColor = false;

    protected Timer timer;

    void Start() {
        timer = new Timer(intervalTime);
        timer.SetElapsedTime(startingTime);
        StartCoroutine(CBounce());
    }

    void Update() {
        if(timer.IsOver()) {
            StartCoroutine(CBounce());
            timer.Reset();
        }
    }

    protected IEnumerator CBounce() {
        Renderer renderer = GetComponent<Renderer>();
        Material material = renderer.material;
        Color startColor = material.color;
        Color startEmissionColor = Color.black;
        if (renderer != null) {
            SetColor(material, startColor, startEmissionColor, 0.0f);
            while (timer.GetElapsedTime() <= timeToBounceSize) {
                float avancement = timer.GetElapsedTime() / timeToBounceSize;
                SetColor(material, startColor, startEmissionColor, avancement);
                yield return null;
            }
            SetColor(material, startColor, startEmissionColor, 1.0f);
            while (timer.GetElapsedTime() <= timeToBounceSize + inBounceTime) {
                yield return null;
            }
            while (timer.GetElapsedTime() <= timeToBounceSize + inBounceTime + timeToNormalSize) {
                float avancement = 1.0f - (timer.GetElapsedTime() - timeToBounceSize - inBounceTime) / timeToNormalSize;
                SetColor(material, startColor, startEmissionColor, avancement);
                yield return null;
            }
            SetColor(material, startColor, startEmissionColor, 0.0f);
        } else {
            Debug.LogWarning("Un GameObject avec un AutoColorBouncer doit avoir un Renderer ! :)");
        }
    }

    protected void SetColor(Material material, Color startColor, Color startEmissionColor, float avancement) {
        if(shouldAlsoModifyEmissionColor)
            startEmissionColor = material.GetColor("_EmissionColor");
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
    }
}
