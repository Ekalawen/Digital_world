using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class SingleEventAddEnnemi : SingleEvent {

    public enum StartPositionType { RANDOM, CUSTOM, IN_LINE_OF_SIGHT };

    [Header("Generation")]
    public int ennemiIndice = 0;
    public int nbEnnemisToAdd = 1;
    public GeoData geoDataGenerateEnnemi;

    [Header("Start Position")]
    public StartPositionType startPositionType = StartPositionType.RANDOM;
    [ConditionalHide("startPositionType", StartPositionType.CUSTOM)]
    public Vector3 customStartPosition;
    [ConditionalHide("startPositionType", StartPositionType.IN_LINE_OF_SIGHT)]
    public Vector2 lineOfSightRange = new Vector2(3.0f, 5.0f);

    [Header("Waiting Time")]
    public bool shouldUseCustomWaitingTime = false;
    [ConditionalHide("shouldUseCustomWaitingTime")]
    public float customWaitingTime = 1.0f;

    protected GameObject ennemiPrefab;
    protected List<Ennemi> ennemisGenerated = new List<Ennemi>();

    public override void Initialize() {
        base.Initialize();
        ennemiPrefab = gm.ennemiManager.ennemisPrefabs[ennemiIndice];
    }

    public override void TriggerSpecific() {
        for(int i = 0; i < nbEnnemisToAdd; i++)
        {
            Ennemi ennemi = null;
            float waitingTime = shouldUseCustomWaitingTime ? customWaitingTime : -1;
            if (startPositionType == StartPositionType.RANDOM)
            {
                ennemi = gm.ennemiManager.PopEnnemi(ennemiPrefab, waitingTime);
            }
            else if (startPositionType == StartPositionType.CUSTOM)
            {
                ennemi = gm.ennemiManager.GenerateEnnemiFromPrefab(ennemiPrefab, customStartPosition, waitingTime);
            }
            else if (startPositionType == StartPositionType.IN_LINE_OF_SIGHT)
            {
                Vector3 pos = gm.map.GetEmptyPositionInPlayerSight(lineOfSightRange);
                ennemi = gm.ennemiManager.GenerateEnnemiFromPrefab(ennemiPrefab, pos, waitingTime);
            }
            AddGeoPoint(ennemi);
            ennemisGenerated.Add(ennemi);
        }
    }

    protected void AddGeoPoint(Ennemi ennemi) {
        GeoData newGeoData = new GeoData(geoDataGenerateEnnemi);
        newGeoData.targetObject = ennemi.transform;
        gm.player.geoSphere.AddGeoPoint(newGeoData);
    }
}
