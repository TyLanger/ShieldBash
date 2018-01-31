using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAbility : Ability {

	bool isPlayer = false;
	EnemyController enemyController;

	public GameObject weapon;
	Collider[] wCollider;

	// timing variables
	// give the time in seconds that an action takes
	// move sword to start position and start rotation
	// move over time timeToGetToStartPos
	public float windupTime = 0.7f;
	public Vector3 weaponStartOffset = new Vector3 (0.5f, 0, 0.5f);
	public Vector3 weaponStartRotation = new Vector3 (1, 0, 2);

	// The time the sword hangs at the start position before it swings
	public float startDelayTime = 0.2f;
	// the time it takes to get from the start of the swing to the end position
	// how long the sword actually takes to swing
	public float swingTime = 0.6f;
	public Vector3 weaponEndOffset = new Vector3 (-0.5f, 0, 0.5f);
	public Vector3 weaponEndRotation = new Vector3 (-2, 0, 1);

	// amount of time the sword hangs at the end of the swing
	public float recoveryTime = 0.6f;

	// shorthand variables
	// gives the time when these actions happen with relation to game time
	// timeOfStartAttack = 124.50 means the attack will start when Time.time == 124.50
	// not that the start of the attack will take 124.5 seconds
	float timeOfStartAttack;
	float timeAtSweepStart;
	float timeAtSweepEnd;
	float timeOfEndAttack;

	void Start()
	{
		wCollider = GetComponents<Collider> ();
	}

	public override void useAbility (Transform spawnPoint, Vector3 aimPoint)
	{
		if (GetComponentInParent<PlayerController> () != null) {
			// has a player controller; is a player
			isPlayer = true;
		} else {
			if (GetComponent<EnemyController> () != null) {
				enemyController = GetComponent<EnemyController> ();
			}
		}
		if(isReady())
		{
			// some shorthand variables
			// just sums of other variables
			// game time at the start of the attack
			timeOfStartAttack = Time.time;
			// time when the weapon start swinging
			timeAtSweepStart = timeOfStartAttack + windupTime + startDelayTime;
			timeAtSweepEnd = timeAtSweepStart + swingTime;
			// total time of the attack
			timeOfEndAttack = timeOfStartAttack + windupTime + startDelayTime + swingTime + recoveryTime;
			cooldown = windupTime + startDelayTime + swingTime + recoveryTime;

			// base.useAbility() sets the cooldown time
			base.useAbility ();

			StartCoroutine(AttacK());

		}
	}

	// All attack code in one method
	// IEnumerator instead of stringing methods together with Invoke
	// Invoke may be slightly more readable
	// But this works over time which works for lerp
	IEnumerator AttacK()
	{
		Vector3 weaponOriginalPosition = weapon.transform.localPosition; 
		Quaternion weaponOriginalRotation = weapon.transform.localRotation;

		Quaternion weaponStartLocalRotation = Quaternion.FromToRotation (Vector3.forward, weaponStartRotation.normalized);
		Quaternion weaponEndLocalRotation = Quaternion.FromToRotation (Vector3.forward, weaponEndRotation.normalized);


		//timeOfNextAttack = Time.time + timeBetweenAttacks;

		while (Time.time < timeOfEndAttack) {
			if (Time.time < timeOfStartAttack + windupTime) {
				// getting the sword into posiiton
				// t = 0 at Time.time
				// t = 1 at Time.time + timeToGetToStartPos
				weapon.transform.localPosition = Vector3.Lerp (weapon.transform.localPosition, weaponStartOffset, (Time.time - timeOfStartAttack) / windupTime);
				weapon.transform.localRotation = Quaternion.Lerp (weapon.transform.localRotation, weaponStartLocalRotation, (Time.time - timeOfStartAttack) / windupTime);
				// if having a custom animation
				// weaponAnim.SetFloat("WeaponToStartPosition", (Time.time - timeOfStartAttack) / timeToGetToStartPos);
				// keep updating the percent the animation should be at
				// Maybe would be a blend tree?
				// first option is the first animation
				// as the time changes, goes through the animations, switching animations when the time gets past the thresholds
			} else if (Time.time < timeAtSweepStart) {
				// hold position
				// this is probably a good time to have the last time the enemy can re-aim the attack
				// i.e. enemy no longer looks at the player after this
				if (!isPlayer) {
					if (!enemyController.rootedForAttack) {
						enemyController.rootedForAttack = true;
					}
				}

			} else if (Time.time < timeAtSweepEnd) {
				// swing the sword
				// turn on the hit box
				for (int i = 0; i < wCollider.Length; i++) {
					// enable all colliders
					if (!wCollider[i].enabled) {
						wCollider[i].enabled = true;
					}
				}
				weapon.transform.localPosition = Vector3.Lerp (weapon.transform.localPosition, weaponEndOffset, (Time.time - timeAtSweepStart) / swingTime);
				weapon.transform.localRotation = Quaternion.Lerp (weapon.transform.localRotation, weaponEndLocalRotation, (Time.time - timeAtSweepStart) / swingTime);
			} else if (Time.time < timeOfEndAttack) {
				// delay time at the end of the swing
				// turn off the collider
				for (int i = 0; i < wCollider.Length; i++) {
					if (wCollider[i].enabled) {
						wCollider[i].enabled = false;
					}
				}
				// return weapon to its default position
				weapon.transform.localPosition = Vector3.Lerp(weapon.transform.localPosition, weaponOriginalPosition, (Time.time - timeAtSweepEnd) / recoveryTime);
				weapon.transform.localRotation = Quaternion.Lerp (weapon.transform.localRotation, weaponOriginalRotation, (Time.time - timeAtSweepEnd) / recoveryTime);

				if (!isPlayer) {
					if (enemyController.rootedForAttack) {
						enemyController.rootedForAttack = false;
					}
				}
			}

			yield return null;
		}
		abilityFinished ();
	}

	public virtual void OnTriggerEnter(Collider col)
	{
		if (col.GetComponent<Health> () != null) {
			if (col.gameObject != self) {
				col.GetComponent<Health> ().takeDamage (damage);
				// much better knockback
				additionalEffects(col.gameObject);


			}
		}
	}

}
