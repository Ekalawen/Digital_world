using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour {

    [HideInInspector] public bool bIsRegular = true;

    public Color GetColor() {
        return GetComponent<MeshRenderer>().material.color;
    }

    public void AddColor(Color addedColor) {
        GetComponent<MeshRenderer>().material.color += addedColor;
    }

    public void SetColor(Color newColor) {
        GetComponent<MeshRenderer>().material.color = newColor;
    }

}
