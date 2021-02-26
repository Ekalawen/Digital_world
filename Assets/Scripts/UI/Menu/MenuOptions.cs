using System;
using System.Collections;
using System.Collections.Generic;
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
    public Toggle conseilOnStartToggle;

    [Header("OtherLinks")]
    public GameObject resetButton;
    public GameObject panelLanguageButton;

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

    public void Run() {
        if(isInGame) {
            gm = GameManager.Instance;
        }
        SetBackground();
        menuPrecedent.SetActive(false);
        menuOptions.SetActive(true);
        BackFromPanel();
        HideSomeOptionsInGame();
        CenterInGamePanels();

        OnMusicVolumeChange(PrefsManager.GetFloat(PrefsManager.MUSIC_VOLUME_KEY, 1.0f));
        OnSoundVolumeChange(PrefsManager.GetFloat(PrefsManager.SOUND_VOLUME_KEY, 1.0f));
        OnMouseSpeedChange(PrefsManager.GetFloat(PrefsManager.MOUSE_SPEED_KEY, 1.81f));
        OnGripActivationPress(PrefsManager.GetBool(PrefsManager.GRIP_KEY, true));
        OnConseilOnStartPress(PrefsManager.GetBool(PrefsManager.ADVICE_ON_START_KEY, true));
    }

    protected void CenterInGamePanels() {
        if(isInGame) {
            Vector2 pos = mainPanel.GetComponent<RectTransform>().anchoredPosition;
            pos.y = -20;
            mainPanel.GetComponent<RectTransform>().anchoredPosition = pos;
        }
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
        Tooltip.Hide();
        if (!isInGame) {
            menuPrecedent.GetComponent<MenuManager>().SetRandomBackground();
        }
    }

    public void OnMusicVolumeChange(float newVal) {
        PrefsManager.SetFloat(PrefsManager.MUSIC_VOLUME_KEY, newVal);
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
        float oldVal = PrefsManager.GetFloat(PrefsManager.SOUND_VOLUME_KEY, 1.0f);
        PrefsManager.SetFloat(PrefsManager.SOUND_VOLUME_KEY, newVal);
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
        PrefsManager.SetFloat(PrefsManager.MOUSE_SPEED_KEY, newVal);
        sliderMouse.value = newVal;
        sliderMouse.GetComponent<SliderScript>().OnChange(newVal);
        if(isInGame) {
            gm.player.GetPlayerSensitivity();
        }
    }

    public void OnGripActivationPress(bool active) {
        PrefsManager.SetBool(PrefsManager.GRIP_KEY, active);
        toggleGrip.isOn = active;
    }

    public void OnConseilOnStartPress(bool active) {
        PrefsManager.SetBool(PrefsManager.ADVICE_ON_START_KEY, active);
        conseilOnStartToggle.isOn = active;
    }

    public void RememberLastLevel(int indiceLevel) {
        PrefsManager.SetInt(PrefsManager.LAST_LEVEL_KEY, indiceLevel);
    }

    public void ReinitialiserSauvegardes() {
        PrefsManager.DeleteAll();
        OnMusicVolumeChange(1.0f);
        OnSoundVolumeChange(1.0f);
        OnMouseSpeedChange(1.81f);
        OnGripActivationPress(true);
        OnConseilOnStartPress(true);
        PrefsManager.SetBool(PrefsManager.FIRST_TIME_CONNEXION_KEY, true);
        int index = LocalizationSettings.AvailableLocales.Locales.IndexOf(LocalizationSettings.SelectedLocale);
        PrefsManager.SetInt(PrefsManager.LOCALE_INDEX_KEY, index);
        PrefsManager.Save();
    }

    public void ChosePanel(PanelType panelType) {
        mainPanel.SetActive(false);
        hasPanelOpen = true;
        CloseAllPanels();
        Tooltip.Hide();
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
