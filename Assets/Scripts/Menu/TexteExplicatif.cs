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
    protected List<Tuple<string, string>> replacementList = new List<Tuple<string, string>>();

    public void Run(int textTreshold = 0) {
        content.SetActive(true);
        DisableHotkeys();
        if(titleTextSource != null && titleTextTarget != null)
            titleTextTarget.text = titleTextSource.text;
        firstFrame = true;

        if (useTextPath) {
            tresholdText = new TresholdText(rootPath + textPath);
            string newText = UseReplacementList(GetTextFromPath(textTreshold));
            mainText.text = newText;
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

    public void AddReplacement(string source, string cible) {
        replacementList.Add(new Tuple<string, string>(source, cible));
    }

    public string UseReplacementList(string text) {
        foreach(Tuple<string, string> replacement in replacementList) {
            string source = replacement.Item1;
            string cible = replacement.Item2;
            text = text.Replace(source, cible);
        }
        return text;
    }
}
