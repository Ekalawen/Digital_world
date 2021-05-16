using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonBlackCube : Cube {

    public static float minColorSaturationAndValue = 0.1f;

    protected bool colorHasBeenChanged = false;

    public override void Initialize() {
        base.Initialize();
        InitColor();
    }

    protected void InitColor() {
        if (GetColor() == Color.black) {
            Color color = ColorSource.LimiteColorSaturation(Color.black, minColorSaturationAndValue);
            BothMaterialsSetColor("_Color", color);
        }
    }

    protected void UninitColor() {
        if (!colorHasBeenChanged) {
            BothMaterialsSetColor("_Color", Color.black);
            colorHasBeenChanged = true;
        }
    }

    public override void AddColor(Color addedColor) {
        UninitColor();
        Color color = ColorSource.LimiteColorSaturation(GetColor() + addedColor, minColorSaturationAndValue);
        BothMaterialsSetColor("_Color", color);
    }

    public override void SetColor(Color newColor) {
        UninitColor();
        Color color = ColorSource.LimiteColorSaturation(newColor, minColorSaturationAndValue);
        base.SetColor(color);
    }
}
