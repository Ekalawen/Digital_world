using EZCameraShake;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheatCode {
    public List<KeyCode> code;
    public Action action;
    public int state = 0;
}

public class CheatCodeManager : MonoBehaviour {

    public List<KeyCode> winCode;
    public List<KeyCode> plus10Code;

    protected GameManager gm;
    protected List<CheatCode> cheatCodes;

    public void Initialize() {
        gm = GameManager.Instance;
        cheatCodes = new List<CheatCode>();

        CheatCode winCheatCode = new CheatCode();
        winCheatCode.code = winCode;
        winCheatCode.action = gm.eventManager.WinGame;
        cheatCodes.Add(winCheatCode);

        if (gm.GetMapType() == MenuLevel.LevelType.INFINITE) {
            CheatCode plus10BlocksCheatCode = new CheatCode();
            plus10BlocksCheatCode.code = plus10Code;
            InfiniteMap infiniteMap = (InfiniteMap)gm.map;
            plus10BlocksCheatCode.action = infiniteMap.Add10BlockRun;
            cheatCodes.Add(plus10BlocksCheatCode);
        } else {
            CheatCode plus10SecondesCheatCode = new CheatCode();
            plus10SecondesCheatCode.code = plus10Code;
            plus10SecondesCheatCode.action = gm.timerManager.Add10Time;
            cheatCodes.Add(plus10SecondesCheatCode);
        }
    }

    public void Update() {
        foreach(CheatCode cheatCode in cheatCodes) {
            KeyCode currentKey = cheatCode.code[cheatCode.state];
            if(Input.GetKeyDown(currentKey)) {
                cheatCode.state += 1;
                if(cheatCode.state == cheatCode.code.Count) {
                    cheatCode.state = 0;
                    cheatCode.action.Invoke();
                }
            } else if (Input.anyKeyDown) {
                cheatCode.state = 0;
            }
        }
    }
}
