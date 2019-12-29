using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuOptions : MonoBehaviour {

    public GameObject menuInitial;
    public GameObject menuOptions;

    public Slider sliderMusic;
    public Slider sliderSon;
    public Slider sliderMouse;
    public Slider sliderLuminosity;
    public Toggle toggleGrip;

    public static string MUSIC_VOLUME_KEY = "musicVolumeKey";
    public static string SOUND_VOLUME_KEY = "soundVolumeKey";
    public static string MOUSE_SPEED_KEY = "mouseSpeedKey";
    public static string LUMINOSITY_KEY = "luminosityKey";
    public static string GRIP_KEY = "gripKey";
    public static string LAST_LEVEL_KEY = "lastLevelKey";

    public void Run() {
        float probaSource = 0.03f;
        int distanceSource = 1;
        float decroissanceSource = 0.003f;
        List<ColorSource.ThemeSource> themes = new List<ColorSource.ThemeSource>();
        themes.Add(ColorSource.ThemeSource.BLEU_NUIT);
        menuInitial.GetComponent<MenuManager>().menuBouncingBackground.SetParameters(
            probaSource, distanceSource, decroissanceSource, themes);
        menuOptions.SetActive(true);
        menuInitial.SetActive(false);

        OnMusicVolumeChange(PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY));
        OnSoundVolumeChange(PlayerPrefs.GetFloat(SOUND_VOLUME_KEY));
        OnMouseSpeedChange(PlayerPrefs.GetFloat(MOUSE_SPEED_KEY));
        OnLuminosityChange(PlayerPrefs.GetFloat(LUMINOSITY_KEY));
        OnGripActivationPress(PlayerPrefs.GetString(GRIP_KEY) == "True");
    }

    private void Update() {
		// Si on appui sur Echap on quitte
		if(!MenuManager.DISABLE_HOTKEYS && Input.GetKeyDown(KeyCode.Escape)) {
            Back();
		}
    }

    public void Back() {
        PlayerPrefs.Save();
        menuInitial.SetActive(true);
        menuOptions.SetActive(false);
        menuInitial.GetComponent<MenuManager>().SetRandomBackground();
    }

    public void OnMusicVolumeChange(float newVal) {
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, newVal);
        sliderMusic.value = newVal;
        sliderMusic.GetComponent<SliderScript>().OnChange(newVal);
    }

    public void OnSoundVolumeChange(float newVal) {
        PlayerPrefs.SetFloat(SOUND_VOLUME_KEY, newVal);
        sliderSon.value = newVal;
        sliderSon.GetComponent<SliderScript>().OnChange(newVal);
    }

    public void OnMouseSpeedChange(float newVal) {
        PlayerPrefs.SetFloat(MOUSE_SPEED_KEY, newVal);
        sliderMouse.value = newVal;
        sliderMouse.GetComponent<SliderScript>().OnChange(newVal);
    }

    public void OnLuminosityChange(float newVal) {
        PlayerPrefs.SetFloat(LUMINOSITY_KEY, newVal);
        sliderLuminosity.value = newVal;
        sliderLuminosity.GetComponent<SliderScript>().OnChange(newVal);
    }

    public void OnGripActivationPress(bool active) {
        PlayerPrefs.SetString(GRIP_KEY, active.ToString());
        toggleGrip.isOn = active;
    }

    public void RememberLastLevel(int indiceLevel) {
        PlayerPrefs.SetInt(LAST_LEVEL_KEY, indiceLevel);
    }

    public void ReinitialiserSauvegardes() {
        PlayerPrefs.DeleteAll();
        OnMusicVolumeChange(1.0f);
        OnSoundVolumeChange(1.0f);
        OnMouseSpeedChange(1.81f);
        OnLuminosityChange(1.1f);
        OnGripActivationPress(true);
        PlayerPrefs.SetString(MenuManager.FIRST_TIME_CONNEXION_KEY, "Done !");
        PlayerPrefs.Save();
    }
}
