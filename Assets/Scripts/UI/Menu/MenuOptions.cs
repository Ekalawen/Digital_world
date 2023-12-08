﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class MenuOptions : MonoBehaviour {

    public enum PanelType {
        CONTROLES,
        AUDIO,
        GAMEPLAY,
        GRAPHISMS,
        LANGUAGE,
    };

    public static float defaultMusicVolume = 0.5f;
    public static float defaultSoundVolume = 0.5f;
    public static float defaultMouseSpeed = 1.81f;
    public static bool defaultJumpWarpActivation = true;
    public static bool defaultWallDistorsionActivation = true;
    public static bool defaultWallWarpActivation = true;
    public static bool defaultShiftWarpActivation = true;
    public static bool defaultTimeScaleEffectActivation = true;
    public static bool defaultConseilOnStart = false;
    public static bool defaultFpsCounter = false;
    public static bool defaultDisplayConsole = false;
    public static int defaultFrameRateIndice = 0; // Auto
    public static float defaultLuminosity = 0;
    public static Lumiere.LumiereQuality defaultLumiereQuality = Lumiere.LumiereQuality.LOW;
    public static List<string> keyOfAchievementsToSaveDuringResetSaves = new List<string>() {
        PrefsManager.TOTAL_DATA_COUNT,
        PrefsManager.TOTAL_BLOCKS_CROSSED,
        PrefsManager.TOTAL_CATCH_SOULROBBER,
        PrefsManager.SUPERCHEATEDPASSWORD_NB_USE,
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
    public Slider sliderLuminosity;
    public Toggle toggleJumpWarp;
    public Toggle toggleWallWarp;
    public Toggle toggleWallDistorsion;
    public Toggle toggleShiftWarp;
    public Toggle toggleTimeScaleEffect;
    public Toggle conseilOnStartToggle;
    public Toggle fpsCounterToggle;
    public Toggle displayConsoleToggle;

    [Header("OtherLinks")]
    public GameObject resetButton;
    public GameObject panelLanguageButton;
    public GameObject returnButton;
    public GameObject achievementManagerPrefab;
    public KeybindingDropdown keybindingDropdown;

    [Header("Titles")]
    public TMP_Text titleText;
    public LocalizedString mainTitle;
    public LocalizedString controleTitle;
    public LocalizedString audioTitle;
    public LocalizedString gameplayTitle;
    public LocalizedString graphismsTitle;
    public LocalizedString languageTitle;

    protected bool hasPanelOpen = false;
    protected GameManager gm;
    protected float titleTextFontSize;

    public void Run() {
        if(isInGame) {
            gm = GameManager.Instance;
        }
        titleTextFontSize = titleText.fontSize;
        SetBackground();
        menuPrecedent.SetActive(false);
        menuOptions.SetActive(true);
        BackFromPanel();
        HideSomeOptionsInGame();

        OnMusicVolumeChange(PrefsManager.GetFloat(PrefsManager.MUSIC_VOLUME, defaultMusicVolume));
        OnSoundVolumeChange(PrefsManager.GetFloat(PrefsManager.SOUND_VOLUME, defaultSoundVolume));
        OnMouseSpeedChange(PrefsManager.GetFloat(PrefsManager.MOUSE_SPEED, defaultMouseSpeed));
        OnLuminosityChange(PrefsManager.GetFloat(PrefsManager.LUMINOSITY, defaultLuminosity));
        OnJumpWarpActivationPress(PrefsManager.GetBool(PrefsManager.JUMP_WARP, defaultJumpWarpActivation));
        OnWallWarpActivationPress(PrefsManager.GetBool(PrefsManager.WALL_WARP, defaultWallWarpActivation));
        OnWallDistorsionActivationPress(PrefsManager.GetBool(PrefsManager.WALL_DISTORSION, defaultWallDistorsionActivation));
        OnShiftWarpActivationPress(PrefsManager.GetBool(PrefsManager.SHIFT_WARP, defaultShiftWarpActivation));
        OnTimeScaleEffectActivationPress(PrefsManager.GetBool(PrefsManager.TIME_SCALE_EFFECT, defaultTimeScaleEffectActivation));
        OnConseilOnStartPress(PrefsManager.GetBool(PrefsManager.ADVICE_ON_START, defaultConseilOnStart));
        OnFpsCounterPress(PrefsManager.GetBool(PrefsManager.FPS_COUNTER, defaultFpsCounter));
        OnDisplayConsolePress(PrefsManager.GetBool(PrefsManager.DISPLAY_CONSOLE, defaultDisplayConsole));
    }

    protected void HideSomeOptionsInGame() {
        if(isInGame) {
            resetButton.SetActive(false);
        }
    }

    protected void SetBackground() {
        if (!isInGame) {
            float probaSource = 0.03f;
            int distanceSource = 1;
            float decroissanceSource = 0.003f;
            List<ColorManager.Theme> themes = new List<ColorManager.Theme>();
            themes.Add(ColorManager.Theme.BLEU);
            //menuPrecedent.GetComponent<MenuManager>().menuBouncingBackground.SetParameters(probaSource, distanceSource, decroissanceSource, themes);
        }
    }

    private void Update() {
        CheckForEscape();
    }

    protected void CheckForEscape() {
        if(Input.GetKeyDown(KeyCode.Escape) && menuOptions.activeInHierarchy == true) {
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
        Tooltip.HideAll();
        if (!isInGame) {
            menuPrecedent.GetComponent<MenuManager>().SetRandomBackground();
        }
    }

    public void OnMusicVolumeChange(float newVal) {
        PrefsManager.SetFloat(PrefsManager.MUSIC_VOLUME, newVal);
        sliderMusic.value = newVal;
        sliderMusic.GetComponent<SliderScript>().OnChange(newVal);
        if(isInGame) {
            gm.soundManager.ApplyAudioVolumes();
        } else {
            UISoundManager.Instance.GetAudioVolumes();
            UISoundManager.Instance.UpdateMusicVolume();
        }
    }

    public void OnSoundVolumeChange(float newVal) {
        float oldVal = PrefsManager.GetFloat(PrefsManager.SOUND_VOLUME, defaultSoundVolume);
        PrefsManager.SetFloat(PrefsManager.SOUND_VOLUME, newVal);
        sliderSon.value = newVal;
        sliderSon.GetComponent<SliderScript>().OnChange(newVal);
        if(isInGame) {
            gm.soundManager.ApplyAudioVolumes();
            if (newVal != oldVal) { // Pour éviter que ça fasse un son quand on ouvre le menu :)
                gm.soundManager.PlayJumpClip(gm.player.transform.position);
            }
        } else {
            UISoundManager.Instance.GetAudioVolumes();
        }
    }

    public void OnMouseSpeedChange(float newVal) {
        PrefsManager.SetFloat(PrefsManager.MOUSE_SPEED, newVal);
        sliderMouse.value = newVal;
        sliderMouse.GetComponent<SliderScript>().OnChange(newVal);
        if(isInGame) {
            gm.player.GetPlayerSensitivity();
        }
    }

    public void OnLuminosityChange(float newVal) {
        PrefsManager.SetFloat(PrefsManager.LUMINOSITY, newVal);
        sliderLuminosity.value = newVal;
        sliderLuminosity.GetComponent<SliderScript>().OnChange(newVal);
        if(isInGame) {
            gm.postProcessManager.SetLuminosityIntensity(newVal);
        } else {
            if(MenuManager.IsInitialized) {
                MenuManager.Instance.SetLuminosityVolume();
            }
        }
    }

    public void OnWallWarpActivationPress(bool active) {
        PrefsManager.SetBool(PrefsManager.WALL_WARP, active);
        toggleWallWarp.isOn = active;
        if(isInGame && !active) {
            gm.postProcessManager.StopWallVfx();
        }
    }

    public void OnWallDistorsionActivationPress(bool active) {
        PrefsManager.SetBool(PrefsManager.WALL_DISTORSION, active);
        toggleWallDistorsion.isOn = active;
        if(isInGame && !active) {
            gm.postProcessManager.StopWallDistorsionEffect();
        }
    }

    public void OnJumpWarpActivationPress(bool active) {
        PrefsManager.SetBool(PrefsManager.JUMP_WARP, active);
        toggleJumpWarp.isOn = active;
    }

    public void OnShiftWarpActivationPress(bool active) {
        PrefsManager.SetBool(PrefsManager.SHIFT_WARP, active);
        toggleShiftWarp.isOn = active;
        if(isInGame && !active) {
            gm.postProcessManager.StopShiftVfx();
        }
    }

    public void OnTimeScaleEffectActivationPress(bool active) {
        PrefsManager.SetBool(PrefsManager.TIME_SCALE_EFFECT, active);
        toggleTimeScaleEffect.isOn = active;
        if(isInGame) {
            gm.postProcessManager.SetTimeScaleEffectActivation(active);
        }
    }

    public void OnConseilOnStartPress(bool active) {
        PrefsManager.SetBool(PrefsManager.ADVICE_ON_START, active);
        conseilOnStartToggle.isOn = active;
    }

    public void OnFpsCounterPress(bool active) {
        PrefsManager.SetBool(PrefsManager.FPS_COUNTER, active);
        fpsCounterToggle.isOn = active;
        if(isInGame) {
            gm.console.InitFrameRateCounter();
        }
    }

    public void OnDisplayConsolePress(bool active) {
        PrefsManager.SetBool(PrefsManager.DISPLAY_CONSOLE, active);
        displayConsoleToggle.isOn = active;
        if(isInGame) {
            gm.console.DisplayOrNotConsole();
        }
    }

    public void RememberLastLevel(int indiceLevel) {
        PrefsManager.SetInt(PrefsManager.LAST_LEVEL, indiceLevel);
    }

    public void ReinitialiserSauvegardesPopup() {
        MenuManager menu = MenuManager.Instance;
        menu.popup.RunPopup(menu.strings.resetSavesTitle, menu.strings.resetSavesTexte, TexteExplicatif.Theme.NEGATIF);
        menu.popup.RemoveDoneButton();
        menu.popup.AddButton(menu.strings.resetSavesNoButton, menu.strings.resetSavesNoButtonTooltip, TexteExplicatif.Theme.NEGATIF, null);
        menu.popup.AddButton(menu.strings.resetSavesYesButton, menu.strings.resetSavesYesButtonTooltip, TexteExplicatif.Theme.POSITIF, ReinitialiserSauvegardes);
        menu.popup.AddActionOnStartAndEnd(DisableReturnButtonIn, EnableReturnButtonIn);
    }

    public void DisableReturnButtonIn() {
        StartCoroutine(CDisableReturnButtonIn());
    }

    protected IEnumerator CDisableReturnButtonIn() {
        yield return new WaitForSeconds(MenuManager.Instance.popup.dureeOpenAnimation);
        returnButton.SetActive(false);
    }

    public void EnableReturnButtonIn() {
        StartCoroutine(CEnableReturnButtonIn());
    }

    protected IEnumerator CEnableReturnButtonIn() {
        yield return new WaitForSeconds(MenuManager.Instance.popup.dureeCloseAnimation);
        returnButton.SetActive(true);
    }

    public void ReinitialiserSauvegardes() {
        Debug.Log($"Réinitialisation des sauvegardes !!!");
        Dictionary<Achievement, bool> achievementsStates = GetAllAchievementsStates();
        Dictionary<string, int> keysToSave = GetKeysOfAchievementsToSave();
        PrefsManager.DeleteAll();
        OnMusicVolumeChange(defaultMusicVolume);
        OnSoundVolumeChange(defaultSoundVolume);
        OnMouseSpeedChange(defaultMouseSpeed);
        OnLuminosityChange(defaultLuminosity);
        OnWallDistorsionActivationPress(defaultWallDistorsionActivation);
        OnWallWarpActivationPress(defaultWallWarpActivation);
        OnConseilOnStartPress(defaultConseilOnStart);
        OnFpsCounterPress(defaultFpsCounter);
        OnDisplayConsolePress(defaultDisplayConsole);
        ApplyAllAchievementsStates(achievementsStates);
        ApplyAllKeysSaved(keysToSave);
        int index = LocalizationSettings.AvailableLocales.Locales.IndexOf(LocalizationSettings.SelectedLocale);
        PrefsManager.SetInt(PrefsManager.LOCALE_INDEX_, index);
        PrefsManager.Save();
    }

    protected Dictionary<Achievement, bool> GetAllAchievementsStates() {
        Dictionary<Achievement, bool> pairs = new Dictionary<Achievement, bool>();
        foreach(Achievement achievement in achievementManagerPrefab.GetComponent<AchievementManager>().GetAllAchievements()) {
            pairs[achievement] = achievement.IsUnlocked();
        }
        return pairs;
    }

    protected void ApplyAllAchievementsStates(Dictionary<Achievement, bool> achievementsStates) {
        foreach(KeyValuePair<Achievement, bool> pair in achievementsStates) {
            if(pair.Value) {
                pair.Key.SetLocalLockState(isUnlock: pair.Value);
            }
        }
    }

    protected Dictionary<string, int> GetKeysOfAchievementsToSave() {
        Dictionary<string, int> pairs = new Dictionary<string, int>();
        foreach(string keyToSave in keyOfAchievementsToSaveDuringResetSaves) {
            pairs[keyToSave] = PrefsManager.GetInt(keyToSave, 0);
        }
        return pairs;
    }

    protected void ApplyAllKeysSaved(Dictionary<string, int> keysToSave) {
        foreach(KeyValuePair<string, int> pair in keysToSave) {
            PrefsManager.SetInt(pair.Key, pair.Value);
        }
    }

    public void ChosePanel(PanelType panelType) {
        mainPanel.SetActive(false);
        hasPanelOpen = true;
        CloseAllPanels();
        Tooltip.HideAll();
        switch (panelType) {
            case PanelType.CONTROLES:
                panelControles.SetActive(true);
                SetTitleText(controleTitle);
                break;
            case PanelType.AUDIO:
                panelAudio.SetActive(true);
                SetTitleText(audioTitle);
                break;
            case PanelType.GAMEPLAY:
                panelGameplay.SetActive(true);
                SetTitleText(gameplayTitle);
                break;
            case PanelType.GRAPHISMS:
                panelGraphisms.SetActive(true);
                SetTitleText(graphismsTitle);
                break;
            case PanelType.LANGUAGE:
                panelLanguage.SetActive(true);
                SetTitleText(languageTitle);
                break;
        }
    }

    protected void SetTitleText(LocalizedString localizedString) {
        StartCoroutine(CSetTitleText(localizedString));
    }

    protected IEnumerator CSetTitleText(LocalizedString localizedString) {
        AsyncOperationHandle<string> handle = localizedString.GetLocalizedString();
        yield return handle;
        titleText.text = handle.Result;
        titleText.fontSize = titleTextFontSize;
        UIHelper.FitTextHorizontally(titleText.text, titleText);
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
        SetTitleText(mainTitle);
        CloseAllPanels();
    }

    public void ResetMenu() {
        hasPanelOpen = false;
        mainPanel.SetActive(true);
        CloseAllPanels();
    }
}
