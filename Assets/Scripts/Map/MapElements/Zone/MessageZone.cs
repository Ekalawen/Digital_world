using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageZone : ZoneCubique {

    public string message;
    public Console.TypeText typeTexte;
    public float frequence = 5.0f;
    public bool useSound = false;

    protected Timer timer;
    protected bool isIn = false;

    protected override void Start() {
        base.Start();
        timer = new Timer(frequence);
    }

    private void Update() {
        if(isIn && timer.IsOver()) {
            DisplayMessage();
        }
    }

    protected override void OnEnter(Collider other) {
        if (other.gameObject.tag == "Player") {
            isIn = true;
            DisplayMessage();
        }
    }

    protected override void OnExit(Collider other) {
        isIn = false;
    }

    public void DisplayMessage() {
        gm.console.AjouterMessage(message, typeTexte);
        if(useSound)
            gm.soundManager.PlayReceivedMessageClip();
        timer.Reset();
    }
}
