using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BlackAndWhiteMap : PlainMap {

    public float minDistanceFromSurface = 3.0f;

    protected override void GenerateMap() {
		GenerateBlackAndWhiteMap();
    }

    // Génère une map particulière !
    void GenerateBlackAndWhiteMap() {

		// On génère les points de contrôles
		Vector3[,] pdc = SurfaceInterpolante.GeneratePointsDeControle(new Vector2Int(tailleX, tailleZ), hauteurMax, ecartsPdc);

        // On crée une surface pour le sol ! :)
        SurfaceInterpolante surface = new SurfaceInterpolante(
            pdc,
            ecartsPdc,
            new Vector2(tailleX, tailleZ),
            roundCubesPositions);
        currentCubeTypeUsed = Cube.CubeType.INDESTRUCTIBLE;
        float offsetY = tailleMap.y / 2.0f - hauteurMax / 2.0f;
        surface.AddOffset(Vector3.up * offsetY);
        surface.GenerateCubes();

        // RandomFilling with corrupted cubes !!
        currentCubeTypeUsed = Cube.CubeType.SPECIAL;
        GenerateRandomFilling();

        // Génerer les lumières !
        GenerateLumieres();
    }

    public override Vector3 GetPlayerStartPosition() {
        while (true) {
            Vector3 pos = GetFreeRoundedLocation();
            if (IsInTopPart(pos))
                return pos;
        }
    }

    // /!\ Se base sur le fait que le milieu de la map est découpé par des cubes indestructibles !
    public bool IsInTopPart(Vector3 pos) {
        Ray ray = new Ray(pos, Vector3.down);
        RaycastHit[] hits = Physics.RaycastAll(ray);
        foreach(RaycastHit hit in hits) {
            Cube cube = hit.collider.gameObject.GetComponent<Cube>();
            if(cube != null && cube.type == Cube.CubeType.INDESTRUCTIBLE) {
                return true;
            }
        }
        return false;
    }

    protected void GenerateLumieres() {
        List<Cube> surfacePositions = GetAllCubesOfType(Cube.CubeType.INDESTRUCTIBLE);
        for (int i = 0; i < nbLumieresInitial; i++) {
            Vector3 pos = GetFarFromEnsemble(surfacePositions, minDistanceFromSurface);
            CreateLumiere(pos, Lumiere.LumiereType.NORMAL);
        }
    }
}
