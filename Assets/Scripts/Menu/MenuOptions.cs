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

    protected string gripKey = "gripKey";
    protected string mouseSpeedKey = "mouseSpeedKey";
    protected string luminosityKey = "luminosityKey";
    protected string lastLevelKey = "lastLevelKey";
    protected string musicVolumeKey = "musicVolumeKey";
    protected string soundVolumeKey = "soundVolumeKey";

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

        OnMusicVolumeChange(PlayerPrefs.GetFloat(musicVolumeKey));
        OnSoundVolumeChange(PlayerPrefs.GetFloat(soundVolumeKey));
        OnMouseSpeedChange(PlayerPrefs.GetFloat(mouseSpeedKey));
        OnLuminosityChange(PlayerPrefs.GetFloat(luminosityKey));
        OnGripActivationPress(PlayerPrefs.GetString(gripKey) == "true");
    }

    private void Update() {
		// Si on appui sur Echap on quitte
		if(Input.GetKeyDown(KeyCode.Escape)) {
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
        PlayerPrefs.SetFloat(musicVolumeKey, newVal);
        sliderMusic.value = newVal;
    }

    public void OnSoundVolumeChange(float newVal) {
        PlayerPrefs.SetFloat(soundVolumeKey, newVal);
        sliderSon.value = newVal;
    }

    public void OnMouseSpeedChange(float newVal) {
        PlayerPrefs.SetFloat(mouseSpeedKey, newVal);
        sliderMouse.value = newVal;
    }

    public void OnLuminosityChange(float newVal) {
        PlayerPrefs.SetFloat(luminosityKey, newVal);
        sliderLuminosity.value = newVal;
    }

    public void OnGripActivationPress(bool active) {
        PlayerPrefs.SetString(gripKey, active.ToString());
    }

    public void RememberLastLevel(int indiceLevel) {
        PlayerPrefs.SetInt(lastLevelKey, indiceLevel);
    }
}
