using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CouronneDestroyer : MonoBehaviour {

    public float dureeDestruction = 1.0f;
    public MapContainer couronne;
    public OrbTrigger orbTrigger;

    protected GameManager gm;

    public void Initialize(MapContainer couronne, OrbTrigger orbTrigger, float dureeDestruction) {
        this.couronne = couronne;
        this.orbTrigger = orbTrigger;
        this.dureeDestruction = dureeDestruction;
        gm = GameManager.Instance;
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
