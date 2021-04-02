using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SwappyCubesHolder : MonoBehaviour {

    public List<int> intervalToDisable;
    public List<int> intervalToEnable;

    protected GameManager gm;
    protected List<Cube> cubes;

    public void Initialize() {
        gm = GameManager.Instance;
        cubes = GatherCubes();
    }

    public void SetCubesLinky() {
        foreach(Cube cube in cubes) {
            cube.SetLinky();
        }
    }

    protected List<Cube> GatherCubes() {
        List<Cube> cubes = new List<Cube>();
        foreach (Transform child in transform) {
            Cube cube = child.gameObject.GetComponent<Cube>();
            if (cube != null)
                cubes.Add(cube);
        }
        return cubes;
    }

    protected void SetCubesVisibleState(bool visibleState) {
        cubes = cubes.FindAll(c => c != null);
        if(cubes.Count > 0) {
            //if(!visibleState) {
            //    foreach(Cube cube in cubes) {
            //        Vector3 impactPoint = cube.GetLinkyCubeComponent().GetAnchor();
            //        float impactRadius = Vector3.Distance(impactPoint, cube.GetLinkyCubeComponent().GetFarestCornerFromAnchor());
            //        float impactDuration = 1.5f;
            //        //Vector3 impactPoint = cube.transform.position;
            //        //float impactRadius = Mathf.Sqrt(3);
            //        //float impactDuration = 0.5f;
            //        cube.StartImpact(impactPoint, impactRadius, impactDuration);
            //    }
            //} else {
            //    foreach(Cube cube in cubes) {
            //        cube.StopImpact();
            //    }
            //}
            cubes[0].SetEnableValue(visibleState);
        } else {
            Destroy(this);
        }
    }

    public List<Cube> GetCubes() {
        return cubes;
    }

    public void NotifyNewInterval(int currentInterval) {
        if(intervalToDisable.Contains(currentInterval)) {
            SetCubesVisibleState(false);
        } else if (intervalToEnable.Contains(currentInterval)) {
            SetCubesVisibleState(true);
        }
    }
}
