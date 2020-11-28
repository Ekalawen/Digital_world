using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour {

    protected static Tooltip instance;

    public RectTransform background;
    public TMPro.TMP_Text text;

    public void Awake() {
        instance = this;
        gameObject.SetActive(false);
    }

    public void Update() {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent.GetComponent<RectTransform>(), 
            Input.mousePosition, 
            null, 
            out localPoint);
        transform.localPosition = localPoint;
    }

    protected void ShowProtected(string message) {
        gameObject.SetActive(true);
        text.text = message;
        background.sizeDelta = text.GetPreferredValues(message);
    }

    protected void HideProtected() {
        gameObject.SetActive(false);
    }

    public static void Show(string message) {
        instance.ShowProtected(message);
    }

    public static void Hide() {
        instance.HideProtected();
    }
}
