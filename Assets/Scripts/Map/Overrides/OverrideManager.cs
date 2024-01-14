using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

[Serializable]
public class OverrideWeight {
    public float weight = 12.0f;
    public GameObject overrideObject = null;
}

[Serializable]
public class OverridesList {
    public float probability = 0.2f;
    public List<OverrideWeight> overrides;
}

public class OverrideManager : MonoBehaviour {

    public int nbBlocksBetweenOverrides = 20;
    public List<OverridesList> overridesLists;

    protected GameManager gm;
    protected MapManager map;

    protected Override currentOverride = null;
    protected int nbBlocksCrossedSinceLastOverride;

    public void Initialize() {
        gm = GameManager.Instance;
        InitializeBlocksCounter();
        InitializeOverride();
    }

    protected void InitializeBlocksCounter() {
        nbBlocksCrossedSinceLastOverride = PrefsManager.GetInt(PrefsManager.NB_BLOCKS_CROSSED_SINCE_LAST_OVERRIDE, nbBlocksBetweenOverrides);
        if(!gm.IsIR()) {
            return;
        }
        gm.GetInfiniteMap().onBlocksCrossedNonStart.AddListener(IncrementNbBlockCrossed);
    }

    protected void IncrementNbBlockCrossed(int nbBlocksCrossed) {
        nbBlocksCrossedSinceLastOverride += nbBlocksCrossed;
        PrefsManager.SetInt(PrefsManager.NB_BLOCKS_CROSSED_SINCE_LAST_OVERRIDE, nbBlocksCrossedSinceLastOverride);
    }

    private void InitializeOverride() {
        if(!ShouldUseOverride()) {
            Debug.Log($"No override this game !");
            return;
        }
        GameObject overridePrefab = SelectOverridePrefab();
        currentOverride = Instantiate(overridePrefab, transform).GetComponent<Override>();
        Debug.Log($"Using {currentOverride.name} this game!");
        currentOverride.Initialize();
        ApplyOverrideGlobalInitialization();
        ResetNbBlocksCrossed();
    }

    protected void ResetNbBlocksCrossed() {
        nbBlocksCrossedSinceLastOverride = 0;
        PrefsManager.SetInt(PrefsManager.NB_BLOCKS_CROSSED_SINCE_LAST_OVERRIDE, 0);
    }

    private void ApplyOverrideGlobalInitialization() {
        gm.console.InitializeOverride(currentOverride);
        gm.postProcessManager.ApplySkyboxOverrideRotation();
    }

    private bool ShouldUseOverride() {
        if(!SkillTreeManager.Instance.IsEnabled(SkillKey.UNLOCK_OVERRIDES)) {
            return false;
        }
        //float value = UnityEngine.Random.value;
        //return value < overridesLists.Select(o => o.probability).Sum();
        return nbBlocksCrossedSinceLastOverride >= nbBlocksBetweenOverrides;
    }

    private GameObject SelectOverridePrefab() {
        OverridesList list = MathTools.ChoseOneWeighted(overridesLists, overridesLists.Select(o => o.probability).ToList());
        GameObject overridePrefab = MathTools.ChoseOneWeighted(list.overrides, list.overrides.Select(o => o.weight).ToList()).overrideObject;
        return overridePrefab;
    }

    bool HasOverride() {
        return currentOverride;
    }
}
