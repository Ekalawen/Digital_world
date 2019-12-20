using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour {

    public MenuLevelSelector menuLevelSelector;
    public MenuOptions menuOptions;
    public MenuBackgroundBouncing menuBouncingBackground;

	void Update() {
		// Si on appui sur Echap on quitte
		if(Input.GetKeyDown(KeyCode.Escape)) {
			OnQuitterPress();
		}
		// Si on appui sur Entrée, on joue !
		if(Input.GetKeyDown(KeyCode.Return)
        || Input.GetKeyDown(KeyCode.KeypadEnter)
        || Input.GetKeyDown(KeyCode.Space)) {
			OnPlayPress();
		}
	}

	// Lorsqu'on appui sur le bouton jouer
	public void OnPlayPress() {
		Debug.Log("On a appuyé sur Play !");
        menuLevelSelector.Run();
	}

	// Lorsqu'on appui sur le bouton tutoriel
	public void OnTutorialPress() {
		Debug.Log("On a appuyé sur Tutoriel !");
		SceneManager.LoadScene("TutorialScene");
	}

	// Lorsqu'on appui sur le bouton options
	public void OnOptionPress() {
		Debug.Log("On a appuyé sur Option !");
        menuOptions.Run();
	}

	// Lorsqu'on appui sur le bouton quitter
	public void OnQuitterPress() {
		Debug.Log("On a appuyé sur Quitter !");
		QuitGame();
	}

	public void QuitGame()
	{
		// save any game data here
		#if UNITY_EDITOR
			// Application.Quit() does not work in the editor so
			// UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
			UnityEditor.EditorApplication.isPlaying = false;
		#else
			Application.Quit();
		#endif
	}

    public void SetRandomBackground() {
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
