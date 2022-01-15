using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;

public class EventManagerWhileTrue : EventManager {

    public int nbLumieresFinales = 10;
    public float decomposeTime = 1.0f;
    public LocalizedString messageAutoDestructionEnclanchee;
    public List<TimedLocalizedMessage> messagesAChaqueLumiere;

    protected int nbLumieresFinalesAttrappees = 0;
    protected bool isFirstStartEndGame = true;


    protected override void StartEndGame() {
        base.StartEndGame();
        if (isFirstStartEndGame) {
            gm.console.WhileTrueEndEventAutoDestructionEnclanchee(messageAutoDestructionEnclanchee);
        }
    }

    protected override void TryGoToEndPhaseOfTimerManager() {
        // Nothing
    }

    public override void OnLumiereCaptured(Lumiere.LumiereType type) {
        if (type == Lumiere.LumiereType.NORMAL) {
            int nbLumieres = map.GetLumieres().Count;
            if (nbLumieres == 0 && !isEndGameStarted && NoMoreElementsToBeDoneBeforeEndGame()) {
                gm.soundManager.PlayEndGameMusic();
                StartEndGame();
                isFirstStartEndGame = false;
            }
        } else if (type == Lumiere.LumiereType.ALMOST_FINAL)
        {
            nbLumieresFinalesAttrappees++;
            StartCoroutine(CResetEndEvent());
        }
        else if (type == Lumiere.LumiereType.FINAL) {
            nbLumieresFinalesAttrappees++;
            WinGame();
        }
    }

    protected IEnumerator CResetEndEvent() {
        if (coroutineDeathCubesCreation != null) {
            StopCoroutine(coroutineDeathCubesCreation);
        } else {
            StopCoroutine(coroutineCubesDestructions);
        } if (messagesAChaqueLumiere.Count > 0) {
            gm.console.AjouterMessageImportant(messagesAChaqueLumiere[nbLumieresFinalesAttrappees - 1]);
        }
        yield return CRestoreOriginalMap();
        StartEndGame();
    }

    protected virtual IEnumerator CRestoreOriginalMap() {
        int nbCubesToDestroyByFrame = 15;  // Equivalent à 10ms sur mon ordi :)
        deathCubes = deathCubes.FindAll(dc => dc != null); // Quick fix sale ! Il y avait 3 DC sur 1100 qui étaient null dans OrbTriggerFactory quand j'ai fais ça mais je ne sais pas pourquoi !
        deathCubes = deathCubes.OrderBy(dc => Vector3.SqrMagnitude(dc.transform.position - gm.player.transform.position)).ToList();
        int nbCubesDestroyed = 0;
        foreach(Cube cube in deathCubes) {
            //map.DeleteCube(cube);
            cube.Decompose(decomposeTime);
            nbCubesDestroyed++;
            if(nbCubesDestroyed >= nbCubesToDestroyByFrame) {
                nbCubesDestroyed = 0;
                yield return new WaitForSeconds(1f / 60f);
            }
        }
        yield return new WaitForSeconds(decomposeTime);
        deathCubes.Clear();
    }

    protected override Lumiere CreateFinalLight(Lumiere.LumiereType lumiereType = Lumiere.LumiereType.FINAL) {
        if (nbLumieresFinalesAttrappees == nbLumieresFinales - 1) {
            return base.CreateFinalLight(Lumiere.LumiereType.FINAL);
        } else {
            return base.CreateFinalLight(Lumiere.LumiereType.ALMOST_FINAL);
        }
    }

    public override int GetNbLumieresAlmostFinales() {
        return nbLumieresFinales - 1;
    }

    public override int GetNbLumieresAlmostFinalesRestantes() {
        return Mathf.Max(0, GetNbLumieresAlmostFinales() - nbLumieresFinalesAttrappees);
    }
}
