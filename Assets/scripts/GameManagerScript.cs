using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerScript : MonoBehaviour {

	//////////////////////////////////////////////////////////////////////////////////////
	// ATTRIBUTS PUBLIQUES
	//////////////////////////////////////////////////////////////////////////////////////

	public GameObject playerPrefabs; // On récupère le personnage !
	public GameObject consolePrefabs; // On récupère la console !
	public GameObject pointeurPrefabs; // Pour avoir un visuel du centre de l'écran
	public GameObject dataBasePrefabs; // Pour créer l'IA chargé de supervisé les ennemis !
	public GameObject mapManagerPrefabs; // Pour gérer la map !

	//////////////////////////////////////////////////////////////////////////////////////
	// ATTRIBUTS PRIVÉES
	//////////////////////////////////////////////////////////////////////////////////////

	private MapManagerScript map;
	private PersonnageScript player;
	private ConsoleScript console;
	private DataBaseScript dataBase;
	[HideInInspector]
	public bool partieDejaTerminee = false;

	//////////////////////////////////////////////////////////////////////////////////////
	// METHODES
	//////////////////////////////////////////////////////////////////////////////////////

	void Start () {
		// On crée la map
		map = Instantiate(mapManagerPrefabs).GetComponent<MapManagerScript>();

		// On crée le joueur, le pointeur, la console et la database
		instantiatePlayer();
		Instantiate (pointeurPrefabs);
		dataBase = Instantiate (dataBasePrefabs).GetComponent<DataBaseScript>();
		console = Instantiate (consolePrefabs).GetComponent<ConsoleScript>();

		// On finit les liaisaons entre eux
		finirLiaisons();
	}

	void instantiatePlayer() {
		// On veut ajouter un playerPrefabs dans le cube central !
		// On arrive en hauteur comme ça on a le temps de bien voir la carte ! =)
		Vector3 posPerso = new Vector3(map.tailleMap / 2, map.tailleMap + 50, map.tailleMap / 2);
		GameObject perso = Instantiate (playerPrefabs, posPerso, Quaternion.identity) as GameObject;
		perso.name = "Joueur";
		player = perso.GetComponent<PersonnageScript>();

		// On veut maintenant activer la caméra du playerPrefabs !
		Camera camPerso = perso.transform.GetChild(0).GetComponent<Camera>() as Camera;
		camPerso.enabled = true;
	}

	void finirLiaisons() {
		player.console = console;
	}


	// Update is called once per frame
	void Update () {

		// Si on a appuyé sur la touche Escape, on quitte le jeu !
		if (Input.GetKey ("escape")) {
			// QuitGame ();
			revenirAuMenu();
		}

		// Si on a attrapé toutes les lumières
		if (!map.lumieresAttrapees && map.nbLumieres <= 0) {
			map.lumieresAttrapees = true;
			// On affiche un message dans la consolePrefabs !
			console.toutesLumieresAttrapees();
		}

		// Si c'est la fin de la partie, on quitte après 7 secondes !
		if (!partieDejaTerminee && partieTerminee ()) {
			partieDejaTerminee = true;
			StartCoroutine (QuitInSeconds(7));
		}
	}

	bool partieTerminee() {
		// Si le joueur est tombé du cube ...
		if (player.transform.position.y < -10) {
			// Si le joueur a perdu ...
			if (!map.lumieresAttrapees) {
				console.joueurEjecte();
			// Si le joueur a gagné !
			} else {
				console.joueurEchappe();
			}
			return true;
		}

		// Ou qu'il est en contact avec un ennemiPrefabs depuis plus de 5 secondes
		// C'est donc qu'il s'est fait conincé !
		//Debug.Log("lastnotcontact = "+ player.GetComponent<PersonnageScript>().lastNotContactEnnemy);
		if (Time.timeSinceLevelLoad - player.GetComponent<PersonnageScript> ().lastNotContactEnnemy >= 5f) {
			console.joueurCapture();
			player.vitesseDeplacement = 0; // On immobilise le joueur
			player.vitesseSaut = 0; // On immobilise le joueur
			return true;
		}
		return false;
	}

	IEnumerator QuitInSeconds(int tps) {
		yield return new WaitForSeconds (tps);
		// QuitGame ();
		revenirAuMenu();
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

	public void revenirAuMenu() {
		// On détruit tout !
		Object.Destroy(map);
		Object.Destroy(player);
		Object.Destroy(console);
		Object.Destroy(dataBase);
		Object.Destroy(this);

		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		SceneManager.LoadScene("MenuScene");
	}
}
