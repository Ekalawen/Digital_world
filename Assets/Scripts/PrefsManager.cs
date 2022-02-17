using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefsManager {

    public static string TRUE = "true";
    public static string FALSE = "false";

    public static string LOCALE_INDEX_KEY = "localeIndexKey";

    public static string HAVE_THINK_ABOUT_TUTORIAL_KEY = "haveThinkAboutTutorialKey";
    public static string FIRST_TIME_SELECTOR_OPENED_KEY = "firstTimeSelectorOpenedKey";

    public static string SHOULD_SET_RANDOM_BACKGROUND_KEY = "shouldSetRandomBackgroundKey";

    public static string TRACE_KEY = "trace";
    public static string CURRENT_INPUT_FIELD_KEY = "currentInputField"; // A renommer en CURRENT_PASSWORD_INPUT_FIELD_KEY
    public static string EYE_MODE_KEY = "eyeModeKey";

    public static string NB_WINS_KEY = "nbVictoires";
    public static string NB_DEATHS_KEY = "nbTries";
    public static string HAS_JUST_WIN_KEY = "hasJustWin";
    public static string BEST_SCORE_KEY = "bestScore";
    public static string PRECEDENT_BEST_SCORE_KEY = "precedentBestScore";
    public static string HAS_JUST_MAKE_BEST_SCORE_KEY = "hasJustMakeBestScore";
    public static string SINCE_LAST_BEST_SCORE_KEY = "sinceLastBestScore";
    public static string SUM_OF_ALL_TRIES_SCORES_KEY = "sumOfAllTriesScores";
    public static string HAS_ALREADY_DISCOVER_LEVEL_KEY = "hasAlreadyDiscoverLevel";
    public static string IS_LEVEL_HIGHLIGHTED_KEY = "isLevelHighlighted";
    public static string DATA_COUNT_KEY = "dataCountKey";
    public static string TOTAL_DATA_COUNT_KEY = "totalDataCountKey";
    public static string TOTAL_BLOCKS_CROSSED_KEY = "totalBlocksCrossedKey";
    public static string PRECEDENT_DATA_COUNT_KEY = "precedentDataCountKey";
    public static string HAS_JUST_INCREASED_DATA_COUNT_KEY = "hasJustIncreaseDataCountKey";
    public static string HAS_OPENED_DOC_KEY = "hasOpenedDocKey";

    public static string IS_UNLOCKED_PATH_KEY = "IS_UNLOCKED_PATH_KEY";
    public static string HAS_DISPLAY_PATH_UNLOCK_POPUP_KEY = "HAS_DISPLAY_PATH_UNLOCK_POPUP_KEY";
    public static string IS_HIGHLIGHTED_PATH_KEY = "IS_HIGHLIGHTED_PATH_KEY";
    public static string NB_SUBMITS_PATH_KEY = "NB_SUBMITS_PATH_KEY";
    public static string CONSEIL_INDICE_KEY = "CONSEIL_INDICE_KEY";

    public static string SAVE_LEVEL_NAME_KEY = "saveLevelNameKey";
    public static string SAVE_LEVEL_NAME_MUST_BE_USED_KEY = "saveLevelNameMustBeUsedKey";

    public static string LAST_LEVEL_KEY = "lastLevelKey";

    public static string MUSIC_VOLUME_KEY = "musicVolumeKey";
    public static string SOUND_VOLUME_KEY = "soundVolumeKey";
    public static string MOUSE_SPEED_KEY = "mouseSpeedKey";
    public static string LUMINOSITY_KEY = "luminosityKey";
    public static string JUMP_WARP_KEY = "jumpWarpKey";
    public static string WALL_DISTORSION_KEY = "wallDistorsionKey";
    public static string WALL_WARP_KEY = "wallWarpKey";
    public static string SHIFT_WARP_KEY = "shiftWarpKey";
    public static string TIME_SCALE_EFFECT_KEY = "timeScaleEffectKey";
    public static string ADVICE_ON_START_KEY = "adviceOnStartKey";
    public static string FPS_COUNTER_KEY = "fpsCounterKey";
    public static string KEYBINDING_INDICE_KEY = "keybindingIndiceKey";
    public static string KEYBINDING_PRECEDENT_INDICE_KEY = "keybindingPrecedentIndiceKey";
    public static string DATA_QUALITY_KEY = "dataQualityKey";
    public static string DISPLAY_CONSOLE_KEY = "displayConsoleKey";

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
