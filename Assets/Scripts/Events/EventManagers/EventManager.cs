using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Le but de la DataBase est de gérer le comportement de tout ce qui entrave le joueur.
// Cela va de la coordination des Drones, à la génération d'évenements néffastes.
public class EventManager : MonoBehaviour {
    public enum DeathReason { TIME_OUT, CAPTURED, FALL_OUT, TOUCHED_DEATH_CUBE, OUT_OF_BLOCKS };
    public enum EndGameType { DEATH_CUBES, CUBES_DESTRUCTIONS };

    [Header("Ejection")]
    public bool ejectionTresholdUseLastCubePosition = false;
    public float ejectionTreshold = -10.0f;

    [Header("Endgame")]
    public float endGameDuration = 20.0f;
    public float endGameFrameRate = 0.2f;
    public EndGameType endGameType = EndGameType.DEATH_CUBES;
    public bool bNoEndgame = false;

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

    public void Initialize() {
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

        // On crée la finaleLight
        //float minRadius = 0.5f + Mathf.Sqrt(3) / 2.0f; // La demi taille de la sphère + la demi-diagonale d'un cube
        //Vector3 posLumiere = map.GetFreeSphereLocation(minRadius);
        Vector3 posLumiere = map.GetFarRoundedLocation(gm.player.transform.position);
        Lumiere finalLight = map.CreateLumiere(posLumiere, Lumiere.LumiereType.FINAL); // Attention à la position qui est arrondi ici !

        gm.player.FreezeLocalisation();

        //gm.console.StartEndGame();

        // On lance la création des blocks de la mort !
        if (endGameType == EndGameType.DEATH_CUBES)
            coroutineDeathCubesCreation = StartCoroutine(FillMapWithDeathCubes(finalLight.transform.position));
        else if (endGameType == EndGameType.CUBES_DESTRUCTIONS)
            coroutineCubesDestructions = StartCoroutine(DestroyAllCubesProgressively(finalLight.transform.position));
    }

    protected IEnumerator FillMapWithDeathCubes(Vector3 centerPos) {
        List<Vector3> allEmptyPositions = map.GetAllEmptyPositions();

        Vector3 playerPos = gm.player.transform.position;

        float nbTimings = endGameDuration / endGameFrameRate;
        int nbCubesToDestroy = (int)(allEmptyPositions.Count / nbTimings);
        deathCubes = new List<Cube>();

        while (allEmptyPositions.Count > 0) {
            playerPos = gm.player.transform.position;
            allEmptyPositions.Sort(delegate (Vector3 A, Vector3 B) {
                float distToA = Mathf.Min(Vector3.Distance(A, centerPos), Vector3.Distance(A, playerPos));
                float distToB = Mathf.Min(Vector3.Distance(B, centerPos), Vector3.Distance(B, playerPos));
                return distToB.CompareTo(distToA);
            });

            Vector3 barycentre = Vector3.zero;
            int nbCubesDestroyed = (int)Mathf.Min(nbCubesToDestroy, allEmptyPositions.Count);
            for (int i = 0; i < nbCubesDestroyed; i++)
            {
                Cube cube = map.AddCube(allEmptyPositions[i], Cube.CubeType.DEATH);
                deathCubes.Add(cube);
                barycentre += allEmptyPositions[i];
            }
            for (int i = 0; i < nbCubesDestroyed; i++)
            {
                allEmptyPositions.RemoveAt(0);
            }

            barycentre /= nbCubesDestroyed;
            gm.soundManager.PlayCreateCubeClip(barycentre);
            yield return new WaitForSeconds(endGameFrameRate);
        }
        for (int i = 0; i < allEmptyPositions.Count; i += nbCubesToDestroy)
        {
            Vector3 barycentre = Vector3.zero;
            for (int j = i; j < (int)Mathf.Min(i + nbCubesToDestroy, allEmptyPositions.Count); j++)
            {
                Cube cube = map.AddCube(allEmptyPositions[j], Cube.CubeType.DEATH);
                deathCubes.Add(cube);
                barycentre += allEmptyPositions[j];
            }
            barycentre /= nbCubesToDestroy;
            gm.soundManager.PlayCreateCubeClip(barycentre);
            yield return new WaitForSeconds(endGameFrameRate);
        }
    }

