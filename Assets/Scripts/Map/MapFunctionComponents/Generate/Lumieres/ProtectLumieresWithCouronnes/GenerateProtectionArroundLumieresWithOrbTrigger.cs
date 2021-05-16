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
            orbTrigger.AddEvent(new UnityAction(DestroyTheCouronne));
            orbTrigger.AddEvent(new UnityAction(DestroyButton));
        }
    }

    public void DestroyTheCouronne() {
        List<Cube> cubes = couronne.GetCubes().Select(c => c).ToList();
        if (cubes.Count == 0) {
            return;
        }
        cubes = cubes.OrderBy(cube => Vector3.Distance(cube.transform.position, gm.player.transform.position)).ToList();
        float distanceMin = Vector3.Distance(cubes.First().transform.position, gm.player.transform.position);
        float distanceMax = Vector3.Distance(cubes.Last().transform.position, gm.player.transform.position);
        for(int i = 0; i < cubes.Count; i++) {
            Cube cube = cubes[i];
            float distance = Vector3.Distance(cube.transform.position, gm.player.transform.position);
            float dureeBeforeExplosion = MathCurves.LinearReversed(distanceMin, distanceMax, distance) * dureeDestruction;
            cube.ExplodeIn(dureeBeforeExplosion);
        }
    }

    public void DestroyButton() {
        Destroy(orbTrigger.gameObject);
    }
}
