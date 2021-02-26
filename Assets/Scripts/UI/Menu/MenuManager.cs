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
        if (!PrefsManager.HasKey(PrefsManager.FIRST_TIME_CONNEXION_KEY)) {
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
        return PrefsManager.HasKey(PrefsManager.HAVE_THINK_ABOUT_TUTORIAL_KEY);
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
        PrefsManager.SetBool(PrefsManager.HAVE_THINK_ABOUT_TUTORIAL_KEY, true);
    }

    public void OnTutorialPress() {
		Debug.Log("On a appuyé sur Tutoriel !");
        PrefsManager.SetBool(PrefsManager.HAVE_THINK_ABOUT_TUTORIAL_KEY, true);

        AsyncOperation loading = SceneManager.LoadSceneAsync("TutorialScene");
        loading.allowSceneActivation = false;

        loadingMenu.gameObject.SetActive(true);
        loadingMenu.InitializeWithTutoriel(loading);
        gameObject.SetActive(false);
    }

    public void OnOptionPress() {
		Debug.Log("On a appuyé sur Option !");
        menuOptions.Run();
	}

	public void OnQuitterPress() {
		Debug.Log("On a appuyé sur Quitter !");
		QuitGame();
	}

	public void QuitGame() {
		#if UNITY_EDITOR
			// Application.Quit() does not work in the editor so
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
        if(PrefsManager.GetBool(PrefsManager.SHOULD_SET_RANDOM_BACKGROUND_KEY, false)) {
            SetRandomBackground();
            PrefsManager.SetBool(PrefsManager.SHOULD_SET_RANDOM_BACKGROUND_KEY, false);
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

        if (PrefsManager.HasKey(PrefsManager.LOCALE_INDEX_KEY)) {
            int index = PrefsManager.GetInt(PrefsManager.LOCALE_INDEX_KEY, 0);
            Debug.Log($"index = {index}");
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
        }
    }

    protected void StartMenuMusic() {
        UISoundManager.Instance.StartMusic();
    }
}
