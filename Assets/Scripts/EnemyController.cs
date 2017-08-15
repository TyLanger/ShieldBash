using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour {

	Transform player;
	public float moveSpeed = 0.5f;

	bool attacking = false;
	float attackTime = 0.05f;
	float timeOfAttackStart = 0;
	float timeBetweenAttacks = 0.75f;
	float timeOfLastAttack = 0;
	float damageDelayTime = 0.5f;
	int attackDamage = 40;

	bool randomWalk = true;
	float timeBetweenRandomWalks = 2;
	float timeOfNextRandomWalk = 0;
	float randomAngle = 0;
	Vector3 randomPoint;
	Vector3 spawnPoint;
	float spawnRadius = 3;

	public Transform sword;
	Animator swordAnim;
	public GameObject swordArc;

	Health health;

	// Use this for initialization
	void Start () {
		player = FindObjectOfType<PlayerController> ().transform;
		spawnPoint = transform.position;
		swordAnim = sword.GetComponent<Animator> ();
		health = GetComponent<Health> ();
		health.onDeath += die;
	}
	
	// Update is called once per frame
	void FixedUpdate () {



		if (attacking) {
			// if attacking, stop moving
			// need to wait for attackTime seconds before you can start the swing
			if (Time.time > timeOfAttackStart + attackTime) {
				if (Time.time > timeOfLastAttack + timeBetweenAttacks) {
					timeOfLastAttack = Time.time;
					// attack;
					//Debug.Log ("Attacking!");
					Invoke("activateAttackArc", damageDelayTime);
					swordAnim.SetTrigger ("SwordTrigger");
					//attacking = false;
				}
			}
		} else {
			
			// default to random walk when the player isn't nearby. This is governed by the EnemyAI
			if (randomWalk) {
				transform.LookAt (randomPoint);
				if (Time.time > timeOfNextRandomWalk) {
					// every timeBetweenRandomWalks (default 2 seconds) find a random point near the spawn point.
					// move towards that point
					timeOfNextRandomWalk = Time.time + timeBetweenRandomWalks;
					// pick a random angle
					randomAngle = Random.Range (0, 2 * Mathf.PI);
					// turn that random angle into a position on the unit circle
					// multiply by a radius to get a point inside the circle of the spawn point.
					// the spawn circle is a circle of radius spawnRadius around the spawn point
					randomPoint = spawnPoint + new Vector3 (Mathf.Cos (randomAngle), 0, Mathf.Sin (randomAngle)) * Random.Range (0, spawnRadius);
				}
				
				Debug.DrawLine (transform.position, randomPoint);
				transform.position = Vector3.MoveTowards (transform.position, randomPoint, moveSpeed);
			} else {
				// only look at the player when moving
				// and only when moving towards player
				transform.LookAt (player);
				transform.position = Vector3.MoveTowards (transform.position, player.transform.position, moveSpeed);
			}
		}

	}

	public void die()
	{
		transform.position = spawnPoint;
		health.resetHealth ();
	}

	public void playerFound()
	{
		randomWalk = false;
	}

	public void playerLost()
	{
		randomWalk = true;
	}

	public void attackPlayer()
	{
		if (!attacking) {
			attacking = true;
			timeOfAttackStart = Time.time;
		}
	}

	public void stopAttacking()
	{
		attacking = false;
		disableAttackArc ();
	}

	void activateAttackArc()
	{
		swordArc.SetActive(true);
		Invoke ("disableAttackArc", 0.1f);
	}

	void disableAttackArc()
	{
		swordArc.SetActive(false);
	}

	void OnDrawGizmos()
	{
		//Gizmos.DrawWireSphere (spawnPoint, spawnRadius);
		UnityEditor.Handles.DrawWireDisc (spawnPoint, Vector3.up, spawnRadius);


	}

	public void OnChildTriggerEnter(Collider col) {
		if (col.GetComponent<Health> () != null) {
			// hit player
			player.GetComponent<Health>().takeDamage(attackDamage);
		}
	}

	public void OnChildTriggerExit(Collider col) {

	}

	public void OnChildTriggerStay(Collider col) {

	}
}
