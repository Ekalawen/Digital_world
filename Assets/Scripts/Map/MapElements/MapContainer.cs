using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapContainer : CubeEnsemble {

    Vector3 origin;
    Vector3 nbCubesParAxe;
    List<Mur> murs;

    public MapContainer(Vector3 origin, Vector3 nbCubesParAxe) : base() {
        this.origin = origin;
        this.nbCubesParAxe = nbCubesParAxe;
        this.murs = new List<Mur>();

        GenerateMapContainer();
    }

    protected void GenerateMapContainer() {
        // Haut
        Mur mur = new Mur(origin + Vector3.up * nbCubesParAxe[2],
            Vector3.right, (int)nbCubesParAxe[0] + 1, 
            Vector3.forward, (int)nbCubesParAxe[1] + 1);
        murs.Add(mur);
        cubes.AddRange(mur.GetCubes());
        // bas
        mur = new Mur(origin, 
            Vector3.right, (int)nbCubesParAxe[0] + 1,
            Vector3.forward, (int)nbCubesParAxe[1] + 1);
        murs.Add(mur);
        cubes.AddRange(mur.GetCubes());
        // gauche
        mur = new Mur(origin, 
            Vector3.forward, (int)nbCubesParAxe[1] + 1, 
            Vector3.up, (int)nbCubesParAxe[2] + 1);
        murs.Add(mur);
        cubes.AddRange(mur.GetCubes());
        // droite
        mur = new Mur(origin + Vector3.right * nbCubesParAxe[0], 
            Vector3.forward, (int)nbCubesParAxe[1] + 1, 
            Vector3.up, (int)nbCubesParAxe[2] + 1);
        murs.Add(mur);
        cubes.AddRange(mur.GetCubes());
        // avant
        mur = new Mur(origin, 
            Vector3.right, (int)nbCubesParAxe[0] + 1, 
            Vector3.up, (int)nbCubesParAxe[2] + 1);
        murs.Add(mur);
        cubes.AddRange(mur.GetCubes());
        // arrière
        mur = new Mur(origin + Vector3.forward * nbCubesParAxe[1], 
            Vector3.right, (int)nbCubesParAxe[0] + 1, 
            Vector3.up, (int)nbCubesParAxe[2] + 1);
        murs.Add(mur);
        cubes.AddRange(mur.GetCubes());
    }

    /// TODO !
    //public List<Cube> GetCoins() {
    //}
}
