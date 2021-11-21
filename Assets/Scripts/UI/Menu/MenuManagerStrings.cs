using System;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "MenuManagerStrings", menuName = "MenuManagerStrings")]
public class MenuManagerStrings : ScriptableObject {
    public LocalizedString resetSavesTitle;
    public LocalizedString resetSavesTexte;
    public LocalizedString resetSavesYesButton;
    public LocalizedString resetSavesYesButtonTooltip;
    public LocalizedString resetSavesNoButton;
    public LocalizedString resetSavesNoButtonTooltip;
}
