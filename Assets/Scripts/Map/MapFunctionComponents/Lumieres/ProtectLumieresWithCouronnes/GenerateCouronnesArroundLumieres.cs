using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GenerateCouronnesArroundLumieres : GenerateCubesMapFunction {

    public float proportionToProtect = 1.0f;
    public bool useNbToProtect = false;
    public int nbToProtect = 0;
    public float dureeDestruction = 1.0f;
    public GameObject activationZonePrefab;
    public bool destroyAlreadyExistingCubes = true;
    public bool destroyAlreadyExistingCubesInMapBordures = false;

    public override void Activate() {
        int nbLumieres = map.GetLumieres().Count;
        int nb = useNbToProtect ? nbToProtect : Mathf.Clamp(Mathf.RoundToInt(nbLumieres * proportionToProtect), 0, nbLumieres);
        nb = Mathf.Min(nb, nbLumieres);
        List<Lumiere> lumieresToProtect = GaussianGenerator.SelecteSomeNumberOf(map.GetLumieres(), nb);
        foreach(Lumiere lumiere in lumieresToProtect) {
            ProtectLumiere(lumiere);
        }
    }

    protected void ProtectLumiere(Lumiere lumiere) {
        DestroyAlreadyExistingCubes(lumiere.transform.position);
        MapContainer couronne = CreateCouronne(lumiere.transform.position);
        PopActivationZoneArround(couronne);
    }

    protected void DestroyAlreadyExistingCubes(Vector3 center) {
        if(destroyAlreadyExistingCubes) {
            List<Cube> cubes = map.GetCubesInBox(center, Vector3.one);
            foreach(Cube cube in cubes) {
                if (destroyAlreadyExistingCubesInMapBordures || map.IsInInsidedRegularMap(cube.transform.position))
                    map.DeleteCube(cube);
            }
        }
    }

    protected MapContainer CreateCouronne(Vector3 position) {
        MapContainer couronne = MapContainer.CreateFromCenter(position, Vector3.one);
        return couronne;
    }

    protected void PopActivationZoneArround(MapContainer couronne) {
        if (activationZonePrefab != null) {
            //List<Vector3> positions = couronne.GetWallCenters(offset: 1.0f);
            //positions = KeepOnlyInInsidedRegularMap(positions);
            //Vector3 position = positions[Random.Range(0, positions.Count)];
            Vector3 position = couronne.GetCenter();
            TimeZoneButton button = Instantiate(activationZonePrefab, position, Quaternion.identity, map.zonesFolder.transform).GetComponentInChildren<TimeZoneButton>();
            DestroyCouronne destroyCouronne = button.gameObject.AddComponent<DestroyCouronne>();
            destroyCouronne.Initialize(couronne, button, dureeDestruction);
            button.AddEvent(new UnityAction(destroyCouronne.DestroyTheCouronne));
            button.AddEvent(new UnityAction(destroyCouronne.DestroyButton));
        }
    }

    protected List<Vector3> KeepOnlyInInsidedRegularMap(List<Vector3> positions) {
        List<Vector3> res = new List<Vector3>();
        foreach(Vector3 pos in positions) {
            if (map.IsInInsidedRegularMap(pos))
                res.Add(pos);
        }
        return res;
    }
}
