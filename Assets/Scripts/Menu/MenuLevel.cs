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
    public InputField inputFieldNext;
    public string nextPassword = "passwd";
    public TexteExplicatif texteExplicatifPasswdError;
    public TexteExplicatif texteExplicatifDonneesHackes;
    public TexteExplicatif texteExplicatifDonneesHackesSuccess;

    // Les propriétés du background de ce level
    public float probaSource = 0.00035f; // La probabilité d'être une source
    public int distanceSource = 8; // La distance d'action de la source
    public float decroissanceSource = 0.01f; // La vitesse de décroissance de la source
    public List<ColorSource.ThemeSource> themes; // Les couleurs des sources :)

    private void Update() {
        // Si on appui sur Echap on quitte
        if (!MenuManager.DISABLE_HOTKEYS) {
            if (Input.GetKeyDown(KeyCode.Return)
            || Input.GetKeyDown(KeyCode.KeypadEnter)
            || Input.GetKeyDown(KeyCode.Space))
            {
                Play();
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

    private void OnEnable() {
        menuBouncingBackground.SetParameters(probaSource, distanceSource, decroissanceSource, themes);
    }

    public void Play() {
        menuLevelSelector.SaveLevelIndice();
        PlayerPrefs.SetString(LEVEL_NAME_KEY, textLevelName.text);
		SceneManager.LoadScene(levelSceneName);
    }

    public void Next() {
        if (inputFieldNext.text == nextPassword)
            menuLevelSelector.Next();
        else
            texteExplicatifPasswdError.Run();
    }
    public void Previous() {
        menuLevelSelector.Previous();
    }
    public void Back() {
        menuLevelSelector.Back();
    }

    public void OpenDonneesHackes() {
        // Changer le texte des données hackés en fonction du nombre de fois où l'on a gagné ce niveau !
        string key = textLevelName.text + "nbVictoires";
        int nbVictoires = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : 0;
        if (nbVictoires == 0) {
            texteExplicatifDonneesHackes.Run();
        } else {
            texteExplicatifDonneesHackesSuccess.Run();
        }
    }
}
