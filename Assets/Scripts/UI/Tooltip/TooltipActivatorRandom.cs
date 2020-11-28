using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TooltipActivatorRandom : TooltipActivator {

    public List<string> messages;

    public override void Show() {
        message = messages[UnityEngine.Random.Range(0, messages.Count)];
        base.Show();
    }
}
