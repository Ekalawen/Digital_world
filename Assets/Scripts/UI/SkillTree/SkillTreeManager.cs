using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public int MultiplyCreditsBy10() {
        int credits = GetCredits();
        if (credits <= 0) {
            SetCredits(1);
            return 1;
        }
        SetCredits(credits * 10);
        return credits * 9;
    }

    public bool IsAffordable(SkillTreeUpgrade upgrade) {
        return GetCredits() >= upgrade.price;
    }

    public bool IsAccessible(SkillTreeUpgrade upgrade) {
        return upgrade.requirements.All(source => IsUnlocked(source.key));
    }

    public bool CanBuy(SkillTreeUpgrade upgrade) {
        return IsAffordable(upgrade) && IsAccessible(upgrade);
    }

    public bool IsUnlocked(SkillKey key) {
        return PrefsManager.GetBool(key.ToString() + UNLOCKED, false);
    }

    public void Unlock(SkillKey key) {
        PrefsManager.SetBool(key.ToString() + UNLOCKED, true);
        Enable(key);
    }

    public void Lock(SkillKey key) {
        PrefsManager.SetBool(key.ToString() + UNLOCKED, false);
        Disable(key);
    }

    public bool IsEnabled(SkillKey key) {
        return PrefsManager.GetBool(key.ToString() + ENABLED, false);
    }

    public void Enable(SkillKey key) {
        PrefsManager.SetBool(key.ToString() + ENABLED, true);
    }

    public void Disable(SkillKey key) {
        PrefsManager.SetBool(key.ToString() + ENABLED, false);
    }
}
