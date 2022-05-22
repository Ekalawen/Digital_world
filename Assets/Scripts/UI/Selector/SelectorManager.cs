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
    public Camera baseCamera;
    public Camera overlayCamera;
    public RectTransform screen;
    public MenuBackgroundBouncing background;
    public LoadingMenu loadingMenu;
    public SelectorPathUnlockScreen unlockScreen;
    public Transform collidersFolder;

    public TexteExplicatif popup;
    public TexteExplicatif popupArchives;
    public SelectorManagerStrings strings;
    public ReplacementStrings archivesReplacementStrings;
    public ReplacementStrings docReplacementStrings;
    public ReplacementStrings DHReplacementStrings;
    public SelectorLevelRunIntroduction introductionRunner;
    public TutorialTooltipManager tutorialTooltipManager;
    public VerticalMenuHandler verticalMenuHandler;
    public SelectorTarget selectorTarget;
    public GameObject achievementManagerPrefab;
    public EndGamesManager endGamesManager;

    [Header("Parameters")]
    public bool isDemo = false;
    public float dureeFading = 0.5f;
    public Vector3 compressionFactor = Vector3.one;

    protected List<SelectorLevel> levels;
    protected List<SelectorPath> paths;
    protected SelectorLevel currentSelectorLevel;
    protected SelectorPath currentSelectorPath = null;
    protected bool hasLevelOpen = false;
    protected bool hasUnlockScreenOpen = false;
    protected Dictionary<GameObject, Coroutine> fadingObjects;
    protected Coroutine backAndDisplayCoroutine;
    protected SelectorCameraController cameraController;
    [HideInInspector]
    public AchievementManager achievementManager;

    [HideInInspector]
    public UnityEvent<SelectorLevel> onDisplayLevel;
    [HideInInspector]
    public UnityEvent<SelectorPath> onDisplayPath;
    [HideInInspector]
    public UnityEvent<SelectorPath> onUnlockPath;
    [HideInInspector]
    public UnityEvent<SelectorPath> onOpenDHPath;
    [HideInInspector]
    public UnityEvent<SelectorPath> onCloseDHPath;
    [HideInInspector]
    public UnityEvent<SelectorPath> onNextLevelFrompath;
    [HideInInspector]
    public UnityEvent<SelectorLevel> onOpenDoc;

    void Awake() {
        if (!_instance) { _instance = this; }
    }

    public IEnumerator Start()
    {
        fadingObjects = new Dictionary<GameObject, Coroutine>();
        Application.targetFrameRate = -1;
        cameraController = baseCamera.GetComponentInParent<SelectorCameraController>();
        InputManager.Instance.SetNotInGame();
        StartMenuMusic();
        verticalMenuHandler.Initialize();
        selectorTarget.Initialize();
        endGamesManager.Initialize();
        GatherLevels();
        GatherPaths();
        ApplyCompressionFactor();
        //background.Initialize();
        //background.SetParameters(0, 0, 0, new List<ColorManager.Theme>() { ColorManager.Theme.ROUGE });
        background.gameObject.SetActive(false);
        SetCurrentLevelBasedOnLastSavedLevel();
        cameraController.PlaceCameraInFrontOfCurrentLevel();
        // StartMenuMusic(); // Old position ! :)
        if (!LocalizationSettings.InitializationOperation.IsDone)
        {
            yield return LocalizationSettings.InitializationOperation;
        }
        InitializePaths();
        InitializeLevels();
        InitializeAchievementManager();
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
        if (!HasSelectorLevelOpen() && !HasUnlockScreenOpen()) {
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
        GatherPathsInFolder(pathsFolder);
    }

    protected void GatherPathsInFolder(Transform folder) {
        if(folder.name == "NotVisiblePaths") {
            return;
        }
        foreach(Transform child in folder) {
            SelectorPath path = child.gameObject.GetComponent<SelectorPath>();
            if (path != null) {
                paths.Add(path);
            } else {
                GatherPathsInFolder(child);
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
        GatherLevelsInFolder(levelsFolder);
    }

    protected void GatherLevelsInFolder(Transform folder) {
        if(folder.name == "NotVisibleLevels" || folder.name == "Colliders") {
            return;
        }
        foreach(Transform child in folder) {
            SelectorLevel level = child.gameObject.GetComponent<SelectorLevel>();
            if (level != null) {
                levels.Add(level);
            } else {
                GatherLevelsInFolder(child);
            }
        }
    }

    public SelectorPath GetCurrentPath() {
        return currentSelectorPath;
    }

    protected void InitializeLevels() {
        foreach(SelectorLevel level in levels) {
            level.Initialize(background);
            level.menuLevel.onOpenDoc.AddListener(onOpenDoc.Invoke);
        }
    }

    protected void InitializeAchievementManager() {
        achievementManager = Instantiate(achievementManagerPrefab, parent: transform).GetComponent<AchievementManager>();
        achievementManager.Initialize(isInGame: false);
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

        //verticalMenuHandler.Close();
        //menuLevel.gameObject.SetActive(false);
        //FadeOut(menuLevel.gameObject, dureeFading);
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
        if(IsLevelAccessible(selectorLevel) && CanUnlockInDemo(selectorLevel)) {
            if(HasVerticalMenuOpen()) {
                if (HasSelectorLevelOpen() && GetCurrentLevel() == selectorLevel) {
                    return;
                } else {
                    BackAndDisplayLevel(selectorLevel, instantDisplay);
                }
            } else {
                DisplayLevel(selectorLevel, instantDisplay);
                cameraController.PlaceCameraInFrontOfCurrentLevel();
                selectorTarget.GoTo(selectorLevel.transform.position, selectorTarget.GetInTime());
            }
        } else {
            if(IsLevelAccessible(selectorLevel) && !CanUnlockInDemo(selectorLevel)) {
                SubmitDemoDenied();
            } else {
                DisplayLevelDenied(selectorLevel);
            }
        }
    }

    protected void DisplayLevelDenied(SelectorLevel selectorLevel) {
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

    protected void SubmitDemoDenied() {
        popup.Initialize(
            title: strings.pathDemoDeniedTitre.GetLocalizedString().Result,
            mainText: strings.pathDemoDeniedTexte.LoadAssetAsync().Result.text,
            theme: TexteExplicatif.Theme.NEUTRAL);
        popup.Run();
        popup.AddButton(
            text: strings.buttonWishlistOnSteam,
            tooltipText: strings.buttonWishlistOnSteamTooltip,
            theme: TexteExplicatif.Theme.POSITIF,
            action: new UnityAction(WishlistOnSteam));
        popup.EnableButtonsBlackBackground();
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
        selectorLevel.DisplayInitialPopupIn(verticalMenuHandler.openTime);
        selectorLevel.menuLevel.Initialize(); // menuLevel.Initialize() doit être après DisplayInitialPopup() pour que l'on sache si il faut highliter les fastUI ou pas :)
        background.gameObject.SetActive(true);
        onDisplayLevel.Invoke(selectorLevel);
        //if (instantDisplay) {
        //    currentSelectorLevel.menuLevel.gameObject.SetActive(true);
        //    currentSelectorLevel.menuLevel.gameObject.GetComponent<CanvasGroup>().alpha = 1.0f;
        //    background.gameObject.SetActive(true);
        //} else {
        //    FadeIn(currentSelectorLevel.menuLevel.gameObject, dureeFading);
        //    FadeIn(background.gameObject, dureeFading);
        //}
        currentSelectorLevel.menuLevel.gameObject.SetActive(true);
        background.gameObject.SetActive(true);
        verticalMenuHandler.Open(instantOpen: instantDisplay);
    }

    public bool HasVerticalMenuOpen() {
        return verticalMenuHandler.IsOpen();
    }

    public bool HasSelectorLevelOpen() {
        return hasLevelOpen;
    }

    public bool HasThisSelectorLevelOpen(SelectorLevel selectorLevel) {
        return hasLevelOpen && currentSelectorLevel == selectorLevel;
    }

    public bool HasUnlockScreenOpen() {
        return hasUnlockScreenOpen;
    }

    public void SetCurrentUnlockScreen(bool value, SelectorPath path) {
        hasUnlockScreenOpen = value;
        currentSelectorPath = path;
        if(value) {
            onDisplayPath.Invoke(path);
        }
    }

    public void BackAndDisplayLevel(SelectorLevel selectorLevel, bool instantDisplay) {
        StopBackAndDisplayCoroutine();
        backAndDisplayCoroutine = StartCoroutine(CBackAndDisplayLevel(selectorLevel, instantDisplay));
    }

    public void BackAndDisplayUnlockScreen(SelectorPath selectorPath, bool instantDisplay) {
        StopBackAndDisplayCoroutine();
        backAndDisplayCoroutine = StartCoroutine(CBackAndDisplayUnlockScreen(selectorPath, instantDisplay));
    }
    
    protected void StopBackAndDisplayCoroutine() {
        if(backAndDisplayCoroutine != null) {
            StopCoroutine(backAndDisplayCoroutine);
        }
    }

    public IEnumerator CBackAndDisplayLevel(SelectorLevel selectorLevel, bool instantDisplay) {
        BackToSelector(willReopen: true);
        hasLevelOpen = true; // Usefull while we are closing the VerticalMenu
        currentSelectorLevel = selectorLevel; // Same
        SetCurrentUnlockScreen(false, null);
        cameraController.PlaceCameraInFrontOfCurrentLevel();
        selectorTarget.GoTo(selectorLevel.transform.position, selectorTarget.GetMovingTime());
        yield return new WaitForSeconds(verticalMenuHandler.closeTime);
        DisplayLevel(selectorLevel, instantDisplay);
    }

    public IEnumerator CBackAndDisplayUnlockScreen(SelectorPath selectorPath, bool instantDisplay) {
        BackToSelector(willReopen: true);
        hasLevelOpen = false; // Usefull while we are closing the VerticalMenu
        hasUnlockScreenOpen = true;
        currentSelectorPath = selectorPath;
        cameraController.PlaceCameraInFrontOfPath(selectorPath);
        yield return new WaitForSeconds(verticalMenuHandler.closeTime);
        selectorPath.OpenUnlockScreen(instantDisplay);
        SetCurrentUnlockScreen(true, selectorPath);
    }

    public void BackToSelector(bool instantBack = false, bool willReopen = false) {
        if (!HasSelectorLevelOpen() && !HasUnlockScreenOpen())
            return;
        if(instantBack) {
            currentSelectorLevel.menuLevel.gameObject.SetActive(false);
            background.gameObject.SetActive(false);
            unlockScreen.gameObject.SetActive(false);
        } else {
            DisableIn(currentSelectorLevel.menuLevel.gameObject, verticalMenuHandler.closeTime);
            DisableIn(background.gameObject, verticalMenuHandler.closeTime);
            DisableIn(unlockScreen.gameObject, verticalMenuHandler.closeTime);
        }
        hasLevelOpen = false;
        SetCurrentUnlockScreen(false, null);
        if (!willReopen) {
            selectorTarget.Shrink(selectorTarget.GetOutTime());
        } else {
            verticalMenuHandler.FixPercentageValueFor(verticalMenuHandler.closeTime + verticalMenuHandler.openTime);
        }
        verticalMenuHandler.Close(instantClose: instantBack);
    }

    public void DisableIn(GameObject go, float duration) {
        StartCoroutine(CDisableIn(go, duration));
    }

    public IEnumerator CDisableIn(GameObject go, float duration) {
        yield return new WaitForSeconds(duration);
        go.SetActive(false);
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
        //if (HasSelectorLevelOpen())
        //    return;
        PrefsManager.SetBool(PrefsManager.SHOULD_SET_RANDOM_BACKGROUND_KEY, true);
        SceneManager.LoadScene("MenuScene");
    }

    public int GetLevelIndice(SelectorLevel level) {
        return levels.FindIndex(l => l == level);
    }

    public int GetPathIndice(SelectorPath path) {
        return paths.FindIndex(p => p == path);
    }

    protected void DisplayCurrentLevel() {
        SelectorLevel currentLevel = GetLastLevelSaved();
        if (currentLevel != null) {
            DisplayLevel(currentLevel, instantDisplay: true);
            selectorTarget.GoTo(currentLevel.transform.position, selectorTarget.GetInTime());
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
        return popup.content.activeInHierarchy || popupArchives.content.activeInHierarchy;
    }

    public bool IsLevelAccessible(SelectorLevel selectorLevel) {
        List<SelectorPath> precedentPaths = paths.FindAll(p => p.endLevel == selectorLevel);
        return precedentPaths.All(p => p.IsUnlocked());
    }

    public bool CanUnlockInDemo(SelectorLevel selectorLevel) {
        List<SelectorPath> precedentPaths = paths.FindAll(p => p.endLevel == selectorLevel);
        return precedentPaths.All(p => p.CanUnlockInDemo());
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

    public string GetUnitesString(int nbUnite, MenuLevel.LevelType levelType, SelectorPath path) {
        if (levelType == MenuLevel.LevelType.INFINITE) {
            return strings.blocs.GetLocalizedString(nbUnite).Result;
        } else {
            int nbVictoiresNeeded = path.GetComponent<NoNeedToWinDataHackees>() == null ? 1 : 0;
            if (nbUnite != 0) {
                return strings.victoires.GetLocalizedString(nbVictoiresNeeded, nbUnite).Result;
            } else {
                return strings.victoiresZero.GetLocalizedString(nbVictoiresNeeded).Result;
            }
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

    public static void WishlistOnSteam() {
        Debug.Log($"Wishlist on Steam ! :)");
        Application.OpenURL("https://store.steampowered.com/app/1734990");
    }

    public SelectorCameraController GetCameraController() {
        return cameraController;
    }

    protected void ApplyCompressionFactor() {
        foreach (SelectorLevel level in levels) {
            level.transform.position = MathTools.VecMul(level.transform.position, compressionFactor);
        }
        foreach (Transform collider in collidersFolder) {
            CapsuleCollider capsule = collider.GetComponent<CapsuleCollider>();
            capsule.height *= compressionFactor.y;
            Vector3 position = capsule.transform.position;
            position.y *= compressionFactor.y;
            capsule.transform.position = position;
        }
        foreach(SelectorPath path in paths) {
            foreach(GameObject point in path.intermediatePoints) {
                point.transform.position = MathTools.VecMul(point.transform.position, compressionFactor);
            }
        }
    }

    public void PlayArchivesClip() {
        //if (hasLevelOpen) { // J'ai besoin de décommenter ça car sinon lors de l'Introduction quand on ouvre le SelecteurDeNiveau pour la première fois ça ne fonctionnera pas (car aucun niveau n'est ouvert !)
            UISoundManager.Instance.UnPauseArchivesClip();
        //}
    }

    public void PauseArchivesClip() {
        //if (hasLevelOpen) { // J'ai besoin de décommenter ça car sinon lors de l'Introduction quand on ouvre le SelecteurDeNiveau pour la première fois ça ne fonctionnera pas (car aucun niveau n'est ouvert !)
            UISoundManager.Instance.PauseArchivesClip();
        //}
    }
}
