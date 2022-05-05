using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CaptureLumieresZone : IZone {

    public List<Lumiere> lumieres;
    public float captureDuration = 5.0f;
    public float curveRadius = 5.0f;
    public AnimationCurve captureCurve;

    protected override void OnEnter(Collider other) {
        foreach(Lumiere lumiere in lumieres) {
            if(lumiere != null && !IsAlreadyMoving(lumiere)) {
                MoveToCapture(lumiere);
            }
        }
    }

    protected bool IsAlreadyMoving(Lumiere lumiere) {
        return lumiere.gameObject.GetComponent<LumiereIsMoving>() != null;
    }

    protected void MoveToCapture(Lumiere lumiere) {
        StartCoroutine(CMoveToCapture(lumiere));
    }

    protected IEnumerator CMoveToCapture(Lumiere lumiere)
    {
        LumiereIsMoving isMoving = lumiere.gameObject.AddComponent<LumiereIsMoving>(); // So that we know this lumiere is moving ! :)
        Timer timer = new Timer(captureDuration);
        Vector3 initialPosition = lumiere.transform.position;

        while (lumiere != null && !timer.IsOver()) {
            float avancement = captureCurve.Evaluate(timer.GetAvancement());
            Vector3 playerPos = gm.player.transform.position;
            Vector3 playerPosTop = playerPos + Vector3.up * curveRadius;
            Vector3 playerPosForward = playerPos + Vector3.up * curveRadius / 2 + Vector3.right * curveRadius;
            Vector3 newPos = BezierCurve.GetCubicCurvePoint(initialPosition, playerPosTop, playerPosForward, playerPos, avancement);
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
