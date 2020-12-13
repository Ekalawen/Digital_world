using EZCameraShake;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EventManager : MonoBehaviour {
    public enum DeathReason {
        TIME_OUT,
        FALL_OUT,
        CAPTURED,
        TOUCHED_DEATH_CUBE,
        TOUCHED_BOUNCY_CUBE,
        POUVOIR_COST,
        OUT_OF_BLOCKS,
        FAILED_JUMP_EVENT,
        SONDE_HIT,
        TRACER_HIT,
        FLIRD_HIT,
    };
    public enum EndGameType { DEATH_CUBES, CUBES_DESTRUCTIONS, HALF_DEATH_CUBES };
    public enum EjectionType { FIX_TRESHOLD, LOWEST_CUBE_TRESHOLD, LOWEST_CUBE_ARROUND_TRESHOLD };

    [Header("Ejection")]
    public EjectionType ejectionType = EjectionType.FIX_TRESHOLD;
    public float ejectionTreshold = -10.0f;
    [ConditionalHide("ejectionType", EjectionType.LOWEST_CUBE_ARROUND_TRESHOLD)]
    public float ejectionCubesArroundRadius = 20.0f;

    [Header("Endgame")]
    public float endGameDuration = 20.0f;
    public float endGameFrameRate = 0.2f;
    public AnimationCurve endEventCurveSpeed;
    public EndGameType endGameType = EndGameType.DEATH_CUBES;
    public bool bNoEndgame = false;

    [Header("ScreenShake on Endgame")]
    public float screenShakeMagnitude = 2;
    public float screenShakeRoughness = 15;
    public float dureeScreenShake = 3;

    [Header("StarEvents")]
    public List<GameObject> randomEventsPrefabs;

    [Header("AddedEvents")]
    public List<int> nbLumieresTriggers;
    public List<GameObject> eventsToAddPrefabs;

    protected GameManager gm;
    protected MapManager map;
    protected Coroutine coroutineDeathCubesCreation, coroutineCubesDestructions;
    protected bool isEndGameStarted = false;
    protected List<Cube> deathCubes;
    protected bool gameIsEnded = false;
    protected bool gameIsWin = false;
    protected List<RandomEvent> randomEvents;
    protected GameObject randomEventsFolder;

    public virtual void Initialize() {
        name = "EventManager";
        gm = FindObjectOfType<GameManager>();
        map = GameObject.Find("MapManager").GetComponent<MapManager>();
        randomEventsFolder = new GameObject("Events");

        randomEvents = new List<RandomEvent>();
        foreach (GameObject randomEventPrefab in randomEventsPrefabs) {
            AddEvent(randomEventPrefab);
        }
    }

    public RandomEvent AddEvent(GameObject randomEventPrefab) {
        RandomEvent randomEvent = Instantiate(randomEventPrefab, randomEventsFolder.transform).GetComponent<RandomEvent>();
        randomEvents.Add(randomEvent);
        return randomEvent;
    }

    public void RemoveEvent(RandomEvent randomEvent) {
        randomEvents.Remove(randomEvent);
        if(randomEvent != null)
            Destroy(randomEvent.gameObject);
    }

    public virtual void OnLumiereCaptured(Lumiere.LumiereType type) {
        int nbLumieres = map.GetLumieres().Count;
        if (type == Lumiere.LumiereType.NORMAL) {
            if (nbLumieres == 0 && !isEndGameStarted) {
                if (!bNoEndgame) {
                    gm.soundManager.PlayEndGameMusic(); // Ici car lorsqu'il y a plusieurs end-games on ne veut pas que la musique restart !
                    StartEndGame();
                } else {
                    WinGame();
                }
            }
        } else if (type == Lumiere.LumiereType.FINAL) {
            WinGame();
        }
        AddEventsBasedOnLumiereCount(nbLumieres);
        gm.console.OnLumiereCaptured();
    }


    protected virtual void StartEndGame()
    {
        isEndGameStarted = true;

        Lumiere finalLight = CreateFinalLight();

        gm.player.FreezeLocalisation();

        // On lance la création des blocks de la mort !
        if (endGameType == EndGameType.DEATH_CUBES || endGameType == EndGameType.HALF_DEATH_CUBES)
            coroutineDeathCubesCreation = StartCoroutine(FillMapWithDeathCubes(finalLight.transform.position));
        else if (endGameType == EndGameType.CUBES_DESTRUCTIONS)
            coroutineCubesDestructions = StartCoroutine(DestroyAllCubesProgressively(finalLight.transform.position));
    }

    protected virtual Lumiere CreateFinalLight() {
        Vector3 posLumiere = map.GetFarRoundedLocation(gm.player.transform.position);
        Lumiere finalLight = map.CreateLumiere(posLumiere, Lumiere.LumiereType.FINAL); // Attention à la position qui est arrondi ici !
        return finalLight;
    }

    protected void PlaySoundRate(Timer timerSoundRate, int nbCubesToDestroy, Vector3 barycentre) {
        if (timerSoundRate.IsOver()) {
            if(nbCubesToDestroy > 0) {
                barycentre /= nbCubesToDestroy;
            } else {
                barycentre = gm.player.transform.position;
            }
            gm.soundManager.PlayCreateCubeClip(barycentre);
            timerSoundRate.Reset();
        }
    }

    protected Vector3 CreateFirstDeathCubes(List<Vector3> allEmptyPositions, int nbCubesToDestroy) {
        Vector3 barycentre = Vector3.zero;
        for (int i = 0; i < nbCubesToDestroy; i++) {
            Cube cube = CreateCubeForDeathCubesEvent(allEmptyPositions[0]);
            deathCubes.Add(cube);
            barycentre += allEmptyPositions[0];
            allEmptyPositions.RemoveAt(0);
        }

        return barycentre;
    }

    protected int GetNbCubesToDo(int nbPositionsLeft, int nbTotalCubesToDo, int nbCubesDone, Timer endGameTimer) {
        float avancement = endEventCurveSpeed.Evaluate(endGameTimer.GetAvancement());
        int nbCubesToDo = (int)(avancement * nbTotalCubesToDo - nbCubesDone);
        nbCubesToDo = Mathf.Min(nbCubesToDo, nbPositionsLeft);
        return nbCubesToDo;
    }

    protected void OrderPositionsFarFromPlayerAndPos(Vector3 centerPos, List<Vector3> allEmptyPositions) {
        Vector3 playerPos = gm.player.transform.position;
        allEmptyPositions.Sort(delegate (Vector3 A, Vector3 B) {
            float distToA = Mathf.Min(Vector3.Distance(A, centerPos), Vector3.Distance(A, playerPos));
            float distToB = Mathf.Min(Vector3.Distance(B, centerPos), Vector3.Distance(B, playerPos));
            return distToB.CompareTo(distToA);
        });
    }

    protected Cube CreateCubeForDeathCubesEvent(Vector3 pos) {
        Cube cube = null;
        if (endGameType == EndGameType.DEATH_CUBES) {
            cube = map.AddCube(pos, Cube.CubeType.DEATH);
        } else if (endGameType == EndGameType.HALF_DEATH_CUBES) {
            Vector3Int posInt = MathTools.RoundToInt(pos);
            if ((posInt.x + posInt.y + posInt.z) % 2 == 0)
                cube = map.AddCube(pos, Cube.CubeType.DEATH);
            else {
                cube = map.AddCube(pos, Cube.CubeType.NORMAL);
                cube.ShouldRegisterToColorSources();
            }
        }
        return cube;
    }

    protected IEnumerator FillMapWithDeathCubes(Vector3 centerPos) {
        List<Vector3> allEmptyPositions = map.GetAllEmptyPositions();

        Timer endGameTimer = new Timer(endGameDuration);
        Timer timerSoundRate = new Timer(endGameFrameRate);
        int nbTotalDeathCubesToCreate = allEmptyPositions.Count;
        int nbDeathCubesCreated = 0;
        deathCubes = new List<Cube>();

        while (allEmptyPositions.Count > 0) {
            OrderPositionsFarFromPlayerAndPos(centerPos, allEmptyPositions);

            int nbDeathCubesToCreate = GetNbCubesToDo(allEmptyPositions.Count, nbTotalDeathCubesToCreate, nbDeathCubesCreated, endGameTimer);
            Vector3 barycentre = CreateFirstDeathCubes(allEmptyPositions, nbDeathCubesToCreate);
            nbDeathCubesCreated += nbDeathCubesToCreate;

            PlaySoundRate(timerSoundRate, nbDeathCubesToCreate, barycentre);
            yield return null;
        }
    }

    protected IEnumerator DestroyAllCubesProgressively(Vector3 centerPos) {
        List<Cube> cubes = map.GetAllCubes();

        Timer endGameTimer = new Timer(endGameDuration);
        Timer timerSoundRate = new Timer(endGameFrameRate);
        int nbTotalCubesToDestroy = cubes.Count;
        int nbCubesDestroyed = 0;
        float seuilProximité = 2.5f;

        while (cubes.Count > 0) {
            // Pour récupérer les cubes crées pendant la destruction de la map !
            cubes = map.GetAllCubes();
            if (cubes.Count == 0) break;

            OrderCubesByDistancesToPlayerAndPos(centerPos, cubes, seuilProximité);

            int nbCubesToDestroy = GetNbCubesToDo(cubes.Count, nbTotalCubesToDestroy, nbCubesDestroyed, endGameTimer);
            Vector3 barycentre = DestroyFirstCubes(cubes, nbCubesToDestroy);
            nbCubesDestroyed += nbCubesToDestroy;

            PlaySoundRate(timerSoundRate, nbCubesToDestroy, barycentre);
            yield return null;
        }
    }

    protected Vector3 DestroyFirstCubes(List<Cube> cubes, int nbCubesToDestroy) {
        Vector3 barycentre = Vector3.zero;
        for (int i = 0; i < nbCubesToDestroy; i++) {
            barycentre += cubes[i].transform.position;
            cubes[i].Explode();
        }

        return barycentre;
    }

    protected void OrderCubesByDistancesToPlayerAndPos(Vector3 centerPos, List<Cube> cubes, float seuilProximité) {
        // On détruit les cubes aléatoirement
        MathTools.Shuffle(cubes);
        // Sauf ceux qui sont proches de nous et du joueur, on les détruit en dernier !
        Vector3 playerPos = gm.player.transform.position;
        cubes.Sort(delegate (Cube cubeA, Cube cubeB)
        {
            Vector3 A = cubeA.transform.position;
            Vector3 B = cubeB.transform.position;
            bool AinSeuil = Vector3.Distance(A, centerPos) <= seuilProximité || Vector3.Distance(A, playerPos) <= seuilProximité;
            bool BinSeuil = Vector3.Distance(B, centerPos) <= seuilProximité || Vector3.Distance(B, playerPos) <= seuilProximité;
            return AinSeuil.CompareTo(BinSeuil);
        });
    }

    public void LoseGame(DeathReason reason)
    {
        if (gameIsEnded)
            return;
        gameIsEnded = true;
        StopEventsAndEndEvents();

        if (reason != DeathReason.FALL_OUT)
            gm.FreezeTime();

        gm.historyManager.SetDureeGame(gm.timerManager.GetElapsedTime());

        gm.player.FreezePouvoirs();

        gm.console.LoseGame(reason);

        ScreenShakeOnLoseGame();

        gm.soundManager.PlayDefeatClip();

        gm.timerManager.StopScreenShake();

        RememberGameResult(success: false);

        StartCoroutine(gm.QuitInSeconds(7));
    }

    protected void ScreenShakeOnLoseGame() {
        CameraShaker.Instance.ShakeOnce(screenShakeMagnitude, screenShakeRoughness, 0.1f, dureeScreenShake);
    }

    public void WinGame()
    {
        if (gameIsEnded)
            return;
        gameIsEnded = true;
        gameIsWin = true;
        StopEventsAndEndEvents();

        gm.FreezeTime();
        gm.player.FreezePouvoirs();

        gm.historyManager.SetDureeGame(gm.timerManager.GetElapsedTime());

        Debug.Log("WIIIIIIIIIIINNNNNNNNNNNN !!!!!!!!");
        gm.console.WinGame();

        gm.soundManager.PlayVictoryClip();

        gm.timerManager.StopScreenShake();

        RememberGameResult(success: true);

        StartCoroutine(gm.QuitInSeconds(7));
    }

    private void StopEventsAndEndEvents()
    {
        if (coroutineDeathCubesCreation != null)
            StopCoroutine(coroutineDeathCubesCreation);
        if (coroutineCubesDestructions != null)
            StopCoroutine(coroutineCubesDestructions);
        StopEvents();
    }

    protected void RememberGameResult(bool success) {
        if (gm.GetLevelType() == MenuLevel.LevelType.INFINITE)
            success = IsNewBestScore();

        RememberSincelastBestScore(success);
        if (!success) {
            IncrementDeathCount();
        } else {
            IncrementWinsCount();
            RecordBestScore();
            RememberHasJustWin();
        }
        IncrementSumOfAllTriesScores();
    }

    protected void RememberHasJustWin() {
        string keyHasJustWin = GetKeyFor(MenuLevel.HAS_JUST_WIN_KEY);
        PlayerPrefs.SetString(keyHasJustWin, MenuManager.TRUE);
    }

    protected void RecordBestScore() {
        if (IsNewBestScore()) {
            string keyHasJustBestScore = GetKeyFor(MenuLevel.HAS_JUST_MAKE_BEST_SCORE_KEY);
            PlayerPrefs.SetString(keyHasJustBestScore, MenuManager.TRUE);

            string keyPrecedentBestScore = GetKeyFor(MenuLevel.PRECEDENT_BEST_SCORE_KEY);
            PlayerPrefs.SetFloat(keyPrecedentBestScore, GetBestScore());
        }

        string keyHighestScore = GetKeyFor(MenuLevel.BEST_SCORE_KEY);
        float score = GetScore();
        float newValueScore = PlayerPrefs.HasKey(keyHighestScore) ?  Mathf.Max(PlayerPrefs.GetFloat(keyHighestScore), score) : score;
        PlayerPrefs.SetFloat(keyHighestScore, newValueScore);
    }

    protected void IncrementWinsCount() {
        string keyNbWins = GetKeyFor(MenuLevel.NB_WINS_KEY);
        int newValue = PlayerPrefs.HasKey(keyNbWins) ? PlayerPrefs.GetInt(keyNbWins) + 1 : 1;
        PlayerPrefs.SetInt(keyNbWins, newValue);
    }

    protected void IncrementDeathCount() {
        string keyNbDeaths = GetKeyFor(MenuLevel.NB_DEATHS_KEY);
        int newValue = PlayerPrefs.HasKey(keyNbDeaths) ? PlayerPrefs.GetInt(keyNbDeaths) + 1 : 1;
        PlayerPrefs.SetInt(keyNbDeaths, newValue);
    }

    protected void RememberSincelastBestScore(bool hasWin) {
        string keySinceLastBestScore = GetKeyFor(MenuLevel.SINCE_LAST_BEST_SCORE_KEY);
        if (hasWin && IsNewBestScore()) {
            PlayerPrefs.SetInt(keySinceLastBestScore, 0);
        } else {
            int newSinceLastBestScore = 1 + (PlayerPrefs.HasKey(keySinceLastBestScore) ? PlayerPrefs.GetInt(keySinceLastBestScore) : 0);
            PlayerPrefs.SetInt(keySinceLastBestScore, newSinceLastBestScore);
        }
    }

    protected void IncrementSumOfAllTriesScores() {
        string keySum = GetKeyFor(MenuLevel.SUM_OF_ALL_TRIES_SCORES_KEY);
        float oldSum = PlayerPrefs.HasKey(keySum) ? PlayerPrefs.GetFloat(keySum) : 0f;
        float newSum = oldSum + GetScore();
        PlayerPrefs.SetFloat(keySum, newSum);
    }

    public float GetScore() {
        if(gm.GetLevelType() == MenuLevel.LevelType.REGULAR) {
            return gm.timerManager.GetRemainingTime();
        } else {
            return gm.GetInfiniteMap().GetNonStartNbBlocksRun();
        }
    }

    public float GetBestScore() {
        string keyBestScore = GetKeyFor(MenuLevel.BEST_SCORE_KEY);
        return PlayerPrefs.HasKey(keyBestScore) ? PlayerPrefs.GetFloat(keyBestScore) : 0;
    }

    public float GetPrecedentBestScore() {
        string keyPrecedentBestScore = GetKeyFor(MenuLevel.PRECEDENT_BEST_SCORE_KEY);
        return PlayerPrefs.HasKey(keyPrecedentBestScore) ? PlayerPrefs.GetFloat(keyPrecedentBestScore) : 0;
    }

    public bool IsNewBestScore() {
        float currentScore = GetScore();
        return currentScore > GetBestScore();
    }

    protected string GetKeyFor(string keySuffix) {
        string levelNameKey = PlayerPrefs.GetString(MenuLevel.LEVEL_NAME_KEY);
        return levelNameKey + keySuffix;
    }

    public bool IsGameOver() {
        return gameIsEnded;
    }

    protected virtual bool IsPlayerEjected() {
        switch(ejectionType) {
            case EjectionType.FIX_TRESHOLD:
                return gm.gravityManager.GetHigh(gm.player.transform.position) < ejectionTreshold;
            case EjectionType.LOWEST_CUBE_TRESHOLD:
                float lowest = GetLessHighCubeAltitude(gm.map.GetAllCubes());
                return gm.gravityManager.GetHigh(gm.player.transform.position) < lowest + ejectionTreshold;
            case EjectionType.LOWEST_CUBE_ARROUND_TRESHOLD:
                List<Cube> cubesArround = gm.map.GetCubesInSphere(gm.player.transform.position, ejectionCubesArroundRadius);
                if (cubesArround.Count == 0)
                    return true;
                float lowestHeight = GetLessHighCubeAltitude(cubesArround);
                return gm.gravityManager.GetHigh(gm.player.transform.position) < lowestHeight + ejectionTreshold;
            default:
                return true;
        }
    }

    protected float GetLessHighCubeAltitude(List<Cube> cubes) {
        if (!cubes.Any())
            return 0;
        float lowest = gm.gravityManager.GetHigh(cubes[0].transform.position);
        foreach(Cube cube in cubes) {
            float altitude = gm.gravityManager.GetHigh(cube.transform.position);
            lowest = Mathf.Min(lowest, altitude);
        }
        return lowest;
    }

    public bool PartieTermine()
    {
        if (IsPlayerEjected())
        {
            LoseGame(EventManager.DeathReason.FALL_OUT);
            return true;
        }

        if (IsPlayerCaptured()) {
            LoseGame(EventManager.DeathReason.CAPTURED);
            return true;
        }

        return false;
    }

    protected bool IsPlayerCaptured() {
        return gm.player.ennemiCaptureTimer.IsOver();
    }

    public bool IsWin() {
        return gameIsWin;
    }

    public bool IsEndGameStarted() {
        return isEndGameStarted;
    }

    protected void StopEvents() {
        foreach(RandomEvent randomEvent in randomEvents) {
            randomEvent.StopEvent();
        }
    }

    protected void AddEventsBasedOnLumiereCount(int nbLumieres) {
        for(int i = 0; i < nbLumieresTriggers.Count; i++) {
            int nbLumieresTrigger = nbLumieresTriggers[i];
            if(nbLumieresTrigger == nbLumieres) {
                RandomEvent newEvent = AddEvent(eventsToAddPrefabs[i]);
                newEvent.Start();
                newEvent.TriggerEvent();
            }
        }
    }
}