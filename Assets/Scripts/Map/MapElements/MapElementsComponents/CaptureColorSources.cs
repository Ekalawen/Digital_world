using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptureColorSources : MonoBehaviour {

    public bool fixValue = false;
    public float fixedValue = 0.0f;

    protected GameManager gm;
    protected bool useMesh;
    protected MeshRenderer meshRenderer;
    protected Light light;

    void Start() {
        gm = GameManager.Instance;
        meshRenderer = GetComponent<MeshRenderer>();
        light = GetComponent<Light>();
        useMesh = meshRenderer != null;
    }

    void Update() {
        Color color = gm.colorManager.GetColorForPosition(transform.position);
        if (useMesh) {
            color.a = meshRenderer.material.color.a;
            if(fixValue) color = FixeValue(color);
            meshRenderer.material.color = color;
        } else {
            color.a = light.color.a;
            if(fixValue) color = FixeValue(color);
            light.color = color;
        }
    }

    protected Color FixeValue(Color color) {
        float H, S, V;
        Color.RGBToHSV(color, out H, out S, out V);
        V = fixedValue;
        return Color.HSVToRGB(H, S, V);
    }
}
