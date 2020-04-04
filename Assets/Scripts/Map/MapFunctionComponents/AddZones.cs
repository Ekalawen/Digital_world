using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddZones : MapFunctionComponent {

    public List<GameObject> zonesPrefabs;
    public List<int> nbZones;
    public Vector3Int offsetHautBasCotes = new Vector3Int(1, 1, 1);
    public Vector2Int sizeRange = new Vector2Int(2, 6);
    public bool extentsAreEqual = true;

    public override void Activate() {
        for(int i = 0; i < zonesPrefabs.Count; i++) {
            for(int j = 0; j < nbZones[i]; j++) {
                CreateZone(zonesPrefabs[i]);
            }
        }
    }

    public void CreateZone(GameObject zonePrefab) {
        Vector3 halfExtents = ComputeHalfExtents();
        Vector3 position = ComputePosition(halfExtents);
        IZone zone = Instantiate(zonePrefab, position, Quaternion.identity, map.zonesFolder.transform).GetComponent<IZone>();
    }

    protected Vector3 ComputeHalfExtents() {
        if(extentsAreEqual) {
            int value = Random.Range(sizeRange[0], sizeRange[1]);
            return Vector3.one * value / 2.0f;
        } else {
            float x = Random.Range(sizeRange[0], sizeRange[1]) / 2.0f;
            float y = Random.Range(sizeRange[0], sizeRange[1]) / 2.0f;
            float z = Random.Range(sizeRange[0], sizeRange[1]) / 2.0f;
            return new Vector3(x, y, z);
        }
    }

    protected Vector3 ComputePosition(Vector3 halfExtents) {
        while(true) {
            Vector3 pos = map.GetFreeRoundedLocation();
            pos = AlignInRoundedPosition(pos, halfExtents);
            if (IsValidPos(pos, halfExtents))
                return pos;
        }
    }

    private Vector3 AlignInRoundedPosition(Vector3 pos, Vector3 halfExtents) {
        if(Mathf.Round(halfExtents.x) != halfExtents.x)
            pos.x += 0.5f * MathTools.RandomSign();
        if(Mathf.Round(halfExtents.y) != halfExtents.y)
            pos.y += 0.5f * MathTools.RandomSign();
        if(Mathf.Round(halfExtents.z) != halfExtents.z)
            pos.z += 0.5f * MathTools.RandomSign();
        return pos;
    }

    protected bool IsValidPos(Vector3 pos, Vector3 halfExtents) {
        return pos.x - halfExtents.x >= offsetHautBasCotes[2]
            && pos.x + halfExtents.x <= map.tailleMap.x - offsetHautBasCotes[2]
            && pos.y - halfExtents.y >= offsetHautBasCotes[1]
            && pos.y + halfExtents.y <= map.tailleMap.x - offsetHautBasCotes[0]
            && pos.z - halfExtents.z >= offsetHautBasCotes[2]
            && pos.z + halfExtents.z <= map.tailleMap.x - offsetHautBasCotes[2];
    }
}
