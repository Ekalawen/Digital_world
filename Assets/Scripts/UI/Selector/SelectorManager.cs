using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectorManager : MonoBehaviour {

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
    public SelectorManagerStrings strings;
    public ReplacementStrings archivesReplacementStrings;
    public ReplacementStrings docReplacementStrings;
    public SelectorLevelRunIntroduction introductionRunner;
    public TutorialTooltipManager tutorialTooltipManager;

    [Header("Parameters")]
    public float dureeFading = 0.5f;

    protected List<SelectorLevel> levels;
    protected List<SelectorPath> paths;
    protected SelectorLevel currentSelectorLevel;
    protected bool hasLevelOpen = false;
    protected bool hasUnlockScreenOpen = false;
    protected Dictionary<GameObject, Coroutine> fadingObjects;

    [HideInInspector]
    public UnityEvent<SelectorLevel> onDisplayLevel;
    [HideInInspector]
    public UnityEvent<SelectorPath> onDisplayPath;
    [HideInInspector]
    public UnityEvent<SelectorPath> onUnlockPath;
    [HideInInspector]
    public UnityEvent<SelectorPath> onOpenDHPath;
    [HideInInspector]
    public UnityEvent<SelectorPath> onNextLevelFrompath;

    void Awake() {
        if (!_instance) { _instance = this; }
    }

    public IEnumerator Start() {
        fadingObjects = new Dictionary<GameObject, Coroutine>();
        onDisplayLevel = new UnityEvent<SelectorLevel>();
        onDisplayPath = new UnityEvent<SelectorPath>();
        onUnlockPath = new UnityEvent<SelectorPath>();
        onOpenDHPath = new UnityEvent<SelectorPath>();
        onNextLevelFrompath = new UnityEvent<SelectorPath>();
        GatherPaths();
        GatherLevels();
        background.Initialize();
        background.gameObject.SetActive(false);
        SetCurrentLevelBasedOnLastSavedLevel();
        PlaceCameraInFrontOfCurrentLevel();
        StartMenuMusic();
        if (!LocalizationSettings.InitializationOperation.IsDone) {
            yield return LocalizationSettings.InitializationOperation;
        }
        InitializePaths();
        InitializeLevels();
        tutorialTooltipManager.Initialize(this);
        DisplayCurrentLevel();
        CleanLastSavedLevel();
        DisplayIntroductionText();
    }

    protected void CleanLastSavedLevel() {
        PrefsManager.DeleteKey(PrefsManager.LAST_LEVEL_KEY);
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
                paths.Add(path);
            }
        }
    }

    protected void InitializePaths() {
        foreach(SelectorPath path in paths) {
            path.Initialize(unlockScreen);
        }
    }

    protected void GatherLevels() {
        levels = new List<SelectorLevel>();
        foreach(Transform child in levelsFolder) {
            SelectorLevel level = child.gameObject.GetComponent<SelectorLevel>();
            if (level != null) {
                levels.Add(level);
            }
        }
    }

    protected void InitializeLevels() {
        foreach(SelectorLevel level in levels) {
            level.Initialize(background);
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
        //PlayerPrefs.SetString(MenuLevel.LEVEL_NAME_ID_KEY, menuLevel.GetNameId());

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
        PrefsManager.SetString(PrefsManager.LAST_LEVEL_KEY, GetCurrentLevel().menuLevel.levelSceneName);
        PlayerPrefs.Save();
    }

    protected SelectorLevel GetLastLevelSaved() {
        if (PrefsManager.HasKey(PrefsManager.LAST_LEVEL_KEY)) {
            string levelSceneName = PrefsManager.GetString(PrefsManager.LAST_LEVEL_KEY, "");
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
        } else {
            string dataHackeesTitre = strings.dataHackees.GetLocalizedString().Result;
            popup.AddReplacement(dataHackeesTitre, UIHelper.SurroundWithColor(dataHackeesTitre, UIHelper.ORANGE));
            RunPopup(strings.niveauVerouilleTitre.GetLocalizedString().Result,
                strings.niveauVerouilleTotalTexte.GetLocalizedString(GetNiveauxManquantToLevelString(selectorLevel)).Result,
                //"Niveau vérouillé.\n" +
                //"Vous devez débloquer toutes les Data Hackées() menant à ce niveau pour le dévérouiller !\n" +
                //$"{GetNiveauxManquantToLevelString(selectorLevel)}",
                TexteExplicatif.Theme.NEGATIF,
                cleanReplacements: false);
        }
    }

    protected string GetNiveauxManquantToLevelString(SelectorLevel selectorLevel) {
        List<string> levelsName = paths.FindAll(p => p.endLevel == selectorLevel && !p.IsUnlocked()).Select(p => p.startLevel.GetVisibleName()).ToList();
        if(levelsName.Count == 1) {
            string name = UIHelper.SurroundWithColor(levelsName[0], UIHelper.BLUE);
            //return $"Il vous faut déverrouiller les Data Hackées() venant du niveau {name} !";
            return strings.niveauVerouilleUnSeulNiveau.GetLocalizedString(name).Result;
        }
        string res = "";
        foreach(string levelName in levelsName) {
            string name = UIHelper.SurroundWithColor(levelName, UIHelper.BLUE);
            res = $"{res}\n- {name}";
        }
        return strings.niveauVerouilleUnNiveauParmiPlusieurs.GetLocalizedString(res).Result;
    }

    public void DisplayLevel(SelectorLevel selectorLevel, bool instantDisplay = false) {
        currentSelectorLevel = selectorLevel;
        hasLevelOpen = true;
        selectorLevel.menuLevel.gameObject.SetActive(true);
        selectorLevel.DisplayInitialPopup();
        selectorLevel.menuLevel.Initialize(); // menuLevel.Initialize() doit être après DisplayInitialPopup() pour que l'on sache si il faut highliter les fastUI ou pas :)
        background.gameObject.SetActive(true);
        onDisplayLevel.Invoke(selectorLevel);
        if (instantDisplay) {
            currentSelectorLevel.menuLevel.gameObject.SetActive(true);
            currentSelectorLevel.menuLevel.gameObject.GetComponent<CanvasGroup>().alpha = 1.0f;
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

    public void SetSelectorPathUnlockScreenOpenness(bool value, SelectorPath path) {
        hasUnlockScreenOpen = value;
        if(value) {
            onDisplayPath.Invoke(path);
        }
    }

    public void BackToSelector() {
        if (!HasSelectorLevelOpen())
            return;
        hasLevelOpen = false;
        FadeOut(currentSelectorLevel.menuLevel.gameObject, dureeFading);
        FadeOut(background.gameObject, dureeFading);
    }

    public void BackToSelectorForFastUI() {
        if (!HasSelectorLevelOpen())
            return;
        hasLevelOpen = false;
        currentSelectorLevel.menuLevel.gameObject.SetActive(false);
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
        PrefsManager.SetBool(PrefsManager.SHOULD_SET_RANDOM_BACKGROUND_KEY, true);
        SceneManager.LoadScene("MenuScene");
    }

    public int GetLevelIndice(SelectorLevel level) {
        return levels.FindIndex(l => l == level);
    }

    public int GetPathIndice(SelectorPath path) {
        return paths.FindIndex(p => p == path);
    }

    public void PlaceCameraInFrontOfCurrentLevel() {
        PlaceCameraInFrontOfInterestTransform(currentSelectorLevel.transform);
    }

    public void PlaceCameraInFrontOfInterestTransform(Transform t) {
        PlaceCameraInFrontOfInterestPoint(t.position, t.forward);
    }

    public void PlaceCameraInFrontOfInterestPoint(Vector3 point) {
        Vector3 forward = GetForwardFromCenterProjection(point);
        PlaceCameraInFrontOfInterestPoint(point, forward);
    }

    protected void PlaceCameraInFrontOfInterestPoint(Vector3 interestPos, Vector3 interestForward) {
        SelectorCameraController cameraController = camera.GetComponent<SelectorCameraController>();
        Vector3 posToGoTo = interestPos + interestForward * cameraController.GetIdealDistanceFromLevel();
        cameraController.PlaceAt(posToGoTo);
        cameraController.transform.LookAt(interestPos, Vector3.up);
    }

    protected static Vector3 GetForwardFromCenterProjection(Vector3 interestPos) {
        Vector3 forward;
        if (interestPos.x == 0.0f && interestPos.z == 0.0f) {
            forward = Vector3.forward;
        } else {
            Vector3 projection = Vector3.Project(interestPos, Vector3.up);
            Vector3 orthoToCenter = interestPos - projection;
            forward = orthoToCenter.normalized;
        }

        return forward;
    }

    protected void DisplayCurrentLevel() {
        SelectorLevel currentLevel = GetLastLevelSaved();
        if (currentLevel != null) {
            DisplayLevel(currentLevel, instantDisplay: true);
        }
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

    public SelectorLevel GetLevelFromMenuLevel(MenuLevel menuLevel) {
        return levels.Find(l => l.menuLevel == menuLevel);
    }

    protected void DisplayIntroductionText() {
        if (!PrefsManager.GetBool(PrefsManager.FIRST_TIME_SELECTOR_OPENED_KEY, false)) {
            MenuLevel firstLevel = levels[0].menuLevel;
            introductionRunner.RunIntroduction();
            PrefsManager.SetBool(PrefsManager.FIRST_TIME_SELECTOR_OPENED_KEY, true);
        }
    }

    protected void FakeLoadAllPasses() {
        StartCoroutine(CFakeLoadAllPasses());
    }

    protected IEnumerator CFakeLoadAllPasses() {
        AsyncOperationHandle<StringTable> handle = LocalizationSettings.StringDatabase.GetTableAsync("Passes");
        yield return handle;
        StringTable passesTable = handle.Result;
        foreach (KeyValuePair<long, StringTableEntry> entry in passesTable) {
            entry.Value.GetLocalizedString();
        }
    }

    public string GetUnitesString(int nbUnite, MenuLevel.LevelType levelType) {
        if (levelType == MenuLevel.LevelType.INFINITE) {
            return strings.blocs.GetLocalizedString(nbUnite).Result;
        } else {
            if(nbUnite != 0)
                return strings.victoires.GetLocalizedString(nbUnite).Result;
            else
                return strings.victoiresZero.GetLocalizedString().Result;
        }
    }

    protected void StartMenuMusic() {
        UISoundManager.Instance.StartMusic();
    }

    public void ResetAllLevelsScores() {
        foreach(SelectorLevel level in levels) {
            level.ResetScores();
        }
        Debug.Log($"Tous les scores ont été réinitialisés.");
    }

    public void SetAllScoresToMaxTresholds() {
        foreach(SelectorLevel level in levels) {
            level.SetScoresToMaxTreshold();
        }
        Debug.Log($"Tous les scores ont maximisés.");
    }

    public void UnlockAllPaths() {
        foreach(SelectorPath path in paths) {
            path.UnlockPath();
        }
        Debug.Log($"Tous les Paths ont été débloqués ! :)");
    }

    public void LockAllPaths() {
        foreach(SelectorPath path in paths) {
            path.LockPath();
        }
        Debug.Log($"Tous les Paths ont été lockés ! :)");
    }

    public void NotifyControllerPlugIn() {
        string controllerName = InputManager.Instance.GetJoystickName();
        string texteString = strings.controllerPlugInTexte.GetLocalizedString(controllerName).Result;
        RunPopup(strings.controllerPlugInTitle.GetLocalizedString().Result, texteString, TexteExplicatif.Theme.NEUTRAL);
    }

    public void NotifyControllerPlugOut() {
        string keybindingName = InputManager.Instance.GetDefaultKeybindingType().ToString();
        string texteString = strings.controllerPlugOutTexte.GetLocalizedString(keybindingName).Result;
        RunPopup(strings.controllerPlugOutTitle.GetLocalizedString().Result, texteString, TexteExplicatif.Theme.NEUTRAL);
    }
}
