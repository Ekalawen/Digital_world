using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;

public class Achievement_OpenAllDocs: Achievement {

    protected override void InitializeSpecific() {
        sm.onOpenDoc.AddListener(CheckOpenAllDocs);
    }

    protected virtual void CheckOpenAllDocs(SelectorLevel selectorLevel) {
        List<SelectorLevel> allLevels = sm.GetLevels();
        if(allLevels.All(l => l.menuLevel.HasOpenedDoc())) {
            Unlock();
        }
    }
}
