using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreeBackButtonInGame : MonoBehaviour {

    public GameObject normalBackButton;
    public GameObject backWithoutSaveButton;
    public GameObject saveAndRestartBackButton;

    protected SkillTreeMenu skillTreeMenu;

    public void Initialize(SkillTreeMenu skillTreeMenu) {
        this.skillTreeMenu = skillTreeMenu;
        SetActiveButtons(skillTreeHasChanged: false);
        skillTreeMenu.onUpgradeChange.AddListener(() => SetActiveButtons(skillTreeHasChanged: true));
    }

    protected void SetActiveButtons(bool skillTreeHasChanged) {
        normalBackButton.SetActive(!skillTreeHasChanged);
        backWithoutSaveButton.SetActive(skillTreeHasChanged);
        saveAndRestartBackButton.SetActive(skillTreeHasChanged);
    }
}
