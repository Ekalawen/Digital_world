using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedMultiplierController : MonoBehaviour {

    [SerializeField]
    protected List<SpeedMultiplier> speedMultipliers;

    protected Character character;

    public void Initialize(Character character) {
        this.character = character;
        speedMultipliers = new List<SpeedMultiplier>();
    }

    public SpeedMultiplier AddMultiplier(SpeedMultiplier speedMultiplier) {
        speedMultiplier.Initialize(this);
        speedMultipliers.Add(speedMultiplier);
        StartCoroutine(CUnregisterAtEnd(speedMultiplier));
        return speedMultiplier;
    }

    protected IEnumerator CUnregisterAtEnd(SpeedMultiplier speedMultiplier) {
        yield return new WaitForSeconds(speedMultiplier.GetTotalDuration());
        RemoveMultiplier(speedMultiplier);
    }

    public void RemoveMultiplier(SpeedMultiplier speedMultiplier) {
        speedMultipliers.Remove(speedMultiplier);
    }

    public void RemoveAllMultiplier() {
        speedMultipliers.Clear();
    }

    public float GetMultiplier() {
        float multiplier = 1.0f;
        foreach(SpeedMultiplier speedMultiplier in speedMultipliers) {
            if(speedMultiplier.GetMultiplier() == float.NaN) {
                Debug.Log("Ici !! (multiplier = Nan !)");
            }
            multiplier += speedMultiplier.GetMultiplier(); // On veut ajouter les vitesses, et pas les multiplier pour éviter des trucs débiles extrêmes :)
        }
        return Mathf.Max(multiplier, 0);
    }

    public List<SpeedMultiplier> GetAllMultipliers() {
        return speedMultipliers;
    }
}
