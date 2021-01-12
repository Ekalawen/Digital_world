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
    public SelectorPathUnlockScreen unlockScreen;
    public TexteExplicatif popup;

    [Header("Parameters")]
    public float dureeFading = 0.5f;

    protected List<SelectorLevel> levels;
    protected List<SelectorPath> paths;
    protected SelectorLevel currentSelectorLevel;
    protected bool hasLevelOpen = false;
    protected bool hasUnlockScreenOpen = false;
    protected Dictionary<GameObject, Coroutine> fadingObjects;

    void Awake() {
        if (!_instance) { _instance = this; }
    }

    public void Start() {
        fadingObjects = new Dictionary<GameObject, Coroutine>();
        GatherPaths();
        GatherLevels();
        background.Initialize();
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
        if (!HasSelectorLevelOpen() && !HasSelectorPathUnlockScreenOpen()) {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                Return();
            }
        }
    }

    public List<SelectorLevel> GetLevels() {
        return levels;
    }

    public List<SelectorPath> GetPaths() {
        return paths;
    }

    protected void GatherPaths() {
        paths = new List<SelectorPath>();
        foreach(Transform child in pathsFolder) {
            SelectorPath path = child.gameObject.GetComponent<SelectorPath>();
            if (path != null) {
                path.Initialize(unlockScreen);
                paths.Add(path);
            }
        }
    }

    protected void GatherLevels() {
        levels = new List<SelectorLevel>();
        foreach(Transform child in levelsFolder) {
            SelectorLevel level = child.gameObject.GetComponent<SelectorLevel>();
            if (level != null) {
                level.Initialize(background);
                levels.Add(level);
            }
        }
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

        FadeOut(menuLevel.gameObject, dureeFading);
        FadeInLoadingMenu(loading, menuLevel);
    }

    public Coroutine FadeIn(GameObject go, float dureeFading) {
        if(fadingObjects.ContainsKey(go)) {
            StopCoroutine(fadingObjects[go]);
            fadingObjects.Remove(go);
        }
        Coroutine coroutine = StartCoroutine(CFadeIn(go, dureeFading));
        fadingObjects[go] = coroutine;
        return coroutine;
    }
    protected IEnumerator CFadeIn(GameObject go, float dureeFading) {
        Timer timer = new Timer(dureeFading);
        go.SetActive(true);
        float startAlpha = go.GetComponent<CanvasGroup>().alpha;
        if (startAlpha == 1.0f)
            startAlpha = 0.0f;
        while(!timer.IsOver()) {
            go.GetComponent<CanvasGroup>().alpha = startAlpha + timer.GetAvancement() * (1.0f - startAlpha);
            yield return null;
        }
        go.GetComponent<CanvasGroup>().alpha = 1.0f;
    }

    public Coroutine FadeOut(GameObject go, float dureeFading) {
        if(fadingObjects.ContainsKey(go)) {
            StopCoroutine(fadingObjects[go]);
            fadingObjects.Remove(go);
        }
        Coroutine coroutine = StartCoroutine(CFadeOut(go, dureeFading));
        fadingObjects[go] = coroutine;
        return coroutine;
    }
    protected IEnumerator CFadeOut(GameObject go, float dureeFading) {
        Timer timer = new Timer(dureeFading);
        float startAlpha = go.GetComponent<CanvasGroup>().alpha;
        if (startAlpha == 0.0f)
            startAlpha = 1.0f;
        while(!timer.IsOver()) {
            go.GetComponent<CanvasGroup>().alpha = startAlpha - timer.GetAvancement() / startAlpha;
            yield return null;
        }
        go.GetComponent<CanvasGroup>().alpha = 0.0f;
        go.SetActive(false);
    }

    protected void FadeInLoadingMenu(AsyncOperation loading, MenuLevel level) {
        loadingMenu.gameObject.SetActive(true);
        loadingMenu.Initialize(loading, level);
        FadeIn(loadingMenu.gameObject, dureeFading);
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

    public SelectorLevel GetCurrentLevel() {
        return currentSelectorLevel;
    }

    public void TryDisplayLevel(SelectorLevel selectorLevel, bool instantDisplay = false) {
        if(IsLevelAccessible(selectorLevel)) {
            DisplayLevel(selectorLevel, instantDisplay);
            selectorLevel.menuLevel.HighlightBackButton(false);
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
        selectorLevel.menuLevel.Initialize();
        selectorLevel.DisplayInitialPopup();
        background.gameObject.SetActive(true);
        if (instantDisplay) {
            currentSelectorLevel.menuLevel.gameObject.SetActive(true);
            background.gameObject.SetActive(true);
        } else {
            FadeIn(currentSelectorLevel.menuLevel.gameObject, dureeFading);
            FadeIn(background.gameObject, dureeFading);
        }
    }

    public bool HasSelectorLevelOpen() {
        return hasLevelOpen;
    }

    public bool HasThisSelectorLevelOpen(SelectorLevel selectorLevel) {
        return hasLevelOpen && currentSelectorLevel == selectorLevel;
    }

    public bool HasSelectorPathUnlockScreenOpen() {
        return hasUnlockScreenOpen;
    }

    public void SetSelectorPathUnlockScreenOpenness(bool value) {
        hasUnlockScreenOpen = value;
    }

    public void BackToSelector() {
        if (!HasSelectorLevelOpen())
            return;
        hasLevelOpen = false;
        FadeOut(currentSelectorLevel.menuLevel.gameObject, dureeFading);
        FadeOut(background.gameObject, dureeFading);
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
        if (currentLevel != null) {
            DisplayLevel(currentLevel, instantDisplay: true);
            currentLevel.menuLevel.HighlightBackButtonBasedOnSave();
        }
    }

    public void RunPopup(string title, string text, TexteExplicatif.Theme theme, bool cleanReplacements = true) {
        popup.Initialize(title: title, mainText: text, theme: theme, cleanReplacements: cleanReplacements);
        popup.Run();
    }

    public bool PopupIsEnabled() {
        return popup.content.activeInHierarchy;
    }

    public bool IsLevelAccessible(SelectorLevel selectorLevel) {
        List<SelectorPath> precedentPaths = paths.FindAll(p => p.endLevel == selectorLevel);
        return precedentPaths.All(p => p.IsUnlocked());
    }

    public List<SelectorPath> GetOutPaths(SelectorLevel selectorLevel) {
        return paths.FindAll(p => p.startLevel == selectorLevel);
    }

    public List<SelectorPath> GetInPaths(SelectorLevel selectorLevel) {
        return paths.FindAll(p => p.endLevel == selectorLevel);
    }
}
