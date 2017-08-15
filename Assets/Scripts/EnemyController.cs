using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour {

	public delegate void deathDelegate();
	public deathDelegate onDeath;

	Transform player;
	public float moveSpeed = 0.5f;


	public bool attacking = false;
	public bool rootedForAttack = false;
	float attackWindUpTime = 0.05f;
	float timeOfAttackStart = 0;
	float timeBetweenAttacks = 0.85f;
	float timeOfNextAttack = 0;
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



		if (rootedForAttack) {
			transform.LookAt (player);
			// if attacking, stop moving

			// moving this to its own function so it can change for different enemies
			// need to wait for attackWindUpTime seconds before you can start the swing
			/*
			if (Time.time > timeOfAttackStart + attackWindUpTime) {
				if (Time.time > timeOfNextAttack) {
					timeOfNextAttack = Time.time + timeBetweenAttacks;
					// attack;
					//Debug.Log ("Attacking!");
					Invoke("activateAttackArc", damageDelayTime);
					swordAnim.SetTrigger ("SwordTrigger");
					//attacking = false;
				}
			}
			*/
		} else if(attacking) {
			
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
		attacking = false;
		rootedForAttack = false;
		health.resetHealth ();
		onDeath ();
	}

	public void playerFound()
	{
		randomWalk = false;
	}

	public void playerLost()
	{
		randomWalk = true;
	}

	bool canAttack()
	{
		return Time.time > timeOfNextAttack;
	}

	public void attackPlayer()
	{
		if (canAttack()) {
			timeOfNextAttack = Time.time + timeBetweenAttacks;
			// attacking = true;
			rootedForAttack = true;
			// set attacking to true so the enemy stops to try to attack the player
			// attacking gets set to false when the player moves out of the attack range
			// then the enemy can move again
			//timeOfAttackStart = Time.time;

			Invoke ("startAttack", attackWindUpTime);
			// stop moving to cast attack for attackWindUpTime seconds
			// Start animation
			// swordAnim.SetTrigger ("SwordTrigger");
			// Invoke damage trigger
			// Invoke("activateAttackArc", damageDelayTime);
		}
	}

	void startAttack()
	{
		
		// stop moving to cast attack for attackWindUpTime seconds
		// Start animation
		attacking = true;
		rootedForAttack = false;
		swordAnim.SetTrigger ("SwordTrigger");
		// Invoke damage trigger
		Invoke("activateAttackArc", damageDelayTime);
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
		// attack is finished, stop attacking so you can move again or queue up another attack

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
