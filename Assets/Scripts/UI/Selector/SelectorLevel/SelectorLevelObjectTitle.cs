using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectorLevelObjectTitle : MonoBehaviour {

    public SelectorLevelObject objectLevel;

    public Vector2 titleSizeRange = new Vector2(1, 1.5f);
    public Vector2 canvasHeightRange = new Vector2(1, 1.6f);
    public Vector2 titleDistanceRange = new Vector2(10, 20);
    public float disableTreshold = 50.0f;
    public TMPro.TMP_Text text;
    public Image background;
    public Material materialUnlocked;
    public Material materialLocked;
    public Material materialFocused;
    public Canvas canvas;

    public void Initialize() {
        SetTitleToLevelName();
        SetUnfocused();
    }

    public void Update() {
        UpdateBackgroundScaleAndEnable();
    }

    protected void UpdateBackgroundScaleAndEnable() {
        Vector3 cameraPosition = SelectorManager.Instance.baseCamera.transform.position;
        float cameraDistanceToCenter = Vector3.Distance(Vector3.ProjectOnPlane(cameraPosition, Vector3.up), Vector3.zero);
        if(cameraDistanceToCenter > disableTreshold) {
            background.gameObject.SetActive(false);
        } else {
            background.gameObject.SetActive(true);
            float avancement = MathCurves.LinearReversed(titleDistanceRange.x, titleDistanceRange.y, cameraDistanceToCenter);
            float scale = MathCurves.Linear(titleSizeRange.x, titleSizeRange.y, avancement);
            background.transform.localScale = Vector3.one * scale;
            float canvasHeight = MathCurves.Linear(canvasHeightRange.x, canvasHeightRange.y, avancement);
            canvas.transform.localPosition = new Vector3(0, canvasHeight, 0);
        }
    }

    protected void SetTitleToLevelName() {
        text.text = objectLevel.level.GetVisibleName();
        UIHelper.FitTextHorizontally(objectLevel.level.GetVisibleName(), text);
    }

    public void SetFocused() {
        background.material = materialFocused;
        background.GetComponent<UpdateUnscaledTime>().Start();
    }

    public void SetUnfocused() {
        SetNormalColor();
    }

    protected void SetNormalColor() {
        background.material = objectLevel.level.IsAccessible() ? materialUnlocked : materialLocked;
        background.GetComponent<UpdateUnscaledTime>().Start();
    }
}
