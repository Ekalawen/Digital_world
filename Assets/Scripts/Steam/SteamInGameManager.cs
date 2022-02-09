using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


public class SteamInGameManager : MonoBehaviour {

	protected GameManager gm;
	protected Callback<GameOverlayActivated_t> m_GameOverlayActivated;

	public void Initialize() {
		gm = GameManager.Instance;
		if (SteamManager.Initialized) {
			m_GameOverlayActivated = Callback<GameOverlayActivated_t>.Create(OnGameOverlayActivated);
		}
	}

	protected void OnGameOverlayActivated(GameOverlayActivated_t pCallback) {
		if (pCallback.m_bActive != 0) {
			Debug.Log("Steam Overlay has been activated");
			gm.Pause();
		} else {
			Debug.Log("Steam Overlay has been closed");
		}
	}
}
