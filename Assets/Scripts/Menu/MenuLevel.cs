using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuLevel : MonoBehaviour {

    public static string LEVEL_NAME_KEY = "levelNameKey";

    public string levelSceneName;
    public MenuLevelSelector menuLevelSelector;
    public MenuBackgroundBouncing menuBouncingBackground;
    public Text textLevelName; 

    // Les propriétés du background de ce level
    public float probaSource = 0.00035f; // La probabilité d'être une source
    public int distanceSource = 8; // La distance d'action de la source
    public float decroissanceSource = 0.01f; // La vitesse de décroissance de la source
    public List<ColorSource.ThemeSource> themes; // Les couleurs des sources :)

    private void Update() {
		// Si on appui sur Echap on quitte
		if(!MenuManager.DISABLE_HOTKEYS && (Input.GetKeyDown(KeyCode.Return)
        || Input.GetKeyDown(KeyCode.KeypadEnter)
        || Input.GetKeyDown(KeyCode.Space))) {
            Play();
		}
    }

    private void OnEnable() {
        menuBouncingBackground.SetParameters(probaSource, distanceSource, decroissanceSource, themes);
    }

    public void Play() {
        menuLevelSelector.SaveLevelIndice();
        PlayerPrefs.SetString(LEVEL_NAME_KEY, textLevelName.text);
		SceneManager.LoadScene(levelSceneName);
    }

    public void Next() {
        menuLevelSelector.Next();
    }
    public void Previous() {
        menuLevelSelector.Previous();
    }
    public void Back() {
        menuLevelSelector.Back();
    }
}
