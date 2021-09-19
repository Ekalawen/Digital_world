using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DataSondeManager : MonoBehaviour {

    public enum NextMethod { NEAR, FAR };

    public NextMethod nextMethod = NextMethod.NEAR;

    protected GameManager gm;
    protected List<OrbTrigger> dataSondesTriggers;

    public void Initialize() {
        gm = GameManager.Instance;
        List<RegisterToDataSoundManager> toRegister = FindObjectsOfType<RegisterToDataSoundManager>().ToList();
        dataSondesTriggers = toRegister.Select(r => r.orbTrigger).ToList();
        ActivateOnlyOneDataSonde();
    }

    public void ActivateOnlyOneDataSonde() {
        if(dataSondesTriggers.Count == 0) {
            return;
        }

        foreach(OrbTrigger orbTrigger in dataSondesTriggers) {
            orbTrigger.Resize(orbTrigger.transform.position, Vector3.zero);
        }
        OrbTrigger chosenOne = MathTools.ChoiceOne(dataSondesTriggers);
        chosenOne.Initialize(chosenOne.rayon, chosenOne.durationToActivate);
    }

    public OrbTrigger ActivateNextDataSonde(Vector3 origin) {
        if(dataSondesTriggers.Count == 0) {
            return null;
        }

        OrbTrigger chosenOne = GetNextOrbTrigger(origin);
        chosenOne.Initialize(chosenOne.rayon, chosenOne.durationToActivate);
        return chosenOne;
    }

    protected OrbTrigger GetNextOrbTrigger(Vector3 origin) {
        if(nextMethod == NextMethod.NEAR) {
            return dataSondesTriggers.OrderBy(ds => Vector3.Distance(ds.transform.position, origin)).First();
        } else {
            return dataSondesTriggers.OrderBy(ds => Vector3.Distance(ds.transform.position, origin)).Last();
        }
    }

    public void UnregisterDataSondeTrigger(OrbTrigger orbTrigger) {
        dataSondesTriggers.Remove(orbTrigger);
    }
}
