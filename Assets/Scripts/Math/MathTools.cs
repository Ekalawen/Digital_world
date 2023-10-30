using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public static class MathTools
{
    public static bool IsInteger(float value) {
        return value == Mathf.Round(value);
    }

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

    public static Vector3 Min(Vector3 v1, Vector3 v2)
    {
        Vector3 res;
        res.x = Mathf.Min(v1.x, v2.x);
        res.y = Mathf.Min(v1.y, v2.y);
        res.z = Mathf.Min(v1.z, v2.z);
        return res;
    }

    public static Vector3 Max(Vector3 v1, Vector3 v2)
    {
        Vector3 res;
        res.x = Mathf.Max(v1.x, v2.x);
        res.y = Mathf.Max(v1.y, v2.y);
        res.z = Mathf.Max(v1.z, v2.z);
        return res;
    }

    public static float CubeDistance(Vector3 v1, Vector3 v2) {
        Vector3Int start = RoundToInt(v1);
        Vector3Int end = RoundToInt(v2);
        return Mathf.Abs(start.x - end.x) + Mathf.Abs(start.y - end.y) + Mathf.Abs(start.z - end.z);
    }

    public static float DistanceLInfini(Vector3 pos1, Vector3 pos2) {
        return Mathf.Max(Mathf.Abs(pos1.x - pos2.x), Mathf.Abs(pos1.y - pos2.y), Mathf.Abs(pos1.z - pos2.z));
    }

    public static Vector3 DistanceLInfiniV3(Vector3 pos1, Vector3 pos2) {
        return new Vector3(Mathf.Abs(pos1.x - pos2.x), Mathf.Abs(pos1.y - pos2.y), Mathf.Abs(pos1.z - pos2.z));
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

    public static float RandArround(float source, float percentage) {
        return UnityEngine.Random.Range(source * (1 - percentage), source * (1 + percentage));
    }

    public static T ChoseOne<T>(List<T> vector) {
        return vector[UnityEngine.Random.Range(0, vector.Count)];
    }

    public static List<Vector3> ChoseSome(List<Vector3> positions, int quantity) {
        if (quantity > positions.Count) {
            return positions;
        }
        return GaussianGenerator.SelecteSomeNumberOf(positions, quantity);
    }

    public static T ChoseOneWeighted<T>(List<T> vector, List<float> weights) {
        Assert.AreEqual(vector.Count, weights.Count, "Les tailles des listes dans ChoiceOneWeighted doivent être égales ! :)");
        float totalWeight = weights.Sum();
        float randomNumber = UnityEngine.Random.Range(0f, 1f) * totalWeight;
        float sum = 0;
        for (int i = 0; i < weights.Count; i++) {
            sum += weights[i];
            if (randomNumber <= sum)
                return vector[i];
        }
        return vector.Last();
    }

    public static bool AreListEqual<T>(List<T> l1, List<T> l2) {
        if (l1.Count != l2.Count)
            return false;
        for (int i = 0; i < l1.Count; i++) {
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

    public static float AABBPointDistance(Vector3 aabbCenter, Vector3 aabbHalfExtents, Vector3 point) {
        return AABBSphereDistance(aabbCenter, aabbHalfExtents, point, 0.0f);
    }

    public static float AABBSphereDistance(Vector3 aabbCenter, Vector3 aabbHalfExtents, Vector3 sphereCenter, float sphereRayon) {
        Vector3 aabbClosestPointToSphere = AABBPoint_ContactPoint(aabbCenter, aabbHalfExtents, sphereCenter);
        return Vector3.Distance(aabbClosestPointToSphere, sphereCenter);
    }

    public static bool AABBSphere(Vector3 aabbCenter, Vector3 aabbHalfExtents, Vector3 sphereCenter, float sphereRayon) {
        return AABBSphereDistance(aabbCenter, aabbHalfExtents, sphereCenter, sphereRayon) <= sphereRayon;
    }

    public static Vector3 AABBPoint_ContactPoint(Vector3 aabbCenter, Vector3 aabbHalfExtents, Vector3 sphereCenter) {
        return new Vector3(
            Mathf.Clamp(sphereCenter.x, aabbCenter.x - aabbHalfExtents.x, aabbCenter.x + aabbHalfExtents.x),
            Mathf.Clamp(sphereCenter.y, aabbCenter.y - aabbHalfExtents.y, aabbCenter.y + aabbHalfExtents.y),
            Mathf.Clamp(sphereCenter.z, aabbCenter.z - aabbHalfExtents.z, aabbCenter.z + aabbHalfExtents.z));
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

    // The capsule is vertical !
    public static bool CapsuleSphere(Vector3 capsuleCenter, float capsuleRadius, float capsuleHeight, Vector3 sphereCenter, float sphereRadius) {
        float distance = CapsuleSphereDistance(capsuleCenter, capsuleRadius, capsuleHeight, sphereCenter, sphereRadius);
        return distance < 0;
    }

    // The capsule is vertical !
    public static float CapsuleSphereDistance(Vector3 capsuleCenter, float capsuleRadius, float capsuleHeight, Vector3 sphereCenter, float sphereRadius) {
        float topOfCylinder = capsuleCenter.y + (capsuleHeight / 2 - capsuleRadius);
        float bottomOfCylinder = capsuleCenter.y - (capsuleHeight / 2 - capsuleRadius);
        Vector3 closestPointOfCylinder = new Vector3(capsuleCenter.x, Mathf.Clamp(sphereCenter.y, bottomOfCylinder, topOfCylinder), capsuleCenter.z);
        float distance = Vector3.Distance(closestPointOfCylinder, sphereCenter) - sphereRadius - capsuleRadius;
        return distance;
    }

    public static bool CapsuleRotatedSphere(Vector3 capsuleCenter, float capsuleRadius, Quaternion capsuleRotation, float capsuleHeight, Vector3 sphereCenter, float sphereRadius) {
        Quaternion alignRotation = Quaternion.Inverse(capsuleRotation);
        Vector3 newCapsuleCenter = alignRotation * capsuleCenter;
        Vector3 newSphereCenter = alignRotation * sphereCenter;
        bool collision = CapsuleSphere(newCapsuleCenter, capsuleRadius, capsuleHeight, newSphereCenter, sphereRadius);
        return collision;
    }

    public static float CapsuleRotatedSphereDistance(Vector3 capsuleCenter, float capsuleRadius, Quaternion capsuleRotation, float capsuleHeight, Vector3 sphereCenter, float sphereRadius) {
        Quaternion alignRotation = Quaternion.Inverse(capsuleRotation);
        Vector3 newCapsuleCenter = alignRotation * capsuleCenter;
        Vector3 newSphereCenter = alignRotation * sphereCenter;
        float distance = CapsuleSphereDistance(newCapsuleCenter, capsuleRadius, capsuleHeight, newSphereCenter, sphereRadius);
        return distance;
    }

    public static bool CapsuleRotatedPoint(Vector3 capsuleCenter, float capsuleRadius, Quaternion capsuleRotation, float capsuleHeight, Vector3 point) {
        return CapsuleRotatedSphere(capsuleCenter, capsuleRadius, capsuleRotation, capsuleHeight, point, 0.0f);
    }

    public static float CapsuleRotatedPointDistance(Vector3 capsuleCenter, float capsuleRadius, Quaternion capsuleRotation, float capsuleHeight, Vector3 point) { return CapsuleRotatedSphereDistance(capsuleCenter, capsuleRadius, capsuleRotation, capsuleHeight, point, 0.0f);
    }

    public static bool CapsulePoint(Vector3 capsuleCenter, float capsuleRadius, float capsuleHeight, Vector3 point) {
        return CapsuleSphere(capsuleCenter, capsuleRadius, capsuleHeight, point, 0.0f);
    }

    public static bool LinePoint(Vector3 linePoint1, Vector3 linePoint2, Vector3 point) {
        Vector3 capsuleCenter = (linePoint1 + linePoint2) / 2;
        float capsuleRadius = 0;
        Quaternion capsuleRotation = Quaternion.FromToRotation(Vector3.up, (linePoint1 - linePoint2).normalized);
        float capsuleHeight = (linePoint1 - linePoint2).magnitude;
        return CapsuleRotatedPoint(capsuleCenter, capsuleRadius, capsuleRotation, capsuleHeight, point);
    }

    public static float LinePointDistance(Vector3 linePoint1, Vector3 linePoint2, Vector3 point) {
        Vector3 capsuleCenter = (linePoint1 + linePoint2) / 2;
        float capsuleRadius = 0;
        Quaternion capsuleRotation = Quaternion.FromToRotation(Vector3.up, (linePoint1 - linePoint2).normalized);
        float capsuleHeight = (linePoint1 - linePoint2).magnitude;
        return CapsuleRotatedPointDistance(capsuleCenter, capsuleRadius, capsuleRotation, capsuleHeight, point);
    }

    // The cylinder is vertical !
    public static bool CylinderSphere(Vector3 cylinderCenter, float cylinderRadius, float cylinderHeight, Vector3 sphereCenter, float sphereRadius) {
        float topOfCylinder = cylinderCenter.y + cylinderHeight;
        float bottomOfCylinder = cylinderCenter.y - cylinderHeight;
        Vector3 closestPointOfCylinderInside = new Vector3(cylinderCenter.x, Mathf.Clamp(sphereCenter.y, bottomOfCylinder, topOfCylinder), cylinderCenter.z);
        Vector3 closestPointOfCylinder = closestPointOfCylinderInside + Vector3.ProjectOnPlane(sphereCenter - cylinderCenter, Vector3.up).normalized * cylinderRadius;
        float distance = Vector3.Distance(closestPointOfCylinder, sphereCenter);
        return distance <= sphereRadius;
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

    public static List<Vector3> orthogonalNormals = new List<Vector3>() {
        Vector3.right,
        Vector3.left,
        Vector3.up,
        Vector3.down,
        Vector3.forward,
        Vector3.back
    };

    public static List<Vector3> diagonalNormals = new List<Vector3>() {
        (Vector3.up + Vector3.left + Vector3.forward).normalized,
        (Vector3.up + Vector3.left + Vector3.back).normalized,
        (Vector3.up + Vector3.right + Vector3.forward).normalized,
        (Vector3.up + Vector3.right + Vector3.back).normalized,
        (Vector3.down + Vector3.left + Vector3.forward).normalized,
        (Vector3.down + Vector3.left + Vector3.back).normalized,
        (Vector3.down + Vector3.right + Vector3.forward).normalized,
        (Vector3.down + Vector3.right + Vector3.back).normalized,
    };

    public static List<Vector3> GetAllOrthogonalNormals() {
        return orthogonalNormals.Select(n => n).ToList();
    }

    public static List<Vector3> GetAllDiagonalNormals() {
        return diagonalNormals.Select(n => n).ToList();
    }

    public static Vector3 SanitizeIfOrthogonal(Vector3 vector) {
        foreach(Vector3 normal in GetAllOrthogonalNormals()) {
            if(AlmostEqual(vector, normal)) {
                return normal;
            }
        }
        return vector;
    }

    public static bool IsOrthogonalRotation(Transform transform) {
        var normales = GetAllOrthogonalNormals();
        return normales.Any(n => AlmostEqual(n, transform.up)) && normales.Any(n => AlmostEqual(n, transform.forward));
    }

    public static Vector3 GetClosestToNormals(Transform t, Vector3 currentNormal) {
        List<Vector3> normals = MathTools.GetAllNormals(t);
        return normals.OrderBy(n => Vector3.Dot(currentNormal, n)).Last();
    }

    public static Vector3 GetClosestToOrthogonalNormals(Vector3 currentNormal) {
        List<Vector3> normals = MathTools.GetAllOrthogonalNormals();
        return normals.OrderBy(n => Vector3.Dot(currentNormal, n)).Last();
    }

    public static bool AlmostEqual(Vector3 v1, Vector3 v2, float epsilon = 0.00001f) {
        return Mathf.Abs(v1.x - v2.x) <= epsilon
            && Mathf.Abs(v1.y - v2.y) <= epsilon
            && Mathf.Abs(v1.z - v2.z) <= epsilon;
    }

    public static Vector3 VecMul(Vector3 vector1, Vector3 vector2) {
        return new Vector3(vector1.x * vector2.x, vector1.y * vector2.y, vector1.z * vector2.z);
    }

    public static Vector3 VecAverage(List<Vector3> vectors) {
        return VecSum(vectors) / vectors.Count;
    }

    public static Vector3 VecSum(List<Vector3> vectors) {
        Vector3 sum = vectors.Aggregate(Vector3.zero, (acc, vec) => acc + vec);
        return sum;
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

    public static bool IsNormalHorizontal(Vector3 normal, GravityManager gravityManager) {
        return MathTools.IsInPlane(normal, Vector3.zero, gravityManager.Up());
    }

    public static Vector3 VecAbs(Vector3 v) {
        return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
    }

    public static bool VecAllLower(Vector3 v1, Vector3 v2) {
        return v1.x <= v2.x && v1.y <= v2.y && v1.z <= v2.z;
    }

    public static bool VecAllStrictLower(Vector3 v1, Vector3 v2) {
        return v1.x < v2.x && v1.y < v2.y && v1.z < v2.z;
    }
    
    public static bool VecAllGreater(Vector3 v1, Vector3 v2) {
        return !VecAllStrictLower(v1, v2);
    }

    public static bool VecAllStrictGreater(Vector3 v1, Vector3 v2) {
        return !VecAllLower(v1, v2);
    }

    public static List<Vector3> RemoveDoublons(List<Vector3> vector) {
        HashSet<Vector3> set = new HashSet<Vector3>(vector);
        return new List<Vector3>(set);
    }

    public static Gradient LerpGradients(Gradient g1, Gradient g2, float avancement) {
        Assert.AreEqual(g1.colorKeys.Length, g2.colorKeys.Length);
        Assert.AreEqual(g1.alphaKeys.Length, g2.alphaKeys.Length);
        GradientColorKey[] colorKeys = new GradientColorKey[g1.colorKeys.Length];
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[g1.alphaKeys.Length];
        for(int i = 0; i < colorKeys.Length; i++) {
            colorKeys[i].color = Color.Lerp(g1.colorKeys[i].color, g2.colorKeys[i].color, avancement);
            colorKeys[i].time = Mathf.Lerp(g1.colorKeys[i].time, g2.colorKeys[i].time, avancement);
        }
        for(int i = 0; i < alphaKeys.Length; i++) {
            alphaKeys[i].alpha = Mathf.Lerp(g1.alphaKeys[i].alpha, g2.alphaKeys[i].alpha, avancement);
            alphaKeys[i].time = Mathf.Lerp(g1.alphaKeys[i].time, g2.alphaKeys[i].time, avancement);
        }
        Gradient res = new Gradient();
        res.SetKeys(colorKeys, alphaKeys);
        return res;
    }

    internal static bool AlmostEqual(Vector3 position, object lastPosition)
    {
        throw new NotImplementedException();
    }
}