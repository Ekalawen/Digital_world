using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteRewardCamera : RewardCamera {

    public float playerTimeOffset = 5f;
    public float verticalOffsetCoef = 5f;
    public AnimationCurve verticalOffsetCurve;
    public AnimationCurve minVerticalOffsetCurve;
    public AnimationCurve smoothingCurve;

    protected RewardTrailThemeDisplayer playerDisplayer;
    protected float minVerticalOffset;

    public override void Initialize() {
        base.Initialize();
        playerDisplayer = rm.GetPlayerDisplayer();
        minVerticalOffset = minVerticalOffsetCurve.Evaluate(playerDisplayer.GetAcceleration());
        transform.position = playerDisplayer.GetCurve().GetAvancement(0.0f);
    }

    public override void Update() {
        Vector3 currentPlayerPosition = GetCurrentPlayerPosition();
        transform.LookAt(currentPlayerPosition, Vector3.up);

        Vector3 playerPositionOffseted = GetPlayerPositionOffseted();
        transform.position = playerPositionOffseted;
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
        Vector3 timeOffsetedPosition = GetTimeOffsetedPlayerPosition();
        Vector3 verticallyOffsetedPosition = VerticallyOffsetPosition(timeOffsetedPosition);
        Vector3 smoothedPosition = SmoothPosition(verticallyOffsetedPosition);

        Timer timer = playerDisplayer.GetDurationTimer();
        float acceleration = playerDisplayer.GetAcceleration();
        float rewardPlayerTimeOffset = playerTimeOffset * acceleration * Mathf.Clamp01(1 - timer.GetAvancement());
        float currentVerticalOffset = Mathf.Max(minVerticalOffset, verticalOffsetCoef * verticalOffsetCurve.Evaluate(timer.GetAvancement()));
        float smoothingCoef = smoothingCurve.Evaluate(timer.GetAvancement());
        Debug.Log($"timeOffset = {rewardPlayerTimeOffset} verticalOffset = {currentVerticalOffset} smmothingCoef = {smoothingCoef}");

        return smoothedPosition;
    }

    protected Vector3 SmoothPosition(Vector3 position) {
        Timer timer = playerDisplayer.GetDurationTimer();
        float smoothingCoef = smoothingCurve.Evaluate(timer.GetAvancement());
        Vector3 smoothedPosition = transform.position * smoothingCoef + position * (1 - smoothingCoef);
        smoothedPosition.x = position.x;
        return smoothedPosition;
    }

    protected Vector3 VerticallyOffsetPosition(Vector3 position) {
        Timer timer = playerDisplayer.GetDurationTimer();
        float currentVerticalOffset = Mathf.Max(minVerticalOffset, verticalOffsetCoef * verticalOffsetCurve.Evaluate(timer.GetAvancement()));
        Vector3 verticallyOffsetedPosition = position + Vector3.up * currentVerticalOffset;
        return verticallyOffsetedPosition;
    }

    protected Vector3 GetTimeOffsetedPlayerPosition() {
        Timer timer = playerDisplayer.GetDurationTimer();
        Curve curve = playerDisplayer.GetCurve();
        float acceleration = playerDisplayer.GetAcceleration();

        float rewardPlayerTimeOffset = playerTimeOffset * acceleration * Mathf.Clamp01(1 - timer.GetAvancement());
        float offsetedTime = Mathf.Max(0, timer.GetElapsedTime() - rewardPlayerTimeOffset);
        float offsetedAvancement = offsetedTime / timer.GetDuree();

        Vector3 timeOffsetedPosition = curve.GetAvancement(offsetedAvancement);
        return timeOffsetedPosition;
    }
}
