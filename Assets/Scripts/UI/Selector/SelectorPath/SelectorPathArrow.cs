using System;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class SelectorPathArrow : MonoBehaviour {

    public SelectorPathUnlockScreen selectorPathUnlockScreen;
    public Image arrowImage;
    public Material openMaterial;
    public Material closeMaterial;

    public void SetCorrectMaterial() {
        arrowImage.material = selectorPathUnlockScreen.selectorPath.IsUnlocked() ? openMaterial : closeMaterial;
        GetComponent<UpdateUnscaledTime>().Start();
    }
}
