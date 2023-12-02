using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Localization.Settings;
using TMPro;
using UnityEngine.SceneManagement;


public class LevelProgressBar : MonoBehaviour {

    public float changeValueDuration = 0.5f;
    public GameObject holder;
    public Scrollbar scrollBar;
    public TMP_Text percentageText;
    public Vector2 percentageTextYPositions = new Vector2(15, -5);
    public TMP_Text totalText;
    public Image fillerImage;
    public GameObject onValueChangeParticlesHolder;
    public GameObject onReachMaxValueParticlesHolder;

    protected GameManager gm;
    protected int maxValue;
    protected Fluctuator valueFluctuator;
    protected bool hasPlayMaxValueParticles = false;

    public void Initialize() {
        gm = GameManager.Instance;
        InitializeProgressBar();
    }

    protected void InitializeProgressBar() {
        holder.SetActive(gm.IsIR());
        if (!gm.IsIR()) {
            return;
        }
        valueFluctuator = new Fluctuator(this, GetProgressBarValue, SetProgressBarValue);
        maxValue = gm.goalManager.GetTreshold();
        totalText.text = StringHelper.ToCreditsShortFormat(maxValue);
        fillerImage.material = new Material(fillerImage.material);
        valueFluctuator.GoTo(GetCurrentAvancement(), changeValueDuration);
        gm.GetInfiniteMap().scoreManager.onScoreChange.AddListener(v => UpdateProgressBarValue());
    }

    protected void UpdateProgressBarValue() {
        float avancement = GetCurrentAvancement();
        valueFluctuator.GoTo(avancement, changeValueDuration);
    }

    protected float GetCurrentAvancement() {
        float currentValue = gm.goalManager.GetCurrentTotalCreditScore();
        return currentValue / maxValue;
    }

    protected void SetProgressBarValue(float avancement) {
        avancement = Mathf.Min(avancement, 1.0f);
        scrollBar.size = avancement;
        percentageText.text = $"{StringHelper.ToCreditsShortFormat((long)(avancement * 100))}%";
        float textYPosition = avancement <= 0.5f ? percentageTextYPositions[0] : percentageTextYPositions[1];
        percentageText.rectTransform.anchoredPosition = new Vector2(percentageText.rectTransform.anchoredPosition.x, textYPosition);
        fillerImage.material.SetFloat("_ColorAvancement", avancement);
        PlayParticlesOnValueChange(avancement);
    }

    protected void PlayParticlesOnValueChange(float avancement) {
        PlayParticles(onValueChangeParticlesHolder);
        if(avancement >= 1.0f && !hasPlayMaxValueParticles && !gm.goalManager.IsInfiniteModeUnlocked()) {
            hasPlayMaxValueParticles = true;
            PlayParticles(onReachMaxValueParticlesHolder);
        }
    }

    protected float GetProgressBarValue() {
        return scrollBar.size;
    }

    protected void PlayParticles(GameObject particlesHolder) {
        foreach(Transform child in particlesHolder.transform) {
            ParticleSystem particleSystem = child.GetComponent<ParticleSystem>();
            if(!particleSystem) {
                continue;
            }
            particleSystem.Play();
        }
    }
}
