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

public class SelectorTarget : MonoBehaviour {

    [Header("Size")]
    public float size = 1.0f;

    [Header("Curves")]
    public AnimationCurve movingCurve;
    public AnimationCurve inCurve;
    public AnimationCurve outCurve;

    protected SelectorManager selectorManager;
    protected Coroutine movingCoroutine;
    protected Fluctuator sizeFluctuator;
    protected bool isShrinked;
    protected Vector3 currentTarget;

    public void Initialize() {
        this.selectorManager = SelectorManager.Instance;
        sizeFluctuator = new Fluctuator(this, GetCurrentSize, SetCurrentSize);
        SetCurrentSize(0);
        isShrinked = true;
        currentTarget = transform.position + Vector3.up; // Something different than where we are
    }

    public void GoTo(Vector3 target, float duration) {
        if (target == currentTarget)
            return;
        currentTarget = target;
        if(isShrinked) {
            transform.position = target;
        } else {
            GoToTarget(target, duration);
        }
        Expand(duration);
    }

    protected void GoToTarget(Vector3 target, float duration) {
        if(movingCoroutine != null) {
            StopCoroutine(movingCoroutine);
        }
        movingCoroutine = StartCoroutine(CGoTo(target, duration));
    }

    protected IEnumerator CGoTo(Vector3 target, float duration) {
        Timer timer = new Timer(duration);
        Vector3 initialPosition = transform.position;
        while(!timer.IsOver()) {
            float avancement = movingCurve.Evaluate(timer.GetAvancement());
            transform.position = Vector3.Lerp(initialPosition, target, avancement);
            yield return null;
        }
        transform.position = target;
    }

    public float GetMovingTime() {
        return GetInTime() + GetOutTime();
    }

    public float GetInTime() {
        return selectorManager.verticalMenuHandler.openTime;
    }

    public float GetOutTime() {
        return selectorManager.verticalMenuHandler.closeTime;
    }

    public float GetCurrentSize() {
        return transform.localScale.x;
    }

    public void SetCurrentSize(float newSize) {
        transform.localScale = Vector3.one * newSize;
    }

    public void Expand(float duration) {
        isShrinked = false;
        sizeFluctuator.GoTo(size, duration, inCurve);
    }

    public void Shrink(float duration) {
        isShrinked = true;
        sizeFluctuator.GoTo(0.0f, duration, outCurve);
    }
}
