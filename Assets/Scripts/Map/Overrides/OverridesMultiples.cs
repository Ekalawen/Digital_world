using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using UnityEngine.Localization.SmartFormat.Utilities;

public class OverridesMultiples : Override {

    public int nbOverridesToChose = 2;
    public List<GameObject> overridePrefabs;

    protected List<Override> overrides;

    protected override void InitializeSpecific() {
        List<GameObject> chosenPrefabs = GaussianGenerator.SelecteSomeNumberOf(overridePrefabs, nbOverridesToChose);
        overrides = new List<Override>();
        foreach (GameObject overridePrefab in chosenPrefabs) {
            Override overrid = Instantiate(overridePrefab, transform).GetComponent<Override>();
            overrid.Initialize();
            overrides.Add(overrid);
        }
    }

    public override string GetName() {
        string result = "";
        foreach (Override overrid in overrides) {
            if(overrid != overrides.First()) {
                result += gm.console.strings.overrideAnd.GetLocalizedString().Result;
            }
            result += overrid.GetName();
        }
        return result;
    }

    public override int GetScoreMultiplier() {
        return GetScoreMultiplier() * overrides.Select(o => o.GetScoreMultiplier()).Sum();
    }
}
