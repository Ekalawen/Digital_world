using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Permet de générer une carte pleine de spikes !
public class FullSpikesMap : MapManager {

    public int offsetBetweenSpikes = 1;
    public bool bGoToEnd = false;
    public int spikesMaxRange = 25;

    [HideInInspector] public int volumeMap;

    protected override void GenerateMap()
    {
        volumeMap = (int)GetVolume();
        GenerateFullSpikesMap();
    }

    // Crée une map en forme de Cube
    void GenerateFullSpikesMap()
    {
        // On crée le contour de la map !
        currentCubeTypeUsed = Cube.CubeType.INDESTRUCTIBLE;
        MapContainer mapContainer = new MapContainer(Vector3.zero, new Vector3(tailleMap.x, tailleMap.y, tailleMap.z) + Vector3.one);

        // On crée les 6 bases de spikes, une pour chaque mur
        currentCubeTypeUsed = Cube.CubeType.NORMAL;
        List<Spikes> spikesGenerators = new List<Spikes>();
        foreach (Mur mur in mapContainer.GetMurs()) {
            Spikes spikes = Spikes.GenerateSpikesFromMur(mur, offsetBetweenSpikes, bGoToEnd, spikesMaxRange, GetCenter());
            spikesGenerators.Add(spikes);
        }

        // Puis tant que l'on peut on génère des spikes aléatoirement !
        while(spikesGenerators.Count > 0) {
            int indSpikeGenerator = Random.Range(0, spikesGenerators.Count);
            Spikes spikes = spikesGenerators[indSpikeGenerator];
            if(spikes.HasStarts()) {
                spikes.GenerateASpike();
            } else {
                spikesGenerators.Remove(spikes);
            }
        }
    }
}