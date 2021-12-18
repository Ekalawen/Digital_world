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
    public TMP_FontAsset fontLevelNameUnlocked;
    public TMP_FontAsset fontLevelNameLocked;
    public Image levelNameLine;
    public Button backButton;
    public Button playButton;
    public Button docButton;

    [HideInInspector]
    public MenuLevelSelector menuLevelSelector;

    protected bool playStarted = false;
    protected bool texteArchivesLinkIsReady = false;

    public void Initialize() {
        SetScores();

        MenuManager.DISABLE_HOTKEYS = false;

        /// This is now done in SelectorLevel !
        //DisplayPopupUnlockLevel();
        //DisplayPopupUnlockNewTreshold();

        MakeBouncePlayButton();
        //GenerateNextAndPreviousButtons();
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
            Quaternion rotation = Quaternion.LookRotation(fastUISystemNextTransform.transform.forward);
            FastUISystem fastUISystem = Instantiate(fastUISystemNextPrefab, pos, rotation, fastUISystemNextTransform).GetComponent<FastUISystem>();
            fastUISystem.Initialize(nextPath, FastUISystem.DirectionType.FORWARD, FastUISystem.FromType.LEVEL);
        }
        fastUISystemNextTransform.gameObject.SetActive(nextPaths.Count > 0);
        for (int i = 0; i < previousPaths.Count; i++) {
            SelectorPath previousPath = previousPaths[i];
            float heightOffset = ComputeHeightOffset();
            Vector3 pos = fastUISystemPreviousTransform.transform.position + Vector3.up * heightOffset * (i - (previousPaths.Count - 1) / 2.0f);
            Quaternion rotation = Quaternion.LookRotation(fastUISystemPreviousTransform.transform.forward);
            FastUISystem fastUISystem = Instantiate(fastUISystemPreviousPrefab, pos, rotation, fastUISystemPreviousTransform).GetComponent<FastUISystem>();
            fastUISystem.Initialize(previousPath, FastUISystem.DirectionType.BACKWARD, FastUISystem.FromType.LEVEL);
        }
        fastUISystemPreviousTransform.gameObject.SetActive(previousPaths.Count > 0);
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
            || Input.GetKeyDown(KeyCode.KeypadEnter)) {
                Play();
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
        AsyncOperationHandle<TextAsset> textAssetHandle = archivesTextAsset.LoadAssetAsync();
        yield return textAssetHandle;
        TextAsset textAsset = textAssetHandle.Result;
        selectorManager.popup.Initialize(useTextAsset: true, textAsset: textAsset);
        foreach(Tuple<string, string> replacement in UIHelper.GetReplacementList(selectorManager.archivesReplacementStrings)) {
            selectorManager.popup.AddReplacement(replacement.Item1, replacement.Item2);
        }
        selectorManager.popup.titleTextTarget.text = selectorManager.strings.archivesTitle.GetLocalizedString().Result;
        selectorManager.popup.Run();
    }

    protected void SetScores() {
        if (GetLevelType() == LevelType.REGULAR) {
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
        string key = GetNameId() + PrefsManager.NB_WINS_KEY;
        return PrefsManager.GetInt(key, 0);
    }

    public void SetNbWins(int nbWins) {
        string key = GetNameId() + PrefsManager.NB_WINS_KEY;
        PrefsManager.SetInt(key, nbWins);
    }

    public int GetNbDeaths() {
        string key = GetNameId() + PrefsManager.NB_DEATHS_KEY;
        return PrefsManager.GetInt(key, 0);
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
        string key = GetNameId() + PrefsManager.SUM_OF_ALL_TRIES_SCORES_KEY;
        return PrefsManager.GetFloat(key, 0);
    }

    public bool HasBestScore() {
        string key = GetNameId() + PrefsManager.BEST_SCORE_KEY;
        return PrefsManager.HasKey(key);
    }

    public float GetBestScore() {
        string key = GetNameId() + PrefsManager.BEST_SCORE_KEY;
        return PrefsManager.GetFloat(key, 0);
    }

    public void SetBestScore(float bestScore) {
        string key = GetNameId() + PrefsManager.BEST_SCORE_KEY;
        PrefsManager.SetFloat(key, bestScore);
    }

    public float GetPrecedentBestScore() {
        string key = GetNameId() + PrefsManager.PRECEDENT_BEST_SCORE_KEY;
        return PrefsManager.GetFloat(key, 0);
    }

    public void SetPrecedentBestScore(float precedentScore) {
        string key = GetNameId() + PrefsManager.PRECEDENT_BEST_SCORE_KEY;
        PrefsManager.SetFloat(key, precedentScore);
    }

    public string GetBestScoreToString() {
        if(GetLevelType() == LevelType.REGULAR)
            return (HasBestScore()) ? GetBestScore().ToString("N2") : "null";
        else
            return (HasBestScore()) ? ((int)GetBestScore()).ToString() : "null";
    }

    public int GetSinceLastBestScore() {
        string key = GetNameId() + PrefsManager.SINCE_LAST_BEST_SCORE_KEY;
        return PrefsManager.GetInt(key, 0);
    }

    public int GetDataCount() {
        string key = GetNameId() + PrefsManager.DATA_COUNT_KEY;
        return PrefsManager.GetInt(key, 0);
    }

    public void SetDataCount(int value) {
        string key = GetNameId() + PrefsManager.DATA_COUNT_KEY;
        PrefsManager.SetInt(key, value);
    }

    public int GetPrecedentDataCount() {
        string key = GetNameId() + PrefsManager.PRECEDENT_DATA_COUNT_KEY;
        return PrefsManager.GetInt(key, 0);
    }

    public bool HasJustIncreaseDataCount() {
        string key = GetNameId() + PrefsManager.HAS_JUST_INCREASED_DATA_COUNT_KEY;
        return PrefsManager.GetBool(key, false);
    }

    public void SetNotJustIncreaseDataCount() {
        string keyHasJustIncreased = GetNameId() + PrefsManager.HAS_JUST_INCREASED_DATA_COUNT_KEY;
        PrefsManager.SetString(keyHasJustIncreased, PrefsManager.FALSE);
        string keyPrecedentDataCount = GetNameId() + PrefsManager.PRECEDENT_DATA_COUNT_KEY;
        PrefsManager.SetInt(keyPrecedentDataCount, GetDataCount());
    }


    public string GetDataCountToString() {
        int dataCount = GetDataCount();
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
        string key = GetNameId() + PrefsManager.TRACE_KEY;
        if (!PrefsManager.HasKey(key)) {
            InitTrace();
        }
        return PrefsManager.GetString(key, "0000");
    }

    protected void InitTrace() {
        string trace = Trace.GenerateTrace();
        print(trace);

        string key = GetNameId() + PrefsManager.TRACE_KEY;
        PrefsManager.SetString(key, trace);
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

    public bool HasJustWin() {
        string key = GetNameId() + PrefsManager.HAS_JUST_WIN_KEY;
        return PrefsManager.GetBool(key, false);
    }

    public void SetNotJustWin() {
        string key = GetNameId() + PrefsManager.HAS_JUST_WIN_KEY;
        PrefsManager.DeleteKey(key);
    }

    public bool HasJustMakeNewBestScore() {
        string key = GetNameId() + PrefsManager.HAS_JUST_MAKE_BEST_SCORE_KEY;
        return PrefsManager.GetBool(key, false);
    }

    public void SetNotJustMakeNewBestScore() {
        string key = GetNameId() + PrefsManager.HAS_JUST_MAKE_BEST_SCORE_KEY;
        PrefsManager.SetBool(key, false);
        SetPrecedentBestScore(GetBestScore());
    }

    public bool HasAlreadyDiscoverLevel() {
        string key = GetNameId() + PrefsManager.HAS_ALREADY_DISCOVER_LEVEL_KEY;
        return PrefsManager.GetBool(key, false);
    }

    public void SetAlreadyDiscoverLevel() {
        string key = GetNameId() + PrefsManager.HAS_ALREADY_DISCOVER_LEVEL_KEY;
        PrefsManager.SetBool(key, true);
    }

    public bool IsPlayStarted() {
        return playStarted;
    }
    public void SetPlayStarted() {
        playStarted = true;
    }

    public bool IsSucceeded() {
        if (GetLevelType() == LevelType.REGULAR) {
            return GetNbWins() > 0;
        } else {
            return GetBestScore() > 0;
        }
    }

    public bool IsFinished() {
        List<SelectorLevel> nextLevels = selectorManager.GetOutPaths(GetSelectorLevel()).Select(p => p.endLevel).ToList();
        return nextLevels.All(nl => nl.IsAccessible());
    }

    public SelectorLevel GetSelectorLevel() {
        return selectorManager.GetLevelFromMenuLevel(this);
    }

    public void HighlightBackButton(bool state) {
        string key = GetNameId() + PrefsManager.IS_LEVEL_HIGHLIGHTED_KEY;
        PrefsManager.SetBool(key, state);
        backButton.GetComponent<ButtonHighlighter>().enabled = state;
    }

    public void HighlightBackButtonBasedOnSave() {
        string key = GetNameId() + PrefsManager.IS_LEVEL_HIGHLIGHTED_KEY;
        bool state = PrefsManager.GetBool(key, false);
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
        if(IsFinished()) {
            textLevelName.font = fontLevelNameUnlocked;
            levelNameLine.color = ColorManager.GetMainGreen();
        } else {
            textLevelName.font = fontLevelNameLocked;
            levelNameLine.color = ColorManager.GetMainRed();
        }
    }

    public LevelType GetLevelType() {
        return levelType;
    }

    public void OpenDoc() {
        if (selectorManager != null && !selectorManager.HasSelectorLevelOpen())
            return;
        StartCoroutine(COpenDoc());
    }

    protected IEnumerator COpenDoc() {
        AsyncOperationHandle<string> handleTitle = selectorManager.strings.docTitle.GetLocalizedString();
        yield return handleTitle;
        string titleString = handleTitle.Result;
        string docText = "";
        int i = 1;
        foreach(LocalizedString localizedString in consolePrefab.GetComponent<Console>().conseils) {
            AsyncOperationHandle<string> conseilHandle = localizedString.GetLocalizedString();
            yield return conseilHandle;
            docText += $"{UIHelper.SurroundWithColor(i.ToString(), UIHelper.GREEN)}) {conseilHandle.Result}\n";
            i++;
        }
        docText = UIHelper.ApplyReplacementList(docText, selectorManager.docReplacementStrings);
        selectorManager.RunPopup(titleString, docText, TexteExplicatif.Theme.NEUTRAL);
    }

    public string GetKey(string keySuffix) {
        return GetNameId() + keySuffix;
    }
}
