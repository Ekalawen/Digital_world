using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LumiereFurtiveTrapApplier : MonoBehaviour {

    protected GameManager gm;
    protected GameObject ennemiToGeneratePrefab;
    protected GeoData geoDataToEnnemi;
    protected float timeBeforeEnnemisActivation = 3.0f;
    protected GameObject lumiereGeneratedPrefab;
    protected int nbLumieresToGenerate = 1;
    protected GeoData geoDataToLumiere;
    protected GameObject pouvoirDashPrefab;
    protected PouvoirGiverItem.PouvoirBinding pouvoirBinding;

    public void ApplyTrap(
        GameObject ennemiToGeneratePrefab,
        GeoData geoDataToEnnemi,
        float timeBeforeEnnemisActivation,
        GameObject lumiereGeneratedPrefab,
        int nbLumieresToGenerate,
        GeoData geoDataToLumiere,
        GameObject pouvoirDashPrefab,
        PouvoirGiverItem.PouvoirBinding pouvoirBinding)
    {
        this.ennemiToGeneratePrefab = ennemiToGeneratePrefab;
        this.geoDataToEnnemi = geoDataToEnnemi;
        this.timeBeforeEnnemisActivation = timeBeforeEnnemisActivation;
        this.lumiereGeneratedPrefab = lumiereGeneratedPrefab;
        this.nbLumieresToGenerate = nbLumieresToGenerate;
        this.geoDataToLumiere = geoDataToLumiere;
        this.pouvoirDashPrefab = pouvoirDashPrefab;
        this.pouvoirBinding = pouvoirBinding;
        gm = GameManager.Instance;
        StartCoroutine(CApplyTrap());
        FindObjectOfType<HelperAnalyze>().SetCaptureFirstData();
    }

    protected IEnumerator CApplyTrap()
    {
        float firstWaitingTime = 2.0f;
        gm.timerManager.isInfinitTime = false;
        gm.timerManager.SetTime(gm.timerManager.initialTime, showVolatileText: false);
        GiveBackDash();
        gm.console.AjouterMessageImportant(gm.console.strings.analyzeTrapWait, Console.TypeText.BLUE_TEXT, firstWaitingTime);
        yield return new WaitForSeconds(firstWaitingTime);
        yield return null;
        float secondWaitingTime = 2.0f;
        gm.console.AjouterMessageImportant(gm.console.strings.analyzeTrapSortie, Console.TypeText.BLUE_TEXT, secondWaitingTime);
        yield return new WaitForSeconds(secondWaitingTime);
        yield return null;
        PopEnnemis();
        PopLumieres();
        gm.console.AnalyzeLevelDeuxiemeSalve();
        gm.soundManager.PlayReceivedMessageClip();
        Destroy(this.gameObject);
    }

    protected void GiveBackDash() {
        PouvoirGiverItem.PouvoirBinding pouvoirBinding = PouvoirGiverItem.PouvoirBinding.LEFT_CLICK;
        gm.player.SetPouvoir(pouvoirDashPrefab, pouvoirBinding);
        IPouvoir pouvoir = pouvoirDashPrefab.GetComponent<IPouvoir>();
        gm.console.CapturePouvoirGiverItem(pouvoir.nom, pouvoirBinding);
    }

    protected void PopEnnemis() {
        List<Vector3> allCorners = gm.map.GetAllInsidedCorners();
        allCorners.Add(gm.map.GetCenter());
        foreach (Vector3 corner in allCorners) {
            Ennemi ennemi = gm.ennemiManager.GenerateEnnemiFromPrefab(ennemiToGeneratePrefab, corner, timeBeforeEnnemisActivation);
            AddGeoPoint(geoDataToEnnemi, ennemi.transform);
        }
    }

    protected void PopLumieres() {
        for (int i = 0; i < nbLumieresToGenerate; i++) {
            Vector3 posLumiere = gm.map.GetFarRoundedLocation(gm.player.transform.position);
            Lumiere lumiere = gm.map.CreateLumiere(posLumiere, Lumiere.LumiereType.SPECIAL);
            AddGeoPoint(geoDataToLumiere, lumiere.transform);
        }
    }

    protected void AddGeoPoint(GeoData geoData, Transform target) {
        GeoData newGeoData = new GeoData(geoData);
        newGeoData.targetObject = target;
        gm.player.geoSphere.AddGeoPoint(newGeoData);
    }
}
