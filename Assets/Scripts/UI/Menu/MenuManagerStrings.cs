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

    public LocalizedString quitGameTitle;
    public LocalizedString quitGameTexte;
    public LocalizedString quitGameYesButton;
    public LocalizedString quitGameYesButtonTooltip;
    public LocalizedString quitGameNoButton;
    public LocalizedString quitGameNoButtonTooltip;
}
