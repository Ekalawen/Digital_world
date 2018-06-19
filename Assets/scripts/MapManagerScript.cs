using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManagerScript : MonoBehaviour {
	//////////////////////////////////////////////////////////////////////////////////////
	// ENUMERATION
	//////////////////////////////////////////////////////////////////////////////////////

	public enum TypeMap {CUBE_MAP, PLAINE_MAP, LABYRINTHE_MAP, GROUND_MAP, EMPTY_MAP, TUTORIAL_MAP};

	//////////////////////////////////////////////////////////////////////////////////////
	// ATTRIBUTS PUBLIQUES
	//////////////////////////////////////////////////////////////////////////////////////

	public GameObject cube; // On récupère ce qu'est un cube !
	public GameObject objectif; // On récupère les objectifs !
	public GameObject ennemiPrefabs; // On récupère un ennemi !
	public float frequenceSource; // La frequence qu'un cube soit une source
	public float distanceSourceMax; // La distance de coloration d'une source
	public TypeMap typeMap;
	public List<CubeScript.ThemeCube> themeCube;
	public int tailleMap;
	public float proportionCaves;
	public float proportionEnnemis; // La quantité d'ennemies relativement à la taille de la carte

	//////////////////////////////////////////////////////////////////////////////////////
	// ATTRIBUTS PRIVÉES
	//////////////////////////////////////////////////////////////////////////////////////

	private List<GameObject> cubes;
	[HideInInspector]
	public int nbLumieres;
	[HideInInspector]
	public bool lumieresAttrapees;
	[HideInInspector]
	public int volumeMap;
	[HideInInspector]
	public int nbEnnemis;

	//////////////////////////////////////////////////////////////////////////////////////
	// METHODES
	//////////////////////////////////////////////////////////////////////////////////////

	void Start() {
		// Initialisation
		name = "MapManager";
		cubes = new List<GameObject>();
		lumieresAttrapees = false;

		// On définit nos cubes
		CubeScript.probaSource = frequenceSource;
		CubeScript.distSourceMax = distanceSourceMax;

		// On charge le thème de la map
		CubeScript.theme = new List<CubeScript.ThemeCube>();
		foreach(CubeScript.ThemeCube theme in themeCube) {
			if(theme == CubeScript.ThemeCube.RANDOM) {
				System.Array values = System.Enum.GetValues(typeof(CubeScript.ThemeCube));
				int indiceTheme = Random.Range(0, values.Length - 1);
				CubeScript.theme.Add((CubeScript.ThemeCube) values.GetValue(indiceTheme));
			} else {
				CubeScript.theme.Add(theme);
			}
		}

		// On charge la bonne map
		volumeMap = (int) Mathf.Pow (tailleMap, 3);
		switch (typeMap)
		{
			case TypeMap.CUBE_MAP:
				generateCubeMap();
			break;
			case TypeMap.GROUND_MAP:
				generateGroundMap();
			break;
			case TypeMap.EMPTY_MAP:
				// Bah on fait rien :D
			break;
			case TypeMap.TUTORIAL_MAP:
				generateTutorialMap();
			break;
			default:
				Debug.Log("Je ne connais pas ce type de Map !");
			break;
		}

		// On veut ajouter des ennemis !
		generateEnnemies();
	}

	// Crée une map en forme de Cube
	void generateCubeMap() {
		// On initialise la position des coins !
		Vector3[] pos = new Vector3[8];
		pos [0] = new Vector3 (0, 0, 0);
		pos [1] = new Vector3 (tailleMap, 0, 0);
		pos [2] = new Vector3 (tailleMap, tailleMap, 0);
		pos [3] = new Vector3 (0, tailleMap, 0);
		pos [4] = new Vector3 (0, 0, tailleMap);
		pos [5] = new Vector3 (tailleMap, 0, tailleMap);
		pos [6] = new Vector3 (tailleMap, tailleMap, tailleMap);
		pos [7] = new Vector3 (0, tailleMap, tailleMap);

		// On veut créer les faces du cube !
		remplirFace(0, 1, 2, 3, pos); // coté 1
		remplirFace(4, 5, 6, 7, pos); // coté 2
		remplirFace(0, 4, 7, 3, pos); // coté 3
		remplirFace(0, 1, 5, 4, pos); // sol !
		remplirFace(1, 2, 6, 5, pos); // coté 4
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
		int nbCaves = (int) Mathf.Ceil (proportionCaves * volumeMap / volumeCave);
		generateCave(nbCaves, 3, tailleMaxCave);
	}

	// Génère juste une map pour pouvoir tester des trucs !
	void generateGroundMap() {
		// On initialise la position des coins !
		Vector3[] pos = new Vector3[8];
		pos [0] = new Vector3 (0, 0, 0);
		pos [1] = new Vector3 (tailleMap, 0, 0);
		pos [2] = new Vector3 (tailleMap, tailleMap, 0);
		pos [3] = new Vector3 (0, tailleMap, 0);
		pos [4] = new Vector3 (0, 0, tailleMap);
		pos [5] = new Vector3 (tailleMap, 0, tailleMap);
		pos [6] = new Vector3 (tailleMap, tailleMap, tailleMap);
		pos [7] = new Vector3 (0, tailleMap, tailleMap);

		// On veut créer les faces du cube !
		// remplirFace(0, 1, 2, 3, pos); // coté 1
		// remplirFace(4, 5, 6, 7, pos); // coté 2
		// remplirFace(0, 4, 7, 3, pos); // coté 3
		remplirFace(0, 1, 5, 4, pos); // sol !
		// remplirFace(1, 2, 6, 5, pos); // coté 4
		//remplirFace(2, 3, 7, 6); // plafond !
	}

	// Génère la map du tutoriel ! =)
	void generateTutorialMap() {
		// Première zone : Arrivée
		remplirFace(Vector3.zero, Vector3.right, 20, Vector3.forward, 10); // sol
		remplirFace(Vector3.zero, Vector3.right, 20, Vector3.up, 10); // les 3 cotés
		remplirFace(Vector3.forward * 10, Vector3.right, 20, Vector3.up, 10);
		remplirFace(Vector3.zero, Vector3.up, 10, Vector3.forward, 10);
		nbLumieres++;
		Instantiate (objectif, new Vector3(20, 1, 5), Quaternion.identity);
		
		// Un pont pour aller à la deuxième =)
		remplirBridge(new Vector3(20, 0, 5), Vector3.right, 10); // première ligne droite
		remplirBridge(new Vector3(30, 0, 5), Vector3.forward, 3); // puis un virage
		remplirBridge(new Vector3(30, 0, 8), Vector3.right, 3); // puis un virage
		remplirBridge(new Vector3(33, 0, 8), new Vector3(1, 0, -1), 5); // puis une diagonale
		remplirBridge(new Vector3(38, 0, 3), new Vector3(1f / 3f, -1f / 3f, 0f), 30); // puis une descente
		nbLumieres++;
		Instantiate (objectif, new Vector3(50, -9, 3), Quaternion.identity);

		// Deuxième zone : Saut
		remplirFace(new Vector3(48, -10, 1), Vector3.right, 10, Vector3.forward, 5); // une plus petite zone
		remplirFace(new Vector3(58, -9, 1), Vector3.right, 15, Vector3.forward, 5); // on change de hauteur
		remplirFace(new Vector3(64, -8, 1), Vector3.right, 2, Vector3.forward, 5); // un petit saut
		nbLumieres++;
		Instantiate (objectif, new Vector3(66, -6, 3), Quaternion.identity);
		remplirCubePlein(new Vector3(73, -9, 1), Vector3.right, 5, Vector3.forward, 5, Vector3.up, 5);
		// et puis on creuse pour créer un passage ! =)
		foreach(Collider c in Physics.OverlapBox(new Vector3(73.5f, -6f, 2f), new Vector3(0.5f, 2f, 0.3f))) // colonne
			Destroy(c.gameObject);
		foreach(Collider c in Physics.OverlapBox(new Vector3(74f, -5f, 3.5f), new Vector3(0.3f, 2f, 0.5f))) // colonne
			Destroy(c.gameObject);
		foreach(Collider c in Physics.OverlapBox(new Vector3(75f, -7f, 4f), new Vector3(0.3f, 0.3f, 0.3f))) // trou unique
			Destroy(c.gameObject);
		foreach(Collider c in Physics.OverlapBox(new Vector3(76f, -7f, 4f), new Vector3(0.3f, 0.3f, 0.3f))) // trou unique
			Destroy(c.gameObject);
		foreach(Collider c in Physics.OverlapBox(new Vector3(76f, -5f, 3f), new Vector3(0.3f, 2f, 0.3f))) // trou unique
			Destroy(c.gameObject);
		foreach(Collider c in Physics.OverlapBox(new Vector3(76f, -4f, 2f), new Vector3(0.3f, 2f, 0.3f))) // colonne
			Destroy(c.gameObject);
		foreach(Collider c in Physics.OverlapBox(new Vector3(76f, -3f, 4f), new Vector3(0.3f, 2f, 0.3f))) // colonne
			Destroy(c.gameObject);
		nbLumieres++;
		Instantiate (objectif, new Vector3(77, -3, 3), Quaternion.identity);

		// Un pont
		remplirBridge(new Vector3(78, -5, 3), Vector3.right, 4); // première ligne droite
		remplirBridge(new Vector3(82, -5, 3), new Vector3(1f / 3f, 1f / 3f, 0f), 15); // puis une montée

		// Troisième zone : Aggripage
		remplirFace(new Vector3(87, 0, 1), Vector3.right, 15, Vector3.forward, 5); // une plus petite zone
		remplirFace(new Vector3(92, 0, 1), Vector3.up, 5, Vector3.forward, 5); // une plus petite zone
		remplirFace(new Vector3(102, 0, 5), Vector3.right, 7, Vector3.up, 5); // un mur horizontal !
		remplirFace(new Vector3(109, 0, 1), Vector3.right, 6, Vector3.forward, 5); // une plus petite zone
		nbLumieres++;
		Instantiate (objectif, new Vector3(114, 1, 3), Quaternion.identity);
		remplirFace(new Vector3(115, 0, 0), Vector3.right, 5, Vector3.up, 5); // un mur horizontal !
		remplirFace(new Vector3(120, 0, 5), Vector3.right, 5, Vector3.up, 5); // un mur horizontal !
		remplirFace(new Vector3(125, 0, 0), Vector3.right, 5, Vector3.up, 5); // un mur horizontal !
		remplirFace(new Vector3(130, 0, 1), Vector3.right, 5, Vector3.forward, 5); // une plus petite zone
		remplirFace(new Vector3(135, 0, 2), Vector3.right, 5, Vector3.forward, 3); // une plus petite zone
		nbLumieres++;
		Instantiate (objectif, new Vector3(134, 1, 3), Quaternion.identity);
		remplirFace(new Vector3(135, 0, 2), Vector3.right, 5, Vector3.up, 15); // un mur horizontal !
		remplirFace(new Vector3(135, 0, 4), Vector3.right, 5, Vector3.up, 15); // un mur horizontal !
		remplirBridge(new Vector3(137, 14, 4), new Vector3(0, 0, 1), 16); // puis un pont latéral
		remplirFace(new Vector3(135, 0, 20), Vector3.right, 5, Vector3.forward, 5); // une petite zone
		nbLumieres++;
		Instantiate (objectif, new Vector3(137, 1, 22), Quaternion.identity);

		// Quatrième zone : Shift
		remplirFace(new Vector3(140, 0, 20), new Vector3(1, 1, 0), 10, new Vector3(0, 0, 1), 5); // un escalier montant
		remplirCubePlein(new Vector3(150, 10, 20), new Vector3(1, 0, 0), 5, new Vector3(0, 0, 1), 5, new Vector3(0, -1, 0), 10); // un trou géant !
		foreach(Collider c in Physics.OverlapBox(new Vector3(152f, 12f, 22f), new Vector3(0.3f, 20f, 0.3f))) // colonne
			Destroy(c.gameObject);

		// Cinquième zone : Localisation
		remplirFace(new Vector3(150, -5, 20), Vector3.right, 25, Vector3.forward, 5); // une petite zone
		nbLumieres++;
		Instantiate (objectif, new Vector3(160, -4, 20), Quaternion.identity);
		nbLumieres++;
		Instantiate (objectif, new Vector3(160, -4, 22), Quaternion.identity);
		nbLumieres++;
		Instantiate (objectif, new Vector3(160, -4, 24), Quaternion.identity);
		nbLumieres++;
		Instantiate (objectif, new Vector3(170, -4, 22), Quaternion.identity);

		// Sixième zone : ennemi !
		remplirFace(new Vector3(175, -5, 0), Vector3.right, 25, Vector3.forward, 25); // une grande zone
		remplirFace(new Vector3(175, -5, 0), Vector3.up, 10, Vector3.forward, 20); // et 4 murs pour protéger
		remplirFace(new Vector3(175, -5, 0), Vector3.right, 25, Vector3.up, 10); // et 4 murs pour protéger
		remplirFace(new Vector3(200, -5, 0), Vector3.up, 10, Vector3.forward, 26); // et 4 murs pour protéger
		remplirFace(new Vector3(175, -5, 25), Vector3.right, 25, Vector3.up, 10); // et 4 murs pour protéger
		Instantiate(ennemiPrefabs, new Vector3(187, -4, 12), Quaternion.identity);
		remplirFace(new Vector3(200, 5, 10), Vector3.right, 5, Vector3.forward, 5); // la zone finale !
		nbLumieres++;
		Instantiate (objectif, new Vector3(202, 6, 12), Quaternion.identity);
	}

	// Remplit un mur qui part d'un point de départ dans 2 directions avec une certaine taille selon les 2 directions
	// C'est clair ? Non ? Bah lit le code <3
	void remplirFace(Vector3 depart, Vector3 direction1, int taille1, Vector3 direction2, int taille2) {
		// On remplit tout
		for(int i = 0; i < taille1; i++) {
			for(int j = 0; j < taille2; j++) {
				if(!(i == 0 && j ==0)) {
					Vector3 pos = depart + direction1 * i + direction2 * j;
					cubes.Add(Instantiate(cube, pos, Quaternion.identity) as GameObject);
				}
			}
		}
		// Plus la première case
		cubes.Add(Instantiate(cube, depart, Quaternion.identity) as GameObject);
	}

	// Génère un pont
	void remplirBridge(Vector3 depart, Vector3 direction, int distance) {
		for(int i = 0; i < distance; i++) {
			Vector3 pos = depart + direction * i;
			cubes.Add(Instantiate(cube, pos, Quaternion.identity) as GameObject);
		}
	}

	// Génère un cube plein
	void remplirCubePlein(Vector3 depart, Vector3 dir1, int dist1, Vector3 dir2, int dist2, Vector3 dir3, int dist3) {
		// On remplit tout
		for(int i = 0; i < dist1; i++) {
			for(int j = 0; j < dist2; j++) {
				for(int k = 0; k < dist3; k++) {
					if(!(i == 0 && j == 0 && k == 0)) {
						Vector3 pos = depart + dir1 * i + dir2 * j + dir3 * k;
						cubes.Add(Instantiate(cube, pos, Quaternion.identity) as GameObject);
					}
				}
			}
		}
		// Plus la première case
		cubes.Add(Instantiate(cube, depart, Quaternion.identity) as GameObject);
	}

	// Crée un mur constitué de cubes entre les 4 coins que constitue les indices
	void remplirFace(int indVertx1, int indVertx2, int indVertx3, int indVertx4, Vector3[] pos) {
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

	void generateEnnemies() {
		nbEnnemis = (int) Mathf.Ceil(volumeMap * proportionEnnemis);
		for (int i = 0; i < nbEnnemis; i++) {
			Vector3 posEnnemi = new Vector3 (Random.Range (1f, tailleMap - 1f), Random.Range (1f, tailleMap - 1f), Random.Range (1f, tailleMap - 1f)); 
			Instantiate (ennemiPrefabs, posEnnemi, Quaternion.identity);
		}
	}
}
