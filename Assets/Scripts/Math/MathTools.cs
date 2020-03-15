using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathTools
{

    public static bool IsRounded(Vector3 pos)
    {
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

    public static Vector3Int RoundToInt(Vector3 pos)
    {
        Vector3Int res = new Vector3Int();
        res.x = (int)Mathf.Round(pos.x);
        res.y = (int)Mathf.Round(pos.y);
        res.z = (int)Mathf.Round(pos.z);
        return res;
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

    public static int RandBetween(Vector2Int range) {
        return Random.Range(range.x, range.y + 1);
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
}