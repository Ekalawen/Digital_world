using System.Collections;
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

    public GameObject playerPrefab; // On récupère le personnage !
	public GameObject consolePrefab; // On récupère la console !
	public GameObject pointeurPrefab; // Pour avoir un visuel du centre de l'écran
	public GameObject eventManagerPrefab; // Pour créer l'IA chargé de supervisé les ennemis !
	public GameObject mapManagerPrefab; // Pour gérer la map !
	public GameObject colorManagerPrefab; // Pour gérer les couleurs !
	public GameObject ennemiManagerPrefab; // Pour gérer les ennemis !
	public GameObject itemManagerPrefab; // Pour gérer les items !
	public GameObject soundManagerPrefab; // Pour gérer les sons et musiques !
    public GameObject postProcessManagerPrefab; // Pour gérer les posteffects !
    public GameObject timerManagerPrefab; // Pour gérer le timer !
    public GameObject gravityManagerPrefab; // Pour gérer la gravité !
    public GameObject scanManagerPrefab; // Pour gérer les scans !
    public GameObject historyManagerPrefab; // Pour retenir des infos sur la partie !

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
	public EventManager eventManager;
	[HideInInspector]
	public ColorManager colorManager;
	[HideInInspector]
	public EnnemiManager ennemiManager;
	[HideInInspector]
	public ItemManager itemManager;
	[HideInInspector]
	public SoundManager soundManager;
    [HideInInspector]
    public PostProcessManager postProcessManager;
    [HideInInspector]
    public TimerManager timerManager;
    [HideInInspector]
    public GravityManager gravityManager;
    [HideInInspector]
    public ScanManager scanManager;
    [HideInInspector]
    public HistoryManager historyManager;
    [HideInInspector]
    public GameObject managerFolder;
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
        managerFolder = new GameObject("Managers");
        timerManager = Instantiate(timerManagerPrefab, managerFolder.transform).GetComponent<TimerManager>();
        gravityManager = Instantiate(gravityManagerPrefab, managerFolder.transform).GetComponent<GravityManager>();
		map = Instantiate(mapManagerPrefab, managerFolder.transform).GetComponent<MapManager>();
        player = Instantiate(playerPrefab, managerFolder.transform).GetComponent<Player>();
		eventManager = Instantiate(eventManagerPrefab, managerFolder.transform).GetComponent<EventManager>();
		console = Instantiate(consolePrefab, managerFolder.transform).GetComponent<Console>();
		colorManager = Instantiate(colorManagerPrefab, managerFolder.transform).GetComponent<ColorManager>();
        pointeur = Instantiate(pointeurPrefab, managerFolder.transform);
        ennemiManager = Instantiate(ennemiManagerPrefab, managerFolder.transform).GetComponent<EnnemiManager>();
        itemManager = Instantiate(itemManagerPrefab, managerFolder.transform).GetComponent<ItemManager>();
        soundManager = Instantiate(soundManagerPrefab, managerFolder.transform).GetComponent<SoundManager>();
        postProcessManager = Instantiate(postProcessManagerPrefab, managerFolder.transform).GetComponent<PostProcessManager>();
        scanManager = Instantiate(scanManagerPrefab, managerFolder.transform).GetComponent<ScanManager>();
        historyManager = Instantiate(historyManagerPrefab, managerFolder.transform).GetComponent<HistoryManager>();

        Initialize();
	}

    protected virtual void Initialize() {
        // Puis on les initialises !
        timerManager.Initialize();
        gravityManager.Initialize();
        map.Initialize();
        Vector3 playerPosition = map.GetPlayerStartPosition();
        Vector2 playerOrientationXY = map.GetPlayerStartOrientationXY(playerPosition);
        player.Initialize(playerPosition, playerOrientationXY);
        eventManager.Initialize();
        colorManager.Initialize();
        ennemiManager.Initialize();
        itemManager.Initialize();
        console.Initialize();
        soundManager.Initialize();
        postProcessManager.Initialize();
        scanManager.Initialize();
        historyManager.Initialize();
    }

	// Update is called once per frame
	void Update () {

        // Si on a appuyé sur la touche Escape, on quitte le jeu !
        if (Input.GetKey (KeyCode.Escape)) {
			QuitterPartie();
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

		// Si c'est la fin de la partie, on quitte après 7 secondes !
		if (!partieDejaTerminee && eventManager.PartieTermine()) {
			partieDejaTerminee = true;
			StartCoroutine (QuitInSeconds(7));
		}
	}

    public IEnumerator QuitInSeconds(int tps) {
		yield return new WaitForSeconds (tps);
		// QuitGame ();
		QuitterPartie();
	}

	public void QuitGame() {
		// save any game data here
		#if UNITY_EDITOR
			// Application.Quit() does not work in the editor so
			// UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
			UnityEditor.EditorApplication.isPlaying = false;
		#else
			Application.Quit();
		#endif
	}

	public void QuitterPartie() {
		// On détruit tout !
		Object.Destroy(map);
		Object.Destroy(player);
		Object.Destroy(console);
		Object.Destroy(eventManager);
		Object.Destroy(this); 

        PlayerPrefs.SetString(MenuLevelSelector.LEVEL_INDICE_MUST_BE_USED_KEY, "True");

		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

        if(eventManager.IsWin()) {
            SceneManager.LoadScene("RewardScene");
        } else {
            SceneManager.LoadScene("MenuScene");
        }
	}
}
