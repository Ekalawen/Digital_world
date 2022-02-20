using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathfinderData {
    public List<Vector3> positions;
    public int nbPositionsTheoretical;
    public bool shouldDetect;
    public GameObject orbesPathPrefab;
    public float orbeStartOffset;
    public Color pathColor;
    public GeoData geoData;
    public int distanceMinToEnd;
    public int stringIndice;
    public bool haveFoundSomething;
    public bool useReversedPathTechnique;

    public PathfinderData(
        List<Vector3> positions,
        int nbPositionsTheoretical,
        bool shouldDetect,
        GameObject orbesPathPrefab,
        Color pathColor,
        GeoData geoData,
        int distanceMinToEnd,
        int stringIndice,
        bool useReversedPathTechnique) {

        this.positions = positions;
        this.nbPositionsTheoretical = nbPositionsTheoretical;
        this.shouldDetect = shouldDetect;
        this.orbesPathPrefab = orbesPathPrefab;
        this.pathColor = pathColor;
        this.geoData = geoData;
        this.distanceMinToEnd = distanceMinToEnd;
        this.stringIndice = stringIndice;
        this.haveFoundSomething = false;
        this.useReversedPathTechnique = useReversedPathTechnique;
    }

    public float ComputeOrbeStartOffset(int nbPathfinders, int nbPathfindersBefore) {
        orbeStartOffset = (float)nbPathfindersBefore / nbPathfinders;
        return orbeStartOffset;
    }
}

public class PouvoirPathfinder : IPouvoir {

    public static int NB_SPHERES_BY_NODES = 4;

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
    public GeoData lumiereGeoData;
    public GeoData itemGeoData;
    public GeoData orbGeoData;

    protected override bool UsePouvoir() {
        List<PathfinderData> pathfinderDatas = GetAllPathfinderDatas();

        int nbObjectifs = pathfinderDatas.Select(data => data.positions.Count).Sum();
        if(!player.CanUseLocalisation() || nbObjectifs == 0) {
            AlertCantUseLocalisation();
            return false;
        }

        bool haveFoundSomething = false;
        foreach(PathfinderData pathfinderData in pathfinderDatas) {
            pathfinderData.haveFoundSomething = DrawPathToPositions(pathfinderData);
            haveFoundSomething = pathfinderData.haveFoundSomething || haveFoundSomething;
        }

        if(!haveFoundSomething) {
            AlertNothingFound();
            return false;
        }
        else { // On a au moins trouvé quelque chose
            gm.console.SummarizePathfinder(pathfinderDatas);
        }

        player.onUsePathfinder.Invoke(this);
        return true;
    }

    private void AlertNothingFound()
    {
        gm.console.FailPathfinderObjectifInateignable();
        gm.soundManager.PlayFailActionClip();
    }

    protected void AlertCantUseLocalisation() {
        if (gm.map.GetLumieresFinalesAndAlmostFinales().Count == 0) {
            gm.console.FailPathfinderUnauthorized();
        } else {
            gm.console.FailPathfinderInEndEvent();
        }
        gm.soundManager.PlayFailActionClip();
    }

    protected List<PathfinderData> GetAllPathfinderDatas() {
        // Datas
        List<PathfinderData> pathfinderDatas = new List<PathfinderData>();
        pathfinderDatas.Add(new PathfinderData(
            positions: GetAllLumieresPositions(),
            nbPositionsTheoretical: gm.map.GetLumieres().Count,
            shouldDetect: detectLumieres,
            orbesPathPrefab: lumiereOrbesPathPrefab,
            pathColor: lumierePathColor,
            geoData: lumiereGeoData,
            distanceMinToEnd: 0,
            stringIndice: 0,
            useReversedPathTechnique: true
        ));
        // Items
        Dictionary<Item.Type, List<Item>> itemsByType = GetItemsByTypes();
        foreach(KeyValuePair<Item.Type, List<Item>> pair in itemsByType) {
            GeoData newItemGeoData = new GeoData(itemGeoData);
            newItemGeoData.color = pair.Value[0].geoSphereColor;
            List<Vector3> positions = pair.Value.Select(i => i.transform.position).ToList();
            pathfinderDatas.Add(new PathfinderData(
                positions: positions,
                nbPositionsTheoretical: positions.Count,
                shouldDetect: detectItems,
                orbesPathPrefab: itemsOrbesPathPrefab,
                pathColor: pair.Value[0].pathColor,
                geoData: newItemGeoData,
                distanceMinToEnd: 0,
                stringIndice: 1,
                useReversedPathTechnique: true
            ));
        }
        // OrbTriggers
        pathfinderDatas.Add(new PathfinderData(
            positions: GetAllOrbTriggersPositions(),
            nbPositionsTheoretical: GetAllOrbTriggersPositions().Count,
            shouldDetect: detectOrbTriggers,
            orbesPathPrefab: orbTriggerPathPrefab,
            pathColor: orbTriggerPathColor,
            geoData: orbGeoData,
            distanceMinToEnd: 2,
            stringIndice: 2,
            useReversedPathTechnique: false
        ));

        pathfinderDatas = pathfinderDatas.FindAll(data => data.positions.Count > 0 && data.shouldDetect);

        for(int i = 0; i < pathfinderDatas.Count; i++) {
            pathfinderDatas[i].ComputeOrbeStartOffset(pathfinderDatas.Count, i);
        }
        return pathfinderDatas;
    }

