using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Le but de la DataBase est de gérer le comportement de tout ce qui entrave le joueur.
// Cela va de la coordination des Drones, à la génération d'évenements néffastes.
public class EventManager : MonoBehaviour {

    public GameObject finalLightPrefab;
    public float endGameDuration = 20.0f;
    public float endGameFrameRate = 0.2f;

    protected GameManager gm;
	protected MapManager map;
    protected List<Coroutine> coroutinesInProgress;
    protected bool isEndGameStarted = false;

    public void Initialize() {
		// Initialisation
		name = "EventManager";
        gm = FindObjectOfType<GameManager>();
		map = GameObject.Find("MapManager").GetComponent<MapManager>();
        coroutinesInProgress = new List<Coroutine>();
    }

    public void OnLumiereCaptured() {
        int nbLumieres = map.lumieres.Count;
        if(nbLumieres == 0 && !isEndGameStarted) {
            StartEndGame();
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
        Lumiere finalLight = Instantiate(finalLightPrefab, posLumiere, Quaternion.identity).GetComponent<Lumiere>();

        gm.player.FreezeLocalisation();

        // On lance la création des blocks de la mort !
        Coroutine coroutine = StartCoroutine(FillMapWithDeathCubes(finalLight.transform.position));
        coroutinesInProgress.Add(coroutine);
    }

    protected IEnumerator FillMapWithDeathCubes(Vector3 centerPos) {
        List<Vector3> allEmptyPositions = map.GetAllEmptyPositions();

        allEmptyPositions.Sort(delegate (Vector3 A, Vector3 B) {
            float distToA = Vector3.Distance(A, centerPos);
            float distToB = Vector3.Distance(B, centerPos);
            return distToB.CompareTo(distToA);
        });

        gm.soundManager.PlayEndGameMusic();

        float nbTimings = endGameDuration / endGameFrameRate;
        int nbCubesToDestroy = (int)(allEmptyPositions.Count / nbTimings);
        AudioSource source = new GameObject().AddComponent<AudioSource>();
        source.spatialBlend = 1.0f;
        for(int i = 0; i < allEmptyPositions.Count; i+= nbCubesToDestroy) {
            Vector3 barycentre = Vector3.zero;
            for(int j = i; j < (int)Mathf.Min(i + nbCubesToDestroy, allEmptyPositions.Count); j++) {
                Cube cube = map.AddCube(allEmptyPositions[j], Cube.CubeType.DEATH);
                barycentre += allEmptyPositions[j];
            }
            barycentre /= nbCubesToDestroy;
            source.transform.position = barycentre;
            gm.soundManager.PlayCreateCubeClip(source);
            yield return new WaitForSeconds(endGameFrameRate);
        }
    }

    public void LoseGame() {
        foreach(Coroutine coroutine in coroutinesInProgress) {
            StopCoroutine(coroutine);
        }

        gm.timeFreezed = true;
        gm.player.pouvoir.FreezePouvoir();

        gm.console.JoueurCapture();

        StartCoroutine(gm.QuitInSeconds(7));
    }

    public void WinGame() {
        foreach(Coroutine coroutine in coroutinesInProgress) {
            StopCoroutine(coroutine);
        }

        gm.timeFreezed = true;
        gm.player.pouvoir.FreezePouvoir();

        gm.console.JoueurEchappe();

        StartCoroutine(gm.QuitInSeconds(7));
    }


    //void Update () {
    //       MajEtatDataBase();
    //       DetecterJoueurSuivi();
    //}

    //   // Cette fonction permet de déterminer l'état de la dataBase, et donc le comportement des drones.
    //   // Tant qu'il reste des lumières, les sondes sont dans l'état WAITING ou TRACKING
    //   // Quand il ne reste plus de lumières, les sondes passent dans l'état RUSHING ou DEFENDING
    //   void MajEtatDataBase() {
    //       // On regarde si il reste des lumières
    //       // Si il n'en reste plus, on passe en état de défense !
    //       mapManager = GameObject.Find("MapManager").GetComponent<MapManager>();
    //       if (mapManager.lumieres.Count <= 0) {
    //           // Si on vient juste de perdre toutes les lumières ...
    //		if (!plusDeLumieres) {
    //               SetUpDefense();                
    //		}

    //           plusDeLumieres = true;
    //           etat = EtatDataBase.DEFENDING;
    //           timingPlusDeLumieres = Time.timeSinceLevelLoad;


    //       // Si il en reste, on est dans l'état normal
    //	} else {
    //           plusDeLumieres = false;
    //           etat = EtatDataBase.NORMAL;
    //       }
    //   }

    //   // Crée les sondes manquantes et associe à chaque sonde son point de la grille de défense
    //   void SetUpDefense() {
    //       int nbSondes = sondes.Count;
    //       int nextSquare = (int) Mathf.Pow(Mathf.Ceil(Mathf.Sqrt(nbSondes)), 2); // On arrondi au carré supérieur
    //       if(nextSquare == 1) {
    //           nextSquare = 4;
    //       }
    //       int longueurGrille = (int)Mathf.Sqrt(nextSquare); // La grille est carrée
    //       int tailleMap = mapManager.tailleMap - 2; // On peut faire -2 pour ne pas être vraiment sur les bords
    //       float distanceDetection = (float)tailleMap / (float)longueurGrille;

    //       // On crée la grille
    //       List<Vector3> positionsGrille = new List<Vector3>();
    //       for(int i = 0; i < longueurGrille; i++) {
    //           for(int j = 0; j < longueurGrille; j++) {
    //               float ratioX = (float)i / ((float)longueurGrille - 1f);
    //               float ratioZ = (float)j / ((float)longueurGrille - 1f);
    //               Debug.Log("ratio = " + ratioX + " + " + ratioZ);
    //               Vector3 pos = new Vector3(ratioX * (float)tailleMap + 1f, (float)tailleMap + 2f, ratioZ * (float)tailleMap + 1f);
    //               positionsGrille.Add(pos);
    //           }
    //       }

    //       // On associe à toutes les sondes déjà existantes une position
    //       for (int i = 0; i < nbSondes; i++) {
    //           Vector3 pos = positionsGrille[Random.Range(0, positionsGrille.Count)];
    //           positionsGrille.Remove(pos);
    //           sondes[i].positionGrilleDefense = pos;
    //           Debug.Log("position associée = " + pos);
    //       }

    //       // On crée autant de sondes qu'il en manque pour arriver au compte =)
    //       int nbsondesFinal = longueurGrille * longueurGrille;
    //       for(int i = nbSondes; i < nbsondesFinal; i++) {
    //           // On lui choisit une position au hasard
    //           Vector3 pos = positionsGrille[Random.Range(0, positionsGrille.Count)];
    //           positionsGrille.Remove(pos);
    //           GameObject go = Instantiate(ennemiPrefabs, pos, Quaternion.identity); // Et normalement la sonde s'ajoute toute seule à la liste :D
    //           go.GetComponent<Sonde>().positionGrilleDefense = pos; // Et on lui set sa position de la grille de défense
    //           Debug.Log("position associée à une nouvelle sonde = " + pos);
    //       }

    //       // Puis on met à jour le coef de detection de toutes les sondes
    //       foreach(Sonde sonde in sondes) {
    //           float coefDistanceDetection = (float)distanceDetection / (float)sonde.distanceDeDetection;
    //           Debug.Log("distanceTotale = " + coefDistanceDetection * (float)sonde.distanceDeDetection);
    //           sonde.coefficiantDeRushDistanceDeDetection = coefDistanceDetection;
    //       }
    //   }

    //   void DetecterJoueurSuivi() {
    //	// Si le joueur était visible avant mais qu'on le perd de vu, alors on le signal ! =)
    //	if (Time.timeSinceLevelLoad >= 10 && isJoueurSuivi && !JoueurSuivi()) {
    //		console.SemerSondes();
    //		isJoueurSuivi = false;
    //	}
    //	if (Time.timeSinceLevelLoad >= 10 && !isJoueurSuivi && JoueurSuivi()) {
    //		console.JoueurRepere();
    //		isJoueurSuivi = true;
    //	}
    //   }

    //public EtatDataBase DemanderOrdre() {
    //	return etat;
    //}

    //// Permet de savoir si le joueur est actuellement suivi
    //public bool JoueurSuivi() {
    //	bool suivi = false;
    //	foreach (Sonde drone in sondes) {
    //		if (drone.isMoving()) {
    //			suivi = true;
    //			break;
    //		}
    //	}
    //	return suivi;
    //}
}
