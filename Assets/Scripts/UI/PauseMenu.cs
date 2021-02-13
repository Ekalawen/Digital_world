using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour {

    public MenuOptions menuOptions;

    protected GameManager gm;

    public void Start() {
        gm = GameManager.Instance;
    }

    public void Update() {
        if(gm.IsPaused()) {
            if(Input.GetKeyDown(KeyCode.Space)) {
                Quitter();
            }
            if(Input.GetKeyDown(KeyCode.O)) {
                Options();
            }
            // R and ESCAPE are already handled
        }
    }

    public void Reprendre() {
        gm.UnPause();
    }

    public void Recommencer() {
        gm.RestartGame();
    }

    public void Options() {
        Tooltip.Hide();
        menuOptions.Run();
    }

    public void Quitter() {
        gm.QuitOrReloadGame();
    }
}
