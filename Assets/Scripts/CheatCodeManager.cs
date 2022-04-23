using EZCameraShake;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CheatCode {
    public List<KeyCode> code;
    public Action action;
    public int state = 0;
}

public class CheatCodeManager : MonoBehaviour {

    [Header("Cheat Codes")]
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
    public List<KeyCode> swapPhasesCode;
    public List<KeyCode> startEndEventCode;
    public List<KeyCode> gravityZeroCode;
    public List<KeyCode> cooldownsZeroCode;
    public List<KeyCode> invincibilityCode;
    public List<KeyCode> hideConsoleCode;
    public List<KeyCode> disableEnnemisCode;
    public List<KeyCode> gainDash333Code;
    public List<KeyCode> gainPathfinder5Code;
    public List<KeyCode> gainGripDashCode;
    public List<KeyCode> gainTimeHackCode;
    public List<KeyCode> hackActivatedOrbTriggersCode;

    [Header("Links")]
    public GameObject dash333Prefab;
    public GameObject pathfinder5Prefab;
    public GameObject gripDashPrefab;
    public GameObject timeHackPrefab;

    protected GameManager gm;
    protected List<CheatCode> cheatCodes;
    [HideInInspector]
    public UnityEvent onUseCheatCode;

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

        // Swap Phases
        CheatCode swapPhasesCheatCode = new CheatCode();
        swapPhasesCheatCode.code = swapPhasesCode;
        swapPhasesCheatCode.action = gm.timerManager.ForceSwapPhases;
        cheatCodes.Add(swapPhasesCheatCode);

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

        // Gain Dash333 CheatCode
        CheatCode gainDash333CheatCode = new CheatCode();
        gainDash333CheatCode.code = gainDash333Code;
        gainDash333CheatCode.action = SwapGiveDash333;
        cheatCodes.Add(gainDash333CheatCode);

        // Gain Pathfinder5 CheatCode
        CheatCode gainPathfinder5CheatCode = new CheatCode();
        gainPathfinder5CheatCode.code = gainPathfinder5Code;
        gainPathfinder5CheatCode.action = SwapGivePathfinder5;
        cheatCodes.Add(gainPathfinder5CheatCode);

        // Gain GripDash CheatCode
        CheatCode gainGripDashCheatCode = new CheatCode();
        gainGripDashCheatCode.code = gainGripDashCode;
        gainGripDashCheatCode.action = SwapGiveGripDash;
        cheatCodes.Add(gainGripDashCheatCode);

        // Gain TimeHack CheatCode
        CheatCode gainTimeHackCheatCode = new CheatCode();
        gainTimeHackCheatCode.code = gainTimeHackCode;
        gainTimeHackCheatCode.action = SwapGiveTimeHack;
        cheatCodes.Add(gainTimeHackCheatCode);

        // Hack activated OrbTriggers
        CheatCode hackActivatedOrbTriggersCheatCode = new CheatCode();
        hackActivatedOrbTriggersCheatCode.code = hackActivatedOrbTriggersCode;
        hackActivatedOrbTriggersCheatCode.action = gm.itemManager.HackAllActivatedOrbTriggers;
        cheatCodes.Add(hackActivatedOrbTriggersCheatCode);
    }

    public void Update() {
        foreach(CheatCode cheatCode in cheatCodes) {
            KeyCode currentKey = cheatCode.code[cheatCode.state];
            if(Input.GetKeyDown(currentKey)) {
                cheatCode.state += 1;
                if(cheatCode.state == cheatCode.code.Count) {
                    cheatCode.state = 0;
                    cheatCode.action.Invoke();
                    onUseCheatCode.Invoke();
                }
            } else if (Input.anyKeyDown) {
                cheatCode.state = 0;
            }
        }
    }

    protected void SwapGiveDash333() {
        if(gm.player.GetPouvoirLeftClick() == null || gm.player.GetPouvoirLeftClick().nom.GetLocalizedString().Result != dash333Prefab.GetComponent<IPouvoir>().nom.GetLocalizedString().Result) {
            PouvoirGiverItem.GivePouvoir(gm, dash333Prefab, PouvoirGiverItem.PouvoirBinding.LEFT_CLICK);
        } else {
            Debug.Log($"On met le pouvoir à null !");
            PouvoirGiverItem.GivePouvoir(gm, gm.player.pouvoirLeftBoutonPrefab, PouvoirGiverItem.PouvoirBinding.LEFT_CLICK);
        }
    }

    protected void SwapGivePathfinder5() {
        if(gm.player.GetPouvoirA() == null || gm.player.GetPouvoirA().nom.GetLocalizedString().Result != pathfinder5Prefab.GetComponent<IPouvoir>().nom.GetLocalizedString().Result) {
            PouvoirGiverItem.GivePouvoir(gm, pathfinder5Prefab, PouvoirGiverItem.PouvoirBinding.A);
        } else {
            Debug.Log($"On met le pouvoir à null !");
            PouvoirGiverItem.GivePouvoir(gm, gm.player.pouvoirAPrefab, PouvoirGiverItem.PouvoirBinding.A);
        }
    }

    protected void SwapGiveGripDash() {
        if(gm.player.GetPouvoirRightClick() == null || gm.player.GetPouvoirRightClick().nom.GetLocalizedString().Result != gripDashPrefab.GetComponent<IPouvoir>().nom.GetLocalizedString().Result) {
            PouvoirGiverItem.GivePouvoir(gm, gripDashPrefab, PouvoirGiverItem.PouvoirBinding.RIGHT_CLICK);
        } else {
            Debug.Log($"On met le pouvoir à null !");
            PouvoirGiverItem.GivePouvoir(gm, gm.player.pouvoirRightBoutonPrefab, PouvoirGiverItem.PouvoirBinding.RIGHT_CLICK);
        }
    }

    protected void SwapGiveTimeHack() {
        if(gm.player.GetPouvoirE() == null || gm.player.GetPouvoirE().nom.GetLocalizedString().Result != timeHackPrefab.GetComponent<IPouvoir>().nom.GetLocalizedString().Result) {
            PouvoirGiverItem.GivePouvoir(gm, timeHackPrefab, PouvoirGiverItem.PouvoirBinding.E);
        } else {
            Debug.Log($"On met le pouvoir à null !");
            PouvoirGiverItem.GivePouvoir(gm, gm.player.pouvoirEPrefab, PouvoirGiverItem.PouvoirBinding.E);
        }
    }
}
