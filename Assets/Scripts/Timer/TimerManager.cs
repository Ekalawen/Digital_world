﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerManager : MonoBehaviour {

    public float initialTime = 40.0f;
    public bool isInfinitTime = false;
    public bool shouldDisplayGameTimer = true;
    public int fontSize = 20;
    public float fontSizeBounceCoef = 1.2f;
    public CounterDisplayer timerDisplayer;

    protected float totalTime;
    protected float debutTime;
    protected GameManager gm;
    protected Timer soundTimeOutTimer;
    protected Timer gameTimer;

    public void Initialize() {
		// Initialisation
		name = "TimerManager";
        gm = FindObjectOfType<GameManager>();
        debutTime = Time.timeSinceLevelLoad;
        totalTime = initialTime;
        soundTimeOutTimer = new Timer(1.0f);
        gameTimer = new Timer(0.0f);
    }

    private void Update() {
        if (gm.eventManager.IsGameOver())
            return;

        if (shouldDisplayGameTimer)
            DisplayGameTimer();
        else
            HideGameTimer();
    }

    private void HideGameTimer() {
        timerDisplayer.Hide();
    }

    protected void DisplayGameTimer() {
        float remainingTime = GetRemainingTime();
        if(remainingTime <= 0) {
            remainingTime = 0.0f;
            gm.eventManager.LoseGame(EventManager.DeathReason.TIME_OUT);
        }
        timerDisplayer.Display(TimerManager.TimerToString(remainingTime));

        PlayTimeOutSound();

        if(remainingTime >= 20.0f) {
            timerDisplayer.SetColor(gm.console.allyColor);
            timerDisplayer.SetFontSize(fontSize);
        } else if (remainingTime >= 10.0f) {
            float avancement = (remainingTime - 10.0f) / 10.0f;
            timerDisplayer.SetColor(avancement * gm.console.allyColor + (1.0f - avancement) * gm.console.ennemiColor);
            timerDisplayer.SetFontSize(fontSize);
        } else {
            timerDisplayer.SetColor(gm.console.ennemiColor);
            float avancement = remainingTime / 10.0f;
            float size = fontSize * (1.0f + (1.0f - avancement));
            if (remainingTime <= 10.0f) {
                float coefBounce = 1 + (1.0f - soundTimeOutTimer.GetAvancement()) * (fontSizeBounceCoef - 1.0f);
                size *= coefBounce;
            }
            timerDisplayer.SetFontSize((int)size);
        }
    }

    public static string TimerToString(float seconds) {
        int secondes = Mathf.FloorToInt(seconds);
        int centiseconds = Mathf.FloorToInt((seconds - secondes) * 100);
        return secondes + ":" + centiseconds.ToString("D2");
    }
    public static string TimerToClearString(float time) {
        int seconds = Mathf.FloorToInt(time);
        int deciseconds = Mathf.FloorToInt((time - seconds) * 10);
        int centiseconds = Mathf.FloorToInt((time - seconds) * 100);
        if (time >= 3.0f)
            return seconds.ToString();
        if (time >= 1.0f)
            return seconds + "." + deciseconds.ToString("D1");
        else
            return seconds + "." + centiseconds.ToString("D2");
    }
    public static string TimerToClearerString(float time) {
        int seconds = Mathf.FloorToInt(time);
        int deciseconds = Mathf.FloorToInt((time - seconds) * 10);
        int centiseconds = Mathf.FloorToInt((time - seconds) * 100);
        if (time >= 2.0f)
            return seconds.ToString();
        else
            return seconds + "." + deciseconds.ToString("D1");
    }


    protected void PlayTimeOutSound() {
        if(GetRemainingTime() <= 10.0f) {
            if (GetRemainingTime() <= 1.6f)
                soundTimeOutTimer.SetDuree(0.2f);
            else if (GetRemainingTime() <= 5.0f)
                soundTimeOutTimer.SetDuree(0.5f);
            else
                soundTimeOutTimer.SetDuree(1.0f);
            if(soundTimeOutTimer.IsOver()) {
                soundTimeOutTimer.Reset();
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
        int secondes = time >= 0 ? Mathf.FloorToInt(time) : Mathf.CeilToInt(time);
        int centiseconds = Mathf.Abs(Mathf.FloorToInt((time - secondes) * 100));
        string volatileText = (time >= 0 ? "+" : "") + secondes + ":" + centiseconds.ToString("D2");
        Color volatileColor = (time >= 0 ? gm.console.allyColor : gm.console.ennemiColor);
        timerDisplayer.AddVolatileText(volatileText, volatileColor);
    }


    public float GetElapsedTime() {
        return gameTimer.GetElapsedTime();
    }

    public bool HasGameStarted() {
        return gameTimer.GetElapsedTime() >= 0.1f;
    }
}
