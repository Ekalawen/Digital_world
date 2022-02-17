using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;

public class Achievement_UnlockPath : Achievement {

    protected override void InitializeSpecific() {
        sm.onUnlockPath.AddListener(UnlockPath);
    }

    protected void UnlockPath(SelectorPath selectorPath) {
        Unlock();
    }
}
