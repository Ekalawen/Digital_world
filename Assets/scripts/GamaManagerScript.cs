using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamaManagerScript : MonoBehaviour {

	public GameObject cube; // On récupère ce qu'est un cube !
	public GameObject personnage; // On récupère le personnage !
	public GameObject ennemi; // On récupère un ennemi !
	public GameObject objectif; // On récupère les objectifs !
	public GameObject console; // On récupère la console !
	public GameObject pointeur; // Pour avoir un visuel du centre de l'écran
	public GameObject dataBase; // Pour créer l'IA chargé de supervisé les ennemis !
	public int tailleMap;
	public float proportionCaves;
	public float proportionEnnemis;


	private List<GameObject> cubes = new List<GameObject>();
	private Vector3[] pos;
	[HideInInspector]
	public int nbLumieres;
	[HideInInspector]
	public int nbEnnemis;

	[HideInInspector]
	public bool partieDejaTerminee = false;
	private bool lumieresAttrapees = false;

	// Use this for initialization
	void Start () {
		// On initialise la position des coins !
		pos = new Vector3[8];
		pos [0] = new Vector3 (0, 0, 0);
		pos [1] = new Vector3 (tailleMap, 0, 0);
		pos [2] = new Vector3 (tailleMap, tailleMap, 0);
		pos [3] = new Vector3 (0, tailleMap, 0);
		pos [4] = new Vector3 (0, 0, tailleMap);
		pos [5] = new Vector3 (tailleMap, 0, tailleMap);
		pos [6] = new Vector3 (tailleMap, tailleMap, tailleMap);
		pos [7] = new Vector3 (0, tailleMap, tailleMap);

		// On crée la console et le pointeur !
		Instantiate (pointeur);
		Instantiate (console);

		// On crée la database
		Instantiate (dataBase);

		// On veut créer les faces du cube !
		remplirFace(0, 1, 2, 3); // coté 1
		remplirFace(4, 5, 6, 7); // coté 2
		remplirFace(0, 4, 7, 3); // coté 3
		remplirFace(0, 1, 5, 4); // sol !
		remplirFace(1, 2, 6, 5); // coté 4
		//remplirFace(2, 3, 7, 6); // plafond !

		// On veut créer des passerelles entre les sources ! <3
		List<CubeScript> sources = new List<CubeScript>();
		int nbSources = 0;
		foreach (GameObject go in cubes) {
			CubeScript c = go.GetComponent<CubeScript> () as CubeScript;				
			if (c.type == CubeScript.CubeType.Source) {
				sources.Add (c);
				nbSources++;
			}
		}
		generateBridges (sources, (int) Mathf.Floor(nbSources / 2));

		// On veut générer des caves dangeureuses :3
		// Et à l'intérieur on crée des lumières ! :D
		nbLumieres = 0;
		int tailleMaxCave = (int) Mathf.Min((int) tailleMap / 2, 10);
		int volumeCave = (int) Mathf.Pow (tailleMaxCave, 3);
		int volumeMap = (int) Mathf.Pow (tailleMap, 3);
		int nbCaves = (int) Mathf.Ceil (proportionCaves * volumeMap / volumeCave);
		generateCave(nbCaves, 3, tailleMaxCave);

		// On veut ajouter un personnage dans le cube central !
		// On arrive en hauteur comme ça on a le temps de bien voir la carte ! =)
		Vector3 posPerso = new Vector3(tailleMap / 2, tailleMap + 50, tailleMap / 2);
		GameObject perso = Instantiate (personnage, posPerso, Quaternion.identity) as GameObject;
		perso.name = "Joueur";

		// On veut maintenant activer la caméra du personnage !
		Camera camPerso = perso.transform.GetChild(0).GetComponent<Camera>() as Camera;
		camPerso.enabled = true;

		// On veut ajouter des ennemis !
		nbEnnemis = (int) Mathf.Ceil(volumeMap * proportionEnnemis);
		for (int i = 0; i < nbEnnemis; i++) {
			Vector3 posEnnemi = new Vector3 (Random.Range (1f, tailleMap - 1f), Random.Range (1f, tailleMap - 1f), Random.Range (1f, tailleMap - 1f)); 
			Instantiate (ennemi, posEnnemi, Quaternion.identity);
		}
	}

	void remplirFace(int indVertx1, int indVertx2, int indVertx3, int indVertx4) {
		Vector3 depart = pos [indVertx1];
		//Vector3 arrivee = pos [indVertx4];
		Vector3 pas1 = (pos [indVertx2] - depart) / tailleMap;
		Vector3 pas2 = (pos [indVertx4] - depart) / tailleMap;

		for (int i = 0; i <= tailleMap; i++) {
			for (int j = 0; j <= tailleMap; j++) {
				Vector3 actuel = depart + pas1 * i + pas2 * j;
				GameObject instance = Instantiate (cube, actuel, Quaternion.identity) as GameObject;

				// On va un peu décaler les cubes pour créer du relief !
				//float decalageMax = personnage.GetComponent<CharacterController>().stepOffset / 2;
				float decalageMax = 0.1f;
				Vector3 directionDecalage = Vector3.Cross (pas1, pas2);
				directionDecalage.Normalize ();
				//instance.transform.Translate (directionDecalage * Random.Range (-decalageMax, decalageMax));

				cubes.Add (instance);
			}
		}
	}

	// Génère des ponts entre les sources ! =)
	void generateBridges(List<CubeScript> sources, int nbBridges) {
		for (int i = 0; i < nbBridges; i++) {
			// On récupère les deux sources qui nous intéressent
			int n = Random.Range (0, sources.Count);
			CubeScript c1 =	sources[n];
			sources.Remove (c1);
			n = Random.Range (0, sources.Count);
			CubeScript c2 =	sources[n];
			sources.Remove (c2);

			// Si le pont n'est pas dans un mur ...
			if (!bridgeInWall(c1.transform.position, c2.transform.position)) {
				// On va les relier par une ligne droite !
				float distance = Vector3.Distance(c1.transform.position, c2.transform.position);
				Vector3 pas = (c2.transform.position - c1.transform.position) / distance;
				Vector3 pos;
				GameObject instance;
				for (int k = 1; k <= Mathf.Floor (distance); k++) {
					// On crée un cube !
					pos = c1.transform.position + pas * k;
					instance = Instantiate (cube, pos, Quaternion.identity) as GameObject;
				}
				// Et on rajoute le dernier cube
				pos = c1.transform.position + pas * distance;
				instance = Instantiate (cube, pos, Quaternion.identity) as GameObject;
			}
		}
	}

	// Permet de savoir si un pont est dans un mur ou non
	bool bridgeInWall(Vector3 debut, Vector3 fin) {
		if((debut.x == 0 && fin.x == 0)
			|| (debut.x == tailleMap && fin.x == tailleMap)
			|| (debut.y == 0 && fin.y == 0)
			|| (debut.y == tailleMap && fin.y == tailleMap)
			|| (debut.z == 0 && fin.z == 0)
			|| (debut.z == tailleMap && fin.z == tailleMap)) {
			return true;
		} else {
			return false;
		}
	}

	void generateCave(int nbCaves, int tailleMinCave, int tailleMaxCave) {
		for (int k = 0; k < nbCaves; k++) {
			// On définit la taille de la cave
			int sizeX = Random.Range (tailleMinCave, tailleMaxCave + 1);
			int sizeY = Random.Range (tailleMinCave, tailleMaxCave + 1);
			int sizeZ = Random.Range (tailleMinCave, tailleMaxCave + 1);

			// On définit sa position sur la carte
			Vector3 position = new Vector3(Random.Range(1, tailleMap - sizeX),
				Random.Range(1, tailleMap - sizeY),
				Random.Range(1, tailleMap - sizeZ));

			// On veut détruire tous les cubes et lumières qui se trouvent dans notre cave !
			Collider[] colliders;
			Vector3 center = position + (sizeX / 2) * Vector3.right + (sizeY / 2) * Vector3.up + (sizeZ / 2) * Vector3.forward;
			Vector3 halfSize = new Vector3 (sizeX / 2, sizeY / 2, sizeZ / 2);
			if ((colliders = Physics.OverlapBox (center, halfSize)).Length > 0) {
				foreach (Collider collider in colliders) {
					if (collider.tag == "Cube" || collider.tag == "Objectif") {
						Destroy (collider.gameObject);
						if (collider.tag == "Objectif") {
							nbLumieres--;
						}
					}
				}
			}

			// On crée la matrice d'identification des nouveaux blocs
			// 1 signifie qu'il y a un bloc, 0 signifie qu'il n'y en a pas !
			// De base c'est une matrice pleine de blocs
			int[,,] cave = new int[sizeX, sizeY, sizeZ];
			for (int i = 0; i < sizeX; i++) {
				for (int j = 0; j < sizeY; j++) {
					for (int l = 0; l < sizeZ; l++) {
						cave [i, j, l] = 1;
					}
				}
			}
			// On va choisir les entrées, une pour chaque coté !
			List<Vector3> entrees = new List<Vector3>();
			entrees.Add (new Vector3(0, Random.Range(0, sizeY), Random.Range(0, sizeZ)));
			entrees.Add (new Vector3(sizeX-1, Random.Range(0, sizeY), Random.Range(0, sizeZ)));
			entrees.Add (new Vector3(Random.Range(0, sizeX), 0, Random.Range(0, sizeZ)));
			entrees.Add (new Vector3(Random.Range(0, sizeX), sizeY-1, Random.Range(0, sizeZ)));
			entrees.Add (new Vector3(Random.Range(0, sizeX), Random.Range(0, sizeY), 0));
			entrees.Add (new Vector3(Random.Range(0, sizeX), Random.Range(0, sizeY), sizeZ-1));

			// On va choisir les points de passages internes
			int nbPointsDePassage = Random.Range(1, (int) Mathf.Ceil(sizeX*sizeY*sizeZ / 15));
			List<Vector3> pointsDePassage = new List<Vector3>();
			for (int i = 0; i < nbPointsDePassage; i++) {
				pointsDePassage.Add (new Vector3 (Random.Range (1, sizeX - 1), Random.Range (1, sizeY - 1), Random.Range (1, sizeZ - 1)));
			}

			// Et maintenant on va chercher à relier tout ces points !
			List<Vector3> ptsCibles = entrees;
			ptsCibles.AddRange (pointsDePassage);

			// On enlève ça pour éviter le problème des trous unique dans les caves :'(
			/*// On creuse dans tout ces points
			for (int i = 0; i < ptsCibles.Count; i++) {
				cave [(int) ptsCibles [i].x, (int) ptsCibles [i].y, (int) ptsCibles [i].z] = 0;
			}*/

			// On part d'un point, on va creuser pour atteindre un autre point, et on va continuer tant qu'on a pas atteint tous les points !
			List<Vector3> ptsAtteints = new List<Vector3>();
			Vector3 depart = ptsCibles [Random.Range (0, ptsCibles.Count)]; // on rajoute le premier point
			ptsAtteints.Add (depart);
			ptsCibles.Remove (depart);
			while (ptsCibles.Count > 0) {
				Vector3 debutChemin = ptsAtteints [Random.Range (0, ptsAtteints.Count)];
				Vector3 finChemin = ptsCibles [Random.Range (0, ptsCibles.Count)];
				cave = relierChemin (debutChemin, finChemin, cave);
				ptsCibles.Remove (finChemin);
				// La prochaine ligne est-elle facultative ?
				ptsAtteints.Add (finChemin);
			}

			// On cherche une case où créer un objectif !
			Vector3 posObjectif = new Vector3(Random.Range(0, sizeX), Random.Range(0, sizeY), Random.Range(0, sizeZ));
			while (cave [(int)posObjectif.x, (int)posObjectif.y, (int)posObjectif.z] == 1) {
				posObjectif = new Vector3(Random.Range(0, sizeX), Random.Range(0, sizeY), Random.Range(0, sizeZ));
			}
			posObjectif += position;
			nbLumieres++;
			Instantiate (objectif, posObjectif, Quaternion.identity);

			/*// On cherche une case creuse qui serait entièrement entouré de pleines !
			// Il faut qu'il n'y en ait pas !!! x)
			for (int i = 1; i < sizeX - 1; i++) {
				for (int j = 1; j < sizeY - 1; j++) {
					for (int l = 1; l < sizeZ - 1; l++) {
						if (cave [i, j, l] == 0
						   && cave [i + 1, j, l] == 1
						   && cave [i - 1, j, l] == 1
						   && cave [i, j + 1, l] == 1
						   && cave [i, j - 1, l] == 1
						   && cave [i, j, l + 1] == 1
						   && cave [i, j, l - 1] == 1) {
							Debug.Log ("Il y a un trou unique !");
						}
					}
				}
			}*/

			// On instancie les cubes de la cave !
			for (int i = 0; i < sizeX; i++) {
				for (int j = 0; j < sizeY; j++) {
					for (int l = 0; l < sizeZ; l++) {
						if (cave [i, j, l] == 1) {
							Vector3 pos = position + i * Vector3.right + j * Vector3.up + l * Vector3.forward;
							Instantiate (cube, pos, Quaternion.identity);
						}
					}
				}
			}
		}
	}

	// Le but de cette fonction est de renvoyer la cave en ayant creusé un tunel allant de debutChemin a finChemin !
	int[,,] relierChemin(Vector3 debutChemin, Vector3 finChemin, int[,,] cave) {
		Vector3 pointsActuel = debutChemin;
		while (pointsActuel != finChemin) {
			// On creuse
			cave[(int) pointsActuel.x, (int) pointsActuel.y, (int) pointsActuel.z] = 0;

			// On liste les bonnes directions à prendre
			List<Vector3> directions = new List<Vector3>();
			if (pointsActuel.x != finChemin.x) {
				if (pointsActuel.x < finChemin.x) {
					directions.Add (new Vector3 (pointsActuel.x + 1, pointsActuel.y, pointsActuel.z));
				} else {
					directions.Add (new Vector3 (pointsActuel.x - 1, pointsActuel.y, pointsActuel.z));
				}
			}
			if (pointsActuel.y != finChemin.y) {
				if (pointsActuel.y < finChemin.y) {
					directions.Add (new Vector3 (pointsActuel.x, pointsActuel.y + 1, pointsActuel.z));
				} else {
					directions.Add (new Vector3 (pointsActuel.x, pointsActuel.y - 1, pointsActuel.z));
				}
			}
			if (pointsActuel.z != finChemin.z) {
				if (pointsActuel.z < finChemin.z) {
					directions.Add (new Vector3 (pointsActuel.x, pointsActuel.y, pointsActuel.z + 1));
				} else {
					directions.Add (new Vector3 (pointsActuel.x, pointsActuel.y, pointsActuel.z - 1));
				}
			}

			// On se déplace dans une bonne direction aléatoirement
			pointsActuel = directions[Random.Range(0, directions.Count)];
		}
		cave[(int) pointsActuel.x, (int) pointsActuel.y, (int) pointsActuel.z] = 0;
		return cave;
	}

	// Update is called once per frame
	void Update () {
		// Si on a appuyé sur la touche Escape, on quitte le jeu !
		if (Input.GetKey ("escape")) {
			QuitGame ();
		}

		// Si on a attrapé toutes les lumières
		ConsoleScript cs = GameObject.Find ("Console").GetComponent<ConsoleScript> ();
		if (!lumieresAttrapees && nbLumieres <= 0) {
			lumieresAttrapees = true;
			// On affiche un message dans la console !
			cs.ajouterMessage ("Vous ne vous en sortirez pas comme ça !", ConsoleScript.TypeText.ENNEMI_TEXT);
			cs.ajouterMessage ("OK Super maintenant faut sortir d'ici !", ConsoleScript.TypeText.ALLY_TEXT);
		}

		// Si c'est la fin de la partie, on quitte après 5 secondes !
		if (!partieDejaTerminee && partieTerminee ()) {
			partieDejaTerminee = true;
			StartCoroutine (QuitInSeconds(7));
		}
	}

	bool partieTerminee() {
		ConsoleScript cs = GameObject.Find ("Console").GetComponent<ConsoleScript> ();
		GameObject player = GameObject.Find ("Joueur");

		// Si le joueur est tombé du cube ...
		if (player.transform.position.y < -10) {
			// Si le joueur a perdu ...
			if (!lumieresAttrapees) {
			// Si le joueur a gagné !
				cs.ajouterMessageImportant ("MENACE ÉJECTÉE !", ConsoleScript.TypeText.ENNEMI_TEXT, 5);
				cs.ajouterMessage ("Désactivation du processus défensif ...", ConsoleScript.TypeText.ENNEMI_TEXT);
				cs.ajouterMessage ("Vous avez été éjecté de la Matrix. ", ConsoleScript.TypeText.BASIC_TEXT);
			} else {
				cs.ajouterMessage ("NOOOOOOOOOOOOOOOOOON ...", ConsoleScript.TypeText.ENNEMI_TEXT);
				cs.ajouterMessage ("J'ai échouée ...", ConsoleScript.TypeText.ENNEMI_TEXT);
				cs.ajouterMessageImportant ("NOUS AVONS RÉUSSI !!!", ConsoleScript.TypeText.ALLY_TEXT, 5);
				StartCoroutine (recompenser (cs));
			}
			return true;
		}

		// Ou qu'il est en contact avec un ennemi depuis plus de 5 secondes
		// C'est donc qu'il s'est fait conincé !
		if (Time.time - player.GetComponent<personnageScript> ().lastNotContactEnnemy >= 5f) {
			cs.ajouterMessageImportant ("MENACE ÉLIMINÉ !", ConsoleScript.TypeText.ENNEMI_TEXT, 5);
			StartCoroutine (seMoquer (cs));
			return true;
		}

		return false;
	}

	IEnumerator QuitInSeconds(int tps) {
		yield return new WaitForSeconds (tps);
		QuitGame ();
	}

	IEnumerator recompenser(ConsoleScript cs) {
		yield return new WaitForSeconds (2);
		string message;
		while (true) {
			message = "";
			for (int i = 0; i < Random.Range (0, 6); i++)
				message += " ";
			message += "On a réussi YES";
			for (int i = 0; i < Random.Range (3, 12); i++)
				message += "S";
			message += " ";
			for (int i = 0; i < Random.Range (1, 6); i++)
				message += "!";
			cs.ajouterMessage (message, ConsoleScript.TypeText.ALLY_TEXT);
			yield return null;
		}
	}

	IEnumerator seMoquer(ConsoleScript cs) {
		yield return new WaitForSeconds (2);
		string message;
		while (true) {
			message = "";
			for (int i = 0; i < Random.Range (0, 6); i++)
				message += " ";
			message += "Nous vous avons attrapé ... MOU";
			for (int i = 0; i < Random.Range (1, 6); i++)
				message += "HA";
			message += " ";
			for (int i = 0; i < Random.Range (1, 6); i++)
				message += "!";
			cs.ajouterMessage (message, ConsoleScript.TypeText.ENNEMI_TEXT);
			yield return null;
		}
	}

	public void QuitGame()
	{
		// save any game data here
		#if UNITY_EDITOR
		// Application.Quit() does not work in the editor so
		// UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
		UnityEditor.EditorApplication.isPlaying = false;
		#else
		Application.Quit();
		#endif
	}
}
