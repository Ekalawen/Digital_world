using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class MenuOptions : MonoBehaviour {

    public enum PanelType {
        CONTROLES,
        AUDIO,
        GAMEPLAY,
        GRAPHISMS,
        LANGUAGE,
    };

    [Header("Others menus")]
    public GameObject menuInitial;
    public GameObject menuOptions;
    public GameObject mainPanel;

    [Header("Panels")]
    public GameObject panelControles;
    public GameObject panelAudio;
    public GameObject panelGameplay;
    public GameObject panelGraphisms;
    public GameObject panelLanguage;

    [Header("Options")]
    public Slider sliderMusic;
    public Slider sliderSon;
    public Slider sliderMouse;
    public Toggle toggleGrip;

    public static string MUSIC_VOLUME_KEY = "musicVolumeKey";
    public static string SOUND_VOLUME_KEY = "soundVolumeKey";
    public static string MOUSE_SPEED_KEY = "mouseSpeedKey";
    public static string LUMINOSITY_KEY = "luminosityKey";
    public static string GRIP_KEY = "gripKey";
    public static string LAST_LEVEL_KEY = "lastLevelKey";

    protected bool hasPanelOpen = false;

    public void Run() {
        float probaSource = 0.03f;
        int distanceSource = 1;
        float decroissanceSource = 0.003f;
        List<ColorManager.Theme> themes = new List<ColorManager.Theme>();
        themes.Add(ColorManager.Theme.BLEU);
        menuInitial.GetComponent<MenuManager>().menuBouncingBackground.SetParameters(
            probaSource, distanceSource, decroissanceSource, themes);
        menuOptions.SetActive(true);
        menuInitial.SetActive(false);
        BackFromPanel();

        OnMusicVolumeChange(PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY));
        OnSoundVolumeChange(PlayerPrefs.GetFloat(SOUND_VOLUME_KEY));
        OnMouseSpeedChange(PlayerPrefs.GetFloat(MOUSE_SPEED_KEY));
        OnGripActivationPress(PlayerPrefs.GetString(GRIP_KEY) == "True");
    }

    private void Update() {
		// Si on appui sur Echap on quitte
		if(!MenuManager.DISABLE_HOTKEYS && menuOptions.activeSelf == true && Input.GetKeyDown(KeyCode.Escape)) {
            Back();
		}
    }

    public void Back() {
        if(!hasPanelOpen) {
            BackFromOptions();
        } else {
            BackFromPanel();
        }
    }

    protected void BackFromOptions() {
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
        OnGripActivationPress(true);
        PlayerPrefs.SetString(MenuManager.FIRST_TIME_CONNEXION_KEY, "Done !");
        int index = LocalizationSettings.AvailableLocales.Locales.IndexOf(LocalizationSettings.SelectedLocale);
        PlayerPrefs.SetInt(MenuManager.LOCALE_INDEX_KEY, index);
        PlayerPrefs.Save();
    }

    public void ChosePanel(PanelType panelType) {
        mainPanel.SetActive(false);
        hasPanelOpen = true;
        CloseAllPanels();
        Tooltip.Hide();
        switch (panelType) {
            case PanelType.CONTROLES:
                panelControles.SetActive(true);
                break;
            case PanelType.AUDIO:
                panelAudio.SetActive(true);
                break;
            case PanelType.GAMEPLAY:
                panelGameplay.SetActive(true);
                break;
            case PanelType.GRAPHISMS:
                panelGraphisms.SetActive(true);
                break;
            case PanelType.LANGUAGE:
                panelLanguage.SetActive(true);
                break;
        }
    }

    public void ChosePanelControles() {
        ChosePanel(PanelType.CONTROLES);
    }
    public void ChosePanelAudio() {
        ChosePanel(PanelType.AUDIO);
    }
    public void ChosePanelGameplay() {
        ChosePanel(PanelType.GAMEPLAY);
    }
    public void ChosePanelGraphisms() {
        ChosePanel(PanelType.GRAPHISMS);
    }
    public void ChosePanelLanguage() {
        ChosePanel(PanelType.LANGUAGE);
    }

    protected void CloseAllPanels() {
        panelControles.SetActive(false);
        panelAudio.SetActive(false);
        panelGameplay.SetActive(false);
        panelGraphisms.SetActive(false);
        panelLanguage.SetActive(false);
    }

    public void BackFromPanel() {
        hasPanelOpen = false;
        mainPanel.SetActive(true);
        CloseAllPanels();
    }
}
