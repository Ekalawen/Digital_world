using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LinkyCubeComponent : MonoBehaviour {

    protected Cube cube;
    protected List<Cube> linkyVoisins;
    protected MapManager map;

    public void Initialize(Cube cube) {
        map = GameManager.Instance.map;
        this.cube = cube;
        RegisterVoisins();
    }

    protected void RegisterVoisins() {
        linkyVoisins = map.GetVoisinsPleins(cube.transform.position).Select(pos => map.GetCubeAt(pos)).ToList();
        linkyVoisins = linkyVoisins.FindAll(c => c.IsLinky());
        foreach(Cube voisin in linkyVoisins) {
            voisin.GetLinkyCubeComponent().AddLinkyVoisin(cube);
        }
    }

    protected void AddLinkyVoisin(Cube voisin) {
        if (!linkyVoisins.Contains(voisin)) {
            linkyVoisins.Add(voisin);
        }
    }

    protected void RemoveLinkyVoisin(Cube voisin) {
        linkyVoisins.Remove(voisin);
    }

    public void OnDestroy() {
        foreach(Cube voisin in linkyVoisins) {
            voisin.GetLinkyCubeComponent().RemoveLinkyVoisin(cube);
        }
    }

    public List<Cube> GetLinkedCubes() {
        HashSet<Cube> linkedCubes = new HashSet<Cube>();
        Stack<Cube> cubesToAdd = new Stack<Cube>();
        cubesToAdd.Push(cube);

        while(cubesToAdd.Count != 0) {
            LinkyCubeComponent linkyCube = cubesToAdd.Pop().GetLinkyCubeComponent();
            if(!linkedCubes.Contains(linkyCube.cube)) {
                linkedCubes.Add(linkyCube.cube);
                foreach(Cube voisin in linkyCube.linkyVoisins) {
                    cubesToAdd.Push(voisin);
                }
            }
        }

        return linkedCubes.ToList();
    }

    public void LinkyDecompose(float duree) {
        foreach(Cube linkedCube in GetLinkedCubes()) {
            linkedCube.RealDecompose(duree);
        }
    }
}
