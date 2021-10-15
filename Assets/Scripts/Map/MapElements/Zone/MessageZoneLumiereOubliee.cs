using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

public class MessageZoneLumiereOubliee : MessageZone {

    public int nbLumieresToHave;

    public override void DisplayMessages() {
        if (GetNbLumieresOubliees() > 0) {
            base.DisplayMessages();
        }
    }

    protected override AsyncOperationHandle<string> GetHandle(LocalizedString localizedString) {
        int nbLumieresOubliees = GetNbLumieresOubliees();
        return localizedString.GetLocalizedString(new object[] { nbLumieresOubliees });
    }

    protected int GetNbLumieresCapturees() {
        return gm.map.nbLumieresInitial - gm.map.GetLumieres().Count;
    }

    protected int GetNbLumieresOubliees() {
        return nbLumieresToHave - GetNbLumieresCapturees();
    }
}
