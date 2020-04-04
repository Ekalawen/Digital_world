using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyCouronne : MonoBehaviour {

    public MapContainer couronne;
    public TimeZoneButton button;

    public void Initialize(MapContainer couronne, TimeZoneButton button) {
        this.couronne = couronne;
        this.button = button;
    }

    public void DestroyTheCouronne() {
        List<Cube> cubes = new List<Cube>();
        foreach (Cube cube in couronne.GetCubes())
            cubes.Add(cube);
        for(int i = 0; i < cubes.Count; i++) {
            Cube cube = cubes[i];
            cube.Explode();
        }
    }

    public void DestroyButton() {
        Destroy(button.gameObject);
    }
}
