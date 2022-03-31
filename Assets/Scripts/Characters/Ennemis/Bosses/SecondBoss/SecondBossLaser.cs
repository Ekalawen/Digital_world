using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecondBossLaser : MonoBehaviour {

    public LineRenderer lineRenderer;
    public float durationBetweenHits = 0.5f;
    public float bossSizeOffsetCoef = 1.0f;

    protected GameManager gm;
    protected SecondBoss secondBoss;
    protected Vector3 direction;
    protected bool isInitialized = false;
    protected Timer durationBetweenHitsTimer;

    public void Initialize(SecondBoss secondBoss, Vector3 direction) {
        gm = GameManager.Instance;
        this.secondBoss = secondBoss;
        this.direction = direction;
        lineRenderer.SetPosition(1, Vector3.forward * secondBoss.laserLenght);
        lineRenderer.widthMultiplier = secondBoss.laserWidth;
        durationBetweenHitsTimer = new Timer(durationBetweenHits, setOver: true);
        StartCoroutine(CSetInitializedNextFrame());
    }

    protected IEnumerator CSetInitializedNextFrame() {
        yield return new WaitForEndOfFrame();
        isInitialized = true;
    }

    public void Update() {
        UpdatePositionAccordingToSecondBoss();
    }

    protected virtual void UpdatePositionAccordingToSecondBoss() {
        // This is needed because triggers can't be the child of a CharacterController or they will block him ... :'(
        transform.position = ComputePosition();
        transform.rotation = ComputeRotation();
    }

    protected virtual Quaternion ComputeRotation() {
        return secondBoss.transform.rotation * Quaternion.LookRotation(direction, gm.gravityManager.Up());
    }

    protected virtual Vector3 ComputePosition() {
        return secondBoss.transform.position + direction * (secondBoss.GetHalfSize() * bossSizeOffsetCoef + secondBoss.laserOffset);
    }

    public Vector3 GetLaserCenter() {
        return transform.position + direction * secondBoss.laserLenght;
    }

    //protected void OnTriggerEnter(Collider other) {
    protected void OnTriggerStay(Collider other) {
        if (!isInitialized) {
            return;
        }
        HitPlayerOnContact(other);
        ExplodCubesOnContact(other);
    }

    protected void HitPlayerOnContact(Collider other) {
        if (other.gameObject.GetComponent<Player>() != null
         && durationBetweenHitsTimer.IsOver()
         && !gm.player.IsTimeHackOn()
         && !gm.eventManager.IsGameOver()) {
            secondBoss.HitPlayerWithLaser();
            durationBetweenHitsTimer.Reset();
        }
    }

    protected void ExplodCubesOnContact(Collider other) {
        Cube cube = other.gameObject.GetComponent<Cube>();
        if (cube != null) {
            cube.Explode();
        }
    }
}
