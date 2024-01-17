using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectorPathCadenas : MonoBehaviour {

    [Header("Colors")]
    public Material materialOpened;
    public Material materialClosed;
    public Material materialFocused;

    [Header("Links")]
    public SelectorPath_Password selectorPath;
    public Image cadenaOpen;
    public Image cadenaClosed;
    public Image background;
    public Canvas canvas;
    public SelectorPathCadenasCollider cadenaCollider;

    protected SelectorManager selectorManager;

    public void Initialize() {
        selectorManager = SelectorManager.Instance;
        DisplayGoodCadena();
        MakeCadenaLookAtCamera();
        RegisterCameraToCanvas();
        cadenaCollider.Initialize();
    }

    protected void RegisterCameraToCanvas() {
        canvas.worldCamera = selectorManager.overlayCamera;
    }

    protected void MakeCadenaLookAtCamera() {
        gameObject.GetComponent<LookAtTransform>().transformToLookAt = selectorManager.baseCamera.transform;
    }

    public void DisplayGoodCadena() {
        if (selectorPath.IsUnlocked()) {
            cadenaOpen.gameObject.SetActive(true);
            cadenaClosed.gameObject.SetActive(false);
            background.material = materialOpened;
        } else {
            cadenaOpen.gameObject.SetActive(false);
            cadenaClosed.gameObject.SetActive(true);
            background.material = materialClosed;
        }
        background.GetComponent<UpdateUnscaledTime>().Start();
    }

    public void DisplayFocusedCadenas() {
        background.material = materialFocused;
        cadenaClosed.gameObject.SetActive(false);
        cadenaOpen.gameObject.SetActive(true);
        background.GetComponent<UpdateUnscaledTime>().Start();
    }

    public void SetFocused() {
        DisplayFocusedCadenas();
    }

    public void SetUnfocused() {
        DisplayGoodCadena();
    }
}
