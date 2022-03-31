using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecondBossTargetingLaser : SecondBossLaser {

    [Header("Targeting")]
    public Vector2 rotationSpeedRange = new Vector2(15f, 45f);
    public Vector2 rotationSpeedAngleTresholds = new Vector2(30f, 105f);
    public float spawningDelay = 1.0f;

    public void Initialize(SecondBoss secondBoss) {
        Initialize(secondBoss, Vector3.zero);
        direction = ComputeFirstDirection();
    }

    protected override void UpdatePositionAccordingToSecondBoss() {
        direction = ComputeDirection();
        if (!gm.eventManager.IsGameWin()) {
            base.UpdatePositionAccordingToSecondBoss();
        }
    }

    protected Vector3 ComputeFirstDirection() {
        Vector3 oldPlayerPosition = gm.historyManager.GetPlayerHistory().GetPositionTimesAgo(spawningDelay);
        return (oldPlayerPosition - secondBoss.transform.position).normalized;
    }

    protected Vector3 ComputeDirection() {
        Vector3 playerPos = gm.player.transform.position;
        Vector3 currentDirection = direction;
        Vector3 wantedDirection = (playerPos - secondBoss.transform.position).normalized;
        Vector3 axis = Vector3.Cross(currentDirection, wantedDirection);
        if(axis == Vector3.zero) { // Because currentDirection && wantedDirection are parallal ! :)
            axis = gm.gravityManager.Up();
        }
        float currentAngle = Vector3.Angle(currentDirection, wantedDirection);
        float rotationSpeed = MathCurves.Remap(currentAngle, rotationSpeedAngleTresholds, rotationSpeedRange);
        float angle = Mathf.Min(rotationSpeed * Time.deltaTime, currentAngle);
        Vector3 newDirection = Quaternion.AngleAxis(angle, axis) * currentDirection;
        return newDirection.normalized;
    }

    protected override Quaternion ComputeRotation() {
        return Quaternion.LookRotation(direction, gm.gravityManager.Up());
    }

    protected override Vector3 ComputePosition() {
        return secondBoss.transform.position + direction * (secondBoss.GetHalfSize() * bossSizeOffsetCoef + secondBoss.laserOffset);
    }
}
