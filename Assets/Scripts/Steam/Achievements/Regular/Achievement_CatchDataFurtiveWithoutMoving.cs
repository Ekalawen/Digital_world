using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;

public class Achievement_CatchDataFurtiveWithoutMoving : Achievement {

    [Header("Parameters")]
    public float dontMoveDuration = 1.0f;
    public float distanceMaxToDontMove = 0.1f;

    protected override void InitializeSpecific() {
        gm.eventManager.onCaptureLumiere.AddListener(OnCaptureLumiere);
    }

    public void OnCaptureLumiere(Lumiere lumiere) {
        if(lumiere.GetComponent<FurtiveController>() != null) {
            Vector3 playerPos = gm.player.transform.position;
            float currentTime = GameManager.Instance.timerManager.GetRealElapsedTime();
            List<TimedVector3> playerPositionsHistory = gm.historyManager.GetPlayerHistory().positions;
            List<TimedVector3> oldPlayerPositionsHistory = playerPositionsHistory.FindAll(tp => tp.time < currentTime - dontMoveDuration);
            if(oldPlayerPositionsHistory.Count == 0) {
                return;
            }
            Vector3 oldPlayerPos = oldPlayerPositionsHistory.Last().position;
            if(Vector3.Distance(oldPlayerPos, playerPos) <= distanceMaxToDontMove) {
                Unlock();
            }
        }
    }
}
