using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.ResourceManagement.AsyncOperations;
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
    protected List<Button> addedButtons = new List<Button>();
    [HideInInspector]
    public UnityEvent onDisable;

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

    public void RunPopup(string title, string text, TexteExplicatif.Theme theme, bool cleanReplacements = true) {
        Initialize(title: title, mainText: text, theme: theme, cleanReplacements: cleanReplacements);
        Run();
    }

    public void RunPopup(LocalizedString title, LocalizedString text, TexteExplicatif.Theme theme, bool cleanReplacements = true) {
        StartCoroutine(CRunPopup(title, text, theme, cleanReplacements));
    }

    public IEnumerator CRunPopup(LocalizedString title, LocalizedString text, TexteExplicatif.Theme theme, bool cleanReplacements = true) {
        AsyncOperationHandle<string> handleTitle = title.GetLocalizedString();
        yield return handleTitle;
        string titleString = handleTitle.Result; // Car les AsyncOperationHandle doivent être utilisé l'un après l'autre ! x)

        AsyncOperationHandle<string> handleText = text.GetLocalizedString();
        yield return handleText;
        string textString = handleText.Result;

        RunPopup(titleString, textString, theme, cleanReplacements);
    }


    public void InitTresholdText() {
        tresholdText = new TresholdText(textAsset);
    }

    public void Run(int textTreshold = 0, bool shouldInitTresholdText = true, ReplacementStrings replacements = null, TMPro.TMP_SpriteAsset imagesAtlas = null) {
        SetColorTheme(currentTheme);
        InitColor();

        if(imagesAtlas != null) {
            mainText.spriteAsset = imagesAtlas;
        }

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

        onDisable.RemoveAllListeners();
        StartRevealingCharacters();
        StartAnimation();
    }

    public void AddButton(LocalizedString text, LocalizedString tooltipText, Theme theme, UnityAction action, int siblingIndex = 0) {
        Button addedButton = Instantiate(doneButton, parent: doneButton.transform.parent).GetComponent<Button>();
        addedButton.transform.SetSiblingIndex(siblingIndex);
        addedButton.gameObject.SetActive(true);
        addedButton.GetComponent<Image>().material = GetButtonMaterialForTheme(theme);
        addedButton.GetComponent<UpdateUnscaledTime>().Start();
        addedButton.GetComponentInChildren<LocalizeStringEvent>().StringReference = text;
        addedButton.GetComponent<TooltipActivator>().localizedMessage = tooltipText;
        if (action != null) {
            addedButton.onClick.AddListener(action);
        }
        addedButtons.Add(addedButton);
    }

    public void EnableButtonsBlackBackground() {
        doneButton.transform.parent.GetComponent<Image>().enabled = true;
    }

    // Attention, n'ajoute pas d'action sur le doneButton ! Si on le fait il faudra aussi penser à l'enlever mais ça je vois pas trop comment faire pour le moment :)
    public void AddActionOnStartAndEnd(UnityAction onStartPopup, UnityAction onEndPopup) {
        onStartPopup.Invoke();
        foreach(Button button in addedButtons) {
            button.onClick.AddListener(onEndPopup);
        }
    }

    public void RemoveDoneButton() {
        doneButton.gameObject.SetActive(false);
    }

    public void ResetDoneButton() {
        doneButton.gameObject.SetActive(true);
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
        ScrollRect sr = mainText.transform.parent.GetComponent<ScrollRect>();
        bool hasScroll = false;
        while (!timer.IsOver()) {
            int nbCharactersVisibles = (int)(nbCharacters * timer.GetAvancement());
            mainText.maxVisibleCharacters = nbCharactersVisibles;
            if(Input.anyKey) {
                float currentAvancement = timer.GetAvancement();
                timer.SetDuree(timer.GetDuree() / 5);
                timer.SetAvancement(currentAvancement);
            }
            if(Input.mouseScrollDelta != Vector2.zero) {
                hasScroll = true;
            }
            if(!hasScroll) {
                sr.verticalScrollbar.value = 1;
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
        //&& !MenuManager.DISABLE_HOTKEYS // Je ne me rappelle plus pourquoi on avait besoin de cette ligne !
        && (Input.GetKeyDown(KeyCode.Escape)
        || Input.GetKeyDown(KeyCode.KeypadEnter) 
        || Input.GetKeyDown(KeyCode.Return) 
        || (!dontDisableOnSpace && Input.GetKeyDown(KeyCode.Space)))) {
            Disable();
        }
        firstFrame = false;
    }

    public void Disable() {
        onDisable.Invoke();
        EndAnimation();
        StartCoroutine(CDisableIn());
    }

    protected void RemoveAddedButtons() {
        foreach(Button button in addedButtons) {
            Destroy(button.gameObject);
        }
        doneButton.transform.parent.GetComponent<Image>().enabled = false;
        addedButtons.Clear();
    }

    protected IEnumerator CDisableIn() {
        if(isInGame)
            yield return new WaitForSecondsRealtime(dureeCloseAnimation);
        else
            yield return new WaitForSeconds(dureeCloseAnimation);
        RemoveAddedButtons();
        ResetDoneButton();
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
                break;
            case Theme.NEGATIF:
                materialExternalTerminal = materialExternalTerminalNegatif;
                materialInternalTerminal = materialInternalTerminalNegatif;
                break;
            case Theme.NEUTRAL:
                materialExternalTerminal = materialExternalTerminalNeutral;
                materialInternalTerminal = materialInternalTerminalNeutral;
                break;
            default:
                break;
        }
        materialButton = GetButtonMaterialForTheme(theme);
    }

    public Material GetButtonMaterialForTheme(Theme theme) {
        switch (theme) {
            case Theme.POSITIF:
                return materialButtonPositif;
            case Theme.NEGATIF:
                return materialButtonNegatif;
            case Theme.NEUTRAL:
                return materialButtonNeutral;
            default:
                return materialButtonPositif;
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
