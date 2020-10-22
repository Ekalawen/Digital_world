using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class ScanManager : MonoBehaviour {

    public bool useVision = false;
    public float visionRange = 10.0f;

    public bool useScans = false;
    public float scanFrequence = 2.0f;
    public float scanDuree = 5.0f;
    public float scanSpeed = 15.0f;
    public float scanWidth = 2.0f;

    protected List<Scan> scans;
    protected GameManager gm;
    protected Timer scanInitializerTimer;

    public void Initialize() {
        gm = GameManager.Instance;
        scans = new List<Scan>();
        scanInitializerTimer = new Timer(scanFrequence);

        ScanInitialize();
    }

    public void Update() {
        OnlyRenderScannableCubes();
    }

    protected void ScanInitialize() {
        if (!useScans)
            return;
    }

    protected void OnlyRenderScannableCubes() {
        if (!useVision && !useScans)
            return;

        if (useScans)
            UpdateScans();

        // On active ce qui est proche du joueur ou dans un scan
        Vector3 center = gm.player.transform.position;
        foreach(Cube cube in gm.map.GetAllCubes()) {
            bool isNear = Vector3.Distance(center, cube.transform.position) <= visionRange;
            bool isInScans = IsInScans(cube.transform.position);
            if (useScans && useVision) {
                cube.gameObject.SetActive(isNear || isInScans);
            } else if (useVision) {
                cube.gameObject.SetActive(isNear);
            } else if (useScans) {
                cube.gameObject.SetActive(isInScans);
            }
        }
    }

    protected void UpdateScans() {
        for (int i = 0; i < scans.Count; i++) {
            Scan scan = scans[i];
            if(scan.IsOver()) {
                scans.RemoveAt(i);
                i--;
            }
        }
        if(scanInitializerTimer.IsOver()) {
            scans.Add(new Scan(scanDuree, scanSpeed, scanWidth));
            scanInitializerTimer.Reset();
        }

    }

    protected bool IsInScans(Vector3 position) {
        foreach(Scan scan in scans) {
            if (scan.IsIn(position))
                return true;
        }
        return false;
    }
}
