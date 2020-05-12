﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PouvoirDetection : IPouvoir {

	public GameObject lumierePathPrefab; // Les lumières à placer quand le personnage fait une détection !
    public bool detectLumieres = true;
    public bool detectItems = true;
    public float dureePath = 3.0f;
    public float vitessePath = 30.0f;

    protected override bool UsePouvoir() {
		if (Input.GetKeyDown (KeyCode.A)) {
            // Penser à ajouter les autres objectifs intéressants !
            List<Vector3> positions = new List<Vector3>();
            if (detectLumieres)
                positions.AddRange(gm.map.GetAllLumieresPositions());
            if (detectItems)
                positions.AddRange(gm.itemManager.GetItemsPositions());

            if (!player.CanUseLocalisation() || positions.Count == 0) {
                gm.console.FailLocalisation();
                gm.soundManager.PlayFailActionClip();
                return false;
            }

            // On choisit la position la plus proche
            Vector3 nearestPosition = positions[0];
            float distMin = Vector3.Distance(nearestPosition, player.transform.position);
            foreach(Vector3 position in positions) {
                float dist = Vector3.Distance(position, player.transform.position);
                if(dist < distMin) {
                    distMin = dist;
                    nearestPosition = position;
                }
            }

            // On trace le chemin
            List<Vector3> posToDodge = gm.map.GetAllNonRegularCubePos();
            for (int i = 0; i < posToDodge.Count; i++)
                posToDodge[i] = MathTools.Round(posToDodge[i]);
            List<Vector3> path = gm.map.GetPath(player.transform.position, nearestPosition, posToDodge, bIsRandom: true);
            if (path != null)
                StartCoroutine(DrawPath(path));
            else
                Debug.Log("Objectif inaccessible en " + nearestPosition + " !");

			// Un petit message
			gm.console.RunDetection(nearestPosition);

            if (detectItems)
                NotifyOnlyVisibleOnTriggerItems();
        }

        return true;
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
                go.GetComponent<MeshRenderer>().material.color = color;
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
