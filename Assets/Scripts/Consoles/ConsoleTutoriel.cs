using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsoleTutoriel : Console {
    public override void PremiersMessages() {
        string levelName = PlayerPrefs.GetString(MenuLevel.LEVEL_NAME_KEY);
		AjouterMessage ("[Niveau]: " + levelName, TypeText.BASIC_TEXT, bUsePrefix: false);
		AjouterMessage ("[Niveau]: Initialisation de la Matrice ...", TypeText.BASIC_TEXT, bUsePrefix: false);
		//AjouterMessageImportant (map.lumieres.Count + " Datas trouvées !", TypeText.ALLY_TEXT, 5);
		//AjouterMessage (gm.ennemiManager.ennemis.Count + " Ennemis détectés !", TypeText.ALLY_TEXT);
    }
    public override void AltitudeCritique() {
        // On le fait pas !
    }

	public override void GrandSaut(float hauteurSaut) {
        // On le fait pas !
	}
}
