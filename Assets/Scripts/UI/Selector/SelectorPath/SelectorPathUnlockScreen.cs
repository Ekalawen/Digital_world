using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class SelectorPathUnlockScreen : MonoBehaviour {

    [Header("Links")]
    public TMPro.TMP_Text fromLevelTitle;
    public TMPro.TMP_Text toLevelTitle;
    public GameObject openCadena;
    public GameObject closedCadena;
    public InputField input;
    public Button hackButton;
    public Button donneesHackeesButton;
    public Button returnButton;
    public TMPro.TMP_Text traceHintText;
    public GameObject traceHintContainer;
    public SelectorPathArrow arrow;

    [Header("Background Unlocked theme")]
    public float probaBackgroundUnlocked;
    public float decroissanceBackgroundUnlocked;
    public int distanceBackgroundUnlocked;
    public List<ColorManager.Theme> themesBackgroundUnlocked;
    public Material materialTitleUnlocked;

    [Header("Background Locked theme")]
    public float probaBackgroundLocked;
    public float decroissanceBackgroundLocked;
    public int distanceBackgroundLocked;
    public List<ColorManager.Theme> themesBackgroundLocked;
    public Material materialTitleLocked;

    [Header("Unlock Animation")]
    public float dureeUnlockAnimation = 1.3f;
    public int nbTurnsUnlockAnimation = 10;
    public AnimationCurve curveUnlockAnimation;
    public GameObject particlesUnlockAnimationPrefab;
    public float dureeUnlockAnimationParticleSystem = 1.4f;
    public float unlockAnimationCancelSoundAcceleration = 4.0f;

    [Header("FastUI")]
    public GameObject fastUISystemNextPrefab;
    public GameObject fastUISystemPreviousPrefab;
    public RectTransform fastUISystemNextTransform;
    public RectTransform fastUISystemPreviousTransform;
    public Button cyclicDHLeftButton;
    public Button cyclicDHRightButton;

    protected SelectorManager selectorManager;
    [HideInInspector]
    public SelectorPath selectorPath;
    protected Fluctuator cadenaAnimationFluctuator;
    protected Coroutine unlockAnimationCoroutine = null;
    protected List<GameObject> particlesToDestroy = new List<GameObject>();

    public void Initialize(SelectorPath selectorPath, bool shouldHighlightDataHackees) {
        this.selectorManager = SelectorManager.Instance;
        this.selectorPath = selectorPath;
        //SetBackgroundAccordingToLockState();
        SetCadenasAndArrowAndTitlesAccordingToLockState();
        SetTitles();
        FillInputWithPasswordIfAlreayDiscovered();
        FillTraceHint();
        HighlightDataHackees(shouldHighlightDataHackees);
        GenerateNextAndPreviousButtons();
        InitializeCyclicDHButtons();
        cadenaAnimationFluctuator = new Fluctuator(this, GetCadenasRotation, SetCadenasRotation);
    }

    protected void GenerateNextAndPreviousButtons() {
        if (fastUISystemNextTransform.childCount > 0 || fastUISystemPreviousTransform.childCount > 0) {
            foreach(Transform t in fastUISystemNextTransform) {
                Destroy(t.gameObject);
            }
            foreach(Transform t in fastUISystemPreviousTransform) {
                Destroy(t.gameObject);
            }
        }
        FastUISystem fastUISystemForward = Instantiate(fastUISystemNextPrefab, fastUISystemNextTransform).GetComponent<FastUISystem>();
        fastUISystemForward.Initialize(selectorPath, FastUISystem.DirectionType.FORWARD, FastUISystem.FromType.UNLOCK_SCREEN);
        FastUISystem fastUISystemBackward = Instantiate(fastUISystemPreviousPrefab, fastUISystemPreviousTransform).GetComponent<FastUISystem>();
        fastUISystemBackward.Initialize(selectorPath, FastUISystem.DirectionType.BACKWARD, FastUISystem.FromType.UNLOCK_SCREEN);
    }

    protected void InitializeCyclicDHButtons() {
        SelectorLevel nextLevel = selectorPath.endLevel;
        SelectorLevel previousLevel = selectorPath.startLevel;
        List<SelectorPath> outPaths = selectorManager.GetOutPaths(previousLevel);
        List<SelectorPath> inPaths = selectorManager.GetInPaths(nextLevel);
        if(outPaths.Count > 1 || inPaths.Count > 1) {
            List<SelectorPath> interestingPaths = Enumerable.Union(outPaths, inPaths).ToList();
            int currentIndice = interestingPaths.FindIndex(p => p == selectorPath);
            InitOneCyclicDHButton(cyclicDHRightButton, interestingPaths[(currentIndice + 1) % interestingPaths.Count]);
            InitOneCyclicDHButton(cyclicDHLeftButton, interestingPaths[(currentIndice - 1 + interestingPaths.Count) % interestingPaths.Count]);
        } else {
            cyclicDHLeftButton.gameObject.SetActive(false);
            cyclicDHRightButton.gameObject.SetActive(false);
        }
    }

    protected void InitOneCyclicDHButton(Button cyclicDHButton, SelectorPath path) {
        cyclicDHButton.gameObject.SetActive(true);
        cyclicDHButton.onClick.RemoveAllListeners();
        cyclicDHButton.onClick.AddListener(path.OpenUnlockScreenInstant);
        cyclicDHButton.onClick.AddListener(cyclicDHButton.GetComponent<TooltipActivator>().ShowImmediate);
        string startLevelName = UIHelper.SurroundWithColorWithoutB(path.startLevel.GetVisibleName(), UIHelper.GREEN);
        string endLevelName = UIHelper.SurroundWithColorWithoutB(path.endLevel.GetVisibleName(), UIHelper.GREEN);
        cyclicDHButton.GetComponent<TooltipActivator>().localizedMessage.Arguments = new object[] { startLevelName, endLevelName };
    }

    protected void SetTitles() {
        string startLevelName = selectorPath.startLevel.GetVisibleName();
        fromLevelTitle.text = startLevelName;
        UIHelper.FitTextHorizontally(startLevelName, fromLevelTitle);
        string endLevelName = selectorPath.endLevel.GetVisibleName();
        toLevelTitle.text = endLevelName;
        UIHelper.FitTextHorizontally(endLevelName, toLevelTitle);
    }

    public void Submit() {
        if (!selectorManager.HasSelectorPathUnlockScreenOpen())
            return;
        if (input.text == selectorPath.GetPassword() || input.text == MenuLevel.SUPER_CHEATED_PASSWORD) {
            if (selectorPath.IsUnlocked()) {
                SubmitGoodUnlocked();
            } else {
                SubmitGoodLocked();
            }
        } else {
            if (selectorPath.IsUnlocked()) {
                SubmitFalseUnlocked();
            } else {
                SubmitFalseLocked();
            }
        }
        RememberNumberOfSubmits();
    }

    protected void RememberNumberOfSubmits() {
        string key = selectorPath.GetNameId() + PrefsManager.NB_SUBMITS_PATH_KEY;
        int nbSubmits = PrefsManager.GetInt(key, 0);
        PrefsManager.SetInt(key, nbSubmits + 1);
    }

    protected void SubmitGoodUnlocked() {
        selectorManager.RunPopup(
            title: selectorManager.strings.pathGoodUnlockedTitle,
            text: selectorManager.strings.pathGoodUnlockedTexte,
            theme: TexteExplicatif.Theme.NEUTRAL);
    }

    protected void SubmitGoodLocked() {
        unlockAnimationCoroutine = StartCoroutine(CUnlockAnimation());
        StartCoroutine(CCancelUnlockAnimation(unlockAnimationCoroutine));
    }

    protected IEnumerator CCancelUnlockAnimation(Coroutine coroutine) {
        float dureeTotaleAnimation = dureeUnlockAnimation + dureeUnlockAnimationParticleSystem;
        Timer timer = new Timer(dureeTotaleAnimation);
        while(!timer.IsOver()) {
            yield return null;
            if(InputManager.Instance.GetAnyKeyOrButtonDown()) {
                StopCoroutine(coroutine);

                SetArrowAccordingToLockState();
                SetCadenasAccordingToLockState();
                selectorPath.cadena.DisplayGoodCadena();

                SetToTitleAccordingToLockState();

                GenerateNextAndPreviousButtons();

                MenuManager.DISABLE_HOTKEYS = false;
                GameManager.ShowCursor();
                RunPathUnlockedPopup();

                StopAllParticles();
                cadenaAnimationFluctuator.GoTo(0, 0.1f);
                UISoundManager.Instance.AccelerateUnlockPathClip(unlockAnimationCancelSoundAcceleration);
                break;
            }
        }
    }

    protected IEnumerator CUnlockAnimation() {
        selectorPath.UnlockPath();
        selectorManager.onUnlockPath.Invoke(selectorPath); // Not in UnlockPath because we don't want to trigger this with cheats !
        //SetBackgroundAccordingToLockState();
        selectorPath.endLevel.objectLevel.cube.SetMaterial(focus: false);
        selectorPath.startLevel?.InitializeObject();
        selectorPath.endLevel?.InitializeObject();
        UISoundManager.Instance.PlayUnlockPathClip();
        MenuManager.DISABLE_HOTKEYS = true;
        GameManager.HideCursor();

        SetFromTitleAccordingToLockState();
        StartUnlockPathParticles(fromLevelTitle.transform);
        cadenaAnimationFluctuator.GoTo(- 360 * nbTurnsUnlockAnimation, dureeUnlockAnimation, oneTimeCurve: curveUnlockAnimation);

        yield return new WaitForSeconds(dureeUnlockAnimation / 3);
        SetArrowAccordingToLockState();
        SetCadenasAccordingToLockState();
        selectorPath.cadena.DisplayGoodCadena();
        StartUnlockPathParticles(arrow.transform);

        yield return new WaitForSeconds(dureeUnlockAnimation / 3);
        SetToTitleAccordingToLockState();
        StartUnlockPathParticles(toLevelTitle.transform);

        yield return new WaitForSeconds(dureeUnlockAnimation / 3);
        StartUnlockPathParticles(fastUISystemNextTransform);
        GenerateNextAndPreviousButtons();

        yield return new WaitForSeconds(dureeUnlockAnimationParticleSystem);
        MenuManager.DISABLE_HOTKEYS = false;
        GameManager.ShowCursor();
        RunPathUnlockedPopup();
    }

    protected void RunPathUnlockedPopup() {
        string key = PrefsManager.HAS_DISPLAY_PATH_UNLOCK_POPUP_KEY;
        if(PrefsManager.GetBool(key, false)) {
            selectorManager.RunPopup(selectorManager.strings.pathGoodLockedTitle, selectorManager.strings.pathGoodLockedTexte, TexteExplicatif.Theme.POSITIF);
            PrefsManager.SetBool(key, true);
        }
    }

    protected void StartUnlockPathParticles(Transform parent, float delay = 0) {
        StartCoroutine(CStartUnlockPathParticles(parent, delay));
    }

    protected IEnumerator CStartUnlockPathParticles(Transform parent, float delay) {
        yield return new WaitForSeconds(delay);
        GameObject go = Instantiate(particlesUnlockAnimationPrefab, parent: parent);
        particlesToDestroy.Add(go);
        yield return new WaitForSeconds(dureeUnlockAnimationParticleSystem);
        if (go != null) {
            particlesToDestroy.Remove(go);
            Destroy(go);
        }
    }

    protected void StopAllParticles() {
        foreach(GameObject particle in particlesToDestroy) {
            foreach (Transform t in particle.transform) {
                t.gameObject.GetComponent<ParticleSystem>().Stop();
            }
        }
    }

    public void DestroyAllParticles() {
        foreach(GameObject particle in particlesToDestroy) {
            Destroy(particle);
        }
        particlesToDestroy.Clear();
    }

    protected float GetCadenasRotation() {
        return openCadena.GetComponent<RectTransform>().localRotation.z;
    }

    protected void SetCadenasRotation(float newRotation) {
        Quaternion newQuaternion = Quaternion.Euler(0, 0, newRotation);
        openCadena.GetComponent<RectTransform>().localRotation = newQuaternion;
        closedCadena.GetComponent<RectTransform>().localRotation = newQuaternion;
    }

    protected void SubmitFalseLocked() {
        string registeredPassword = input.text;
        string goodPassword = selectorPath.GetPassword();
        string advice = GetPasswordAdvice(registeredPassword, goodPassword);
        LocalizedString texte = null;
        if (advice != "") {
            advice = UIHelper.SurroundWithColor(advice, UIHelper.GREEN);
            texte = selectorManager.strings.pathFalseLockedTexte;
            texte.Arguments = new object[] { advice };
        } else {
            texte = selectorManager.strings.pathFalseLockedTexteWithoutAdvice;
        }
        string dataHackeesString = selectorManager.strings.dataHackees.GetLocalizedString().Result;
        selectorManager.popup.AddReplacement(dataHackeesString, UIHelper.SurroundWithColor(dataHackeesString, UIHelper.ORANGE));
        selectorManager.RunPopup(selectorManager.strings.pathFalseLockedTitle, texte, TexteExplicatif.Theme.NEGATIF, cleanReplacements: false);
    }

    protected string GetPasswordAdvice(string registeredPassword, string goodPassword) {
        string advice = Trace.GetPasswordAdvice(registeredPassword, goodPassword, selectorPath.adviceType, selectorPath.levenshteinDistance);
        return advice;
    }

    protected void SubmitFalseUnlocked() {
        string password = selectorPath.GetPassword();
        selectorManager.popup.Initialize(
            title: selectorManager.strings.pathFalseUnlockedTitre.GetLocalizedString().Result,
            mainText: selectorManager.strings.pathFalseUnlockedTexte.GetLocalizedString(password).Result,
            theme: TexteExplicatif.Theme.NEUTRAL);
        selectorManager.popup.AddReplacement(password, UIHelper.SurroundWithColor(password, UIHelper.GREEN));
        selectorManager.popup.Run();
    }

    public void SubmitIfEnter() {
        if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
            Submit();
    }

    public void OpenDonneesHackees() {
        if (!selectorManager.HasSelectorPathUnlockScreenOpen())
            return;
        int currentTreshold = GetCurrentTreshold();
        OpenDonneesHackeesWithCurrentTreshold(currentTreshold);
        selectorPath.HighlightPath(false);
        HighlightDataHackees(false);
        selectorManager.onOpenDHPath.Invoke(selectorPath);
    }

    protected int GetCurrentTreshold() {
        if (GetStartLevelType() == MenuLevel.LevelType.REGULAR)
            return selectorPath.startLevel.menuLevel.GetDataCount();
        else
            return (int)selectorPath.startLevel.menuLevel.GetBestScore();
    }

    public MenuLevel.LevelType GetStartLevelType() {
        return selectorPath.startLevel.menuLevel.GetLevelType();
    }

    protected void OpenDonneesHackeesWithCurrentTreshold(int currentTreshold) {
        selectorManager.popup.Initialize(
            title: selectorManager.strings.dataHackees.GetLocalizedString().Result,
            useTextAsset: true,
            textAsset: selectorPath.GetDataHackeesTextAsset(),
            theme: TexteExplicatif.Theme.NEUTRAL);
        AddReplacementForDonneesHackeesToPopup(selectorManager.popup);
        selectorManager.popup.InitTresholdText();
        ReplaceTresholdsWithGoalTresholds(selectorManager.popup, selectorPath.goalTresholds);
        AddNextPallierMessage(selectorManager.popup, currentTreshold: currentTreshold);
        selectorManager.popup.Run(
            textTreshold: selectorManager.popup.GetMaxTreshold(),
            shouldInitTresholdText: false,
            replacements: selectorManager.DHReplacementStrings,
            imagesAtlas: selectorPath.imagesAtlas);
    }

    protected void ReplaceTresholdsWithGoalTresholds(TexteExplicatif texteExplicatif, GoalTresholds goalTresholds) {
        if(texteExplicatif.GetTresholdText().GetAllFragments().Count != goalTresholds.tresholds.Count) {
            Debug.LogError($"Il n'y a pas le même nombre de tresholds dans le texte explicatif et dans le goalTreshold pour le path {selectorPath.GetNameId()} !");
        }
        List<TresholdFragment> fragments = texteExplicatif.GetTresholdText().GetAllFragmentsOrdered();
        for(int i = 0; i < goalTresholds.tresholds.Count; i++) {
            fragments[i].treshold = goalTresholds.tresholds[i];
        }
    }

    protected void AddReplacementForDonneesHackeesToPopup(TexteExplicatif popup) {
        popup.AddReplacement("%Trace%", UIHelper.SurroundWithColor(selectorPath.GetTrace(), UIHelper.PURE_GREEN));
        popup.AddReplacement("%Trace1%", UIHelper.SurroundWithColor(selectorPath.GetTrace().Substring(0, 1), UIHelper.PURE_GREEN));
        popup.AddReplacement("%Trace2%", UIHelper.SurroundWithColor(selectorPath.GetTrace().Substring(1, 1), UIHelper.PURE_GREEN));
        popup.AddReplacement("%Trace3%", UIHelper.SurroundWithColor(selectorPath.GetTrace().Substring(2, 1), UIHelper.PURE_GREEN));
        popup.AddReplacement("%Trace4%", UIHelper.SurroundWithColor(selectorPath.GetTrace().Substring(3, 1), UIHelper.PURE_GREEN));
        popup.AddReplacement("%Passe%", UIHelper.SurroundWithColor(selectorPath.GetPasse(), UIHelper.PURE_GREEN));
        //MatchEvaluator blueSurrounder = new MatchEvaluator(TexteExplicatif.SurroundWithBlueColor);
        //MatchEvaluator orangeSurrounder = new MatchEvaluator(TexteExplicatif.SurroundWithOrangeColor);
        //string passe = selectorManager.strings.passe.GetLocalizedString().Result;
        //string passes = selectorManager.strings.passes.GetLocalizedString().Result;
        //string trace = selectorManager.strings.trace.GetLocalizedString().Result;
        //string traces = selectorManager.strings.traces.GetLocalizedString().Result;
        //string dataHackees = selectorManager.strings.dataHackees.GetLocalizedString().Result;
        //popup.AddReplacementEvaluator($@"({passes}|{passe})", blueSurrounder);
        //popup.AddReplacementEvaluator($@"({traces}|{trace})", blueSurrounder);
        //popup.AddReplacementEvaluator($@"{dataHackees}", orangeSurrounder);
    }

    protected void AddNextPallierMessage(TexteExplicatif texteExplicatif, int currentTreshold) {
        TresholdText tresholdText = texteExplicatif.GetTresholdText();
        List<TresholdFragment> fragments = tresholdText.GetAllFragmentsOrdered();
        bool levelSucceeded = selectorPath.startLevel.IsSucceeded();
        for(int i = 0; i < fragments.Count; i++)
        {
            TresholdFragment fragment = fragments[i];
            //string nextPallierText = GetNextPallierText(fragment, fragments);
            string textOfTreshold = GetTresholdTextForPallier(fragment.treshold);
            string pallierNumberText = selectorManager.strings.palierTotalTexte.GetLocalizedString(i + 1, textOfTreshold, "").Result;
            pallierNumberText = UIHelper.SurroundWithColor(pallierNumberText, UIHelper.GREEN);

            if (levelSucceeded && fragment.treshold <= currentTreshold) {
                fragment.text = pallierNumberText + fragment.text;
            } else {
                string palierNotUnlockedYet = selectorManager.strings.palierNotUnlockedYet.GetLocalizedString().Result;
                fragment.text = pallierNumberText + palierNotUnlockedYet;
            }

            if (fragment == fragments.First()) {
                string currentTresholdText = GetCurrentTresholdText(currentTreshold);
                fragment.text = currentTresholdText + "\n\n" + fragment.text;
            }
        }
        int maxTreshold = texteExplicatif.GetMaxTreshold();
        texteExplicatif.mainText.text = texteExplicatif.ComputeText(maxTreshold);
        texteExplicatif.mainText.text = TexteExplicatif.ApplyColorReplacements(texteExplicatif.mainText.text);
    }

    protected string GetCurrentTresholdText(int currentTreshold) {
        string currentTresholdText;
        if (selectorPath.startLevel.menuLevel.GetLevelType() == MenuLevel.LevelType.REGULAR) {
            int nbVictoires = selectorPath.startLevel.menuLevel.GetNbWins();
            currentTresholdText = selectorManager.strings.palierCurrentTresholdRegular.GetLocalizedString(currentTreshold, nbVictoires).Result;
            string dataNumberToReplace = currentTreshold.ToString() + "<b></b>"; // To prevent to replace 0 in <#00FF00FF> XD
            string victoryNumberToReplace = nbVictoires.ToString() + "<a></a>"; // To prevent to replace 0 in <#00FF00FF> XD
            Tuple<string, string> victoryNumberReplacement = new Tuple<string, string>(victoryNumberToReplace, UIHelper.SurroundWithColor(nbVictoires.ToString(), UIHelper.CYAN));
            Tuple<string, string> dataNumberReplacement = new Tuple<string, string>(dataNumberToReplace, UIHelper.SurroundWithColor(currentTreshold.ToString(), UIHelper.CYAN));
            currentTresholdText = UIHelper.ApplyReplacement(currentTresholdText, victoryNumberReplacement);
            currentTresholdText = UIHelper.ApplyReplacement(currentTresholdText, dataNumberReplacement);
        } else {
            currentTresholdText = selectorManager.strings.palierCurrentTresholdInfinite.GetLocalizedString(currentTreshold).Result;
            Tuple<string, string> blockNumberReplacement = new Tuple<string, string>(currentTreshold.ToString(), UIHelper.SurroundWithColor(currentTreshold.ToString(), UIHelper.CYAN));
            currentTresholdText = UIHelper.ApplyReplacement(currentTresholdText, blockNumberReplacement);
        }
        currentTresholdText = UIHelper.SurroundWithColor(currentTresholdText, UIHelper.GREEN);
        return currentTresholdText;
    }

    protected string GetTresholdTextForPallier(int tresholdText) {
        return (selectorPath.startLevel.menuLevel.GetLevelType() == MenuLevel.LevelType.INFINITE) ?
            selectorManager.strings.blocs.GetLocalizedString(tresholdText).Result :
            (tresholdText == 0 ?
            selectorManager.strings.victoiresZero.GetLocalizedString().Result :
            selectorManager.strings.victoires.GetLocalizedString(tresholdText).Result);
    }

    protected string GetNextPallierText(TresholdFragment fragment, List<TresholdFragment> fragments) {
        //TresholdFragment currentFragment = fragments.FindAll(f => f.treshold <= textTreshold).Last();
        TresholdFragment currentFragment = fragments.Last();
        if (fragment == currentFragment) {
            if (fragment == fragments.Last()) {
                return selectorManager.strings.palierNextPalierDernier.GetLocalizedString().Result;
            } else {
                int nextTreshold = fragments[fragments.FindIndex(f => f == currentFragment) + 1].treshold;
                string unite = selectorManager.GetUnitesString(nextTreshold, GetStartLevelType());
                return selectorManager.strings.palierNextPalierProchain.GetLocalizedString(unite).Result;
            }
        } else {
            return "";
        }
    }

    public void Return() {
        if (!selectorManager.HasSelectorPathUnlockScreenOpen())
            return;
        selectorPath.CloseUnlockScreen();
    }

    protected void SetBackgroundAccordingToLockState() {
        if (selectorPath.IsUnlocked()) {
            selectorManager.background.SetParameters(probaBackgroundUnlocked,
                distanceBackgroundUnlocked,
                decroissanceBackgroundUnlocked,
                themesBackgroundUnlocked);
        } else {
            selectorManager.background.SetParameters(probaBackgroundLocked,
                distanceBackgroundLocked,
                decroissanceBackgroundUnlocked,
                themesBackgroundLocked);
        }
    }

    protected void SetCadenasAndArrowAndTitlesAccordingToLockState()
    { SetArrowAccordingToLockState();
        SetCadenasAccordingToLockState();
        SetFromTitleAccordingToLockState();
        SetToTitleAccordingToLockState();
    }

    private void SetToTitleAccordingToLockState() {
        if (selectorPath.IsUnlocked()) {
            toLevelTitle.transform.parent.GetComponent<Image>().material = materialTitleUnlocked;
            toLevelTitle.transform.parent.GetComponent<UpdateUnscaledTime>().Start();
        } else {
            toLevelTitle.transform.parent.GetComponent<Image>().material = materialTitleLocked;
            toLevelTitle.transform.parent.GetComponent<UpdateUnscaledTime>().Start();
        }
    }

    private void SetFromTitleAccordingToLockState() {
        if (selectorPath.IsUnlocked()) {
            fromLevelTitle.transform.parent.GetComponent<Image>().material = materialTitleUnlocked;
            fromLevelTitle.transform.parent.GetComponent<UpdateUnscaledTime>().Start();
        } else {
            fromLevelTitle.transform.parent.GetComponent<Image>().material = materialTitleLocked;
            fromLevelTitle.transform.parent.GetComponent<UpdateUnscaledTime>().Start();
        }
    }

    private void SetCadenasAccordingToLockState() {
        if (selectorPath.IsUnlocked()) {
            openCadena.SetActive(true);
            closedCadena.SetActive(false);
        } else {
            openCadena.SetActive(false);
            closedCadena.SetActive(true);
        }
    }

    private void SetArrowAccordingToLockState() {
        arrow.SetCorrectMaterial();
    }

    protected void FillInputWithPasswordIfAlreayDiscovered() {
        if(selectorPath.IsUnlocked()) {
            input.text = selectorPath.GetPassword();
        } else {
            input.text = "";
        }
    }

    protected void FillTraceHint() {
        int currentTreshold = GetCurrentTreshold();
        List<int> unlockedTresholds = selectorPath.GetTresholds().FindAll(i => i <= currentTreshold);
        MenuLevel menuLevel = selectorPath.startLevel.menuLevel;
        if(menuLevel.GetLevelType() == MenuLevel.LevelType.REGULAR && menuLevel.GetNbWins() == 0) {
            unlockedTresholds.Clear();
        }
        if(unlockedTresholds.Count >= selectorPath.nbTresholdsToSeeTraceHint) {
            traceHintContainer.SetActive(true);
            string traceString = UIHelper.SurroundWithB(selectorPath.GetTrace());
            traceHintText.text = selectorManager.strings.traceDisplayer.GetLocalizedString(traceString).Result;
        } else {
            traceHintContainer.SetActive(false);
        }
    }

    protected void HighlightDataHackees(bool state) {
        donneesHackeesButton.GetComponent<ButtonHighlighter>().enabled = state;
    }
}
