using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tracer : Ennemi {

    public enum TracerState { WAITING, RUSHING, EMITING };

    public GameObject explosionParticlesPrefab;

    protected TracerState state;
    protected List<Vector3> path;

    public override void Start() {
        base.Start();
        state = TracerState.WAITING;
    }

    public override void UpdateSpecific () {
        GetEtat();

        switch(state)
        {
            case TracerState.WAITING:
                break;
            case TracerState.RUSHING:
                if(Vector3.Distance(transform.position, path[0]) < 0.001f) {
                    path.RemoveAt(0);
                }
                if(path.Count == 0) {
                    state = TracerState.WAITING;
                    return;
                }

                // On va au premier point du chemin !
                Vector3 move = Move(path[0]);
                break;
        }
	}

    void GetEtat() {
        if(state == TracerState.WAITING) {
            if (IsPlayerVisible()) {
                state = TracerState.RUSHING;
                ComputePath();
            }
        }
    }

    void ComputePath() {
        Vector3 start = MathTools.Round(transform.position);
        Vector3 end = MathTools.Round(player.transform.position);
        path = gm.map.GetPath(start, end);
    }

	void OnControllerColliderHit(ControllerColliderHit hit) {
        Cube cube = hit.collider.gameObject.GetComponent<Cube>();
        Player player = hit.collider.gameObject.GetComponent<Player>();
        GameManager gm = GameManager.Instance;

        if(cube != null) {
            if (!cube.bIsRegular) {
                GameObject go = Instantiate(explosionParticlesPrefab, cube.transform.position, Quaternion.identity);
                ParticleSystem particle = go.GetComponent<ParticleSystem>();
                ParticleSystemRenderer psr = go.GetComponent<ParticleSystemRenderer>();
                Material mat = psr.material;
                Material newMaterial = new Material(mat);
                newMaterial.color = cube.GetColor();
                psr.material = newMaterial;
                float particuleTime = particle.main.duration;
                Destroy(go, particuleTime);
                gm.map.DeleteCube(cube);
            }
        }

		if (player != null) {
            HitPlayer();
		}
	}

    protected void HitPlayer() {
    }
}
