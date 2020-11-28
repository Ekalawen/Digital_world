using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TooltipActivator : MonoBehaviour {

    public string message = "TooltipActivatorMessage";

    protected RectTransform trigger;
    protected bool wasContaining = false;

    public void Start() {
        trigger = GetComponent<RectTransform>();
        if (trigger == null)
            Debug.LogWarning("Un TooltipActivator doit être placé sur un GameObject avec un RectTransform ! ;)");
    }

    public void Update() {
        bool currentlyContaining = RectTransformUtility.RectangleContainsScreenPoint(trigger, Input.mousePosition);
        if(currentlyContaining && !wasContaining) {
            Tooltip.Show(message);
        }
        if(!currentlyContaining && wasContaining) {
            Tooltip.Hide();
        }
        wasContaining = currentlyContaining;
    }
}