    protected IEnumerator DestroyAllCubesProgressively(Vector3 centerPos) {
        List<Cube> cubes = map.GetAllCubes();

        Vector3 playerPos = gm.player.transform.position;

        float nbTimings = endGameDuration / endGameFrameRate;
        int nbCubesToDestroy = (int)(cubes.Count / nbTimings);

        float seuilProximité = 2.5f;
        while (cubes.Count > 0) {
            // Pour récupérer les cubes crées pendant la destruction de la map !
            cubes = map.GetAllCubes();
            if (cubes.Count == 0) break;

            // On détruit les cubes aléatoirement
            MathTools.Shuffle(cubes);

            // Sauf ceux qui sont proches de nous et du joueur, on les détruit en dernier !
            playerPos = gm.player.transform.position;
            cubes.Sort(delegate (Cube cubeA, Cube cubeB) {
                Vector3 A = cubeA.transform.position;
                Vector3 B = cubeB.transform.position;
                bool AinSeuil = Vector3.Distance(A, centerPos) <= seuilProximité || Vector3.Distance(A, playerPos) <= seuilProximité;
                bool BinSeuil = Vector3.Distance(B, centerPos) <= seuilProximité || Vector3.Distance(B, playerPos) <= seuilProximité;
                return AinSeuil.CompareTo(BinSeuil);
            });

            Vector3 barycentre = Vector3.zero;
            int nbCubesDestroyed = (int)Mathf.Min(nbCubesToDestroy, cubes.Count);
            for (int i = 0; i < nbCubesDestroyed; i++) {
                barycentre += cubes[i].transform.position;
                cubes[i].Explode();
            }

            barycentre /= nbCubesDestroyed;
            gm.soundManager.PlayCreateCubeClip(barycentre);
            yield return new WaitForSeconds(endGameFrameRate);
        }
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

        gm.soundManager.PlayDefeatClip();

        RememberGameResult(success: false);

        StartCoroutine(gm.QuitInSeconds(7));
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
        if (!ejectionTresholdUseLastCubePosition)
            return gm.gravityManager.GetHigh(gm.player.transform.position) < ejectionTreshold;
        else {
            float lowest = GetLessHighCubeAltitude();
            return gm.gravityManager.GetHigh(gm.player.transform.position) < lowest + ejectionTreshold;
        }
    }

    protected float GetLessHighCubeAltitude()
    {
        List<Cube> cubes = gm.map.GetAllCubes();
        if (!cubes.Any())
            return 0;
        float lowest = gm.gravityManager.GetHigh(cubes[0].transform.position);
        foreach(Cube cube in cubes) {
            float altitude = gm.gravityManager.GetHigh(cube.transform.position);
            lowest = Mathf.Min(lowest, altitude);
        }
        return lowest;
    }

    public bool PartieTermine() {
		// Si le joueur est tombé du cube ...
		if (IsPlayerEjected()) {
            //console.JoueurEjecte();
            LoseGame(EventManager.DeathReason.FALL_OUT);
			//// Si le joueur a perdu ...
			//if (map.GetLumieres().Count > 0) {
                ////console.JoueurEjecte();
                //LoseGame(EventManager.DeathReason.FALL_OUT);
			//// Si le joueur a gagné !
			//} else {
			//	gm.console.WinGame();
			//}
			return true;
		}

		// Ou qu'il est en contact avec un ennemiPrefabs depuis plus de 5 secondes
		// C'est donc qu'il s'est fait conincé !
		// Debug.Log("lastnotcontact = "+ player.GetComponent<Personnage>().lastNotContactEnnemy);
		if (Time.timeSinceLevelLoad - gm.player.GetComponent<Player>().lastNotContactEnnemy >= 5f) {
            LoseGame(EventManager.DeathReason.CAPTURED);
			//gm.console.LoseGame(EventManager.DeathReason.CAPTURED);
			//gm.player.vitesseDeplacement = 0; // On immobilise le joueur
			//gm.player.vitesseSaut = 0; // On immobilise le joueur
			return true;
		}
		return false;
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