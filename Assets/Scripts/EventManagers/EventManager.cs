using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Le but de la DataBase est de gérer le comportement de tout ce qui entrave le joueur.
// Cela va de la coordination des Drones, à la génération d'évenements néffastes.
public class EventManager : MonoBehaviour {

    public float endGameDuration = 20.0f;
    public float endGameFrameRate = 0.2f;

    protected GameManager gm;
	protected MapManager map;
    protected Coroutine coroutineDeathCubesCreation;
    protected bool isEndGameStarted = false;
    protected List<Cube> deathCubes;

    public void Initialize() {
		// Initialisation
		name = "EventManager";
        gm = FindObjectOfType<GameManager>();
		map = GameObject.Find("MapManager").GetComponent<MapManager>();
    }

    public virtual void OnLumiereCaptured(Lumiere.LumiereType type) {
        if (type == Lumiere.LumiereType.NORMAL) {
            int nbLumieres = map.lumieres.Count;
            if (nbLumieres == 0 && !isEndGameStarted) {
                gm.soundManager.PlayEndGameMusic();
                StartEndGame();
            }
        } else if (type == Lumiere.LumiereType.FINAL) {
            Debug.Log("WIIIIIIIIIIINNNNNNNNNNNN !!!!!!!!");
            gm.eventManager.WinGame();
        }
    }

    protected void StartEndGame() {
        isEndGameStarted = true;

        // On crée la finaleLight
        float minRadius = 0.5f + Mathf.Sqrt(3) / 2.0f; // La demi taille de la sphère + la demi-diagonale d'un cube
        Vector3 posLumiere = map.GetFreeSphereLocation(minRadius);

        // On évite que la lumière soit trop loin, car sinon on peut insta-die !
        while(Vector3.Distance(gm.player.transform.position, posLumiere) >= gm.map.tailleMap * 0.9f) {
            posLumiere = map.GetFreeSphereLocation(minRadius);
        }
        Lumiere finalLight = map.CreateLumiere(posLumiere, Lumiere.LumiereType.FINAL); // Attention à la position qui est arrondi ici !

        gm.player.FreezeLocalisation();

        gm.console.StartEndGame();

        // On lance la création des blocks de la mort !
        coroutineDeathCubesCreation = StartCoroutine(FillMapWithDeathCubes(finalLight.transform.position));
    }

    protected IEnumerator FillMapWithDeathCubes(Vector3 centerPos) {
        List<Vector3> allEmptyPositions = map.GetAllEmptyPositions();

        allEmptyPositions.Sort(delegate (Vector3 A, Vector3 B) {
            float distToA = Vector3.Distance(A, centerPos);
            float distToB = Vector3.Distance(B, centerPos);
            return distToB.CompareTo(distToA);
        });

        float nbTimings = endGameDuration / endGameFrameRate;
        int nbCubesToDestroy = (int)(allEmptyPositions.Count / nbTimings);
        AudioSource source = new GameObject().AddComponent<AudioSource>();
        source.spatialBlend = 1.0f;
        deathCubes = new List<Cube>();
        for(int i = 0; i < allEmptyPositions.Count; i+= nbCubesToDestroy) {
            Vector3 barycentre = Vector3.zero;
            for(int j = i; j < (int)Mathf.Min(i + nbCubesToDestroy, allEmptyPositions.Count); j++) {
                Cube cube = map.AddCube(allEmptyPositions[j], Cube.CubeType.DEATH);
                deathCubes.Add(cube);
                barycentre += allEmptyPositions[j];
            }
            barycentre /= nbCubesToDestroy;
            source.transform.position = barycentre;
            gm.soundManager.PlayCreateCubeClip(source);
            yield return new WaitForSeconds(endGameFrameRate);
        }
    }

    public void LoseGame() {
        StopCoroutine(coroutineDeathCubesCreation);

        gm.timeFreezed = true;
        gm.player.pouvoir.FreezePouvoir();

        gm.console.JoueurCapture();

        StartCoroutine(gm.QuitInSeconds(7));
    }

    public void WinGame() {
        StopCoroutine(coroutineDeathCubesCreation);

        gm.timeFreezed = true;
        gm.player.pouvoir.FreezePouvoir();

        gm.console.JoueurEchappe();

        StartCoroutine(gm.QuitInSeconds(7));
    }
}
