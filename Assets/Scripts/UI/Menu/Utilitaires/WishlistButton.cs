using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WishlistButton : MonoBehaviour {
    public void Start() {
        SelectorManager selectorManager = SelectorManager.Instance;
        gameObject.SetActive(selectorManager.isDemo);
    }
}
