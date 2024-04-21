using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Localization.Settings;
using TMPro;
using UnityEngine.SceneManagement;


public class EndLevelScreen : MonoBehaviour {

    public GameObject holder;
    public GameObject unlockGroupPrefab;
    public GameObject unlockGroupsHolder;

    public GameObject deathAstuce;
    public GameObject restartButton; // Le truc qui clignote pour nous dire d'appuyer sur Escape à la fin du jeu !
    public TMP_Text restartButtonText;

    public GameObject skillTreeNormalButton;
    public GameObject skillTreeWithNewSkillButton;

    protected GameManager gm;
    protected Console console;
    protected List<EndLevelUnlockGroup> unlockGroups;

    public void Initialize() {
        gm = GameManager.Instance;
        console = gm.console;
        holder.SetActive(false);
        InitializeUnlockGroups();
        gm.console.GetPauseMenu().skillTreeMenu.onClose.AddListener(DisplaySkillTreeButton);
    }

    protected void InitializeUnlockGroups() {
        unlockGroups = new List<EndLevelUnlockGroup>();
        foreach (GoalLevel goalLevel in gm.goalManager.goalLevels) {
            CreateUnlockGroup(goalLevel);
        }
    }

    protected void CreateUnlockGroup(GoalLevel goalLevel) {
        if (goalLevel.IsSet() && goalLevel.GetPath().IsUnlocked()) {
            return;
        }
        EndLevelUnlockGroup unlockGroup = Instantiate(unlockGroupPrefab, parent: unlockGroupsHolder.transform).GetComponent<EndLevelUnlockGroup>();
        unlockGroups.Add(unlockGroup);
        unlockGroup.Initialize(goalLevel);
    }

    public void Open() {
        MouseDisplayer.Instance.ShowCursor();
        holder.SetActive(true);
        DisplaySkillTreeButton();
        unlockGroups.ForEach(g => g.Display());
        DisplayRestartButton();
        //DisplayDeathAstuces(); // Desactivated
    }

    protected void DisplaySkillTreeButton() {
        bool hasNewlyAffordableUpgrade = ShouldHightlightSkillTreeButton();
        skillTreeNormalButton.SetActive(!hasNewlyAffordableUpgrade);
        skillTreeWithNewSkillButton.SetActive(hasNewlyAffordableUpgrade);
    }

    protected bool ShouldHightlightSkillTreeButton() {
        return gm.console.GetPauseMenu().skillTreeMenu.HasNewlyAffordableUpgrades(additionnalCredits: 0);
    }

    public void Close() {
        holder.SetActive(false);
    }

    public void SetOpen(bool isOpen) {
        if (isOpen)
            Open();
        else
            Close();
    }

    public void DisplayRestartButton() {
        restartButton.SetActive(console.IsVisible());
        string binding = InputManager.Instance.GetCurrentInputController().GetStringForBinding(MessageZoneBindingParameters.Bindings.RESTART);
        restartButtonText.text = console.strings.restartButtonRestart.GetLocalizedString(binding).Result;
        bool shouldBeHightlighted = !unlockGroups.Any(g => g.progressBar.IsFull()) && !ShouldHightlightSkillTreeButton();
        restartButton.GetComponent<ButtonHighlighter>().enabled = shouldBeHightlighted; 
    }

    public void DisplayDeathAstuces() {
        if (gm.eventManager.ShouldQuitOrReload() == EventManager.QuitType.RELOAD) {
            StartCoroutine(CDisplayDeathAstuces());
        }
    }

    public IEnumerator CDisplayDeathAstuces() {
        deathAstuce.SetActive(true);
        string conseilKey = StringHelper.GetKeyFor(PrefsManager.CONSEIL_INDICE);
        int conseilIndice = PrefsManager.GetInt(conseilKey, 0);
        PrefsManager.SetInt(conseilKey, (conseilIndice + 1) % console.conseils.Count);
        yield return console.CComputeConseil(conseilIndice);
        string conseil = console.computedConseil;
        TMP_Text text = deathAstuce.GetComponentInChildren<TMP_Text>();
        text.text = text.text.Substring(0, text.text.Count() - 1) + " "; // Delete ending '\n'
        text.text += conseil;
    }

}
