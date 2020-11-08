using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PouvoirDetection : IPouvoir {

	public GameObject lumierePathPrefab; // Les lumières à placer quand le personnage fait une détection !
    public bool detectLumieres = true;
    public bool detectItems = true;
    public float dureePath = 3.0f;
    public float vitessePath = 30.0f;

    protected override bool UsePouvoir() {
		if (Input.GetKeyDown (KeyCode.A))
        {
            List<Vector3> positions = GetAllInterestPoints();

            if (!player.CanUseLocalisation() || positions.Count == 0) {
                gm.console.FailLocalisation();
                gm.soundManager.PlayFailActionClip();
                return false;
            }

            Vector3 nearestPosition = positions.OrderBy(p => Vector3.Distance(p, player.transform.position)).First();

            DrawPathToPosition(nearestPosition);

            gm.console.RunDetection(nearestPosition);

            if (detectItems)
                NotifyOnlyVisibleOnTriggerItems();
        }

        return true;
    }

    protected void DrawPathToPosition(Vector3 position) {
        List<Vector3> posToDodge = gm.map.GetAllNonRegularCubePos();
        for (int i = 0; i < posToDodge.Count; i++)
            posToDodge[i] = MathTools.Round(posToDodge[i]);
        List<Vector3> path = gm.map.GetPath(player.transform.position, position, posToDodge, bIsRandom: true);
        if (path != null)
            StartCoroutine(DrawPath(path));
        else
            Debug.Log("Objectif inaccessible en " + position + " !");
    }

    protected List<Vector3> GetAllInterestPoints() {
        List<Vector3> positions = new List<Vector3>();
        if (detectLumieres) {
            List<LumiereSwitchable> lumieresSwitchablesOn = gm.map.GetLumieresSwitchables();
            lumieresSwitchablesOn = lumieresSwitchablesOn.FindAll(ls => ls.GetState() == LumiereSwitchable.LumiereSwitchableState.ON);
            if(lumieresSwitchablesOn.Count > 0) {
                positions.AddRange(lumieresSwitchablesOn.Select(ls => ls.transform.position).ToList());
            } else {
                positions.AddRange(gm.map.GetAllLumieresPositions());
            }
        }
        if (detectItems) {
            positions.AddRange(gm.itemManager.GetItemsPositions());
        }
        return positions;
    }

    protected IEnumerator DrawPath(List<Vector3> path) {
        int nbSpheresByNodes = 4;
        for(int i = 0; i < path.Count - 1; i++) {
            Vector3 current = path[i];
            Vector3 next = path[i + 1];
            for(int j = 0; j < nbSpheresByNodes; j++) {
                Vector3 direction = next - current;
                Vector3 pos = current + direction / nbSpheresByNodes * (j + 1);
                GameObject go = Instantiate(lumierePathPrefab, pos, Quaternion.identity);
                Color color = gm.colorManager.GetColorForPosition(go.transform.position);
                color = Color.white - color;
                Material material = go.GetComponent<MeshRenderer>().material;
                material.color = color;
                material.SetColor("_EmissionColor", color);
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
