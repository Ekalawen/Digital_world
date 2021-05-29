using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using UnityEngine.Localization;

public class CustomPasseNbTries : CustomPasse {
    public override string GetPasse(SelectorPath selectorPath) {
        string key = selectorPath.GetNameId() + PrefsManager.NB_SUBMITS_PATH_KEY;
        int nbSubmits = PrefsManager.GetInt(key, 0);
        return nbSubmits.ToString();
    }
}
