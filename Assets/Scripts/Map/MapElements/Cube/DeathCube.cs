using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathCube : Cube {

    public float minColorSaturationAndValue = 0.1f;

    protected override void Start() {
        base.Start();
        GameManager gm = GameManager.Instance;
        Vector3 playerPos = gm.player.transform.position;
        if(gm.player.DoubleCheckInteractWithCube(this)) {
            gm.eventManager.LoseGame(EventManager.DeathReason.TOUCHED_DEATH_CUBE);
        }
    }

    public override void AddColor(Color addedColor) {
        Color color = ColorSource.LimiteColorSaturation(GetColor() + addedColor, minColorSaturationAndValue);
        GetComponent<MeshRenderer>().material.color = color;
    }

    public override void SetColor(Color newColor) {
        Color color = ColorSource.LimiteColorSaturation(newColor, minColorSaturationAndValue);
        GetComponent<MeshRenderer>().material.color = color;
    }

    public override void InteractWithPlayer() {
        Debug.Log("Looooooooooooooooose ! :'(");
        gm.eventManager.LoseGame(EventManager.DeathReason.TOUCHED_DEATH_CUBE);
    }
}
