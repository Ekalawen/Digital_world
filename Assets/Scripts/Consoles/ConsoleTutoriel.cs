using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

public class ConsoleTutoriel : Console {
    public override void PremiersMessages() {
        string levelName = PlayerPrefs.GetString(MenuLevel.LEVEL_NAME_KEY);
        LocalizedString initializationNiveau = strings.initialisationNiveau;
        initializationNiveau.Arguments = new object[] { levelName };
		AjouterMessage (initializationNiveau, TypeText.BASIC_TEXT, bUsePrefix: false);
		AjouterMessage (strings.initialisationMatrice, TypeText.BASIC_TEXT, bUsePrefix: false);
    }
    public override void AltitudeCritique() {
        // On le fait pas !
    }

	public override void GrandSaut(float hauteurSaut) {
        // On le fait pas !
	}
}
