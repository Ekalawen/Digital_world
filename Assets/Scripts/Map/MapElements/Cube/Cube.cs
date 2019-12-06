using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour {

    public enum CubeType { NORMAL, DEATH, INDESTRUCTIBLE };

    public CubeType type;

    [HideInInspector] public bool bIsRegular = true;

    protected virtual void Start() {
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
}
