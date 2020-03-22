using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PouvoirDisplay : MonoBehaviour {

    public Text textName;
    public Text textDescription;
    public Image image;

    public void Initialize(string name, string description, Sprite sprite) {
        textName.text = textName.text.Replace("%PouvoirName%", name);
        textDescription.text = description;
        this.image.sprite = sprite;
    }
}
