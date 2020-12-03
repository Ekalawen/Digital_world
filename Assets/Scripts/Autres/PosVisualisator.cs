using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PosVisualisator : MonoBehaviour {

    public KeyCode keyCode = KeyCode.Mouse2;
    public GameObject visualisatorPrefab;

    protected GameManager gm;
    protected Player.EtatPersonnage etatPlayer = Player.EtatPersonnage.AU_SOL;

    public void Start() {
        gm = GameManager.Instance;
    }

    void FixedUpdate() {
        if(Input.GetKeyDown(keyCode)) {
            CreateObjectAtPlayerPos();
        }

        //if(gm.player.GetEtat() == Player.EtatPersonnage.EN_CHUTE && etatPlayer != Player.EtatPersonnage.EN_CHUTE) {
        //    CreateObjectAtPlayerPos();
        //}
        //etatPlayer = gm.player.GetEtat();
    }

    public void CreateObjectAtPlayerPos()
    {
        GameObject go = Instantiate(visualisatorPrefab, gm.player.transform.position, gm.player.transform.rotation);
        go.transform.localScale = gm.player.transform.localScale;
    }
}
