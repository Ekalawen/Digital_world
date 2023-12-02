using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuLevelSelector : MonoBehaviour {

    public GameObject menuInitial;
    public List<GameObject> levels;
    public float dureeFading = 0.5f;
    public LoadingMenu loadingMenu;

    protected int levelIndice = 0;

    private void Update() {
        // Si on appui sur Echap on quitte
        if (!MenuManager.DISABLE_HOTKEYS && !GetCurrentLevel().IsPlayStarted()) {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                Back();
            }
        }
    }

    public void Run(int indice = 0) {
        levelIndice = indice;
        menuInitial.SetActive(false);
        Play(levelIndice);
    }

    public void Next() {
        levelIndice = (levelIndice + 1) % levels.Count;
        Play(levelIndice);
    }

    public void Previous() {
        if (levelIndice == 0)
            return;
        levelIndice = (levelIndice == 0) ? levels.Count - 1 : levelIndice - 1;
        Play(levelIndice);
    }

    protected void Play(int indice) {
        for(int i = 0; i < levels.Count;i++) {
            levels[i].SetActive(i == indice);
        }
    }

    public void Back() {
        for(int i = 0; i < levels.Count;i++) {
            levels[i].SetActive(false);
        }
        menuInitial.SetActive(true);
        menuInitial.GetComponent<MenuManager>().SetRandomBackground();
    }

    public void SaveLevelName() {
        string levelName = menuInitial.GetComponent<MenuLevel>().GetNameId();
        PrefsManager.SetString(PrefsManager.SAVE_LEVEL_NAME, levelName);
        PrefsManager.Save();
    }

    public void CleanLevelIndice() {
        PrefsManager.DeleteKey(PrefsManager.SAVE_LEVEL_NAME);
        PrefsManager.SetBool(PrefsManager.SAVE_LEVEL_NAME_MUST_BE_USED, false);
        PrefsManager.Save();
    }

    public void Play(string levelSceneName) {
        MenuLevel menuLevel = GetCurrentLevel();
        if (menuLevel.IsPlayStarted()) { // Pour éviter de lancer deux fois le play !
            return;
        }
        //SceneManager.LoadScene(levelSceneName, LoadSceneMode.Additive);
        AsyncOperation loading = SceneManager.LoadSceneAsync(levelSceneName);
        loading.allowSceneActivation = false;
        menuLevel.SetPlayStarted();
        SaveLevelName();
        //PlayerPrefs.SetString(MenuLevel.LEVEL_NAME_ID_KEY, menuLevel.GetNameId());

        StartCoroutine(FadeOutMenuLevel(menuLevel.gameObject));
        StartCoroutine(FadeInLoadingMenu(loading, menuLevel));
    }

    protected IEnumerator FadeOutMenuLevel(GameObject menuLevel) {
        Timer timer = new Timer(dureeFading);
        while(!timer.IsOver()) {
            menuLevel.GetComponent<CanvasGroup>().alpha = 1.0f - timer.GetAvancement();
            yield return null;
        }
        menuLevel.SetActive(false);
    }

    protected IEnumerator FadeInLoadingMenu(AsyncOperation loading, MenuLevel level) {
        Timer timer = new Timer(dureeFading);
        loadingMenu.gameObject.SetActive(true);
        loadingMenu.Initialize(loading, level);
        loadingMenu.GetComponent<CanvasGroup>().alpha = 0.0f;
        while(!timer.IsOver()) {
            loadingMenu.GetComponent<CanvasGroup>().alpha = timer.GetAvancement();
            yield return null;
        }
    }

    protected MenuLevel GetCurrentLevel() {
        return levels[levelIndice].GetComponent<MenuLevel>();
    }

    public int GetLevelIndice() {
        return levelIndice;
    }
}
