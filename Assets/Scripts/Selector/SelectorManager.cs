using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectorManager : MonoBehaviour {

    static SelectorManager _instance;
    public static SelectorManager Instance { get { return _instance ?? (_instance = new GameObject().AddComponent<SelectorManager>()); } }

    public Transform levelsFolder;
    public Transform pathsFolder;
    public Camera camera;
    public MenuBackgroundBouncing background;

    protected List<SelectorLevel> levels;
    protected List<SelectorPath> paths;

    void Awake() {
        if (!_instance) { _instance = this; }
    }

    void Start() {
        GatherLevels();
        GatherPaths();
        background.gameObject.SetActive(false);
        //levels[0].menuLevel.gameObject.SetActive(true);
    }

    public List<SelectorLevel> GetLevels() {
        return levels;
    }

    protected void GatherPaths() {
        paths = new List<SelectorPath>();
        foreach(Transform child in pathsFolder) {
            SelectorPath path = child.gameObject.GetComponent<SelectorPath>();
            if (path != null)
                paths.Add(path);
        }
    }

    protected void GatherLevels() {
        levels = new List<SelectorLevel>();
        foreach(Transform child in levelsFolder) {
            SelectorLevel level = child.gameObject.GetComponent<SelectorLevel>();
            if (level != null)
                levels.Add(level);
        }
    }
}
