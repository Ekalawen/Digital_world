using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class TexteExplicatif : MonoBehaviour {

    public static string ROOT_REPOSITORY = "Assets/Texts/";
    public static string ROOT_LEVELS_REPOSITORY = ROOT_REPOSITORY + "Levels/";

    public GameObject content;
    public Text titleTextSource;
    public Text titleTextTarget;
    public Text mainText;
    public bool useTextPath = false;
    public string textPath;

    protected bool firstFrame;
    protected TresholdText tresholdText;
    protected string rootPath = "";

    public void Run(int textTreshold = 0) {
        content.SetActive(true);
        DisableHotkeys();
        if(titleTextSource != null && titleTextTarget != null)
            titleTextTarget.text = titleTextSource.text;
        firstFrame = true;

        if (useTextPath) {
            tresholdText = new TresholdText(rootPath + textPath);
            mainText.text = GetTextFromPath(textTreshold);
        }
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

    protected string GetTextFromPath(int textTreshold) {
        return tresholdText.GetUnderTresholdFragmentsString(textTreshold);
    }

    public void SetRootPath(string path) {
        rootPath = path;
    }
}
