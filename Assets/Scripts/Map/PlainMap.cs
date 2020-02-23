using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlainMap : MapManager {

	public int tailleX = 60; // La taille de la map en x
	public int tailleZ = 80; // La taille de la map en z
	public int ecartsPdc = 5; // Le pas d'échantillonnage des points de contrôles (leurs distances quoi)
	public float hauteurMax = 5f; // La hauteur maximale que peut prendre un point de contrôle, à partir de 0
	public int nbArbres = 1; // La quantité d'arbres
    public bool roundCubesPositions = true; // Permet de savoir s'il faut arrondir les positions des cubes à l'entier le plus proche

    protected override void GenerateMap() {
		generatePlainsMap(tailleX, tailleZ, ecartsPdc, hauteurMax);
    }

    // Génère une map avec une plaine interpolé
    void generatePlainsMap(int tailleX, int tailleZ, int ecartsPdc, float hauteurMax) {

		// On génère les points de contrôles
		Vector3[,] pdc = SurfaceInterpolante.GeneratePointsDeControle(new Vector2Int(tailleX, tailleZ), hauteurMax, ecartsPdc);

        // On crée une surface pour le sol ! :)
        SurfaceInterpolante surface = new SurfaceInterpolante(
            pdc,
            ecartsPdc,
            new Vector2(tailleX, tailleZ),
            roundCubesPositions);
        surface.GenerateCubes();

        // Puis on crée tous les arbres
        // On définit le nombre d'arbre en fonction de la surface de la map
        int surfaceMap = tailleX * tailleZ;
        List<Vector3> posArbresPossibles = new List<Vector3>();
        foreach (Vector3 pos in surface.GetAllPositions())
            posArbresPossibles.Add(pos);
        for (int i = 0; i < nbArbres; i++) {
            int indice = Random.Range(0, posArbresPossibles.Count);
            Arbre arbre = new Arbre(
                posArbresPossibles[indice],
                nbPalliersRange: new Vector2Int(3, 8),
                taillePallierRange: new Vector2Int(3, 8),
                hauteurPallierRange: new Vector2Int(8, 12),
                pasPdcPallier: 3,
                hauteurPdcPallier: 3f);
            //generateArbre(posArbresPossibles[indice]);
            posArbresPossibles.RemoveAt(indice);
        }
    }

    public override Vector3 GetPlayerStartPosition() {
        Vector3 res = base.GetPlayerStartPosition();
        while (!IsOverCube(res))
            res = base.GetPlayerStartPosition();
        return res;
    }

    public bool IsOverCube(Vector3 position) {
        RaycastHit hit;
        Ray ray = new Ray (position, Vector3.down);
        return Physics.Raycast(ray, out hit) && hit.collider.tag == "Cube";
    }
}
