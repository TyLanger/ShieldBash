﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {


	public float moveSpeed = 0.5f;
	Vector3 moveDirection;
	Vector3 spawnPoint;

	public float timeBetweenAttacks = 0.25f;
	float timeOfLastAttack = 0;

	public Transform shield;
	public Transform shieldLeft;
	public Transform shieldRight;

	int shieldDamage = 50;

	Animator shieldAnim;
	public Animation shieldSmashAnim;

	Health myHealth;
	int maxHealth = 100;



	// Use this for initialization
	void Start () {
		shieldAnim = shield.GetComponent<Animator> ();
		spawnPoint = transform.position;
		myHealth = GetComponent<Health> ();
		// when the health component decides the player dies, get it to call die in this script
		myHealth.onDeath += die;
		myHealth.setMaxHealth (maxHealth);
	}

	void Update() {

		if (Input.GetButtonDown ("Fire1")) {
			// left click
			if (Time.time > timeOfLastAttack + timeBetweenAttacks) {
				timeOfLastAttack = Time.time;
				shieldBash ();
			}
		}
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
		transform.position = Vector3.MoveTowards(transform.position, transform.position + moveDirection, moveSpeed);

		Ray cameraRay = Camera.main.ScreenPointToRay (Input.mousePosition);
		Plane eyePlane = new Plane(Vector3.up, new Vector3(0, 0, 0));
		float cameraDist;

		if (eyePlane.Raycast (cameraRay, out cameraDist)) {
			Vector3 lookPoint = cameraRay.GetPoint (cameraDist);
			// use the tansform pos instead of the look point for the y so the player doesn't try to look down.
			transform.LookAt (new Vector3(lookPoint.x, transform.position.y, lookPoint.z));
		}


	}

	void shieldBash()
	{

		shieldAnim.SetTrigger ("smashTrigger");

		// fire out rays from the shield to see if it hits anything
		RaycastHit hit;
		Vector3 rayOrigin = shield.transform.position;

		// Fire forward from the direction the shield is facing
		Debug.DrawRay (rayOrigin, shield.transform.forward * 3.0f, Color.red);
		Debug.DrawRay (shieldLeft.position, shield.transform.forward * 3.0f, Color.green);
		Debug.DrawRay (shieldRight.position, shield.transform.forward * 3.0f, Color.blue);

		// if any of the rays hits something
		// seems to work
		// The one that hits should return true and continue the if. It should also be the one that stored something in hit last
		if (Physics.Raycast (rayOrigin, shield.transform.forward, out hit, 3.0f) || Physics.Raycast (shieldLeft.position, shield.transform.forward, out hit, 3.0f) || Physics.Raycast (shieldRight.position, shield.transform.forward, out hit, 3.0f)) {
			//Debug.Log ("Ray hit something "+hit.collider.ToString());
			/*
			EnemyController enemyHit = hit.transform.GetComponent<EnemyController> ();
			if (enemyHit != null) {
				enemyHit.die ();
			}*/
			if (hit.transform.GetComponent<Health> () != null) {
				hit.transform.GetComponent<Health> ().takeDamage (shieldDamage);
			}
		}

		//shieldAnim.SetBool ("isSmashing", false);

	}

	public void die()
	{
		transform.position = spawnPoint;
		myHealth.resetHealth ();
	}

}