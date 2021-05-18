using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class GenerateProtectionArroundLumieresWithOrbTrigger : GenerateProtectionArroundLumieres {

    public float rayon = 3.5f;
    public float durationToActivate = 7.0f;
    public float dureeDestruction = 1.0f;
    public float dureeDecompose = 0.4f;
    public GameObject orbTriggerPrefab;

    protected Lumiere lumiere;
    protected MapContainer couronne;
    protected OrbTrigger orbTrigger;
    protected CouronneDestroyer couronneDestroyer;

    protected override void ProtectSpecific(Lumiere lumiere, MapContainer couronne) {
        this.lumiere = lumiere;
        this.couronne = couronne;
        if (orbTriggerPrefab != null) {
            Vector3 lumierePosition = lumiere.transform.position;
            lumiere.SetInaccessible();
            orbTrigger = Instantiate(orbTriggerPrefab, lumierePosition, Quaternion.identity, map.zonesFolder.transform).GetComponentInChildren<OrbTrigger>();
            orbTrigger.Initialize(rayon, durationToActivate);
            couronneDestroyer = orbTrigger.gameObject.AddComponent<CouronneDestroyer>();
            couronneDestroyer.Initialize(couronne, orbTrigger, lumiere, dureeDestruction, dureeDecompose);
            orbTrigger.AddEvent(new UnityAction(couronneDestroyer.DestroyProtection));
            couronne.onDeleteCube.AddListener(new UnityAction<Cube>(couronneDestroyer.OnDeleteCouronneCube));
        }
    }
}
