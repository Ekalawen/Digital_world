using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class MenuLevel : MonoBehaviour {

    public enum LevelType { REGULAR, INFINITE };

    public static string LEVEL_NAME_KEY = "levelNameKey";
    public static string CURRENT_INPUT_FIELD_KEY = "currentInputField";
    public static string NB_WINS_KEY = "nbVictoires";
    public static string NB_DEATHS_KEY = "nbTries";
    public static string SUM_OF_ALL_TRIES_SCORES_KEY = "sumOfAllTriesScores";
    public static string BEST_SCORE_KEY = "bestScore";
    public static string PRECEDENT_BEST_SCORE_KEY = "precedentBestScore";
    public static string SINCE_LAST_BEST_SCORE_KEY = "sinceLastBestScore";
    public static string TRACE_KEY = "trace";
    public static string HAS_JUST_WIN_KEY = "hasJustWin";
    public static string HAS_JUST_MAKE_BEST_SCORE_KEY = "hasJustMakeBestScore";
    public static string HAS_ALREADY_DISCOVER_LEVEL_KEY = "hasAlreadyDiscoverLevel";
    public static string SUPER_CHEATED_PASSWORD = "lecreateurdecejeuestmonuniquedieuetmaitre";


    [Header("Name")]
    public string levelSceneName;
    public string levelFolderName;
    public LevelType levelType;

    [Header("Background")]
    public float probaSource = 0.00035f;
    public int distanceSource = 8;
    public float decroissanceSource = 0.01f;
    public List<ColorSource.ThemeSource> themes;

    [Header("Links to Level")]
    public string nextPassword = "passwd";
    public GameObject joueurPrefab;
    public GameObject consolePrefab;

    [Header("Textes Explicatifs")]
    public TexteExplicatif texteInformations;
    public TexteExplicatif texteExplicatifPasswdError;
    public TexteExplicatif texteExplicatifDonneesHackes;
    public TexteExplicatif texteExplicatifDonneesHackesSuccess;
    public TexteExplicatif texteExplicatifIntroduction;

    [Header("Scores")]
    public GameObject scoresRegular;
    public GameObject scoresInfinite;
    public TMPro.TMP_Text score_nbTries;
    public TMPro.TMP_Text score_nbWins;
    public TMPro.TMP_Text score_winrate;
    public TMPro.TMP_Text score_highestScore;
    public TMPro.TMP_Text score_nbGames;
    public TMPro.TMP_Text score_meanScore;
    public TMPro.TMP_Text score_sinceLastBestScore;
    public TMPro.TMP_Text score_bestScore;

    [Header("Other Links")]
    public SelectorManager selectorManager;
    public MenuBackgroundBouncing menuBouncingBackground;
    public TMPro.TMP_Text textLevelName;
    public InputField inputFieldNext;

    [HideInInspector]
    public MenuLevelSelector menuLevelSelector;
    protected bool playStarted = false;

    public void Initialize() {
        menuBouncingBackground.SetParameters(probaSource, distanceSource, decroissanceSource, themes);
        SetScores();

        MenuManager.DISABLE_HOTKEYS = false;
        InitTextesExplicatifs();

        /// This is now done in SelectorLevel !
        //DisplayPopupUnlockLevel();
        //DisplayPopupUnlockNewTreshold();

        string key = GetName() + CURRENT_INPUT_FIELD_KEY;
        inputFieldNext.text = PlayerPrefs.GetString(key);
        UIHelper.FitTextHorizontaly(textLevelName.text, textLevelName);
    }

    private void Update() {
        // Si on appui sur Echap on quitte
        if (!MenuManager.DISABLE_HOTKEYS) {
            if (Input.GetKeyDown(KeyCode.Return)
            || Input.GetKeyDown(KeyCode.KeypadEnter)
            || Input.GetKeyDown(KeyCode.Space)) {
                Play();
            }
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) {
                Next();
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.Q)) {
                Previous();
            }
            if (Input.GetKeyDown(KeyCode.Escape)) {
                Back();
            }
        }
    }

    public void Play() {
        menuLevelSelector?.Play(levelSceneName);
        selectorManager?.Play(levelSceneName);
    }

    public void Next() {
        if (inputFieldNext.text == GetPassword()) {
            menuLevelSelector?.Next();
            selectorManager?.Next();
        } else if (inputFieldNext.text == SUPER_CHEATED_PASSWORD) {
            menuLevelSelector?.Next();
            selectorManager?.Next();
        } else {
            texteExplicatifPasswdError.Run();
        }
    }

    public void NextIfEnter() {
        if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
            Next();
        else
            texteInformations.EnableHotkeysNextFrame();
    }

    public void Previous() {
        menuLevelSelector?.Previous();
        selectorManager?.Previous();
    }

    public void Back() {
        menuLevelSelector?.Back();
        selectorManager?.BackToSelector();
    }

    public void OpenInformations() {
        if (selectorManager != null && !selectorManager.HasSelectorLevelOpen())
            return;
        texteInformations.Run(GetNbWins());
    }

    public void OpenDonneesHackes() {
        if (selectorManager != null && !selectorManager.HasSelectorLevelOpen())
            return;
        // Changer le texte des données hackés en fonction du nombre de fois où l'on a gagné ce niveau !
        string key = textLevelName.text + NB_WINS_KEY;
        int nbVictoires = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : 0;
        if (nbVictoires == 0) {
            texteExplicatifDonneesHackes.Run(GetNbWins());
        } else {
            texteExplicatifDonneesHackesSuccess.Run(GetNbWins());
            AddNextPallierMessageToAllFragments();
        }
    }

    protected void AddNextPallierMessageToAllFragments() {
        TresholdText tresholdText = texteExplicatifDonneesHackesSuccess.GetTresholdText();
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
        texteExplicatifDonneesHackesSuccess.mainText.text = texteExplicatifDonneesHackesSuccess.ComputeText(GetNbWins());
    }

    public void SaveNextInputField() {
        if (inputFieldNext.text == GetPassword() || inputFieldNext.text == SUPER_CHEATED_PASSWORD) {
            string key = textLevelName.text + CURRENT_INPUT_FIELD_KEY;
            PlayerPrefs.SetString(key, inputFieldNext.text);
        }
    }

    protected void SetScores() {
        if (levelType == LevelType.REGULAR) {
            scoresRegular.SetActive(true);
            scoresInfinite.SetActive(false);
            SetRegularScores();
        } else {
            scoresRegular.SetActive(false);
            scoresInfinite.SetActive(true);
            SetInfiniteScores();
        }
    }

    protected void SetRegularScores() {
        score_nbWins.text = ChangeLastWord(score_nbWins.text, GetNbWins().ToString());
        score_nbTries.text = ChangeLastWord(score_nbTries.text, GetNbDeaths().ToString());
        string winrateString = (100.0f * GetWinrate()).ToString("N2") + "%";
        score_winrate.text = ChangeLastWord(score_winrate.text, winrateString);
        score_highestScore.text = ChangeLastWord(score_highestScore.text, GetBestScoreToString());
    }

    protected void SetInfiniteScores() {
        score_nbGames.text = ChangeLastWord(score_nbGames.text, GetNbTries().ToString());
        score_meanScore.text = ChangeLastWord(score_meanScore.text, ((int)GetMeanScore()).ToString());
        score_sinceLastBestScore.text = ChangeLastWord(score_sinceLastBestScore.text, GetSinceLastBestScore().ToString());
        score_bestScore.text = ChangeLastWord(score_bestScore.text, GetBestScoreToString());
    }

    public int GetNbWins() {
        string key = textLevelName.text + NB_WINS_KEY;
        return PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : 0;
    }

    public void SetNbWins(int nbWins) {
        string key = textLevelName.text + NB_WINS_KEY;
        PlayerPrefs.SetInt(key, nbWins);
    }

    public int GetNbDeaths() {
        string key = textLevelName.text + NB_DEATHS_KEY;
        return PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : 0;
    }

    public int GetNbTries() {
        return GetNbWins() + GetNbDeaths();
    }

    public float GetWinrate() {
        int nbTries = GetNbTries();
        if (nbTries == 0)
            return 0;
        return (float)GetNbWins() / (float)nbTries;
    }

    public float GetMeanScore() {
        float nbTries = GetNbTries();
        if (nbTries > 0)
            return GetSumOfAllTriesScores() / (float)GetNbTries();
        else
            return 0;
    }

    public float GetSumOfAllTriesScores() {
        string key = textLevelName.text + SUM_OF_ALL_TRIES_SCORES_KEY;
        return PlayerPrefs.HasKey(key) ? PlayerPrefs.GetFloat(key) : 0;
    }

    public bool HasBestScore() {
        string key = textLevelName.text + BEST_SCORE_KEY;
        return PlayerPrefs.HasKey(key);
    }

    public float GetBestScore() {
        string key = textLevelName.text + BEST_SCORE_KEY;
        return PlayerPrefs.HasKey(key) ? PlayerPrefs.GetFloat(key) : 0.0f;
    }

    public void SetBestScore(float bestScore) {
        string key = textLevelName.text + BEST_SCORE_KEY;
        PlayerPrefs.SetFloat(key, bestScore);
    }

    public float GetPrecedentBestScore() {
        string key = textLevelName.text + PRECEDENT_BEST_SCORE_KEY;
        return PlayerPrefs.HasKey(key) ? PlayerPrefs.GetFloat(key) : 0.0f;
    }

    public void SetPrecedentBestScore(float precedentScore) {
        string key = textLevelName.text + PRECEDENT_BEST_SCORE_KEY;
        PlayerPrefs.SetFloat(key, precedentScore);
    }

    public string GetBestScoreToString() {
        if(levelType == LevelType.REGULAR)
            return (HasBestScore()) ? GetBestScore().ToString("N2") : "null";
        else
            return (HasBestScore()) ? ((int)GetBestScore()).ToString() : "null";
    }

    public int GetSinceLastBestScore() {
        string key = textLevelName.text + SINCE_LAST_BEST_SCORE_KEY;
        return PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : 0;
    }

    public static string ChangeLastWord(string str, string lastWordReplacement) {
        string[] splited = str.Split(' ');
        splited[splited.Length - 1] = lastWordReplacement;
        return string.Join(" ", splited);
    }

    public string GetTrace() {
        string key = textLevelName.text + TRACE_KEY;
        if (!PlayerPrefs.HasKey(key))
            InitTrace();
        return PlayerPrefs.GetString(key);
    }

    protected void InitTrace() {
        string trace = Trace.GenerateTrace();
        print(trace);

        string key = textLevelName.text + TRACE_KEY;
        PlayerPrefs.SetString(key, trace);
    }

    public string GetName() {
        return textLevelName.text;
    }

    public string GetPasse() {
        return nextPassword;
    }

    public string GetPassword() {
        return GetPasse() + GetTrace();
    }

    public static string SurroundWithBlueColor(Match match) {
        return UIHelper.SurroundWithColor(match.Value, UIHelper.BLUE);
    }

    protected void InitTextesExplicatifs() {
        string rootPath = "Assets/Texts/Levels/" + levelFolderName + "/";
        texteInformations.SetRootPath(rootPath);
        texteExplicatifPasswdError.SetRootPath(rootPath);
        texteExplicatifDonneesHackes.SetRootPath(rootPath);
        texteExplicatifDonneesHackesSuccess.SetRootPath(rootPath);
        if(texteExplicatifIntroduction != null)
            texteExplicatifIntroduction.SetRootPath(rootPath);

        MatchEvaluator evaluator = new MatchEvaluator(SurroundWithBlueColor);
        texteExplicatifDonneesHackesSuccess.AddReplacement("%Trace%", GetTrace());
        texteExplicatifDonneesHackesSuccess.AddReplacement("%Passe%", nextPassword);
        texteExplicatifDonneesHackesSuccess.AddReplacementEvaluator(@"Passes?", evaluator);
        texteExplicatifDonneesHackesSuccess.AddReplacementEvaluator(@"Traces?", evaluator);
        // L'ajout des next palliers se fait dans la fonction AddNextPallierMessageToAllFragments()

        texteInformations.AddReplacement("[Cilliannelle Crittefigiée]", UIHelper.SurroundWithColor("[Cilliannelle Crittefigiée]", UIHelper.PURE_GREEN));
        texteInformations.AddReplacement("[Morgensoul*]", UIHelper.SurroundWithColor("[Morgensoul*]", UIHelper.CYAN));
        texteInformations.AddReplacement("[V1P3R]", UIHelper.SurroundWithColor("[V1P3R]", UIHelper.CYAN));
    }

    public bool HasJustWin() {
        string key = GetName() + HAS_JUST_WIN_KEY;
        return PlayerPrefs.HasKey(key) && PlayerPrefs.GetString(key) == MenuManager.TRUE;
    }

    public void SetNotJustWin() {
        string key = GetName() + HAS_JUST_WIN_KEY;
        PlayerPrefs.DeleteKey(key);
    }

    public bool HasJustMakeNewBestScore() {
        string key = GetName() + HAS_JUST_MAKE_BEST_SCORE_KEY;
        return PlayerPrefs.HasKey(key) && PlayerPrefs.GetString(key) == MenuManager.TRUE;
    }

    public void SetNotJustMakeNewBestScore() {
        string key = GetName() + HAS_JUST_MAKE_BEST_SCORE_KEY;
        PlayerPrefs.DeleteKey(key);
    }

    protected void DisplayPopupUnlockNewTreshold() {
        if (HasJustWin()) {
            List<int> tresholds = texteExplicatifDonneesHackesSuccess.GetAllTresholds();
            if(tresholds.Contains(GetNbWins())) {
                MenuManager.Instance.RunPopup(
                    "Pallier débloqué !", 
                    "Félicitation ! Vous venez de débloquer le pallier de" + (GetNbWins() > 1 ? "s" : "") + " " + GetNbWins() + " victoire" + ((GetNbWins() > 1) ? "s" : "") + " !\nAllez le consulter dans les Données Hackées !",
                    TexteExplicatif.Theme.POSITIF);
            }
            SetNotJustWin();
        }
    }
    
    protected void DisplayPopupUnlockLevel() {
        if (!HasAlreadyDiscoverLevel()) {
            bool shouldCongrats = menuLevelSelector != null ? menuLevelSelector.GetLevelIndice() != 0 : selectorManager.GetLevelIndice() != 0;
            if (shouldCongrats) {
                MenuManager.Instance.RunPopup(
                    "Niveau débloqué !",
                    "Félicitation ! Vous venez de débloquer le niveau " + GetName() + " !\nContinuez comme ça !\nEt Happy Hacking ! :)",
                    TexteExplicatif.Theme.POSITIF);
            } else {
                texteExplicatifIntroduction.Run();
            }
            SetAlreadyDiscoverLevel();
        }
    }

    public bool HasAlreadyDiscoverLevel() {
        string key = GetName() + HAS_ALREADY_DISCOVER_LEVEL_KEY;
        return PlayerPrefs.HasKey(key) && PlayerPrefs.GetString(key) == "True";
    }

    public void SetAlreadyDiscoverLevel() {
        string key = GetName() + HAS_ALREADY_DISCOVER_LEVEL_KEY;
        PlayerPrefs.SetString(key, "True");
    }

    public bool IsPlayStarted() {
        return playStarted;
    }
    public void SetPlayStarted() {
        playStarted = true;
    }

    public bool IsSucceeded() {
        return GetNbWins() > 0;
    }
}
