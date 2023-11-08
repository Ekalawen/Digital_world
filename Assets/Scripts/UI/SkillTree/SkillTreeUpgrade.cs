using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreeUpgrade : MonoBehaviour {

    protected SkillTreeManager skillTreeManager;
    public void Initialize(SkillTreeManager skillTreeManager) {
        this.skillTreeManager = skillTreeManager;
    }
}
