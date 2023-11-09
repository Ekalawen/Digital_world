using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockTriggerZone : IZone {

    public Block block;

    protected InfiniteMap map;
    protected Coroutine coroutine = null;
    protected bool hasPressedShift = false;

    protected override void Start() {
        base.Start();
        map = (InfiniteMap)gm.map;
    }

    protected override void OnEnter(Collider other) {
        if (block != null && map != null && other.GetComponent<Player>() != null) {
            map.OnEnterBlock(block);
            StartRememberShiftPressed();
            block.onEnterBlock.Invoke(block);
        }
    }

    protected override void OnExit(Collider other) {
        if (block != null && map != null) {
            map.OnExitBlock(block);
            NotifyMapIfShiftNotPressed();
        }
    }

    protected void StartRememberShiftPressed() {
        coroutine = StartCoroutine(CStartRememberShiftPressed());
    }
    protected IEnumerator CStartRememberShiftPressed() {
        if (block.shouldPlayerPressShift) {
            hasPressedShift = false;
            while (true) {
                if (InputManager.Instance.GetShift()) {
                    hasPressedShift = true;
                    break;
                }
                yield return null;
            }
        }
    }

    protected void NotifyMapIfShiftNotPressed() {
        if (!block.shouldPlayerPressShift)
            return;
        if (!hasPressedShift)
            map.PlayerForgotToPressShift(block);
        hasPressedShift = false;
        if (coroutine != null)
            StopCoroutine(coroutine);
    }
}
