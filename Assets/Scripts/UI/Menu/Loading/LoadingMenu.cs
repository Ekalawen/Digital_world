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

        while (!Input.anyKeyDown) {
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
            LocalizedString conseil = console.conseils[UnityEngine.Random.Range(0, console.conseils.Count)];
            AsyncOperationHandle<string> handle = conseil.GetLocalizedString();
            yield return handle.Task;
            conseilText.text += handle.Result;
        } else { // Tutoriel !
            AsyncOperationHandle<string> handle = tutorielInitialisationMatrice.GetLocalizedString();
            yield return handle;
            conseilText.text = handle.Result;
        }
    }

    protected void InitPouvoirs() {
        Player player = level.joueurPrefab.GetComponent<Player>();
        InitPouvoir(player.pouvoirAPrefab, pouvoirA, pouvoirA.keyName.GetLocalizedString().Result);
        InitPouvoir(player.pouvoirEPrefab, pouvoirE, pouvoirE.keyName.GetLocalizedString().Result);
        InitPouvoir(player.pouvoirLeftBoutonPrefab, pouvoirLeftClick, pouvoirLeftClick.keyName.GetLocalizedString().Result);
        InitPouvoir(player.pouvoirRightBoutonPrefab, pouvoirRightClick, pouvoirRightClick.keyName.GetLocalizedString().Result);
    }

    protected void InitPouvoir(GameObject pouvoirPrefab, PouvoirDisplay display, string keyName) {
        IPouvoir pouvoir = pouvoirPrefab ? pouvoirPrefab.GetComponent<IPouvoir>() : null;
        string nom = pouvoir ? pouvoir.nom.GetLocalizedString().Result : PouvoirDisplay.GetNullName();
        string levelToUnlockPouvoirName = display.levelToUnlockPouvoirName.GetLocalizedString().Result;
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
