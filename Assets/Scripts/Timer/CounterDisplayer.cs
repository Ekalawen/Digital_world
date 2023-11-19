using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CounterDisplayer : MonoBehaviour {

    public TMP_Text displayText;
    public GameObject movingTextPrefab;
    public float dureeMoving = 1.0f;
    public float distanceMoving = 20.0f;
    public RectTransform textContainer;
    public Image imageContainer;
    public int fontSize = 20;

    public void Display(string message) {
        displayText.text = message;
    }

    public void Hide() {
        displayText.text = "";
        imageContainer.gameObject.SetActive(false);
    }

    public Color GetTextColor() {
        return displayText.color;
    }

    public void SetColor(Color color) {
        displayText.color = color;
        Color hdrColor = color * 64;
        imageContainer.material.SetColor("_Color", hdrColor);
    }

    public void SetFontSize(int size) {
        displayText.fontSize = size;
    }

    public void SetBackgroundSize(float scale) {
        imageContainer.rectTransform.localScale = Vector3.one * scale;
    }

    public void AddVolatileText(string message, Color color, float downOffset = 0.0f) {
        if(!isActiveAndEnabled) {
            return;
        }
        TMP_Text t = Instantiate(movingTextPrefab, textContainer).GetComponent<TMP_Text>();
        t.rectTransform.localPosition = t.rectTransform.localPosition + Vector3.down * downOffset;
        t.gameObject.SetActive(true);
        t.text = message;
        t.color = color;
        StartCoroutine(MoveTextDown(t));
    }

    protected IEnumerator MoveTextDown(TMP_Text t) {
        float debut = Time.timeSinceLevelLoad;
        float yDebut = t.rectTransform.anchoredPosition.y - 10;
        while (Time.timeSinceLevelLoad - debut <= dureeMoving) {
            float avancement = (Time.timeSinceLevelLoad - debut) / dureeMoving;
            Vector2 pos = t.rectTransform.anchoredPosition;
            pos.y = yDebut - avancement * distanceMoving;
            t.rectTransform.anchoredPosition = pos;
            Color color = t.color;
            color.a = 1.0f - avancement;
            t.color = color;
            yield return null;
        }
        Destroy(t.gameObject);
    }

}
