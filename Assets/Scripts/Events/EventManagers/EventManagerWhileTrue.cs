using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class EventManagerWhileTrue : EventManager {

    public int nbLumieresFinales = 10;
    public List<string> messagesAChaqueLumiere;

    protected int nbLumieresFinalesAttrappees = 0;

    public override void OnLumiereCaptured(Lumiere.LumiereType type) {
        if (type == Lumiere.LumiereType.NORMAL) {
            int nbLumieres = map.GetLumieres().Count;
            if (nbLumieres == 0 && !isEndGameStarted) {
                gm.soundManager.PlayEndGameMusic();
                StartEndGame();
            }
        } else if (type == Lumiere.LumiereType.FINAL) {
            nbLumieresFinalesAttrappees++;
            if(nbLumieresFinalesAttrappees == nbLumieresFinales) {
                WinGame();
            } else {
                StartCoroutine(CResetDeathCubes());
            }
        }
    }

    protected IEnumerator CResetDeathCubes() {
        StopCoroutine(coroutineDeathCubesCreation);
        if(messagesAChaqueLumiere.Count > 0)
            gm.console.AjouterMessageImportant(messagesAChaqueLumiere[nbLumieresFinalesAttrappees - 1], Console.TypeText.ALLY_TEXT, 2f);
        yield return CDestroyAllDeathCubes();
        StartEndGame();
    }

    protected IEnumerator CDestroyAllDeathCubes() {
        int nbCubesToDestroyByFrame = 15;  // Equivalent à 10ms sur mon ordi :)
        deathCubes = deathCubes.OrderBy(dc => Vector3.SqrMagnitude(dc.transform.position - gm.player.transform.position)).ToList();
        int nbCubesDestroyed = 0;
        foreach(Cube cube in deathCubes) {
            map.DeleteCube(cube);
            nbCubesDestroyed++;
            if(nbCubesDestroyed >= nbCubesToDestroyByFrame) {
                nbCubesDestroyed = 0;
                yield return new WaitForSeconds(1f / 60f);
            }
        }
        deathCubes.Clear();
    }
}
