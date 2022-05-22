using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using UnityEngine.Localization;

public class AudioBipsPasse : MonoBehaviour {

    [Header("Sound")]
    public List<int> nbOfBips;
    public float bipOffset = 0.3f;
    public float numberOffset = 0.85f;
    public float loopOffset = 3.0f;
    public float beforeFirstLoopOffset = 1.0f;
    public float relativeVolume = 1.5f;

    protected SelectorManager sm;
    protected SelectorPath path;
    protected Coroutine bippingCoroutine = null;
    protected Coroutine firstCoroutine = null;
    protected UISoundManager soundManager;

    protected void Start() {
        sm = SelectorManager.Instance;
        soundManager = UISoundManager.Instance;
        path = GetComponent<SelectorPath>();
        sm.onOpenDHPath.AddListener(OnOpenDHPath);
        sm.onCloseDHPath.AddListener(OnCloseDHPath);
    }

    protected void OnOpenDHPath(SelectorPath pathOpened) {
        if(pathOpened != path) {
            return;
        }
        StartBippingForFirstTime();
    }

    protected void StartBippingForFirstTime() {
        StopBipping();
        firstCoroutine = StartCoroutine(CStartBippingForFirstTime());
    }

    protected IEnumerator CStartBippingForFirstTime() {
        yield return new WaitForSeconds(beforeFirstLoopOffset);
        StartBipping();
    }

    protected void OnCloseDHPath(SelectorPath pathClosed) {
        StopBipping();
    }

    protected void StartBipping() {
        StopBipping();
        bippingCoroutine = StartCoroutine(CStartBipping());
    }

    protected void StopBipping() {
        if (bippingCoroutine != null) {
            StopCoroutine(bippingCoroutine);
        }
        if(firstCoroutine != null) {
            StopCoroutine(firstCoroutine);
        }
    }

    protected IEnumerator CStartBipping() {
        foreach (int bipNb in nbOfBips) {
            for (int i = 0; i < bipNb; i++) {
                soundManager.PlayHoverButtonClipWithVolume(relativeVolume);
                if (i != bipNb - 1) {
                    yield return new WaitForSeconds(bipOffset);
                }
            }
            yield return new WaitForSeconds(numberOffset);
        }
        yield return new WaitForSeconds(loopOffset);
        StartBipping();
    }
}
