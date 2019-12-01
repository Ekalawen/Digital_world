using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour {
	//////////////////////////////////////////////////////////////////////////////////////
	// ENUMERATION
	//////////////////////////////////////////////////////////////////////////////////////

	// public enum TypeMap {CUBE_MAP, PLAINE_MAP, LABYRINTHE_MAP, GROUND_MAP, EMPTY_MAP, TUTORIAL_MAP}; // Plus vraiment utile ! :D

	//////////////////////////////////////////////////////////////////////////////////////
	// ATTRIBUTS PUBLIQUES
	//////////////////////////////////////////////////////////////////////////////////////

	public GameObject cube; // On récupère ce qu'est un cube !
	public GameObject objectif; // On récupère les objectifs !
	public GameObject ennemiPrefabs; // On récupère un ennemi !
	public int tailleMap; // La taille de la map, en largeur, hauteur et profondeur
	// Plus vraiment utile : public TypeMap typeMap; // Le type de la map, Cube, Plains ...
	public List<Cube.ThemeCube> themeCube; // Le thème, cad la couleur de la map !
	public float frequenceSource; // La frequence qu'un cube soit une source
	public float distanceSourceMax; // La distance de coloration d'une source

	//////////////////////////////////////////////////////////////////////////////////////
	// ATTRIBUTS PRIVÉES
	//////////////////////////////////////////////////////////////////////////////////////

	protected List<GameObject> cubes;
	[HideInInspector]
	public int nbLumieres;
	[HideInInspector]
	public bool lumieresAttrapees;
	[HideInInspector]
	public int nbEnnemis;

	//////////////////////////////////////////////////////////////////////////////////////
	// METHODES
	//////////////////////////////////////////////////////////////////////////////////////

	protected void Start() {
	}

    public virtual void Initialize() {
		// Initialisation
		name = "MapManager";
		cubes = new List<GameObject>();
		lumieresAttrapees = false;

		// On définit nos cubes
		Cube.probaSource = frequenceSource;
		Cube.distSourceMax = distanceSourceMax;

		// On charge le thème de la map
		Cube.theme = new List<Cube.ThemeCube>();
		foreach(Cube.ThemeCube theme in themeCube) {
			if(theme == Cube.ThemeCube.RANDOM) {
				System.Array values = System.Enum.GetValues(typeof(Cube.ThemeCube));
				int indiceTheme = Random.Range(0, values.Length - 1);
				Cube.theme.Add((Cube.ThemeCube) values.GetValue(indiceTheme));
			} else {
				Cube.theme.Add(theme);
			}
		}

		// Ce sont les classes qui hériteront de cette classe qui créeront leur propre map !
    }

	// Remplit un mur qui part d'un point de départ dans 2 directions avec une certaine taille selon les 2 directions
	// C'est clair ? Non ? Bah lit le code <3
	protected void remplirFace(Vector3 depart, Vector3 direction1, int taille1, Vector3 direction2, int taille2) {
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

	// Génère un pont qui part d'un point de départ dans une direction et sur une certaine distance !
	public void remplirBridge(Vector3 depart, Vector3 direction, int distance) {
		for(int i = 0; i < distance; i++) {
			Vector3 pos = depart + direction * i;
			cubes.Add(Instantiate(cube, pos, Quaternion.identity) as GameObject);
		}
	}

	// Génère un cube plein qui part d'un point de départ et qui va dans 3 directions avec 3 distances !
	protected void remplirCubePlein(Vector3 depart, Vector3 dir1, int dist1, Vector3 dir2, int dist2, Vector3 dir3, int dist3) {
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
	protected void remplirFace(int indVertx1, int indVertx2, int indVertx3, int indVertx4, Vector3[] pos) {
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
}
