using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class TexteExplicatif : MonoBehaviour {

    public static string ROOT_REPOSITORY = "Assets/Texts/";
    public static string ROOT_LEVELS_REPOSITORY = ROOT_REPOSITORY + "Levels/";

    public enum Theme { POSITIF, NEGATIF, NEUTRAL };

    public GameObject content;
    public Text titleTextSource;
    public Text titleTextTarget;
    public Text mainText;
    public bool useTextAsset = false;
    public TextAsset textAsset;

    public Color color;
    public Button doneButton;
    public Image fondSombre;
    public Color positifThemeColor;
    public Color negatifThemeColor;
    public Color neutralThemeColor;

    public bool dontDisableOnSpace = false;

    protected SelectorManager selectorManager;
    protected bool firstFrame;
    protected TresholdText tresholdText;
    protected string rootPath = "";
    protected List<Tuple<string, string>> replacementList = new List<Tuple<string, string>>();
    protected List<Tuple<string, MatchEvaluator>> replacementListEvaluator = new List<Tuple<string, MatchEvaluator>>();

    public void Start() {
        selectorManager = SelectorManager.Instance;
    }

    public void Initialize(string title = "",
        string mainText = "",
        bool useTextAsset = false,
        TextAsset textAsset = null,
        Theme theme = Theme.POSITIF,
        bool cleanReplacements = true) {
        if (cleanReplacements) {
            CleanReplacements();
        }
        this.titleTextTarget.text = title;
        this.mainText.text = mainText;
        this.useTextAsset = useTextAsset;
        this.textAsset = textAsset;
        SetColorTheme(theme);
    }

    protected void InitTresholdText() {
        tresholdText = new TresholdText(textAsset);
    }

    public void Run(int textTreshold = 0) {
        InitColor();

        if (useTextAsset) {
            InitTresholdText();
        }

        content.SetActive(true);
        DisableHotkeys();
        if(titleTextSource != null && titleTextTarget != null)
            titleTextTarget.text = titleTextSource.text;
        firstFrame = true;

        if (useTextAsset) {
            mainText.text = ComputeText(textTreshold);
        } else {
            mainText.text = UseReplacementList(mainText.text);
        }

        PutMainTextOnBottom();
    }

    public string ComputeText(int textTreshold = 0) {
        return UseReplacementList(GetTextFromPath(textTreshold));
    }

    protected void PutMainTextOnBottom() {
        Vector3 pos = mainText.rectTransform.position;
        pos.y -= 1000;
        mainText.rectTransform.position = pos;
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

    public void SetColorTheme(Theme theme) {
        switch(theme) {
            case Theme.POSITIF:
                color = positifThemeColor;
                break;
            case Theme.NEGATIF:
                color = negatifThemeColor;
                break;
            case Theme.NEUTRAL:
                color = neutralThemeColor;
                break;
            default:
                break;
        }
    }

    protected void InitColor() {
        fondSombre.color = color;
        Color saturated = color;
        saturated.a = 1.0f;
        doneButton.GetComponent<Image>().color = saturated;
    }

    public TresholdText GetTresholdText() {
        return tresholdText;
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

    public void ApplyReplacementEvaluatorToAllFragments(Tuple<string, MatchEvaluator> replacement) {
        tresholdText.ApplyReplacementEvaluatorToAllFragment(replacement);
    }

    public void CleanReplacements() {
        replacementList.Clear();
        replacementListEvaluator.Clear();
    }

    public static string SurroundWithRedColor(Match match) {
        return $"<color={UIHelper.RED}>" + match.Value + "</color>";
    }

    public static string SurroundWithGreenColor(Match match) {
        return $"<color={UIHelper.GREEN}>" + match.Value + "</color>";
    }

    public static string SurroundWithBlueColor(Match match) {
        return $"<color={UIHelper.BLUE}>" + match.Value + "</color>";
    }
}
