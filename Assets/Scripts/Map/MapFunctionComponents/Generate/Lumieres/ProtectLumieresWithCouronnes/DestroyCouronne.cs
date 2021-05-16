using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DestroyCouronne : MonoBehaviour {

    public float dureeDestruction = 1.0f;
    public MapContainer couronne;
    public OrbTrigger button;

    protected GameManager gm;

    public void Initialize(MapContainer couronne, OrbTrigger button, float dureeDestruction) {
        this.couronne = couronne;
        this.button = button;
        this.dureeDestruction = dureeDestruction;
        gm = GameManager.Instance;
    }

    public void DestroyTheCouronne() {
        List<Cube> cubes = new List<Cube>();
        foreach (Cube cube in couronne.GetCubes())
            cubes.Add(cube);
        cubes = cubes.OrderBy(cube => Vector3.Distance(cube.transform.position, gm.player.transform.position)).ToList();
        float distanceMin = Vector3.Distance(cubes[0].transform.position, gm.player.transform.position);
        float distanceMax = Vector3.Distance(cubes[cubes.Count - 1].transform.position, gm.player.transform.position);
        for(int i = 0; i < cubes.Count; i++) {
            Cube cube = cubes[i];
            float distance = Vector3.Distance(cube.transform.position, gm.player.transform.position);
            float dureeBeforeExplosion = (distance - distanceMin) / (distanceMax - distanceMin) * dureeDestruction;
            cube.ExplodeIn(dureeBeforeExplosion);
        }
    }

    public void DestroyButton() {
        Destroy(button.gameObject);
    }
}
