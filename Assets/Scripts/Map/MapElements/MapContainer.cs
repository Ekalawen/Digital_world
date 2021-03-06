﻿using System.Collections;
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

    protected override void InitializeCubeEnsembleType() {
        cubeEnsembleType = CubeEnsembleType.MAP_CONTAINER;
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

    protected void GenerateMapContainer() {
        int x = (int)nbCubesParAxe[0];
        int y = (int)nbCubesParAxe[1];
        int z = (int)nbCubesParAxe[2];
        // bas
        Mur mur = new Mur(origin, Vector3.right, x, Vector3.forward, z);
        murs.Add(mur);
        cubes.AddRange(mur.GetCubes());
        // haut
        mur = new Mur(origin + Vector3.up * (y - 1), Vector3.right, x, Vector3.forward, z);
        murs.Add(mur);
        cubes.AddRange(mur.GetCubes());
        // gauche
        mur = new Mur(origin, Vector3.forward, z, Vector3.up, y);
        murs.Add(mur);
        cubes.AddRange(mur.GetCubes());
        // droite
        mur = new Mur(origin + Vector3.right * (x - 1), Vector3.forward, z, Vector3.up, y);
        murs.Add(mur);
        cubes.AddRange(mur.GetCubes());
        // avant
        mur = new Mur(origin, Vector3.right, x, Vector3.up, y);
        murs.Add(mur);
        cubes.AddRange(mur.GetCubes());
        // arrière
        mur = new Mur(origin + Vector3.forward * (z - 1), Vector3.right, x, Vector3.up, y);
        murs.Add(mur);
        cubes.AddRange(mur.GetCubes());
    }

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