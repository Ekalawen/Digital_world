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
    public GameObject onValueChangeParticlesHolderPrefab;
    public GameObject onReachMaxValueParticlesHolderPrefab;

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
        PlayParticlesOnValueChange(avancement);
    }

    protected float GetCurrentAvancement() {
        float currentValue = gm.goalManager.GetCurrentTotalCreditScore();
        return currentValue / maxValue;
    }

    protected void SetProgressBarValue(float avancement) {
        percentageText.text = $"{StringHelper.ToCreditsShortFormat((long)(avancement * 100))}%";
        avancement = Mathf.Min(avancement, 1.0f);
        scrollBar.size = avancement;
        float textYPosition = avancement <= 0.5f ? percentageTextYPositions[0] : percentageTextYPositions[1];
        percentageText.rectTransform.anchoredPosition = new Vector2(percentageText.rectTransform.anchoredPosition.x, textYPosition);
        fillerImage.material.SetFloat("_ColorAvancement", avancement);
    }

    protected void PlayParticlesOnValueChange(float avancement) {
        PlayParticles(onValueChangeParticlesHolderPrefab);
        if(avancement >= 1.0f && !hasPlayMaxValueParticles && !gm.goalManager.IsInfiniteModeUnlocked()) {
            hasPlayMaxValueParticles = true;
            PlayParticles(onReachMaxValueParticlesHolderPrefab);
        }
    }

    protected float GetProgressBarValue() {
        return scrollBar.size;
    }

    protected void PlayParticles(GameObject particlesHolderPrefab) {
        GameObject particlesHolder = Instantiate(particlesHolderPrefab, parent: fillerImage.transform);
        foreach(Transform child in particlesHolder.transform) {
            ParticleSystem particleSystem = child.GetComponent<ParticleSystem>();
            if(!particleSystem) {
                continue;
            }
            particleSystem.loop = true;
            particleSystem.Play();
            //Destroy(particleSystem.gameObject, 5.0f);
        }
        particlesHolder.transform.SetParent(gm.player.particlesCanvas.transform, true);
        //Vector2 screenPoint = percentageText.GetComponent<RectTransform>().position;
        RectTransform rect = percentageText.GetComponent<RectTransform>();
        //float x = rect.anchorMin.x * Screen.width * rect.gameObject.GetComponentInParent<Canvas>().scaleFactor;
        //float y = rect.anchorMin.y * Screen.height * rect.gameObject.GetComponentInParent<Canvas>().scaleFactor;
        Vector2 size = Vector2.Scale(rect.sizeDelta, rect.lossyScale);
        Rect newRect = new Rect((Vector2)rect.position - (size * rect.pivot), size);
        //Vector2 screenPoint = new Vector2(rect.anchorMin.x * Screen.width, rect.anchorMin.y * Screen.height);
        Vector2 screenPoint = new Vector2(newRect.x + newRect.width, newRect.y - newRect.height);
        Canvas canvas = gm.console.GetComponent<Canvas>();
        Vector2 canvasSize = canvas.GetComponent<RectTransform>().sizeDelta;
        particlesHolder.GetComponent<RectTransform>().localScale = Vector3.one;
        particlesHolder.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, 0);
        particlesHolder.GetComponent<RectTransform>().localPosition = - canvasSize + screenPoint;
    }
}
