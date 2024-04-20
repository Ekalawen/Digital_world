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

    protected GameManager gm;
    protected Console console;
    protected List<EndLevelUnlockGroup> unlockGroups;

    public void Initialize() {
        gm = GameManager.Instance;
        console = gm.console;
        holder.SetActive(false);
        InitializeUnlockGroups();
    }

    protected void InitializeUnlockGroups() {
        unlockGroups = new List<EndLevelUnlockGroup>();
        CreateUnlockGroup();
    }

    protected void CreateUnlockGroup() {
        EndLevelUnlockGroup unlockGroup = Instantiate(unlockGroupPrefab, parent: unlockGroupsHolder.transform).GetComponent<EndLevelUnlockGroup>();
        unlockGroups.Add(unlockGroup);
        unlockGroup.Initialize();
    }

    public void Open() {
        holder.SetActive(true);
        DisplayEscapeButton();
        unlockGroups.ForEach(g => g.StartProgressBar());
        //DisplayDeathAstuces(); // Desactivated
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

    public void DisplayEscapeButton() {
        restartButton.SetActive(console.IsVisible());
        if (gm.eventManager.ShouldQuitOrReload() == EventManager.QuitType.RELOAD) {
            string binding = InputManager.Instance.GetCurrentInputController().GetStringForBinding(MessageZoneBindingParameters.Bindings.RESTART);
            restartButtonText.text = console.strings.restartButtonRestart.GetLocalizedString(binding).Result;
        } else {
            string binding = InputManager.Instance.GetCurrentInputController().GetStringForBinding(MessageZoneBindingParameters.Bindings.PAUSE);
            restartButtonText.text = console.strings.restartButtonContinue.GetLocalizedString(binding).Result;
        }
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
