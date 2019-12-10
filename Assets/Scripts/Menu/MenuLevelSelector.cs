using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuLevelSelector : MonoBehaviour {

    public GameObject menuInitial;
    public MenuBackgroundBouncing menuBouncingBackground;
    public List<GameObject> levels;

    protected int levelIndice = 0;

    private void Update() {
		// Si on appui sur Echap on quitte
		if(Input.GetKeyDown(KeyCode.Escape)) {
            Back();
		}
        // Les cotes pour changer de niveau
		if(Input.GetKeyDown(KeyCode.RightArrow)) {
            Next();
		}
		if(Input.GetKeyDown(KeyCode.LeftArrow)) {
            Previous();
		}
    }

    public void Run() {
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
        float probaSource = Random.Range(0.00002f, 0.0035f);
        int distanceSource = Random.Range(1, 12);
        float decroissanceSource = Random.Range(0.002f, 0.02f);
        List<ColorSource.ThemeSource> themes = new List<ColorSource.ThemeSource>();
        int nbThemes = Random.Range(1, 4);
        for(int i = 0; i < nbThemes; i++)
            themes.Add(ColorManager.GetRandomTheme());
        menuBouncingBackground.SetParameters(probaSource, distanceSource, decroissanceSource, themes);
    }
}
