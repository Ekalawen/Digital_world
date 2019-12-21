using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuLevelSelector : MonoBehaviour {

    public static string LEVEL_INDICE_KEY = "levelIndiceKey";
    public static string LEVEL_INDICE_MUST_BE_USED_KEY = "levelIndiceMustBeUsedKey";

    public GameObject menuInitial;
    public List<GameObject> levels;

    protected int levelIndice = 0;

    private void Update() {
        // Si on appui sur Echap on quitte
        if (!MenuManager.DISABLE_HOTKEYS) {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                Back();
            }
            // Les cotes pour changer de niveau
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) {
                Next();
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.Q)) {
                Previous();
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
}
