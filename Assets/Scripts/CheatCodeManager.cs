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
    public List<KeyCode> plus1000Code;
    public List<KeyCode> minus10Code;
    public List<KeyCode> minus100Code;
    public List<KeyCode> minus1000Code;
    public List<KeyCode> plus10DataCount;
    public List<KeyCode> plus100DataCount;
    public List<KeyCode> startEndEventCode;
    public List<KeyCode> gravityZeroCode;
    public List<KeyCode> cooldownsZeroCode;
    public List<KeyCode> invincibilityCode;
    public List<KeyCode> hideConsoleCode;
    public List<KeyCode> disableEnnemisCode;

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

        // Plus 1000
        if (gm.GetMapType() == MenuLevel.LevelType.REGULAR) {
            CheatCode plus1000SecondesCheatCode = new CheatCode();
            plus1000SecondesCheatCode.code = plus1000Code;
            plus1000SecondesCheatCode.action = gm.timerManager.Add1000Time;
            cheatCodes.Add(plus1000SecondesCheatCode);
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

        // Minus 1000
        if (gm.GetMapType() == MenuLevel.LevelType.REGULAR) {
            CheatCode minus1000SecondesCheatCode = new CheatCode();
            minus1000SecondesCheatCode.code = minus1000Code;
            minus1000SecondesCheatCode.action = gm.timerManager.Minus1000Time;
            cheatCodes.Add(minus1000SecondesCheatCode);
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

        // Start End Event
        CheatCode startEndEventCheatCode = new CheatCode();
        startEndEventCheatCode.code = startEndEventCode;
        startEndEventCheatCode.action = gm.eventManager.ExternalStartEndGame;
        cheatCodes.Add(startEndEventCheatCode);

        // Gravity Zero
        CheatCode gravityZeroCheatCode = new CheatCode();
        gravityZeroCheatCode.code = gravityZeroCode;
        gravityZeroCheatCode.action = gm.gravityManager.SetGravityZeroSwap;
        cheatCodes.Add(gravityZeroCheatCode);

        // Cooldowns Zero
        CheatCode cooldownsZeroCheatCode = new CheatCode();
        cooldownsZeroCheatCode.code = cooldownsZeroCode;
        cooldownsZeroCheatCode.action = gm.player.SetPouvoirsCooldownZeroSwap;
        cheatCodes.Add(cooldownsZeroCheatCode);

        // Invincibility
        CheatCode invincibilityCheatCode = new CheatCode();
        invincibilityCheatCode.code = invincibilityCode;
        invincibilityCheatCode.action = gm.player.SwapInvincible;
        cheatCodes.Add(invincibilityCheatCode);

        // Hide Console
        CheatCode hideConsoleCheatCode = new CheatCode();
        hideConsoleCheatCode.code = hideConsoleCode;
        hideConsoleCheatCode.action = gm.console.SwapConsoleVisibility;
        cheatCodes.Add(hideConsoleCheatCode);

        // Disable Ennemis
        CheatCode disableEnnemisCheatCode = new CheatCode();
        disableEnnemisCheatCode.code = disableEnnemisCode;
        disableEnnemisCheatCode.action = gm.ennemiManager.SwapDisableEnnemis;
        cheatCodes.Add(disableEnnemisCheatCode);
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
