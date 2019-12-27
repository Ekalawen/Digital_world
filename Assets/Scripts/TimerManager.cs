using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerManager : MonoBehaviour {

    public float initialTime = 40.0f;
    public bool isInfinitTime = false;
    public Text displayText;
    public GameObject movingTextPrefab;
    public float dureeMoving = 2.0f;
    public float distanceMoving = 100.0f;
    public RectTransform textContainer;
    public float fontSize = 20;

    protected float totalTime;
    protected float debutTime;
    protected GameManager gm;
    protected float lastTimeSoundTimeOut;

    public void Initialize() {
		// Initialisation
		name = "TimerManager";
        gm = FindObjectOfType<GameManager>();
        debutTime = Time.timeSinceLevelLoad;
        totalTime = initialTime;
        lastTimeSoundTimeOut = Time.timeSinceLevelLoad;
    }

    private void Update() {
        if (gm.eventManager.IsGameOver())
            return;

        float remainingTime = GetRemainingTime();
        if(remainingTime <= 0) {
            remainingTime = 0.0f;
            gm.eventManager.LoseGame(EventManager.DeathReason.TIME_OUT);
        }
        int secondes = Mathf.FloorToInt(remainingTime);
        int centiseconds = Mathf.FloorToInt((remainingTime - secondes) * 100);
        displayText.text = secondes + ":" + centiseconds.ToString("D2");

        if(remainingTime >= 20.0f) {
            displayText.color = gm.console.allyColor;
        } else if (remainingTime >= 10.0f) {
            float avancement = (remainingTime - 10.0f) / 10.0f;
            displayText.color = avancement * gm.console.allyColor + (1.0f - avancement) * gm.console.ennemiColor;
        } else {
            displayText.color = gm.console.ennemiColor;
            float avancement = remainingTime / 10.0f;
            displayText.fontSize = (int)(fontSize * (1.0f + (1.0f - avancement)));
        }

        if(remainingTime <= 10.0f) {
            if(Time.timeSinceLevelLoad - lastTimeSoundTimeOut >= 1.0f) {
                lastTimeSoundTimeOut = Time.timeSinceLevelLoad;
                gm.soundManager.PlayTimeOutClip();
            }
        }
    }

    public float GetRemainingTime() {
        if (!isInfinitTime)
            return totalTime - (Time.timeSinceLevelLoad - debutTime);
        else
            return 99999.99f;
    }

    public void AddTime(float time) {
        if (gm.eventManager.IsGameOver())
            return;

        totalTime += time;
        Text t = Instantiate(movingTextPrefab, textContainer).GetComponent<Text>();
        t.gameObject.SetActive(true);
        int secondes = Mathf.FloorToInt(time);
        int centiseconds = Mathf.FloorToInt((time - secondes) * 100);
        t.text = (time >= 0 ? "+" : "") + secondes + ":" + centiseconds.ToString("D2");
        t.color = (time >= 0 ? gm.console.allyColor : gm.console.ennemiColor);
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
