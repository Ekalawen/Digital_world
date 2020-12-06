using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
        if (level != null) {
            Console console = level.consolePrefab.GetComponent<Console>();
            string conseil = console.conseils[UnityEngine.Random.Range(0, console.conseils.Count - 1)];
            conseilText.text += conseil;
        } else { // Tutoriel !
            conseilText.text = "Initialisation de la Matrice dans 3 ... 2 ... 1 ... 0 !";
        }
    }

    protected void InitPouvoirs() {
        Player player = level.joueurPrefab.GetComponent<Player>();
        InitPouvoir(player.pouvoirAPrefab, pouvoirA);
        InitPouvoir(player.pouvoirEPrefab, pouvoirE);
        InitPouvoir(player.pouvoirLeftBoutonPrefab, pouvoirLeftClick);
        InitPouvoir(player.pouvoirRightBoutonPrefab, pouvoirRightClick);
    }

    protected void InitPouvoir(GameObject pouvoirPrefab, PouvoirDisplay display) {
        IPouvoir pouvoir = pouvoirPrefab ? pouvoirPrefab.GetComponent<IPouvoir>() : null;
        string nom = pouvoir ? pouvoir.nom : PouvoirDisplay.NULL_NAME_VALUE;
        string description = pouvoir ? pouvoir.description : PouvoirDisplay.NULL_DESCRIPTION_VALUE;
        Sprite sprite = pouvoir ? pouvoir.sprite : null;
        display.Initialize(nom, description, sprite);
    }

    protected void HidePouvoirs() {
        pouvoirA.gameObject.SetActive(false);
        pouvoirE.gameObject.SetActive(false);
        pouvoirLeftClick.gameObject.SetActive(false);
        pouvoirRightClick.gameObject.SetActive(false);
    }
}
