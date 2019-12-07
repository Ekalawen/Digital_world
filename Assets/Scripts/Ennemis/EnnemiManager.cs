using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnnemiManager : MonoBehaviour {


    //// public enum EtatDataBase {NORMAL, DEFENDING};

    public GameObject sondePrefab; // On récupère la sonde !
    public float proportionSondes;

    [HideInInspector]
    public List<Sonde> sondes; // Elle connait toutes les sondes

    protected GameManager gm;

    public void Initialize() {
        gm = GameManager.Instance;

        GenerateEnnemies();
    }

    void GenerateEnnemies() {
        sondes = new List<Sonde>();
        int nbSondes = (int)Mathf.Ceil(gm.map.GetVolume() * proportionSondes);
        for (int i = 0; i < nbSondes; i++) {
            Vector3 posSonde = gm.map.GetFreeSphereLocation(1.0f);
            Sonde sonde = Instantiate(sondePrefab, posSonde, Quaternion.identity).GetComponent<Sonde>();
            sondes.Add(sonde);
        }
    }

}
