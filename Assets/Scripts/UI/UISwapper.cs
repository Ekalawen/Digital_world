using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISwapper : MonoBehaviour {

    public GameObject objectToSwap;
    public BackgroundCarousel carousel;
    public bool swapBloom;
    [ConditionalHide("swapBloom")]
    public GameObject bloomVolume;

    public void Swap() {
        bool newState = !objectToSwap.activeSelf;
        objectToSwap.SetActive(newState);
        bloomVolume.SetActive(newState);
        if (newState) {
            carousel.AddFiltre();
        } else {
            carousel.RemoveFiltre();
        }
    }
}
