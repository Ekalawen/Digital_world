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
            if(Input.GetKeyDown(KeyCode.Escape)) {
                menuOptions.ResetMenu();
                menuOptions.gameObject.SetActive(false);
                Reprendre();
            }
            // O and R are already handled
        }
    }

    public void Reprendre() {
        gm.UnPause();
    }

    public void Recommencer() {
        gm.RestartGame();
    }

    public void Options() {
        menuOptions.gameObject.SetActive(true);
        menuOptions.Run();
        Tooltip.Hide();
    }

    public void Quitter() {
        gm.QuitOrReloadGame();
    }
}
