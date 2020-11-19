using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class SelectorPathUnlockScreen : MonoBehaviour {

    [Header("Links")]
    public Text fromLevelTitle;
    public Text toLevelTitle;
    public GameObject openCadena;
    public GameObject closedCadena;
    public InputField input;
    public Button hackButton;
    public Button donneesHackeesButton;
    public Button returnButton;

    [Header("Background Unlocked theme")]
    public float probaBackgroundUnlocked;
    public float decroissanceBackgroundUnlocked;
    public int distanceBackgroundUnlocked;
    public List<ColorSource.ThemeSource> themesBackgroundUnlocked;

    [Header("Background Locked theme")]
    public float probaBackgroundLocked;
    public float decroissanceBackgroundLocked;
    public int distanceBackgroundLocked;
    public List<ColorSource.ThemeSource> themesBackgroundLocked;

    protected SelectorManager selectorManager;
    protected SelectorPath selectorPath;

    public void Initialize(SelectorPath selectorPath) {
        this.selectorManager = SelectorManager.Instance;
        this.selectorPath = selectorPath;
        SetBackgroundAccordingToLockState();
        SetCadenasAccordingToLockState();
        SetTitles();
        FillInputWithPasswordIfAlreayDiscovered();
    }

    protected void SetTitles() {
        string startLevelName = selectorPath.startLevel.GetName();
        fromLevelTitle.text = startLevelName;
        UIHelper.FitTextHorizontaly(startLevelName, fromLevelTitle);
        string endLevelName = selectorPath.endLevel.GetName();
        toLevelTitle.text = endLevelName;
        UIHelper.FitTextHorizontaly(endLevelName, toLevelTitle);
    }

    public void Submit() {
        if (!selectorManager.HasSelectorPathUnlockScreenOpen())
            return;
        if (input.text == selectorPath.GetPassword() || input.text == MenuLevel.SUPER_CHEATED_PASSWORD) {
            if (selectorPath.IsUnlocked()) {
                SubmitGoodUnlocked();
            } else {
                SubmitGoodLocked();
            }
        } else {
            if (selectorPath.IsUnlocked()) {
                SubmitFalseUnlocked();
            } else {
                SubmitFalseLocked();
            }
        }
    }

    protected void SubmitGoodUnlocked() {
        selectorManager.RunPopup(
            title: "Déjà débloqué !",
            text: "Vous avez déjà dévérouillé ce Path() !",
            theme: TexteExplicatif.Theme.NEUTRAL);
    }

    protected void SubmitGoodLocked() {
        selectorPath.UnlockPath();
        SetBackgroundAccordingToLockState();
        SetCadenasAccordingToLockState();
        selectorPath.endLevel.objectLevel.cube.SetMaterial(focus: false);
        selectorPath.cadena.DisplayGoodCadena();
        selectorManager.RunPopup("Path() hacké !",
            "Félicitation !\n" +
            "Vous avez trouvé le bon mot de passe pour hacker et dévérouiller ce Path() !" +
            "\n" +
            "Keep hacking !",
            TexteExplicatif.Theme.POSITIF);
    }

    protected void SubmitFalseLocked() {
        string registeredPassword = input.text;
        string goodPassword = selectorPath.GetPassword();
        string advice = GetPasswordAdvice(registeredPassword, goodPassword);
        selectorManager.RunPopup("Mot de passe érroné.",
            "Ce n'est pas le bon mot de passe.\n" +
            advice +
            "Réussissez ce niveau pour obtenir le mot de passe dans les Données Hackées().\n" +
            "\n" +
            "Bonne Chance !",
            TexteExplicatif.Theme.NEGATIF);
    }

    protected string GetPasswordAdvice(string registeredPassword, string goodPassword) {
        if (selectorPath.dontUseAdvice)
            return "";
        string advice = Trace.GetPasswordAdvice(registeredPassword, goodPassword);
        return UIHelper.SurroundWithColor(advice, UIHelper.GREEN) + "\n";
    }

    protected void SubmitFalseUnlocked() {
        string password = selectorPath.GetPassword();
        selectorManager.popup.Initialize(
            title: "C'était mieux avant.",
            mainText: "Ce Path() est déjà dévérouillé !\n" +
            "Et pourtant ce n'est plus le bon mot de passe.\n" +
            $"Le mot de passe était {password}.\n",
            theme: TexteExplicatif.Theme.NEUTRAL);
        selectorManager.popup.AddReplacement(password, UIHelper.SurroundWithColor(password, UIHelper.GREEN));
        selectorManager.popup.Run();
    }

    public void SubmitIfEnter() {
        if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
            Submit();
    }

    public void OpenDonneesHackees() {
        if (!selectorManager.HasSelectorPathUnlockScreenOpen())
            return;
        int currentTreshold = GetCurrentTreshold();
        int firstTreshold = new TresholdText(selectorPath.donneesHackees.text).GetFirstFragment().treshold;
        if (currentTreshold == 0 || currentTreshold < firstTreshold) {
            if (currentTreshold == 0)
                OpenDonneesHackeesJamaisHackee();
            else
                OpenDonneesHackeesBeforeFirstTreshold(firstTreshold);
        } else {
            OpenDonneesHackeesWithTreshold(currentTreshold);
        }
    }

    protected int GetCurrentTreshold() {
        if (GetStartLevelType() == MenuLevel.LevelType.REGULAR)
            return selectorPath.startLevel.menuLevel.GetNbWins();
        else
            return (int)selectorPath.startLevel.menuLevel.GetBestScore();
    }

    public MenuLevel.LevelType GetStartLevelType() {
        return selectorPath.startLevel.menuLevel.levelType;
    }

    protected void OpenDonneesHackeesWithTreshold(int treshold) {
        selectorManager.popup.Initialize(
            title: "Données Hackées",
            useTextAsset: true,
            textAsset: selectorPath.donneesHackees,
            theme: TexteExplicatif.Theme.NEUTRAL);
        AddReplacementForDonneesHackeesToPopup(selectorManager.popup);
        selectorManager.popup.Run(textTreshold: treshold);
        AddNextPallierMessage(selectorManager.popup, textTreshold: treshold);
    }

    protected void OpenDonneesHackeesJamaisHackee() {
        selectorManager.RunPopup(
            title: "Données Hackées",
            text: "Vous n'avez encore jamais hackée niveau.\n" +
            "Aucunes données accessibles.",
            theme: TexteExplicatif.Theme.NEUTRAL);
    }

    protected void OpenDonneesHackeesBeforeFirstTreshold(int firstTreshold) {
        string unite = (GetStartLevelType() == MenuLevel.LevelType.INFINITE) ? "block" : "victoire";
        unite += (firstTreshold > 1) ? "s" : "";
        selectorManager.RunPopup(
            title: "Données Hackées",
            text: "Vous n'avez encore jamais hackée niveau.\n" +
            $"Premier pallier à {firstTreshold} {unite}.\n" +
            "Aucunes données accessibles.",
            theme: TexteExplicatif.Theme.NEUTRAL);
    }

    protected void AddReplacementForDonneesHackeesToPopup(TexteExplicatif popup) {
        popup.AddReplacement("%Trace%", UIHelper.SurroundWithColor(selectorPath.GetTrace(), UIHelper.PURE_GREEN));
        popup.AddReplacement("%Passe%", UIHelper.SurroundWithColor(selectorPath.passwordPasse, UIHelper.PURE_GREEN));
        MatchEvaluator evaluator = new MatchEvaluator(TexteExplicatif.SurroundWithBlueColor);
        popup.AddReplacementEvaluator(@"Passes?", evaluator);
        popup.AddReplacementEvaluator(@"Traces?", evaluator);
        popup.AddReplacementEvaluator(@"Donn[ée]es Hack[ée]es", evaluator);
    }

    protected void AddNextPallierMessage(TexteExplicatif texteExplicatif, int textTreshold) {
        TresholdText tresholdText = texteExplicatif.GetTresholdText();
        List<TresholdFragment> fragments = tresholdText.GetAllFragmentsOrdered();
        for(int i = 0; i < fragments.Count; i++) {
            TresholdFragment fragment = fragments[i];
            string nextPallierText = GetNextPallierText(fragment, fragments, textTreshold);
            string pallierNumberText = $"Pallier n°{i + 1} : {nextPallierText}\n";
            pallierNumberText = UIHelper.SurroundWithColor(pallierNumberText, UIHelper.GREEN);
            fragment.text = pallierNumberText + fragment.text;
        }
        texteExplicatif.mainText.text = texteExplicatif.ComputeText(textTreshold);
    }

    protected string GetNextPallierText(TresholdFragment fragment, List<TresholdFragment> fragments, int textTreshold) {
        TresholdFragment currentFragment = fragments.FindAll(f => f.treshold <= textTreshold).Last();
        if(fragment == currentFragment) {
            if (fragment == fragments.Last()) {
                return "(Dernier pallier)";
            } else {
                int nextTreshold = fragments[fragments.FindIndex(f => f == currentFragment) + 1].treshold;
                string unite = (GetStartLevelType() == MenuLevel.LevelType.INFINITE) ? "block" : "victoire";
                unite += (nextTreshold > 1) ? "s" : "";
                return $"(Prochain pallier à {nextTreshold} {unite})";
            }
        } else {
            return "";
        }
    }

    public void Return() {
        if (!selectorManager.HasSelectorPathUnlockScreenOpen())
            return;
        selectorPath.CloseUnlockScreen();
    }

    protected void SetBackgroundAccordingToLockState() {
        if (selectorPath.IsUnlocked()) {
            selectorManager.background.SetParameters(probaBackgroundUnlocked,
                distanceBackgroundUnlocked,
                decroissanceBackgroundUnlocked,
                themesBackgroundUnlocked);
        } else {
            selectorManager.background.SetParameters(probaBackgroundLocked,
                distanceBackgroundLocked,
                decroissanceBackgroundUnlocked,
                themesBackgroundLocked);
        }
    }

    protected void SetCadenasAccordingToLockState() {
        if (selectorPath.IsUnlocked()) {
            openCadena.SetActive(true);
            closedCadena.SetActive(false);
        } else {
            openCadena.SetActive(false);
            closedCadena.SetActive(true);
        }
    }

    protected void FillInputWithPasswordIfAlreayDiscovered() {
        if(selectorPath.IsUnlocked()) {
            input.text = selectorPath.GetPassword();
        } else {
            input.text = "";
        }
    }
}
