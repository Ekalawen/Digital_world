using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreeManager : MonoBehaviour {

    public Transform upgradesFolder;
    public Transform linksFolder;
    public GameObject linkPrefab;

    protected List<SkillTreeUpgrade> upgrades;
    protected List<SkillTreeLink> links;

    public void Initilalize() {
        links = new List<SkillTreeLink>();
        GatherUpgrades();
        InitializeUpgrades();
    }

    protected void InitializeUpgrades() {
        upgrades.ForEach(u => u.Initialize(this));
    }

    protected void GatherUpgrades() {
        upgrades = new List<SkillTreeUpgrade>();
        GatherUpgradesIn(upgradesFolder);
    }

    protected void GatherUpgradesIn(Transform folder) {
        foreach (Transform child in folder) {
            SkillTreeUpgrade upgrade = child.GetComponent<SkillTreeUpgrade>();
            if(upgrade) {
                upgrades.Add(upgrade);
            } else {
                GatherUpgradesIn(child);
            }
        }
    }

    public void CreateLink(SkillTreeUpgrade source, SkillTreeUpgrade target) {
        SkillTreeLink link = Instantiate(linkPrefab, linksFolder).GetComponent<SkillTreeLink>();
        link.Initialize(this, source, target);
        links.Add(link);
    }
}
