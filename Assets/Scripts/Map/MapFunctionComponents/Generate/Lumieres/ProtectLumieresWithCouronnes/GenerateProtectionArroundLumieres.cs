using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GenerateProtectionArroundLumieres : GenerateCubesMapFunction {

    public GenerateMode generateMode = GenerateMode.COUNT;
    [ConditionalHide("generateMode", GenerateMode.COUNT)]
    public int nbToProtect = 0;
    [ConditionalHide("generateMode", GenerateMode.PROPORTION)]
    public float proportionToProtect = 1.0f;
    public bool destroyAlreadyExistingCubes = true;
    public bool destroyAlreadyExistingCubesInMapBordures = false;

    //public float dureeDestruction = 1.0f;
    //public GameObject activationZonePrefab;

    public override void Activate() {
        int nbLumieres = map.GetLumieres().Count;
        int nb = (generateMode == GenerateMode.COUNT) ? nbToProtect : Mathf.Clamp(Mathf.RoundToInt(nbLumieres * proportionToProtect), 0, nbLumieres);
        nb = Mathf.Min(nb, nbLumieres);
        List<Lumiere> lumieresToProtect = GaussianGenerator.SelecteSomeNumberOf(map.GetLumieres(), nb);
        foreach(Lumiere lumiere in lumieresToProtect) {
            ProtectLumiere(lumiere);
        }
    }

    protected void ProtectLumiere(Lumiere lumiere) {
        DestroyAlreadyExistingCubes(lumiere.transform.position);
        MapContainer couronne = CreateCouronne(lumiere.transform.position);
        ProtectSpecific(lumiere, couronne);
    }

    protected virtual void ProtectSpecific(Lumiere lumiere, MapContainer couronne) {
    }

    protected void DestroyAlreadyExistingCubes(Vector3 center) {
        if(destroyAlreadyExistingCubes) {
            List<Cube> cubes = map.GetCubesInBox(center, Vector3.one);
            foreach(Cube cube in cubes) {
                if (destroyAlreadyExistingCubesInMapBordures || map.IsInInsidedRegularMap(cube.transform.position)) {
                    map.DeleteCube(cube);
                }
            }
        }
    }

    protected MapContainer CreateCouronne(Vector3 position) {
        MapContainer couronne = MapContainer.CreateFromCenter(position, Vector3.one);
        return couronne;
    }

    /// Utile pour créer les OrbTriggers !
    //protected void PopActivationZoneArround(MapContainer couronne) {
    //    if (activationZonePrefab != null) {
    //        Vector3 position = couronne.GetCenter();
    //        TimeZoneButton button = Instantiate(activationZonePrefab, position, Quaternion.identity, map.zonesFolder.transform).GetComponentInChildren<TimeZoneButton>();
    //        DestroyCouronne destroyCouronne = button.gameObject.AddComponent<DestroyCouronne>();
    //        destroyCouronne.Initialize(couronne, button, dureeDestruction);
    //        button.AddEvent(new UnityAction(destroyCouronne.DestroyTheCouronne));
    //        button.AddEvent(new UnityAction(destroyCouronne.DestroyButton));
    //    }
    //}
}
