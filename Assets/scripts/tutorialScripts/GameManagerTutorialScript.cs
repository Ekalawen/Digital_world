using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerTutorialScript : GameManagerScript {

	public override void instantiatePlayer() {
		base.instantiatePlayer();

		// On veut changer la position du joueur
		player.transform.position = new Vector3(5, 5, 5);
	}
}
