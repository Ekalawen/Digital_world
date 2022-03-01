using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

public class TextRevelator : MonoBehaviour {

    public float offsetBeforeReveal = 3.0f;
    public float revealCharacterTime = 0.1f;
    public float newlineRevealTime = 0.5f;
    public bool isLooping = false;
    [ConditionalHide("isLooping")]
    public float loopDuration = 3.0f;

    protected TMP_Text text;
    protected int nbCharacters;

    private void Start() {
        text = GetComponent<TMP_Text>();
        StartCoroutine(CLoop());
    }

    protected IEnumerator CLoop() {
        StartCoroutine(StartReveal());
        if (isLooping) {
            yield return new WaitForSeconds(loopDuration);
            StartCoroutine(CLoop());
        }
    }

    protected IEnumerator StartReveal() {
        nbCharacters = text.text.Length;
        text.maxVisibleCharacters = 0;
        yield return new WaitForSeconds(offsetBeforeReveal);
        for(int i = 0; i < nbCharacters; i++) {
            text.maxVisibleCharacters = i;
            if (text.text[i] == '\n') {
                yield return new WaitForSeconds(newlineRevealTime);
            } else {
                yield return new WaitForSeconds(revealCharacterTime);
            }
        }
        text.maxVisibleCharacters = nbCharacters;
    }
}
