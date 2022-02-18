using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;

public class AchievementSpecific_NoMoreCubeType : Achievement_FinishLevel {

    public Cube.CubeType cubeType = Cube.CubeType.BRISABLE;

    public override void UnlockSpecific() {
        if (gm.map.GetAllCubesOfType(cubeType).Count == 0) {
            Unlock();
        }
    }
}
