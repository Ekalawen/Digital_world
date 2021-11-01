using EZCameraShake;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerManager : MonoBehaviour {

    public AnimationCurve curve = new AnimationCurve();

    [Header("Time")]
    public bool isInfinitTime = false;
    public float initialTime = 40.0f;

    [Header("ScreenShake on remaining time")]
    public float timeToStartScreenShake = 5;
    public Vector2 screenShakeMagnitudeInterval;
    public Vector2 screenShakeRoughnessInterval;

    [Header("UI")]
    public bool shouldDisplayGameTimer = true;
    public int fontSize = 20;
    public float fontSizeBounceCoef = 1.2f;
    public CounterDisplayer timerDisplayer;


    protected GameManager gm;
    protected Timer soundTimeOutTimer;
    protected Timer gameTimer; // Celui-ci est réinitialisé quand on prend un TimeResetItem !
    protected Timer realGameTimer; // On ne peut pas réinitialiser celui-ci !
    protected CameraShakeInstance cameraShakeInstance;

    public void Initialize() {
		name = "TimerManager";
        Time.fixedDeltaTime = 1.0f / 120; // FixedUpdates are really small and are permorfed really fast, so we can do a lot of them ! (120 instead of 50 :))
        gm = FindObjectOfType<GameManager>();
        gameTimer = new Timer(initialTime);
        realGameTimer = new Timer();
        soundTimeOutTimer = new Timer(1.0f);
        cameraShakeInstance = CameraShaker.Instance.StartShake(0, 0, 0);
    }

    private void Update() {
        if (gm.eventManager.IsGameOver())
            return;

        if (shouldDisplayGameTimer)
            DisplayGameTimer();
        else
            HideGameTimer();

        ScreenShakeOnRemainingTime();
        UpdateSkyboxColorBasedOnRemainingTime();
    }

    private void HideGameTimer() {
        timerDisplayer.Hide();
    }

    protected void DisplayGameTimer()
    {
        float remainingTime = GetRemainingTime();
        TestLoseGame(EventManager.DeathReason.TIME_OUT);

        PlayTimeOutSound();

        SetVisualGameTimer(remainingTime);
    }

    public float GetAvancementOnRemainingTime() {
        float remainingTime = GetRemainingTime();
        if(remainingTime >= 20) {
            return 1.0f;
        } else if (remainingTime >= 10) {
            return (remainingTime - 10) / 10;
        } else {
            return 0;
        }
    }

    public Color GetColorBasedOnRemainingTime(Color goodColor, Color badColor) {
        float avancement = GetAvancementOnRemainingTime();
        return ColorManager.InterpolateHSV(goodColor, badColor, 1 - avancement);
    }

    private void SetVisualGameTimer(float remainingTime) {
        timerDisplayer.Display(TimerManager.TimerToString(remainingTime));
        timerDisplayer.SetColor(ColorManager.InterpolateColors(gm.console.allyColor, gm.console.ennemiColor, 1 - GetAvancementOnRemainingTime()));
        if (remainingTime >= 20.0f) {
            timerDisplayer.SetFontSize(fontSize);
        } else if (remainingTime >= 10.0f) {
            float avancement = (remainingTime - 10.0f) / 10.0f;
            timerDisplayer.SetFontSize(fontSize);
        } else {
            float avancement = 1 - remainingTime / 10.0f;
            float size = fontSize * (1.0f + avancement);
            float soundTimeOutTimerAvancement = Mathf.Min(soundTimeOutTimer.GetAvancement(), 1.0f);
            float coefBounce = 1 + (1.0f - soundTimeOutTimerAvancement) * (fontSizeBounceCoef - 1.0f);
            size *= coefBounce;
            timerDisplayer.SetFontSize((int)size);
        }
    }

    protected void UpdateSkyboxColorBasedOnRemainingTime() {
        if (gm.GetMapType() == MenuLevel.LevelType.REGULAR) {
            Color color = GetColorBasedOnRemainingTime(gm.console.basicColor, gm.console.ennemiColor);
            color = gm.postProcessManager.GetSkyboxHDRColor(color);
            RenderSettings.skybox.SetColor("_RectangleColor", color);
            float avancement = MathCurves.LinearReversed(5, 20, GetRemainingTime());
            float proportion = MathCurves.Linear(gm.postProcessManager.skyboxProportionRectanglesCriticalBound, gm.postProcessManager.skyboxProportionRectangles, avancement);
            RenderSettings.skybox.SetFloat("_ProportionRectangles", proportion);
        }
    }

    public void TestLoseGame(EventManager.DeathReason reason) {
        if (GetRemainingTime() <= 0) {
            SetVisualGameTimer(0);
            gm.eventManager.LoseGame(reason);
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
            if (GetRemainingTime() <= 1.6f) {
                soundTimeOutTimer.SetDuree(0.2f);
            } else if (GetRemainingTime() <= 5.0f) {
                soundTimeOutTimer.SetDuree(0.5f);
            } else {
                soundTimeOutTimer.SetDuree(1.0f);
            }
            if (soundTimeOutTimer.IsOver()) {
                soundTimeOutTimer.Reset();
                gm.soundManager.PlayTimeOutClip();
            }
        }
    }

    public float GetRemainingTime() {
        if (!isInfinitTime) {
            float remainingTime = gameTimer.GetRemainingTime();
            return Mathf.Max(0, remainingTime);
        } else {
            return 99999.99f;
        }
    }

    public void AddTime(float time, bool showVolatileText = true) {
        if (gm.eventManager.IsGameOver())
            return;

        gameTimer.AddDuree(time);

        if (showVolatileText) {
            ShowVolatileTextOnAddTime(time);
        }
    }

    public void Add10Time() {
        AddTime(10);
    }

    public void Add100Time() {
        AddTime(100);
    }

    public void Add1000Time() {
        AddTime(1000);
    }

    public void Minus10Time() {
        RemoveTime(10, EventManager.DeathReason.TIME_OUT);
    }

    public void Minus100Time() {
        RemoveTime(100, EventManager.DeathReason.TIME_OUT);
    }

    public void Minus1000Time() {
        RemoveTime(1000, EventManager.DeathReason.TIME_OUT);
    }

    public void RemoveTime(float timeToRemove, EventManager.DeathReason reason) {
        AddTime(-timeToRemove);
        TestLoseGame(reason);
    }

    public void SetTime(float settedTime, bool showVolatileText = true) {
        float oldTotalTime = gameTimer.GetRemainingTime();
        gameTimer = new Timer(settedTime);
        if(showVolatileText) {
            ShowVolatileTextOnAddTime(settedTime - oldTotalTime);
        }
    }

    protected void ShowVolatileTextOnAddTime(float time) {
        int secondes = time >= 0 ? Mathf.FloorToInt(time) : Mathf.CeilToInt(time);
        int centiseconds = Mathf.Abs(Mathf.FloorToInt((time - secondes) * 100));
        string volatileText = (time >= 0 ? "+" : "") + secondes + ":" + centiseconds.ToString("D2");
        Color volatileColor = (time >= 0 ? gm.console.allyColor : gm.console.ennemiColor);
        timerDisplayer.AddVolatileText(volatileText, volatileColor);
    }

    public float GetElapsedTime() {
        return gameTimer.GetElapsedTime();
    }

    public float GetRealElapsedTime() {
        return realGameTimer.GetElapsedTime();
    }

    public bool HasGameStarted() {
        //return gameTimer.GetElapsedTime() >= 0.1f; // Peut-être qu'il faudra revenir à cette version si jamais on a certains bugs d'initialization ^^
        return gm.IsInitializationOver();
    }

    protected void ScreenShakeOnRemainingTime() {
        float remainingTime = GetRemainingTime();
        if (remainingTime <= timeToStartScreenShake) {
            float avancement = 1 - remainingTime / timeToStartScreenShake;
            cameraShakeInstance.Magnitude = screenShakeMagnitudeInterval[0] + avancement * (screenShakeMagnitudeInterval[1] - screenShakeMagnitudeInterval[0]);
            cameraShakeInstance.Roughness = screenShakeRoughnessInterval[0] + avancement * (screenShakeRoughnessInterval[1] - screenShakeRoughnessInterval[0]);
        } else {
            cameraShakeInstance.Magnitude = 0;
            cameraShakeInstance.Roughness = 0;
        }
    }

    public void StopScreenShake() {
        cameraShakeInstance.StartFadeOut(0);
    }
    
    public Timer GetRealGameTimer() {
        return realGameTimer;
    }
}
