using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
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
    public BackgroundCarousel carousel;
    public GameObject uiSwapperButton;

    [Header("TutorielTexts")]
    public MenuManagerStrings strings;
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
        SetRandomBackgroundIfNeeded();
        StartMenuMusic();
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
        AsyncOperationHandle<string> handleTutorial = tutoriel.GetLocalizedString();
        yield return handleTutorial;
        string tutorialString = handleTutorial.Result;

        AsyncOperationHandle<string> handleTitre = tutorielRecommandeTitre.GetLocalizedString();
        yield return handleTitre;
        string titreString = handleTitre.Result;

        string tutorielFormate = UIHelper.SurroundWithColor(tutorialString, UIHelper.BLUE);
        AsyncOperationHandle<string> handleTexte = tutorielRecommandeTexte.GetLocalizedString(tutorielFormate);
        yield return handleTexte;
        string texteString = handleTexte.Result;

        popup.AddReplacement("999999.999%", UIHelper.SurroundWithColor("999999.999%", UIHelper.GREEN));
        RunPopup(
            title: titreString,
            text: texteString,
            theme: TexteExplicatif.Theme.NEUTRAL,
            cleanReplacements: false);
        PrefsManager.SetBool(PrefsManager.HAVE_THINK_ABOUT_TUTORIAL_KEY, true);
    }

    public void OnTutorialPress() {
		Debug.Log("On a appuyé sur Tutoriel !");
        PrefsManager.SetBool(PrefsManager.HAVE_THINK_ABOUT_TUTORIAL_KEY, true);

        AsyncOperation loading = SceneManager.LoadSceneAsync("TutorialScene");
        loading.allowSceneActivation = false;

        carousel.GoToBlack();
        loadingMenu.gameObject.SetActive(true);
        loadingMenu.InitializeWithTutoriel(loading);
        gameObject.SetActive(false);

    }

    public void OnOptionPress() {
		Debug.Log("On a appuyé sur Option !");
        menuOptions.Run();
        uiSwapperButton.SetActive(false);
	}

    public void OnEnable() {
        uiSwapperButton.SetActive(true);
    }

    public void OnQuitterPress() {
        popup.RunPopup(strings.quitGameTitle, strings.quitGameTexte, TexteExplicatif.Theme.NEGATIF);
        popup.RemoveDoneButton();
        popup.AddButton(strings.quitGameNoButton, strings.quitGameNoButtonTooltip, TexteExplicatif.Theme.NEGATIF, null);
        popup.AddButton(strings.quitGameYesButton, strings.quitGameYesButtonTooltip, TexteExplicatif.Theme.POSITIF, QuitGame);
	}

	public void QuitGame() {
		Debug.Log("On a appuyé sur Quitter !");
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

    public void RunPopup(LocalizedString title, LocalizedString text, TexteExplicatif.Theme theme, bool cleanReplacements = true) {
        StartCoroutine(CRunPopup(title, text, theme, cleanReplacements));
    }

    public IEnumerator CRunPopup(LocalizedString title, LocalizedString text, TexteExplicatif.Theme theme, bool cleanReplacements = true) {
        AsyncOperationHandle<string> handleTitle = title.GetLocalizedString();
        yield return handleTitle;
        string titleString = handleTitle.Result; // Car les AsyncOperationHandle doivent être utilisé l'un après l'autre ! x)

        AsyncOperationHandle<string> handleText = text.GetLocalizedString();
        yield return handleText;
        string textString = handleText.Result;

        RunPopup(titleString, textString, theme, cleanReplacements);
    }

    protected void SetSavedLocale() {
        StartCoroutine(CSetSavedLocale());
    }

    protected IEnumerator CSetSavedLocale() {
        // Wait for the localization system to initialize, loading Locales, preloading etc.
        Debug.Log($"Initializing locale ...");
        yield return LocalizationSettings.InitializationOperation;
        Debug.Log($"Initialization done !");

        if (PrefsManager.HasKey(PrefsManager.LOCALE_INDEX_KEY)) {
            Debug.Log($"Setting new locale ...");
            int index = PrefsManager.GetInt(PrefsManager.LOCALE_INDEX_KEY, 0);
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
            Debug.Log($"New locale = {LocalizationSettings.SelectedLocale}");
        }
    }

    protected void StartMenuMusic() {
        UISoundManager.Instance.StartMusic();
    }
}
