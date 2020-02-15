using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TexteExplicatif : MonoBehaviour {

    public GameObject content;
    public Text titleTextSource;
    public Text titleTextTarget;
    public Text mainText;

    protected bool firstFrame;

    public void Run() {
        content.SetActive(true);
        DisableHotkeys();
        if(titleTextSource != null && titleTextTarget != null)
            titleTextTarget.text = titleTextSource.text;
        firstFrame = true;

        //mainText.font.lineHeight;
        //mainText.text.
    }

    private void Update() {
        if(!firstFrame && content.activeInHierarchy && Input.anyKeyDown) {
            content.SetActive(false);
            EnableHotkeys();
        }
        firstFrame = false;
    }

    public void DisableHotkeys() {
        MenuManager.DISABLE_HOTKEYS = true;
    }
    public void EnableHotkeys() {
        MenuManager.DISABLE_HOTKEYS = false;
    }
    public void EnableHotkeysNextFrame() {
        StartCoroutine(CEnableHotkeysNextFrame());
    }
    protected IEnumerator CEnableHotkeysNextFrame() {
        yield return new WaitForEndOfFrame();
        EnableHotkeys();
    }
}
