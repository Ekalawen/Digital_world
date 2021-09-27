using System;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "RewardStrings", menuName = "RewardStrings")]
public class RewardStrings : ScriptableObject {

    public LocalizedString levelCompleted;

    public LocalizedString score;
    public LocalizedString bestScore;
    public LocalizedString previousBestScore;

    public LocalizedString gameDuration;
    public LocalizedString replayDuration;
    public LocalizedString acceleration;
    public LocalizedString nbDifferentBlocks;
    public LocalizedString nbSameBlockMax;
}
