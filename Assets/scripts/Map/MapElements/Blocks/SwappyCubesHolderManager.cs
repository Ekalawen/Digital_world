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

    public void Initialize() {
        gm = GameManager.Instance;
        holders = GatherHolders();
        foreach(SwappyCubesHolder holder in holders) {
            holder.Initialize(this);
        }
    }

    public void StartSwapping() {
        SetCubesLinky();
        StartCoroutine(CStartSwapping());
    }

    protected void SetCubesLinky() {
        if (setLinky) {
            foreach (SwappyCubesHolder holder in holders) {
                holder.SetCubesLinky();
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
        NotifyHolderNewInterval(currentInterval);

        while(!gm.eventManager.IsGameOver()) {
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
