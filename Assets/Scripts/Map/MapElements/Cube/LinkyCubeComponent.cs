using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LinkyCubeComponent : MonoBehaviour {

    protected Cube cube;
    protected List<Cube> linkyVoisins;
    protected MapManager map;
    protected Vector3 anchor = Vector3.zero;
    protected float timeFixedOffset;
    protected Color linkyColor;

    public void Initialize(Cube cube) {
        map = GameManager.Instance.map;
        this.cube = cube;
        RegisterVoisins();
        ChooseAnchor();
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

    public Vector3 GetBarycentre() {
        List<Vector3> linkedPositions = GetLinkedCubes().Select(c => c.transform.position).ToList();
        Vector3 sum = Vector3.zero;
        foreach(Vector3 linkedPosition in linkedPositions) {
            sum += linkedPosition;
        }
        return sum / linkedPositions.Count;
    }

    public void OnDestroy() {
        if (linkyVoisins != null) {
            foreach (Cube voisin in linkyVoisins) {
                voisin.GetLinkyCubeComponent().RemoveLinkyVoisin(cube);
            }
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

    protected void ChooseAnchor() {
        if(linkyVoisins.Count == 0) {
            InitParams();
        } else {
            LinkyCubeComponent mainVoisin = linkyVoisins[0].GetLinkyCubeComponent();
            foreach(Cube linkedCube in GetLinkedCubes()) {
                linkedCube.GetLinkyCubeComponent().CopyParamsFrom(mainVoisin);
            }
        }
    }

    protected void CopyParamsFrom(LinkyCubeComponent linkyCubeComponent) {
        this.anchor = linkyCubeComponent.anchor;
        this.timeFixedOffset = linkyCubeComponent.timeFixedOffset;
        this.linkyColor = linkyCubeComponent.linkyColor;
        WriteParamsToShader();
    }

    protected void InitParams() {
        anchor = cube.transform.position;
        timeFixedOffset = UnityEngine.Random.Range(0.0f, 1000.0f);
        WriteParamsToShader();
    }

    protected void WriteParamsToShader() {
        cube.GetMaterial().SetVector("_LinkyCubeAnchor", anchor);
        cube.GetMaterial().SetFloat("_LinkyCubeTimeFixedOffset", timeFixedOffset);
    }

    public void LinkyExplode() {
        foreach(Cube linkedCube in GetLinkedCubes()) {
            linkedCube.RealExplode();
        }
    }

    public void LinkyDisable() {
        foreach(Cube linkedCube in GetLinkedCubes()) {
            linkedCube.RealDisable();
        }
    }

    public void LinkyEnable() {
        foreach(Cube linkedCube in GetLinkedCubes()) {
            linkedCube.RealEnable();
        }
    }

    public void LinkySetEnableValueIn(bool value, float duration, Vector3 impactPoint) {
        foreach(Cube linkedCube in GetLinkedCubes()) {
            linkedCube.RealSetEnableValueIn(value, duration, impactPoint);
        }
    }


    public Vector3 GetAnchor() {
        return anchor;
    }

    public Cube GetFarestCubeFromPoint(Vector3 point) {
        return GetLinkedCubes().OrderBy(cube => Vector3.Distance(point, cube.transform.position)).Last();
    }

    public Vector3 GetFarestCornerFromPoint(Vector3 point) {
        return GetFarestCubeFromPoint(point).GetCornerPositions().OrderBy(pos => Vector3.Distance(point, pos)).Last();
    }
}
