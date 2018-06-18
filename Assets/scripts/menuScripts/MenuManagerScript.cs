using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManagerScript : MonoBehaviour {

	void Update() {
		// Si on appui sur Echap on quitte
		if(Input.GetKey(KeyCode.Escape)) {
			OnQuitterPress();
		}
	}

	// Lorsqu'on appui sur le bouton jouer
	public void OnPlayPress() {
		Debug.Log("On a appuyé sur Play !");
		SceneManager.LoadScene("first_scene");
	}

	// Lorsqu'on appui sur le bouton tutoriel
	public void OnTutorialPress() {
		Debug.Log("On a appuyé sur Tutoriel !");
	}

	// Lorsqu'on appui sur le bouton options
	public void OnOptionPress() {
		Debug.Log("On a appuyé sur Option !");
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
}
