using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapContainer : CubeEnsemble
{

    Vector3 origin;
    Vector3 nbCubesParAxe;
    List<Mur> murs;

    public MapContainer(Vector3 origin, Vector3 nbCubesParAxe) : base()
    {
        this.origin = origin;
        this.nbCubesParAxe = nbCubesParAxe;
        this.murs = new List<Mur>();

        GenerateMapContainer();
    }

    // nbCubesParAxe = 1 + halfExtents * 2
    public static MapContainer CreateFromCenter(Vector3 center, Vector3 halfExtentsParAxe) {
        Vector3 nbCubesParAxe = Vector3.one + halfExtentsParAxe * 2.0f;
        Vector3 origin = center - halfExtentsParAxe;
        return new MapContainer(origin, nbCubesParAxe);
    }

    public override string GetName()
    {
        return "MapContainer";
    }

    protected void GenerateMapContainer()
    {
        // Haut
        Mur mur = new Mur(origin + Vector3.up * (nbCubesParAxe[2] - 1),
            Vector3.right, (int)nbCubesParAxe[0],
            Vector3.forward, (int)nbCubesParAxe[1]);
        murs.Add(mur);
        cubes.AddRange(mur.GetCubes());
        // bas
        mur = new Mur(origin,
            Vector3.right, (int)nbCubesParAxe[0],
            Vector3.forward, (int)nbCubesParAxe[1]);
        murs.Add(mur);
        cubes.AddRange(mur.GetCubes());
        // gauche
        mur = new Mur(origin,
            Vector3.forward, (int)nbCubesParAxe[1],
            Vector3.up, (int)nbCubesParAxe[2]);
        murs.Add(mur);
        cubes.AddRange(mur.GetCubes());
        // droite
        mur = new Mur(origin + Vector3.right * (nbCubesParAxe[0] - 1),
            Vector3.forward, (int)nbCubesParAxe[1],
            Vector3.up, (int)nbCubesParAxe[2]);
        murs.Add(mur);
        cubes.AddRange(mur.GetCubes());
        // avant
        mur = new Mur(origin,
            Vector3.right, (int)nbCubesParAxe[0],
            Vector3.up, (int)nbCubesParAxe[2]);
        murs.Add(mur);
        cubes.AddRange(mur.GetCubes());
        // arrière
        mur = new Mur(origin + Vector3.forward * (nbCubesParAxe[1] - 1),
            Vector3.right, (int)nbCubesParAxe[0],
            Vector3.up, (int)nbCubesParAxe[2]);
        murs.Add(mur);
        cubes.AddRange(mur.GetCubes());
    }

    /// TODO !
    //public List<Cube> GetCoins() {
    //}

    public List<Mur> GetMurs() {
        return murs;
    }

    public Vector3 GetCenter() {
        return origin + (nbCubesParAxe - Vector3.one) / 2.0f;
    }

    public List<Vector3> GetWallCenters(float offset = 0.0f) {
        Vector3 center = GetCenter();
        List<Vector3> res = new List<Vector3>();
        res.Add(center + Vector3.right * ((nbCubesParAxe.x - 1) / 2.0f + offset));
        res.Add(center + Vector3.left * ((nbCubesParAxe.x - 1) / 2.0f + offset));
        res.Add(center + Vector3.up * ((nbCubesParAxe.y - 1) / 2.0f + offset));
        res.Add(center + Vector3.down * ((nbCubesParAxe.y - 1) / 2.0f + offset));
        res.Add(center + Vector3.forward * ((nbCubesParAxe.z - 1) / 2.0f + offset));
        res.Add(center + Vector3.back * ((nbCubesParAxe.z - 1) / 2.0f + offset));
        return res;
    }
}