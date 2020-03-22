using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingMenu : MonoBehaviour {

    public GameObject appuyezSurUneToucheTexte;
    public Text conseilText;
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

    protected IEnumerator CConfirmPlay() {
        while (loading.progress <= 0.89f) { // loading.isDone ne fonctionne pas !!! x)
            yield return null;
        }
        appuyezSurUneToucheTexte.SetActive(true);

        while (!Input.anyKeyDown) {
            yield return null;
        }

        loading.allowSceneActivation = true;
    }

    protected void InitAppuyezSurUneTouche() {
        appuyezSurUneToucheTexte.SetActive(false);
    }

    protected void InitConseil() {
        Console console = level.consolePrefab.GetComponent<Console>();
        string conseil = console.conseils[UnityEngine.Random.Range(0, console.conseils.Count - 1)];
        conseilText.text += conseil;
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
        string nom = pouvoir ? pouvoir.nom : "404 Not Found";
        string description = pouvoir ? pouvoir.description : "Null";
        Sprite sprite = pouvoir ? pouvoir.sprite : null;
        display.Initialize(nom, description, sprite);
    }
}
