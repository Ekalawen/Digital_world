using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateSpikesEverywhere : GenerateCubesMapFunction {

    public int offsetBetweenSpikes = 1;
    public bool bGoToEnd = false;
    public int spikesMaxRange = 25;

    public override void Activate() {
        GenerateSpikes();
    }

    protected void GenerateSpikes() {
        List<Spikes> spikesGenerators = new List<Spikes>();
        MapContainer mapContainer = map.GetMapElementsOfType<MapContainer>()[0]; // Assertion bolzy !
        foreach (Mur mur in mapContainer.GetMurs()) {
            Spikes spikes = Spikes.GenerateSpikesFromMur(mur, offsetBetweenSpikes, bGoToEnd, spikesMaxRange, map.GetCenter());
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
