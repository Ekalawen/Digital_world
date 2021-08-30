using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

public class TextRevelator : MonoBehaviour {

    public float offsetBeforeReveal = 3.0f;
    public float revealCharacterTime = 0.1f;
    public bool isLooping = false;
    [ConditionalHide("isLooping")]
    public float offsetAfterReveal = 3.0f;

    protected TMP_Text text;
    protected int nbCharacters;

    private void Start() {
        text = GetComponent<TMP_Text>();
        StartCoroutine(StartReveal());
    }

    protected IEnumerator StartReveal() {
        nbCharacters = text.text.Length;
        text.maxVisibleCharacters = 0;
        yield return new WaitForSeconds(offsetBeforeReveal);
        for(int i = 0; i < nbCharacters; i++) {
            text.maxVisibleCharacters = i;
            yield return new WaitForSeconds(revealCharacterTime);
        }
        text.maxVisibleCharacters = nbCharacters;

        if(isLooping) {
            yield return new WaitForSeconds(offsetAfterReveal);
            StartCoroutine(StartReveal());
        }
    }
}
