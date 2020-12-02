using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SelectorPath)), CanEditMultipleObjects]
public class SelectorPathEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        SelectorPath selectorPath = target as SelectorPath;
        if (GUILayout.Button("Unlock Path")) {
            selectorPath.UnlockPath();
        } if (GUILayout.Button("Lock Path")) {
            selectorPath.LockPath();
        }
    }
}
