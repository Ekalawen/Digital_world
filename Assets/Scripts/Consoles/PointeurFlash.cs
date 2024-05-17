using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PointeurFlash : MonoBehaviour {

    protected Pointeur pointeur;
    protected Player player;

    public void Initialize(Pointeur pointeur) {
        this.pointeur = pointeur;
        player = GameManager.Instance.player;
        Cooldown cooldown = player.GetDash().GetCooldown();
        if(cooldown) {
            cooldown.onGainCharge.AddListener(nb => Flash());
        }
    }

    public void Flash() {
        Debug.Log($"FLASH ! <3");
    }
}
