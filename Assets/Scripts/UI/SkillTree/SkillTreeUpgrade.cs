﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public enum SkillKey {
    BLUE_SLIDE = 0,
    UNLOCK_OVERRIDES = 1,
    UNLOCK_TRESHOLDS = 2,
    ACCELERATED_LANDING = 3,
    EXPLOSIVE_LANDING = 4,
    FIRST_DASH = 5,
    DASH_RESET_WHILE_ON_GROUND = 6,
    SPEED_BOOST_1 = 7,
    UI_OVERRIDE_PREDICTOR = 8,
}

public class SkillTreeUpgrade : MonoBehaviour {

    [Header("Parameters")]
    public SkillKey key;
    public LocalizedString nom;
    public LocalizedString description;
    public int price;
    public Sprite sprite;
    public List<SkillTreeUpgrade> requirements;

    [Header("Links")]
    public Image image;
    public GameObject priceObject;
    public TMP_Text priceText;
    public TooltipActivator mainTooltip;
    public TooltipActivator priceTooltip;
    public GameObject lockedStateIcon;
    public GameObject enabledStateIcon;
    public GameObject disabledStateIcon;

    protected SkillTreeMenu skillTreeMenu;
    protected List<SkillTreeLink> links;

    public void Initialize(SkillTreeMenu skillTreeMenu) {
        this.skillTreeMenu = skillTreeMenu;
        image.sprite = sprite;
        string priceString = GetPriceString();
        priceText.text = priceString;
        mainTooltip.localizedMessage = nom;
        priceTooltip.localizedMessage.Arguments = new object[] { priceString };
        InitializeRequirementLinks();
        DisplayGoodState();
        skillTreeMenu.onClose.AddListener(SetHasBeenNewlyAffordable);
    }

    public void SetHasBeenNewlyAffordable() {
        if(SkillTreeManager.Instance.IsNewlyAffordable(this)) {
            SkillTreeManager.Instance.SetHasBeenNewlyAffordable(key, true);
        }
    }

    public string GetPriceString() {
        return StringHelper.ToCreditsFormat(price);
    }

    protected void InitializeRequirementLinks() {
        links = new List<SkillTreeLink>();
        requirements.ForEach(r => links.Add(skillTreeMenu.CreateLink(r, this)));
    }

    public void Select() {
        skillTreeMenu.Select(this);
    }

    public void Enable() {
        skillTreeMenu.Enable(this);
    }

    public void Disable() {
        skillTreeMenu.Disable(this);
    }

    public void DisplayGoodState() {
        if(!SkillTreeManager.Instance.IsUnlocked(key)) {
            DisplayLockedState();
        } else {
            if(SkillTreeManager.Instance.IsEnabled(key)) {
                DisplayEnabledState();
            } else {
                DisplayDisabledState();
            }
        }
    }

    protected void DisplayDisabledState() {
        lockedStateIcon.gameObject.SetActive(false);
        enabledStateIcon.gameObject.SetActive(false);
        disabledStateIcon.gameObject.SetActive(true);
        priceObject.gameObject.SetActive(false);
    }

    protected void DisplayEnabledState() {
        lockedStateIcon.gameObject.SetActive(false);
        enabledStateIcon.gameObject.SetActive(true);
        disabledStateIcon.gameObject.SetActive(false);
        priceObject.gameObject.SetActive(false);
    }

    protected void DisplayLockedState() {
        lockedStateIcon.gameObject.SetActive(true);
        enabledStateIcon.gameObject.SetActive(false);
        disabledStateIcon.gameObject.SetActive(false);
        priceObject.gameObject.SetActive(true);
    }
}
