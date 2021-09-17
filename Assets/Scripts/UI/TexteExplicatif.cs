using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class TexteExplicatif : MonoBehaviour {

    public static string ROOT_REPOSITORY = "Assets/Texts/";
    public static string ROOT_LEVELS_REPOSITORY = ROOT_REPOSITORY + "Levels/";

    public enum Theme { POSITIF, NEGATIF, NEUTRAL };

    public float nbCharactersPrintedBySeconds = 150;
    public bool dontDisableOnSpace = false;
    public bool isInGame = false;

    [Header("Links")]
    public GameObject content;
    public Text titleTextSource;
    public TMPro.TMP_Text titleTextTarget;
    public TMPro.TMP_Text mainText;
    public bool useTextAsset = false;
    public TextAsset textAsset;
    public Button doneButton;
    public Image externalTerminal;
    public Image internalTerminal;

    [Header("Animation")]
    public float dureeOpenAnimation = 0.4f;
    public float dureeCloseAnimation = 0.2f;

    [Header("Themes")]
    public Material materialInternalTerminalPositif;
    public Material materialInternalTerminalNegatif;
    public Material materialInternalTerminalNeutral;
    public Material materialExternalTerminalPositif;
    public Material materialExternalTerminalNegatif;
    public Material materialExternalTerminalNeutral;
    public Material materialButtonPositif;
    public Material materialButtonNegatif;
    public Material materialButtonNeutral;

    protected SelectorManager selectorManager;
    protected bool firstFrame;
    protected TresholdText tresholdText;
    protected string rootPath = "";
    protected List<Tuple<string, string>> replacementList = new List<Tuple<string, string>>();
    protected List<Tuple<string, MatchEvaluator>> replacementListEvaluator = new List<Tuple<string, MatchEvaluator>>();
    protected Theme currentTheme;
    protected Material materialInternalTerminal;
    protected Material materialExternalTerminal;
    protected Material materialButton;
    protected Fluctuator animationFluctuator;

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
        this.currentTheme = theme;
        SetColorTheme(theme);
        animationFluctuator = new Fluctuator(this, GetPopupScale, SetPopupScale, useUnscaleTime: isInGame);
    }

    public void InitTresholdText() {
        tresholdText = new TresholdText(textAsset);
    }

    public void Run(int textTreshold = 0, bool shouldInitTresholdText = true, ReplacementStrings replacements = null) {
        SetColorTheme(currentTheme);
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
        mainText.text = ApplyColorReplacements(mainText.text);

        if(replacements != null) {
            mainText.text = UIHelper.ApplyReplacementList(mainText.text, replacements);
        }

        //PutMainTextOnBottom();

        StartRevealingCharacters();
        StartAnimation();
    }

    public float GetPopupScale() {
        return externalTerminal.rectTransform.localScale.x;
    }

    public void SetPopupScale(float newScale) {
        externalTerminal.rectTransform.localScale = Vector3.one * newScale;
    }

    protected void StartAnimation() {
        SetPopupScale(0);
        animationFluctuator.GoTo(1, dureeOpenAnimation);
    }

    protected void EndAnimation() {
        animationFluctuator.GoTo(0, dureeCloseAnimation);
    }

    protected void StartRevealingCharacters() {
        StartCoroutine(CStartRevealingCharacters());
    }

    protected IEnumerator CStartRevealingCharacters() {
        int nbCharacters = mainText.text.Length;
        mainText.maxVisibleCharacters = 0;
        float timerDuration = (float)nbCharacters / nbCharactersPrintedBySeconds;
        Timer timer = isInGame ? new UnpausableTimer(timerDuration) : new Timer(timerDuration);
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
        && !MenuManager.DISABLE_HOTKEYS
        && (Input.GetKeyDown(KeyCode.Escape)
        || Input.GetKeyDown(KeyCode.KeypadEnter) 
        || Input.GetKeyDown(KeyCode.Return) 
        || (!dontDisableOnSpace && Input.GetKeyDown(KeyCode.Space)))) {
            Disable();
        }
        firstFrame = false;
    }

    public void Disable()
    {
        EndAnimation();
        StartCoroutine(CDisableIn());
    }

    protected IEnumerator CDisableIn() {
        if(isInGame)
            yield return new WaitForSecondsRealtime(dureeCloseAnimation);
        else
            yield return new WaitForSeconds(dureeCloseAnimation);
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
                materialExternalTerminal = materialExternalTerminalPositif;
                materialInternalTerminal = materialInternalTerminalPositif;
                materialButton = materialButtonPositif;
                break;
            case Theme.NEGATIF:
                materialExternalTerminal = materialExternalTerminalNegatif;
                materialInternalTerminal = materialExternalTerminalNegatif;
                materialButton = materialButtonNegatif;
                break;
            case Theme.NEUTRAL:
                materialExternalTerminal = materialExternalTerminalNeutral;
                materialInternalTerminal = materialInternalTerminalNeutral;
                materialButton = materialButtonNeutral;
                break;
            default:
                break;
        }
    }

    protected void InitColor() {
        externalTerminal.material = materialExternalTerminal;
        internalTerminal.material = materialInternalTerminal;
        doneButton.GetComponent<Image>().material = materialButton;
        externalTerminal.GetComponent<UpdateUnscaledTime>().Start();
        internalTerminal.GetComponent<UpdateUnscaledTime>().Start();
        doneButton.GetComponent<UpdateUnscaledTime>().Start();
    }

    public static string ApplyColorReplacements(string originalText) {
        foreach(Tuple<string, string> color in UIHelper.GetColorMapping()) {
            originalText = originalText.Replace(color.Item1, color.Item2);
        }
        return originalText;
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

    public int GetMaxTreshold() {
        return GetTresholdText().GetAllTresholds().Last();
    }
}
