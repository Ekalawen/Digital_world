using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateUnscaledTime : MonoBehaviour {

    protected Material material;

    public void Start() {
        Image image = gameObject.GetComponent<Image>();
        if (image != null) {
            image.material = new Material(image.material);
            material = image.material;
        } else {
            Renderer renderer = GetComponent<Renderer>();
            renderer.material = new Material(renderer.material);
            material = renderer.material;
        }
    }

    public void Update() {
        material.SetFloat("_UnscaledTime", Time.unscaledTime);
    }
}
