﻿using System;
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

    public bool isInGame = false;

    [Header("Others menus")]
    public GameObject menuPrecedent;
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

    [Header("OtherLinks")]
    public GameObject resetButton;
    public GameObject panelLanguageButton;

    public static string MUSIC_VOLUME_KEY = "musicVolumeKey";
    public static string SOUND_VOLUME_KEY = "soundVolumeKey";
    public static string MOUSE_SPEED_KEY = "mouseSpeedKey";
    public static string LUMINOSITY_KEY = "luminosityKey";
    public static string GRIP_KEY = "gripKey";
    public static string LAST_LEVEL_KEY = "lastLevelKey";

    protected bool hasPanelOpen = false;
    protected GameManager gm;

    public void Run() {
        if(isInGame) {
            gm = GameManager.Instance;
        }
        SetBackground();
        menuPrecedent.SetActive(false);
        menuOptions.SetActive(true);
        BackFromPanel();
        HideSomeOptionsInGame();

        OnMusicVolumeChange(PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY));
        OnSoundVolumeChange(PlayerPrefs.GetFloat(SOUND_VOLUME_KEY));
        OnMouseSpeedChange(PlayerPrefs.GetFloat(MOUSE_SPEED_KEY));
        OnGripActivationPress(PlayerPrefs.GetString(GRIP_KEY) == MenuManager.TRUE);
    }

    protected void HideSomeOptionsInGame() {
        if(isInGame) {
            resetButton.SetActive(false);
            panelLanguageButton.SetActive(false);
        }
    }

    protected void SetBackground() {
        if (!isInGame) {
            float probaSource = 0.03f;
            int distanceSource = 1;
            float decroissanceSource = 0.003f;
            List<ColorManager.Theme> themes = new List<ColorManager.Theme>();
            themes.Add(ColorManager.Theme.BLEU);
            menuPrecedent.GetComponent<MenuManager>().menuBouncingBackground.SetParameters(
                probaSource, distanceSource, decroissanceSource, themes);
        }
    }

    private void Update() {
        CheckForEscape();
    }

    protected void CheckForEscape() {
        if(Input.GetKeyDown(KeyCode.Escape) && menuOptions.activeSelf == true) {
            if(isInGame || !MenuManager.DISABLE_HOTKEYS) {
                Back();
            }
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
        menuPrecedent.SetActive(true);
        menuOptions.SetActive(false);
        if (!isInGame) {
            menuPrecedent.GetComponent<MenuManager>().SetRandomBackground();
        }
    }

    public void OnMusicVolumeChange(float newVal) {
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, newVal);
        sliderMusic.value = newVal;
        sliderMusic.GetComponent<SliderScript>().OnChange(newVal);
        if(isInGame) {
            gm.soundManager.ApplyAudioVolumes();
        }
    }

    public void OnSoundVolumeChange(float newVal) {
        PlayerPrefs.SetFloat(SOUND_VOLUME_KEY, newVal);
        sliderSon.value = newVal;
        sliderSon.GetComponent<SliderScript>().OnChange(newVal);
        if(isInGame) {
            gm.soundManager.ApplyAudioVolumes();
        }
    }

    public void OnMouseSpeedChange(float newVal) {
        PlayerPrefs.SetFloat(MOUSE_SPEED_KEY, newVal);
        sliderMouse.value = newVal;
        sliderMouse.GetComponent<SliderScript>().OnChange(newVal);
        if(isInGame) {
            gm.player.GetPlayerSensitivity();
        }
    }

    public void OnGripActivationPress(bool active) {
        PlayerPrefs.SetString(GRIP_KEY, MenuManager.BoolToString(active));
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
