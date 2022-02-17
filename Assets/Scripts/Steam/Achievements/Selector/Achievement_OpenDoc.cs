using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;

public class Achievement_OpenDoc : Achievement {

    protected override void InitializeSpecific() {
        sm.onOpenDoc.AddListener(UnlockWithDoc);
    }

    protected virtual void UnlockWithDoc(SelectorLevel selectorLevel) {
        Unlock();
    }
}
