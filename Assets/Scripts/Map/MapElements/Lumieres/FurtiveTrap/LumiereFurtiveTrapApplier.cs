using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LumiereFurtiveTrapApplier : MonoBehaviour {


    protected GameManager gm;
    protected GameObject ennemiToGeneratePrefab;
    protected float timeBeforeEnnemisActivation = 3.0f;
    protected GameObject lumiereGeneratedPrefab;
    protected int nbLumieresToGenerate = 1;
    protected GameObject pouvoirDashPrefab;
    protected PouvoirGiverItem.PouvoirBinding pouvoirBinding;

    public void ApplyTrap(
        GameObject ennemiToGeneratePrefab,
        float timeBeforeEnnemisActivation,
        GameObject lumiereGeneratedPrefab,
        int nbLumieresToGenerate,
        GameObject pouvoirDashPrefab,
        PouvoirGiverItem.PouvoirBinding pouvoirBinding) {
        this.ennemiToGeneratePrefab = ennemiToGeneratePrefab;
        this.timeBeforeEnnemisActivation = timeBeforeEnnemisActivation;
        this.lumiereGeneratedPrefab = lumiereGeneratedPrefab;
        this.nbLumieresToGenerate = nbLumieresToGenerate;
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
        gm.console.AjouterMessageImportant(gm.console.strings.analyzeTrapWait, Console.TypeText.ALLY_TEXT, firstWaitingTime);
        yield return new WaitForSeconds(firstWaitingTime);
        yield return null;
        float secondWaitingTime = 2.0f;
        gm.console.AjouterMessageImportant(gm.console.strings.analyzeTrapSortie, Console.TypeText.ALLY_TEXT, secondWaitingTime);
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
            Ennemi ennemi = gm.ennemiManager.GenerateEnnemiFromPrefab(ennemiToGeneratePrefab, corner);
            IController ennemiController = ennemi.GetComponent<IController>();
            ennemiController.tempsInactifDebutJeu = Time.timeSinceLevelLoad + timeBeforeEnnemisActivation;
        }
    }

    protected void PopLumieres() {
        for (int i = 0; i < nbLumieresToGenerate; i++) {
            Vector3 posLumiere = gm.map.GetFarRoundedLocation(gm.player.transform.position);
            gm.map.CreateLumiere(posLumiere, Lumiere.LumiereType.SPECIAL);
        }
    }
}
