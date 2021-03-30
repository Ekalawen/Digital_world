using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class LumiereSwitchableChild : MonoBehaviour {
    public VisualEffect lumiereHighVfx;
    public GameObject lumiereLow;
    public VisualEffect lumiereTrails;
    public GameObject pointLight;

    public virtual void SetLumiereQuality(Lumiere.LumiereQuality quality) {
        lumiereHighVfx.gameObject.SetActive(quality == Lumiere.LumiereQuality.HIGH);
        lumiereLow.SetActive(quality == Lumiere.LumiereQuality.LOW);
    }
}
