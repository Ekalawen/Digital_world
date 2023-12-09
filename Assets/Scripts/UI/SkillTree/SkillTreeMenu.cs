using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class SkillTreeMenu : MonoBehaviour {

    public Transform upgradesFolder;
    public Transform linksFolder;
    public GameObject linkPrefab;

    [Header("Resizing")]
    public LayoutElement layout;
    public float openingDuration = 0.35f;
    public float closingDuration = 0.35f;
    public float upgradeDisplayOpeningDuration = 0.35f;
    public float upgradeDisplayClosingDuration = 0.35f;
    public AnimationCurve resizingCurve;
    public LayoutElement upgradeDisplayLayout;
    public CanvasGroup treeGroup;

    [Header("Links")]
    public SkillTreeBackButtonInGame skillTreeBackButtonInGame;
    public CounterDisplayerUpdater creditsCounterUpdater;
    public LocalizeStringEvent currentUpgradeName;
    public LocalizeStringEvent currentUpgradeDescription;
    public LocalizeStringEvent currentUpgradePrice;
    public TooltipActivator currentUpgradePriceTooltip;
    public GameObject currentUpgradeBuyButtonHolder;
    public Button currentUpgradeBuyButton;
    public Button currentUpgradeBuyButtonDisabled;
    public Button currentUpgradeEnableButton;
    public Button currentUpgradeDisableButton;
    public Image currentUpgradeGif;

    protected List<SkillTreeUpgrade> upgrades;
    protected List<SkillTreeLink> links;
    protected bool isOpen = false;
    protected Fluctuator sizeFluctuator;
    protected Fluctuator upgradeDisplaySizeFluctuator;
    protected SingleCoroutine setActiveCoroutine;
    protected float upgradeDisplayOriginalSize;
    protected float upgradeDisplayOriginalPadding;
    protected float openPrefferedWidth;
    protected SkillTreeUpgrade currentUpgrade;
    protected bool isUpgradeDisplayOpen;
    [HideInInspector]
    public UnityEvent onUpgradeChange;

    public void Initilalize() {
        links = new List<SkillTreeLink>();
        sizeFluctuator = new Fluctuator(this, GetSizeRatio, SetSizeRatio, resizingCurve, useUnscaleTime: true);
        upgradeDisplaySizeFluctuator = new Fluctuator(this, GetUpgradeDisplaySizeRatio, SetUpgradeDisplaySizeRatio, useUnscaleTime: true);
        setActiveCoroutine = new SingleCoroutine(this);
        openPrefferedWidth = layout.preferredWidth;
        InitializeUpgradeDisplay();
        GatherUpgrades();
        InitializeUpgrades();
        InitializeCreditsCounter();
        InitializeSkillTreeBackButtonInGame();
    }

    protected void InitializeSkillTreeBackButtonInGame() {
        if(!skillTreeBackButtonInGame) {
            return;
        }
        skillTreeBackButtonInGame.Initialize(this);
    }

    protected void InitializeCreditsCounter() {
        CounterDisplayer counterDisplayer = creditsCounterUpdater.GetComponent<CounterDisplayer>();
        creditsCounterUpdater.Initialize(counterDisplayer, SkillTreeManager.Instance.GetCredits);
    }

    protected void InitializeUpgradeDisplay() {
        isUpgradeDisplayOpen = false;
        upgradeDisplayOriginalSize = upgradeDisplayLayout.preferredWidth;
        upgradeDisplayLayout.preferredWidth = 0.0f;
        upgradeDisplayOriginalPadding = upgradeDisplayLayout.GetComponent<VerticalLayoutGroup>().padding.left;
        SetSizeRatio(0.0f);
        SetUpgradeDisplaySizeRatio(0.0f);
    }

    protected void SetSizeRatio(float percentageValue) {
        float pixelValue = openPrefferedWidth * percentageValue;
        layout.preferredWidth = pixelValue;
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

        upgradeDisplayLayout.gameObject.SetActive(percentageValue > 0.0f);
    }

    protected float GetUpgradeDisplaySizeRatio() {
        return upgradeDisplayLayout.preferredWidth / upgradeDisplayOriginalSize;
    }

    protected float GetSizeRatio() {
        return layout.preferredWidth / openPrefferedWidth;
    }

    protected void InitializeUpgrades() {
        links.ForEach(l => Destroy(l.gameObject));
        links.Clear();
        upgrades.ForEach(u => u.Initialize(this));
        WarnIfDuplicateKey();
    }

    protected void WarnIfDuplicateKey() {
        if(upgrades.Select(u => u.key).Distinct().Count() < upgrades.Count()) {
            Debug.LogError($"Certaines SkillTreeUpgrade.key sont en doubles !!!");
        }
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
        InitializeSkillTreeBackButtonInGame();
    }

    public void Close() {
        isOpen = false;
        sizeFluctuator.GoTo(0.0f, closingDuration);
        setActiveCoroutine.Start(CSetActiveIn(closingDuration, false));
        CloseUpgradeDisplay();
    }

    public void CloseInstantly() {
        isOpen = false;
        SetSizeRatio(0.0f);
        GetCounterDisplayer().RemoveAllVolatilesTexts();
        creditsCounterUpdater.UpdateValueInstantly();
        CloseUpgradeDisplayInstantly();
        SetActive(false);
    }

    public void CloseUpgradeDisplay() {
        isUpgradeDisplayOpen = false;
        upgradeDisplaySizeFluctuator.GoTo(0.0f, upgradeDisplayClosingDuration);
    }

    protected void CloseUpgradeDisplayInstantly() {
        isUpgradeDisplayOpen = false;
        SetUpgradeDisplaySizeRatio(0.0f);
    }

    public void OpenUpgradeDisplay() {
        isUpgradeDisplayOpen = true;
        upgradeDisplaySizeFluctuator.GoTo(1.0f, upgradeDisplayOpeningDuration);
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
        OpenUpgradeDisplay();
    }

    protected void PopulateVerticalMenuWith(SkillTreeUpgrade upgrade) {
        if(!upgrade) {
            return;
        }
        currentUpgradeName.StringReference = upgrade.nom;
        currentUpgradeDescription.StringReference = upgrade.description;
        currentUpgradeDescription.StringReference.Arguments = upgrade.GetComponent<InputTextBindingParameters>().GetArguments();
        currentUpgradePrice.StringReference.Arguments = new object[] { upgrade.GetPriceString() };
        currentUpgradePriceTooltip.localizedMessage.Arguments = new object[] { upgrade.GetPriceString() };
        currentUpgradeGif.sprite = upgrade.sprite;
        ShowAppropriateButtonFor(upgrade);
    }

    private void ShowAppropriateButtonFor(SkillTreeUpgrade upgrade) {
        if (!SkillTreeManager.Instance.IsUnlocked(upgrade.key)) {
            ShowBuyButton(upgrade);
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

    protected void ShowBuyButton(SkillTreeUpgrade upgrade) {
        currentUpgradeBuyButtonHolder.gameObject.SetActive(true);
        currentUpgradeDisableButton.gameObject.SetActive(false);
        currentUpgradeEnableButton.gameObject.SetActive(false);
        ShowGoodBuytButton(upgrade);
    }

    protected void ShowGoodBuytButton(SkillTreeUpgrade upgrade) {
        bool canBuy = SkillTreeManager.Instance.CanBuy(upgrade);
        currentUpgradeBuyButton.gameObject.SetActive(canBuy);
        currentUpgradeBuyButtonDisabled.gameObject.SetActive(!canBuy);
    }

    public void BuyCurrentUpgrade() {
        if(!currentUpgrade) {
            return;
        }
        if (SkillTreeManager.Instance.IsUnlocked(currentUpgrade.key)) {
            return;
        }
        if (!SkillTreeManager.Instance.IsAffordable(currentUpgrade)) {
            return;
        }
        BuyUpgrade(currentUpgrade);
        onUpgradeChange.Invoke();
    }

    protected void BuyUpgrade(SkillTreeUpgrade upgrade) {
        SkillTreeManager.Instance.RemoveCredits(upgrade.price);
        SkillTreeManager.Instance.Unlock(upgrade.key);
        DisplayVolatileCredits(- upgrade.price);
        upgrade.DisplayGoodState();
        PopulateVerticalMenuWith(upgrade);
        InitializeUpgrades(); // Mostly for links
        // Run animation ! :D
    }

    private void DisplayVolatileCredits(int creditsVariation) {
        string creditsVariationString = creditsCounterUpdater.ApplyToCreditsFormating(Mathf.Abs(creditsVariation));
        string signString = creditsVariation >= 0 ? "+" : "-";
        GetCounterDisplayer().AddVolatileText($"{signString} {creditsVariationString}", GetCounterDisplayer().GetTextColor());
        creditsCounterUpdater.UpdateValue();
    }

    protected CounterDisplayer GetCounterDisplayer() {
        return creditsCounterUpdater.GetComponent<CounterDisplayer>();
    }

    public void Enable(SkillTreeUpgrade upgrade) {
        SkillTreeManager.Instance.Enable(upgrade.key);
        ShowAppropriateButtonFor(upgrade);
        upgrade.DisplayGoodState();
        onUpgradeChange.Invoke();
    }

    public void Disable(SkillTreeUpgrade upgrade) {
        SkillTreeManager.Instance.Disable(upgrade.key);
        ShowAppropriateButtonFor(upgrade);
        upgrade.DisplayGoodState();
        onUpgradeChange.Invoke();
    }

    public void EnableCurrentUpgrade() {
        Enable(currentUpgrade);
    }

    public void DisableCurrentUpgrade() {
        Disable(currentUpgrade);
    }

    public void ResetAllUpgrades() {
        foreach (SkillTreeUpgrade upgrade in upgrades) {
            SkillTreeManager.Instance.Lock(upgrade.key);
        }
        InitializeUpgrades();
        PopulateVerticalMenuWith(currentUpgrade);
        onUpgradeChange.Invoke();
    }

    public void MultiplyCreditsBy10() {
        int variation = SkillTreeManager.Instance.MultiplyCreditsBy10();
        if (IsOpen()) {
            DisplayVolatileCredits(variation);
            PopulateVerticalMenuWith(currentUpgrade);
            InitializeUpgrades();
        }
    }

    public void ResetCredits() {
        int variation = - SkillTreeManager.Instance.GetCredits();
        SkillTreeManager.Instance.SetCredits(0);
        if (IsOpen()) {
            DisplayVolatileCredits(variation);
            PopulateVerticalMenuWith(currentUpgrade);
            InitializeUpgrades();
        }
    }
}
