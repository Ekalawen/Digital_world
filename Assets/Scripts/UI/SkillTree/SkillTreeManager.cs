﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class SkillTreeManager : Singleton<SkillTreeManager> {

    public static string UNLOCKED = "_UNLOCKED";
    public static string ENABLED = "_ENABLED";
    public static string CREDITS_COUNT = "CREDITS_COUNT_KEY";

    public int GetCredits() {
        return PrefsManager.GetInt(CREDITS_COUNT, 0);
    }

    public void SetCredits(int newCreditsCount) {
        PrefsManager.SetInt(CREDITS_COUNT, Mathf.Max(newCreditsCount, 0));
    }

    public void AddCredits(int addedCredits) {
        PrefsManager.SetInt(CREDITS_COUNT, GetCredits() + addedCredits);
    }

    public void RemoveCredits(int removedCredits) {
        AddCredits(-removedCredits);
    }

    public bool IsUnlocked(string key) {
        return PrefsManager.GetBool(key + UNLOCKED, false);
    }

    public void Unlock(string key) {
        PrefsManager.SetBool(key + UNLOCKED, true);
        Enable(key);
    }

    public void Lock(string key) {
        PrefsManager.SetBool(key + UNLOCKED, false);
        Disable(key);
    }

    public bool IsEnabled(string key) {
        return PrefsManager.GetBool(key + ENABLED, false);
    }

    public void Enable(string key) {
        PrefsManager.SetBool(key + ENABLED, true);
    }

    public void Disable(string key) {
        PrefsManager.SetBool(key + ENABLED, false);
    }
}
