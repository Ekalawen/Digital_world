using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tracer : Ennemi {

    public enum TracerState { WAITING, RUSHING, EMITING };

    public float dureePauseEntreNodes = 0.1f;
    public GameObject explosionParticlesPrefab;

    protected TracerState state;
    protected List<Vector3> path;
    protected float lastTimePause;

    protected bool bIsStuck = false;
    protected Vector3 lastPosition;
    protected float debutStuck;
    protected float dureeMaxStuck = 0.1f;

    public override void Start() {
        base.Start();
        state = TracerState.WAITING;
        lastTimePause = Time.timeSinceLevelLoad;
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
                    lastTimePause = Time.timeSinceLevelLoad;
                }
                if(path.Count == 0) {
                    state = TracerState.WAITING;
                    return;
                }

                // On va au premier point du chemin !
                if (Time.timeSinceLevelLoad - lastTimePause > dureePauseEntreNodes) {
                    Vector3 move = Move(path[0]);

                    // On essaye de se débloquer si on est bloqué !
                    TryUnStuck();
                }
                break;
        }
        lastPosition = transform.position;
	}

    protected void TryUnStuck() {
        if(transform.position == lastPosition) {
            if (!bIsStuck) {
                debutStuck = Time.timeSinceLevelLoad;
                bIsStuck = true;
            }
            if(bIsStuck && Time.timeSinceLevelLoad - debutStuck > dureeMaxStuck) {
                ComputePath(path[path.Count - 1]); // On va au même endroit que précédemment !
                bIsStuck = false;
            }
        }
    }

    void GetEtat() {
        if(state == TracerState.WAITING) {
            if (IsPlayerVisible()) {
                state = TracerState.RUSHING;
                ComputePath(player.transform.position);
            }
        }
    }

    void ComputePath(Vector3 end) {
        Vector3 start = MathTools.Round(transform.position);
        end = MathTools.Round(end);
        List<Vector3> posToDodge = gm.ennemiManager.GetAllRoundedEnnemisPositions();
        path = gm.map.GetPath(start, end, posToDodge, bIsRandom: true);
        if(path == null) {
            state = TracerState.WAITING;
        }
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
