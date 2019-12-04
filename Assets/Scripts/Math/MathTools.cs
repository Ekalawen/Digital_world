using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathTools {

    public static bool IsRounded(Vector3 pos) {
        return Mathf.Round(pos.x) == pos.x && Mathf.Round(pos.y) == pos.y && Mathf.Round(pos.z) == pos.z;
    }

}
