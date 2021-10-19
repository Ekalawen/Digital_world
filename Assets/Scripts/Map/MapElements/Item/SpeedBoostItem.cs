using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class SpeedBoostItem : Item {
    public override void OnTrigger(Collider hit) {
        Debug.Log($"Boost de vitesse on !");
    }
}
