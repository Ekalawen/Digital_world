using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

public class MenuLevel : MonoBehaviour {

    public enum LevelType { REGULAR, INFINITE };

    //public static string LEVEL_NAME_ID_KEY = "levelNameIdKey";
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
    public static string IS_LEVEL_HIGHLIGHTED_KEY = "isLevelHighlighted";
    public static string SUPER_CHEATED_PASSWORD = "supercheatedpassword"; // "lecreateurdecejeuestmonuniquedieuetmaitre";


    [Header("Name")]
    public string levelSceneName;
    public LocalizedString visibleName;
    public LevelType levelType;

    [Header("Background")]
    public float probaSource = 0.00035f;
    public int distanceSource = 8;
    public float decroissanceSource = 0.01f;
    public List<ColorManager.Theme> themes;

    [Header("Links to Level")]
    public string nextPassword = "passwd";
    public GameObject joueurPrefab;
    public GameObject consolePrefab;

    [Header("Archives")]
    public TexteExplicatif texteArchivesLink;
    public LocalizedTextAsset archivesTextAsset;
    public bool displayArchivesOnUnlock = false;

    [Header("Scores")]
    public GameObject scoresRegular;
    public GameObject scoresInfinite;
    public TMP_Text score_nbTries;
    public TMP_Text score_nbWins;
    public TMP_Text score_winrate;
    public TMP_Text score_highestScore;
    public TMP_Text score_nbGames;
    public TMP_Text score_meanScore;
    public TMP_Text score_sinceLastBestScore;
    public TMP_Text score_bestScore;
    public TMP_Text score_dataCount;

    [Header("FastUI")]
    public GameObject fastUISystemNextPrefab;
    public GameObject fastUISystemPreviousPrefab;
    public RectTransform fastUISystemNextTransform;
    public RectTransform fastUISystemPreviousTransform;

    [Header("Other Links")]
    public SelectorManager selectorManager;
    public MenuBackgroundBouncing menuBouncingBackground;
    public TMP_Text textLevelName;
    public InputField inputFieldNext;
    public Button backButton;
    public Button playButton;

    [HideInInspector]
    public MenuLevelSelector menuLevelSelector;

    protected bool playStarted = false;
    protected bool texteArchivesLinkIsReady = false;

    public void Initialize() {
        menuBouncingBackground.SetParameters(probaSource, distanceSource, decroissanceSource, themes);
        SetScores();

        MenuManager.DISABLE_HOTKEYS = false;
        InitTextesExplicatifs();

        /// This is now done in SelectorLevel !
        //DisplayPopupUnlockLevel();
        //DisplayPopupUnlockNewTreshold();

        string key = GetNameId() + CURRENT_INPUT_FIELD_KEY;
        inputFieldNext.text = PlayerPrefs.GetString(key);
        UIHelper.FitTextHorizontaly(textLevelName.text, textLevelName);

        MakeBouncePlayButton();
        GenerateNextAndPreviousButtons();
        SetLevelName();
    }

    protected void GenerateNextAndPreviousButtons() {
        if (fastUISystemNextTransform.childCount > 0 || fastUISystemPreviousTransform.childCount > 0) {
            foreach (Transform t in fastUISystemNextTransform) {
                Destroy(t.gameObject);
            }
            foreach (Transform t in fastUISystemPreviousTransform) {
                Destroy(t.gameObject);
            }
        }
        SelectorLevel selectorLevel = selectorManager.GetCurrentLevel();
        List<SelectorPath> nextPaths = selectorManager.GetOutPaths(selectorLevel);
        List<SelectorPath> previousPaths = selectorManager.GetInPaths(selectorLevel);
        nextPaths.Reverse();
        previousPaths.Reverse();
        for(int i = 0; i < nextPaths.Count; i++) {
            SelectorPath nextPath = nextPaths[i];
            float heightOffset = ComputeHeightOffset();
            Vector3 pos = fastUISystemNextTransform.transform.position + Vector3.up * heightOffset * (i - (nextPaths.Count - 1) / 2.0f);
            FastUISystem fastUISystem = Instantiate(fastUISystemNextPrefab, pos, Quaternion.identity, fastUISystemNextTransform).GetComponent<FastUISystem>();
            fastUISystem.Initialize(nextPath, FastUISystem.DirectionType.FORWARD, FastUISystem.FromType.LEVEL);
        }
        for (int i = 0; i < previousPaths.Count; i++) {
            SelectorPath previousPath = previousPaths[i];
            float heightOffset = ComputeHeightOffset();
            Vector3 pos = fastUISystemPreviousTransform.transform.position + Vector3.up * heightOffset * (i - (previousPaths.Count - 1) / 2.0f);
            FastUISystem fastUISystem = Instantiate(fastUISystemPreviousPrefab, pos, Quaternion.identity, fastUISystemPreviousTransform).GetComponent<FastUISystem>();
            fastUISystem.Initialize(previousPath, FastUISystem.DirectionType.BACKWARD, FastUISystem.FromType.LEVEL);
        }
    }

