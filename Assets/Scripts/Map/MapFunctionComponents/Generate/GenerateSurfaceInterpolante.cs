using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateSurfaceInterpolante : GenerateCubesMapFunction {

	public int ecartsPdc = 5; // Le pas d'échantillonnage des points de contrôles (leurs distances quoi)
	public float hauteurMax = 5f; // La hauteur maximale que peut prendre un point de contrôle, à partir de 0
    public bool roundCubesPositions = true; // Permet de savoir s'il faut arrondir les positions des cubes à l'entier le plus proche

    public override void Activate() {
        GenerateSurface();
    }

    protected void GenerateSurface() {
		// On génère les points de contrôles
		Vector3[,] pdc = SurfaceInterpolante.GeneratePointsDeControle(new Vector2Int(map.tailleMap.x, map.tailleMap.z), hauteurMax, ecartsPdc);

        // On crée une surface pour le sol ! :)
        SurfaceInterpolante surface = new SurfaceInterpolante(
            pdc,
            ecartsPdc,
            new Vector2(map.tailleMap.x, map.tailleMap.z),
            roundCubesPositions);
        float offsetY = map.tailleMap.y / 2.0f - hauteurMax / 2.0f; // De base la surface est centrée ! :)
        surface.AddOffset(Vector3.up * offsetY);
        surface.GenerateCubes();
    }
}
