﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class TexteExplicatif : MonoBehaviour {

    public static string ROOT_REPOSITORY = "Assets/Texts/";
    public static string ROOT_LEVELS_REPOSITORY = ROOT_REPOSITORY + "Levels/";

    public GameObject content;
    public Text titleTextSource;
    public Text titleTextTarget;
    public Text mainText;
    public bool useTextAsset = false;
    public TextAsset textAsset;

    public Color color;
    public Button doneButton;
    public Image fondSombre;

    public bool dontDisableOnSpace = false;

    protected bool firstFrame;
    protected TresholdText tresholdText;
    protected string rootPath = "";
    protected List<Tuple<string, string>> replacementList = new List<Tuple<string, string>>();
    protected List<Tuple<string, MatchEvaluator>> replacementListEvaluator = new List<Tuple<string, MatchEvaluator>>();

    public void Start() {
        InitColor();

        if (useTextAsset) {
            InitTresholdText();
        }
    }

    protected void InitTresholdText() {
        tresholdText = new TresholdText(textAsset);
    }

    public void Run(int textTreshold = 0) {
        content.SetActive(true);
        DisableHotkeys();
        if(titleTextSource != null && titleTextTarget != null)
            titleTextTarget.text = titleTextSource.text;
        firstFrame = true;

        if (useTextAsset) {
            string newText = UseReplacementList(GetTextFromPath(textTreshold));
            mainText.text = newText;
        }
    }

    public void SetText(string newTitle, string newText) {
        titleTextTarget.text = newTitle;
        mainText.text = newText;
    }

    private void Update() {
        if(content.activeInHierarchy
        && !firstFrame /*&& content.activeInHierarchy*/
        && (Input.GetKeyDown(KeyCode.Escape)
        || Input.GetKeyDown(KeyCode.KeypadEnter) 
        || Input.GetKeyDown(KeyCode.Return) 
        || (!dontDisableOnSpace && Input.GetKeyDown(KeyCode.Space)))) {
            Disable();
        }
        firstFrame = false;
    }

    public void Disable() {
        EnableHotkeysNextFrame();
        content.SetActive(false);
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

    public void AddReplacementEvaluator(string source, MatchEvaluator cible) {
        replacementListEvaluator.Add(new Tuple<string, MatchEvaluator>(source, cible));
    }

    public string UseReplacementList(string text) {
        foreach(Tuple<string, string> replacement in replacementList) {
            string source = replacement.Item1;
            string cible = replacement.Item2;
            text = text.Replace(source, cible);
        }
        foreach(Tuple<string, MatchEvaluator> replacement in replacementListEvaluator) {
            string source = replacement.Item1;
            MatchEvaluator evaluator = replacement.Item2;
            text = Regex.Replace(text, source, evaluator);
        }
        return text;
    }

    protected void InitColor() {
        fondSombre.color = color;
        Color saturated = color;
        saturated.a = 1.0f;
        doneButton.GetComponent<Image>().color = saturated;
    }

    public List<int> GetAllTresholds() {
        if(useTextAsset) {
            if (tresholdText == null)
                InitTresholdText();
            return tresholdText.GetAllTresholds();
        } else {
            return new List<int>();
        }
    }
}
