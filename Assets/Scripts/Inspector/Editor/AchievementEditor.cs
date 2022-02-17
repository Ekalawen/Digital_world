using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Achievement), true), CanEditMultipleObjects]
public class AchievementEditor : Editor
{
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        Achievement achievement = target as Achievement;
        GUILayout.Space(15);
        EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
        GUILayout.Space(5);
        if (GUILayout.Button("Unlock!")) {
            achievement.Unlock();
        }
        if (GUILayout.Button("Lock!")) {
            achievement.Lock();
        }
    }
}

