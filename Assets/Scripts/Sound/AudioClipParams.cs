using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AudioClipParams {

    public static float BASE_VOLUME = 0.2f;

    [Range(0, 5)]
    public float relativeVolume = 1.0f; // Compris entre 0 et 2.0
    [Range(0, 1)]
    public float spatialBlend = 1.0f; // 1.0 = spatialisé, 0.0 = global
    public bool bReverse = false;
    public bool bIsMusic = false;
    public bool bLoop = false;
    public List<AudioClip> clips;
}
