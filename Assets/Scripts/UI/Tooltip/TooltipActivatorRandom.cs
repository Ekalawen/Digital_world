using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Localization;

public class TooltipActivatorRandom : TooltipActivator {

    public List<LocalizedString> localizedMessages;

    public override void Show() {
        localizedMessage = localizedMessages[UnityEngine.Random.Range(0, localizedMessages.Count)];
        base.Show();
    }
}
