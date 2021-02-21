using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour {

    static MenuManager _instance;
    public static MenuManager Instance { get { return _instance ?? (_instance = new GameObject().AddComponent<MenuManager>()); } }

    public static bool DISABLE_HOTKEYS = false;
    public static string FIRST_TIME_CONNEXION_KEY = "firstTimeConnexionKey";
    public static string HAVE_THINK_ABOUT_TUTORIAL_KEY = "haveThinkAboutTutorialKey";
    public static string SHOULD_SET_RANDOM_BACKGROUND_KEY = "shouldSetRandomBackgroundKey";
    public static string TRUE = "true";
    public static string FALSE = "false";
    public static string LOCALE_INDEX_KEY = "localeIndexKey";

    [Header("Links")]
    public MenuOptions menuOptions;
    public LoadingMenu loadingMenu;
    public MenuBackgroundBouncing menuBouncingBackground;
    public TexteExplicatif popup;

    [Header("TutorielTexts")]
    public LocalizedString tutoriel;
    public LocalizedString tutorielRecommandeTitre;
    public LocalizedString tutorielRecommandeTexte;

    protected SelectorManager selectorManager;

    void Awake() {
        if (!_instance) { _instance = this; }
    }

    private void Start() {
        selectorManager = SelectorManager.Instance;
        SetSavedLocale();
        menuBouncingBackground.Initialize();
        ResetPlayerPrefsIfFirstConnexion();
        SetRandomBackgroundIfNeeded();
        StartMenuMusic();
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
        if (!HaveThinkAboutTutorial()) {
            AdvicePlayTutorial();
        } else {
            SceneManager.LoadScene("SelectorScene");
        }
    }

    protected bool HaveThinkAboutTutorial() {
        return PlayerPrefs.HasKey(HAVE_THINK_ABOUT_TUTORIAL_KEY);
    }

    protected void AdvicePlayTutorial() {
        StartCoroutine(CAdvicePlayTutorial());
    }

    protected IEnumerator CAdvicePlayTutorial() {
        var handleTutorial = tutoriel.GetLocalizedString();
        yield return handleTutorial;

        var handleTitre = tutorielRecommandeTitre.GetLocalizedString();
        yield return handleTitre;

        string tutorielFormate = UIHelper.SurroundWithColor(handleTutorial.Result, UIHelper.BLUE);
        var handleTexte = tutorielRecommandeTexte.GetLocalizedString(tutorielFormate);
        yield return handleTexte;

        popup.AddReplacement("999999.999%", UIHelper.SurroundWithColor("999999.999%", UIHelper.GREEN));
        RunPopup(
            title: handleTitre.Result,
            text: handleTexte.Result,
            theme: TexteExplicatif.Theme.NEUTRAL,
            cleanReplacements: false);
        PlayerPrefs.SetString(HAVE_THINK_ABOUT_TUTORIAL_KEY, MenuManager.TRUE);
    }

    // Lorsqu'on appui sur le bouton tutoriel
    public void OnTutorialPress() {
		Debug.Log("On a appuyé sur Tutoriel !");
        PlayerPrefs.SetString(HAVE_THINK_ABOUT_TUTORIAL_KEY, MenuManager.TRUE);
        //PlayerPrefs.SetString(MenuLevel.LEVEL_NAME_ID_KEY, "TutorialScene");

		//SceneManager.LoadScene("TutorialScene");
        AsyncOperation loading = SceneManager.LoadSceneAsync("TutorialScene");
        loading.allowSceneActivation = false;

        loadingMenu.gameObject.SetActive(true);
        loadingMenu.InitializeWithTutoriel(loading);
        gameObject.SetActive(false);
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
        float probaSource = MathCurves.CubicRandom(0.0002f, 0.0035f);
        int distanceSource = UnityEngine.Random.Range(1, 12);
        float decroissanceSource = UnityEngine.Random.Range(0.002f, 0.02f);
        List<ColorManager.Theme> themes = new List<ColorManager.Theme>();
        int nbThemes = UnityEngine.Random.Range(1, 4);
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

    public void RunPopup(string title, string text, TexteExplicatif.Theme theme, bool cleanReplacements = true) {
        popup.Initialize(title: title, mainText: text, theme: theme, cleanReplacements: cleanReplacements);
        popup.Run();
    }

    protected void SetSavedLocale() {
        StartCoroutine(CSetSavedLocale());
    }

    protected IEnumerator CSetSavedLocale() {
        // Wait for the localization system to initialize, loading Locales, preloading etc.
        yield return LocalizationSettings.InitializationOperation;

        if (PlayerPrefs.HasKey(LOCALE_INDEX_KEY)) {
            int index = PlayerPrefs.GetInt(LOCALE_INDEX_KEY);
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
        }
    }

    public static string BoolToString(bool input) {
        return input ? TRUE : FALSE;
    }

    protected void StartMenuMusic() {
        UISoundManager.Instance.StartMusic();
    }
}
