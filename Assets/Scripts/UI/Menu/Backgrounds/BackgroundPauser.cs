using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundPauser : MonoBehaviour {

    public MenuBackgroundBouncing background;

    public void Update() {
        if (Input.GetKeyDown(KeyCode.P)) {
            if(background.IsPaused()) {
                background.Unpause();
            } else {
                background.Pause();
            }
        }
    }
}
