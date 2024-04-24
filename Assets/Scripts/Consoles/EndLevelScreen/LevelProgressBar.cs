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
using UnityEngine.Events;
using UnityEditor.PackageManager.UI;
using UnityEngine.Localization.Components;

public class LevelProgressBar : MonoBehaviour {

    public float changeValueDuration = 0.5f;
    public AnimationCurve changeValueCurve;
    public AnimationCurve changeValueOver100Curve;
    public GameObject holder;
    public Scrollbar scrollBar;
    public TMP_Text percentageText;
    public Vector2 percentageTextYPositions = new Vector2(15, -5);
    public TMP_Text totalText;
    public Image fillerImage;

    [Header("Particles")]
    public float onChangeParticlesCountLogProgression = 4;
    public GameObject onValueChangeParticlesHolderPrefab;
    public float onValueChangeParticlesFrequence = 0.1f;
    public GameObject onReachMaxValueParticlesHolderPrefab;
    public float offsetSize = 165f; // How much vertical pixel place are taking other elements of the UI in order to redimension it

    [Header("Tooltips")]
    public TooltipActivator totalTextTooltip;
    public TooltipActivator percentageTextTooltip;

    protected GameManager gm;
    protected int maxValue;
    protected int currentValue;
    protected Fluctuator valueFluctuator;
    protected bool hasPlayMaxValueParticles = false;
    protected float startAvancement;
    protected float displayedAvancement;
    protected Timer onValueChangeParticlesTimer;
    [HideInInspector]
    public UnityEvent onReachMaxValueVisual;
    protected bool hasReachedMaxValue = false;

    public void Initialize(int maxValue, int currentValue) {
        gm = GameManager.Instance;
        holder.SetActive(true);
        InitializeSize();
        onValueChangeParticlesTimer = new Timer(onValueChangeParticlesFrequence, setOver: true);
        valueFluctuator = new Fluctuator(this, GetProgressBarDisplayedAvancement, SetProgressBarValue);
        this.maxValue = maxValue;
        this.currentValue = currentValue;
        startAvancement = GetCurrentAvancement();
        totalText.text = StringHelper.ToCreditsShortFormat(maxValue);
        fillerImage.material = new Material(fillerImage.material);
        SetProgressBarValue(avancement: startAvancement);
    }

    protected void SetTooltipsValues() {
        float avancement = currentValue / (float)maxValue * 100;
        avancement = Mathf.Round(avancement * 100) / 100; // Only 2 digits of precisionj
        totalTextTooltip.localizedMessage.Arguments = new object[] { StringHelper.ToCreditsFormat(maxValue) };
        percentageTextTooltip.localizedMessage.Arguments = new object[] { StringHelper.ToCreditsFormat(currentValue), avancement };
    }

    protected void InitializeSize() {
        Vector2 size = GetComponent<RectTransform>().sizeDelta;
        RectTransform canvasSize = gm.console.GetComponent<RectTransform>(); // The canvas is on the console
        size.y = canvasSize.sizeDelta.y - offsetSize;
        GetComponent<RectTransform>().sizeDelta = size;
        GetComponent<VerticalLayoutGroup>().CalculateLayoutInputVertical();
    }

    protected void UpdateProgressBarValue() {
        float avancement = GetCurrentAvancement();
        AnimationCurve curve = avancement >= 1.0f ? changeValueOver100Curve : changeValueCurve;
        valueFluctuator.GoTo(avancement, changeValueDuration, curve);
        PlayParticlesOnValueChange(avancement);
    }

    protected float GetCurrentAvancement() {
        return (float)currentValue / maxValue;
    }

    protected float GetProgressBarDisplayedAvancement() {
        return displayedAvancement;
    }

    public void SetCurrentValue(int value) {
        currentValue = value;
        UpdateProgressBarValue();
    }

    protected void SetProgressBarValue(float avancement) {
        percentageText.text = $"{StringHelper.ToCreditsShortFormat((long)(avancement * 100))}%";
        displayedAvancement = avancement;
        avancement = Mathf.Min(avancement, 1.0f);
        scrollBar.size = avancement;
        float textYPosition = avancement <= 0.5f ? percentageTextYPositions[0] : percentageTextYPositions[1];
        percentageText.rectTransform.anchoredPosition = new Vector2(percentageText.rectTransform.anchoredPosition.x, textYPosition);
        fillerImage.material.SetFloat("_ColorAvancement", avancement);
        SetTooltipsValues();
        SendHasReachMaxValue(avancement);
    }

    protected void SendHasReachMaxValue(float avancement) {
        if(avancement < 1.0f) {
            return;
        }
        if(hasReachedMaxValue) {
            return;
        }
        hasReachedMaxValue = true;
        onReachMaxValueVisual.Invoke();
    }

    protected void PlayParticlesOnValueChange(float avancement) {
        if(!onValueChangeParticlesTimer.IsOver()) {
            return;
        }
        onValueChangeParticlesTimer.Reset();
        if (avancement < 1.0f && startAvancement < 1.0f) {
            float gainQuantity = maxValue * (avancement - GetProgressBarDisplayedAvancement());
            PlayParticles(onValueChangeParticlesHolderPrefab, gainQuantity);
        }
        if (avancement >= 1.0f && startAvancement < 1.0f && !hasPlayMaxValueParticles) {
            hasPlayMaxValueParticles = true;
            PlayParticles(onReachMaxValueParticlesHolderPrefab, -1);
        }
    }

    protected void PlayParticles(GameObject particlesHolderPrefab, float gainQuantity) {
        GameObject particlesHolder = Instantiate(particlesHolderPrefab, parent: fillerImage.transform);
        foreach(Transform child in particlesHolder.transform) {
            ParticleSystem particleSystem = child.GetComponent<ParticleSystem>();
            if(!particleSystem) {
                continue;
            }
            if (gainQuantity >= 0) {
                ParticleSystem.Burst[] bursts = new ParticleSystem.Burst[particleSystem.emission.burstCount];
                particleSystem.emission.GetBursts(bursts);
                bursts[0].count = Mathf.RoundToInt(Mathf.Max(Mathf.Log(gainQuantity, onChangeParticlesCountLogProgression), 2));
                particleSystem.emission.SetBurst(0, bursts[0]);
            }
            particleSystem.Play();
            Destroy(particlesHolder, 5.0f);
        }
    }

    public bool IsFull() {
        return currentValue >= maxValue;
    }

    public void ReceiveParticle(int particleValue) {
        SetCurrentValue(currentValue + particleValue);
    }
}
