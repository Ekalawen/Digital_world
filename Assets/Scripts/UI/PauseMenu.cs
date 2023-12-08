using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PauseMenu : MonoBehaviour {

    public MenuOptions menuOptions;
    public SkillTreeMenu skillTreeMenu;
    public TexteExplicatif popup;
    public ReplacementStrings docReplacementStrings;
    public SelectorManagerStrings strings;

    protected GameManager gm;
    protected InputManager inputManager;

    public void Initialize() {
        gm = GameManager.Instance;
        inputManager = InputManager.Instance;
        skillTreeMenu.Initilalize();
    }

    public void Update() {
        if(gm.IsPaused()) {
            if(inputManager.GetPauseReturnToMenu()) {
                Quitter();
            }
            if(inputManager.GetPauseGame()) {
                if (skillTreeMenu.IsOpen()) {
                    skillTreeMenu.Close();
                } else {
                    skillTreeMenu.CloseInstantly();
                    menuOptions.ResetMenu();
                    menuOptions.gameObject.SetActive(false);
                    Reprendre();
                }
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
        GameManager.Instance.console.SetHasAlreadyOpenedTheDoc(); // Car appelé à la première frame, et donc peut-être pas initialisé :/
        AsyncOperationHandle<string> handleTitle = strings.docTitle.GetLocalizedString();
        yield return handleTitle;
        string titleString = handleTitle.Result;
        string docText = "";
        for(int i = 0; i < gm.console.conseils.Count; i++) {
            yield return gm.console.CComputeConseil(i);
            docText += $"{UIHelper.SurroundWithColor((i + 1).ToString(), UIHelper.GREEN)}) {gm.console.computedConseil}\n";
        }
        docText = UIHelper.ApplyReplacementList(docText, docReplacementStrings);
        RunPopup(titleString, docText, TexteExplicatif.Theme.NEUTRAL);
    }

    public void RunPopup(string title, string text, TexteExplicatif.Theme theme, bool cleanReplacements = true) {
        popup.Initialize(title: title, mainText: text, theme: theme, cleanReplacements: cleanReplacements);
        popup.Run();
    }

    public void OpenOptions() {
        menuOptions.gameObject.SetActive(true);
        menuOptions.Run();
        Tooltip.HideAll();
    }

    public void CloseOptions() {
        menuOptions.gameObject.SetActive(false);
        Tooltip.HideAll();
    }

    public bool IsOptionsOpen() {
        return menuOptions.gameObject.activeInHierarchy;
    }

    public void OpenSkillTree() {
        skillTreeMenu.Open();
        Tooltip.HideAll();
    }

    public bool IsSkillTreeOpen() {
        return skillTreeMenu.IsOpen();
    }

    public void CloseSkillTree() {
        skillTreeMenu.Close();
        Tooltip.HideAll();
    }

    public void CloseSkillTreeInstantly() {
        skillTreeMenu.CloseInstantly();
        Tooltip.HideAll();
    }

    public void Quitter() {
        gm.QuitOrReloadGame();
    }
}
