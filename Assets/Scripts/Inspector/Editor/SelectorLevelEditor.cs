using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SelectorLevel)), CanEditMultipleObjects]
public class SelectorLevelEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        SelectorLevel selectorLevel = target as SelectorLevel;
        if (GUILayout.Button("Reset Scores")) {
            selectorLevel.ResetScores();
        }
        if (GUILayout.Button("Scores to Max Treshold")) {
            selectorLevel.SetScoresToMaxTreshold();
        }
    }
}

