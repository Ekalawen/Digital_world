using System;
using System.Collections;
using System.Collections.Generic;
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
        selectorPath.Unlock(input.text);
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
        selectorManager.RunPopup("Mot de passe érroné.",
            "Ce n'est pas le bon mot de passe.\n" +
            "Réussissez ce niveau pour obtenir le mot de passe dans les Données Hackées().\n" +
            "\n" +
            "Bonne Chance !",
            TexteExplicatif.Theme.NEGATIF);
    }

    protected void SubmitFalseUnlocked() {
        string password = selectorPath.GetPassword();
        selectorManager.popup.Initialize(
            title: "C'était mieux avant.",
            mainText: "Ce Path() est déjà dévérouillé !\n" +
            "Et pourtant ce n'est plus le bon mot de passe.\n" +
            $"Le mot de passe était {password}\n",
            theme: TexteExplicatif.Theme.NEUTRAL);
        selectorManager.popup.AddReplacement(password, $"<color=green>{password}</color>");
        selectorManager.popup.Run();
    }

    public void SubmitIfEnter() {
        if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
            Submit();
    }

    public void OpenDonneesHackees() {
        string key = selectorPath.startLevel.menuLevel.textLevelName.text + MenuLevel.NB_WINS_KEY;
        int nbVictoires = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : 0;
        if (nbVictoires == 0) {
            selectorManager.RunPopup(
                title: "Données Hackées",
                text: "Vous n'avez encore jamais hackée niveau.\n" +
                "Aucunes données accessibles.",
                theme: TexteExplicatif.Theme.NEUTRAL);
        } else {
            selectorManager.popup.Initialize(
                title: "Données Hackées",
                useTextAsset: true,
                textAsset: selectorPath.donneesHackees,
                theme: TexteExplicatif.Theme.POSITIF);
            AddReplacementForDonneesHackeesToPopup(selectorManager.popup);
            selectorManager.popup.Run(textTreshold: nbVictoires);
            AddNextPallierMessageToAllFragments(selectorManager.popup, textTreshold: nbVictoires);
        }
    }

    public static string SurroundWithBlueColor(Match match) {
        return "<color=blue>" + match.Value + "</color>";
    }

    protected void AddReplacementForDonneesHackeesToPopup(TexteExplicatif popup) {
        popup.AddReplacement("%Trace%", selectorPath.GetTrace());
        popup.AddReplacement("%Passe%", selectorPath.passwordPasse);
        MatchEvaluator evaluator = new MatchEvaluator(SurroundWithBlueColor);
        popup.AddReplacementEvaluator(@"Passes?", evaluator);
        popup.AddReplacementEvaluator(@"Traces?", evaluator);
        // L'ajout des next palliers se fait dans la fonction AddNextPallierMessageToAllFragments()
    }

    protected void AddNextPallierMessageToAllFragments(TexteExplicatif texteExplicatif, int textTreshold) {
        TresholdText tresholdText = texteExplicatif.GetTresholdText();
        List<TresholdFragment> fragments = tresholdText.GetAllFragmentsOrdered();
        for (int i = 0; i < fragments.Count; i++) {
            if (i < fragments.Count - 1) {
                int nextTreshold = fragments[i + 1].treshold;
                fragments[i].ApplyReplacementEvaluator(
                    new Tuple<string, MatchEvaluator>(@"$(?![\r\n])", // Match EOF
                    (Match match) => "Prochain pallier à " + nextTreshold + " victoires.\n\n\n"));
            } else {
                fragments[i].ApplyReplacementEvaluator(
                    new Tuple<string, MatchEvaluator>(@"$(?![\r\n])", // Match EOF
                    (Match match) => "Dernier pallier.\n\n\n"));
            }
        }
        texteExplicatif.mainText.text = texteExplicatif.ComputeText(textTreshold);
    }


    public void Return() {
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
        }
    }
}
