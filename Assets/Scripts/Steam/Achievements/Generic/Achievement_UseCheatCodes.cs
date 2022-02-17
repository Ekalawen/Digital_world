using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;

public class Achievement_UseCheatCodes : Achievement {

    protected override void InitializeSpecific() {
        gm.cheatCodeManager.onUseCheatCode.AddListener(Unlock);
    }
}
