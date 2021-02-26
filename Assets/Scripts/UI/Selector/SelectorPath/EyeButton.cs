using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EyeButton : MonoBehaviour {

    public enum EyeMode { PASSWORD, STANDARD };

    public InputField inputField;
    public GameObject otherEye;
    public EyeMode mode;

    public void Start() {
        if(PrefsManager.GetString(PrefsManager.EYE_MODE_KEY, EyeMode.PASSWORD.ToString()) == EyeMode.PASSWORD.ToString()) {
            SetPasswordMode();
        } else {
            SetStandardMode();
        }
    }

    public void SetPasswordMode() {
        inputField.contentType = InputField.ContentType.Password;
        OnlyDisplayOnTyping.NotifyInput(inputField);

        SetEnableOnlyDisplay(true);
        PrefsManager.SetString(PrefsManager.EYE_MODE_KEY, EyeMode.PASSWORD.ToString());

        otherEye.SetActive(true);
        gameObject.SetActive(false);
    }

    public void SetStandardMode() {
        inputField.contentType = InputField.ContentType.Standard;
        OnlyDisplayOnTyping.NotifyInput(inputField);

        SetEnableOnlyDisplay(false);
        PrefsManager.SetString(PrefsManager.EYE_MODE_KEY, EyeMode.STANDARD.ToString());

        otherEye.SetActive(true);
        gameObject.SetActive(false);
    }

    protected void SetEnableOnlyDisplay(bool value) {
        OnlyDisplayOnTyping onlyDisplay = inputField.gameObject.GetComponent<OnlyDisplayOnTyping>();
        if (onlyDisplay != null) {
            onlyDisplay.enabled = value;
        }
    }
}
