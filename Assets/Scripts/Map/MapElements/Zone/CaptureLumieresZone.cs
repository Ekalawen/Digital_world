using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

// To mark the lumiere as moving :)
public class LumiereIsMoving : MonoBehaviour { }

public class CaptureLumieresZone : IZone {

    public List<Lumiere> lumieres;
    public float captureDuration = 5.0f;
    public AnimationCurve captureCurve;

    protected override void OnEnter(Collider other) {
        foreach(Lumiere lumiere in lumieres) {
            if(lumiere != null && !IsAlreadyMoving(lumiere)) {
                MoveToCapture(lumiere);
            }
        }
    }

    protected bool IsAlreadyMoving(Lumiere lumiere) {
        return lumiere.GetComponent<LumiereIsMoving>() != null;
    }

    protected void MoveToCapture(Lumiere lumiere) {
        StartCoroutine(CMoveToCapture(lumiere));
    }

    protected IEnumerator CMoveToCapture(Lumiere lumiere) {
        LumiereIsMoving isMoving = lumiere.gameObject.AddComponent<LumiereIsMoving>(); // So that we know this lumiere is moving ! :)
        Timer timer = new Timer(captureDuration);
        Vector3 initialPosition = lumiere.transform.position;
        while(lumiere != null && !timer.IsOver()) {
            Vector3 playerPos = gm.player.transform.position;
            float avancement = captureCurve.Evaluate(timer.GetAvancement());
            Vector3 newPos = Vector3.Lerp(initialPosition, playerPos, avancement);
            lumiere.transform.position = newPos;
            yield return null;
        }
        if (lumiere != null) {
            lumiere.transform.position = gm.player.transform.position;
            Destroy(isMoving);
        }
    }

    protected override void OnExit(Collider other) {
        // Nothing to do here
    }
}
