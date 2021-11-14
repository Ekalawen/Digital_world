using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AllumeLumiere : MonoBehaviour {

    public enum AllumageType { PROCHE, ELOIGNE }

    public AllumageType type = AllumageType.PROCHE;
    public bool shouldThrowLightningToNewLumiere = true;
    [ConditionalHide("shouldThrowLightningToNewLumiere")]
    public GameObject lightningPrefab;
    public GeoData geoData;

    protected GameManager gm;
    protected MapManager map;

    protected void Start() {
        gm = GameManager.Instance;
        map = gm.map;
    }

    public void AllumeOneLumiere(Vector3 position)
    {
        List<Lumiere> lumieres = map.GetLumieres();
        foreach (Lumiere lumiere in lumieres)
        {
            LumiereSwitchable ls = (LumiereSwitchable)lumiere;
            ls.SetState(LumiereSwitchable.LumiereSwitchableState.OFF);
        }
        if (lumieres.Count <= 0)
            return;
        Lumiere chosenOne = GetChosenOne(position);
        LumiereSwitchable chosenOneSwitchable = (LumiereSwitchable)chosenOne;
        chosenOneSwitchable.SetState(LumiereSwitchable.LumiereSwitchableState.ON);
        chosenOneSwitchable.TriggerLightExplosion();

        Lightning lightning = ThrowLightning(position, chosenOneSwitchable.transform.position);
        AddGeoPoint(chosenOneSwitchable.transform.position, lightning);
    }

    public void AddGeoPoint(Vector3 targetPosition, Lightning lightning) {
        // On va volontairement ne pas créer de copie de la geoData pour qu'il ne puisse y en avoir qu'une seule de ce type à la fois !
        float duration = lightning != null ? lightning.GetTotalDuration() : geoData.duration;
        geoData.duration = duration;
        geoData.SetTargetPosition(targetPosition);
        gm.player.geoSphere.AddGeoPoint(geoData);
    }

    protected Lightning ThrowLightning(Vector3 from, Vector3 to) {
        if (shouldThrowLightningToNewLumiere) {
            Lightning lightning = Instantiate(lightningPrefab, from, Quaternion.identity).GetComponent<Lightning>();
            lightning.Initialize(from, to);
            return lightning;
        }
        return null;
    }

    private Lumiere GetChosenOne(Vector3 position) {
        if(type == AllumageType.PROCHE)
            return map.GetLumieres().OrderBy(l => Vector3.Distance(l.transform.position, position)).First();
        else
            return map.GetLumieres().OrderBy(l => Vector3.Distance(l.transform.position, position)).Last();
    }
}
