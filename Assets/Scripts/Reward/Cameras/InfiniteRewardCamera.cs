using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteRewardCamera : RewardCamera {

    public float playerTimeOffset = 5f;
    public float verticalOffset = 5f;
    public float minVerticalOffset = 0.25f;

    protected RewardTrailThemeDisplayer playerDisplayer;

    public override void Initialize() {
        base.Initialize();
        playerDisplayer = rm.GetPlayerDisplayer();
    }

    public override void Update() {
        Vector3 currentPlayerPosition = GetCurrentPlayerPosition();
        transform.LookAt(currentPlayerPosition, Vector3.up);

        Vector3 playerPositionOffseted = GetPlayerPositionOffseted();
        Vector3 newPosition = (transform.position + playerPositionOffseted) / 2;
        newPosition.x = playerPositionOffseted.x;
        transform.position = newPosition;
    }

    protected Vector3 GetCurrentPlayerPosition() {
        GameObject obj = playerDisplayer.GetObject();
        if(obj != null) {
            return obj.transform.position;
        } else {
            return transform.position + transform.forward;
        }
    }

    protected Vector3 GetPlayerPositionOffseted() {
        Curve curve = playerDisplayer.GetCurve();
        Timer timer = playerDisplayer.GetDurationTimer();
        float acceleration = playerDisplayer.GetAcceleration();
        float rewardPlayerTimeOffset = playerTimeOffset * acceleration * (1 - timer.GetAvancement());
        float offsetedTime = Mathf.Max(0, timer.GetElapsedTime() - rewardPlayerTimeOffset);
        float offsetedAvancement = offsetedTime / timer.GetDuree();
        Vector3 offsetedPosition = curve.GetAvancement(offsetedAvancement);
        float currentVerticalOffset = Mathf.Max(minVerticalOffset, verticalOffset * (1 - timer.GetAvancement()));
        return offsetedPosition + Vector3.up * currentVerticalOffset;
    }
}
