using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuLevel : MonoBehaviour {

    public string levelSceneName;
    public MenuLevelSelector menuLevelSelector;
    public MenuBackgroundMethode3 menuBouncingBackground;

    // Les propriétés du background de ce level
    public float probaSource = 0.00035f; // La probabilité d'être une source
    public int distanceSource = 8; // La distance d'action de la source
    public float decroissanceSource = 0.01f; // La vitesse de décroissance de la source

    private void Update() {
		// Si on appui sur Echap on quitte
		if(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) {
            Play();
		}
    }

    private void OnEnable() {
        menuBouncingBackground.SetParameters(probaSource, distanceSource, decroissanceSource);
    }

    public void Play() {
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
