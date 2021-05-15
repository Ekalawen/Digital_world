using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GenerateCubesMapFunction : MapFunctionComponent {

    public enum GenerateMode { PROPORTION, COUNT };

    public Cube.CubeType cubeType = Cube.CubeType.NORMAL;

    public override void Initialize() {
        base.Initialize();
        map.SetCurrentCubeType(cubeType);
    }
}
