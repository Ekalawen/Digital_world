using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageZone : IZone {

    public string message;
    public Console.TypeText typeTexte;
    public bool isImportant = false;
    public bool isImportantOnly = false;
    public float frequence = 5.0f;
    public bool useSound = false;

    protected Timer timer;
    protected bool isIn = false;

    protected override void Start() {
        base.Start();
        timer = new Timer(frequence);
        timer.SetOver();
    }

    private void Update() {
        if(isIn && timer.IsOver()) {
            DisplayMessage();
        }
    }

    protected override void OnEnter(Collider other) {
        if (other.gameObject.tag == "Player") {
            isIn = true;
        }
    }

    protected override void OnExit(Collider other) {
        isIn = false;
    }

    public void DisplayMessage() {
        if(isImportant)
            gm.console.AjouterMessageImportant(message, typeTexte, 2.0f, !isImportantOnly);
        else
            gm.console.AjouterMessage(message, typeTexte);
        if(useSound)
            gm.soundManager.PlayReceivedMessageClip();
        timer.Reset();
    }
}
