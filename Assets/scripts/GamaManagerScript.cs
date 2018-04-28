using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamaManagerScript : MonoBehaviour {

	public GameObject cube; // On récupère ce qu'est un cube !
	public GameObject personnage; // On récupère le personnage !
	public static int tailleMap = 10;
	public static int nbBridges = 1;


	private List<GameObject> cubes = new List<GameObject>();
	private Vector3[] pos = { new Vector3(0, 0, 0),
							  new Vector3(tailleMap, 0, 0),
							  new Vector3(tailleMap, tailleMap, 0),
							  new Vector3(0, tailleMap, 0),
							  new Vector3(0, 0, tailleMap),
							  new Vector3(tailleMap, 0, tailleMap),
							  new Vector3(tailleMap, tailleMap, tailleMap),
							  new Vector3(0, tailleMap, tailleMap)};

	// Use this for initialization
	void Start () {
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

		// On veut ajouter un personnage dans le cube central !
		Vector3 posPerso = new Vector3(tailleMap / 2, tailleMap / 2, tailleMap / 2);
		GameObject perso = Instantiate (personnage, posPerso, Quaternion.identity) as GameObject;
		perso.name = "Joueur";

		// On veut maintenant activer la caméra du personnage !
		Camera camPerso = perso.transform.GetChild(0).GetComponent<Camera>() as Camera;
		camPerso.enabled = true;
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

	void generateBridges(List<CubeScript> sources, int nbBridges) {
		for (int i = 0; i < nbBridges; i++) {
			// On récupère les deux sources qui nous intéressent
			int n = Random.Range (0, sources.Count - 1);
			CubeScript c1 =	sources[n];
			sources.Remove (c1);
			n = Random.Range (0, sources.Count - 1);
			CubeScript c2 =	sources[n];
			sources.Remove (c2);

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
	
	// Update is called once per frame
	void Update () {
		// Si on a appuyé sur la touche Escape, on quitte le jeu !
		if (Input.GetKey ("escape")) {
			QuitGame ();
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
