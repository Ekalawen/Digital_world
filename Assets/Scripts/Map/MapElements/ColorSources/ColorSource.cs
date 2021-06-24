using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Une ColorSource s'occupe de gérer la couleur des cubes
public class ColorSource : MonoBehaviour {

    [HideInInspector] public Color color;
    [HideInInspector] public float range;
    [HideInInspector] public List<Cube> cubesInRange;
    protected MapManager map;
    protected Coroutine goingToColor = null;
    protected List<Cube> cubesNotInRangeAnymore;

    public void Initialize(Color color, float range) {
        this.color = color;
        this.range = range;
        this.map = FindObjectOfType<MapManager>();
        this.cubesNotInRangeAnymore = new List<Cube>();

        // Récupère tous les cubes à portée !
        cubesInRange = new List<Cube>();
        List<Cube> cubes = map.GetCubesInSphere(transform.position, range); // Plus opti que OverlapShpere ! <3
        foreach(Cube cube in cubes) {
            cubesInRange.Add(cube);
        }

        // Met à jour la couleurs de tous les cubes à portée !
        AddColor();
    }

    public void AddCube(Cube cube) {
        if(!cubesInRange.Contains(cube)) {
            cubesInRange.Add(cube);
            Color colorToAdd = GetColorForPosition(cube.transform.position);
            cube.AddColor(colorToAdd);
        }
    }

    public Color GetColorForPosition(Vector3 pos) {
        float distance = Vector3.Distance(pos, transform.position);
        float affaiblissement = 1f - distance / range;
        return color * affaiblissement;
    }

    public void Delete() {
        RemoveColor();
        Destroy(gameObject);
    }

    protected void AddColor() {
        SanitizeCubesInRange();
        foreach(Cube cube in cubesInRange) {
            if (cube != null) {
                float distance = Vector3.Distance(cube.transform.position, this.transform.position);
                float affaiblissement = 1f - distance / range;
                cube.AddColor(color * affaiblissement);
            }
        }
    }

    protected void RemoveColor() {
        SanitizeCubesInRange();
        foreach(Cube cube in cubesInRange) {
            if (cube != null) {
                float distance = Vector3.Distance(cube.transform.position, this.transform.position);
                float affaiblissement = 1f - distance / range;
                cube.AddColor(-1.0f * color * affaiblissement);
            }
        }
    }

    public void ChangeColor(Color newColor) {
        RemoveColor();
        color = newColor;
        AddColor();
    }

    public void InverseColor() {
        ChangeColor(new Color(1.0f - color.r, 1.0f - color.g, 1.0f - color.b));
    }

    public void RemoveCube(Cube cube) {
        cubesNotInRangeAnymore.Add(cube);
    }

    protected void SanitizeCubesInRange() {
        if (cubesNotInRangeAnymore.Count > 0) {
            cubesInRange = cubesInRange.FindAll(c => !cubesNotInRangeAnymore.Contains(c));
            cubesNotInRangeAnymore.Clear();
        }
    }

    public void GoToColor(Color newColor, float overTime) {
        if(goingToColor == null)
            goingToColor = StartCoroutine(CGoToColor(newColor, overTime));
    }

    protected IEnumerator CGoToColor(Color targetColor, float overTime) {
        Color debutColor = color;
        Timer timer = new Timer(overTime);
        while(!timer.IsOver()) {
            float avancement = timer.GetAvancement();
            Color middleColor = debutColor * (1 - avancement) + targetColor * avancement;
            ChangeColor(middleColor);
            yield return new WaitForSeconds(Mathf.Min(0.035f, overTime / 10));
        }
        ChangeColor(targetColor);
        goingToColor = null;
    }

    public static Color LimiteColorSaturation(Color color, float minColorSaturationAndValue = 0.1f) {
        float mean = (color.r + color.g + color.b) / 3.0f;
        if(mean < minColorSaturationAndValue) {
            float ecart = minColorSaturationAndValue - mean;
            return new Color(color.r + ecart, color.g + ecart, color.b + ecart, color.a);
        }
        return color;
    }
}
