using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreeBackButtonInGame : MonoBehaviour {

    public Button normalBackButton;
    public Button backWithoutSaveButton;
    public Button saveAndRestartBackButton;

    protected SkillTreeMenu skillTreeMenu;

    public void Initialize(SkillTreeMenu skillTreeMenu) {
        this.skillTreeMenu = skillTreeMenu;
        SetActiveButtons(skillTreeHasChanged: false);
        skillTreeMenu.onUpgradeChange.AddListener(() => SetActiveButtons(skillTreeHasChanged: true));
    }

    protected void SetActiveButtons(bool skillTreeHasChanged) {
        normalBackButton.gameObject.SetActive(!skillTreeHasChanged);
        backWithoutSaveButton.gameObject.SetActive(skillTreeHasChanged);
        saveAndRestartBackButton.gameObject.SetActive(skillTreeHasChanged);
    }
}
