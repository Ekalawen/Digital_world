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

    public CheatCode(List<KeyCode> code, Action action) {
        this.code = code;
        this.action = action;
    }
}

public class CheatCodeManager : MonoBehaviour {

    [Header("Cheat Codes")]
    [Header("Progression related")]
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

    [Header("Power related")]
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

    [Header("SkillTree related")]
    public List<KeyCode> resetAllSkillTreeCode;
    public List<KeyCode> creditsTimes10Code;
    public List<KeyCode> resetCreditsCode;

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

        cheatCodes.Add(new CheatCode(winCode, gm.eventManager.WinGame));
        cheatCodes.Add(new CheatCode(loseCode, gm.eventManager.LoseGameWithTimeOut));

        if (gm.GetMapType() == MenuLevel.LevelType.INFINITE) {
            cheatCodes.Add(new CheatCode(plus10Code, gm.GetInfiniteMap().Add10BlockRun));
            cheatCodes.Add(new CheatCode(plus100Code, gm.GetInfiniteMap().Add100BlockRun));
        }

        if (gm.GetMapType() == MenuLevel.LevelType.REGULAR) {
            cheatCodes.Add(new CheatCode(plus10Code, gm.timerManager.Add10Time));
            cheatCodes.Add(new CheatCode(plus100Code, gm.timerManager.Add100Time));
            cheatCodes.Add(new CheatCode(plus1000Code, gm.timerManager.Add1000Time));
            cheatCodes.Add(new CheatCode(minus10Code, gm.timerManager.Minus10Time));
            cheatCodes.Add(new CheatCode(minus100Code, gm.timerManager.Minus100Time));
            cheatCodes.Add(new CheatCode(minus1000Code, gm.timerManager.Minus1000Time));
            cheatCodes.Add(new CheatCode(plus10DataCount, gm.eventManager.Add10DataCount));
            cheatCodes.Add(new CheatCode(plus100DataCount, gm.eventManager.Add100DataCount));
        }

        cheatCodes.Add(new CheatCode(swapPhasesCode, gm.timerManager.ForceSwapPhases));
        cheatCodes.Add(new CheatCode(startEndEventCode, gm.eventManager.ExternalStartEndGame));
        cheatCodes.Add(new CheatCode(gravityZeroCode, gm.gravityManager.SetGravityZeroSwap));
        cheatCodes.Add(new CheatCode(cooldownsZeroCode, gm.player.SetPouvoirsCooldownZeroSwap));
        cheatCodes.Add(new CheatCode(invincibilityCode, gm.player.SwapInvincible));
        cheatCodes.Add(new CheatCode(hideConsoleCode, gm.console.SwapConsoleVisibility));
        cheatCodes.Add(new CheatCode(disableEnnemisCode, gm.ennemiManager.SwapDisableEnnemis));
        cheatCodes.Add(new CheatCode(gainDash333Code, SwapGiveDash333));
        cheatCodes.Add(new CheatCode(gainPathfinder5Code, SwapGivePathfinder5));
        cheatCodes.Add(new CheatCode(gainGripDashCode, SwapGiveGripDash));
        cheatCodes.Add(new CheatCode(gainTimeHackCode, SwapGiveTimeHack));
        cheatCodes.Add(new CheatCode(hackActivatedOrbTriggersCode, gm.itemManager.HackAllActivatedOrbTriggers));

        cheatCodes.Add(new CheatCode(resetAllSkillTreeCode, gm.console.GetPauseMenu().skillTreeMenu.ResetAllUpgrades));
        cheatCodes.Add(new CheatCode(creditsTimes10Code, gm.console.GetPauseMenu().skillTreeMenu.MultiplyCreditsBy10));
        cheatCodes.Add(new CheatCode(resetCreditsCode, gm.console.GetPauseMenu().skillTreeMenu.ResetCredits));
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
