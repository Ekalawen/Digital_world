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
        foreach(Cube cube in cubesInRange) {
            float distance = Vector3.Distance(cube.transform.position, this.transform.position);
            float affaiblissement = 1f - distance / range;
            cube.AddColor(color * affaiblissement);
        }
    }
}
