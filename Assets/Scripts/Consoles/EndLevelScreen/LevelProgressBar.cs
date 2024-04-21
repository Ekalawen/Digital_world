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
    public AnimationCurve changeValueCurve;
    public float onChangeParticlesCountLogProgression = 4;
    public GameObject holder;
    public Scrollbar scrollBar;
    public TMP_Text percentageText;
    public Vector2 percentageTextYPositions = new Vector2(15, -5);
    public TMP_Text totalText;
    public Image fillerImage;
    public GameObject onValueChangeParticlesHolderPrefab;
    public float onValueChangeParticlesFrequence = 0.1f;
    public GameObject onReachMaxValueParticlesHolderPrefab;

    protected GameManager gm;
    protected int maxValue;
    protected int currentValue;
    protected Fluctuator valueFluctuator;
    protected bool hasPlayMaxValueParticles = false;
    protected float startAvancement;
    protected float displayedAvancement;
    protected Timer onValueChangeParticlesTimer;

    public void Initialize(int maxValue) {
        gm = GameManager.Instance;
        holder.SetActive(true);
        onValueChangeParticlesTimer = new Timer(onValueChangeParticlesFrequence, setOver: true);
        valueFluctuator = new Fluctuator(this, GetProgressBarDisplayedAvancement, SetProgressBarValue);
        this.maxValue = maxValue;
        currentValue = 0;
        startAvancement = GetCurrentAvancement();
        totalText.text = StringHelper.ToCreditsShortFormat(maxValue);
        fillerImage.material = new Material(fillerImage.material);
        SetProgressBarValue(avancement: 0.0f);
    }

    protected void UpdateProgressBarValue() {
        float avancement = GetCurrentAvancement();
        valueFluctuator.GoTo(avancement, changeValueDuration, changeValueCurve);
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
        PlayParticlesOnValueChange(avancement);
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
        /// BAD IDEA ALWAYS LEAD TO BAD STUFF !!!
        /// When you can't do something, find the plugins that does ! <3
        //particlesHolder.transform.SetParent(gm.player.particlesCanvas.transform, true);
        ////Vector2 screenPoint = percentageText.GetComponent<RectTransform>().position;
        //RectTransform rect = percentageText.GetComponent<RectTransform>();
        ////float x = rect.anchorMin.x * Screen.width * rect.gameObject.GetComponentInParent<Canvas>().scaleFactor;
        ////float y = rect.anchorMin.y * Screen.height * rect.gameObject.GetComponentInParent<Canvas>().scaleFactor;
        //Vector2 size = Vector2.Scale(rect.sizeDelta, rect.lossyScale);
        //Rect newRect = new Rect((Vector2)rect.position - (size * rect.pivot), size);
        ////Vector2 screenPoint = new Vector2(rect.anchorMin.x * Screen.width, rect.anchorMin.y * Screen.height);
        //Vector2 screenPoint = new Vector2(newRect.x + newRect.width, newRect.y - newRect.height);
        //Canvas canvas = gm.console.GetComponent<Canvas>();
        //Vector2 canvasSize = canvas.GetComponent<RectTransform>().sizeDelta;
        //particlesHolder.GetComponent<RectTransform>().localScale = Vector3.one;
        //particlesHolder.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, 0);
        //particlesHolder.GetComponent<RectTransform>().localPosition = - canvasSize + screenPoint;
    }
}
