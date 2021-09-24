using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PouvoirPathfinder : IPouvoir {

	public GameObject lumiereOrbesPathPrefab;
	public GameObject itemsOrbesPathPrefab;
	public GameObject orbTriggerPathPrefab;
    public bool detectLumieres = true;
    public bool detectItems = true;
    public bool detectOrbTriggers = true;
    public float dureePath = 3.0f;
    public float vitessePath = 30.0f;
    [ColorUsage(true, true)]
    public Color lumierePathColor = Color.yellow;
    [ColorUsage(true, true)]
    public Color itemPathColor = Color.magenta;
    [ColorUsage(true, true)]
    public Color orbTriggerPathColor = Color.blue;

    protected override bool UsePouvoir() {
        List<Vector3> lumieresPositions = GetAllLumieresPositions();
        List<Vector3> itemsPositions = GetAllItemsPositions();
        List<Vector3> orbTriggersPositions = GetAllOrbTriggersPositions();

        int nbObjectifs = lumieresPositions.Count + itemsPositions.Count + orbTriggersPositions.Count;
        if (!player.CanUseLocalisation() || nbObjectifs == 0) {
            if (gm.map.GetLumieresFinalesAndAlmostFinales().Count == 0) {
                gm.console.FailLocalisationUnauthorized();
            } else {
                gm.console.FailLocalisationInEndEvent();
            }
            gm.soundManager.PlayFailActionClip();
            return false;
        }

        float nbPathsToDraw = ((detectLumieres && lumieresPositions.Count > 0) ? 1 : 0)
                          + ((detectItems && itemsPositions.Count > 0) ? 1 : 0)
                          + ((detectOrbTriggers && orbTriggersPositions.Count > 0) ? 1 : 0);
        float lumiereOrbeStartOffset = 0;
        float itemOrbeStartOffset = ((detectLumieres && lumieresPositions.Count > 0) ? 1 : 0) / nbPathsToDraw;
        float orbTriggerOrbeStartOffset = (((detectLumieres && lumieresPositions.Count > 0) ? 1 : 0) + ((detectItems && itemsPositions.Count > 0) ? 1 : 0)) / nbPathsToDraw;
        bool haveFoundLumiere = detectLumieres && DrawPathToPositions(lumieresPositions, lumierePathColor, lumiereOrbesPathPrefab, lumiereOrbeStartOffset, distanceMinToEnd: 0);
        bool haveFoundItem = detectItems && DrawPathToPositions(itemsPositions, itemPathColor, itemsOrbesPathPrefab, itemOrbeStartOffset, distanceMinToEnd: 0);
        bool haveFoundOrbTrigger = detectOrbTriggers && DrawPathToPositions(orbTriggersPositions, orbTriggerPathColor, orbTriggerPathPrefab, orbTriggerOrbeStartOffset, distanceMinToEnd: 2);

        if (!haveFoundLumiere && !haveFoundItem && !haveFoundOrbTrigger) {
            gm.console.FailLocalisationObjectifInateignable();
            gm.soundManager.PlayFailActionClip();
            return false;
        }

        return true;
    }

    protected bool DrawPathToPositions(List<Vector3> positions, Color pathColor, GameObject orbePrefab, float orbeStartOffset, float distanceMinToEnd) {
        try {
            Vector3 nearestPosition = DrawPathToNearestPosition(positions, pathColor, orbePrefab, orbeStartOffset, distanceMinToEnd);

            gm.console.RunDetection(nearestPosition);

            if (detectItems)
                NotifyOnlyVisibleOnTriggerItems();

            return true;
        } catch (System.Exception e) {
            //Debug.LogWarning($"Pouvoir Detection fails :\n{e.StackTrace}");
            return false;
        }
    }

    private Vector3 DrawPathToNearestPosition(List<Vector3> positions, Color pathColor, GameObject orbePrefab, float orbeStartOffset, float distanceMinToEnd) {
        //Vector3 nearestPosition = positions.OrderBy(p => Vector3.Distance(p, player.transform.position)).First();
        //List<Vector3> posToDodge = GetPosToDodge();
        //List<Vector3> shortestPath = GetPathForPosition(nearestPosition, posToDodge);
        List<List<Vector3>> pathsToPositions = new List<List<Vector3>>();
        List<Vector3> posToDodge = GetNonRegularCubesToDodge();
        positions = positions.OrderBy(p => Vector3.Distance(p, player.transform.position)).Take(3).ToList();
        foreach (Vector3 pos in positions) {
            List<Vector3> pathToPosition = GetPathForPosition(pos, posToDodge, distanceMinToEnd);
            pathsToPositions.Add(pathToPosition);
        }
        List<List<Vector3>> notNullPaths = pathsToPositions.FindAll(p => p != null);
        if (notNullPaths.Count <= 0)
            throw new System.Exception("Aucune position accessible !");
        List<Vector3> shortestPath = notNullPaths.OrderBy(p => p.Count).First();
        Vector3 nearestPosition = positions[pathsToPositions.IndexOf(shortestPath)];

        DrawPathToPosition(nearestPosition, shortestPath, pathColor, orbePrefab, orbeStartOffset);
        return nearestPosition;
    }

    protected void DrawPathToPosition(Vector3 position, List<Vector3> path, Color pathColor, GameObject orbePrefab, float orbeStartOffset) {
        if (path != null)
            StartCoroutine(DrawPath(path, pathColor, orbePrefab, orbeStartOffset));
        else
            Debug.Log("Objectif inaccessible en " + position + " !");
    }

    protected List<Vector3> GetPathForPosition(Vector3 position, List<Vector3> posToDodge, float distanceMinToEnd) {
        List<Vector3> path = gm.map.GetPath(
            start: player.transform.position,
            end: position,
            posToDodge: posToDodge,
            bIsRandom: true,
            useNotInMapVoisins: true,
            ignoreSwappyCubes: true,
            distanceMinToEnd: distanceMinToEnd);
        if(distanceMinToEnd > 0 && path != null && path.Last() != position) {
            List<Vector3> complementaryPath = gm.map.GetPath(
                start: path.Last(),
                end: position,
                posToDodge: null,
                bIsRandom: false,
                useNotInMapVoisins: true,
                collideWithCubes: false, // c'est surtout cette ligne qui compte !
                ignoreSwappyCubes: true,
                distanceMinToEnd: 0 );
            path.AddRange(complementaryPath);
        }
        return path;
    }

    protected List<Vector3> GetNonRegularCubesToDodge() {
        List<Vector3> posToDodge = gm.map.GetAllNonRegularCubePos();
        for (int i = 0; i < posToDodge.Count; i++)
            posToDodge[i] = MathTools.Round(posToDodge[i]);
        return posToDodge;
    }

    protected List<Vector3> GetAllLumieresPositions() {
        List<Vector3> positions = new List<Vector3>();

        List<LumiereSwitchable> lumieresSwitchablesOn = gm.map.GetLumieresSwitchables().FindAll(ls => ls.GetState() == LumiereSwitchable.LumiereSwitchableState.ON);
        lumieresSwitchablesOn = lumieresSwitchablesOn.FindAll(ls => ls.IsAccessible());
        List<Vector3> lumieresAccessiblesPositions = gm.map.GetLumieres().FindAll(l => l.IsAccessible()).Select(l => l.transform.position).ToList();

        if(lumieresSwitchablesOn.Count > 0) {
            positions.AddRange(lumieresSwitchablesOn.Select(ls => ls.transform.position).ToList());
        } else {
            positions.AddRange(lumieresAccessiblesPositions);
        }

        return positions;
    }

    protected List<Vector3> GetAllItemsPositions() {
        return gm.itemManager.GetItemsPositions();
    }

    protected List<Vector3> GetAllOrbTriggersPositions() {
        return gm.itemManager.GetAllOrbTriggers().Select(o => o.transform.position).ToList();
    }

    protected IEnumerator DrawPath(List<Vector3> path, Color pathColor, GameObject orbePrefab, float orbeStartOffset) {
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
                AutoColorBouncer colorBouncer = go.GetComponent<AutoColorBouncer>();
                colorBouncer.colorToBounceTo = pathColor;
                colorBouncer.startingTime = colorBouncer.intervalTime * orbeStartOffset;
                AutoBouncer bouncer = go.GetComponent<AutoBouncer>();
                bouncer.startingTime = bouncer.intervalTime * orbeStartOffset;
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
