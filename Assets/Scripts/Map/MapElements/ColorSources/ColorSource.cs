using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Une ColorSource s'occupe de gérer la couleur des cubes
public class ColorSource : MonoBehaviour {

	public enum ThemeSource {JAUNE, VERT, BLEU_GLACE, BLEU_NUIT, VIOLET, ROUGE, MULTICOLOR, BLANC, NOIR, RANDOM};

    [HideInInspector] public Color color;
    [HideInInspector] public float range;
    [HideInInspector] public List<Cube> cubesInRange;
    private MapManager map;

    public void Initialize(Color color, float range) {
        this.color = color;
        this.range = range;
        this.map = FindObjectOfType<MapManager>();

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
        foreach(Cube cube in cubesInRange) {
            if (cube != null) {
                float distance = Vector3.Distance(cube.transform.position, this.transform.position);
                float affaiblissement = 1f - distance / range;
                cube.AddColor(color * affaiblissement);
            }
        }
    }

    protected void RemoveColor() {
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
        cubesInRange.Remove(cube);
    }
}
