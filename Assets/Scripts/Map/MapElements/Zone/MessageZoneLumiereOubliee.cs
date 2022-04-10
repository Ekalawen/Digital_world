using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

public class MessageZoneLumiereOubliee : MessageZone {

    public int nbLumieresToHave;
    public GeoData geoDataToLumiere;
    public List<Lumiere> lumiereToThinkAbout;

    protected List<GeoPoint> geoPoints;
    protected List<MessageZoneLumiereOubliee> allMessageZoneLumiereOubliee;

    protected override void Start() {
        base.Start();
        geoPoints = new List<GeoPoint>();
        allMessageZoneLumiereOubliee = FindObjectsOfType<MessageZoneLumiereOubliee>().ToList();
    }

    public override void DisplayMessages() {
        if (GetNbLumieresOubliees() > 0) {
            base.DisplayMessages();
            AddGeoPointsToLumieres();
        }
    }

    protected override AsyncOperationHandle<string> GetHandle(LocalizedString localizedString, int indice) {
        int nbLumieresOubliees = GetNbLumieresOubliees();
        return localizedString.GetLocalizedString(new object[] { nbLumieresOubliees });
    }

    protected int GetNbLumieresCapturees() {
        return gm.map.nbLumieresInitial - gm.map.GetLumieres().Count;
    }

    protected int GetNbLumieresOubliees() {
        return nbLumieresToHave - GetNbLumieresCapturees();
    }

    protected void AddGeoPointsToLumieres() {
        foreach(Lumiere lumiere in lumiereToThinkAbout) {
            if(lumiere != null && !lumiere.IsCaptured() && !LumiereHasAlreadyAnyGeoPoint(lumiere)) {
                AddGeoPointsToLumiere(lumiere);
            }
        }
    }

    protected bool LumiereHasAlreadyAnyGeoPoint(Lumiere lumiere) {
        return allMessageZoneLumiereOubliee.Any(mz => mz.LumiereHasGeoPoint(lumiere));
    }

    protected bool LumiereHasGeoPoint(Lumiere lumiere) {
        return geoPoints.Select(g => g.data.targetObject).Contains(lumiere.transform);
    }

    protected void AddGeoPointsToLumiere(Lumiere lumiere) {
        GeoData newGeoData = new GeoData(geoDataToLumiere);
        newGeoData.targetObject = lumiere.transform;
        GeoPoint geoPoint = gm.player.geoSphere.AddGeoPoint(newGeoData);
        geoPoints.Add(geoPoint);
        lumiere.RegisterOnCapture(StopGeoPoint);
    }

    protected void StopGeoPoint(Lumiere lumiere) {
        for(int i = 0; i < geoPoints.Count; i++) {
            GeoPoint geoPoint = geoPoints[i];
            if(geoPoint != null && geoPoint.data.targetObject == lumiere.transform) {
                geoPoint.Stop();
                geoPoints[i] = null;
            }
        }
        geoPoints = geoPoints.FindAll(g => g != null);
    }
}
