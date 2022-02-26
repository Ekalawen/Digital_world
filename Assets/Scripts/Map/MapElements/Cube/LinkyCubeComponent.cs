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
    protected bool isSwappy = false;

    public void Initialize(Cube cube) {
        map = GameManager.Instance.map;
        this.cube = cube;
        RegisterToVoisins();
        ChooseAnchor();
    }

    public void InitializeInReward(Cube cube) {
        this.cube = cube;
        linkyVoisins = new List<Cube>();
        ChooseAnchor();
    }

    protected void RegisterToVoisins() {
        linkyVoisins = map.GetVoisinsPleinsAll(cube.transform.position).Select(pos => map.GetCubeAt(pos)).ToList();
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

    public Vector3 GetBarycentre(List<Vector3> preComputedLinkedPositions = null) {
        List<Vector3> linkedPositions = preComputedLinkedPositions != null ? preComputedLinkedPositions : GetLinkedCubes().Select(c => c.transform.position).ToList();
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

    public void LinkyBuild() {
        foreach(BuilderCube linkedCube in GetLinkedCubes().FindAll(c => c.type == Cube.CubeType.BUILDER).Select(c => (BuilderCube)c)) {
            linkedCube.RealBuild();
        }
    }

    protected void ChooseAnchor() {
        if(linkyVoisins.Count == 0) {
            InitParams();
        } else {
            LinkyCubeComponent mainVoisin = linkyVoisins[0].GetLinkyCubeComponent();
            List<Cube> linkedCubes = GetLinkedCubes();
            Vector3 barycentre = mainVoisin.GetBarycentre(preComputedLinkedPositions: linkedCubes.Select(c => c.transform.position).ToList());
            foreach(Cube linkedCube in linkedCubes) {
                linkedCube.GetLinkyCubeComponent().CopyParamsFrom(mainVoisin, barycentre);
            }
        }
    }

    protected void CopyParamsFrom(LinkyCubeComponent linkyCubeComponent, Vector3 barycentre) {
        this.anchor = barycentre;
        this.timeFixedOffset = linkyCubeComponent.timeFixedOffset;
        this.linkyColor = linkyCubeComponent.linkyColor;
        WriteParamsToShader(anchor, timeFixedOffset);
    }

    protected void InitParams() {
        anchor = cube.transform.position;
        timeFixedOffset = UnityEngine.Random.Range(0.0f, 1000.0f);
        WriteParamsToShader(anchor, timeFixedOffset);
    }

    protected void WriteParamsToShader(Vector3 newAnchor, float newTimeFixedOffset) {
        cube.BothMaterialsSetVector("_LinkyCubeAnchor", newAnchor);
        cube.BothMaterialsSetFloat("_LinkyCubeTimeFixedOffset", newTimeFixedOffset);
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

    public void SetSwappy() {
        isSwappy = true;
    }

    public bool IsSwappy() {
        return isSwappy;
    }
}
