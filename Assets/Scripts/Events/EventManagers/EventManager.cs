﻿using EZCameraShake;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        TRACER_BLAST,
        FIRST_BOSS_HIT,
        FIRST_BOSS_BLAST,
        SOUL_ROBBER_ASPIRATION,
    };
    public enum EndEventType {
        DEATH_CUBES,
        HALF_DEATH_CUBES,
        CUBES_DESTRUCTIONS,
        EMPTY_END_EVENT,
        CUBES_DESTRUCTIONS_PARTIAL, // Ne pas réordonner sinon ça va changer les prefabs !
    };
    public enum EndEventCubesSelectionMethod {
        FAR_FROM_PLAYER_AND_POS,
        FROM_TOP_OR_BOTTOM,
        FAR_FROM_POS,
        RANDOM_NOT_CLOSE_TO_PLAYER_AND_POS,
    }
    public enum FinalLightSpawnMode {
        FAR_FROM_PLAYER,
        OPTIMALLY_SPACED_FROM_PLAYER,
    }
    public enum EjectionType {
        FIX_TRESHOLD,
        LOWEST_CUBE_TRESHOLD,
        LOWEST_CUBE_ARROUND_TRESHOLD,
    };
    public enum QuitType {
        QUIT,
        RELOAD,
    };

    [Header("Ejection")]
    public EjectionType ejectionType = EjectionType.FIX_TRESHOLD;
    public float ejectionTreshold = -10.0f;
    [ConditionalHide("ejectionType", EjectionType.LOWEST_CUBE_ARROUND_TRESHOLD)]
    public float ejectionCubesArroundRadius = 20.0f;

    [Header("Endgame")]
    public float endGameDuration = 20.0f;
    public float endGameFrameRate = 0.2f;
    public AnimationCurve endEventCurveSpeed;
    public EndEventType endGameType = EndEventType.DEATH_CUBES;
    public EndEventCubesSelectionMethod endEventCubesSelectionMethod = EndEventCubesSelectionMethod.FAR_FROM_PLAYER_AND_POS;
    [ConditionalHide("endEventCubesSelectionMethod", EndEventCubesSelectionMethod.RANDOM_NOT_CLOSE_TO_PLAYER_AND_POS)]
    public float endEventSeuilProximite = 2.5f;
    public float dureeDestructionCubesDestruction = 2.0f;
    [ConditionalHide("endGameType", EndEventType.CUBES_DESTRUCTIONS_PARTIAL)]
    public float proportionToKeep = 0.0f;
    public bool bNoEndgame = false;

    [Header("Final Light")]
    public FinalLightSpawnMode finalLightSpawnMode = FinalLightSpawnMode.FAR_FROM_PLAYER;
    public List<MapFunctionComponent> mapFunctionsOnCreateFinalLight;

    [Header("ScreenShake on Endgame")]
    public float screenShakeMagnitude = 2;
    public float screenShakeRoughness = 15;
    public float dureeScreenShake = 3;

    [Header("StarEvents (Random/Single)")]
    public List<GameObject> eventsPrefabs;

    [Header("AddedEvents")]
    public List<int> nbLumieresTriggers;
    public List<SingleEvent> nbLumieresSingleEvents;

    [Header("ChangingScenesTimes")]
    public float quitSceneTime = 7.0f;
    public float reloadSceneTime = 7.0f;

    protected GameManager gm;
    protected MapManager map;
    protected Coroutine coroutineDeathCubesCreation, coroutineCubesDestructions;
    protected bool isEndGameStarted = false;
    protected List<Cube> deathCubes;
    protected bool gameIsEnded = false;
    protected bool gameIsWin = false;
    protected bool gameIsLost = false;
    protected List<RandomEvent> randomEvents;
    protected Transform randomEventsFolder;
    protected Transform singleEventsFolder;
    protected bool shouldAutomaticallyQuitScene = true;
    protected Coroutine automaticallyQuitSceneCoroutine = null;
    protected List<object> elementsToBeDoneBeforeStartEndGame;

    public virtual void Initialize() {
        name = "EventManager";
        gm = FindObjectOfType<GameManager>();
        map = gm.map;
        randomEventsFolder = new GameObject("RandomEvents").transform;
        singleEventsFolder = new GameObject("SingleEvents").transform;
        elementsToBeDoneBeforeStartEndGame = new List<object>();
        PreFillPoolIfDeathCubesEndEvent();

        AddRandomEventsAndStartSingleEvents();
    }

    private void AddRandomEventsAndStartSingleEvents() {
        randomEvents = new List<RandomEvent>();
        foreach (GameObject eventPrefab in eventsPrefabs) {
            if (eventPrefab.GetComponent<RandomEvent>() != null) {
                AddRandomEvent(eventPrefab);
            } else if (eventPrefab.GetComponent<SingleEvent>() != null) {
                StartSingleEvent(eventPrefab);
            }
        }
    }

    public virtual void Update() {
        CheckPartiePerdu();

        CheckPlayerUsingArrowKeys();
    }

    public RandomEvent AddRandomEvent(GameObject randomEventPrefab) {
        RandomEvent randomEvent = Instantiate(randomEventPrefab, randomEventsFolder).GetComponent<RandomEvent>();
        randomEvents.Add(randomEvent);
        randomEvent.Initialize();
        return randomEvent;
    }

    public SingleEvent StartSingleEvent(GameObject eventPrefab) {
        SingleEvent singleEvent = Instantiate(eventPrefab, singleEventsFolder).GetComponent<SingleEvent>();
        singleEvent.Initialize();
        singleEvent.Trigger();
        return singleEvent;
    }

    public void RemoveEvent(RandomEvent randomEvent) {
        randomEvents.Remove(randomEvent);
        if(randomEvent != null)
            Destroy(randomEvent.gameObject);
    }

    public virtual void OnLumiereCaptured(Lumiere.LumiereType type) {
        int nbLumieres = map.GetLumieres().Count;
        if (type == Lumiere.LumiereType.NORMAL) {
            if (nbLumieres == 0 && !isEndGameStarted && NoMoreElementsToBeDoneBeforeEndGame()) {
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
        TriggerSingleEventsBasedOnLumiereCount(nbLumieres);
        TestNewTresholdReached();
        gm.console.OnLumiereCaptured();
    }

    public void ExternalStartEndGame() {
        gm.soundManager.PlayEndGameMusic(); // Ici car lorsqu'il y a plusieurs end-games on ne veut pas que la musique restart !
        StartEndGame();
    }

    protected virtual void StartEndGame()
    {
        isEndGameStarted = true;

        Lumiere finalLight = CreateFinalLight();

        gm.player.FreezeLocalisation();

        // On lance la création des blocks de la mort !
        if (endGameType == EndEventType.DEATH_CUBES || endGameType == EndEventType.HALF_DEATH_CUBES) {
            coroutineDeathCubesCreation = StartCoroutine(FillMapWithDeathCubes(finalLight.transform.position));
        } else if (endGameType == EndEventType.CUBES_DESTRUCTIONS || endGameType == EndEventType.CUBES_DESTRUCTIONS_PARTIAL) {
            coroutineCubesDestructions = StartCoroutine(DestroyAllCubesProgressively(finalLight.transform.position));
        }
    }

    protected virtual Lumiere CreateFinalLight(Lumiere.LumiereType lumiereType = Lumiere.LumiereType.FINAL) {
        Vector3 posLumiere = GetFinalLightPos();
        Lumiere finalLight = map.CreateLumiere(posLumiere, lumiereType); // Attention à la position qui est arrondi ici !
        foreach (MapFunctionComponent function in mapFunctionsOnCreateFinalLight) {
            function.Initialize();
            function.Activate();
        }
        return finalLight;
    }

    protected virtual Vector3 GetFinalLightPos() {
        Vector3 posLumiere;
        switch (finalLightSpawnMode) {
            case FinalLightSpawnMode.FAR_FROM_PLAYER:
                posLumiere = map.GetFarRoundedLocation(gm.player.transform.position);
                return posLumiere;
            case FinalLightSpawnMode.OPTIMALLY_SPACED_FROM_PLAYER:
                List<Vector3> farFromPositions = new List<Vector3>() { gm.player.transform.position };
                posLumiere = GetOptimalySpacedPositions.GetOneSpacedPosition(map, farFromPositions, 100, mode: GetOptimalySpacedPositions.Mode.MAX_MIN_DISTANCE);
                return posLumiere;
            default:
                throw new Exception($"Valeur inconnu pour finalLightSpawnMode ({finalLightSpawnMode}) dans GetFinalLightPos() !");
        }
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
        int nbCubesToDo = Mathf.CeilToInt(avancement * nbTotalCubesToDo - nbCubesDone);
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

    protected void OrderCubesFarFromPlayerAndPos(Vector3 centerPos, List<Cube> cubes) {
        Vector3 playerPos = gm.player.transform.position;
        cubes.Sort(delegate (Cube A, Cube B) {
            float distToA = Mathf.Min(Vector3.Distance(A.transform.position, centerPos), Vector3.Distance(A.transform.position, playerPos));
            float distToB = Mathf.Min(Vector3.Distance(B.transform.position, centerPos), Vector3.Distance(B.transform.position, playerPos));
            return distToB.CompareTo(distToA);
        });
    }

    protected void OrderPositionsFarFromPos(Vector3 centerPos, List<Vector3> allEmptyPositions) {
        allEmptyPositions.Sort(delegate (Vector3 A, Vector3 B) {
            float distToA = Vector3.Distance(A, centerPos);
            float distToB = Vector3.Distance(B, centerPos);
            return distToB.CompareTo(distToA);
        });
    }

    protected void OrderPositionsFromTopOrBottom(Vector3 exitPos, List<Vector3> allEmptyPositions) {
        Vector3 playerPos = gm.player.transform.position;
        GravityManager.Direction direction;
        if(gm.gravityManager.GetHeightInMap(playerPos) <= gm.gravityManager.GetHeightInMap(exitPos)) { // Player is under exit
            direction = GravityManager.OppositeDir(gm.gravityManager.gravityDirection);
        } else {
            direction = gm.gravityManager.gravityDirection;
        }
        allEmptyPositions.Sort(delegate (Vector3 A, Vector3 B) {
            float distToA = map.GetTrancheIndice(A, direction) + Vector3.SqrMagnitude(A - exitPos) / 1000f;
            float distToB = map.GetTrancheIndice(B, direction) + Vector3.SqrMagnitude(B - exitPos) / 1000f;
            return distToB.CompareTo(distToA);
        });
    }

    protected Cube CreateCubeForDeathCubesEvent(Vector3 pos) {
        Cube cube = null;
        if (endGameType == EndEventType.DEATH_CUBES) {
            cube = map.AddCube(pos, Cube.CubeType.DEATH);
        } else if (endGameType == EndEventType.HALF_DEATH_CUBES) {
            Vector3Int posInt = MathTools.RoundToInt(pos);
            if ((posInt.x + posInt.y + posInt.z) % 2 == 0)
                cube = map.AddCube(pos, Cube.CubeType.DEATH);
            else {
                cube = map.AddCube(pos, Cube.CubeType.NORMAL);
                cube.RegisterCubeToColorSources();
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

        if (endEventCubesSelectionMethod == EndEventCubesSelectionMethod.FROM_TOP_OR_BOTTOM) {
            OrderPositionsFromTopOrBottom(centerPos, allEmptyPositions);
        }

        while (allEmptyPositions.Count > 0) {
            if (endEventCubesSelectionMethod == EndEventCubesSelectionMethod.FAR_FROM_PLAYER_AND_POS) {
                OrderPositionsFarFromPlayerAndPos(centerPos, allEmptyPositions);
            } else if (endEventCubesSelectionMethod == EndEventCubesSelectionMethod.FAR_FROM_POS) {
                OrderPositionsFarFromPos(centerPos, allEmptyPositions);
            }

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
        int nbCubesToReach = 0;
        if(endGameType == EndEventType.CUBES_DESTRUCTIONS_PARTIAL) {
            nbCubesToReach = (int)(cubes.Count * proportionToKeep);
            nbTotalCubesToDestroy = (int)(cubes.Count * (1.0f - proportionToKeep));
        }
        int nbCubesDestroyed = 0;

        while (ShouldKeepDestroyingCubes(cubes.Count, nbCubesToReach, endGameTimer)) {
            // Pour récupérer les cubes crées pendant la destruction de la map !
            cubes = map.GetAllCubes().FindAll(c => !c.IsDecomposing());
            if (cubes.Count == 0) break;

            OrderCubesAccordingToMethod(centerPos, cubes);

            int nbCubesToDestroy = GetNbCubesToDo(cubes.Count, nbTotalCubesToDestroy, nbCubesDestroyed, endGameTimer);
            Vector3 barycentre = DestroyFirstCubes(cubes, nbCubesToDestroy);
            nbCubesDestroyed += nbCubesToDestroy;

            PlaySoundRate(timerSoundRate, nbCubesToDestroy, barycentre);
            yield return null;
        }
        Debug.Log($"{map.GetAllCubes().FindAll(c => !c.IsDecomposing()).Count}/{nbCubesToReach}");
    }

    protected void OrderCubesAccordingToMethod(Vector3 pos, List<Cube> cubes) {
        switch (endEventCubesSelectionMethod) {
            case EndEventCubesSelectionMethod.FAR_FROM_PLAYER_AND_POS:
                OrderCubesFarFromPlayerAndPos(pos, cubes);
                break;
            case EndEventCubesSelectionMethod.FROM_TOP_OR_BOTTOM:
                throw new NotImplementedException();
                break;
            case EndEventCubesSelectionMethod.FAR_FROM_POS:
                throw new NotImplementedException();
                break;
            case EndEventCubesSelectionMethod.RANDOM_NOT_CLOSE_TO_PLAYER_AND_POS:
                OrderCubesByDistancesToPlayerAndPos(pos, cubes);
                break;
        }
    }

    protected bool ShouldKeepDestroyingCubes(int nbCubes, int nbCubesToReach, Timer endGameTimer) {
        if(endGameType == EndEventType.CUBES_DESTRUCTIONS) {
            return nbCubes > 0;
        } else {
            return nbCubes > nbCubesToReach && !endGameTimer.IsOver();
        }
    }

    protected Vector3 DestroyFirstCubes(List<Cube> cubes, int nbCubesToDestroy) {
        Vector3 barycentre = Vector3.zero;
        for (int i = 0; i < nbCubesToDestroy; i++) {
            barycentre += cubes[i].transform.position;
            cubes[i].Decompose(dureeDestructionCubesDestruction);
        }

        return barycentre;
    }

    protected void OrderCubesByDistancesToPlayerAndPos(Vector3 centerPos, List<Cube> cubes) {
        // On détruit les cubes aléatoirement
        MathTools.Shuffle(cubes);
        // Sauf ceux qui sont proches de nous et du joueur, on les détruit en dernier !
        Vector3 playerPos = gm.player.transform.position;
        cubes.Sort(delegate (Cube cubeA, Cube cubeB)
        {
            Vector3 A = cubeA.transform.position;
            Vector3 B = cubeB.transform.position;
            bool AinSeuil = Vector3.Distance(A, centerPos) <= endEventSeuilProximite || Vector3.Distance(A, playerPos) <= endEventSeuilProximite;
            bool BinSeuil = Vector3.Distance(B, centerPos) <= endEventSeuilProximite || Vector3.Distance(B, playerPos) <= endEventSeuilProximite;
            return AinSeuil.CompareTo(BinSeuil);
        });
    }

    public void LoseGame(DeathReason reason) {
        if (gameIsEnded || gm.player.IsInvincible())
            return;
        gameIsEnded = true;
        gameIsLost = true;

        RememberGameResult(success: false);

        StopEventsAndEndEvents();

        if (reason != DeathReason.FALL_OUT)
            gm.FreezeTime();

        if (gm.GetMapType() == MenuLevel.LevelType.INFINITE)
            gm.GetInfiniteMap().RememberLastKillingBlock();

        gm.historyManager.SetDureeGame(gm.timerManager.GetRealGameTimer().GetElapsedTime());

        gm.player.FreezePouvoirs();

        gm.player.geoSphere.StopAllContinueGeoPoints();

        gm.console.LoseGame(reason);

        ScreenShakeOnLoseGame();

        gm.soundManager.PlayDefeatClip();

        gm.timerManager.StopScreenShake();

        QuitOrReloadInSeconds();
    }

    public void LoseGameWithTimeOut() {
        LoseGame(DeathReason.TIME_OUT);
    }

    public void QuitOrReloadInSeconds() {
        QuitType quitType = ShouldQuitOrReload();
        if(quitType == QuitType.QUIT) {
            QuitSceneInseconds(quitSceneTime);
        } else {
            ReloadSceneInSeconds(reloadSceneTime);
        }
    }

    public void QuitOrReload() {
        QuitType quitType = ShouldQuitOrReload();
        if(quitType == QuitType.QUIT) {
            gm.QuitterPartie();
        } else {
            ReloadScene();
        }
    }

    public QuitType ShouldQuitOrReload() {
        if(!IsGameOver()) {
            return QuitType.QUIT;
        }
        if(IsGameWin()) {
            return QuitType.QUIT;
        }
        if(gm.GetMapType() == MenuLevel.LevelType.REGULAR) {
            return QuitType.RELOAD;
        }
        //return QuitType.QUIT; // To Always see IR Reward ! :)
        // Below it is only IR levels :)
        if (IsIRFirstTresholdHasBeenBeaten()) {
            if (HasJustBeatIRFirstTreshold()) {
                return QuitType.QUIT;
            }
            if (IsNewBestScoreAfterBestScoreAssignation()) {
                return QuitType.QUIT;
            }
        }
        return QuitType.RELOAD;
    }

    protected void ReloadSceneInSeconds(float seconds) {
        if(shouldAutomaticallyQuitScene) {
            automaticallyQuitSceneCoroutine = StartCoroutine(CReloadSceneInSeconds(seconds));
        }
    }

    protected IEnumerator CReloadSceneInSeconds(float seconds) {
        yield return new WaitForSeconds(seconds);
        ReloadScene();
    }

    public void ReloadScene() {
        Time.timeScale = 1.0f;
        string sceneName = SceneManager.GetActiveScene().name;
        Destroy(gm.historyManager.gameObject);
        SceneManager.LoadScene(sceneName);
    }

    protected void QuitSceneInseconds(float seconds) {
        if (shouldAutomaticallyQuitScene) {
            automaticallyQuitSceneCoroutine = StartCoroutine(gm.QuitInSeconds(seconds));
        }
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

        gm.player.geoSphere.StopAllContinueGeoPoints();

        gm.historyManager.SetDureeGame(gm.timerManager.GetRealGameTimer().GetElapsedTime());

        Debug.Log("WIIIIIIIIIIINNNNNNNNNNNN !!!!!!!!");
        gm.console.WinGame();

        gm.soundManager.PlayVictoryClip();

        gm.timerManager.StopScreenShake();

        RememberGameResult(success: true);

        QuitOrReloadInSeconds();
    }

    private void StopEventsAndEndEvents() {
        StartUnrobbIfSoulRobbers();
        if (coroutineDeathCubesCreation != null)
            StopCoroutine(coroutineDeathCubesCreation);
        if (coroutineCubesDestructions != null)
            StopCoroutine(coroutineCubesDestructions);
        StopEventsAtEndOfFrame();
    }

    protected void StartUnrobbIfSoulRobbers() {
        if (SoulRobber.IsPlayerRobbed() && IsGameWin()) {
            SoulRobber soulRobber = gm.ennemiManager.GetEnnemisOfType<SoulRobber>().First();
            soulRobber.StartUnrobb();
        }
    }

    public void RememberGameResult(bool success) {
        if (gm.GetMapType() == MenuLevel.LevelType.INFINITE)
            success = IsNewBestScore();

        RememberSincelastBestScore(success);
        gm.historyManager.score = GetScore();
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
        string keyHasJustWin = GetKeyFor(PrefsManager.HAS_JUST_WIN_KEY);
        PrefsManager.SetBool(keyHasJustWin, true);
    }

    protected void RecordBestScore() {
        if (IsNewBestScore()) {
            string keyHasJustBestScore = GetKeyFor(PrefsManager.HAS_JUST_MAKE_BEST_SCORE_KEY);
            PrefsManager.SetBool(keyHasJustBestScore, true);
        }

        string bestScoreKey = GetKeyFor(PrefsManager.BEST_SCORE_KEY);
        string precedentBestScoreKey = GetKeyFor(PrefsManager.PRECEDENT_BEST_SCORE_KEY);
        float score = GetScore();
        float bestScore = PrefsManager.GetFloat(bestScoreKey, 0);
        float precedentBestScore = PrefsManager.GetFloat(precedentBestScoreKey, 0);
        if(score > bestScore) {
            PlayerPrefs.SetFloat(bestScoreKey, score);
            PlayerPrefs.SetFloat(precedentBestScoreKey, bestScore);
        } else if(score > precedentBestScore) {
            PlayerPrefs.SetFloat(precedentBestScoreKey, score);
        }
    }

    protected void IncrementWinsCount() {
        string keyNbWins = GetKeyFor(PrefsManager.NB_WINS_KEY);
        int newValue = PlayerPrefs.HasKey(keyNbWins) ? PlayerPrefs.GetInt(keyNbWins) + 1 : 1;
        PlayerPrefs.SetInt(keyNbWins, newValue);
    }

    protected void IncrementDeathCount() {
        string keyNbDeaths = GetKeyFor(PrefsManager.NB_DEATHS_KEY);
        int newValue = PlayerPrefs.HasKey(keyNbDeaths) ? PlayerPrefs.GetInt(keyNbDeaths) + 1 : 1;
        PlayerPrefs.SetInt(keyNbDeaths, newValue);
    }

    protected void RememberSincelastBestScore(bool hasWin) {
        string keySinceLastBestScore = GetKeyFor(PrefsManager.SINCE_LAST_BEST_SCORE_KEY);
        if (hasWin && IsNewBestScore()) {
            PlayerPrefs.SetInt(keySinceLastBestScore, 0);
        } else {
            int newSinceLastBestScore = 1 + (PlayerPrefs.HasKey(keySinceLastBestScore) ? PlayerPrefs.GetInt(keySinceLastBestScore) : 0);
            PlayerPrefs.SetInt(keySinceLastBestScore, newSinceLastBestScore);
        }
    }

    protected void IncrementSumOfAllTriesScores() {
        string keySum = GetKeyFor(PrefsManager.SUM_OF_ALL_TRIES_SCORES_KEY);
        float oldSum = PlayerPrefs.HasKey(keySum) ? PlayerPrefs.GetFloat(keySum) : 0f;
        float newSum = oldSum + GetScore();
        PlayerPrefs.SetFloat(keySum, newSum);
    }

    public float GetScore() {
        if(gm.GetMapType() == MenuLevel.LevelType.REGULAR) {
            return gm.timerManager.GetRemainingTime();
        } else {
            return gm.GetInfiniteMap().GetNonStartNbBlocksRun();
        }
    }

    public float GetBestScore() {
        string keyBestScore = GetKeyFor(PrefsManager.BEST_SCORE_KEY);
        return PrefsManager.GetFloat(keyBestScore, 0);
    }

    public float GetPrecedentBestScore() {
        string key = GetKeyFor(PrefsManager.PRECEDENT_BEST_SCORE_KEY);
        return PrefsManager.GetFloat(key, 0);
    }

    public bool IsNewBestScore() {
        float currentScore = GetScore();
        return currentScore > GetBestScore();
    }

    public bool IsNewBestScoreAfterBestScoreAssignation() {
        string hasMakeBestScoreKey = GetKeyFor(PrefsManager.HAS_JUST_MAKE_BEST_SCORE_KEY);
        string precedentBestScoreKey = GetKeyFor(PrefsManager.PRECEDENT_BEST_SCORE_KEY);
        return PrefsManager.GetBool(hasMakeBestScoreKey, false) && PrefsManager.GetFloat(precedentBestScoreKey, 0) != 0;
    }

    protected bool IsIRFirstTresholdHasBeenBeaten() {
        string bestScoreKey = GetKeyFor(PrefsManager.BEST_SCORE_KEY);
        float firstTreshold = (float)gm.goalManager.GetFirstTreshold();
        return PrefsManager.GetFloat(bestScoreKey, 0) >= firstTreshold;
    }

    protected bool HasJustBeatIRFirstTreshold() {
        float score = GetScore();
        float firstTreshold = (float)gm.goalManager.GetFirstTreshold();
        string precedentBestScoreKey = GetKeyFor(PrefsManager.PRECEDENT_BEST_SCORE_KEY);
        return score >= firstTreshold && PrefsManager.GetFloat(precedentBestScoreKey, 0) < firstTreshold;
    }

    public string GetKeyFor(string keySuffix) {
        string levelNameKey = SceneManager.GetActiveScene().name;
        return levelNameKey + keySuffix;
    }

    protected virtual bool IsPlayerEjected() {
        switch (ejectionType) {
            case EjectionType.FIX_TRESHOLD:
                return gm.gravityManager.GetHeightInMap(gm.player.transform.position) < ejectionTreshold;
            case EjectionType.LOWEST_CUBE_TRESHOLD:
                float lowest = GetLessHighCubeAltitude(gm.map.GetAllCubes());
                return gm.gravityManager.GetHeightInMap(gm.player.transform.position) < lowest + ejectionTreshold;
            case EjectionType.LOWEST_CUBE_ARROUND_TRESHOLD:
                List<Cube> cubesArround = gm.map.GetCubesInSphere(gm.player.transform.position, ejectionCubesArroundRadius);
                if (cubesArround.Count == 0)
                    return true;
                float lowestHeight = GetLessHighCubeAltitude(cubesArround);
                float playerHeight = gm.gravityManager.GetHeightInMap(gm.player.transform.position);
                return playerHeight < lowestHeight + ejectionTreshold;
            default:
                return true;
        }
    }

    protected float GetLessHighCubeAltitude(List<Cube> cubes) {
        if (!cubes.Any())
            return 0;
        return cubes.Select(c => gm.gravityManager.GetHeightInMap(c.transform.position)).Min();
    }

    public bool CheckPartiePerdu() {
        if (IsPlayerEjected() && !gm.player.IsInvincible()) {
            LoseGame(EventManager.DeathReason.FALL_OUT);
            return true;
        }

        return false;
    }

    public bool IsGameOver() {
        return gameIsEnded;
    }

    public bool IsGameWin() {
        return gameIsWin;
    }

    public bool IsGameLost() {
        return gameIsLost;
    }

    public bool IsEndGameStarted() {
        return isEndGameStarted;
    }

    protected void StopEventsAtEndOfFrame() {
        StartCoroutine(CStopEventsAtEndOfFrame());
    }

    protected IEnumerator CStopEventsAtEndOfFrame() {
        yield return new WaitForEndOfFrame();
        StopEvents();
    }

    protected void StopEvents() {
        foreach(RandomEvent randomEvent in randomEvents) {
            randomEvent.StopEvent();
        }
    }

    protected void TriggerSingleEventsBasedOnLumiereCount(int nbLumieres) {
        for(int i = 0; i < nbLumieresTriggers.Count; i++) {
            if(nbLumieresTriggers[i] == nbLumieres) {
                nbLumieresSingleEvents[i].Initialize();
                nbLumieresSingleEvents[i].Trigger();
            }
        }
    }

    public void ShouldNotAutomaticallyQuit() {
        shouldAutomaticallyQuitScene = false;
        if(automaticallyQuitSceneCoroutine != null) {
            StopCoroutine(automaticallyQuitSceneCoroutine);
        }
    }

    protected void CheckPlayerUsingArrowKeys() {
        if(Input.GetKeyDown(KeyCode.UpArrow)
        || Input.GetKeyDown(KeyCode.DownArrow)
        || Input.GetKeyDown(KeyCode.LeftArrow)
        || Input.GetKeyDown(KeyCode.RightArrow)) {
            gm.console.DontUseArrowKeys();
        }
    }

    public void Add10DataCount() {
        int nb = 10;
        int dataCount = Lumiere.IncrementDataCount(nb);
        gm.console.AddToDataCountText(dataCount, nb);
    }

    public void Add100DataCount() {
        int nb = 100;
        int dataCount = Lumiere.IncrementDataCount(nb);
        gm.console.AddToDataCountText(dataCount, nb);
    }

    protected void TestNewTresholdReached() {
        if(gm.GetMapType() == MenuLevel.LevelType.REGULAR) {
            int dataCount = Lumiere.GetCurrentDataCount();
            if(gm.goalManager.GetAllTresholds().Contains(dataCount)) {
                RewardForNewRegularTresholdReached(dataCount);
            }
        }
    }

    protected void RewardForNewRegularTresholdReached(int dataCount) {
        gm.console.RewardNewRegularTreshold(dataCount);
        gm.soundManager.PlayRewardBestScore();
    }

    protected void PreFillPoolIfDeathCubesEndEvent() {
        if(endGameType == EndEventType.DEATH_CUBES || endGameType == EndEventType.HALF_DEATH_CUBES) {
            List<Vector3> emptyPositions = map.GetAllEmptyPositions();
            for(int i = 0; i < emptyPositions.Count; i++) {
                Cube.CubeType cubeType = Cube.CubeType.DEATH;
                if(endGameType == EndEventType.HALF_DEATH_CUBES && i > emptyPositions.Count / 2) {
                    cubeType = Cube.CubeType.NORMAL;
                }
                map.PoolAddCube(cubeType);
            }
        }
    }

    public void AddElementToBeDoneBeforeEndGame(object element) {
        if (!elementsToBeDoneBeforeStartEndGame.Contains(element)) {
            elementsToBeDoneBeforeStartEndGame.Add(element);
        }
    }

    public void RemoveElementToBeDoneBeforeEndGame(object element) {
        elementsToBeDoneBeforeStartEndGame.Remove(element);
    }
    
    public bool NoMoreElementsToBeDoneBeforeEndGame() {
        return elementsToBeDoneBeforeStartEndGame.Count == 0;
    }
}