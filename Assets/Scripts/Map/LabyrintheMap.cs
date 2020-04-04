using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Permet de générer une carte contenant un cube plein. A l'intérieur duquel est creusé un labyrinthe parfait ! :)
public class LabyrintheMap : MapManager {

    //public int largeurCouloir = 1;

    [HideInInspector] public int volumeMap;
    protected GameObject mainSpaceCubesFolder;

    protected override void GenerateMap() {
        volumeMap = (int)GetVolume();
        mainSpaceCubesFolder = new GameObject("MainSpace");
        mainSpaceCubesFolder.transform.SetParent(cubesFolder.transform);

        GenerateLabyrintheMap();
    }

    // Crée une map en forme de Cube et creuse un labyrinthe à l'intérieur ! :)
    void GenerateLabyrintheMap() {
        // On crée le contour de la map !
        currentCubeTypeUsed = Cube.CubeType.INDESTRUCTIBLE;
        MapContainer mapContainer = new MapContainer(Vector3.zero, new Vector3(tailleMap.x, tailleMap.y, tailleMap.z) + Vector3.one);

        // On remplit la map
        currentCubeTypeUsed = Cube.CubeType.NORMAL;
        for(int i = 1; i <= tailleMap.x - 1; i++) {
            for(int j = 1; j <= tailleMap.y - 1; j++) {
                for(int k = 1; k <= tailleMap.z - 1; k++) {
                    Vector3 pos = new Vector3(i, j, k);
                    AddCube(pos, Cube.CubeType.NORMAL, Quaternion.identity, mainSpaceCubesFolder.transform);
                }
            }
        }

        // Puis on creuse à l'interieur !
        GenerateLabyrinthe();
    }

    protected void GenerateLabyrinthe() {
        List<Vector3Int> chain = new List<Vector3Int>();
        chain.Add(MathTools.RoundToInt(GetCenter()));
        DeleteCubesAt(chain[0]);

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
            DeleteCubesAt(selected);

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
        if (IsInInsidedRegularMap(new Vector3Int(i + 1, j, k))
            && cubesRegular[i + 1, j, k] != null
            && GetVoisinsLibres(cubesRegular[i + 1, j, k].transform.position).Count == 1)
            res.Add(new Vector3Int(i + 1, j, k));
        // GAUCHE
        if (IsInInsidedRegularMap(new Vector3Int(i - 1, j, k)) 
            && cubesRegular[i - 1, j, k] != null
            && GetVoisinsLibres(cubesRegular[i - 1, j, k].transform.position).Count == 1)
            res.Add(new Vector3Int(i - 1, j, k));
        // HAUT
        if (IsInInsidedRegularMap(new Vector3Int(i, j + 1, k)) 
            && cubesRegular[i, j + 1, k] != null
            && GetVoisinsLibres(cubesRegular[i, j + 1, k].transform.position).Count == 1)
            res.Add(new Vector3Int(i, j + 1, k));
        // BAS
        if (IsInInsidedRegularMap(new Vector3Int(i, j - 1, k)) 
            && cubesRegular[i, j - 1, k] != null
            && GetVoisinsLibres(cubesRegular[i, j - 1, k].transform.position).Count == 1)
            res.Add(new Vector3Int(i, j - 1, k));
        // DEVANT
        if (IsInInsidedRegularMap(new Vector3Int(i, j, k + 1)) 
            && cubesRegular[i, j, k + 1] != null
            && GetVoisinsLibres(cubesRegular[i, j, k + 1].transform.position).Count == 1)
            res.Add(new Vector3Int(i, j, k + 1));
        // DERRIRE
        if (IsInInsidedRegularMap(new Vector3Int(i, j, k - 1)) 
            && cubesRegular[i, j, k - 1] != null
            && GetVoisinsLibres(cubesRegular[i, j, k - 1].transform.position).Count == 1)
            res.Add(new Vector3Int(i, j, k - 1));
        return res;
    }

    protected bool HasValidatedVoisins(Vector3Int current) {
        return GetValidatedVoinsins(current).Count > 0;
    }
}