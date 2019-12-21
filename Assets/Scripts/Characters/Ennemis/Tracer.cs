using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tracer : Ennemi {

    public enum TracerState { WAITING, RUSHING, EMITING };

    public float dureeEmiting = 4.0f;
    public float rangeEmiting = 5.0f;
    public float forceEmiting = 1.0f;
    public float dureePauseEntreNodes = 0.1f;
    public GameObject explosionParticlesPrefab;
    public Material emissiveMaterial;

    protected TracerState state;
    protected List<Vector3> path;
    protected float lastTimePause;

    protected bool bIsStuck = false;
    protected Vector3 lastPosition;
    protected float debutStuck;
    protected float dureeMaxStuck = 0.1f;

    protected Material normalMaterial;
    protected ParticleSystem emitionParticleSystem;
    protected float debutEmiting;
    protected Poussee pousseeEmiting = null;

    public override void Start() {
        base.Start();
        SetState(TracerState.WAITING);
        lastTimePause = Time.timeSinceLevelLoad;
        emitionParticleSystem = GetComponentInChildren<ParticleSystem>(includeInactive: true);
        normalMaterial = gameObject.GetComponent<MeshRenderer>().material;
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
                    SetState(TracerState.EMITING);
                    return;
                }

                // On va au premier point du chemin !
                if (Time.timeSinceLevelLoad - lastTimePause > dureePauseEntreNodes) {
                    Vector3 move = Move(path[0]);

                    // On essaye de se débloquer si on est bloqué !
                    TryUnStuck();
                }
                break;
            case TracerState.EMITING:
                float distance = Vector3.Distance(transform.position, player.transform.position);
                if (distance <= rangeEmiting + 0.5f) {
                    if (pousseeEmiting == null) {
                        Vector3 direction = transform.position - player.transform.position;
                        float dureeRestante = dureeEmiting - (Time.timeSinceLevelLoad - debutEmiting);
                        pousseeEmiting = Poussee.CreatePoussee(direction, dureeRestante, forceEmiting);
                        player.AddPoussee(pousseeEmiting);
                    } else {
                        Vector3 direction = transform.position - player.transform.position;
                        pousseeEmiting.Redirect(direction);
                    }
                } else {
                    if(pousseeEmiting != null) {
                        pousseeEmiting.Stop();
                        pousseeEmiting = null;
                    }
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
                SetState(TracerState.RUSHING);
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
            SetState(TracerState.EMITING);
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

    protected override void HitPlayerSpecific() {
    }

    protected void SetState(TracerState newState) {
        TracerState oldState = state;
        state = newState;
        if(newState == TracerState.EMITING && oldState != TracerState.EMITING) {
            StartCoroutine(StartEmiting());
        }
        if (newState != TracerState.EMITING)
            pousseeEmiting = null;
        if (newState == TracerState.RUSHING && oldState != TracerState.RUSHING)
            gm.soundManager.PlayDetectionClip(GetComponentInChildren<AudioSource>());
    }

    protected IEnumerator StartEmiting() {
        debutEmiting = Time.timeSinceLevelLoad;
        emitionParticleSystem.gameObject.SetActive(true);
        ParticleSystem.ShapeModule shape = emitionParticleSystem.shape;
        shape.radius = rangeEmiting;
        GetComponent<MeshRenderer>().material = emissiveMaterial;
        gm.soundManager.PlayEmissionTracerClip(GetComponentInChildren<AudioSource>(), dureeEmiting);

        yield return new WaitForSeconds(dureeEmiting);

        StopEmiting();
    }
    protected void StopEmiting() {
        emitionParticleSystem.gameObject.SetActive(false);
        GetComponent<MeshRenderer>().material = normalMaterial;
        SetState(TracerState.WAITING);
    }

    public override bool IsInactive() {
        return state == TracerState.WAITING;
    }
}
