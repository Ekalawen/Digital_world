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
    public LocalizedString name;
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
        string priceString = StringHelper.ToCreditsFormat(price);
        priceText.text = priceString;
        mainTooltip.localizedMessage = name;
        priceTooltip.localizedMessage.Arguments = new object[] { priceString };
        InitializeRequirementLinks();
    }

    protected void InitializeRequirementLinks() {
        requirements.ForEach(r => skillTreeMenu.CreateLink(r, this));
    }
}
