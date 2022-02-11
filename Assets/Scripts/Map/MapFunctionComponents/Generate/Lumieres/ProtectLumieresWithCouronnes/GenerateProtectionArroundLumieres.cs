using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class GenerateProtectionArroundLumieres : GenerateCubesMapFunction {

    public GenerateMode generateMode = GenerateMode.COUNT;
    [ConditionalHide("generateMode", GenerateMode.COUNT)]
    public int nbToProtect = 0;
    [ConditionalHide("generateMode", GenerateMode.PROPORTION)]
    public float proportionToProtect = 1.0f;
    public float couronneSize = 1;
    public bool destroyAlreadyExistingCubes = true;
    public bool destroyAlreadyExistingCubesInMapBordures = false;
    public bool setLinky = true;
    public bool setLumiereInaccessible = false;
    public bool makeOneDirectionHole = false;

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
        if(setLinky) {
            foreach(Cube cube in couronne.GetCubes()) {
                cube.SetLinky();
            }
        }
        if(gm.timerManager.HasGameStarted()) {
            foreach(Cube cube in couronne.GetCubes()) {
                cube.RegisterCubeToColorSources();
            }
        }
        ProtectSpecific(lumiere, couronne);
        SetLumiereInaccessible(lumiere, couronne);
    }

    protected void SetLumiereInaccessible(Lumiere lumiere, MapContainer couronne) {
        if (!setLumiereInaccessible) {
            return;
        }
        lumiere.SetInaccessible();
        couronne.onDeleteCube.AddListener(lumiere.SetAccessibleForLumiereProtection);
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
        MapContainer couronne = MapContainer.CreateFromCenter(position, Vector3.one * couronneSize);
        if(makeOneDirectionHole) {
            RemoveCubesOfCouronneInOneDirection(couronne);
        }
        return couronne;
    }

    protected void RemoveCubesOfCouronneInOneDirection(MapContainer couronne) {
        Vector3 randomDirection = MathTools.ChoseOne(new List<Vector3>() { Vector3.right, Vector3.up, Vector3.forward });
        Mur murAvant = couronne.GetMurInDirection(randomDirection);
        Mur murArriere = couronne.GetMurInDirection(-randomDirection);
        List<Cube> cubesInMurs = murAvant.GetCubesNotInEdges();
        cubesInMurs.AddRange(murArriere.GetCubesNotInEdges());
        foreach(Cube cube in cubesInMurs) {
            map.DeleteCube(cube);
        }
    }
}
