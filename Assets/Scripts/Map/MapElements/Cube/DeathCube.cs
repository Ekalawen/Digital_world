using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathCube : Cube {

    protected override void Start() {
        base.Start();
        GameManager gm = GameManager.Instance;
        Vector3 playerPos = gm.player.transform.position;
        if(Vector3.Distance(playerPos, transform.position) <= Mathf.Sqrt(2)) {
            gm.eventManager.LoseGame(EventManager.DeathReason.TOUCHED_DEATH_CUBE);
        }
    }

    public override void RegisterCubeToColorSources() {
    }

    public override void AddColor(Color addedColor) {
    }

    public override void SetColor(Color newColor) {
    }
}
