using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathCube : Cube {

    public bool shouldRegisterToColorSources = true;
    public float minColorSaturationAndValue = 0.1f;

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

    public override void AddColor(Color addedColor) {
        Color color = ThresholdColorSaturation(GetColor() + addedColor);
        if(color != GetColor() + addedColor)
            Debug.LogFormat("oldColor = {0} newColor = {1}", GetColor() + addedColor, color);
        GetComponent<MeshRenderer>().material.color = color;
    }

    public override void SetColor(Color newColor) {
        Color color = ThresholdColorSaturation(newColor);
        GetComponent<MeshRenderer>().material.color = color;
    }

    protected Color ThresholdColorSaturation(Color color) {
        float mean = (color.r + color.g + color.b) / 3.0f;
        if(mean < minColorSaturationAndValue) {
            float ecart = minColorSaturationAndValue - mean;
            return new Color(color.r + ecart, color.g + ecart, color.b + ecart, color.a);
        }
        return color;
    }

    public override void InteractWithPlayer() {
        Debug.Log("Looooooooooooooooose ! :'(");
        gm.eventManager.LoseGame(EventManager.DeathReason.TOUCHED_DEATH_CUBE);
    }

    //public void Update() {
    //    //GetComponent<MeshRenderer>().material.GetTexture("Albedo").
    //    GetComponent<MeshRenderer>().material.color = Color.cyan;
    //}
}
