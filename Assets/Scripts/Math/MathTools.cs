﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MathTools
{

    public static bool IsRounded(Vector3 pos) {
        return Mathf.Round(pos.x) == pos.x && Mathf.Round(pos.y) == pos.y && Mathf.Round(pos.z) == pos.z;
    }

    public static Vector3 Round(Vector3 pos)
    {
        Vector3 res;
        res.x = Mathf.Round(pos.x);
        res.y = Mathf.Round(pos.y);
        res.z = Mathf.Round(pos.z);
        return res;
    }

    public static float CubeDistance(Vector3 v1, Vector3 v2) {
        Vector3Int start = RoundToInt(v1);
        Vector3Int end = RoundToInt(v2);
        return Mathf.Abs(start.x - end.x) + Mathf.Abs(start.y - end.y) + Mathf.Abs(start.z - end.z);
    }

    public static Vector3Int RoundToInt(Vector3 pos) {
        Vector3Int res = new Vector3Int();
        res.x = (int)Mathf.Round(pos.x);
        res.y = (int)Mathf.Round(pos.y);
        res.z = (int)Mathf.Round(pos.z);
        return res;
    }

    public static Vector3Int FloorToInt(Vector3 pos) {
        return new Vector3Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
    }

    public static void Shuffle<T>(List<T> ts)
    {
        var count = ts.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i)
        {
            var r = UnityEngine.Random.Range(i, count);
            var tmp = ts[i];
            ts[i] = ts[r];
            ts[r] = tmp;
        }
    }

    public static int RandBetween(int startInclusive, int endInclusive) {
        return UnityEngine.Random.Range(startInclusive, endInclusive + 1);
    }

    public static int RandBetween(Vector2Int range) {
        return RandBetween(range.x, range.y);
    }

    public static T GetOne<T>(List<T> vector) {
        return vector[UnityEngine.Random.Range(0, vector.Count)];
    }

    public static bool AreListEqual<T>(List<T> l1, List<T> l2) {
        if (l1.Count != l2.Count)
            return false;
        for(int i = 0; i < l1.Count; i++) {
            if (!l1[i].Equals(l2[i]))
                return false;
        }
        return true;
    }

    public static List<Vector3> GetAllVoisins(Vector3 pos) {
        List<Vector3> res = new List<Vector3>();
        res.Add(pos + Vector3.forward);
        res.Add(pos - Vector3.forward);
        res.Add(pos + Vector3.right);
        res.Add(pos - Vector3.right);
        res.Add(pos + Vector3.up);
        res.Add(pos - Vector3.up);
        return res;
    }

    public static float RandomSign() {
        return UnityEngine.Random.value < 0.5f ? 1.0f : -1.0f;
    }

    public static bool AABBSphere(Vector3 aabbCenter, Vector3 aabbHalfExtents, Vector3 sphereCenter, float sphereRayon) {
        Vector3 aabbClosestPointToSphere = new Vector3(
            Mathf.Clamp(sphereCenter.x, aabbCenter.x - aabbHalfExtents.x, aabbCenter.x + aabbHalfExtents.x),
            Mathf.Clamp(sphereCenter.y, aabbCenter.y - aabbHalfExtents.y, aabbCenter.y + aabbHalfExtents.y),
            Mathf.Clamp(sphereCenter.z, aabbCenter.z - aabbHalfExtents.z, aabbCenter.z + aabbHalfExtents.z));
        return Vector3.Distance(aabbClosestPointToSphere, sphereCenter) <= sphereRayon;
    }

    public static bool AABB_AABB(Vector3 center1, Vector3 halfExtents1, Vector3 center2, Vector3 halfExtents2) {
        Vector3 centersDistances = new Vector3(
            Mathf.Abs(center1.x - center2.x),
            Mathf.Abs(center1.y - center2.y),
            Mathf.Abs(center1.z - center2.z));
        bool collisionX = halfExtents1.x + halfExtents2.x > centersDistances.x;
        bool collisionY = halfExtents1.y + halfExtents2.y > centersDistances.y;
        bool collisionZ = halfExtents1.z + halfExtents2.z > centersDistances.z;
        return collisionX && collisionY && collisionZ;
    }


    public static bool OBBSphere(Vector3 obbCenter, Vector3 obbHalfExtents, Quaternion obbRotation, Vector3 sphereCenter, float sphereRayon) {
        Quaternion alignRotation = Quaternion.Inverse(obbRotation);
        Vector3 newObbCenter = alignRotation * obbCenter;
        Vector3 newSphereCenter = alignRotation * sphereCenter;
        bool collision = AABBSphere(newObbCenter, obbHalfExtents, newSphereCenter, sphereRayon);
        return collision;

        /// Pour si un jour on fait des tests XD
        //Vector3 obbCenter = new Vector3(0, 0, 0);
        //Vector3 obbHalfExtent = Vector3.one;
        //Quaternion rotation = Quaternion.identity;
        //Vector3 sphereCenter = new Vector3(2, 2, 2);
        //float sphereRayon = 1.73f;
        //bool collision = MathTools.OBBSphere(obbCenter, obbHalfExtent, rotation, sphereCenter, sphereRayon);
        //Debug.Log($"obb = ({obbCenter}, {rotation}) sphere = ({sphereCenter}, {sphereRayon}) collision = {collision}");
        //// Doit retourner FALSE;

        //obbCenter = new Vector3(0, 0, 0);
        //obbHalfExtent = Vector3.one;
        //rotation = Quaternion.identity;
        //sphereCenter = new Vector3(2, 2, 2);
        //sphereRayon = 1.74f;
        //collision = MathTools.OBBSphere(obbCenter, obbHalfExtent, rotation, sphereCenter, sphereRayon);
        //Debug.Log($"obb = ({obbCenter}, {rotation}) sphere = ({sphereCenter}, {sphereRayon}) collision = {collision}");
        //// Doit retourner TRUE;

        //obbCenter = new Vector3(0, 0, 0);
        //obbHalfExtent = Vector3.one;
        //rotation = Quaternion.Euler(0, 0, 0);
        //sphereCenter = new Vector3(2, 0, 0);
        //sphereRayon = 0.9f;
        //collision = MathTools.OBBSphere(obbCenter, obbHalfExtent, rotation, sphereCenter, sphereRayon);
        //Debug.Log($"obb = ({obbCenter}, {rotation}) sphere = ({sphereCenter}, {sphereRayon}) collision = {collision}");
        //// Doit retourner FALSE;

        //obbCenter = new Vector3(0, 0, 0);
        //obbHalfExtent = Vector3.one;
        //rotation = Quaternion.Euler(0, 0, 0);
        //sphereCenter = new Vector3(2, 0, 0);
        //sphereRayon = 1.1f;
        //collision = MathTools.OBBSphere(obbCenter, obbHalfExtent, rotation, sphereCenter, sphereRayon);
        //Debug.Log($"obb = ({obbCenter}, {rotation}) sphere = ({sphereCenter}, {sphereRayon}) collision = {collision}");
        //// Doit retourner TRUE;

        //obbCenter = new Vector3(0, 0, 0);
        //obbHalfExtent = Vector3.one;
        //rotation = Quaternion.Euler(0, 0, 45);
        //sphereCenter = new Vector3(2, 0, 0);
        //sphereRayon = 0.58f;
        //collision = MathTools.OBBSphere(obbCenter, obbHalfExtent, rotation, sphereCenter, sphereRayon);
        //Debug.Log($"obb = ({obbCenter}, {rotation}) sphere = ({sphereCenter}, {sphereRayon}) collision = {collision}");
        //// Doit retourner FALSE;

        //obbCenter = new Vector3(0, 0, 0);
        //obbHalfExtent = Vector3.one;
        //rotation = Quaternion.Euler(0, 0, 45);
        //sphereCenter = new Vector3(2, 0, 0);
        //sphereRayon = 0.60f;
        //collision = MathTools.OBBSphere(obbCenter, obbHalfExtent, rotation, sphereCenter, sphereRayon);
        //Debug.Log($"obb = ({obbCenter}, {rotation}) sphere = ({sphereCenter}, {sphereRayon}) collision = {collision}");
        //// Doit retourner TRUE;

        //obbCenter = new Vector3(10, 100, 0);
        //obbHalfExtent = Vector3.one;
        //rotation = Quaternion.Euler(45, 0, 0);
        //sphereCenter = new Vector3(10, 100, 2);
        //sphereRayon = 0.58f;
        //collision = MathTools.OBBSphere(obbCenter, obbHalfExtent, rotation, sphereCenter, sphereRayon);
        //Debug.Log($"obb = ({obbCenter}, {rotation}) sphere = ({sphereCenter}, {sphereRayon}) collision = {collision}");
        //// Doit retourner FALSE;

        //obbCenter = new Vector3(10, 100, 0);
        //obbHalfExtent = Vector3.one;
        //rotation = Quaternion.Euler(45, 0, 0);
        //sphereCenter = new Vector3(10, 100, 2);
        //sphereRayon = 0.60f;
        //collision = MathTools.OBBSphere(obbCenter, obbHalfExtent, rotation, sphereCenter, sphereRayon);
        //Debug.Log($"obb = ({obbCenter}, {rotation}) sphere = ({sphereCenter}, {sphereRayon}) collision = {collision}");
        //// Doit retourner TRUE;
    }

    public static List<Vector3> GetAllNormals(Transform t) {
        return new List<Vector3>() {
            t.forward,
            - t.forward,
            t.right,
            - t.right,
            t.up,
            - t.up,
        };
    }

    public static Vector3 GetClosestToNormals(Transform t, Vector3 currentNormal) {
        List<Vector3> normals = MathTools.GetAllNormals(t);
        return normals.OrderBy(n => Vector3.Dot(currentNormal, n)).Last();
    }

    public static Vector3 VecMul(Vector3 vector1, Vector3 vector2) {
        return new Vector3(vector1.x * vector2.x, vector1.y * vector2.y, vector1.z * vector2.z);
    }

    public static bool IsAdjacent(Cube c1, Cube c2) {
        return IsAdjacent(c1.transform.position, c2.transform.position);
    }

    public static bool IsAdjacent(Vector3 v1, Vector3 v2) {
        return IsRounded(v1) && IsRounded(v2) && CubeDistance(v1, v2) == 1;
    }

    public static List<Vector3> GetBoxCorners() {
        return new List<Vector3>() {
            new Vector3(-1.0f, -1.0f, -1.0f),
            new Vector3(-1.0f, -1.0f, +1.0f),
            new Vector3(+1.0f, -1.0f, -1.0f),
            new Vector3(+1.0f, -1.0f, +1.0f),

            new Vector3(-1.0f, +1.0f, -1.0f),
            new Vector3(-1.0f, +1.0f, +1.0f),
            new Vector3(+1.0f, +1.0f, -1.0f),
            new Vector3(+1.0f, +1.0f, +1.0f),
        };
    }

    public static List<Tuple<Vector3, Vector3>> GetBoxEdges() {
        List<Tuple<Vector3, Vector3>> edges = new List<Tuple<Vector3, Vector3>>() {
            // face du bas
            new Tuple<Vector3, Vector3>(new Vector3(-1, -1, -1), new Vector3(-1, -1, 1)),
            new Tuple<Vector3, Vector3>(new Vector3(-1, -1, -1), new Vector3(1, -1, -1)),
            new Tuple<Vector3, Vector3>(new Vector3(1, -1, 1), new Vector3(-1, -1, 1)),
            new Tuple<Vector3, Vector3>(new Vector3(1, -1, 1), new Vector3(1, -1, -1)),
            // face du haut
            new Tuple<Vector3, Vector3>(new Vector3(-1, 1, -1), new Vector3(-1, 1, 1)),
            new Tuple<Vector3, Vector3>(new Vector3(-1, 1, -1), new Vector3(1, 1, -1)),
            new Tuple<Vector3, Vector3>(new Vector3(1, 1, 1), new Vector3(-1, 1, 1)),
            new Tuple<Vector3, Vector3>(new Vector3(1, 1, 1), new Vector3(1, 1, -1)),
            // arêtes verticales
            new Tuple<Vector3, Vector3>(new Vector3(-1, -1, -1), new Vector3(-1, 1, -1)),
            new Tuple<Vector3, Vector3>(new Vector3(-1, -1, 1), new Vector3(-1, 1, 1)),
            new Tuple<Vector3, Vector3>(new Vector3(1, -1, -1), new Vector3(1, 1, -1)),
            new Tuple<Vector3, Vector3>(new Vector3(1, -1, 1), new Vector3(1, 1, 1)),
        };
        return edges;
    }

    public static bool IsInPlane(Vector3 point, Vector3 planePoint, Vector3 planeNormal) {
        return point == Vector3.ProjectOnPlane(point - planePoint, planeNormal) + planePoint;
    }
}