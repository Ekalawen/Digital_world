using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PointeurFlash : MonoBehaviour {

    public Animator animator;

    protected Pointeur pointeur;
    protected Player player;

    public void Initialize(Pointeur pointeur) {
        this.pointeur = pointeur;
        player = GameManager.Instance.player;
        LinkToCooldown();
    }

    private void LinkToCooldown() {
        if(!SkillTreeManager.Instance.IsEnabled(SkillKey.DASH_CURSOR_UI)) {
            return;
        }
        Cooldown cooldown = player.GetDash().GetCooldown();
        if (cooldown) {
            cooldown.onGainCharge.AddListener(nb => Flash());
        }
    }

    public void Flash() {
        animator.SetTrigger("Flash");
    }
}
