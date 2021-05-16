using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class GenerateProtectionArroundLumieresWithOrbTrigger : GenerateProtectionArroundLumieres {

    public float rayon = 3.5f;
    public float durationToActivate = 7.0f;
    public float dureeDestruction = 1.0f;
    public GameObject orbTriggerPrefab;

    protected Lumiere lumiere;
    protected MapContainer couronne;
    protected OrbTrigger orbTrigger;

    protected override void ProtectSpecific(Lumiere lumiere, MapContainer couronne) {
        this.lumiere = lumiere;
        this.couronne = couronne;
        if (orbTriggerPrefab != null) {
            Vector3 lumierePosition = lumiere.transform.position;
            orbTrigger = Instantiate(orbTriggerPrefab, lumierePosition, Quaternion.identity, map.zonesFolder.transform).GetComponentInChildren<OrbTrigger>();
            orbTrigger.Initialize(rayon, durationToActivate);
            CouronneDestroyer couronneDestroyer = orbTrigger.gameObject.AddComponent<CouronneDestroyer>();
            couronneDestroyer.Initialize(couronne, orbTrigger, dureeDestruction);
            orbTrigger.AddEvent(new UnityAction(couronneDestroyer.DestroyTheCouronne));
            orbTrigger.AddEvent(new UnityAction(couronneDestroyer.DestroyButton));
        }
    }
}
