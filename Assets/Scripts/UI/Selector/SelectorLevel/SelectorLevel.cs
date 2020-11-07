using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class SelectorLevel : MonoBehaviour {

    public MenuLevel menuLevel;
    public SelectorLevelObject objectLevel;

    protected SelectorManager selectorManager;

    public void Initialize(MenuBackgroundBouncing background) {
        selectorManager = SelectorManager.Instance;
        objectLevel.title.GetComponent<LookAtTransform>().transformToLookAt = selectorManager.camera.transform;
        objectLevel.Initialize();
        menuLevel.selectorManager = selectorManager;
        menuLevel.menuBouncingBackground = background;
        RewardIfNewBestScore();
    }

    public void OnMouseEnter() {
    }

    public void OnMouseExit() {
    }

    public string GetName() {
        return menuLevel.textLevelName.text;
    }

    public void OnMouseDown() {
        if (!selectorManager.HasSelectorLevelOpen())
            selectorManager.TryDisplayLevel(this);
    }

    public bool IsSucceeded() {
        return menuLevel.IsSucceeded();
    }

    public bool IsAccessible() {
        return selectorManager.IsLevelAccessible(this);
    }

    protected void RewardIfNewBestScore() {
        string keyHasBestScore = GetName() + MenuLevel.HAS_JUST_MAKE_BEST_SCORE_KEY;
        if (PlayerPrefs.HasKey(keyHasBestScore)) {
            RewardNewBestScore();
            PlayerPrefs.DeleteKey(keyHasBestScore);
        }
    }

    protected void RewardNewBestScore() {
        string bestScoreString = menuLevel.GetBestScoreToString();
        selectorManager.popup.CleanReplacements();
        selectorManager.popup.AddReplacement(bestScoreString, $"<color=green>{bestScoreString}</color>");
        selectorManager.RunPopup("MEILLEUR SCORE !!!",
            "Incroyable !\n" +
            "Vous avez battu votre record et fais le Meilleur Score !\n" +
            $"Vous avez fait {bestScoreString} !",
            TexteExplicatif.Theme.POSITIF,
            cleanReplacements: false);
    }

    public void ResetScores() {
        List<string> keysSuffix = new List<string>() {
            MenuLevel.NB_WINS_KEY,
            MenuLevel.NB_DEATHS_KEY,
            MenuLevel.SUM_OF_ALL_TRIES_SCORES_KEY,
            MenuLevel.HIGHEST_SCORE_KEY,
            MenuLevel.SINCE_LAST_BEST_SCORE_KEY,
            MenuLevel.HAS_JUST_WIN_KEY,
            MenuLevel.HAS_JUST_MAKE_BEST_SCORE_KEY,
        };
        foreach(string keySuffix in keysSuffix) {
            string key = GetName() + keySuffix;
            PlayerPrefs.DeleteKey(key);
        }
        Debug.Log($"Scores of level {GetName()} reset !");
    }

    public void SetScoresToMaxTreshold() {
        int maxTreshold = GetMaxTreshold();
        if(menuLevel.levelType == MenuLevel.LevelType.REGULAR) {
            menuLevel.SetNbWins(maxTreshold);
            Debug.Log($"{GetName()} a maintenant {maxTreshold} victoires !");
        }
        if(menuLevel.levelType == MenuLevel.LevelType.INFINITE) {
            menuLevel.SetBestScore(maxTreshold);
            Debug.Log($"{GetName()} a maintenant un Meilleur Score de {maxTreshold} !");
        }
    }

    protected int GetMaxTreshold() {
        List<SelectorPath> paths = selectorManager.GetPaths().FindAll(p => p.startLevel == this);
        List<int> maxTresholds = paths.Select(p => p.GetMaxTreshold()).ToList();
        return maxTresholds.Max();
    }
}

[CustomEditor(typeof(SelectorLevel)), CanEditMultipleObjects]
public class SelectorLevelEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        SelectorLevel selectorLevel = target as SelectorLevel;
        if(GUILayout.Button("Reset Scores")) {
            selectorLevel.ResetScores();
        }
        if(GUILayout.Button("Scores to Max Treshold")) {
            selectorLevel.SetScoresToMaxTreshold();
        }
    }
}

//[CustomEditor(typeof(GenerateCaves)), CanEditMultipleObjects]
//public class GenerateCavesEditor : Editor {
//    public override void OnInspectorGUI() {
//        GenerateCaves generateCaves = target as GenerateCaves;
//        serializedObject.Update();
//        SerializedProperty property = serializedObject.GetIterator();
//        while(property.NextVisible(true)) {
//            switch(property.name) {
//                case "nbCaves":
//                    if(generateCaves.useNbCaves) EditorGUILayout.PropertyField(property);
//                    break;
//                case "proportionCaves":
//                    if(!generateCaves.useNbCaves) EditorGUILayout.PropertyField(property);
//                    break;
//                default:
//                    EditorGUILayout.PropertyField(property);
//                    break;
//            }
//        }
//        serializedObject.ApplyModifiedProperties();
//    }
//}
