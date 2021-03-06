﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MovementController {

	/* in parent
	public float moveSpeed = 0.5f;
	Vector3 moveDirection;
	*/
	Vector3 spawnPoint;



	Health myHealth;
	int maxHealth = 100;

	bool usingAbility = false;

	public Ability m1Ability;
	public Ability m2Ability;
	public Ability qAbility;
	public Ability spaceAbility;
	public Ability eAbility;
	public Ability rAbility;
	public Ability fAbility;

	Vector3 lookPoint;
	// lock the ability to turn the player while using abilities
	bool canLook = true;

	// Use this for initialization
	protected override void Start () {
		base.Start ();

		spawnPoint = transform.position;
		myHealth = GetComponent<Health> ();
		// when the health component decides the player dies, get it to call die in this script
		myHealth.onDeath += die;
		//myHealth.onDamage += tookDamage;
		myHealth.setMaxHealth (maxHealth);
	}

	void Update() {

		if (Input.GetButtonDown ("Fire1")) {
			// left click

			if (!usingAbility) {
				StartAbility (0);
				m1Ability.useAbility (transform, lookPoint);
			}

		}
		if(Input.GetButtonDown("Fire2")) {
			// right click

			if (!usingAbility) {
				StartAbility (1);
				m2Ability.useAbility (transform, lookPoint);
			}

		}
		if (Input.GetButtonDown ("Ability1") ) {
			// default q
			// set the point where it should spawn and the place it should travel to
			if (!usingAbility) {
				StartAbility (2);
				qAbility.useAbility (transform, lookPoint);
			}
		}
		if (Input.GetButtonDown ("Jump") ) {
			// default space

			if (!usingAbility) {
				StartAbility (3);
				spaceAbility.useAbility (transform, lookPoint);
			}
		}
		if (Input.GetButtonDown ("Ability2")) {
			// default e
			if (!usingAbility) {
				StartAbility (4);
				eAbility.useAbility (transform, lookPoint);
			}
		}
		if (Input.GetButtonDown ("Ability3")) {
			// default r
			if (!usingAbility) {
				StartAbility (5);
				rAbility.useAbility (transform, lookPoint);
			}
		}
		if (Input.GetButtonDown ("Ability4")) {
			// default f
			if (!usingAbility) {
				StartAbility (6);
				fAbility.useAbility (transform, lookPoint);
			}
		}
	}
	
	// Update is called once per frame
	protected override void FixedUpdate () {
		
		base.FixedUpdate ();
		// don't rotate the player while using an ability
		// so you can't move the hitbox after you've aimed the abilty
		// this may be a little heavy-handed. Would rather have the player stop rotating after a certain point in the ability
		// this makes the player unable to adjust their aim during the ability cast time
		//
			Ray cameraRay = Camera.main.ScreenPointToRay (Input.mousePosition);
			Plane eyePlane = new Plane (Vector3.up, new Vector3 (0, 0, 0));
			float cameraDist;

		if (eyePlane.Raycast (cameraRay, out cameraDist)) {
			lookPoint = cameraRay.GetPoint (cameraDist);
			// use the tansform pos instead of the look point for the y so the player doesn't try to look down.
			if (!usingAbility) {
				// don't rotate the player while using an ability
				// this is kind of a bandaid fix for now.
				// It stops the melee ability from being abused by rotating the player to extend the hitbox
				// but it means the player can aim projectiles during their cast time. However, the model won't rotate to show this
				// will make it look weird if i ever get models and animations
				transform.LookAt (new Vector3 (lookPoint.x, transform.position.y, lookPoint.z));
			}
		}

	}

	public override Vector3 getPlayerTargetMoveLocation()
	{
		//return transform.position + new Vector3 (Input.GetAxisRaw ("Horizontal"), 0, Input.GetAxisRaw ("Vertical"));
		inputDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
		return transform.position + inputDirection;
	}

	public override Vector3 getAimPoint()
	{
		return lookPoint;
	}

	public override bool isPlayer()
	{
		return true;
	}

	void StartAbility (int abilityIndex)
	{
		usingAbility = true;
		Ability currentAbility = m1Ability;

		switch (abilityIndex) {
		case 0:
			currentAbility = m1Ability;
			break;
		case 1:
			currentAbility = m2Ability;
			break;
		case 2:
			currentAbility = qAbility;
			break;
		case 3:
			currentAbility = spaceAbility;
			break;
		case 4:
			currentAbility = eAbility;
			break;
		case 5:
			currentAbility = rAbility;
			break;
		case 6:
			currentAbility = fAbility;
			break;
		}
		// make sure the index was valid
		if (abilityIndex >= 0 && abilityIndex < 7)
		{
			// -= to clear the EndAbility before you add it
			// -= works even when abilityOver is empty (i.e. the first time the ability is used)
			// without the -=, EndAbility will be called many times as the ability is used more
			// (The 100th time the ability is cast, EndAbility is called 100 times because abilityOver has 100 instances of EndAbility)
			currentAbility.abilityOver -= EndAbility;
			currentAbility.abilityOver += EndAbility;

			currentAbility.slowMovement -= SelfSlow;
			currentAbility.slowMovement += SelfSlow;
		}
	}



	void EndAbility()
	{
		// currentAbility.abilityOver -= EndAbility;
		usingAbility = false;
	}

	public void die()
	{
		transform.position = spawnPoint;
		myHealth.resetHealth ();
	}

	void tookDamage()
	{
		// created now to suppress errors with health calling onDamage()
		// damage sound effect
		// blood splash or something
	}

}
