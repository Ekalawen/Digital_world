using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundStarter : MonoBehaviour {

    public MenuBackgroundBouncing background;
    public float probaSource;
    public int distanceSource = 5;
    public float decroissanceSource;
    public List<ColorSource.ThemeSource> themes;

    public void Start() {
        background.Initialize();
        background.SetParameters(probaSource, distanceSource, decroissanceSource, themes);
    }
}
