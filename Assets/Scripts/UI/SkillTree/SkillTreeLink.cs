using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreeLink : MonoBehaviour {

    //public UILineRenderer lineRenderer;

    protected SkillTreeManager skillTreeManager;
    protected SkillTreeUpgrade sourceUpgrade;
    protected SkillTreeUpgrade targetUpgrade;

    public void Initialize(SkillTreeManager skillTreeManager, SkillTreeUpgrade sourceUpgrade, SkillTreeUpgrade targetUpgrade) {
        this.skillTreeManager = skillTreeManager;
        this.sourceUpgrade = sourceUpgrade;
        this.targetUpgrade = targetUpgrade;
    }
}
