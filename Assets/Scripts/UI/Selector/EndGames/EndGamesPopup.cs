using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[Serializable]
public class EndGamesPopup {
    public LocalizedString title;
    public LocalizedTextAsset texte;
    public TexteExplicatif.Theme theme = TexteExplicatif.Theme.POSITIF;

    public bool hasYes = true;
    public LocalizedString yes;
    public LocalizedString yesTooltip;

    public bool hasNo = true;
    public LocalizedString no;
    public LocalizedString noTooltip;

    public bool shouldAutomaticallyTransition = false;
    public float automaticTransitionDelay = 0.0f;
}
