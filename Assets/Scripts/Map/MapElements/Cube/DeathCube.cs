using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathCube : Cube {

    public bool shouldRegisterToColorSources = true;

    protected override void Start() {
        base.Start();
        GameManager gm = GameManager.Instance;
        Vector3 playerPos = gm.player.transform.position;
        if(gm.player.DoubleCheckInteractWithCube(this)) {
            gm.eventManager.LoseGame(EventManager.DeathReason.TOUCHED_DEATH_CUBE);
        }
        if(shouldRegisterToColorSources)
            RegisterCubeToColorSources();
    }

    //public override void RegisterCubeToColorSources() {
    //    base.RegisterCubeToColorSources();
    //    //Color color = GetColor();
    //    //color = color - color;
    //    //Color newColor = new Color(1.0f - color.r, 1.0f - color.g, 1.0f - color.b);
    //    //SetColor(newColor);
    //    SetColor(Color.white - GetColor());
    //}

    //public override void AddColor(Color addedColor) {
    //}

    //public override void SetColor(Color newColor) {
    //}

    public override void InteractWithPlayer() {
        Debug.Log("Looooooooooooooooose ! :'(");
        gm.eventManager.LoseGame(EventManager.DeathReason.TOUCHED_DEATH_CUBE);
    }

    //public void Update() {
    //    //GetComponent<MeshRenderer>().material.GetTexture("Albedo").
    //    GetComponent<MeshRenderer>().material.color = Color.cyan;
    //}
}
