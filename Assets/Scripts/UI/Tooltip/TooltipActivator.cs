using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Linq;

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

    public void ShowImmediate() {
        StartCoroutine(CShowInTime(showImmediate: true));
    }

    protected IEnumerator CShowInTime(bool showImmediate = false) {
        yield return StartCoroutine(CWaitForTime(showImmediate));
        if (localizedMessage != null) {
            AsyncOperationHandle<string> handle = localizedMessage.GetLocalizedString();
            yield return handle;
            string message = handle.Result;
            string parsedMessage = message.Replace("\\n", "\n");
            Tooltip.Show(parsedMessage, this);
        } else {
            Debug.LogWarning($"{gameObject.name} possède un TooltipActivator avec un localizedMessage null !", gameObject);
        }
        showingCoroutine = null;
    }

    protected IEnumerator CWaitForTime(bool showImmediate = false) {
        if (!showImmediate) {
            UnpausableTimer timer = new UnpausableTimer(timeBeforeShowing);
            while(!timer.IsOver()) {
                yield return null;
            }
        }
    }

    public void Hide() {
        if (showingCoroutine != null) {
            StopCoroutine(showingCoroutine);
        }
        Tooltip.Hide(this);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (FindGameObjectInParent(eventData.pointerCurrentRaycast.gameObject)) {
            Show();
        }
    }

    public bool FindGameObjectInParent(GameObject raycastTarget) {
        if(raycastTarget == gameObject) {
            return true;
        }
        if(raycastTarget.transform.parent == null) {
            return false;
        }
        return FindGameObjectInParent(raycastTarget.transform.parent.gameObject);
    }

    public void OnPointerExit(PointerEventData eventData) {
        Hide();
    }

    public bool IsHovering() {
        PointerEventData pe = new PointerEventData(EventSystem.current);
        pe.position = Input.mousePosition;
        List<RaycastResult> hits = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pe, hits);
        return hits.Any(hit => FindGameObjectInParent(hit.gameObject));
    }
}
