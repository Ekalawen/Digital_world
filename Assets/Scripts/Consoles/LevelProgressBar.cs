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

    public GameObject holder;
    public Scrollbar scrollBar;
    public TMP_Text percentageText;
    public Vector2 percentageTextYPositions = new Vector2(15, -5);
    public TMP_Text totalText;
    public Image fillerImage;

    protected GameManager gm;
    protected int maxValue;

    public void Initialize() {
        gm = GameManager.Instance;
        InitializeProgressBar();
    }

    public void Update() {
        SetProgressBarValue();
    }

    protected void InitializeProgressBar() {
        holder.SetActive(gm.IsIR());
        if (!gm.IsIR()) {
            return;
        }
        maxValue = gm.goalManager.GetTreshold();
        totalText.text = StringHelper.ToCreditsShortFormat(maxValue);
        fillerImage.material = new Material(fillerImage.material);
        SetProgressBarValue();
    }

    protected void SetProgressBarValue() {
        float currentValue = gm.goalManager.GetTotalCreditScore();
        Debug.Log($"currentValue = {currentValue} bestScore = {gm.goalManager.GetBestCreditScore()}");
        float avancement = currentValue / maxValue;
        scrollBar.size = avancement;
        percentageText.text = $"{avancement * 100:N0}%";
        float textYPosition = avancement <= 0.5f ? percentageTextYPositions[0] : percentageTextYPositions[1];
        percentageText.rectTransform.anchoredPosition = new Vector2(percentageText.rectTransform.anchoredPosition.x, textYPosition);
        fillerImage.material.SetFloat("_ColorAvancement", avancement);
    }
}
