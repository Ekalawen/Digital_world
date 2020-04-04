using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptureColorSources : MonoBehaviour {

    public bool onlyOnStart = false;
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
        CaptureColor();
    }

    void Update() {
        if (!onlyOnStart)
            CaptureColor();
    }

    protected void CaptureColor() {
        Color color = gm.colorManager.GetColorForPosition(transform.position);
        if (useMesh) {
            Debug.Log("a = " + color.a);
            if(fixValue) color = FixeValue(color);
            color.a = meshRenderer.material.color.a;
            meshRenderer.material.color = color;
        } else {
            if(fixValue) color = FixeValue(color);
            color.a = light.color.a;
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
