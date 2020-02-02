using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManagerWhileTrue : EventManager {

    public int nbLumieresFinales = 10;
    public List<string> messagesAChaqueLumiere;

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
                if(messagesAChaqueLumiere.Count > 0)
                    gm.console.AjouterMessageImportant(messagesAChaqueLumiere[nbLumieresFinalesAttrappees - 1], Console.TypeText.ALLY_TEXT, 2f);
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
