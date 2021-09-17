using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PauseMenu : MonoBehaviour {

    public MenuOptions menuOptions;
    public TexteExplicatif popup;
    public ReplacementStrings docReplacementStrings;
    public SelectorManagerStrings strings;

    protected GameManager gm;
    protected InputManager inputManager;

    public void Start() {
        gm = GameManager.Instance;
        inputManager = InputManager.Instance;
    }

    public void Update() {
        if(gm.IsPaused()) {
            if(inputManager.GetPauseReturnToMenu()) {
                Quitter();
            }
            if(inputManager.GetPauseGame()) {
                menuOptions.ResetMenu();
                menuOptions.gameObject.SetActive(false);
                Reprendre();
            }
            // O and R are already handled
        }
    }

    public void Reprendre() {
        gm.UnPause();
    }

    public void Recommencer() {
        gm.RestartGame();
    }

    public void OpenDoc() {
        StartCoroutine(COpenDoc());
    }

    protected IEnumerator COpenDoc() {
        AsyncOperationHandle<string> handleTitle = strings.docTitle.GetLocalizedString();
        yield return handleTitle;
        string titleString = handleTitle.Result;
        string docText = "";
        int i = 1;
        foreach(LocalizedString localizedString in gm.console.conseils) {
            AsyncOperationHandle<string> conseilHandle = localizedString.GetLocalizedString();
            yield return conseilHandle;
            docText += $"{UIHelper.SurroundWithColor(i.ToString(), UIHelper.GREEN)}) {conseilHandle.Result}\n";
            i++;
        }
        docText = UIHelper.ApplyReplacementList(docText, docReplacementStrings);
        RunPopup(titleString, docText, TexteExplicatif.Theme.NEUTRAL);
    }

    public void RunPopup(string title, string text, TexteExplicatif.Theme theme, bool cleanReplacements = true) {
        popup.Initialize(title: title, mainText: text, theme: theme, cleanReplacements: cleanReplacements);
        popup.Run();
    }

    public void Options() {
        menuOptions.gameObject.SetActive(true);
        menuOptions.Run();
        Tooltip.Hide();
    }

    public void Quitter() {
        gm.QuitOrReloadGame();
    }
}
