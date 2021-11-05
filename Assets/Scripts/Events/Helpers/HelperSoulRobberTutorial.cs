using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class HelperSoulRobberTutorial : MonoBehaviour {

    public HelperMessage letSoulRobberTouchYou;

    protected GameManager gm;
    protected bool hasLetSoulRobberTouchYou = false;

    public void Start() {
        gm = GameManager.Instance;
        SoulRobber soulRobber = gm.ennemiManager.GetEnnemisOfType<SoulRobber>().First();
        SoulRobberController soulRobberController = soulRobber.GetComponent<SoulRobberController>();
        soulRobberController.startEscapingEvents.AddListener(new UnityAction(RegisterSoulRobberHadTouchYou));
    }

    public void Update() {
        if (!hasLetSoulRobberTouchYou && gm.timerManager.GetElapsedTime() >= letSoulRobberTouchYou.GetTiming()) {
            letSoulRobberTouchYou.DisplayMessage();
        }
    }

    public void RegisterSoulRobberHadTouchYou() {
        hasLetSoulRobberTouchYou = true;
    }
}
