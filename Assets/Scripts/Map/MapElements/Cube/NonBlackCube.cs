using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonBlackCube : Cube {

    public static float minColorSaturationAndValue = 0.1f;

    protected bool colorHasBeenChanged = false;

    public override void Start() {
        base.Start();
        InitColor();
    }

    protected void InitColor() {
        if (GetColor() == Color.black) {
            Color color = ColorSource.LimiteColorSaturation(Color.black, minColorSaturationAndValue);
            GetComponent<MeshRenderer>().material.color = color;
        }
    }

    protected void UninitColor() {
        if (!colorHasBeenChanged) {
            GetComponent<MeshRenderer>().material.color = Color.black;
            colorHasBeenChanged = true;
        }
    }

    public override void AddColor(Color addedColor) {
        UninitColor();
        Color color = ColorSource.LimiteColorSaturation(GetColor() + addedColor, minColorSaturationAndValue);
        GetComponent<MeshRenderer>().material.color = color;
    }

    public override void SetColor(Color newColor) {
        UninitColor();
        Color color = ColorSource.LimiteColorSaturation(newColor, minColorSaturationAndValue);
        GetComponent<MeshRenderer>().material.color = color;
    }
}
