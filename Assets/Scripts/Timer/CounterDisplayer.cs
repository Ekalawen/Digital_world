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

    protected List<GameObject> volatilesText = new List<GameObject>();

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
        TMP_Text text = Instantiate(movingTextPrefab, textContainer).GetComponent<TMP_Text>();
        text.rectTransform.localPosition = text.rectTransform.localPosition + Vector3.down * downOffset;
        text.gameObject.SetActive(true);
        text.text = message;
        text.color = color;
        StartCoroutine(MoveTextDown(text));
    }

    protected IEnumerator MoveTextDown(TMP_Text text) {
        Timer timer = new UnpausableTimer(dureeMoving);
        float yDebut = text.rectTransform.anchoredPosition.y - 10;
        volatilesText.Add(text.gameObject);
        while (!timer.IsOver() && text) {
            float avancement = timer.GetAvancement();
            Vector2 pos = text.rectTransform.anchoredPosition;
            pos.y = yDebut - avancement * distanceMoving;
            text.rectTransform.anchoredPosition = pos;
            Color color = text.color;
            color.a = 1.0f - avancement;
            text.color = color;
            yield return null;
        }
        if (text) {
            volatilesText.Remove(text.gameObject);
            Destroy(text.gameObject);
        }
    }

    public void RemoveAllVolatilesTexts() {
        foreach(GameObject gameObject in volatilesText) {
            Destroy(gameObject);
        }
        volatilesText.Clear();
    }
}
