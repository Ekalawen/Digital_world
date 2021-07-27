using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    /// Reference to this script
    /// See http://clearcutgames.net/home/?p=437 for singleton pattern.
    // Returns _instance if it exists, otherwise create one and set it has current _instance
    static GameManager _instance;
    public static GameManager Instance { get { return _instance ?? (_instance = new GameObject().AddComponent<GameManager>()); } }


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
    public GameObject flockManagerPrefab; // Pour gérer les flock controllers !
    public GameObject cheatCodeManagerPrefab; // Pour gérer les cheat codes ! :)
    public GameObject goalManagerPrefab; // Pour gérer les goal tresholds ! :)

    [HideInInspector]
	public MapManager map;
	[HideInInspector]
	public Player player;
	[HideInInspector]
	public Pointeur pointeur;
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
    public FlockManager flockManager;
    [HideInInspector]
    public CheatCodeManager cheatCodeManager;
    [HideInInspector]
    public GoalManager goalManager;
    [HideInInspector]
    public GameObject managerFolder;
    [HideInInspector]
    public bool partieDejaTerminee = false;
    [HideInInspector]
    protected bool timeFreezed = false;
    protected bool isPaused = false;
    protected InputManager inputManager;
    protected bool initializationIsOver = false;
    [HideInInspector]
    public UnityEvent onInitilizationFinish;

    void Awake() {
        if (!_instance) {
            _instance = this;
        }
    }

    void Start () {
        managerFolder = new GameObject("Managers");
        timerManager = Instantiate(timerManagerPrefab, managerFolder.transform).GetComponent<TimerManager>();
        gravityManager = Instantiate(gravityManagerPrefab, managerFolder.transform).GetComponent<GravityManager>();
        goalManager = Instantiate(goalManagerPrefab, managerFolder.transform).GetComponent<GoalManager>();
		map = Instantiate(mapManagerPrefab, managerFolder.transform).GetComponent<MapManager>();
        player = Instantiate(playerPrefab).GetComponent<Player>();
		eventManager = Instantiate(eventManagerPrefab, managerFolder.transform).GetComponent<EventManager>();
		console = Instantiate(consolePrefab, managerFolder.transform).GetComponent<Console>();
		colorManager = Instantiate(colorManagerPrefab, managerFolder.transform).GetComponent<ColorManager>();
        pointeur = Instantiate(pointeurPrefab, managerFolder.transform).GetComponent<Pointeur>();
        ennemiManager = Instantiate(ennemiManagerPrefab, managerFolder.transform).GetComponent<EnnemiManager>();
        itemManager = Instantiate(itemManagerPrefab, managerFolder.transform).GetComponent<ItemManager>();
        soundManager = Instantiate(soundManagerPrefab, managerFolder.transform).GetComponent<SoundManager>();
        postProcessManager = Instantiate(postProcessManagerPrefab, managerFolder.transform).GetComponent<PostProcessManager>();
        scanManager = Instantiate(scanManagerPrefab, managerFolder.transform).GetComponent<ScanManager>();
        historyManager = Instantiate(historyManagerPrefab/*, managerFolder.transform*/).GetComponent<HistoryManager>(); // On le met à la racine car DontDestroyOnLoad only works for root components !
        flockManager = Instantiate(flockManagerPrefab, managerFolder.transform).GetComponent<FlockManager>();
        cheatCodeManager = Instantiate(cheatCodeManagerPrefab, managerFolder.transform).GetComponent<CheatCodeManager>();
        inputManager = InputManager.Instance;

        Initialize();
	}

    protected virtual void Initialize()
    {
        timerManager.Initialize();
        gravityManager.Initialize();
        goalManager.Initialize();
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
        pointeur.Initialize();
        postProcessManager.Initialize();
        scanManager.Initialize();
        historyManager.Initialize();
        flockManager.Initialize();
        cheatCodeManager.Initialize();
        FinishInitialization();
    }

    private void FinishInitialization() {
        initializationIsOver = true;
        onInitilizationFinish.Invoke();
    }

    void Update () {
        CheckRestartGame();

        CheckPauseToggling();

        CheckOptionsToggling();

        CheckCursorToggling();

#if UNITY_EDITOR
        CheckRecordMoments();
#endif
    }

    protected void CheckPauseToggling() {
        if (inputManager.GetPauseGame()) {
            if (!isPaused) {
                if (eventManager.IsGameOver()) {
                    SaveGameResultIfQuitBeforeEnding();
                    eventManager.QuitOrReload();
                } else {
                    Pause();
                }
            }
            // Le UnPause se fait dans le PauseMenu ! :)
        }
    }

    protected void CheckOptionsToggling() {
        if (inputManager.GetOptions()) {
            if (!isPaused && ! eventManager.IsGameOver()) {
                Pause();
                console.pauseMenu.GetComponentInChildren<PauseMenu>().Options();
            }
        }
    }

    public void Pause() {
        isPaused = true;
        Time.timeScale = 0.0f;
        ShowCursor();
        pointeur.gameObject.SetActive(false);
        console.OpenPauseMenu();
        soundManager.PauseSounds();
    }

    public void UnPause() {
        isPaused = false;
        Time.timeScale = 1.0f;
        HideCursor();
        pointeur.gameObject.SetActive(true);
        Tooltip.Hide();
        console.ClosePauseMenu();
        soundManager.UnPauseSounds();
        postProcessManager.UnPauseEffects();
    }

    public bool IsPaused() {
        return isPaused;
    }

    protected void CheckRecordMoments() {
        if (Input.GetKeyDown(KeyCode.G)) {
            Debug.Log("On ne quittera pas la scène.");
            eventManager.ShouldNotAutomaticallyQuit();
        }
    }

    protected static void CheckCursorToggling() {
        if (Input.GetKeyDown(KeyCode.F1)) {
            if (Cursor.visible) {
                HideCursor();
            } else {
                ShowCursor();
            }
        }
    }

    public static void ShowCursor() {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public static void HideCursor() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void QuitOrReloadGame() {
        SaveGameResultIfQuitBeforeEnding();
        eventManager.QuitOrReload();
    }

    protected void CheckRestartGame() {
        if (inputManager.GetRestartGame()) {
            RestartGame();
        }
    }

    public void RestartGame() {
        SaveGameResultIfQuitBeforeEnding();
        eventManager.ReloadScene();
    }

    protected void SaveGameResultIfQuitBeforeEnding() {
        if (GetMapType() == MenuLevel.LevelType.INFINITE
        && !eventManager.IsGameOver()
        && eventManager.IsNewBestScore()) {
            eventManager.RememberGameResult(success: false);
        } else if (timerManager.GetRealElapsedTime() > 10
                && !eventManager.IsGameOver()) {
            eventManager.RememberGameResult(success: false);
        }
    }

    public IEnumerator QuitInSeconds(float tps) {
		yield return new WaitForSeconds (tps);
		QuitterPartie();
	}

	public void QuitGame() {
		#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
		#else
			Application.Quit();
		#endif
	}

	public void QuitterPartie() {
		//Object.Destroy(map);
		//Object.Destroy(player);
		//Object.Destroy(console);
		//Object.Destroy(eventManager);
		//Object.Destroy(this);

        PrefsManager.SetString(PrefsManager.LAST_LEVEL_KEY, SceneManager.GetActiveScene().name);

        Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
        Time.timeScale = 1.0f;

        if(SceneManager.GetActiveScene().name == "TutorialScene") {
            Destroy(historyManager.gameObject);
            SceneManager.LoadScene("MenuScene");
        } else if ((GetMapType() == MenuLevel.LevelType.REGULAR && eventManager.IsGameWin())) {
            SceneManager.LoadScene("RewardScene");
        } else {
            Destroy(historyManager.gameObject);
            SceneManager.LoadScene("SelectorScene");
        }
	}

    public bool IsTimeFreezed() {
        return timeFreezed;
    }
    public void FreezeTime() {
        timeFreezed = true;
    }
    public void UnFreezeTime() {
        timeFreezed = false;
    }

    public InfiniteMap GetInfiniteMap() {
        return map as InfiniteMap;
    }

    internal MenuLevel.LevelType GetMapType() {
        if(map.gameObject.GetComponent<InfiniteMap>() == null) {
            return MenuLevel.LevelType.REGULAR;
        } else {
            return MenuLevel.LevelType.INFINITE;
        }
    }

    public bool IsInitializationOver() {
        return initializationIsOver;
    }
}

