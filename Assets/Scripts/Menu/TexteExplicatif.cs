using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TexteExplicatif : MonoBehaviour {

    public GameObject content;

    public void Run() {
        content.SetActive(true);
        MenuManager.DISABLE_HOTKEYS = true;
    }

    private void Update() {
        if(content.activeInHierarchy && Input.anyKeyDown) {
            content.SetActive(false);
            MenuManager.DISABLE_HOTKEYS = false;
        }
    }
}
