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
    public List<KeyCode> loseCode;
    public List<KeyCode> plus10Code;
    public List<KeyCode> plus100Code;
    public List<KeyCode> minus10Code;
    public List<KeyCode> minus100Code;
    public List<KeyCode> plus10DataCount;
    public List<KeyCode> plus100DataCount;

    protected GameManager gm;
    protected List<CheatCode> cheatCodes;

    public void Initialize() {
        gm = GameManager.Instance;
        cheatCodes = new List<CheatCode>();

        CheatCode winCheatCode = new CheatCode();
        winCheatCode.code = winCode;
        winCheatCode.action = gm.eventManager.WinGame;
        cheatCodes.Add(winCheatCode);

        CheatCode loseCheatCode = new CheatCode();
        loseCheatCode.code = loseCode;
        loseCheatCode.action = gm.eventManager.LoseGameWithTimeOut;
        cheatCodes.Add(loseCheatCode);

        // Plus 10
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

        // Plus 100
        if (gm.GetMapType() == MenuLevel.LevelType.INFINITE) {
            CheatCode plus100BlocksCheatCode = new CheatCode();
            plus100BlocksCheatCode.code = plus100Code;
            InfiniteMap infiniteMap = (InfiniteMap)gm.map;
            plus100BlocksCheatCode.action = infiniteMap.Add100BlockRun;
            cheatCodes.Add(plus100BlocksCheatCode);
        } else {
            CheatCode plus100SecondesCheatCode = new CheatCode();
            plus100SecondesCheatCode.code = plus100Code;
            plus100SecondesCheatCode.action = gm.timerManager.Add100Time;
            cheatCodes.Add(plus100SecondesCheatCode);
        }

        // Minus 10
        if (gm.GetMapType() == MenuLevel.LevelType.REGULAR) {
            CheatCode minus10SecondesCheatCode = new CheatCode();
            minus10SecondesCheatCode.code = minus10Code;
            minus10SecondesCheatCode.action = gm.timerManager.Minus10Time;
            cheatCodes.Add(minus10SecondesCheatCode);
        }

        // Minus 100
        if (gm.GetMapType() == MenuLevel.LevelType.REGULAR) {
            CheatCode minus100SecondesCheatCode = new CheatCode();
            minus100SecondesCheatCode.code = minus100Code;
            minus100SecondesCheatCode.action = gm.timerManager.Minus100Time;
            cheatCodes.Add(minus100SecondesCheatCode);
        }

        // Plus 10 DataCount
        if (gm.GetMapType() == MenuLevel.LevelType.REGULAR) {
            CheatCode plus10DataCountCheatCode = new CheatCode();
            plus10DataCountCheatCode.code = plus10DataCount;
            plus10DataCountCheatCode.action = gm.eventManager.Add10DataCount;
            cheatCodes.Add(plus10DataCountCheatCode);
        }

        // Plus 100 DataCount
        if (gm.GetMapType() == MenuLevel.LevelType.REGULAR) {
            CheatCode plus100DataCountCheatCode = new CheatCode();
            plus100DataCountCheatCode.code = plus100DataCount;
            plus100DataCountCheatCode.action = gm.eventManager.Add100DataCount;
            cheatCodes.Add(plus100DataCountCheatCode);
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
