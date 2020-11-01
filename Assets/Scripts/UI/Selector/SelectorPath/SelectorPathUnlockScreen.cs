﻿using System;
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
        SetTitle();
    }

    protected void SetTitle() {
        string startLevelName = selectorPath.startLevel.GetName();
        fromLevelTitle.text = startLevelName;
        string endLevelName = selectorPath.endLevel.GetName();
        toLevelTitle.text = endLevelName;
    }

    public void Submit() {
        if (input.text == selectorPath.GetPassword() || input.text == MenuLevel.SUPER_CHEATED_PASSWORD) {
            selectorPath.Unlock(input.text);
            SetBackgroundAccordingToLockState();
        } else {
            selectorManager.RunPopup("Mot de passe érroné.",
                "Ce n'est pas le bon mot de passe.\n" +
                "Réussissez ce niveau pour obtenir le mot de passe dans les Données Hackées().\n" +
                "\n" +
                "Bonne Chance !",
                TexteExplicatif.Theme.NEGATIF);
        }
    }

    public void OpenDonneesHackees() {
        string key = selectorPath.startLevel.menuLevel.textLevelName.text + MenuLevel.NB_WINS_KEY;
        int nbVictoires = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : 0;
        if (nbVictoires == 0) {
            selectorPath.donneesHackees.Run(textTreshold: 0);
        } else {
            selectorPath.donneesHackees.Run(textTreshold: nbVictoires);
            AddNextPallierMessageToAllFragments(selectorPath.donneesHackees, textTreshold: nbVictoires);
        }
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
        texteExplicatif.ComputeText(textTreshold);
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
}
