using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectorManager : MonoBehaviour {

    public static string LAST_LEVEL_KEY = "lastLevelKey";

    static SelectorManager _instance;
    //public static SelectorManager Instance { get { return _instance ?? (_instance = new GameObject().AddComponent<SelectorManager>()); } }
    public static SelectorManager Instance { get { return _instance; } }

    [Header("Links")]
    public Transform levelsFolder;
    public Transform pathsFolder;
    public new Camera camera;
    public MenuBackgroundBouncing background;
    public LoadingMenu loadingMenu;
    public TexteExplicatif popup;

    [Header("Parameters")]
    public float dureeFading = 0.5f;

    protected List<SelectorLevel> levels;
    protected List<SelectorPath> paths;
    protected SelectorLevel currentSelectorLevel;
    protected bool hasLevelOpen = false;

    void Awake() {
        if (!_instance) { _instance = this; }
    }

    public void Start() {
        GatherLevels();
        GatherPaths();
        background.gameObject.SetActive(false);
        SetCurrentLevelBasedOnLastSavedLevel();
        PlaceCameraInFrontOfCurrentLevel();
        DisplayCurrentLevel();
        CleanLastSavedLevel();
    }

    protected void CleanLastSavedLevel() {
        PlayerPrefs.DeleteKey(LAST_LEVEL_KEY);
    }

    protected void SetCurrentLevelBasedOnLastSavedLevel() {
        SelectorLevel lastLevel = GetLastLevelSaved();
        currentSelectorLevel = lastLevel ?? levels[0];
    }

    public void Update() {
        if (!HasSelectorLevelOpen()) {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                Return();
            }
        }
    }

    public List<SelectorLevel> GetLevels() {
        return levels;
    }

    protected void GatherPaths() {
        paths = new List<SelectorPath>();
        foreach(Transform child in pathsFolder) {
            SelectorPath path = child.gameObject.GetComponent<SelectorPath>();
            if (path != null) {
                LinkPath(path);
                paths.Add(path);
            }
        }
    }

    protected void LinkPath(SelectorPath path) {
    }

    protected void GatherLevels() {
        levels = new List<SelectorLevel>();
        foreach(Transform child in levelsFolder) {
            SelectorLevel level = child.gameObject.GetComponent<SelectorLevel>();
            if (level != null) {
                LinkLevel(level);
                levels.Add(level);
            }
        }
    }

    protected void LinkLevel(SelectorLevel level) {
        level.menuLevel.selectorManager = this;
        level.menuLevel.menuBouncingBackground = background;
    }

    public void Play(string levelSceneName) {
        if (!HasSelectorLevelOpen())
            return;
        MenuLevel menuLevel = GetCurrentLevel().menuLevel;
        if (menuLevel.IsPlayStarted()) { // Pour éviter de lancer deux fois le play !
            return;
        }
        //SceneManager.LoadScene(levelSceneName, LoadSceneMode.Additive);
        AsyncOperation loading = SceneManager.LoadSceneAsync(levelSceneName);
        loading.allowSceneActivation = false;
        menuLevel.SetPlayStarted();
        SaveLastLevel();
        PlayerPrefs.SetString(MenuLevel.LEVEL_NAME_KEY, menuLevel.GetName());

        StartCoroutine(FadeOut(menuLevel.gameObject));
        FadeInLoadingMenu(loading, menuLevel);
    }

    protected IEnumerator FadeOut(GameObject go) {
        Timer timer = new Timer(dureeFading);
        go.GetComponent<CanvasGroup>().alpha = 1.0f;
        while(!timer.IsOver()) {
            go.GetComponent<CanvasGroup>().alpha = 1.0f - timer.GetAvancement();
            yield return null;
        }
        go.GetComponent<CanvasGroup>().alpha = 0.0f;
        go.SetActive(false);
    }

    protected IEnumerator FadeIn(GameObject go) {
        Timer timer = new Timer(dureeFading);
        gameObject.SetActive(true);
        go.GetComponent<CanvasGroup>().alpha = 0.0f;
        while(!timer.IsOver()) {
            go.GetComponent<CanvasGroup>().alpha = timer.GetAvancement();
            yield return null;
        }
        go.GetComponent<CanvasGroup>().alpha = 1.0f;
    }

    protected void FadeInLoadingMenu(AsyncOperation loading, MenuLevel level) {
        loadingMenu.gameObject.SetActive(true);
        loadingMenu.Initialize(loading, level);
        StartCoroutine(FadeIn(loadingMenu.gameObject));
    }

    protected void SaveLastLevel() {
        PlayerPrefs.SetString(LAST_LEVEL_KEY, GetCurrentLevel().menuLevel.levelSceneName);
        PlayerPrefs.Save();
    }

    protected SelectorLevel GetLastLevelSaved() {
        if (PlayerPrefs.HasKey(LAST_LEVEL_KEY)) {
            string levelSceneName = PlayerPrefs.GetString(LAST_LEVEL_KEY);
            SelectorLevel level = levels.Find(l => l.menuLevel.levelSceneName == levelSceneName);
            if (level != null) {
                return level;
            }
        }
        return null;
    }

    private SelectorLevel GetCurrentLevel() {
        return currentSelectorLevel;
    }

    public void TryDisplayLevel(SelectorLevel selectorLevel, bool instantDisplay = false) {
        List<SelectorPath> precedentPaths = paths.FindAll(p => p.endLevel == selectorLevel);
        if(precedentPaths.All(p => p.IsUnlocked())) {
            DisplayLevel(selectorLevel, instantDisplay);
        } else {
            RunPopup("Niveau vérouillé !",
                "Niveau vérouillé.\n" +
                "Vous devez débloquer tous les chemins menant à ce niveau pour le dévérouiller !", TexteExplicatif.Theme.NEGATIF);
        }
    }

    public void DisplayLevel(SelectorLevel selectorLevel, bool instantDisplay = false) {
        currentSelectorLevel = selectorLevel;
        hasLevelOpen = true;
        selectorLevel.menuLevel.gameObject.SetActive(true);
        background.gameObject.SetActive(true);
        if (instantDisplay) {
            currentSelectorLevel.menuLevel.gameObject.SetActive(true);
            background.gameObject.SetActive(true);
        } else {
            StartCoroutine(FadeIn(currentSelectorLevel.menuLevel.gameObject));
            StartCoroutine(FadeIn(background.gameObject));
        }
    }

    public bool HasSelectorLevelOpen() {
        return hasLevelOpen;
    }

    public void BackToSelector() {
        if (!HasSelectorLevelOpen())
            return;
        hasLevelOpen = false;
        StartCoroutine(FadeOut(currentSelectorLevel.menuLevel.gameObject));
        StartCoroutine(FadeOut(background.gameObject));
    }

    public void Next() {
        if (!HasSelectorLevelOpen())
            return;
    }

    public void Previous() {
        if (!HasSelectorLevelOpen())
            return;
    }

    public void Return() {
        if (HasSelectorLevelOpen())
            return;
        PlayerPrefs.SetString(MenuManager.SHOULD_SET_RANDOM_BACKGROUND_KEY, MenuManager.TRUE);
        SceneManager.LoadScene("MenuScene");
    }

    public int GetLevelIndice() {
        return 0;
    }

    protected void PlaceCameraInFrontOfCurrentLevel() {
        SelectorCameraController cameraController = camera.GetComponent<SelectorCameraController>();
        Vector3 levelPos = currentSelectorLevel.transform.position;
        Vector3 posToGoTo = levelPos + currentSelectorLevel.transform.forward * cameraController.GetIdealDistanceFromLevel();
        cameraController.PlaceAt(posToGoTo);
    }

    protected void DisplayCurrentLevel() {
        SelectorLevel currentLevel = GetLastLevelSaved();
        if(currentLevel != null)
            DisplayLevel(currentLevel, instantDisplay:true);
    }

    public void RunPopup(string title, string text, TexteExplicatif.Theme theme) {
        popup.SetColorTheme(theme);
        popup.SetText(title, text);
        popup.Run();
    }
}
