using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateUnscaledTime : MonoBehaviour {

    protected Material material;

    public void Start() {
        material = gameObject.GetComponent<Image>().material;
    }

    public void Update() {
        material.SetFloat("_UnscaledTime", Time.unscaledTime);
    }
}
