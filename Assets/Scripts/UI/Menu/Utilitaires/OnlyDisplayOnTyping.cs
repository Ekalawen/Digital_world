using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OnlyDisplayOnTyping : MonoBehaviour {

    public InputField input;
    public float delay = 0.5f;

    protected Coroutine coroutine = null;

    protected void Start() {
        input.onValueChanged.AddListener(OnValueChanged);
    }

    public void OnValueChanged(string str) {
        if (!enabled)
            return;
        input.contentType = InputField.ContentType.Standard;
        if (coroutine != null)
            StopCoroutine(coroutine);
        coroutine = StartCoroutine(CSetBackToPassword());
        //string chars = "";
        //for (int i = 0; i < str.Length - 1; i++)
        //    chars += "*";
        //chars += str[str.Length - 1];
        ////input.asteriskChar = chars[str.Length - 1];
        //input.SetTextWithoutNotify(chars);
    }

    protected IEnumerator CSetBackToPassword() {
        yield return new WaitForSeconds(delay);
        input.contentType = InputField.ContentType.Password;
        NotifyInput(input);
    }

    public static void NotifyInput(InputField input) {
        // Just to notify x) Sinon l'input ne se met pas à jour :/
        string oldValue = input.text;
        int oldCaretPosition = input.caretPosition;
        input.SetTextWithoutNotify("pingouin" + oldValue); // Quelquechose de différent ! :)
        input.SetTextWithoutNotify(oldValue);
        input.caretPosition = oldCaretPosition;
    }
}
