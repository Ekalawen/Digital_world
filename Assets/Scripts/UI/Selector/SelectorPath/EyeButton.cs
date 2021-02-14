using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EyeButton : MonoBehaviour {

    public static string EYE_MODE_KEY = "eyeModeKey";

    public enum EyeMode { PASSWORD, STANDARD };

    public InputField inputField;
    public GameObject otherEye;
    public EyeMode mode;

    public void Start() {
        if(!PlayerPrefs.HasKey(EYE_MODE_KEY) || PlayerPrefs.GetString(EYE_MODE_KEY) == EyeMode.PASSWORD.ToString()) {
            SetPasswordMode();
        } else {
            SetStandardMode();
        }
    }

    public void SetPasswordMode() {
        inputField.contentType = InputField.ContentType.Password;
        OnlyDisplayOnTyping.NotifyInput(inputField);

        SetEnableOnlyDisplay(true);
        PlayerPrefs.SetString(EYE_MODE_KEY, EyeMode.PASSWORD.ToString());

        otherEye.SetActive(true);
        gameObject.SetActive(false);
    }

    public void SetStandardMode() {
        inputField.contentType = InputField.ContentType.Standard;
        OnlyDisplayOnTyping.NotifyInput(inputField);

        SetEnableOnlyDisplay(false);
        PlayerPrefs.SetString(EYE_MODE_KEY, EyeMode.STANDARD.ToString());

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
