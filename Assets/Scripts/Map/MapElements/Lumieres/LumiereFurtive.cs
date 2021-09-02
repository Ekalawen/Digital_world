using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LumiereFurtive : Lumiere {
    // Tout le code de déplacement de LumièreFurtive a été ajouté dans FurtiveController

    protected override void CapturedSpecific() {
        base.CapturedSpecific();
        CharacterController controller = GetComponent<CharacterController>();
        controller.enabled = false; // We don't want to collide with the controller once we captured the data ! :)
    }
}
