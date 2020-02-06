using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Le but de la DataBase est de gérer le comportement de tout ce qui entrave le joueur.
// Cela va de la coordination des Drones, à la génération d'évenements néffastes.
public class EventManager : MonoBehaviour {

    public enum DeathReason { TIME_OUT, CAPTURED, FALL_OUT, TOUCHED_DEATH_CUBE };

    public float endGameDuration = 20.0f;
    public float endGameFrameRate = 0.2f;

    public List<GameObject> randomEventsPrefabs;

    protected GameManager gm;
    protected MapManager map;
    protected Coroutine coroutineDeathCubesCreation;
    protected bool isEndGameStarted = false;
    protected List<Cube> deathCubes;
    protected bool gameIsEnded = false;
    protected List<RandomEvent> randomEvents;
    protected GameObject randomEventsFolder;

    public void Initialize()
    {
        // Initialisation
        name = "EventManager";
        gm = FindObjectOfType<GameManager>();
        map = GameObject.Find("MapManager").GetComponent<MapManager>();
        randomEventsFolder = new GameObject("Events");

        randomEvents = new List<RandomEvent>();
        foreach (GameObject randomEventPrefab in randomEventsPrefabs)
        {
            RandomEvent randomEvent = Instantiate(randomEventPrefab, randomEventsFolder.transform).GetComponent<RandomEvent>();
            randomEvents.Add(randomEvent);
        }
    }

    public virtual void OnLumiereCaptured(Lumiere.LumiereType type)
    {
        if (type == Lumiere.LumiereType.NORMAL)
        {
            int nbLumieres = map.lumieres.Count;
            if (nbLumieres == 0 && !isEndGameStarted)
            {
                gm.soundManager.PlayEndGameMusic();
                StartEndGame();
            }
        }
        else if (type == Lumiere.LumiereType.FINAL)
        {
            Debug.Log("WIIIIIIIIIIINNNNNNNNNNNN !!!!!!!!");
            gm.eventManager.WinGame();
        }
    }

    protected virtual void StartEndGame()
    {
        isEndGameStarted = true;

        // On crée la finaleLight
        //float minRadius = 0.5f + Mathf.Sqrt(3) / 2.0f; // La demi taille de la sphère + la demi-diagonale d'un cube
        //Vector3 posLumiere = map.GetFreeSphereLocation(minRadius);
        Vector3 posLumiere = map.GetFreeRoundedLocation();

        // On évite que la lumière soit trop proche
        float moyenneTailleMap = (gm.map.tailleMap.x + gm.map.tailleMap.y + gm.map.tailleMap.z) / 3.0f;
        while (Vector3.Distance(gm.player.transform.position, posLumiere) <= moyenneTailleMap * 0.9f)
        {
            posLumiere = map.GetFreeRoundedLocation();
            moyenneTailleMap *= 0.95f; // Pour éviter qu'il n'y ait aucune zone atteignable x)
        }
        Lumiere finalLight = map.CreateLumiere(posLumiere, Lumiere.LumiereType.FINAL); // Attention à la position qui est arrondi ici !

        gm.player.FreezeLocalisation();

        gm.console.StartEndGame();

        // On lance la création des blocks de la mort !
        coroutineDeathCubesCreation = StartCoroutine(FillMapWithDeathCubes(finalLight.transform.position));
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

    public void LoseGame(DeathReason reason)
    {
        if (gameIsEnded)
            return;
        gameIsEnded = true;
        if (coroutineDeathCubesCreation != null)
            StopCoroutine(coroutineDeathCubesCreation);

        gm.timeFreezed = true;

        gm.player.FreezePouvoirs();

        gm.console.LoseGame(reason);

        StartCoroutine(gm.QuitInSeconds(7));
    }

    public void WinGame()
    {
        if (gameIsEnded)
            return;
        gameIsEnded = true;
        if (coroutineDeathCubesCreation != null)
            StopCoroutine(coroutineDeathCubesCreation);

        gm.timeFreezed = true;
        gm.player.FreezePouvoirs();

        gm.console.WinGame();

        // On retient que l'on a gagné ce niveau une fois de plus !
        string key = PlayerPrefs.GetString(MenuLevel.LEVEL_NAME_KEY) + "nbVictoires";
        int newValue = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) + 1 : 1;
        Debug.Log("key = " + key + " newValue = " + newValue);
        PlayerPrefs.SetInt(key, newValue);

        StartCoroutine(gm.QuitInSeconds(7));
    }

    public bool IsGameOver()
    {
        return gameIsEnded;
    }
}