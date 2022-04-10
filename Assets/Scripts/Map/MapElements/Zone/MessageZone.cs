﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

public class MessageZone : IZone {

    public List<LocalizedString> messages;
    public List<Console.TypeText> typeTextes;
    public MessageZoneBindingParameters parameters;
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
                DisplayMessageImportant(messages[i], typeTextes[i], i);
            }
        }
        if(!isImportant || (isImportant && !isImportantOnly)) {
            if(addGapBeforeText) {
                gm.console.AddGapInConsole();
            }
            for(int i = 0; i < messages.Count; i++) {
                DisplayMessageInConsole(messages[i], typeTextes[i], i);
            }
        }
        if (useSound) {
            gm.soundManager.PlayReceivedMessageClip();
        }
        timer.Reset();
    }

    protected void DisplayMessageImportant(LocalizedString localizedString, Console.TypeText typeTexte, int indice) {
        StartCoroutine(CDisplayMessageImportant(localizedString, typeTexte, indice));
    }

    protected IEnumerator CDisplayMessageImportant(LocalizedString localizedString, Console.TypeText typeTexte, int indice) {
        if (localizedString != null) {
            AsyncOperationHandle<string> handle = GetHandle(localizedString, indice);
            yield return handle;
            string message = handle.Result;
            gm.console.AjouterMessageImportant(message, typeTexte, dureeImportantMessage, bAfficherInConsole: false);
        } else {
            Debug.LogWarning($"{gameObject.name} possède un MessageZone avec un localizedMessage null !", gameObject);
        }
    }

    protected void DisplayMessageInConsole(LocalizedString localizedString, Console.TypeText typeTexte, int indice) {
        StartCoroutine(CDisplayMessageInConsole(localizedString, typeTexte, indice));
    }

    protected IEnumerator CDisplayMessageInConsole(LocalizedString localizedString, Console.TypeText typeTexte, int indice) {
        if (localizedString != null) {
            AsyncOperationHandle<string> handle = GetHandle(localizedString, indice);
            yield return handle;
            string message = handle.Result;
            gm.console.AjouterMessage(message, typeTexte);
        } else {
            Debug.LogWarning($"{gameObject.name} possède un MessageZone avec un localizedMessage null !", gameObject);
        }
    }

    protected virtual AsyncOperationHandle<string> GetHandle(LocalizedString localizedString, int indice) {
        if (parameters != null) {
            return localizedString.GetLocalizedString(parameters.GetParameterFor(indice));
        }
        return localizedString.GetLocalizedString();
    }
}
