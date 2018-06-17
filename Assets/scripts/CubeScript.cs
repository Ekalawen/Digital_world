﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeScript : MonoBehaviour {

	//////////////////////////////////////////////////////////////////////////////////////
	// STATIC
	//////////////////////////////////////////////////////////////////////////////////////

	public enum CubeType {Source, Recepteur};
	public static float probaSource = 0.02f;
	public static float distSourceMax = 5f;

	//////////////////////////////////////////////////////////////////////////////////////
	// ATTRIBUTS PUBLIQUES
	//////////////////////////////////////////////////////////////////////////////////////

	public CubeType type;
	public Color couleur;

	//////////////////////////////////////////////////////////////////////////////////////
	// METHODES
	//////////////////////////////////////////////////////////////////////////////////////

	void Awake () {
		// De base, tous les cubes sont des récepteurs avec une probabilité 1-p !
		if (Random.Range (0f, 1f) <= probaSource) {
			type = CubeType.Source;
			// Si le cube est une source, on va changer sa couleur !
			Color c = Color.HSVToRGB(Random.Range(0f, 1f), 0.8f, 1f);
			couleur = c;
			GetComponent<MeshRenderer> ().material.color = c;
		} else {
			type = CubeType.Recepteur;
			couleur = Color.black;
			GetComponent<MeshRenderer> ().material.color = Color.black;
		}
	}
	
	void Start () {
		// La couleur des récepteurs se met à jour en fonction des sources qui sont suffisamment proches d'eux !
		// On récupère toutes les sources à moins de distSourceMax
		if (type == CubeType.Recepteur) {
			Collider[] colliders = Physics.OverlapSphere(this.transform.position, distSourceMax);
			foreach (Collider collider in colliders) {
				if (collider.tag == "Cube") {
					CubeScript c = collider.gameObject.GetComponent<CubeScript> () as CubeScript;				
					if (c.type == CubeType.Source) {
						float distance = Vector3.Distance (c.transform.position, this.transform.position);
						if (distance < distSourceMax) {
							float affaiblissement = 1f - distance / distSourceMax;
							GetComponent<MeshRenderer> ().material.color += (c.couleur * affaiblissement);
						}
					}
				}
			}
		}
	}
}
