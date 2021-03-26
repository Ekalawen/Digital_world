using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour {

    public MenuOptions menuOptions;

    protected GameManager gm;
    protected InputManager inputManager;

    public void Start() {
        gm = GameManager.Instance;
        inputManager = InputManager.Instance;
    }

    public void Update() {
        if(gm.IsPaused()) {
            if(inputManager.GetPauseReturnToMenu()) {
                Quitter();
            }
            if(inputManager.GetPauseGame()) {
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
