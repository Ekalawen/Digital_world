using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderScript : MonoBehaviour {
    public Text text;

    public void OnChange(float newValue) {
        text.text = newValue.ToString("N2");
    }
}
