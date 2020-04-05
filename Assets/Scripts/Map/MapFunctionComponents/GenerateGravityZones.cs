using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateGravityZones : MapFunctionComponent {

    public GameObject gravityZonePrefab;
    public int nbGravityZones = 1;
    public Vector2Int halfExtentsRange = new Vector2Int(3, 6);

    public override void Activate() {
        // Générer des gravityZones
        for (int i = 0; i < nbGravityZones; i++) {
            Vector3 pos = map.GetRoundedLocation();
            Vector3 halfExtents = new Vector3(Random.Range(halfExtentsRange[0], halfExtentsRange[1]),
                Random.Range(halfExtentsRange[0], halfExtentsRange[1]),
                Random.Range(halfExtentsRange[0], halfExtentsRange[1])) + Vector3.one * 0.49f;
            GravityZone zone = Instantiate(gravityZonePrefab, pos, Quaternion.identity).GetComponent<GravityZone>();
            zone.Resize(pos, halfExtents);
        }
    }
}
