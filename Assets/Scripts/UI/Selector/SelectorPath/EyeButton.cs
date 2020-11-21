using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EyeButton : MonoBehaviour {

    public InputField inputField;
    public GameObject otherEye;

    public void SetPasswordMode()
    {
        inputField.contentType = InputField.ContentType.Password;
        OnlyDisplayOnTyping.NotifyInput(inputField);
        SetEnableOnlyDisplay(true);
        otherEye.SetActive(true);
        gameObject.SetActive(false);
    }

    public void SetStandardMode() {
        inputField.contentType = InputField.ContentType.Standard;
        OnlyDisplayOnTyping.NotifyInput(inputField);
        SetEnableOnlyDisplay(false);
        otherEye.SetActive(true);
        gameObject.SetActive(false);
    }

    protected void SetEnableOnlyDisplay(bool value) {
        OnlyDisplayOnTyping onlyDisplay = inputField.gameObject.GetComponent<OnlyDisplayOnTyping>();
        if (onlyDisplay != null) {
            Debug.Log($"New value = {value}");
            onlyDisplay.enabled = value;
        }
    }
}
