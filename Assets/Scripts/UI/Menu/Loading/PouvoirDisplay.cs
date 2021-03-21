using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class PouvoirDisplay : MonoBehaviour {

    public TMPro.TMP_Text textName;
    public TMPro.TMP_Text textDescription;
    public LocalizedString keyName;
    public LocalizedString levelToUnlockPouvoirName;
    public Image image;
    public Image bordure;
    public Color bordureColorActive;
    public Color bordureColorSpecial;

    public void Initialize(string name, string keyName, string description, Sprite sprite) {
        textName.text = textName.text.Replace("%PouvoirName%", name);
        textName.text = textName.text.Replace("%ToucheName%", keyName);
        textDescription.text = description;
        if(name != GetNullName()) {
            if(name != "PathFinder" && name != "Localisateur")
                bordure.color = bordureColorSpecial;
            else
                bordure.color = bordureColorActive;
        }
        this.image.sprite = sprite;
        if(sprite != null)
            this.image.color = Color.white; // Sinon c'est transparent !
    }

    public static string GetNullName() {
        return LocalizationSettings.StringDatabase.GetLocalizedStringAsync("Pouvoirs", "PouvoirNullName").Result;
    }

    public static string GetNullDescription(string levelName) {
        return LocalizationSettings.StringDatabase.GetLocalizedStringAsync("Pouvoirs", "PouvoirNullDescription", new object[] { levelName }).Result;
    }
}
