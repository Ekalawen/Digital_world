using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CouronneDestroyer : MonoBehaviour {

    protected float dureeDestruction = 1.0f;
    protected float dureeDecompose = 0.4f;
    protected MapContainer couronne;
    protected OrbTrigger orbTrigger;
    protected Lumiere lumiere;
    protected GameManager gm;
    protected bool isDestroyed = false;

    public void Initialize(MapContainer couronne, OrbTrigger orbTrigger, Lumiere lumiere, float dureeDestruction, float dureeDecompose) {
        this.couronne = couronne;
        this.orbTrigger = orbTrigger;
        this.lumiere = lumiere;
        this.dureeDestruction = dureeDestruction;
        this.dureeDecompose = dureeDecompose;
        gm = GameManager.Instance;
    }

    public void DestroyTheCouronne() {
        List<Cube> cubes = couronne.GetCubes().Select(c => c).ToList();
        if (cubes.Count == 0) {
            return;
        }
        if (!cubes[0].IsLinky()) {
            DecomposeProgressively(cubes);
        } else {
            List<Cube> linkedCubes = new List<Cube>();
            foreach(Cube cube in cubes) {
                if (!linkedCubes.Contains(cube)) {
                    linkedCubes.AddRange(cube.GetLinkyCubeComponent().GetLinkedCubes());
                }
            }
            DecomposeProgressively(linkedCubes);
        }
    }

    public void DecomposeProgressively(List<Cube> cubes) {
        cubes = cubes.OrderBy(cube => Vector3.Distance(cube.transform.position, gm.player.transform.position)).ToList();
        float distanceMin = Vector3.Distance(cubes.First().transform.position, gm.player.transform.position);
        float distanceMax = Vector3.Distance(cubes.Last().transform.position, gm.player.transform.position);
        for (int i = 0; i < cubes.Count; i++) {
            Cube cube = cubes[i];
            float distance = Vector3.Distance(cube.transform.position, gm.player.transform.position);
            float dureeBeforeDecompose = MathCurves.LinearReversed(distanceMin, distanceMax, distance) * dureeDestruction;
            cube.RealDecomposeIn(dureeDecompose, dureeBeforeDecompose);
        }
    }

    public void DestroyProtection() {
        if (!isDestroyed) {
            isDestroyed = true;
            DestroyTheCouronne();
            orbTrigger.ReduceAndDestroy();
            lumiere.SetAccessible();
        }
    }

    public void OnDeleteCouronneCube(Cube cube) {
        if (gm.timerManager.HasGameStarted()) {
            DestroyProtection();
        }
    }
}
