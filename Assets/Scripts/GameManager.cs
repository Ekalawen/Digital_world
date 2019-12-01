﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    /// Reference to this script
    /// See http://clearcutgames.net/home/?p=437 for singleton pattern.
    // Returns _instance if it exists, otherwise create one and set it has current _instance
    static GameManager _instance;
    public static GameManager Instance { get { return _instance ?? (_instance = new GameObject().AddComponent<GameManager>()); } }


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

	[HideInInspector]
	public MapManager map;
	[HideInInspector]
	public Player player;
	[HideInInspector]
	public GameObject pointeur;
	[HideInInspector]
	public Console console;
	[HideInInspector]
	public DataBase dataBase;
    [HideInInspector]
    public bool partieDejaTerminee = false;
    [HideInInspector]
    public bool timeFreezed = false;

    //////////////////////////////////////////////////////////////////////////////////////
    // METHODES
    //////////////////////////////////////////////////////////////////////////////////////

    void Awake() {
        if (!_instance) { _instance = this; }
    }

    void Start () {
		// On crée ce dont on a besoin
		map = Instantiate(mapManagerPrefabs).GetComponent<MapManager>();
        player = Instantiate(playerPrefabs).GetComponent<Player>();
		dataBase = Instantiate (dataBasePrefabs).GetComponent<DataBase>();
		console = Instantiate (consolePrefabs).GetComponent<Console>();

        Initialize();
	}

    protected virtual void Initialize() {
        // Puis on les initialises !
        map.Initialize();
        player.Initialize(new Vector3(map.tailleMap / 2, map.tailleMap * 2, map.tailleMap / 2), new Vector2(180, 0));
        dataBase.Initialize();
        console.Initialize();
    }

	// Update is called once per frame
	void Update () {

		// Si on a appuyé sur la touche Escape, on quitte le jeu !
		if (Input.GetKey ("escape")) {
			RevenirAuMenu();
		}

		// Si on a appuyé sur F1 on récupère la souris, ou on la cache ! :D
		if(Input.GetKeyDown(KeyCode.F1)) {
			if(Cursor.visible) {
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			} else {
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
		}

		// Si on a attrapé toutes les lumières
		if (!map.lumieresAttrapees && map.nbLumieres <= 0) {
			map.lumieresAttrapees = true;
			// On affiche un message dans la consolePrefabs !
			console.ToutesLesLumieresAttrapees();
		}

		// Si c'est la fin de la partie, on quitte après 7 secondes !
		if (!partieDejaTerminee && PartieTermine ()) {
			partieDejaTerminee = true;
			StartCoroutine (QuitInSeconds(7));
		}
	}

	bool PartieTermine() {
		// Si le joueur est tombé du cube ...
		if (player.transform.position.y < -10) {
			// Si le joueur a perdu ...
			if (!map.lumieresAttrapees) {
				console.JoueurEjecte();
			// Si le joueur a gagné !
			} else {
				console.JoueurEchappe();
			}
			return true;
		}

		// Ou qu'il est en contact avec un ennemiPrefabs depuis plus de 5 secondes
		// C'est donc qu'il s'est fait conincé !
		// Debug.Log("lastnotcontact = "+ player.GetComponent<Personnage>().lastNotContactEnnemy);
		if (Time.timeSinceLevelLoad - player.GetComponent<Player>().lastNotContactEnnemy >= 5f) {
			console.JoueurCapture();
			player.vitesseDeplacement = 0; // On immobilise le joueur
			player.vitesseSaut = 0; // On immobilise le joueur
			return true;
		}
		return false;
	}

	IEnumerator QuitInSeconds(int tps) {
		yield return new WaitForSeconds (tps);
		// QuitGame ();
		RevenirAuMenu();
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

	public void RevenirAuMenu() {
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
