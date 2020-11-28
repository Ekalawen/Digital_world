using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TooltipActivator : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{

    public string message = "TooltipActivatorMessage";
    public float timeBeforeShowing = 0.4f;

    protected Coroutine showingCoroutine = null;

    public void Show() {
        showingCoroutine = StartCoroutine(CShowInTime());
    }

    protected IEnumerator CShowInTime() {
        yield return new WaitForSeconds(timeBeforeShowing);
        string parsedMessage = message.Replace("\\n", "\n");
        Tooltip.Show(parsedMessage);
    }

    public void Hide() {
        if (showingCoroutine != null)
            StopCoroutine(showingCoroutine);
        Tooltip.Hide();
    }

    public void OnPointerEnter(PointerEventData eventData) {
        Show();
    }

    public void OnPointerExit(PointerEventData eventData) {
        Hide();
    }
}
