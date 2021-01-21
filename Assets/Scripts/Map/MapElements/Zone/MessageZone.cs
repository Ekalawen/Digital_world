﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageZone : IZone {

    public List<string> messages;
    public List<Console.TypeText> typeTextes;
    public bool isImportant = false;
    [ConditionalHide("isImportant")]
    public float dureeImportantMessage = 2.0f;
    [ConditionalHide("isImportant")]
    public bool isImportantOnly = false;
    public float frequence = 5.0f;
    public bool useSound = false;
    public bool addGapBeforeText = false;
    public bool cleanOtherImportantTexts = false;

    protected Timer timer;
    protected bool isIn = false;

    protected override void Start() {
        base.Start();
        timer = new Timer(frequence);
        timer.SetOver();
    }

    private void Update() {
        if(isIn && timer.IsOver()) {
            DisplayMessages();
        }
    }

    protected override void OnEnter(Collider other) {
        if (other.gameObject.tag == "Player") {
            isIn = true;
        }
    }

    protected override void OnExit(Collider other) {
        if (other.gameObject.tag == "Player") {
            isIn = false;
        }
    }

    public virtual void DisplayMessages() {
        if(cleanOtherImportantTexts) {
            gm.console.CleanAllImportantTexts();
        }
        if(isImportant) {
            for(int i = messages.Count - 1; i >= 0; i--) {
                DisplayMessageImportant(messages[i], typeTextes[i]);
            }
        }
        if(!isImportant || (isImportant && !isImportantOnly)) {
            if(addGapBeforeText) {
                gm.console.AddGapInConsole();
            }
            for(int i = 0; i < messages.Count; i++) {
                DisplayMessageInConsole(messages[i], typeTextes[i]);
            }
        }
        if (useSound) {
            gm.soundManager.PlayReceivedMessageClip();
        }
        timer.Reset();
    }

    protected void DisplayMessageImportant(string message, Console.TypeText typeTexte) {
        gm.console.AjouterMessageImportant(message, typeTexte, dureeImportantMessage, bAfficherInConsole: false);
    }

    protected void DisplayMessageInConsole(string message, Console.TypeText typeTexte) {
        gm.console.AjouterMessage(message, typeTexte);
    }
}
