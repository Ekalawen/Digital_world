using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuLevelSelector : MonoBehaviour {

    public static string LEVEL_INDICE_KEY = "levelIndiceKey";
    public static string LEVEL_INDICE_MUST_BE_USED_KEY = "levelIndiceMustBeUsedKey";

    public GameObject menuInitial;
    public List<GameObject> levels;
    public float dureeFading = 0.5f;
    public LoadingMenu loadingMenu;

    protected int levelIndice = 0;

    private void Update() {
        // Si on appui sur Echap on quitte
        if (!MenuManager.DISABLE_HOTKEYS) {
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

    public void SaveLevelIndice() {
        PlayerPrefs.SetInt(LEVEL_INDICE_KEY, levelIndice);
        PlayerPrefs.Save();
    }

    public void CleanLevelIndice() {
        PlayerPrefs.SetInt(LEVEL_INDICE_KEY, -1);
        PlayerPrefs.SetString(LEVEL_INDICE_MUST_BE_USED_KEY, "False");
        PlayerPrefs.Save();
    }

    public void Play(string levelSceneName) {
        MenuLevel menuLevel = levels[levelIndice].GetComponent<MenuLevel>();
        if (menuLevel.IsPlayStarted()) {// Pour éviter de lancer deux fois le play !
            return;
        }
        //SceneManager.LoadScene(levelSceneName, LoadSceneMode.Additive);
        AsyncOperation loading = SceneManager.LoadSceneAsync(levelSceneName);
        loading.allowSceneActivation = false;
        menuLevel.SetPlayStarted();
        SaveLevelIndice();
        PlayerPrefs.SetString(MenuLevel.LEVEL_NAME_KEY, menuLevel.GetName());

        StartCoroutine(FadeOutMenuLevel(menuLevel.gameObject));
        StartCoroutine(FadeInLoadingMenu(loading));
    }

    protected IEnumerator FadeOutMenuLevel(GameObject menuLevel) {
        Timer timer = new Timer(dureeFading);
        while(!timer.IsOver()) {
            menuLevel.GetComponent<CanvasGroup>().alpha = 1.0f - timer.GetAvancement();
            yield return null;
        }
        menuLevel.SetActive(false);
    }

    protected IEnumerator FadeInLoadingMenu(AsyncOperation loading) {
        Timer timer = new Timer(dureeFading);
        loadingMenu.gameObject.SetActive(true);
        loadingMenu.Initialize(loading);
        loadingMenu.GetComponent<CanvasGroup>().alpha = 0.0f;
        while(!timer.IsOver()) {
            loadingMenu.GetComponent<CanvasGroup>().alpha = timer.GetAvancement();
            yield return null;
        }
    }
}
