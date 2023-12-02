using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using UnityEngine.Localization;

public class CustomPasseNbDatas : CustomPasse {
    public override string GetPasse(SelectorPath selectorPath) {
        int nbDatas = PrefsManager.GetInt(PrefsManager.TOTAL_DATA_COUNT, 0);
        return nbDatas.ToString();
    }
}
