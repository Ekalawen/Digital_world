using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RewardManager : MonoBehaviour {

    static RewardManager _instance;
    public static RewardManager Instance { get { return _instance ?? (_instance = new GameObject().AddComponent<RewardManager>()); } }

    [Header("Parameters Regular")]
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

    [Header("Parameters Infinite")]
    public Color blocksColor;
    public float blocksDistance = 30;
    public GameObject bloomProfile;
    public Vector4 gridRect;
    public float blockRotationSpeed = 3.0f;

    [Header("Links")]
    public RewardStrings strings;
    public RegularRewardCamera regularCamera;
    public InfiniteStaticRewardCamera infiniteCamera;
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
    protected Transform blocksFolder;
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
        blocksFolder = new GameObject("Blocks").transform;
        hm = HistoryManager.Instance;
        ComputeDureeGameAndReward();

        InitializeCamera();

        if (hm.GetMapType() == MenuLevel.LevelType.REGULAR) {
            InitializeRegularLevelMode();
        } else { // INFINITE
            InitializeInfiniteLevelMode();
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
    }

    protected void InitializeRegularLevelMode() {
        ObjectHistory playerHistory = hm.GetPlayerHistory();
        List<ObjectHistory> ennemisHistory = hm.GetEnnemisHistory();
        List<ObjectHistory> lumieresHistory = hm.GetLumieresHistory();
        List<ObjectHistory> itemsHistory = hm.GetItemsHistory();

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
    }

    protected void InitializeInfiniteLevelMode() {
        bloomProfile.SetActive(false);
        int nbBlocks = hm.GetBlocksPassedPrefabs().Count;
        Vector2 gridShape = ComputeGridShape(nbBlocks);
        List<Vector2> gridPositions = ComputeGridPositions(nbBlocks, gridShape);
        for(int i = 0; i < nbBlocks; i++) {
            GameObject blockPrefab = hm.GetBlocksPassedPrefabs()[i];
            BlockTriggerZone triggerZone = blockPrefab.GetComponentInChildren<BlockTriggerZone>();
            Vector2 screenPosition = gridPositions[i];
            Vector3 worldPosition = camera.cam.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, blocksDistance));
            worldPosition = worldPosition - (triggerZone.transform.localPosition - triggerZone.transform.localScale / 2);
            Block block = Instantiate(blockPrefab, worldPosition, Quaternion.identity, parent: blocksFolder).GetComponent<Block>();
            foreach(Cube cube in block.GetCubesNonInitialized()) {
                cube.GetComponent<MeshRenderer>().material.SetColor("_Color", blocksColor);
            }
            AutoRotate rotator = block.gameObject.AddComponent<AutoRotate>();
            rotator.vitesse = blockRotationSpeed;
        }
    }

    protected Vector2 ComputeGridShape(int nbBlocks) {
        Vector2 columnsRows = Vector2.one;
        for(int i = 0; i <= nbBlocks; i++) {
            if(columnsRows.x * columnsRows.y < nbBlocks) {
                if(columnsRows.x == columnsRows.y) {
                    columnsRows.x += 1;
                } else {
                    columnsRows.y += 1;
                }
            } else {
                break;
            }
        }
        return columnsRows;
    }

    protected List<Vector2> ComputeGridPositions(int nbBlocks, Vector2 gridShape) {
        List<Vector2> gridPositions = new List<Vector2>();
        int nbNotOnLastLine = (int)(gridShape.x * (gridShape.y - 1));
        int nbOnLastLine = nbBlocks - nbNotOnLastLine;
        for(int i = 0; i < nbBlocks; i++) {
            Vector2 position;
            if(i < nbNotOnLastLine) {
                position = new Vector2((i % gridShape.x + 1) / (gridShape.x + 1), 1 - (i / (int)gridShape.x + 1) / (gridShape.y + 1));
            } else {
                position = new Vector2((i - nbNotOnLastLine + 1) / (float)(nbOnLastLine + 1), 1 - (i / (int)gridShape.x + 1) / (gridShape.y + 1));
            }
            position.x = gridRect.x * camera.cam.pixelWidth + position.x * (camera.cam.pixelWidth * (1 - gridRect.x - gridRect.z));
            position.y = gridRect.y * camera.cam.pixelHeight + position.y * (camera.cam.pixelHeight * (1 - gridRect.y - gridRect.w));
            gridPositions.Add(position);
        }
        return gridPositions;
    }


    protected void ComputeDureeGameAndReward() {
        float dureeGame = hm.GetDureeGame();
        dureeReward = ComputeDurationTrail(dureeGame);
        accelerationCoefficiant = dureeReward / dureeGame;
        Debug.Log("DureeGame = " + dureeGame + " DureeReward = " + dureeReward + " Acceleration = " + accelerationCoefficiant);
    }

    protected void InitializeTitleAndStats() {
        titleCompletedText.text = strings.levelCompleted.GetLocalizedString(hm.levelNameVisual).Result;
        UIHelper.FitTextHorizontaly(titleCompletedText.text, titleCompletedText);

        string score = hm.score.ToString("0.00");
        string bestScore = PrefsManager.GetFloat(GetKeyFor(PrefsManager.BEST_SCORE_KEY), 0).ToString("0.00");
        scoreText.text = strings.score.GetLocalizedString(score).Result;
        bestScoreText.text = strings.bestScore.GetLocalizedString(bestScore).Result;

        if (hm.GetMapType() == MenuLevel.LevelType.REGULAR) {
            SetAllReplayStatsTexts(currentReplayLength: 0.0f);
        }
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
        if (hm.GetMapType() == MenuLevel.LevelType.REGULAR) {
            SetAllReplayStatsTexts(currentReplayLength: GetCurrentReplayTime());
        }
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
