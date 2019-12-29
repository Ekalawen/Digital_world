using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityEvent : RandomEvent {

    public GravityManager.Direction direction;
    public bool bUseCurrentGravity = false;
    public bool bUseRandomDirection = false;
    public Vector2 intensite;

    protected GravityManager.Direction oldDirection;
    protected GravityManager.Direction newDir;
    protected float newIntensity;
    protected float oldIntensity;

    protected override void Start() {
        base.Start();
    }

    public override void StartEvent() {
        oldDirection = gm.gravityManager.gravityDirection;
        oldIntensity = gm.gravityManager.gravityIntensity;
        if(bUseRandomDirection)
            direction = GravityManager.GetRandomDirection(gm.gravityManager.gravityDirection);
        newDir = bUseCurrentGravity ? oldDirection : direction;
        newIntensity = Random.Range(intensite.x, intensite.y);
        gm.gravityManager.SetGravity(newDir, newIntensity);
    }

    public override void EndEvent() {
        gm.gravityManager.SetGravity(oldDirection, oldIntensity);
    }

    public override void StartEventConsoleMessage() {
        gm.console.GravityEventMessage(newDir, newIntensity);
    }
}
