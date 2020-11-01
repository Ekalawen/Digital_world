using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectorPathCadenas : MonoBehaviour {

    [Header("Colors")]
    public Color openColorBackground;
    public Color closedColorBackground;

    [Header("Links")]
    public SelectorPath selectorPath;
    public Image cadenaOpen;
    public Image cadenaClosed;
    public Image background;
    public Canvas canvas;

    protected SelectorManager selectorManager;

    public void Start() {
        selectorManager = SelectorManager.Instance;
        DisplayGoodCadena();
        MakeCadenaLookAtCamera();
        RegisterCameraToCanvas();
    }

    protected void RegisterCameraToCanvas() {
        canvas.worldCamera = selectorManager.camera;
    }

    protected void MakeCadenaLookAtCamera() {
        gameObject.GetComponent<LookAtTransform>().transformToLookAt = selectorManager.camera.transform;
    }

    public void DisplayGoodCadena() {
        if (selectorPath.IsUnlocked()) {
            cadenaOpen.gameObject.SetActive(true);
            cadenaClosed.gameObject.SetActive(false);
            background.color = openColorBackground;
        } else {
            cadenaOpen.gameObject.SetActive(false);
            cadenaClosed.gameObject.SetActive(true);
            background.color = closedColorBackground;
        }
    }

    public void DisplayReverseCadena() {
        if (!selectorPath.IsUnlocked()) {
            cadenaOpen.gameObject.SetActive(true);
            cadenaClosed.gameObject.SetActive(false);
            background.color = openColorBackground;
        } else {
            cadenaOpen.gameObject.SetActive(false);
            cadenaClosed.gameObject.SetActive(true);
            background.color = closedColorBackground;
        }
    }

    public void SetFocused() {
        DisplayReverseCadena();
    }

    public void SetUnfocused() {
        DisplayGoodCadena();
    }
}
