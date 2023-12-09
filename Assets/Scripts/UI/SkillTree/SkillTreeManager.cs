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
    public static string CREDITS_COUNT = "_CREDITS_COUNT_KEY";
    public static string HAS_BEEN_NEWLY_AFFORDABLE = "_HAS_BEEN_NEWLY_AFFORDABLE_KEY";

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

    public bool IsAffordable(SkillTreeUpgrade upgrade, int additionnalCredits = 0) {
        return GetCredits() + additionnalCredits >= upgrade.price;
    }

    public bool IsAccessible(SkillTreeUpgrade upgrade) {
        return upgrade.requirements.All(source => IsUnlocked(source.key));
    }

    public bool CanBuy(SkillTreeUpgrade upgrade, int additionnalCredits = 0) {
        return IsAffordable(upgrade, additionnalCredits) && IsAccessible(upgrade);
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
        SetHasBeenNewlyAffordable(key, false);
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

    public bool HasBeenNewlyAffordable(SkillKey key) {
        return PrefsManager.GetBool(key.ToString() + HAS_BEEN_NEWLY_AFFORDABLE, false);
    }

    public void SetHasBeenNewlyAffordable(SkillKey key, bool value) {
        PrefsManager.SetBool(key.ToString() + HAS_BEEN_NEWLY_AFFORDABLE, value);
    }

    public bool IsNewlyAffordable(SkillTreeUpgrade upgrade, int additionnalCredits = 0) {
        return !HasBeenNewlyAffordable(upgrade.key) && CanBuy(upgrade, additionnalCredits);
    }
}
