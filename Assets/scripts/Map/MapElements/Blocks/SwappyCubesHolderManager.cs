using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SwappyCubesHolderManager : MonoBehaviour {

    public float frequenceInterval = 1.5f;
    public int nbIntervals = 2;
    public Vector2Int startIntervalRange = new Vector2Int(0, 1);
    public bool setLinky = true;

    protected GameManager gm;
    protected List<SwappyCubesHolder> holders;
    protected Texture2D linkyTexture;

    public void Initialize(bool gatherCubesInChildren) {
        gm = GameManager.Instance;
        linkyTexture = Resources.Load<Texture2D>("linky_cube_circle");
        holders = GatherHolders();
        foreach(SwappyCubesHolder holder in holders) {
            holder.Initialize(this, gatherCubesInChildren);
        }
    }

    public void StartSwapping() {
        StartCoroutine(CStartSwapping());
    }

    protected void SetCubesLinky(int initialInterval) {
        if (setLinky) {
            foreach (SwappyCubesHolder holder in holders) {
                holder.SetCubesLinky(initialInterval, linkyTexture);
            }
        }
    }

    protected List<SwappyCubesHolder> GatherHolders() {
        List<SwappyCubesHolder> swappyCubesHolders = new List<SwappyCubesHolder>();
        foreach (Transform child in transform) {
            SwappyCubesHolder swappyCubesHolder = child.gameObject.GetComponent<SwappyCubesHolder>();
            if (swappyCubesHolder != null)
                swappyCubesHolders.Add(swappyCubesHolder);
        }
        return swappyCubesHolders;
    }

    protected IEnumerator CStartSwapping() {
        Timer timer = new Timer(frequenceInterval);
        int currentInterval = UnityEngine.Random.Range(startIntervalRange[0], startIntervalRange[1] + 1);
        SetCubesLinky(currentInterval);

        while (!gm.eventManager.IsGameOver()) {
            if(timer.IsOver()) {
                currentInterval = (currentInterval + 1) % nbIntervals;
                NotifyHolderNewInterval(currentInterval);
                timer.Reset();
            }
            yield return null;
        }
    }

    protected void NotifyHolderNewInterval(int currentInterval) {
        foreach(SwappyCubesHolder holder in holders) {
            holder.NotifyNewInterval(currentInterval);
        }
    }

    public List<Cube> GetCubes() {
        return holders.SelectMany(holder => holder.GetCubes()).ToList();
    }
}