    protected float ComputeHeightOffset() {
        float scaleFactor = GetComponentInParent<Canvas>().GetComponent<RectTransform>().localScale.x;
        float heightOffset = (fastUISystemNextPrefab.GetComponent<RectTransform>().rect.height + 3) * scaleFactor;
        return heightOffset;
    }

    protected void Update() {
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
        //} else {
        //    texteExplicatifPasswdError.Run();
        }
    }

    public void NextIfEnter() {
        if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
            Next();
        else
            texteArchivesLink.EnableHotkeysNextFrame();
    }

    public void Previous() {
        menuLevelSelector?.Previous();
        selectorManager?.Previous();
    }

    public void Back() {
        menuLevelSelector?.Back();
        selectorManager?.BackToSelector();
    }

    public void OpenArchives()
    {
        if (selectorManager != null && !selectorManager.HasSelectorLevelOpen())
            return;
        StartCoroutine(COpenArchives());
    }

    protected IEnumerator COpenArchives() {
        while (!texteArchivesLinkIsReady)
            yield return null;
        texteArchivesLink.Run(GetNbWins());
    }

    //public void OpenDonneesHackes() {
    //    if (selectorManager != null && !selectorManager.HasSelectorLevelOpen())
    //        return;
    //    // Changer le texte des données hackés en fonction du nombre de fois où l'on a gagné ce niveau !
    //    string key = textLevelName.text + NB_WINS_KEY;
    //    int nbVictoires = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : 0;
    //    if (nbVictoires == 0) {
    //        texteExplicatifDonneesHackes.Run(GetNbWins());
    //    } else {
    //        texteExplicatifDonneesHackesSuccess.Run(GetNbWins());
    //        AddNextPallierMessageToAllFragments();
    //    }
    //}

    //protected void AddNextPallierMessageToAllFragments() {
    //    TresholdText tresholdText = texteExplicatifDonneesHackesSuccess.GetTresholdText();
    //    List<TresholdFragment> fragments = tresholdText.GetAllFragmentsOrdered();
    //    for (int i = 0; i < fragments.Count; i++) {
    //        if (i < fragments.Count - 1) {
    //            int nextTreshold = fragments[i + 1].treshold;
    //            fragments[i].ApplyReplacementEvaluator(
    //                new Tuple<string, MatchEvaluator>(@"$(?![\r\n])", // Match EOF
    //                (Match match) => "Prochain pallier à " + nextTreshold + " victoires.\n\n\n"));
    //        } else {
    //            fragments[i].ApplyReplacementEvaluator(
    //                new Tuple<string, MatchEvaluator>(@"$(?![\r\n])", // Match EOF
    //                (Match match) => "Dernier pallier.\n\n\n"));
    //        }
    //    }
    //    texteExplicatifDonneesHackesSuccess.mainText.text = texteExplicatifDonneesHackesSuccess.ComputeText(GetNbWins());
    //}

    public void SaveNextInputField() {
        if (inputFieldNext.text == GetPassword() || inputFieldNext.text == SUPER_CHEATED_PASSWORD) {
            string key = GetNameId() + CURRENT_INPUT_FIELD_KEY;
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
        score_dataCount.text = ChangeLastWord(score_dataCount.text, GetDataCountToString());
    }

    protected void SetInfiniteScores() {
        score_nbGames.text = ChangeLastWord(score_nbGames.text, GetNbTries().ToString());
        score_meanScore.text = ChangeLastWord(score_meanScore.text, ((int)GetMeanScore()).ToString());
        score_sinceLastBestScore.text = ChangeLastWord(score_sinceLastBestScore.text, GetSinceLastBestScore().ToString());
        score_bestScore.text = ChangeLastWord(score_bestScore.text, GetBestScoreToString());
    }

    public int GetNbWins() {
        string key = GetNameId() + NB_WINS_KEY;
        return PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : 0;
    }

    public void SetNbWins(int nbWins) {
        string key = GetNameId() + NB_WINS_KEY;
        PlayerPrefs.SetInt(key, nbWins);
    }

    public int GetNbDeaths() {
        string key = GetNameId() + NB_DEATHS_KEY;
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
        string key = GetNameId() + SUM_OF_ALL_TRIES_SCORES_KEY;
        return PlayerPrefs.HasKey(key) ? PlayerPrefs.GetFloat(key) : 0;
    }

    public bool HasBestScore() {
        string key = GetNameId() + BEST_SCORE_KEY;
        return PlayerPrefs.HasKey(key);
    }

    public float GetBestScore() {
        string key = GetNameId() + BEST_SCORE_KEY;
        return PlayerPrefs.HasKey(key) ? PlayerPrefs.GetFloat(key) : 0.0f;
    }

    public void SetBestScore(float bestScore) {
        string key = GetNameId() + BEST_SCORE_KEY;
        PlayerPrefs.SetFloat(key, bestScore);
    }

    public float GetPrecedentBestScore() {
        string key = GetNameId() + PRECEDENT_BEST_SCORE_KEY;
        return PlayerPrefs.HasKey(key) ? PlayerPrefs.GetFloat(key) : 0.0f;
    }

    public void SetPrecedentBestScore(float precedentScore) {
        string key = GetNameId() + PRECEDENT_BEST_SCORE_KEY;
        PlayerPrefs.SetFloat(key, precedentScore);
    }

    public string GetBestScoreToString() {
        if(levelType == LevelType.REGULAR)
            return (HasBestScore()) ? GetBestScore().ToString("N2") : "null";
        else
            return (HasBestScore()) ? ((int)GetBestScore()).ToString() : "null";
    }

    public int GetSinceLastBestScore() {
        string key = GetNameId() + SINCE_LAST_BEST_SCORE_KEY;
        return PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : 0;
    }

    public string GetDataCountToString() {
        string key = GetNameId() + Lumiere.DATA_COUNT_KEY;
        int dataCount = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : 0;
        SelectorLevel selectorLevel = selectorManager.GetLevelFromMenuLevel(this);
        List<SelectorPath> outPaths = selectorManager.GetOutPaths(selectorLevel);
        List<int> tresholds = outPaths.SelectMany(path => path.GetTresholds()).Distinct().OrderBy(n => n).ToList();
        tresholds.Add(int.MaxValue);
        int nextTreshold = tresholds.Find(t => t > dataCount);
        string tresholdSymbol = nextTreshold == int.MaxValue ? "∞" : nextTreshold.ToString();
        return $"{dataCount}/{tresholdSymbol}";
    }

    public static string ChangeLastWord(string str, string lastWordReplacement) {
        string[] splited = str.Split(' ');
        splited[splited.Length - 1] = lastWordReplacement;
        return string.Join(" ", splited);
    }

    public string GetTrace() {
        string key = GetNameId() + TRACE_KEY;
        if (!PlayerPrefs.HasKey(key))
            InitTrace();
        return PlayerPrefs.GetString(key);
    }

    protected void InitTrace() {
        string trace = Trace.GenerateTrace();
        print(trace);

        string key = GetNameId() + TRACE_KEY;
        PlayerPrefs.SetString(key, trace);
    }

    public string GetNameId() {
        return levelSceneName;
    }

    public string GetVisibleName() {
        return visibleName.GetLocalizedString().Result;
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

    protected void InitTextesExplicatifs()
    {
        StartCoroutine(CInitTextesExplicatifs());
    }

    protected IEnumerator CInitTextesExplicatifs() {
        AsyncOperationHandle<TextAsset> handle = archivesTextAsset.LoadAssetAsync();
        yield return handle;
        texteArchivesLink.textAsset = handle.Result;
        texteArchivesLink.AddReplacement("CRINM", UIHelper.SurroundWithColor("CRINM", UIHelper.BLUE));
        texteArchivesLink.AddReplacement("H@ckers", UIHelper.SurroundWithColor("H@ckers", UIHelper.BLUE));
        texteArchivesLink.AddReplacement("[Cilliannelle Crittefigiée]", UIHelper.SurroundWithColor("[Cilliannelle Crittefigiée]", UIHelper.PURE_GREEN));
        texteArchivesLink.AddReplacement("[Morgensoul*]", UIHelper.SurroundWithColor("[Morgensoul*]", UIHelper.CYAN));
        texteArchivesLink.AddReplacement("[V1P3R]", UIHelper.SurroundWithColor("[V1P3R]", UIHelper.CYAN));
        texteArchivesLinkIsReady = true;
    }

    public bool HasJustWin() {
        string key = GetNameId() + HAS_JUST_WIN_KEY;
        return PlayerPrefs.HasKey(key) && PlayerPrefs.GetString(key) == MenuManager.TRUE;
    }

    public void SetNotJustWin() {
        string key = GetNameId() + HAS_JUST_WIN_KEY;
        PlayerPrefs.DeleteKey(key);
    }

    public bool HasJustMakeNewBestScore() {
        string key = GetNameId() + HAS_JUST_MAKE_BEST_SCORE_KEY;
        return PlayerPrefs.HasKey(key) && PlayerPrefs.GetString(key) == MenuManager.TRUE;
    }

    public void SetNotJustMakeNewBestScore() {
        string key = GetNameId() + HAS_JUST_MAKE_BEST_SCORE_KEY;
        PlayerPrefs.DeleteKey(key);
    }

    //protected void DisplayPopupUnlockNewTreshold() {
    //    if (HasJustWin()) {
    //        List<int> tresholds = texteExplicatifDonneesHackesSuccess.GetAllTresholds();
    //        if(tresholds.Contains(GetNbWins())) {
    //            MenuManager.Instance.RunPopup(
    //                "Pallier débloqué !", 
    //                "Félicitation ! Vous venez de débloquer le pallier de" + (GetNbWins() > 1 ? "s" : "") + " " + GetNbWins() + " victoire" + ((GetNbWins() > 1) ? "s" : "") + " !\n" +
    //                "Allez le consulter dans les Data Hackées !",
    //                TexteExplicatif.Theme.POSITIF);
    //        }
    //        SetNotJustWin();
    //    }
    //}
    
    //protected void DisplayPopupUnlockLevel() {
    //    if (!HasAlreadyDiscoverLevel()) {
    //        bool shouldCongrats = menuLevelSelector != null ? menuLevelSelector.GetLevelIndice() != 0 : selectorManager.GetLevelIndice() != 0;
    //        if (shouldCongrats) {
    //            MenuManager.Instance.RunPopup(
    //                "Niveau débloqué !",
    //                "Félicitation ! Vous venez de débloquer le niveau " + GetName() + " !\nContinuez comme ça !\nEt Happy Hacking ! :)",
    //                TexteExplicatif.Theme.POSITIF);
    //        } else {
    //            texteExplicatifIntroduction.Run();
    //        }
    //        SetAlreadyDiscoverLevel();
    //    }
    //}

    public bool HasAlreadyDiscoverLevel() {
        string key = GetNameId() + HAS_ALREADY_DISCOVER_LEVEL_KEY;
        return PlayerPrefs.HasKey(key) && PlayerPrefs.GetString(key) == "True";
    }

    public void SetAlreadyDiscoverLevel() {
        string key = GetNameId() + HAS_ALREADY_DISCOVER_LEVEL_KEY;
        PlayerPrefs.SetString(key, "True");
    }

    public bool IsPlayStarted() {
        return playStarted;
    }
    public void SetPlayStarted() {
        playStarted = true;
    }

    public bool IsSucceeded() {
        if (levelType == LevelType.REGULAR) {
            return GetNbWins() > 0;
        } else {
            return GetBestScore() >= 10;
        }
    }

    public void HighlightBackButton(bool state) {
        string key = GetNameId() + IS_LEVEL_HIGHLIGHTED_KEY;
        PlayerPrefs.SetString(key, state ? MenuManager.TRUE : MenuManager.FALSE);
        backButton.GetComponent<ButtonHighlighter>().enabled = state;
    }

    public void HighlightBackButtonBasedOnSave() {
        string key = GetNameId() + IS_LEVEL_HIGHLIGHTED_KEY;
        bool state = PlayerPrefs.HasKey(key) && PlayerPrefs.GetString(key) == MenuManager.TRUE;
        backButton.GetComponent<ButtonHighlighter>().enabled = state;
    }

    protected void MakeBouncePlayButton() {
        SelectorLevel level = selectorManager.GetLevelFromMenuLevel(this);
        playButton.GetComponent<ButtonHighlighter>().enabled = level.IsHighlighted();
    }

    public void RunIntroduction() {
        SelectorLevel selectorLevel = selectorManager.GetLevelFromMenuLevel(this);
        if (selectorManager.HasThisSelectorLevelOpen(selectorLevel) && !selectorManager.PopupIsEnabled()) {
            SelectorLevelRunIntroduction introductionRunner = GetComponent<SelectorLevelRunIntroduction>();
            if(introductionRunner != null) {
                introductionRunner.RunIntroduction();
            }
        }
    }

    protected void SetLevelName() {
        textLevelName.text = GetVisibleName();
    }
}