    protected Dictionary<Item.Type, List<Item>> GetItemsByTypes() {
        Dictionary<Item.Type, List<Item>> itemsByType = new Dictionary<Item.Type, List<Item>>();
        foreach(Item item in gm.itemManager.GetItems()) {
            if(itemsByType.ContainsKey(item.type)) {
                itemsByType[item.type].Add(item);
            } else {
                itemsByType[item.type] = new List<Item>() { item };
            }
        }
        return itemsByType;
    }

    protected bool DrawPathToPositions(PathfinderData pathfinderData) {
        try {
            Vector3 nearestPosition = DrawPathToNearestPosition(pathfinderData);

            gm.console.RunPathfinder(nearestPosition);

            if (detectItems)
                NotifyOnlyVisibleOnTriggerItems();

            return true;
        } catch (System.Exception e) {
            //Debug.LogWarning($"Pouvoir Detection fails :\n{e.StackTrace}");
            return false;
        }
    }

    private Vector3 DrawPathToNearestPosition(PathfinderData pathfinderData) {
        //Vector3 nearestPosition = positions.OrderBy(p => Vector3.Distance(p, player.transform.position)).First();
        //List<Vector3> posToDodge = GetPosToDodge();
        //List<Vector3> shortestPath = GetPathForPosition(nearestPosition, posToDodge);
        List<List<Vector3>> pathsToPositions = new List<List<Vector3>>();
        List<Vector3> posToDodge = GetNonRegularCubesToDodge();
        List<Vector3> positions = pathfinderData.positions.OrderBy(p => Vector3.Distance(p, player.transform.position)).Take(3).ToList();
        foreach (Vector3 pos in positions) {
            List<Vector3> pathToPosition = GetPathForPosition(pos, posToDodge, pathfinderData);
            pathsToPositions.Add(pathToPosition);
        }
        List<List<Vector3>> notNullPaths = pathsToPositions.FindAll(p => p != null);
        if (notNullPaths.Count <= 0)
            throw new System.Exception("Aucune position accessible !");
        List<Vector3> shortestPath = notNullPaths.OrderBy(p => p.Count).First();
        Vector3 nearestPosition = positions[pathsToPositions.IndexOf(shortestPath)];

        DrawPathToPosition(nearestPosition, shortestPath, pathfinderData);
        return nearestPosition;
    }

    protected void DrawPathToPosition(Vector3 position, List<Vector3> path, PathfinderData pathfinderData) {
        if (path != null) {
            GeoData geoData = new GeoData(pathfinderData.geoData);
            geoData.SetTargetPosition(position);
            geoData.duration = ComputeDuration(path);
            player.geoSphere.AddGeoPoint(geoData);
            StartCoroutine(DrawPath(path, pathfinderData));
        } else {
            Debug.Log("Objectif inaccessible en " + position + " !");
        }
    }

    protected float ComputeDuration(List<Vector3> path) {
        return (1.0f / vitessePath) * NB_SPHERES_BY_NODES * path.Count + dureePath;
    }

    protected List<Vector3> GetPathForPosition(Vector3 position, List<Vector3> posToDodge, PathfinderData pathfinderData) {
        List<Vector3> path = gm.map.GetPath(
            // On inverse le Pathfinder pour éviter les cas où une data est enfermé dans une protection !
            start: pathfinderData.useReversedPathTechnique ? position : player.transform.position,
            end: pathfinderData.useReversedPathTechnique ? player.transform.position : position,
            posToDodge: posToDodge,
            bIsRandom: true,
            useNotInMapVoisins: true,
            ignoreSwappyCubes: true,
            distanceMinToEnd: pathfinderData.distanceMinToEnd);
        if (path != null && pathfinderData.useReversedPathTechnique) { // Et on inverse l'inversion ! :)
            path.Reverse();
        }
        if(pathfinderData.distanceMinToEnd > 0 && path != null && path.Last() != position) {
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

    protected IEnumerator DrawPath(List<Vector3> path, PathfinderData pathfinderData) {
        for(int i = 0; i < path.Count - 1; i++) {
            Vector3 current = path[i];
            Vector3 next = path[i + 1];
            for(int j = 0; j < NB_SPHERES_BY_NODES; j++) {
                Vector3 direction = next - current;
                Vector3 pos = current + direction / NB_SPHERES_BY_NODES * (j + 1);
                GameObject go = Instantiate(pathfinderData.orbesPathPrefab, pos, Quaternion.identity);
                Color color = gm.colorManager.GetColorForPosition(go.transform.position);
                color = Color.white - color;
                Material material = go.GetComponent<MeshRenderer>().material;
                material.color = color;
                material.SetColor("_EmissionColor", color);
                AutoColorBouncer colorBouncer = go.GetComponent<AutoColorBouncer>();
                colorBouncer.colorToBounceTo = pathfinderData.pathColor;
                colorBouncer.startingTime = colorBouncer.intervalTime * pathfinderData.orbeStartOffset;
                AutoBouncer bouncer = go.GetComponent<AutoBouncer>();
                bouncer.startingTime = bouncer.intervalTime * pathfinderData.orbeStartOffset;
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

    public override bool IsEnabled() {
        return pouvoirEnabled && !pouvoirFreezed; // We want to be able to use it while TimeHack !
    }
}
