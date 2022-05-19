using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SelectorLevel : MonoBehaviour {

    public MenuLevel menuLevel;
    public SelectorLevelObject objectLevel;
    public GameObject selectorIconePrefab;

    protected SelectorManager selectorManager;

    public void Initialize(MenuBackgroundBouncing background)
    {
        selectorManager = SelectorManager.Instance;
        objectLevel.title.GetComponent<LookAtTransform>().transformToLookAt = selectorManager.baseCamera.transform;
        InitializeObject();
        menuLevel.selectorManager = selectorManager;
        menuLevel.menuBouncingBackground = background;
    }

    public void InitializeObject() {
        objectLevel.Initialize(IsHighlighted(), selectorIconePrefab);
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
        //if (!selectorManager.HasSelectorLevelOpen())
        selectorManager.TryDisplayLevel(this);
    }

    public bool IsSucceeded() {
        return menuLevel.IsSucceeded();
    }

    public bool IsAccessible() {
        return selectorManager.IsLevelAccessible(this);
    }

    protected void RewardIfNewBestScore() {
        if (menuLevel.HasJustMakeNewBestScore()) {
            /// On a plus besoin de cette Popup avec le nouveau boutton de BestScore dans le Reward ! :)
            //if (menuLevel.GetPrecedentBestScore() > 0) {
            //    RewardNewBestScore();
            //}
            menuLevel.SetNotJustMakeNewBestScore();
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
        if(selectorManager.GetOutPaths(this).Count == 0)
        {
            //if(DisplayEndBetaMessage()) {
            //    return true;
            //}
            if (DisplayEndGamePopups()) {
                return true;
            }
        }
        if (menuLevel.GetLevelType() == MenuLevel.LevelType.REGULAR) {
            hasDisplay = DisplayNewPallierMessageRegular();
        } else {
            hasDisplay = DisplayNewPallierMessageInfinite();
        }
        return hasDisplay;
    }

    protected bool DisplayEndGamePopups() {
        //selectorManager.endGamesManager.StartEndGame();
        //return true;
        if (menuLevel.GetLevelType() == MenuLevel.LevelType.REGULAR && menuLevel.HasJustWin()) {
            selectorManager.endGamesManager.StartEndGame();
            menuLevel.SetNotJustWin();
            return true;
        } else if (menuLevel.GetLevelType() == MenuLevel.LevelType.INFINITE && menuLevel.HasJustMakeNewBestScore()) {
            selectorManager.endGamesManager.StartEndGame();
            menuLevel.SetNotJustMakeNewBestScore();
            return true;
        }
        return false;
    }

    protected bool DisplayEndBetaMessage() {
        if(menuLevel.GetLevelType() == MenuLevel.LevelType.REGULAR && menuLevel.HasJustWin()) {
            selectorManager.RunPopup(
                selectorManager.strings.endBetaTitre.GetLocalizedString().Result,
                selectorManager.strings.endBetaTexte.LoadAssetAsync().Result.text,
                theme: TexteExplicatif.Theme.POSITIF);
            menuLevel.SetNotJustWin();
            return true;
        } else if (menuLevel.GetLevelType() == MenuLevel.LevelType.INFINITE && menuLevel.HasJustMakeNewBestScore()) {
            selectorManager.RunPopup(
                selectorManager.strings.endBetaTitre.GetLocalizedString().Result,
                selectorManager.strings.endBetaTexte.LoadAssetAsync().Result.text,
                theme: TexteExplicatif.Theme.POSITIF);
            menuLevel.SetNotJustMakeNewBestScore();
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
            List<SelectorPath> nextPaths = new List<SelectorPath>();
            foreach (SelectorPath selectorPath in selectorManager.GetOutPaths(this)) {
                List<int> tresholds = selectorPath.GetTresholds();
                List<int> candidateTresholds = tresholds.FindAll(t => (t <= bestScore && t > precedentBestScore));
                candidateTresholds.Sort();
                if (candidateTresholds.Count > 0) {
                    foreach (int candidateTreshold in candidateTresholds) {
                        nextLevels.Add(selectorPath.endLevel.GetVisibleName());
                        nextTresholds.Add(candidateTreshold);
                        nextPaths.Add(selectorPath);
                        selectorPath.HighlightPath(true);
                    }
                }
            }
            if (nextLevels.Count > 0) {
                DisplayNewPallierMessage(nextTresholds, nextLevels, nextPaths);
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
            List<SelectorPath> nextPaths = new List<SelectorPath>();
            foreach (SelectorPath selectorPath in selectorManager.GetOutPaths(this)) {
                List<int> tresholds = selectorPath.GetTresholds();
                foreach(int treshold in tresholds) {
                    if(treshold == 0 && precedentDataCount == 0
                    || (precedentDataCount < treshold && treshold <= currentDataCount)) {
                        nextLevels.Add(selectorPath.endLevel.GetVisibleName());
                        tresholdUnlocked.Add(treshold);
                        nextPaths.Add(selectorPath);
                        selectorPath.HighlightPath(true);
                    }
                }
            }
            if (nextLevels.Count > 0) {
                DisplayNewPallierMessage(tresholdUnlocked, nextLevels, nextPaths);
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

    protected void DisplayNewPallierMessage(List<int> tresholdsUnlocked, List<string> nextLevels, List<SelectorPath> nextPaths) {
        string congrats = "";
        nextLevels.Sort();
        for(int i = 0; i < tresholdsUnlocked.Count; i++) {
            int treshold = tresholdsUnlocked[i];
            string nextLevel = nextLevels[i];
            SelectorPath nextPath = nextPaths[i];
            string unites = UIHelper.SurroundWithColor(selectorManager.GetUnitesString(treshold, menuLevel.GetLevelType(), nextPath), UIHelper.GREEN);
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
            PrefsManager.NB_WINS_KEY,
            PrefsManager.NB_DEATHS_KEY,
            PrefsManager.SUM_OF_ALL_TRIES_SCORES_KEY,
            PrefsManager.BEST_SCORE_KEY,
            PrefsManager.PRECEDENT_BEST_SCORE_KEY,
            PrefsManager.SINCE_LAST_BEST_SCORE_KEY,
            PrefsManager.HAS_JUST_WIN_KEY,
            PrefsManager.HAS_JUST_MAKE_BEST_SCORE_KEY,
            PrefsManager.HAS_ALREADY_DISCOVER_LEVEL_KEY,
            PrefsManager.IS_LEVEL_HIGHLIGHTED_KEY,
            PrefsManager.DATA_COUNT_KEY,
            PrefsManager.PRECEDENT_DATA_COUNT_KEY,
            PrefsManager.HAS_JUST_INCREASED_DATA_COUNT_KEY,
        };
        foreach(string keySuffix in keysSuffix) {
            string key = GetNameId() + keySuffix;
            PlayerPrefs.DeleteKey(key);
        }
        Debug.Log($"Scores of level {GetNameId()} reset !");
    }

    public void SetScoresToMaxTreshold() {
        int maxTreshold = GetMaxTreshold();
        if(menuLevel.GetLevelType() == MenuLevel.LevelType.REGULAR) {
            menuLevel.SetNbWins(1);
            menuLevel.SetDataCount(maxTreshold);
            Debug.Log($"{GetNameId()} a maintenant {maxTreshold} de Data Count !");
            List<SelectorPath> outPathOfVictoryType = selectorManager.GetOutPaths(this).FindAll(path => path.goalTresholds.type == GoalManager.GoalType.VICTORY);
            if(outPathOfVictoryType.Count > 0) {
                int nbWinsNeeded = outPathOfVictoryType.Select(p => p.goalTresholds.tresholds.Max()).Max();
                menuLevel.SetNbWins(nbWinsNeeded);
                Debug.Log($"{GetNameId()} a maintenant un nombre de victoires de {nbWinsNeeded} !");
            }
        }
        if(menuLevel.GetLevelType() == MenuLevel.LevelType.INFINITE) {
            menuLevel.SetBestScore(maxTreshold);
            menuLevel.SetPrecedentBestScore(maxTreshold);
            Debug.Log($"{GetNameId()} a maintenant un Meilleur Score de {maxTreshold} !");
        }
    }

    protected int GetMaxTreshold() {
        List<SelectorPath> paths = selectorManager.GetPaths().FindAll(p => p.startLevel == this);
        List<int> maxTresholds = paths.Select(p => p.GetMaxTreshold()).ToList();
        if(maxTresholds.Count == 0) {
            return 0;
        }
        return maxTresholds.Max();
    }

    public void DisplayInitialPopupIn(float time) {
        StartCoroutine(CDisplayInitialPopupIn(time));
    }

    protected IEnumerator CDisplayInitialPopupIn(float time) {
        yield return new WaitForSeconds(time);
        DisplayInitialPopup();
    }
}
