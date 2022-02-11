using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleEventAddGenerator : SingleEvent {

    public GameObject generatorPrefab;
    public int nbGeneratorToAdd = 1;

    [Header("Start Position")]
    public bool useRandomStartPosition = true;
    [ConditionalHide("useRandomStartPosition", false)]
    public Vector3 customStartPosition;

    [Header("Visibility")]
    public GeoData geoDataToGenerator;

    public override void TriggerSpecific() {
        for(int i = 0; i < nbGeneratorToAdd; i++) {
            AddGenerator();
        }
    }

    public void AddGenerator()
    {
        Vector3 pos = GetGeneratorPos();
        IGenerator generator = Instantiate(generatorPrefab, pos, Quaternion.identity, parent: gm.map.zonesFolder).GetComponent<IGenerator>();
        generator.Initialize();
        AddGeoPointToGenerator(generator.transform.position);
    }

    protected Vector3 GetGeneratorPos() {
        return useRandomStartPosition ? gm.map.GetFreeRoundedLocation() : customStartPosition;
    }

    protected void AddGeoPointToGenerator(Vector3 position) {
        GeoData newGeoData = new GeoData(geoDataToGenerator);
        newGeoData.SetTargetPosition(position);
        gm.player.geoSphere.AddGeoPoint(newGeoData);
    }
}
