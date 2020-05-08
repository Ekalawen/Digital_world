using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstBoss : Sonde {

    public float periodeAttacks = 2.0f;
    public GameObject attacksPrefab;

    protected List<Sonde> satellites;
    protected Timer timerAttacks;
    protected Transform projectilesFolder;

	public override void Start () {
        base.Start();
        name = "FirstBoss";
        satellites = new List<Sonde>(GetComponentsInChildren<Sonde>());
        satellites.Remove(this);
        timerAttacks = new Timer(periodeAttacks);
        projectilesFolder = new GameObject("Projectiles").transform;
        projectilesFolder.transform.parent = transform;
    }

    public override void UpdateSpecific () {
        base.UpdateSpecific();

        if (Time.timeSinceLevelLoad < GetComponent<IController>().tempsInactifDebutJeu)
            return;

        if (timerAttacks.IsOver()) {
            timerAttacks.Reset();
            Attack();
        }
	}

    public void Attack() {
        //Sonde sonde = satellites[Random.Range(0, satellites.Count)];
        //satellites.Remove(sonde);
        //Destroy(sonde.GetComponent<IController>());
        //GoToDirectionController newController = sonde.gameObject.AddComponent<GoToDirectionController>();
        //newController.direction = player.transform.position - sonde.transform.position;
        //newController.vitesse = attacksVitesse;
        Vector3 direction = (player.transform.position - transform.position).normalized;
        Vector3 spawn = transform.position + direction * transform.localScale[0];
        GoToDirectionController projectile = Instantiate(attacksPrefab, spawn, Quaternion.identity, projectilesFolder).GetComponent<GoToDirectionController>();
        projectile.direction = direction;
    }
}
