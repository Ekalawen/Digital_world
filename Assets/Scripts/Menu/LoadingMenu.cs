using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingMenu : MonoBehaviour {

    public GameObject appuyezSurUneToucheTexte;

    protected AsyncOperation loading;

    public void Initialize(AsyncOperation loading) {
        this.loading = loading;
        appuyezSurUneToucheTexte.SetActive(false);
        StartCoroutine(CConfirmPlay());
    }

    protected IEnumerator CConfirmPlay() {
        while (loading.progress <= 0.89f) { // loading.isDone ne fonctionne pas !!! x)
            yield return null;
        }
        appuyezSurUneToucheTexte.SetActive(true);

        while (!Input.anyKeyDown) {
            yield return null;
        }

        loading.allowSceneActivation = true;
    }
}
