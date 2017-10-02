using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MovementController {

	/* in parent
	public float moveSpeed = 0.5f;
	Vector3 moveDirection;
	*/
	Vector3 spawnPoint;

	public float timeBetweenAttacks = 0.25f;
	float timeOfLastAttack = 0;

	public Transform shield;
	public Transform shieldLeft;
	public Transform shieldRight;
	float rayDistance = 4.2f;

	int shieldDamage = 50;

	Animator shieldAnim;

	Health myHealth;
	int maxHealth = 100;

	bool dashing = false;
	float dashSpeedMultiplier = 2;
	float dashDistance = 10;
	Vector3 dashStart;

	bool shielding = false;

	public Ability eAbility;
	public Ability qAbility;
	public Ability fAbility;

	Vector3 lookPoint;

	// Use this for initialization
	protected override void Start () {
		base.Start ();
		shieldAnim = shield.GetComponent<Animator> ();
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
			if (Time.time > timeOfLastAttack + timeBetweenAttacks) {
				timeOfLastAttack = Time.time;
				shieldBash ();
			}
		}
		if(Input.GetButtonDown("Fire2")) {
			// right click
			shielding = true;
		}
		if (Input.GetButtonUp ("Fire2")) {
			shielding = false;
		}
		if (Input.GetButtonDown ("Ability1") ) {
			// default q
			// set the point where it should spawn and the place it should travel to
			qAbility.useAbility (transform, lookPoint);
		}
		if (Input.GetButtonDown ("Ability2")) {
			// default e
			eAbility.useAbility();
		}
		if (Input.GetButtonDown ("Ability3")) {
			// default r
			if (!dashing) {
				dashAbility ();
			}
		}
		if (Input.GetButtonDown ("Ability4")) {
			// default f
			fAbility.useAbility(transform, lookPoint);
		}
	}
	
	// Update is called once per frame
	protected override void FixedUpdate () {
		
		base.FixedUpdate ();

		Ray cameraRay = Camera.main.ScreenPointToRay (Input.mousePosition);
		Plane eyePlane = new Plane(Vector3.up, new Vector3(0, 0, 0));
		float cameraDist;

		if (eyePlane.Raycast (cameraRay, out cameraDist)) {
			lookPoint = cameraRay.GetPoint (cameraDist);
			// use the tansform pos instead of the look point for the y so the player doesn't try to look down.
			transform.LookAt (new Vector3(lookPoint.x, transform.position.y, lookPoint.z));
		}


	}

	public override Vector3 getPlayerTargetMoveLocation()
	{
		return transform.position + new Vector3 (Input.GetAxisRaw ("Horizontal"), 0, Input.GetAxisRaw ("Vertical"));
		/* dashing doesn't work at the moment
		if (!dashing) {
			return new Vector3 (Input.GetAxisRaw ("Horizontal"), 0, Input.GetAxisRaw ("Vertical"));
		} else {
			// if the dash button is pressed while standing still, the player can get stuck
			// the code after the || should safeguard against that
			// Dash should be redone with the new MovementController
			// moveDirection used to be based on input
			if ((Vector3.Distance (transform.position, dashStart) > dashDistance) || (moveDirection.magnitude < 0.1f)) {
				moveSpeed /= dashSpeedMultiplier;
				dashing = false;
			}


		}*/
	}

	public override bool isPlayer()
	{
		return true;
	}

	void shieldBash()
	{

		shieldAnim.SetTrigger ("smashTrigger");

		// fire out rays from the shield to see if it hits anything
		RaycastHit hit;
		Vector3 rayOrigin = shield.transform.position;

		// Fire forward from the direction the shield is facing
		Debug.DrawRay (rayOrigin, shield.transform.forward * rayDistance, Color.red);
		Debug.DrawRay (shieldLeft.position, shield.transform.forward * rayDistance, Color.green);
		Debug.DrawRay (shieldRight.position, shield.transform.forward * rayDistance, Color.blue);

		// if any of the rays hits something
		// seems to work
		// The one that hits should return true and continue the if. It should also be the one that stored something in hit last
		if (Physics.Raycast (rayOrigin, shield.transform.forward, out hit, rayDistance) || Physics.Raycast (shieldLeft.position, shield.transform.forward, out hit, rayDistance) || Physics.Raycast (shieldRight.position, shield.transform.forward, out hit, rayDistance)) {
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

	public bool isShielding()
	{
		return shielding;
	}

	void dashAbility()
	{
		//Debug.Log ("Dashing");
		dashing = true;
		//moveSpeed *= dashSpeedMultiplier;
		dashStart = transform.position;

		// dash in direction of mouse
		// moveDirection = new Vector3(
		Debug.Log(lookPoint - transform.position );
		//moveDirection = lookPoint - transform.position;
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
