using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PouvoirMessageDisplayer : IPouvoir {

    public TimedLocalizedMessage message;
    public bool afficherInConsole = true;

    protected override bool UsePouvoir() {
        gm.console.AjouterMessageImportant(message, bAfficherInConsole: afficherInConsole);
        return true;
    }
}
