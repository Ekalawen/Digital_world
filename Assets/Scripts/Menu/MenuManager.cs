using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour {

    static MenuManager _instance;
    public static MenuManager Instance { get { return _instance ?? (_instance = new GameObject().AddComponent<MenuManager>()); } }


    public static bool DISABLE_HOTKEYS = false;
    public static string FIRST_TIME_CONNEXION_KEY = "firstTimeConnexionKey";
    public static string SHOULD_SET_RANDOM_BACKGROUND_KEY = "shouldSetRandomBackgroundKey";
    public static string TRUE = "true";
    public static string FALSE = "false";

    public MenuLevelSelector menuLevelSelector;
    public MenuOptions menuOptions;
    public MenuBackgroundBouncing menuBouncingBackground;
    public TexteExplicatif popup;

    void Awake() {
        if (!_instance) { _instance = this; }
    }

    private void Start() {
        ResetPlayerPrefsIfFirstConnexion();
        SetRandomBackgroundIfNeeded();
    }

    protected void ResetPlayerPrefsIfFirstConnexion() {
        if (!PlayerPrefs.HasKey(FIRST_TIME_CONNEXION_KEY)) {
            menuOptions.ReinitialiserSauvegardes();
        }
    }

    void Update() {
		// Si on appui sur Echap on quitte
		if(!DISABLE_HOTKEYS && Input.GetKeyDown(KeyCode.Escape)) {
			OnQuitterPress();
		}
		// Si on appui sur Entrée, on joue !
		if(!DISABLE_HOTKEYS && (Input.GetKeyDown(KeyCode.Return)
        || Input.GetKeyDown(KeyCode.KeypadEnter)
        || Input.GetKeyDown(KeyCode.Space))) {
			OnPlayPress();
		}
	}

	// Lorsqu'on appui sur le bouton jouer
	public void OnPlayPress() {
		Debug.Log("On a appuyé sur Play !");
        //menuLevelSelector.Run(0);
        SceneManager.LoadScene("SelectorScene");
    }

	// Lorsqu'on appui sur le bouton tutoriel
	public void OnTutorialPress() {
		Debug.Log("On a appuyé sur Tutoriel !");
        menuLevelSelector.CleanLevelIndice();
        PlayerPrefs.SetString(MenuLevel.LEVEL_NAME_KEY, "Tutoriel");
		SceneManager.LoadScene("TutorialScene");
	}

	// Lorsqu'on appui sur le bouton options
	public void OnOptionPress() {
		Debug.Log("On a appuyé sur Option !");
        menuOptions.Run();
	}

	// Lorsqu'on appui sur le bouton quitter
	public void OnQuitterPress() {
		Debug.Log("On a appuyé sur Quitter !");
		QuitGame();
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

    public void SetRandomBackground() {
        float probaSource = Random.Range(0.00002f, 0.0035f);
        int distanceSource = Random.Range(1, 12);
        float decroissanceSource = Random.Range(0.002f, 0.02f);
        List<ColorSource.ThemeSource> themes = new List<ColorSource.ThemeSource>();
        int nbThemes = Random.Range(1, 4);
        for(int i = 0; i < nbThemes; i++)
            themes.Add(ColorManager.GetRandomTheme());
        menuBouncingBackground.SetParameters(probaSource, distanceSource, decroissanceSource, themes);
    }

    protected void SetRandomBackgroundIfNeeded() {
        if(PlayerPrefs.HasKey(SHOULD_SET_RANDOM_BACKGROUND_KEY)) {
            string shouldSet = PlayerPrefs.GetString(SHOULD_SET_RANDOM_BACKGROUND_KEY);
            if(shouldSet == TRUE) {
                SetRandomBackground();
                PlayerPrefs.SetString(SHOULD_SET_RANDOM_BACKGROUND_KEY, FALSE);
            }
        }
    }

    public void RunPopup(string title, string text) {
        popup.SetText(title, text);
        popup.Run();
    }
}
