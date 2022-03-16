using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

public class ConsoleTutoriel : Console {
    public override void PremiersMessages() {
        string levelName = levelVisualName.GetLocalizedString().Result;
        LocalizedString initializationNiveau = strings.initialisationNiveau;
        initializationNiveau.Arguments = new object[] { levelName };
		AjouterMessage (initializationNiveau, TypeText.GREEN_TEXT, bUsePrefix: false);
		AjouterMessage (strings.initialisationMatrice, TypeText.GREEN_TEXT, bUsePrefix: false);
    }
    public override void AltitudeCritique() {
        // On le fait pas !
    }

	public override void GrandSaut(float hauteurSaut) {
        // On le fait pas !
	}

    // On affiche toujours la Console dans le Tuto ! :)
    public override void DisplayOrNotConsole() {
        consoleBackground.SetActive(true);
    }

    protected override void InitializeDataCountText() {
        // On ne le fait pas ! :)
    }
}
