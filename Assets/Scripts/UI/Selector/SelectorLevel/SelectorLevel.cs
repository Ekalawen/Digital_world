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

    public string GetVisibleName() {
        return menuLevel.GetVisibleName();
    }

    public string GetNameId() {
        return menuLevel.GetNameId();
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
        string keyHasBestScore = GetNameId() + MenuLevel.HAS_JUST_MAKE_BEST_SCORE_KEY;
        string keyPrecedentBestScore = GetNameId() + MenuLevel.PRECEDENT_BEST_SCORE_KEY;
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
        selectorManager.RunPopup(selectorManager.strings.meilleurScoreTitre.GetLocalizedString().Result,
            selectorManager.strings.meilleurScoreTexte.GetLocalizedString(bestScoreString).Result,
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
        if(selectorManager.GetOutPaths(this).Count == 0) {
            if(DisplayEndBetaMessage()) {
                return true;
            }
        }
        if (menuLevel.levelType == MenuLevel.LevelType.REGULAR) {
            hasDisplay = DisplayNewPallierMessageRegular();
        } else {
            hasDisplay = DisplayNewPallierMessageInfinite();
        }
        return hasDisplay;
    }

    protected bool DisplayEndBetaMessage() {
        if(menuLevel.HasJustWin()) {
            selectorManager.RunPopup(
                selectorManager.strings.endBetaTitre.GetLocalizedString().Result,
                selectorManager.strings.endBetaTexte.LoadAssetAsync().Result.text,
                theme: TexteExplicatif.Theme.POSITIF);
            menuLevel.SetNotJustWin();
            return true;
        }
        return false;
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
                        nextLevels.Add(selectorPath.endLevel.GetVisibleName());
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
        if (menuLevel.GetNbWins() > 0 && menuLevel.HasJustIncreaseDataCount()) {
            List<int> tresholdUnlocked = new List<int>();
            int currentDataCount = menuLevel.GetDataCount();
            int precedentDataCount = menuLevel.GetPrecedentDataCount();
            List<string> nextLevels = new List<string>();
            foreach (SelectorPath selectorPath in selectorManager.GetOutPaths(this)) {
                List<int> tresholds = selectorPath.GetTresholds();
                foreach(int treshold in tresholds) {
                    if(treshold == 0 && precedentDataCount == 0
                    || (precedentDataCount < treshold && treshold <= currentDataCount)) {
                        nextLevels.Add(selectorPath.endLevel.GetVisibleName());
                        tresholdUnlocked.Add(treshold);
                        selectorPath.HighlightPath(true);
                    }
                }
            }
            if (nextLevels.Count > 0) {
                DisplayNewPallierMessage(tresholdUnlocked, nextLevels);
                hasDisplayed = true;
                menuLevel.SetNotJustWin();
                menuLevel.SetNotJustIncreaseDataCount();
            }
        }
        return hasDisplayed;
    }

    public bool IsHighlighted() {
        List<SelectorPath> outPaths = selectorManager.GetOutPaths(this);
        return IsAccessible() && (outPaths.Any(p => !p.IsUnlocked()) || outPaths.Count == 0);
    }

    protected void DisplayNewPallierMessage(List<int> tresholdsUnlocked, List<string> nextLevels) {
        string congrats = "";
        nextLevels.Sort();
        for(int i = 0; i < tresholdsUnlocked.Count; i++) {
            int treshold = tresholdsUnlocked[i];
            string nextLevel = nextLevels[i];
            string unites = UIHelper.SurroundWithColor(selectorManager.GetUnitesString(treshold, menuLevel.levelType), UIHelper.GREEN);
            string levelTitle = UIHelper.SurroundWithColor(nextLevel, UIHelper.BLUE);
            congrats += selectorManager.strings.palierDebloqueUnPalier.GetLocalizedString(treshold, unites, levelTitle).Result;
            congrats += '\n';
        }

        string dataHackeesString = selectorManager.strings.dataHackees.GetLocalizedString().Result;
        selectorManager.popup.AddReplacement(dataHackeesString, UIHelper.SurroundWithColor(dataHackeesString, UIHelper.ORANGE));
        selectorManager.RunPopup(
            selectorManager.strings.palierDebloqueTitre.GetLocalizedString().Result,
            selectorManager.strings.palierDebloqueTexte.GetLocalizedString(congrats, tresholdsUnlocked.Count).Result,
            theme: TexteExplicatif.Theme.POSITIF,
            cleanReplacements: false);
        selectorManager.popup.HighlightDoneButton(true);
    }

    protected bool DisplayPopupUnlockLevel() {
        bool hasDisplayed = false;
        if (!menuLevel.HasAlreadyDiscoverLevel()) {
            bool shouldCongrats = !menuLevel.displayArchivesOnUnlock;
            if (shouldCongrats) {
                string levelString = UIHelper.SurroundWithColor(GetVisibleName(), UIHelper.BLUE);
                selectorManager.RunPopup(
                    title: selectorManager.strings.niveauDebloqueTitre.GetLocalizedString().Result,
                    text: selectorManager.strings.niveauDebloqueTexte.GetLocalizedString(levelString).Result,
                    theme: TexteExplicatif.Theme.POSITIF);
            } else {
                menuLevel.OpenArchives();
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
            Lumiere.DATA_COUNT_KEY,
            Lumiere.PRECEDENT_DATA_COUNT_KEY,
            Lumiere.HAS_JUST_INCREASED_DATA_COUNT_KEY,
        };
        foreach(string keySuffix in keysSuffix) {
            string key = GetNameId() + keySuffix;
            PlayerPrefs.DeleteKey(key);
        }
        Debug.Log($"Scores of level {GetNameId()} reset !");
    }

    public void SetScoresToMaxTreshold() {
        int maxTreshold = GetMaxTreshold();
        if(menuLevel.levelType == MenuLevel.LevelType.REGULAR) {
            menuLevel.SetNbWins(1);
            menuLevel.SetDataCount(maxTreshold);
            Debug.Log($"{GetNameId()} a maintenant {maxTreshold} de Data Count !");
        }
        if(menuLevel.levelType == MenuLevel.LevelType.INFINITE) {
            menuLevel.SetBestScore(maxTreshold);
            menuLevel.SetPrecedentBestScore(maxTreshold);
            Debug.Log($"{GetNameId()} a maintenant un Meilleur Score de {maxTreshold} !");
        }
    }

    protected int GetMaxTreshold() {
        List<SelectorPath> paths = selectorManager.GetPaths().FindAll(p => p.startLevel == this);
        List<int> maxTresholds = paths.Select(p => p.GetMaxTreshold()).ToList();
        return maxTresholds.Max();
    }
}
