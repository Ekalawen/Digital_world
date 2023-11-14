﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreeMenu : MonoBehaviour {

    public Transform upgradesFolder;
    public Transform linksFolder;
    public GameObject linkPrefab;

    [Header("Resizing")]
    public LayoutElement layout;
    public float openingDuration = 1.0f;
    public float closingDuration = 1.0f;
    public AnimationCurve resizingCurve;
    public LayoutElement upgradeDisplayLayout;
    public CanvasGroup treeGroup;

    protected List<SkillTreeUpgrade> upgrades;
    protected List<SkillTreeLink> links;
    protected bool isOpen = false;
    protected Fluctuator sizeFluctuator;
    protected SingleCoroutine setActiveCoroutine;
    protected float upgradeDisplayOriginalSize;
    protected float upgradeDisplayOriginalPadding;
    protected float openPrefferedWidth;

    public void Initilalize() {
        links = new List<SkillTreeLink>();
        sizeFluctuator = new Fluctuator(this, GetSizeRatio, SetSizeRatio, resizingCurve);
        setActiveCoroutine = new SingleCoroutine(this);
        openPrefferedWidth = layout.preferredWidth;
        InitializeUpgradeDisplay();
        GatherUpgrades();
        InitializeUpgrades();
    }

    protected void InitializeUpgradeDisplay() {
        upgradeDisplayOriginalSize = upgradeDisplayLayout.preferredWidth;
        upgradeDisplayLayout.preferredWidth = 0.0f;
        upgradeDisplayOriginalPadding = upgradeDisplayLayout.GetComponent<VerticalLayoutGroup>().padding.left;
        SetSizeRatio(0.0f);
    }

    protected void SetSizeRatio(float percentageValue) {
        float pixelValue = openPrefferedWidth * percentageValue;
        layout.preferredWidth = pixelValue;
        SetUpgradeDisplaySizeRatio(percentageValue);
        SetTreeAlpha(percentageValue);
    }

    protected void SetTreeAlpha(float percentageValue) {
        treeGroup.alpha = percentageValue;
    }

    protected void SetUpgradeDisplaySizeRatio(float percentageValue) {
        float pixelValue = upgradeDisplayOriginalSize * percentageValue;
        upgradeDisplayLayout.preferredWidth = pixelValue;

        float paddingPixelValue = upgradeDisplayOriginalPadding * percentageValue;
        VerticalLayoutGroup verticalLayoutGroup = upgradeDisplayLayout.GetComponent<VerticalLayoutGroup>();
        verticalLayoutGroup.padding.left = (int)paddingPixelValue;
        verticalLayoutGroup.padding.right = (int)paddingPixelValue;
    }

    protected float GetSizeRatio() {
        float width = layout.preferredWidth;
        return width / openPrefferedWidth;
    }

    protected void InitializeUpgrades() {
        upgrades.ForEach(u => u.Initialize(this));
    }

    protected void GatherUpgrades() {
        upgrades = new List<SkillTreeUpgrade>();
        GatherUpgradesIn(upgradesFolder);
    }

    protected void GatherUpgradesIn(Transform folder) {
        foreach (Transform child in folder) {
            SkillTreeUpgrade upgrade = child.GetComponent<SkillTreeUpgrade>();
            if(upgrade) {
                upgrades.Add(upgrade);
            } else {
                GatherUpgradesIn(child);
            }
        }
    }

    public void CreateLink(SkillTreeUpgrade source, SkillTreeUpgrade target) {
        SkillTreeLink link = Instantiate(linkPrefab, linksFolder).GetComponent<SkillTreeLink>();
        RectTransform rect = link.GetComponent<RectTransform>();
        rect.anchorMin = Vector3.zero;
        rect.anchorMax = Vector3.one;
        rect.offsetMin = Vector3.zero;
        rect.offsetMax = Vector3.zero;
        link.Initialize(this, source, target);
        links.Add(link);
    }

    public void Toggle() {
        if(isOpen) {
            Close();
        } else {
            Open();
        }
    }

    protected void Open() {
        isOpen = true;
        SetActive(true);
        setActiveCoroutine.Stop();
        sizeFluctuator.GoTo(1.0f, openingDuration);
    }

    protected void Close() {
        isOpen = false;
        sizeFluctuator.GoTo(0.0f, closingDuration);
        setActiveCoroutine.Start(CSetActiveIn(closingDuration, false));
    }

    protected IEnumerator CSetActiveIn(float duration, bool isActive) {
        yield return new WaitForSeconds(duration);
        SetActive(isActive);
    }

    protected void SetActive(bool isActive) {
        gameObject.SetActive(isActive);
    }

    public bool IsOpen() {
        return isOpen;
    }
}
