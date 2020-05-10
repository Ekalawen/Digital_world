using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationColorimetrique : MonoBehaviour {

    public static float frequenceRotation = 0.1f;

    public float vitesse = 1.0f;
    public bool fixSAndV = true;
    public float Sfixed = 1.0f;
    public float Vfixed = 0.5f;

    protected GameManager gm;
    protected bool useMesh;
    protected MeshRenderer meshRenderer;
    protected Light light;
    protected Timer timer;

    void Start() {
        gm = GameManager.Instance;
        meshRenderer = GetComponent<MeshRenderer>();
        light = GetComponent<Light>();
        useMesh = meshRenderer != null;
        timer = new Timer(frequenceRotation);
    }

    void Update() {
        if (timer.IsOver()) {
            Rotation();
            timer.Reset();
        }
    }

    protected void Rotation() {
        if (useMesh) {
            Color color = meshRenderer.material.color;
            float a = color.a;
            color = RotateColor(color);
            color.a = a;
            meshRenderer.material.color = color;
        } else {
            Color color = light.color;
            float a = color.a;
            color = RotateColor(color);
            color.a = a;
            light.color = color;
        }
    }

    protected Color RotateColor(Color color) {
        float H, S, V;
        Color.RGBToHSV(color, out H, out S, out V);
        H = (H + vitesse * Time.deltaTime * (1.0f / frequenceRotation)) % 1.0f;
        S = Sfixed;
        V = Vfixed;
        return Color.HSVToRGB(H, S, V);
    }
}
