using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateLabyrinthe : GenerateCubesMapFunction {

    protected GameObject mainSpaceCubesFolder;

    public override void Activate() {
        mainSpaceCubesFolder = new GameObject("MainSpace");
        mainSpaceCubesFolder.transform.SetParent(map.cubesFolder.transform);
        FullfillMapWithCubes();
        GenerateTheLabyrinthe();
    }

    protected void FullfillMapWithCubes() {
        // On remplit la map
        for(int i = 1; i <= map.tailleMap.x - 1; i++) {
            for(int j = 1; j <= map.tailleMap.y - 1; j++) {
                for(int k = 1; k <= map.tailleMap.z - 1; k++) {
                    Vector3 pos = new Vector3(i, j, k);
                    map.AddCube(pos, cubeType, Quaternion.identity, mainSpaceCubesFolder.transform);
                }
            }
        }
    }

    protected void GenerateTheLabyrinthe() {
        List<Vector3Int> chain = new List<Vector3Int>();
        chain.Add(MathTools.RoundToInt(map.GetCenter()));
        map.DeleteCubesAt(chain[0]);

        while(chain.Count > 0) {
            // Get un élément random
            Vector3Int current = chain[Random.Range(0, chain.Count)];

            // Si il a des voisins disponibles
            if (HasValidatedVoisins(current)) {
                // Prolonger l'un de ses elements jusqu'à son terme
                List<Vector3Int> newChain = GeneratePathFromPos(current);

                // Et rajouter tous ces nouveaux éléments à la chaîne !
                chain.AddRange(newChain);

            // Sinon le supprimer de la chaîne
            } else {
                chain.Remove(current);
            }
        }
    }

    protected List<Vector3Int> GeneratePathFromPos(Vector3Int current) {
        List<Vector3Int> path = new List<Vector3Int>();
        while(HasValidatedVoisins(current)) {
            List<Vector3Int> voisins = GetValidatedVoinsins(current);
            Vector3Int selected = voisins[Random.Range(0, voisins.Count)];

            path.Add(selected);
            map.DeleteCubesAt(selected);

            current = selected;
        }
        return path;
    }

    // Un voisins "validated" est un voisin "plein" qui n'est pas en contact direct avec un voisin "vide" ! :)
    // Un voisins "validated" ne peut pas non plus être sur le bord de la map !
    // ATTENTION : Fonction difficilement réutilisable ! (à cause du "== 1" !)
    protected List<Vector3Int> GetValidatedVoinsins(Vector3Int current) {
        List<Vector3Int> res = new List<Vector3Int>();
        int i = current.x, j = current.y, k = current.z;
        // DROITE
        if (map.IsInInsidedRegularMap(new Vector3Int(i + 1, j, k))
            && map.GetCubeAt(i + 1, j, k) != null
            && map.GetVoisinsLibresInMap(map.GetCubeAt(i + 1, j, k).transform.position).Count == 1)
            res.Add(new Vector3Int(i + 1, j, k));
        // GAUCHE
        if (map.IsInInsidedRegularMap(new Vector3Int(i - 1, j, k)) 
            && map.GetCubeAt(i - 1, j, k) != null
            && map.GetVoisinsLibresInMap(map.GetCubeAt(i - 1, j, k).transform.position).Count == 1)
            res.Add(new Vector3Int(i - 1, j, k));
        // HAUT
        if (map.IsInInsidedRegularMap(new Vector3Int(i, j + 1, k)) 
            && map.GetCubeAt(i, j + 1, k) != null
            && map.GetVoisinsLibresInMap(map.GetCubeAt(i, j + 1, k).transform.position).Count == 1)
            res.Add(new Vector3Int(i, j + 1, k));
        // BAS
        if (map.IsInInsidedRegularMap(new Vector3Int(i, j - 1, k)) 
            && map.GetCubeAt(i, j - 1, k) != null
            && map.GetVoisinsLibresInMap(map.GetCubeAt(i, j - 1, k).transform.position).Count == 1)
            res.Add(new Vector3Int(i, j - 1, k));
        // DEVANT
        if (map.IsInInsidedRegularMap(new Vector3Int(i, j, k + 1)) 
            && map.GetCubeAt(i, j, k + 1) != null
            && map.GetVoisinsLibresInMap(map.GetCubeAt(i, j, k + 1).transform.position).Count == 1)
            res.Add(new Vector3Int(i, j, k + 1));
        // DERRIRE
        if (map.IsInInsidedRegularMap(new Vector3Int(i, j, k - 1)) 
            && map.GetCubeAt(i, j, k - 1) != null
            && map.GetVoisinsLibresInMap(map.GetCubeAt(i, j, k - 1).transform.position).Count == 1)
            res.Add(new Vector3Int(i, j, k - 1));
        return res;
    }

    protected bool HasValidatedVoisins(Vector3Int current) {
        return GetValidatedVoinsins(current).Count > 0;
    }
}
