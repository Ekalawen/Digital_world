using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tracer : Ennemi {

    public enum TracerState { WAITING, RUSHING, EMITING };

    public float dureeEmiting = 4.0f;
    public float rangeEmiting = 5.0f;
    public float forceEmiting = 1.0f;
    public GameObject explosionParticlesPrefab;
    public Material emissiveMaterial;

    protected Material normalMaterial;
    protected ParticleSystem emitionParticleSystem;
    protected float debutEmiting;
    protected Poussee pousseeEmiting = null;
    protected Coroutine emitingCoroutine = null;
    protected AudioSource audioSource = null;

    public override void Start() {
        base.Start();
        emitionParticleSystem = GetComponentInChildren<ParticleSystem>(includeInactive: true);
        normalMaterial = gameObject.GetComponent<MeshRenderer>().material;
    }

    public override void UpdateSpecific() {
    }

	void OnControllerColliderHit(ControllerColliderHit hit) {
        Cube cube = hit.collider.gameObject.GetComponent<Cube>();
        Player player = hit.collider.gameObject.GetComponent<Player>();
        GameManager gm = GameManager.Instance;

        if(cube != null) {
            if (!cube.bIsRegular) {
                cube.Explode();
                //GameObject go = Instantiate(explosionParticlesPrefab, cube.transform.position, Quaternion.identity);
                //ParticleSystem particle = go.GetComponent<ParticleSystem>();
                //ParticleSystemRenderer psr = go.GetComponent<ParticleSystemRenderer>();
                //Material mat = psr.material;
                //Material newMaterial = new Material(mat);
                //newMaterial.color = cube.GetColor();
                //psr.material = newMaterial;
                //float particuleTime = particle.main.duration;
                //Destroy(go, particuleTime);
                //gm.map.DeleteCube(cube);
            }
        }

        if (player != null) {
            HitPlayer();
        }
    }

    protected override void HitPlayerSpecific() {
    }

    protected override void HitContinuousPlayerSpecific() {
    }

    public void StartEmiting() {
        Debug.Log("StartEmiting()");
        emitingCoroutine = StartCoroutine(CStartEmiting());
    }

    protected IEnumerator CStartEmiting() {
        Debug.Log("CStartEmiting()");
        debutEmiting = Time.timeSinceLevelLoad;
        emitionParticleSystem.gameObject.SetActive(true);
        ParticleSystem.ShapeModule shape = emitionParticleSystem.shape;
        shape.radius = rangeEmiting;
        GetComponent<MeshRenderer>().material = emissiveMaterial;
        audioSource = gm.soundManager.PlayEmissionTracerClip(transform.position, transform);

        while (true) {
            AttractPlayer();
            yield return null;
        }
    }

    public void StopEmiting() {
        if (emitingCoroutine != null) {
            StopCoroutine(emitingCoroutine);
        }
        emitionParticleSystem.gameObject.SetActive(false);
        GetComponent<MeshRenderer>().material = normalMaterial;
        pousseeEmiting = null;
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }

    protected void AttractPlayer() {
        float distance = Vector3.Distance(transform.position, player.transform.position);
        if (distance <= rangeEmiting + 0.5f) {
            Vector3 direction = transform.position - player.transform.position;
            if (pousseeEmiting == null) {
                float dureeRestante = dureeEmiting - (Time.timeSinceLevelLoad - debutEmiting);
                pousseeEmiting = Poussee.CreatePoussee(direction, dureeRestante, forceEmiting);
                player.AddPoussee(pousseeEmiting);
            } else {
                pousseeEmiting.Redirect(direction);
            }
            HitPlayer();
        } else {
            if(pousseeEmiting != null) {
                pousseeEmiting.Stop();
                pousseeEmiting = null;
            }
        }
    }

    public override void PlayHitSound() {
        gm.soundManager.PlayHitTracerClip(transform.position, transform);
    }

    public override void DisplayHitMessage() {
        if (!gm.partieDejaTerminee) {
            gm.console.JoueurToucheTracer();
        }
    }
}
