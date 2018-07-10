using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Le but de la DataBase est de gérer le comportement de tout ce qui entrave le joueur.
// Cela va de la coordination des Drones, à la génération d'évenements néffastes.
public class DataBaseScript : MonoBehaviour {

    /// Reference to this script
    /// See http://clearcutgames.net/home/?p=437 for singleton pattern.
    // Returns _instance if it exists, otherwise create one and set it has current _instance
    static DataBaseScript _instance;
    public static DataBaseScript Instance { get { return _instance ?? (_instance = new GameObject().AddComponent<DataBaseScript>()); } }

	public enum EtatDataBase {NORMAL, DEFENDING};

	//////////////////////////////////////////////////////////////////////////////////////
	// ATTRIBUTS PUBLIQUES
	//////////////////////////////////////////////////////////////////////////////////////

	public GameObject ennemiPrefabs; // On récupère un ennemi !

	//////////////////////////////////////////////////////////////////////////////////////
	// ATTRIBUTS PRIVÉES
	//////////////////////////////////////////////////////////////////////////////////////

	[HideInInspector]
	public List<EnnemiScript> sondes; // Elle connait tous les drones !
	[HideInInspector]
	public EnnemiScript.EtatEnnemi etatDrones; // Permet de donner des ordres aux drones
	[HideInInspector]
	public GameObject player; // Le joueur
	[HideInInspector]
	public ConsoleScript console; // La console
	[HideInInspector]
	public MapManagerScript mapManager; // la map
	private bool plusDeLumieres;
    private EtatDataBase etat; // L'état de la base de données
	private float timingPlusDeLumieres;
	private bool isJoueurSuivi; // Permet de savoir si le joueur est suivi

	//////////////////////////////////////////////////////////////////////////////////////
	// ATTRIBUTS PRIVÉES
	//////////////////////////////////////////////////////////////////////////////////////

    void Awake() {
        if (!_instance) { _instance = this; }
    }

	void Start () {
		// Initialisation
		name = "DataBase";
        sondes = new List<EnnemiScript>(); // Les sondes viennent se renseigner tout seuls =)
		player = GameObject.Find ("Joueur");
		mapManager = GameObject.Find("MapManager").GetComponent<MapManagerScript>();
		plusDeLumieres = false;
        etat = EtatDataBase.NORMAL;
		isJoueurSuivi = false;
	}
	
	void Update () {
        majEtatDataBase();
        detecterJoueurSuivi();
	}

    // Cette fonction permet de déterminer l'état de la dataBase, et donc le comportement des drones.
    // Tant qu'il reste des lumières, les sondes sont dans l'état WAITING ou TRACKING
    // Quand il ne reste plus de lumières, les sondes passent dans l'état RUSHING ou DEFENDING
    void majEtatDataBase() {
		// On regarde si il reste des lumières
        // Si il n'en reste plus, on passe en état de défense !
		if (mapManager.nbLumieres <= 0) {
            // Si on vient juste de perdre toutes les lumières ...
			if (!plusDeLumieres) {
                setUpDefense();                
			}

            plusDeLumieres = true;
            etat = EtatDataBase.DEFENDING;
            timingPlusDeLumieres = Time.timeSinceLevelLoad;


        // Si il en reste, on est dans l'état normal
		} else {
            plusDeLumieres = false;
            etat = EtatDataBase.NORMAL;
        }
    }

    // Crée les sondes manquantes et associe à chaque sonde son point de la grille de défense
    void setUpDefense() {
        int nbSondes = sondes.Count;
        int nextSquare = (int) Mathf.Pow(Mathf.Ceil(Mathf.Sqrt(nbSondes)), 2); // On arrondi au carré supérieur
        if(nextSquare == 1) {
            nextSquare = 4;
        }
        int longueurGrille = (int)Mathf.Sqrt(nextSquare); // La grille est carrée
        int tailleMap = mapManager.tailleMap - 2; // On peut faire -2 pour ne pas être vraiment sur les bords
        float distanceDetection = (float)tailleMap / (float)longueurGrille;

        // On crée la grille
        List<Vector3> positionsGrille = new List<Vector3>();
        for(int i = 0; i < longueurGrille; i++) {
            for(int j = 0; j < longueurGrille; j++) {
                float ratioX = (float)i / ((float)longueurGrille - 1f);
                float ratioZ = (float)j / ((float)longueurGrille - 1f);
                Debug.Log("ratio = " + ratioX + " + " + ratioZ);
                Vector3 pos = new Vector3(ratioX * (float)tailleMap + 1f, (float)tailleMap + 2f, ratioZ * (float)tailleMap + 1f);
                positionsGrille.Add(pos);
            }
        }

        // On associe à toutes les sondes déjà existantes une position
        for (int i = 0; i < nbSondes; i++) {
            Vector3 pos = positionsGrille[Random.Range(0, positionsGrille.Count)];
            positionsGrille.Remove(pos);
            sondes[i].positionGrilleDefense = pos;
            Debug.Log("position associée = " + pos);
        }

        // On crée autant de sondes qu'il en manque pour arriver au compte =)
        int nbsondesFinal = longueurGrille * longueurGrille;
        for(int i = nbSondes; i < nbsondesFinal; i++) {
            // On lui choisit une position au hasard
            Vector3 pos = positionsGrille[Random.Range(0, positionsGrille.Count)];
            positionsGrille.Remove(pos);
            GameObject go = Instantiate(ennemiPrefabs, pos, Quaternion.identity); // Et normalement la sonde s'ajoute toute seule à la liste :D
            go.GetComponent<EnnemiScript>().positionGrilleDefense = pos; // Et on lui set sa position de la grille de défense
            Debug.Log("position associée à une nouvelle sonde = " + pos);
        }

        // Puis on met à jour le coef de detection de toutes les sondes
        foreach(EnnemiScript sonde in sondes) {
            float coefDistanceDetection = (float)distanceDetection / (float)sonde.distanceDeDetection;
            Debug.Log("distanceTotale = " + coefDistanceDetection * (float)sonde.distanceDeDetection);
            sonde.coefficiantDeRushDistanceDeDetection = coefDistanceDetection;
        }
    }

    void detecterJoueurSuivi() {
		// Si le joueur était visible avant mais qu'on le perd de vu, alors on le signal ! =)
		if (Time.timeSinceLevelLoad >= 10 && isJoueurSuivi && !joueurSuivi()) {
			console.semerSondes();
			isJoueurSuivi = false;
		}
		if (Time.timeSinceLevelLoad >= 10 && !isJoueurSuivi && joueurSuivi()) {
			console.joueurRepere();
			isJoueurSuivi = true;
		}
    }

	public EtatDataBase demanderOrdre() {
		return etat;
	}

	// Permet de savoir si le joueur est actuellement suivi
	public bool joueurSuivi() {
		bool suivi = false;
		foreach (EnnemiScript drone in sondes) {
			if (drone.isMoving()) {
				suivi = true;
				break;
			}
		}
		return suivi;
	}
}
