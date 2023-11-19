using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class SkillTreeUpgrade : MonoBehaviour {

    [Header("Parameters")]
    public string key;
    public LocalizedString nom;
    public LocalizedString description;
    public int price;
    public Sprite sprite;
    public List<SkillTreeUpgrade> requirements;

    [Header("Links")]
    public Image image;
    public TMP_Text priceText;
    public TooltipActivator mainTooltip;
    public TooltipActivator priceTooltip;

    protected SkillTreeMenu skillTreeMenu;

    public void Initialize(SkillTreeMenu skillTreeMenu) {
        this.skillTreeMenu = skillTreeMenu;
        image.sprite = sprite;
        string priceString = GetPriceString();
        priceText.text = priceString;
        mainTooltip.localizedMessage = nom;
        priceTooltip.localizedMessage.Arguments = new object[] { priceString };
        InitializeRequirementLinks();
    }

    public string GetPriceString() {
        return StringHelper.ToCreditsFormat(price);
    }

    protected void InitializeRequirementLinks() {
        requirements.ForEach(r => skillTreeMenu.CreateLink(r, this));
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
}
