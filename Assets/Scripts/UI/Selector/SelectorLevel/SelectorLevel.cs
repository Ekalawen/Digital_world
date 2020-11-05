using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectorLevel : MonoBehaviour {

    public MenuLevel menuLevel;
    public SelectorLevelObject objectLevel;
    public bool resetScores = false;

    protected SelectorManager selectorManager;

    public void Initialize(MenuBackgroundBouncing background) {
        selectorManager = SelectorManager.Instance;
        ResetScores();
        objectLevel.title.GetComponent<LookAtTransform>().transformToLookAt = selectorManager.camera.transform;
        objectLevel.Initialize();
        menuLevel.selectorManager = selectorManager;
        menuLevel.menuBouncingBackground = background;
        RewardIfNewBestScore();
    }

    public void OnMouseEnter() {
    }

    public void OnMouseExit() {
    }

    public string GetName() {
        return menuLevel.textLevelName.text;
    }

    public void OnMouseDown() {
        if (!selectorManager.HasSelectorLevelOpen())
            selectorManager.TryDisplayLevel(this);
    }

    public bool IsSucceeded() {
        return menuLevel.IsSucceeded();
    }

    public bool IsAccessible() {
        return selectorManager.IsLevelAccessible(this);
    }

    protected void RewardIfNewBestScore() {
        string keyHasBestScore = GetName() + MenuLevel.HAS_JUST_MAKE_BEST_SCORE_KEY;
        if (PlayerPrefs.HasKey(keyHasBestScore)) {
            RewardNewBestScore();
            PlayerPrefs.DeleteKey(keyHasBestScore);
        }
    }

    protected void RewardNewBestScore() {
        string bestScoreString = menuLevel.GetBestScoreToString();
        selectorManager.popup.CleanReplacements();
        selectorManager.popup.AddReplacement(bestScoreString, $"<color=green>{bestScoreString}</color>");
        selectorManager.RunPopup("MEILLEUR SCORE !!!",
            "Incroyable !\n" +
            "Vous avez battu votre record et fais le Meilleur Score !\n" +
            $"Vous avez fait {bestScoreString} !",
            TexteExplicatif.Theme.POSITIF,
            cleanReplacements: false);
    }

    protected void ResetScores() {
        if (resetScores) {
            List<string> keysSuffix = new List<string>() {
                MenuLevel.NB_WINS_KEY,
                MenuLevel.NB_DEATHS_KEY,
                MenuLevel.SUM_OF_ALL_TRIES_SCORES_KEY,
                MenuLevel.HIGHEST_SCORE_KEY,
                MenuLevel.SINCE_LAST_BEST_SCORE_KEY,
                MenuLevel.HAS_JUST_WIN_KEY,
                MenuLevel.HAS_JUST_MAKE_BEST_SCORE_KEY,
            };
            foreach(string keySuffix in keysSuffix) {
                string key = GetName() + keySuffix;
                PlayerPrefs.DeleteKey(key);
            }
        }
    }
}
