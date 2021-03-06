﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SelectorManager)), CanEditMultipleObjects]
public class SelectorManagerEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        SelectorManager selectorManager = target as SelectorManager;
        if (GUILayout.Button("Reset all keys")) {
            PrefsManager.DeleteAll();
            Debug.Log("Toutes les clés ont été supprimées ! :)");
        }
        if(GUILayout.Button("Reset all levels scores")) {
            selectorManager.ResetAllLevelsScores();
        }
        if(GUILayout.Button("Set all scores to max tresholds")) {
            selectorManager.SetAllScoresToMaxTresholds();
        }
        if (GUILayout.Button("Unlock all paths")) {
            selectorManager.UnlockAllPaths();
        } if (GUILayout.Button("Lock all paths")) {
            selectorManager.LockAllPaths();
        }
    }
}
