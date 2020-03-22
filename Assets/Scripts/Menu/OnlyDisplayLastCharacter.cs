using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OnlyDisplayLastCharacter : MonoBehaviour {

    public InputField input;

    protected void Start() {
        input.onValueChanged.AddListener(OnValueChanged);
    }

    public void OnValueChanged(string str) {
        //string chars = "";
        //for (int i = 0; i < str.Length - 1; i++)
        //    chars += "*";
        //chars += str[str.Length - 1];
        ////input.asteriskChar = chars[str.Length - 1];
        //input.SetTextWithoutNotify(chars);
    }
}
