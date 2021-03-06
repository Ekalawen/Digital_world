﻿using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

public class TooltipActivator : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    public LocalizedString localizedMessage;
    public float timeBeforeShowing = 0.4f;

    protected Coroutine showingCoroutine = null;

    public virtual void Show() {
        showingCoroutine = StartCoroutine(CShowInTime());
    }

    protected IEnumerator CShowInTime() {
        UnpausableTimer showingTimer = new UnpausableTimer(timeBeforeShowing);
        while(!showingTimer.IsOver()) {
            yield return null;
        }
        if (localizedMessage != null) {
            AsyncOperationHandle<string> handle = localizedMessage.GetLocalizedString();
            yield return handle;
            string message = handle.Result;
            string parsedMessage = message.Replace("\\n", "\n");
            Tooltip.Show(parsedMessage);
        } else {
            Debug.LogWarning($"{gameObject.name} possède un TooltipActivator avec un localizedMessage null !");
        }
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
