using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
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

    [Header("Links")]
    public CounterDisplayerUpdater creditsCounterUpdater;
    public LocalizeStringEvent currentUpgradeName;
    public LocalizeStringEvent currentUpgradeDescription;
    public LocalizeStringEvent currentUpgradePrice;
    public TooltipActivator currentUpgradePriceTooltip;
    public GameObject currentUpgradeBuyButtonHolder;
    public Button currentUpgradeBuyButton;
    public Button currentUpgradeEnableButton;
    public Button currentUpgradeDisableButton;
    public Image currentUpgradeGif;

    protected List<SkillTreeUpgrade> upgrades;
    protected List<SkillTreeLink> links;
    protected bool isOpen = false;
    protected Fluctuator sizeFluctuator;
    protected SingleCoroutine setActiveCoroutine;
    protected float upgradeDisplayOriginalSize;
    protected float upgradeDisplayOriginalPadding;
    protected float openPrefferedWidth;
    protected SkillTreeUpgrade currentUpgrade;

    public void Initilalize() {
        links = new List<SkillTreeLink>();
        sizeFluctuator = new Fluctuator(this, GetSizeRatio, SetSizeRatio, resizingCurve, useUnscaleTime: true);
        setActiveCoroutine = new SingleCoroutine(this);
        openPrefferedWidth = layout.preferredWidth;
        InitializeUpgradeDisplay();
        GatherUpgrades();
        InitializeUpgrades();
        InitializeCreditsCounter();
    }

    protected void InitializeCreditsCounter() {
        CounterDisplayer counterDisplayer = creditsCounterUpdater.GetComponent<CounterDisplayer>();
        creditsCounterUpdater.Initialize(counterDisplayer, SkillTreeManager.Instance.GetCredits);
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
        currentUpgrade = null;
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

    public void Open() {
        isOpen = true;
        SetActive(true);
        setActiveCoroutine.Stop();
        sizeFluctuator.GoTo(1.0f, openingDuration);
    }

    public void Close() {
        isOpen = false;
        sizeFluctuator.GoTo(0.0f, closingDuration);
        setActiveCoroutine.Start(CSetActiveIn(closingDuration, false));
    }

    protected IEnumerator CSetActiveIn(float duration, bool isActive) {
        yield return new WaitForSecondsRealtime(duration);
        SetActive(isActive);
    }

    protected void SetActive(bool isActive) {
        gameObject.SetActive(isActive);
    }

    public bool IsOpen() {
        return isOpen;
    }

    public void Select(SkillTreeUpgrade upgrade) {
        currentUpgrade = upgrade;
        PopulateVerticalMenuWith(upgrade);
    }

    protected void PopulateVerticalMenuWith(SkillTreeUpgrade upgrade) {
        currentUpgradeName.StringReference = upgrade.nom;
        currentUpgradeDescription.StringReference = upgrade.description;
        currentUpgradePrice.StringReference.Arguments = new object[] { upgrade.GetPriceString() };
        currentUpgradePriceTooltip.localizedMessage.Arguments = new object[] { upgrade.GetPriceString() };
        currentUpgradeGif.sprite = upgrade.sprite;
        ShowAppropriateButtonFor(upgrade);
    }

    private void ShowAppropriateButtonFor(SkillTreeUpgrade upgrade) {
        if (!SkillTreeManager.Instance.IsUnlocked(upgrade.key)) {
            ShowBuyButton();
        } else {
            if (SkillTreeManager.Instance.IsEnabled(upgrade.key)) {
                ShowDisableButton();
            } else {
                ShowEnableButton();
            }
        }
    }

    protected void ShowEnableButton() {
        currentUpgradeBuyButtonHolder.gameObject.SetActive(false);
        currentUpgradeDisableButton.gameObject.SetActive(false);
        currentUpgradeEnableButton.gameObject.SetActive(true);
    }

    protected void ShowDisableButton() {
        currentUpgradeBuyButtonHolder.gameObject.SetActive(false);
        currentUpgradeDisableButton.gameObject.SetActive(true);
        currentUpgradeEnableButton.gameObject.SetActive(false);
    }

    protected void ShowBuyButton() {
        currentUpgradeBuyButtonHolder.gameObject.SetActive(true);
        currentUpgradeDisableButton.gameObject.SetActive(false);
        currentUpgradeEnableButton.gameObject.SetActive(false);
    }

    public void BuyCurrentUpgrade() {
        if(!currentUpgrade) {
            return;
        }
        //if(SkillTreeManager.Instance.IsUnlocked(currentUpgrade.key)) {
        //    return;
        //}
        if (currentUpgrade.price > SkillTreeManager.Instance.GetCredits()) {
            return;
        }
        BuyUpgrade(currentUpgrade);
    }

    protected void BuyUpgrade(SkillTreeUpgrade upgrade) {
        SkillTreeManager.Instance.RemoveCredits(upgrade.price);
        SkillTreeManager.Instance.Unlock(upgrade.key);
        string priceString = creditsCounterUpdater.ApplyToCreditsFormating(upgrade.price);
        GetCounterDisplayer().AddVolatileText($"- {priceString}", GetCounterDisplayer().GetTextColor());
        creditsCounterUpdater.UpdateValue();
        upgrade.Initialize(this);
        PopulateVerticalMenuWith(upgrade);
        // Run animation ! :D
    }

    protected CounterDisplayer GetCounterDisplayer() {
        return creditsCounterUpdater.GetComponent<CounterDisplayer>();
    }

    public void Enable(SkillTreeUpgrade upgrade) {
        SkillTreeManager.Instance.Enable(upgrade.key);
        ShowAppropriateButtonFor(upgrade);
        upgrade.DisplayGoodState();
    }

    public void Disable(SkillTreeUpgrade upgrade) {
        SkillTreeManager.Instance.Disable(upgrade.key);
        ShowAppropriateButtonFor(upgrade);
        upgrade.DisplayGoodState();
    }

    public void EnableCurrentUpgrade() {
        Enable(currentUpgrade);
    }

    public void DisableCurrentUpgrade() {
        Disable(currentUpgrade);
    }
}
