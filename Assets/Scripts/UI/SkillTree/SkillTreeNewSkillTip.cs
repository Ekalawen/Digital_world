using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreeNewSkillTip : MonoBehaviour {

    public GameObject newSkillTip;
    //public CounterDisplayer scoreCounter;

    protected GameManager gm;
    protected SkillTreeMenu skillTreeMenu;

    public void Initialize() {
        gm = GameManager.Instance;
        skillTreeMenu = gm.console.GetPauseMenu().skillTreeMenu;
        SetActiveNewSkillTip();
    }

    public void SetActiveNewSkillTip() {
        newSkillTip.SetActive(skillTreeMenu.HasNewlyAffordableUpgrades());
    }
}
