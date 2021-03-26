using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PouvoirPathfinder : IPouvoir {

	public GameObject lumiereOrbesPathPrefab;
	public GameObject itemsOrbesPathPrefab;
    public bool detectLumieres = true;
    public bool detectItems = true;
    public float dureePath = 3.0f;
    public float vitessePath = 30.0f;
    public Color lumierePathColor = Color.yellow;
    public Color itemPathColor = Color.magenta;

    protected override bool UsePouvoir() {
        List<Vector3> lumieresPositions = GetAllLumieresPositions();
        List<Vector3> itemsPositions = GetAllItemsPositions();

        if (!player.CanUseLocalisation() || lumieresPositions.Count + itemsPositions.Count == 0) {
            if (gm.map.GetLumieresFinalesAndAlmostFinales().Count == 0) {
                gm.console.FailLocalisationUnauthorized();
            } else {
                gm.console.FailLocalisationInEndEvent();
            }
            gm.soundManager.PlayFailActionClip();
            return false;
        }

        bool haveFoundLumiere = detectLumieres && DrawPathToPositions(lumieresPositions, lumierePathColor, lumiereOrbesPathPrefab);
        bool haveFoundItem = detectItems && DrawPathToPositions(itemsPositions, itemPathColor, itemsOrbesPathPrefab);

        if (!haveFoundLumiere && !haveFoundItem) {
            gm.console.FailLocalisationObjectifInateignable();
            gm.soundManager.PlayFailActionClip();
            return false;
        }

        return true;
    }

    protected bool DrawPathToPositions(List<Vector3> positions, Color pathColor, GameObject orbePrefab) {
        try {
            Vector3 nearestPosition = DrawPathToNearestPosition(positions, pathColor, orbePrefab);

            gm.console.RunDetection(nearestPosition);

            if (detectItems)
                NotifyOnlyVisibleOnTriggerItems();

            return true;
        } catch (System.Exception e) {
            //Debug.LogWarning($"Pouvoir Detection fails :\n{e.StackTrace}");
            return false;
        }
    }

    private Vector3 DrawPathToNearestPosition(List<Vector3> positions, Color pathColor, GameObject orbePrefab) {
        //Vector3 nearestPosition = positions.OrderBy(p => Vector3.Distance(p, player.transform.position)).First();
        //List<Vector3> posToDodge = GetPosToDodge();
        //List<Vector3> shortestPath = GetPathForPosition(nearestPosition, posToDodge);
        List<List<Vector3>> pathsToPositions = new List<List<Vector3>>();
        List<Vector3> posToDodge = GetPosToDodge();
        positions = positions.OrderBy(p => Vector3.Distance(p, player.transform.position)).Take(3).ToList();
        foreach (Vector3 pos in positions) {
            List<Vector3> pathToPosition = GetPathForPosition(pos, posToDodge);
            pathsToPositions.Add(pathToPosition);
        }
        List<List<Vector3>> notNullPaths = pathsToPositions.FindAll(p => p != null);
        if (notNullPaths.Count <= 0)
            throw new System.Exception("Aucune position accessible !");
        List<Vector3> shortestPath = notNullPaths.OrderBy(p => p.Count).First();
        Vector3 nearestPosition = positions[pathsToPositions.IndexOf(shortestPath)];

        DrawPathToPosition(nearestPosition, shortestPath, pathColor, orbePrefab);
        return nearestPosition;
    }

    protected void DrawPathToPosition(Vector3 position, List<Vector3> path, Color pathColor, GameObject orbePrefab) {
        if (path != null)
            StartCoroutine(DrawPath(path, pathColor, orbePrefab));
        else
            Debug.Log("Objectif inaccessible en " + position + " !");
    }

    protected List<Vector3> GetPathForPosition(Vector3 position, List<Vector3> posToDodge) {
        List<Vector3> path = gm.map.GetPath(player.transform.position, position, posToDodge, bIsRandom: true, useNotInMapVoisins: true);
        return path;
    }

    private List<Vector3> GetPosToDodge()
    {
        List<Vector3> posToDodge = gm.map.GetAllNonRegularCubePos();
        for (int i = 0; i < posToDodge.Count; i++)
            posToDodge[i] = MathTools.Round(posToDodge[i]);
        return posToDodge;
    }

    protected List<Vector3> GetAllLumieresPositions() {
        List<Vector3> positions = new List<Vector3>();
        List<LumiereSwitchable> lumieresSwitchablesOn = gm.map.GetLumieresSwitchables();
        lumieresSwitchablesOn = lumieresSwitchablesOn.FindAll(ls => ls.GetState() == LumiereSwitchable.LumiereSwitchableState.ON);
        if(lumieresSwitchablesOn.Count > 0) {
            positions.AddRange(lumieresSwitchablesOn.Select(ls => ls.transform.position).ToList());
        } else {
            positions.AddRange(gm.map.GetAllLumieresPositions());
        }
        return positions;
    }

    protected List<Vector3> GetAllItemsPositions() {
        return gm.itemManager.GetItemsPositions();
    }

    protected IEnumerator DrawPath(List<Vector3> path, Color pathColor, GameObject orbePrefab) {
        int nbSpheresByNodes = 4;
        for(int i = 0; i < path.Count - 1; i++) {
            Vector3 current = path[i];
            Vector3 next = path[i + 1];
            for(int j = 0; j < nbSpheresByNodes; j++) {
                Vector3 direction = next - current;
                Vector3 pos = current + direction / nbSpheresByNodes * (j + 1);
                GameObject go = Instantiate(orbePrefab, pos, Quaternion.identity);
                Color color = gm.colorManager.GetColorForPosition(go.transform.position);
                color = Color.white - color;
                Material material = go.GetComponent<MeshRenderer>().material;
                material.color = color;
                material.SetColor("_EmissionColor", color);
                go.GetComponent<AutoColorBouncer>().colorToBounceTo = pathColor;
                Destroy(go, dureePath);
                yield return new WaitForSeconds(1.0f / vitessePath);
            }
        }
    }

    protected void NotifyOnlyVisibleOnTriggerItems() {
        foreach(Item item in gm.itemManager.GetItems()) {
            OnlyVisibleOnTrigger component = item.GetComponent<OnlyVisibleOnTrigger>();
            if(component != null && component.enabled) {
                component.Activate();
            }
        }
    }
}
