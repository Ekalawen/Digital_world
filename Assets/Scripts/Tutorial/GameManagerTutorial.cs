using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerTutorial : GameManager {

    protected override void Initialize() {
        map.Initialize();
        player.Initialize(new Vector3(5, 5, 5), new Vector2(0, 90));
        eventManager.Initialize();
        console.Initialize();
        //ennemiManager.Initialize();
        soundManager.Initialize();
        postProcessManager.Initialize();
        timerManager.Initialize();
    }
}
