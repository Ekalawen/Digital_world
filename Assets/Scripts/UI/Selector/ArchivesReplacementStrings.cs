using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "ArchivesReplacementStrings", menuName = "ArchivesReplacementStrings")]
public class ArchivesReplacementStrings : ScriptableObject {
    public List<LocalizedString> blueReplacements;
    public List<LocalizedString> cyanReplacements;
    public List<LocalizedString> pureGreenReplacements;
}
