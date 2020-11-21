using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PouvoirDisplay : MonoBehaviour {

    public static string NULL_NAME_VALUE = "404 Not Found";
    public static string NULL_DESCRIPTION_VALUE = "Null";

    public TMPro.TMP_Text textName;
    public TMPro.TMP_Text textDescription;
    public Image image;
    public Image bordure;
    public Color bordureColorActive;
    public Color bordureColorSpecial;

    public void Initialize(string name, string description, Sprite sprite) {
        textName.text = textName.text.Replace("%PouvoirName%", name);
        textDescription.text = description;
        if(name != NULL_NAME_VALUE) {
            if(name != "PathFinder" && name != "Localisateur")
                bordure.color = bordureColorSpecial;
            else
                bordure.color = bordureColorActive;
        }
        this.image.sprite = sprite;
        if(sprite != null)
            this.image.color = Color.white; // Sinon c'est transparent !
    }
}
