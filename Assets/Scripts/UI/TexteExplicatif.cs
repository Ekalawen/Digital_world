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
    public TMPro.TMP_Text titleTextTarget;
    public TMPro.TMP_Text mainText;
    public bool useTextAsset = false;
    public TextAsset textAsset;

    public Color color;
    public Button doneButton;
    public Image fondSombre;
    public Image fondClair;
    public Color positifThemeColor;
    public Color negatifThemeColor;
    public Color neutralThemeColor;

    public float nbCharactersPrintedBySeconds = 150;

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
        SetText(mainText);
        this.useTextAsset = useTextAsset;
        this.textAsset = textAsset;
        SetColorTheme(theme);
    }

    public void InitTresholdText() {
        tresholdText = new TresholdText(textAsset);
    }

    public void Run(int textTreshold = 0, bool shouldInitTresholdText = true) {
        InitColor();

        if (useTextAsset && shouldInitTresholdText) {
            InitTresholdText();
        }

        content.SetActive(true);
        DisableHotkeys();
        if(titleTextSource != null && titleTextTarget != null)
            titleTextTarget.text = titleTextSource.text;
        firstFrame = true;

        if (useTextAsset) {
            SetText(ComputeText(textTreshold));
        } else {
            SetText(UseReplacementList(mainText.text));
        }
        ApplyColorReplacements();

        //PutMainTextOnBottom();

        StartRevealingCharacters();
    }

    protected void StartRevealingCharacters() {
        StartCoroutine(CStartRevealingCharacters());
    }

    protected IEnumerator CStartRevealingCharacters() {
        int nbCharacters = mainText.text.Length;
        mainText.maxVisibleCharacters = 0;
        Timer timer = new Timer((float)nbCharacters / nbCharactersPrintedBySeconds);
        while (!timer.IsOver()) {
            int nbCharactersVisibles = (int)(nbCharacters * timer.GetAvancement());
            mainText.maxVisibleCharacters = nbCharactersVisibles;
            ScrollRect sr = mainText.transform.parent.GetComponent<ScrollRect>();
            sr.verticalScrollbar.value = 1;
            if(Input.anyKey || Input.mouseScrollDelta != Vector2.zero) {
                break;
            }
            yield return null;
        }
        mainText.maxVisibleCharacters = nbCharacters;
    }

    public string ComputeText(int textTreshold = 0) {
        return UseReplacementList(GetTextFromPath(textTreshold));
    }

    protected void PutMainTextOnBottom() {
        Vector3 pos = mainText.rectTransform.position;
        pos.y -= 1000;
        mainText.rectTransform.position = pos;
    }

    public void SetTextAndTitle(string newTitle, string newText) {
        titleTextTarget.text = newTitle;
        SetText(newText);
    }

    public void SetText(string newText) {
        mainText.text = UIHelper.SurroundWithColorWithoutB(newText, UIHelper.WHITE);
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
        Color halfSaturated = color;
        halfSaturated.a = 0.90f;
        fondClair.color = halfSaturated;
        Color saturated = color;
        saturated.a = 1.0f;
        doneButton.GetComponent<Image>().color = saturated;
    }

    public void ApplyColorReplacements() {
        string t = mainText.text;
        foreach(Tuple<string, string> color in UIHelper.GetColorMapping()) {
            t = t.Replace(color.Item1, color.Item2);
        }
        mainText.text = t;
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
        return UIHelper.SurroundWithColor(match.Value, UIHelper.RED);
    }

    public static string SurroundWithGreenColor(Match match) {
        return UIHelper.SurroundWithColor(match.Value, UIHelper.GREEN);
    }

    public static string SurroundWithBlueColor(Match match) {
        return UIHelper.SurroundWithColor(match.Value, UIHelper.BLUE);
    }

    public static string SurroundWithOrangeColor(Match match) {
        return UIHelper.SurroundWithColor(match.Value, UIHelper.ORANGE);
    }

    public void HighlightDoneButton(bool state) {
        doneButton.GetComponent<ButtonHighlighter>().enabled = state;
    }
}
