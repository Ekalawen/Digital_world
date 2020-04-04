using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ProtectLumieresWithCouronnes : MapFunctionComponent {

    public float proportionToProtect = 1.0f;
    public Cube.CubeType cubeType = Cube.CubeType.INDESTRUCTIBLE;
    public GameObject activationZonePrefab;

    public override void Activate() {
        int nbLumieres = map.GetLumieres().Count;
        int nbToProtect = Mathf.Clamp(Mathf.RoundToInt(nbLumieres * proportionToProtect), 0, nbLumieres);
        List<Lumiere> lumieresToProtect = GaussianGenerator.SelecteSomeNumberOf(map.GetLumieres(), nbToProtect);
        foreach(Lumiere lumiere in lumieresToProtect) {
            ProtectLumiere(lumiere);
        }
    }

    protected void ProtectLumiere(Lumiere lumiere) {
        MapContainer couronne = CreateCouronne(lumiere.transform.position);
        PopActivationZoneArround(couronne);
    }

    protected MapContainer CreateCouronne(Vector3 position) {
        Cube.CubeType oldType = map.GetCurrentCubeType();
        map.SetCurrentCubeType(cubeType);
        MapContainer couronne = MapContainer.CreateFromCenter(position, Vector3.one);
        map.SetCurrentCubeType(oldType);
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
            destroyCouronne.Initialize(couronne, button);
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
