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

    protected void InitializeProgressBar() {
        holder.SetActive(gm.IsIR());
        if (!gm.IsIR()) {
            return;
        }
        maxValue = gm.goalManager.GetTreshold();
        totalText.text = StringHelper.ToCreditsShortFormat(maxValue);
        fillerImage.material = new Material(fillerImage.material);
        SetProgressBarValue();
        gm.GetInfiniteMap().scoreManager.onScoreChange.AddListener(v => SetProgressBarValue());
    }

    protected void SetProgressBarValue() {
        float currentValue = gm.goalManager.GetCurrentTotalCreditScore();
        float avancement = Mathf.Min(currentValue / maxValue, 1.0f);
        scrollBar.size = avancement;
        percentageText.text = $"{StringHelper.ToCreditsShortFormat((long)(avancement * 100))}%";
        float textYPosition = avancement <= 0.5f ? percentageTextYPositions[0] : percentageTextYPositions[1];
        percentageText.rectTransform.anchoredPosition = new Vector2(percentageText.rectTransform.anchoredPosition.x, textYPosition);
        fillerImage.material.SetFloat("_ColorAvancement", avancement);
    }
}
