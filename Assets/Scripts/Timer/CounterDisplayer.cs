using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CounterDisplayer : MonoBehaviour {
    public Text displayText;
    public GameObject movingTextPrefab;
    public float dureeMoving = 1.0f;
    public float distanceMoving = 20.0f;
    public RectTransform textContainer;
    public int fontSize = 20;

    public void Initialize() {
    }

    public void Display(string message) {
        displayText.text = message;
    }

    public void Hide() {
        displayText.text = "";
    }

    public void SetColor(Color color) {
        displayText.color = color;
    }

    public void SetFontSize(int size) {
        displayText.fontSize = size;
    }

    public void AddVolatileText(string message, Color color) {
        Text t = Instantiate(movingTextPrefab, textContainer).GetComponent<Text>();
        t.gameObject.SetActive(true);
        t.text = message;
        t.color = color;
        StartCoroutine(MoveTextDown(t));
    }

    protected IEnumerator MoveTextDown(Text t) {
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
