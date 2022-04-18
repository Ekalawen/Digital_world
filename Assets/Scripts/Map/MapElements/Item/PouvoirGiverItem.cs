using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

public class PouvoirGiverItem : Item {

    public enum PouvoirBinding { A, E, LEFT_CLICK, RIGHT_CLICK };

    public GameObject pouvoirPrefab;
    public PouvoirBinding pouvoirBinding;
    public bool printDefaultMessage = true;
    public bool printCustomMessage = false;
    [ConditionalHide("printCustomMessage")]
    public TimedLocalizedMessage customMessage;

    public override void OnTrigger(Collider hit)
    {
        PrintCustomMessage();
        GivePouvoir(gm, pouvoirPrefab, pouvoirBinding, printDefaultMessage);
    }

    protected void PrintCustomMessage() {
        if (!printCustomMessage) {
            return;
        }
        MessageZoneBindingParameters bindingParameters = GetComponent<MessageZoneBindingParameters>();
        if (bindingParameters == null) {
            gm.console.AjouterMessageImportant(customMessage);
            return;
        }
        string bindingArgument = InputManager.Instance.GetCurrentInputController().GetStringForBinding(bindingParameters.bindingParameter[0]);
        LocalizedString ls = customMessage.localizedString;
        ls.Arguments = new object[] { bindingArgument };
        gm.console.AjouterMessageImportant(ls, customMessage.type, customMessage.duree);
    }

    public static void GivePouvoir(GameManager gm, GameObject pouvoirPrefab, PouvoirBinding pouvoirBinding, bool printDefaultMessage = true) {
        gm.player.SetPouvoir(pouvoirPrefab, pouvoirBinding);
        if(pouvoirPrefab != null) {
            IPouvoir pouvoir = pouvoirPrefab.GetComponent<IPouvoir>();
            if (printDefaultMessage) {
                gm.console.CapturePouvoirGiverItem(pouvoir.nom, pouvoirBinding);
            }
            gm.soundManager.PlayGainPouvoirClip();
            gm.pointeur.Initialize();
        }
    }
}
