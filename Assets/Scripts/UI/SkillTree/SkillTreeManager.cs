using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreeManager : MonoBehaviour {

    public Transform upgradesFolder;
    public Transform linksFolder;

    protected List<SkillTreeUpgrade> upgrades;
    protected List<SkillTreeLink> links;

    public void Initilalize() {
        GatherUpgrades();
        GatherLinks();
        InitializeUpgrades();
        InitializeLinks();
    }

    protected void InitializeLinks() {
        links.ForEach(l => l.Initialize(this));
    }

    protected void InitializeUpgrades() {
        upgrades.ForEach(u => u.Initialize(this));
    }

    protected void GatherLinks() {
        links = new List<SkillTreeLink>();
        GatherLinksIn(linksFolder);
    }

    protected void GatherLinksIn(Transform folder) {
        foreach (Transform child in folder) {
            SkillTreeLink link = child.GetComponent<SkillTreeLink>();
            if(link) {
                links.Add(link);
            } else {
                GatherLinksIn(child);
            }
        }
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
}
