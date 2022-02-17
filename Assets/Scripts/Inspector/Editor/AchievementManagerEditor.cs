using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AchievementManager), true), CanEditMultipleObjects]
public class AchievementManagerEditor : Editor
{
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        AchievementManager AchievementManager = target as AchievementManager;
        GUILayout.Space(15);
        EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
        GUILayout.Space(5);
        EditorGUILayout.LabelField("ALL", EditorStyles.boldLabel);
        if (GUILayout.Button("Unlock ALL Achievements!")) {
            AchievementManager.UnlockAll();
        }
        if (GUILayout.Button("Lock ALL Achievements!")) {
            AchievementManager.LockAll();
        }
        EditorGUILayout.LabelField("Relevant", EditorStyles.boldLabel);
        if (GUILayout.Button("Unlock all relevant Achievements!")) {
            AchievementManager.UnlockAllRelevant();
        }
        if (GUILayout.Button("Lock all relevant Achievements!")) {
            AchievementManager.LockAllRelevant();
        }
    }
}

