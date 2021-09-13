using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RewardManager : MonoBehaviour {

    static RewardManager _instance;
    public static RewardManager Instance { get { return _instance ?? (_instance = new GameObject().AddComponent<RewardManager>()); } }

    [Header("Parameters")]
    public float delayBetweenTrails = 10.0f;
    public float durationTrailMinimum = 10.0f;
    public float durationLogarithmeRegular = 1.3f;
    public float durationDividerInfinite = 3.0f;
    public float pourcentageEnnemiTrailTime = 0.2f;
    public float pointDisplayerScaleFactor = 0.2f;

    [Header("Prefabs")]
    public GameObject playerTrailPrefab;
    public GameObject ennemiTrailPrefab;
    public GameObject consolePrefab; // On récupère la console !

    [Header("Links")]
    public RewardStrings strings;
    public RegularRewardCamera regularCamera;
    public InfiniteRewardCamera infiniteCamera;
    public TMP_Text titleCompletedText;
    public TMP_Text scoreText;
    public TMP_Text bestScoreText;
    public TMP_Text gameDurationText;
    public TMP_Text accelerationText;
    public TMP_Text replayDurationText;
    public RewardNewBestScoreButton newBestScoreButton;
    public RewardNewBestScoreButton firstTimeButton;

    protected float dureeReward;
    protected float accelerationCoefficiant;

    protected Transform displayersFolder;
    protected RewardTrailThemeDisplayer playerDisplayer;
    protected List<RewardTrailDisplayer> ennemisDisplayers;
    protected List<RewardPointDisplayer> lumieresDisplayers;
    protected List<RewardPointDisplayer> itemsDisplayers;

    protected HistoryManager hm;
	protected RewardConsole console;
    protected RewardCamera camera;

    void Awake() {
        if (!_instance) { _instance = this; }
    }

    public void Start() {
        displayersFolder = new GameObject("Displayers").transform;

        hm = HistoryManager.Instance;
        Debug.Log($"HistoryManager = {hm}");
        ObjectHistory playerHistory = hm.GetPlayerHistory();
        Debug.Log($"PlayerHistory = {playerHistory}");
        List<ObjectHistory> ennemisHistory = hm.GetEnnemisHistory();
        Debug.Log($"ennemisHistory.Count = {ennemisHistory.Count}");
        List<ObjectHistory> lumieresHistory = hm.GetLumieresHistory();
        Debug.Log($"lumieresHistory.Count = {lumieresHistory.Count}");
        List<ObjectHistory> itemsHistory = hm.GetItemsHistory();
        Debug.Log($"itemsHistory.Count = {itemsHistory.Count}");

        float dureeGame = hm.GetDureeGame();
        dureeReward = ComputeDurationTrail(dureeGame);
        accelerationCoefficiant = dureeReward / dureeGame;
        Debug.Log("DureeGame = " + dureeGame + " DureeReward = " + dureeReward + " Acceleration = " + accelerationCoefficiant);

        float playerTrailDurationTime = dureeReward;
        float ennemiTrailDurationTime = playerTrailDurationTime * pourcentageEnnemiTrailTime;

        playerDisplayer = gameObject.AddComponent<RewardTrailThemeDisplayer>();
        playerDisplayer.Initialize(playerTrailPrefab, playerHistory, dureeReward, delayBetweenTrails, accelerationCoefficiant, hm.themes);

        ennemisDisplayers = new List<RewardTrailDisplayer>();
        foreach (ObjectHistory history in ennemisHistory)
        {
            RewardTrailDisplayer displayer = gameObject.AddComponent<RewardTrailDisplayer>();
            displayer.Initialize(ennemiTrailPrefab, history, dureeReward, delayBetweenTrails, accelerationCoefficiant, ennemiTrailDurationTime, Color.red);
            ennemisDisplayers.Add(displayer);
        }

        lumieresDisplayers = new List<RewardPointDisplayer>();
        foreach (ObjectHistory history in lumieresHistory)
        {
            RewardPointDisplayer displayer = gameObject.AddComponent<RewardPointDisplayer>();
            displayer.Initialize(history.prefab, history, dureeReward, delayBetweenTrails, accelerationCoefficiant, history.prefab.gameObject.transform.localScale.x);
            lumieresDisplayers.Add(displayer);
        }

        itemsDisplayers = new List<RewardPointDisplayer>();
        foreach (ObjectHistory history in itemsHistory)
        {
            RewardPointDisplayer displayer = gameObject.AddComponent<RewardPointDisplayer>();
            displayer.Initialize(history.prefab, history, dureeReward, delayBetweenTrails, accelerationCoefficiant, pointDisplayerScaleFactor);
            itemsDisplayers.Add(displayer);
        }

        //On lance la console ! :)
        console = Instantiate(consolePrefab).GetComponent<RewardConsole>();
        console.timedMessages = new List<TimedMessage>();
        foreach (TimedMessage tm in hm.GetTimedMessages())
        {
            tm.timing *= accelerationCoefficiant;
            console.timedMessages.Add(tm);
        }
        console.Initialize();
        console.SetDureeReward(dureeReward, delayBetweenTrails);

        InitializeTitleAndStats();

        InitializeNewBestScoreAndFirstTimeButtons();

        InitializeCamera();
    }

    protected void InitializeTitleAndStats() {
        titleCompletedText.text = strings.levelCompleted.GetLocalizedString(hm.levelNameVisual).Result;
        UIHelper.FitTextHorizontaly(titleCompletedText.text, titleCompletedText);

        string score = hm.score.ToString("0.00");
        string bestScore = PrefsManager.GetFloat(GetKeyFor(PrefsManager.BEST_SCORE_KEY), 0).ToString("0.00");
        scoreText.text = strings.score.GetLocalizedString(score).Result;
        bestScoreText.text = strings.bestScore.GetLocalizedString(bestScore).Result;

        SetAllReplayStatsTexts(currentReplayLength: 0.0f);
    }

    protected void SetAllReplayStatsTexts(float currentReplayLength) {
        string gameLength = hm.GetDureeGame().ToString("0.00");
        string replayLength = dureeReward.ToString("0.00");
        string currentReplayLengthText = currentReplayLength.ToString("0.00");
        string acceleration = ((1.0f / accelerationCoefficiant - 1) * 100).ToString("0.00");
        gameDurationText.text = strings.gameDuration.GetLocalizedString(gameLength).Result;
        accelerationText.text = strings.acceleration.GetLocalizedString(acceleration).Result;
        replayDurationText.text = strings.replayDuration.GetLocalizedString($"{currentReplayLengthText}/{replayLength}").Result;
    }

    protected void InitializeCamera() {
        if(hm.GetMapType() == MenuLevel.LevelType.REGULAR) {
            camera = regularCamera;
            regularCamera.enabled = true;
            infiniteCamera.enabled = false;
        } else {
            camera = infiniteCamera;
            regularCamera.enabled = false;
            infiniteCamera.enabled = true;
        }
        camera.Initialize();
    }

    public void Update() {
        TestExit();
        SetAllReplayStatsTexts(currentReplayLength: GetCurrentReplayTime());
    }

    protected float GetCurrentReplayTime() {
        Timer replayTimer = playerDisplayer.GetDurationTimer();
        return Mathf.Min(replayTimer.GetElapsedTime(), dureeReward);
    }

    public void TestExit() {
        if (Input.GetKey (KeyCode.Escape) 
         || Input.GetKey(KeyCode.KeypadEnter)
         || Input.GetKey(KeyCode.Return)
         || Input.GetKey(KeyCode.Space)) {
            Exit();
		}
    }

    public void Exit() {
        if (hm != null) {
            Destroy(hm.gameObject); // Need to destoy it because it is in DontDestroyOnLoad :)
        }
        SceneManager.LoadScene("SelectorScene");
    }

    protected float ComputeDurationTrail(float dureeGame) {
        if (dureeGame <= durationTrailMinimum) {
            return dureeGame;
        } else {
            if (hm.GetMapType() == MenuLevel.LevelType.REGULAR) {
                return Mathf.Max(durationTrailMinimum, Mathf.Log(dureeGame, durationLogarithmeRegular));
            } else {
                return Mathf.Max(durationTrailMinimum, dureeGame / durationDividerInfinite);
            }
        }
    }

    public Transform GetDisplayersFolder() {
        return displayersFolder;
    }

    public RewardTrailThemeDisplayer GetPlayerDisplayer() {
        return playerDisplayer;
    }

    protected string GetKeyFor(string keySuffix) {
        string levelNameKey = hm.levelNameId;
        return levelNameKey + keySuffix;
    }

    public bool IsNewBestScoreAfterBestScoreAssignation() {
        string key = GetKeyFor(PrefsManager.HAS_JUST_MAKE_BEST_SCORE_KEY);
        return PrefsManager.GetBool(key, false);
    }
    public float GetPrecedentBestScore() {
        return PrefsManager.GetFloat(GetKeyFor(PrefsManager.PRECEDENT_BEST_SCORE_KEY), 0);
    }
    public bool IsFirstTimeFinishingLevel() {
        bool hasJustWin = PrefsManager.GetBool(GetKeyFor(PrefsManager.HAS_JUST_WIN_KEY), false);
        int nbWins = PrefsManager.GetInt(GetKeyFor(PrefsManager.NB_WINS_KEY), 0);
        return hasJustWin && nbWins == 1;
    }

    protected void InitializeNewBestScoreAndFirstTimeButtons() {
        if(IsNewBestScoreAfterBestScoreAssignation() && GetPrecedentBestScore() > 0) {
            newBestScoreButton.gameObject.SetActive(true);
            newBestScoreButton.Initialize();
        } else {
            newBestScoreButton.gameObject.SetActive(false);
        }
        if(IsFirstTimeFinishingLevel()) {
            firstTimeButton.gameObject.SetActive(true);
            firstTimeButton.Initialize();
        } else {
            firstTimeButton.gameObject.SetActive(false);
        }
    }
}
