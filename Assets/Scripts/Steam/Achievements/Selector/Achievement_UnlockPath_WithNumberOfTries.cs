using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;

public class Achievement_UnlockPath_WithNumberOfTries : Achievement_UnlockPath {

    public int nbTriesTreshold = 10;
    public bool exactlyThisTreshold = false;

    protected override void UnlockPath(SelectorPath selectorPath) {
        string key = selectorPath.GetNameId() + PrefsManager.NB_SUBMITS_PATH;
        int nbTries = PrefsManager.GetInt(key, 0);
        if(exactlyThisTreshold ? nbTries == nbTriesTreshold - 1 : nbTries >= nbTriesTreshold - 1) {
            Unlock();
        }
    }
}
