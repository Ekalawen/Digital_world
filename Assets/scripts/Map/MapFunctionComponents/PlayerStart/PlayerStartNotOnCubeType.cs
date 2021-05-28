using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStartNotOnCubeType : PlayerStartComponent {

    public Cube.CubeType cubeType = Cube.CubeType.DEATH;

    public override Vector3 GetRawPlayerStartPosition() {
        int kmax = 1000;
        for(int k = 0; k < kmax; k++) {
            Vector3 pos = map.GetFreeRoundedLocation();
            if (IsNotOverCubeType(pos)) {
                return pos;
            }
        }
        Debug.LogWarning($"On a pas trouvé de position pas au-dessus d'un Cube {cubeType} ! :'(");
        return map.GetFreeRoundedLocation();
    }

    public bool IsNotOverCubeType(Vector3 pos) {
        pos = MathTools.Round(pos);
        for(int i = 1; i <= pos.y; i++) {
            Cube cube = map.GetCubeAt(pos + i * Vector3.down);
            if(cube != null) {
                return cube.type != cubeType;
            }
        }
        return true; // Au-dessus du vide :D
    }
}
