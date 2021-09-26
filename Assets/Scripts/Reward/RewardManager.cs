using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public float revealTimeAt10Blocks = 2.5f;
    public float revealTimeAt100Blocks = 10.0f;
    public AnimationCurve revealCurve;
    public float blocksDistance = 30;
    public GameObject bloomProfile;
    public Vector4 gridRect;
    public float blockRotationSpeed = 3.0f;
    public AnimationCurve colorCurve;
    public AnimationCurve mainColorCurve;
    public float bounceTime = 1.5f;
    public AnimationCurve bounceCurve;

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
        StartCoroutine(CInitializeInfiniteLevelMode());
    }

    protected IEnumerator CInitializeInfiniteLevelMode() {
        bloomProfile.SetActive(false);
        int nbBlocks = hm.GetBlocksPassedPrefabs().Count;
        Vector2 gridShape = ComputeGridShape(nbBlocks);
        List<Vector2> gridPositions = ComputeGridPositions(nbBlocks, gridShape);
        Vector3 tailleMaxBlock = ComputeTailleMaxBlock(gridShape, gridPositions);
        Color pureColor = ColorManager.GetColor(ColorManager.RemoveSpecificColorFromThemes(hm.themes, ColorManager.Theme.BLANC));
        float revealTime = MathCurves.Linear(revealTimeAt10Blocks, revealTimeAt100Blocks, MathCurves.LinearReversedUnclamped(10, 100, nbBlocks));
        Timer timer = new Timer(revealTime);
        Debug.Log($"RevealTime = {revealTime}");
        int indiceToReveal = 0;
        while(indiceToReveal < nbBlocks) {
            float avancement = timer.GetAvancement();
            if (revealCurve.Evaluate(avancement) >= (float)indiceToReveal / Mathf.Max(1, (nbBlocks - 1))) {
                RevealBlock(nbBlocks, gridPositions, tailleMaxBlock, pureColor, indiceToReveal);
                indiceToReveal++;
            }
            yield return null;
        }
    }

    protected void RevealBlock(int nbBlocks, List<Vector2> gridPositions, Vector3 tailleMaxBlock, Color pureColor, int indice) {
        GameObject blockPrefab = hm.GetBlocksPassedPrefabs()[indice];
        float blockRedimensionnement = GetBlockRedimensionnement(tailleMaxBlock, blockPrefab);
        Vector3 worldPosition = GetBlockWorldPosition(gridPositions[indice], blockPrefab, blockRedimensionnement);
        Color color = ColorManager.InterpolateColors(Color.white, pureColor, mainColorCurve.Evaluate((float)indice / Mathf.Max(1, (nbBlocks - 1))));
        InstantiateBlockPrefab(blockPrefab, worldPosition, blockRedimensionnement, color);
    }

    protected Vector3 ComputeTailleMaxBlock(Vector2 gridShape, List<Vector2> gridPositions) {
        float tailleMaxLargeur;
        float tailleMaxHauteur;
        if(gridShape.x == 1) {
            Vector2 gridPointLeft = new Vector2(gridRect.x * camera.cam.pixelWidth, 0);
            Vector2 gridPointRight = new Vector2(gridRect.x * camera.cam.pixelWidth + (camera.cam.pixelWidth * (1 - gridRect.x - gridRect.z)), 0);
            tailleMaxLargeur = DistanceBetweenGridPoints(gridPointLeft, gridPointRight);
        } else {
            tailleMaxLargeur = DistanceBetweenGridPoints(gridPositions[0], gridPositions[1]);
        }
        if(gridShape.y == 1) {
            Vector2 gridPointHaut = new Vector2(gridRect.y * camera.cam.pixelHeight, 0);
            Vector2 gridPointBas = new Vector2(gridRect.y * camera.cam.pixelHeight + (camera.cam.pixelHeight * (1 - gridRect.y - gridRect.w)), 0);
            tailleMaxHauteur = DistanceBetweenGridPoints(gridPointHaut, gridPointBas);
        } else {
            tailleMaxHauteur = DistanceBetweenGridPoints(gridPositions[0], gridPositions[(int)gridShape.x]);
        }
        return new Vector3(tailleMaxLargeur, tailleMaxHauteur, tailleMaxLargeur);
    }

    protected float DistanceBetweenGridPoints(Vector2 gridPoint1, Vector2 gridPoint2) {
        Vector3 p1WorldSpace = camera.cam.ScreenToWorldPoint(new Vector3(gridPoint1.x, gridPoint1.y, blocksDistance));
        Vector3 p2WorldSpace = camera.cam.ScreenToWorldPoint(new Vector3(gridPoint2.x, gridPoint2.y, blocksDistance));
        return Vector3.Distance(p1WorldSpace, p2WorldSpace);
    }

    private Vector3 GetBlockWorldPosition(Vector2 screenPosition, GameObject blockPrefab, float redimensionnement) {
        Vector3 worldPosition = camera.cam.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, blocksDistance));
        BlockTriggerZone triggerZone = blockPrefab.GetComponent<Block>().triggerZone;
        worldPosition = worldPosition - triggerZone.transform.localPosition / redimensionnement; // La position de la triggerZone est en son centre ! Theuh ! XD
        return worldPosition;
    }

    protected void InstantiateBlockPrefab(GameObject blockPrefab, Vector3 worldPosition, float redimensionnement, Color targetColor) {
        Block block = Instantiate(blockPrefab, worldPosition, Quaternion.identity, parent: blocksFolder).GetComponent<Block>();
        block.transform.localScale /= redimensionnement;
        List<Cube> cubes = block.GetCubesNonInitialized();
        List<float> distances = cubes.Select(c => Vector3.Distance(c.transform.position, block.triggerZone.transform.position)).ToList();
        Vector2 distancesInterval = new Vector2(distances.Min(), distances.Max());
        foreach (Cube cube in cubes) {
            float distance = Vector3.Distance(cube.transform.position, block.triggerZone.transform.position);
            float avancement = colorCurve.Evaluate(MathCurves.Remap(distance, distancesInterval, new Vector2(0, 1)));
            cube.GetComponent<MeshRenderer>().material.SetColor("_Color", ColorManager.InterpolateColors(targetColor, Color.white, avancement));
        }
        AutoRotate rotator = block.gameObject.AddComponent<AutoRotate>();
        rotator.vitesse = blockRotationSpeed;
        rotator.usePivot = true;
        rotator.pivot = block.triggerZone.transform.localPosition / redimensionnement;

        StartCoroutine(MakeBlockBounce(block));
    }

    protected IEnumerator MakeBlockBounce(Block block) {
        Timer timer = new Timer(bounceTime);
        Vector3 initialSize = block.transform.localScale;
        while(!timer.IsOver()) {
            block.transform.localScale = initialSize * bounceCurve.Evaluate(timer.GetAvancement());
            yield return null;
        }
        block.transform.localScale = initialSize;
    }

    protected float GetBlockRedimensionnement(Vector3 tailleMaxBlock, GameObject block) {
        Vector3 sizeBlock = block.GetComponent<Block>().triggerZone.transform.localScale;
        float redimensionnement = Mathf.Max(sizeBlock.x / tailleMaxBlock.x, sizeBlock.y / tailleMaxBlock.y, sizeBlock.z / tailleMaxBlock.z);
        if (redimensionnement > 1) {
            return redimensionnement;
        }
        return 1;
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
