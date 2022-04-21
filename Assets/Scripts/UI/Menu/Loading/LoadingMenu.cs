using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingMenu : MonoBehaviour {

    public GameObject fromMenu;
    public GameObject appuyezSurUneToucheTexte;
    public TMPro.TMP_Text conseilText;
    public PouvoirDisplay pouvoirA;
    public PouvoirDisplay pouvoirE;
    public PouvoirDisplay pouvoirLeftClick;
    public PouvoirDisplay pouvoirRightClick;
    public LocalizedString tutorielInitialisationMatrice;

    protected AsyncOperation loading;
    protected MenuLevel level;

    public void Initialize(AsyncOperation loading, MenuLevel level) {
        this.loading = loading;
        this.level = level;
        InitAppuyezSurUneTouche();
        InitConseil();
        InitPouvoirs();
        StartCoroutine(CConfirmPlay());
    }

    public void InitializeWithTutoriel(AsyncOperation loading) {
        this.loading = loading;
        this.level = null;
        InitAppuyezSurUneTouche();
        InitConseil();
        HidePouvoirs();
        StartCoroutine(CConfirmPlay());
    }

    protected IEnumerator CConfirmPlay() {
        while (loading.progress <= 0.89f) { // loading.isDone ne fonctionne pas !!! x)
            yield return null;
        }
        appuyezSurUneToucheTexte.SetActive(true);

        while (!InputManager.Instance.GetAnyKeyOrButtonDown()) {
            yield return null;
        }

        //if (Input.GetKeyDown(KeyCode.Escape)) {
        //    SceneManager.UnloadSceneAsync(GetSceneName());
        //    Resources.UnloadUnusedAssets();
        //    fromMenu.SetActive(true);
        //    gameObject.SetActive(false);
        //} else {
        loading.allowSceneActivation = true;
        //}
    }

    protected string GetSceneName() {
        if(level == null) {
            return "TutorialScene";
        } else {
            return level.levelSceneName;
        }
    }

    protected void InitAppuyezSurUneTouche() {
        appuyezSurUneToucheTexte.SetActive(false);
    }

    protected void InitConseil() {
        StartCoroutine(CInitConseil());
    }

    protected IEnumerator CInitConseil() {
        if (level != null) {
            Console console = level.consolePrefab.GetComponent<Console>();
            string conseilKey = level.GetKey(PrefsManager.CONSEIL_INDICE_KEY);
            int conseilIndice = PrefsManager.GetInt(conseilKey, 0);
            PrefsManager.SetInt(conseilKey, (conseilIndice + 1) % console.conseils.Count);
            yield return console.CComputeConseil(conseilIndice);
            conseilText.text += console.computedConseil;
        } else { // Tutoriel !
            AsyncOperationHandle<string> handle = tutorielInitialisationMatrice.GetLocalizedString();
            yield return handle;
            conseilText.text = handle.Result;
        }
    }

    protected void InitPouvoirs() {
        Player player = level.joueurPrefab.GetComponent<Player>();
        InitPouvoir(player.pouvoirAPrefab, pouvoirA, pouvoirA.GetKeyName());
        InitPouvoir(player.pouvoirEPrefab, pouvoirE, pouvoirE.GetKeyName());
        InitPouvoir(player.pouvoirLeftBoutonPrefab, pouvoirLeftClick, pouvoirLeftClick.GetKeyName());
        InitPouvoir(player.pouvoirRightBoutonPrefab, pouvoirRightClick, pouvoirRightClick.GetKeyName());
    }

    protected void InitPouvoir(GameObject pouvoirPrefab, PouvoirDisplay display, string keyName) {
        IPouvoir pouvoir = pouvoirPrefab ? pouvoirPrefab.GetComponent<IPouvoir>() : null;
        string nom = pouvoir ? pouvoir.nom.GetLocalizedString().Result : PouvoirDisplay.GetNullName();
        string levelToUnlockPouvoirName = UIHelper.SurroundWithColorWithoutB(display.GetLevelToUnlockPouvoirName(), UIHelper.BLUE);
        string description = pouvoir ? pouvoir.description.GetLocalizedString().Result : PouvoirDisplay.GetNullDescription(levelToUnlockPouvoirName);
        Sprite sprite = pouvoir ? pouvoir.sprite : null;
        PouvoirDisplay.PouvoirType pouvoirType = pouvoir ? pouvoir.pouvoirType : PouvoirDisplay.PouvoirType.DEFAULT;
        display.Initialize(nom, keyName, description, sprite, pouvoirType);
    }

    protected void HidePouvoirs() {
        pouvoirA.gameObject.SetActive(false);
        pouvoirE.gameObject.SetActive(false);
        pouvoirLeftClick.gameObject.SetActive(false);
        pouvoirRightClick.gameObject.SetActive(false);
    }
}
