using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RewardConsole : Console {

    protected Timer timerReward;
    protected Timer timerRewardPlusDelay;

    // On veut tout sauf ce qui est en rapport avec le game, puisque les objets du game n'existent plus !
    public override void Initialize() {
		// Initialisation des variables
		name = "Console";
		lines = new List<GameObject> ();
		numLines = new List<int> ();
		importantText.text = "";

        DisplayOrNotConsole();

        InitTimersMessages();
    }
    public override void DisplayOrNotConsole() {
        bool shouldDisplayConsole = PrefsManager.GetBool(PrefsManager.DISPLAY_CONSOLE_KEY, MenuOptions.defaultDisplayConsole);
        consoleBackground.SetActive(shouldDisplayConsole);
        // don't display frame rate here as we don't have one :)
    }

    public override void Update() {
        RunTimedMessages();

        if (timerReward.IsOver()) {
            CleanAllLines();
        }
        if (timerRewardPlusDelay.IsOver()) {
            foreach (TimerMessage tm in timersMessages)
                tm.timer.Reset();
            timerReward.Reset();
            timerRewardPlusDelay.Reset();
        }
    }

    protected override void RunTimedMessages() {
        for(int i = 0; i < timersMessages.Count; i++) {
            TimerMessage timerMessage = timersMessages[i];
            if(timerMessage.timer.IsOver()) {
                AjouterMessage(timerMessage.message.message, timerMessage.message.type, false);
                timerMessage.timer.Stop();
            }
        }
    }

    public void SetDureeReward(float dureeReward, float dureeDelay) {
        timerReward = new Timer(dureeReward);
        timerRewardPlusDelay = new Timer(dureeReward + dureeDelay);
    }
}
