using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class SelectorLevel : MonoBehaviour {

    public MenuLevel menuLevel;
    public SelectorLevelObject objectLevel;

    protected SelectorManager selectorManager;

    public void Initialize(MenuBackgroundBouncing background)
    {
        selectorManager = SelectorManager.Instance;
        objectLevel.title.GetComponent<LookAtTransform>().transformToLookAt = selectorManager.camera.transform;
        InitializeObject();
        menuLevel.selectorManager = selectorManager;
        menuLevel.menuBouncingBackground = background;
    }

    public void InitializeObject() {
        objectLevel.Initialize(IsHighlighted());
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
        string keyPrecedentBestScore = GetName() + MenuLevel.PRECEDENT_BEST_SCORE_KEY;
        if (PlayerPrefs.HasKey(keyHasBestScore)) {
            if (PlayerPrefs.GetFloat(keyPrecedentBestScore) > 0) {
                RewardNewBestScore();
            }
            PlayerPrefs.DeleteKey(keyHasBestScore);
        }
    }

    protected void RewardNewBestScore() {
        string bestScoreString = menuLevel.GetBestScoreToString();
        selectorManager.popup.CleanReplacements();
        selectorManager.popup.AddReplacement(bestScoreString, UIHelper.SurroundWithColor(bestScoreString, UIHelper.GREEN));
        selectorManager.RunPopup("MEILLEUR SCORE !!!",
            "Incroyable !\n" +
            "Vous avez battu votre record et fais le Meilleur Score !\n" +
            $"Vous avez fait {bestScoreString} !",
            TexteExplicatif.Theme.POSITIF,
            cleanReplacements: false);
    }

    public void DisplayInitialPopup()
    {
        bool hasDisplay = false;
        hasDisplay = DisplayPopupUnlockLevel();
        if (hasDisplay)
            return;
        hasDisplay = DisplayNewPallierMessage();
        if (hasDisplay) {
            menuLevel.SetNotJustMakeNewBestScore();
            return;
        }
        RewardIfNewBestScore();
    }

    private bool DisplayNewPallierMessage() {
        bool hasDisplay = false;
        if (menuLevel.levelType == MenuLevel.LevelType.REGULAR) {
            hasDisplay = DisplayNewPallierMessageRegular();
        } else {
            hasDisplay = DisplayNewPallierMessageInfinite();
        }
        return hasDisplay;
    }

    protected bool DisplayNewPallierMessageInfinite() {
        bool hasDisplayed = false;
        if (menuLevel.HasJustMakeNewBestScore()) {
            int bestScore = (int)menuLevel.GetBestScore();
            int precedentBestScore = (int)menuLevel.GetPrecedentBestScore();
            List<string> nextLevels = new List<string>();
            List<int> nextTresholds = new List<int>();
            foreach (SelectorPath selectorPath in selectorManager.GetOutPaths(this)) {
                List<int> tresholds = selectorPath.GetTresholds();
                List<int> candidateTresholds = tresholds.FindAll(t => (t <= bestScore && t > precedentBestScore));
                candidateTresholds.Sort();
                if (candidateTresholds.Count > 0) {
                    foreach (int candidateTreshold in candidateTresholds) {
                        nextLevels.Add(selectorPath.endLevel.GetName());
                        nextTresholds.Add(candidateTreshold);
                        selectorPath.HighlightPath(true);
                    }
                }
            }
            if (nextLevels.Count > 0) {
                DisplayNewPallierMessage(nextTresholds, nextLevels);
                hasDisplayed = true;
                menuLevel.SetNotJustMakeNewBestScore();
            }
        }
        return hasDisplayed;
    }

    protected bool DisplayNewPallierMessageRegular() {
        bool hasDisplayed = false;
        if (menuLevel.HasJustWin()) {
            int nbWins = menuLevel.GetNbWins();
            List<string> nextLevels = new List<string>();
            foreach (SelectorPath selectorPath in selectorManager.GetOutPaths(this)) {
                List<int> tresholds = selectorPath.GetTresholds();
                if (tresholds.Contains(nbWins)) {
                    nextLevels.Add(selectorPath.endLevel.GetName());
                    selectorPath.HighlightPath(true);
                }
            }
            if (nextLevels.Count > 0) {
                DisplayNewPallierMessage(nbWins, nextLevels);
                hasDisplayed = true;
                menuLevel.SetNotJustWin();
            }
        }
        return hasDisplayed;
    }

    public bool IsHighlighted() {
        return IsAccessible() && selectorManager.GetOutPaths(this).Any(p => !p.IsUnlocked());
    }

    protected void DisplayNewPallierMessage(int nbWin, List<string> nextLevels) {
        List<int> nbWins = new List<int>();
        for (int i = 0; i < nextLevels.Count; i++)
            nbWins.Add(nbWin);
        DisplayNewPallierMessage(nbWins, nextLevels);
    }
    protected void DisplayNewPallierMessage(List<int> nbWins, List<string> nextLevels) {
        string congrats = "";
        nextLevels.Sort();
        for(int i = 0; i < nbWins.Count; i++) {
            int nbWin = nbWins[i];
            string nextLevel = nextLevels[i];
            string de = $"de{(nbWin > 1 ? "s" : "")}";
            string unite = (menuLevel.levelType == MenuLevel.LevelType.REGULAR) ? "victoire" : "block";
            string victoire = $"{unite}{(nbWin > 1 ? "s" : "")}";
            congrats += $"Pallier " +
                $"{de} {UIHelper.SurroundWithColor($"{nbWin} {victoire}", UIHelper.GREEN)} vers le niveau " +
                $"{UIHelper.SurroundWithColor(nextLevel, UIHelper.BLUE)} débloqué !\n";
        }
        string le = $"le{((nbWins.Count > 1) ? "s" : "")}";
        selectorManager.popup.AddReplacement("Data Hackées()", UIHelper.SurroundWithColor("Data Hackées()", UIHelper.ORANGE));
        selectorManager.RunPopup(
            "Pallier débloqué !",
            "Félicitations !\n" + 
            congrats +
            $"Allez {le} consulter dans les Data Hackées() !",
            theme: TexteExplicatif.Theme.POSITIF,
            cleanReplacements: false);
        selectorManager.popup.HighlightDoneButton(true);
    }

    protected bool DisplayPopupUnlockLevel() {
        bool hasDisplayed = false;
        if (!menuLevel.HasAlreadyDiscoverLevel()) {
            SelectorLevelRunIntroduction introductionRunner = menuLevel.gameObject.GetComponent<SelectorLevelRunIntroduction>();
            bool shouldCongrats = introductionRunner == null;
            if (shouldCongrats) {
                selectorManager.RunPopup(
                    title: "Niveau débloqué !",
                    text: $"Félicitation ! Vous venez de débloquer le niveau {UIHelper.SurroundWithColor(GetName(), UIHelper.BLUE)} !\n" + 
                    "Continuez comme ça !\n" +
                    "Et Happy Hacking ! :)",
                    theme: TexteExplicatif.Theme.POSITIF);
            } else {
                introductionRunner.RunIntroduction();
            }
            hasDisplayed = true;
            menuLevel.SetAlreadyDiscoverLevel();
        }
        return hasDisplayed;
    }

    public void ResetScores() {
        List<string> keysSuffix = new List<string>() {
            MenuLevel.NB_WINS_KEY,
            MenuLevel.NB_DEATHS_KEY,
            MenuLevel.SUM_OF_ALL_TRIES_SCORES_KEY,
            MenuLevel.BEST_SCORE_KEY,
            MenuLevel.PRECEDENT_BEST_SCORE_KEY,
            MenuLevel.SINCE_LAST_BEST_SCORE_KEY,
            MenuLevel.HAS_JUST_WIN_KEY,
            MenuLevel.HAS_JUST_MAKE_BEST_SCORE_KEY,
            MenuLevel.HAS_ALREADY_DISCOVER_LEVEL_KEY,
            MenuLevel.IS_LEVEL_HIGHLIGHTED_KEY,
        };
        foreach(string keySuffix in keysSuffix) {
            string key = GetName() + keySuffix;
            PlayerPrefs.DeleteKey(key);
        }
        Debug.Log($"Scores of level {GetName()} reset !");
    }

    public void SetScoresToMaxTreshold() {
        int maxTreshold = GetMaxTreshold();
        if(menuLevel.levelType == MenuLevel.LevelType.REGULAR) {
            menuLevel.SetNbWins(maxTreshold);
            Debug.Log($"{GetName()} a maintenant {maxTreshold} victoires !");
        }
        if(menuLevel.levelType == MenuLevel.LevelType.INFINITE) {
            menuLevel.SetBestScore(maxTreshold);
            menuLevel.SetPrecedentBestScore(maxTreshold);
            Debug.Log($"{GetName()} a maintenant un Meilleur Score de {maxTreshold} !");
        }
    }

    protected int GetMaxTreshold() {
        List<SelectorPath> paths = selectorManager.GetPaths().FindAll(p => p.startLevel == this);
        List<int> maxTresholds = paths.Select(p => p.GetMaxTreshold()).ToList();
        return maxTresholds.Max();
    }
}
