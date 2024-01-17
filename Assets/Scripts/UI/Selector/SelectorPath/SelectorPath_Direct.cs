using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SelectorPath_Direct : SelectorPath {

    public override void Initialize(SelectorPathUnlockScreen unlockScreen) {
        base.Initialize(unlockScreen);
    }

    public override TYPE GetPathType() {
        return TYPE.DIRECT;
    }
}
