using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefsManager {

    public static string TRUE = "true";
    public static string FALSE = "false";

    public static string LOCALE_INDEX_ = "localeIndexKey";

    public static string HAVE_THINK_ABOUT_TUTORIAL = "haveThinkAboutTutorialKey";
    public static string FIRST_TIME_SELECTOR_OPENED = "firstTimeSelectorOpenedKey";
    public static string SHOULD_RESET_SAVE_ON_NEXT_MENU_SCENE = "shouldResetSaveOnNextMenuSceneKey";

    public static string SHOULD_SET_RANDOM_BACKGROUND = "shouldSetRandomBackgroundKey";

    public static string TRACE = "trace";
    public static string CURRENT_INPUT_FIELD = "currentInputField"; // A renommer en CURRENT_PASSWORD_INPUT_FIELD_KEY
    public static string EYE_MODE = "eyeModeKey";

    public static string NB_WINS = "nbVictoires";
    public static string NB_DEATHS = "nbTries";
    public static string HAS_JUST_WIN = "hasJustWin";
    public static string BEST_CREDITS_SCORE = "bestCreditsScore";
    public static string TOTAL_CREDITS_SCORE = "totalCreditsScore";
    public static string BEST_BLOCKS_SCORE = "bestBlocksScore";
    public static string TOTAL_BLOCKS_SCORE = "totalBlocksScore";
    public static string PRECEDENT_BEST_BLOCKS_SCORE = "precedentBestBlocksScore";
    public static string HAS_JUST_MAKE_BEST_BLOCKS_SCORE = "hasJustMakeBestBlocksScore";
    public static string SINCE_LAST_BEST_BLOCKS_SCORE = "sinceLastBestBlocksScore";
    public static string HAS_ALREADY_DISCOVER_LEVEL = "hasAlreadyDiscoverLevel";
    public static string IS_LEVEL_HIGHLIGHTED = "isLevelHighlighted";
    public static string DATA_COUNT = "dataCountKey";
    public static string TOTAL_DATA_COUNT = "totalDataCountKey";
    public static string TOTAL_BLOCKS_CROSSED = "totalBlocksCrossedKey";
    public static string TOTAL_CATCH_SOULROBBER = "totalCatchSoulRobberKey";
    public static string PRECEDENT_DATA_COUNT = "precedentDataCountKey";
    public static string HAS_JUST_INCREASED_DATA_COUNT = "hasJustIncreaseDataCountKey";
    public static string HAS_OPENED_DOC = "hasOpenedDocKey";
    public static string NB_BLOCKS_CROSSED_SINCE_LAST_OVERRIDE = "nbBlocksCrossedSinceLastOverrideKey";

    public static string IS_UNLOCKED_PATH = "IS_UNLOCKED_PATH_KEY";
    public static string HAS_DISPLAY_PATH_UNLOCK_POPUP = "HAS_DISPLAY_PATH_UNLOCK_POPUP_KEY";
    public static string IS_HIGHLIGHTED_PATH = "IS_HIGHLIGHTED_PATH_KEY";
    public static string NB_SUBMITS_PATH = "NB_SUBMITS_PATH_KEY";
    public static string CONSEIL_INDICE = "CONSEIL_INDICE_KEY";
    public static string SUPERCHEATEDPASSWORD_NB_USE = "supercheatedpasswordNbUseKey";

    public static string SAVE_LEVEL_NAME = "saveLevelNameKey";
    public static string SAVE_LEVEL_NAME_MUST_BE_USED = "saveLevelNameMustBeUsedKey";

    public static string LAST_LEVEL = "lastLevelKey";

    public static string MUSIC_VOLUME = "musicVolumeKey";
    public static string SOUND_VOLUME = "soundVolumeKey";
    public static string MOUSE_SPEED = "mouseSpeedKey";
    public static string LUMINOSITY = "luminosityKey";
    public static string JUMP_WARP = "jumpWarpKey";
    public static string WALL_DISTORSION = "wallDistorsionKey";
    public static string WALL_WARP = "wallWarpKey";
    public static string SHIFT_WARP = "shiftWarpKey";
    public static string TIME_SCALE_EFFECT = "timeScaleEffectKey";
    public static string ADVICE_ON_START = "adviceOnStartKey";
    public static string FPS_COUNTER = "fpsCounterKey";
    public static string KEYBINDING_INDICE = "keybindingIndiceKey";
    public static string KEYBINDING_PRECEDENT_INDICE = "keybindingPrecedentIndiceKey";
    public static string DATA_QUALITY = "dataQualityKey";
    public static string DISPLAY_CONSOLE = "displayConsoleKey";
    public static string FRAME_RATES_INDICE = "frameRatesIndiceKey";

    public static string HAS_DISPLAY_TUTORIAL_TOOLTIP_PREFIX = "hasDisplayTutorialTooltipPrefix"; // <==
    public static string HAS_DISPLAY_TUTORIAL_TOOLTIP_SELECTOR_MOUVEMENT = "hasDisplayTutorialTooltipSelectorMouvement";
    public static string HAS_DISPLAY_TUTORIAL_TOOLTIP_SELECTOR_LEVEL = "hasDisplayTutorialTooltipSelectorLevel";
    public static string HAS_DISPLAY_TUTORIAL_TOOLTIP_SELECTOR_CADENAS = "hasDisplayTutorialTooltipSelectorCadenas";
    public static string HAS_DISPLAY_TUTORIAL_TOOLTIP_TO_DH = "hasDisplayTutorialTooltipToDH";
    public static string HAS_DISPLAY_TUTORIAL_TOOLTIP_OPEN_DH = "hasDisplayTutorialTooltipOpenDH";
    public static string HAS_DISPLAY_TUTORIAL_TOOLTIP_HACKER = "hasDisplayTutorialTooltipHacker";
    public static string HAS_DISPLAY_TUTORIAL_TOOLTIP_TO_NEXT_LEVEL = "hasDisplayTutorialTooltipSelectorToNextLevel";


    public static int GetInt(string key, int defaultValue) {
        return PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : defaultValue;
    }
    public static float GetFloat(string key, float defaultValue) {
        return PlayerPrefs.HasKey(key) ? PlayerPrefs.GetFloat(key) : defaultValue;
    }
    public static string GetString(string key, string defaultValue) {
        return PlayerPrefs.HasKey(key) ? PlayerPrefs.GetString(key) : defaultValue;
    }
    public static bool GetBool(string key, bool defaultValue) {
        return PlayerPrefs.HasKey(key) ? PlayerPrefs.GetString(key) == PrefsManager.TRUE : defaultValue;
    }

    public static void SetInt(string key, int value) {
        PlayerPrefs.SetInt(key, value);
    }
    public static void IncrementInt(string key, int valueToAdd, int defaultValue) {
        int newValue = GetInt(key, defaultValue) + valueToAdd;
        PlayerPrefs.SetInt(key, newValue);
    }
    public static void SetFloat(string key, float value) {
        PlayerPrefs.SetFloat(key, value);
    }
    public static void SetString(string key, string value) {
        PlayerPrefs.SetString(key, value);
    }
    public static void SetBool(string key, bool value) {
        string boolString = value ? TRUE : FALSE;
        PlayerPrefs.SetString(key, boolString);
    }

    public static bool HasKey(string key) {
        return PlayerPrefs.HasKey(key);
    }

    public static void DeleteKey(string key) {
        PlayerPrefs.DeleteKey(key);
    }

    public static void DeleteAll() {
        PlayerPrefs.DeleteAll();
    }

    public static void Save() {
        PlayerPrefs.Save();
    }
}
