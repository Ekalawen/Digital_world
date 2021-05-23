using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Localization;

[Serializable]
public struct ReplacementString {
    public LocalizedString localizedString;
    public UIHelper.UIColor color;
}

[CreateAssetMenu(fileName = "ArchivesReplacementStrings", menuName = "ArchivesReplacementStrings")]
public class ReplacementStrings : ScriptableObject {
    public List<ReplacementString> replacements;
}
