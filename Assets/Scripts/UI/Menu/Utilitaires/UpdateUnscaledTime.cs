using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateUnscaledTime : MonoBehaviour {

    protected Material material;

    public void Start() {
        Image image = gameObject.GetComponent<Image>();
        image.material = new Material(image.material);
        material = image.material;
        //material = gameObject.GetComponent<Image>().material;
    }

    public void Update() {
        material.SetFloat("_UnscaledTime", Time.unscaledTime);
    }
}
