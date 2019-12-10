using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManagerWhileTrue : EventManager {

    public int nbLumieresFinales = 10;

    protected int nbLumieresFinalesAttrappees = 0;

    public override void OnLumiereCaptured(Lumiere.LumiereType type) {
        if (type == Lumiere.LumiereType.NORMAL) {
            int nbLumieres = map.lumieres.Count;
            if (nbLumieres == 0 && !isEndGameStarted) {
                gm.soundManager.PlayEndGameMusic();
                StartEndGame();
            }
        } else if (type == Lumiere.LumiereType.FINAL) {
            nbLumieresFinalesAttrappees++;
            if(nbLumieresFinalesAttrappees == nbLumieresFinales) {
                WinGame();
            } else {
                StopCoroutine(coroutineDeathCubesCreation);
                DestroyAllDeathCubes();
                StartEndGame();
            }
        }
    }

    protected void DestroyAllDeathCubes() {
        foreach(Cube cube in deathCubes) {
            map.DeleteCube(cube, bJustInactive: true);
        }
        deathCubes.Clear();
    }
}
