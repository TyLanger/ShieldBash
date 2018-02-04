using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnAbility : Ability {

	// to surpress errors for now
	//public ProjectileController projectile;


	public ProjectileController[] projectiles;
	public int numProjectiles = 1;
	public int[] indexOrder = { 0 };
	public float[] waitAfterProjectile;
	// true means you can't adjust aim after first projectile
	public bool lockedAimDirection = false;
	[Range(0, 100)]
	public int selfSlowPercent = 30;


	// get the projectile to position
	// cast the ability
	// e.g. time to draw back the bow
	// can adjust aim during this time
	// except player can't rotate once the ability button has been pushed....
	public float castTime = 1;

	/*
	public override void useAbility(Transform spawnPoint, Vector3 aimPoint)
	{
		if (isReady ()) {
			base.useAbility ();

			var projectileCopy = Instantiate (projectile.gameObject, spawnPoint.position, spawnPoint.rotation);
			var projectileController = projectileCopy.GetComponent<ProjectileController> ();
			projectileController.setDamage (damage);
			projectileController.moveTo (aimPoint, spawnPoint.gameObject);
			projectileController.setAbility (this);
		}
		abilityFinished ();
	}*/

	public override void useAbility(Transform spawnPoint, Vector3 aimPoint)
	{
		if (isReady ()) {
			base.useAbility ();
			StartCoroutine (Spawn (spawnPoint, aimPoint));
		} else {
			// only call if the ability isn't ready
			// if the ability is ready, it will run the coroutine and that will call abilityFinished()
			abilityFinished ();
		}
	}


	IEnumerator Spawn(Transform spawnPoint, Vector3 aimPoint) {

		// the aimPoint given is ignored in this case
		// instead it gets the aimPoint when the cast time is done

		if (self == null) {
			setSelf (transform.parent.gameObject);
		}
			

		/*
		while (Time.time < startTime + castTime) {
			// do nothing, you are casting
			// call selfSlow() once
			// could even call is before the while
		}*/
		selfSlow(selfSlowPercent);
		// wait for castTime seconds
		// may want a loop once I get cast bars so that it animates over time
		yield return new WaitForSeconds(castTime);

		Vector3 originalAimDirection = Vector3.zero;
		if (lockedAimDirection) {
			//aimPoint = self.GetComponent<MovementController> ().getAimPoint ();
			originalAimDirection = self.GetComponent<MovementController> ().getAimPoint () - spawnPoint.position;
		}

		for (int i = 0; i < numProjectiles; i++) {
			// use the indexOrder array to find the appropriate projectile to spawn
			// currently, all projectiles spawn on the caster's position
			// because the caster's transform is passed in, it updates to their current position
			// even if the caster moves during the cast time, the projectile will still spawn from the new caster location
			GameObject projectileCopy = Instantiate (projectiles[indexOrder[i]].gameObject, spawnPoint.position, spawnPoint.rotation);
			ProjectileController projectileController = projectileCopy.GetComponent<ProjectileController> ();
			// at the moment, all projectiles have the same damage
			// could make an array for the damages, but base damage would still show in the inspector and be potentially confusing
			// int[] damageChanges = {0, 10, 10}; could work, but is still a bit abiguous. Are they cumulative? Does the first attack do 0 damage? or base + 0?
			projectileController.setDamage (damage);

			// if the aim direction is not locked
			if (lockedAimDirection) {
				aimPoint = spawnPoint.position + originalAimDirection;
			} else {
				aimPoint = self.GetComponent<MovementController> ().getAimPoint ();
			}

			projectileController.moveTo (aimPoint, spawnPoint.gameObject);
			projectileController.setAbility (this);

			yield return new WaitForSeconds(waitAfterProjectile [i]);
		}

		// end the self slow
		selfSlow(0);
		abilityFinished ();
		/*
		while (Time.time < endTime) {

			if (Time.time < startTime + castTime) {
				// if casting
				// only need to call this once
				selfSlow (30);

			} else {
				// cast complete
				allowAiming (false);
				var projectileCopy = Instantiate (projectile.gameObject, spawnPoint.position, spawnPoint.rotation);
				var projectileController = projectileCopy.GetComponent<ProjectileController> ();
				projectileController.setDamage (damage);
				// aimpoint should be updated until allowAiming is false;
				// can't just be the mouse position because enemies don't use mice
				// This should get PlayerController for players and EnemyController for enemies
				aimPoint = self.GetComponent<MovementController> ().getAimPoint ();
				projectileController.moveTo (aimPoint, spawnPoint.gameObject);
				projectileController.setAbility (this);

				// done casting
				selfSlow (0);
				allowAiming (true);

				// only spawn projectiles once
				break;
				// for a SpawnAbility, is there anything that happens after the ability is spawned?
				// like a follow-through like in a melee ability?
			}
			yield return null;
		}
		*/
	}

	IEnumerator Spawn(Transform[] spawnPoints, Vector3[] aimPoints) {
		// for spawning multiple entities
		// Other Spawn() handles multiple spawns
		// this is just notes

		// if firing multiple projectiles over time, might want to be able to readjust aim point as they fire
		// But might also no
		// Ability where you can aim individual attacks
		// Ability where your direction is decided at the start
		// Can't have the player call the ability multiple times. That would complicate dragging and dropping abilitites
		// Ability now needs to know the mouse position. Probably better going forward for aiming anyway for aiming

		// how do multiple projectiles work?
		// int numProjectiles = 3;
		//public ProjectileController[] projectiles;
		// Option 1:
		// slot in a different projectile object for each projectile
		// makes it easy if you fire 3 different projectiles
		// slightly messy if first x projectiles are all the same, with just the last 1 different
		// Option 2:
		// public ProjectileController[] projectiles;
		// int[] indexOrder = {0, 0, 1}
		// use the first projectile twice, then the second projectile on the last one
		// makes it fairly easy to swap back and forth between projectiles
		// could be useful for projetiles whose difference is only left or right alignment
		// e.g. fire left missile, fire right missile, fire left missile, fire right missile
		// {0, 1, 0, 1, 0, 1}
		// Option 3:
		// have a list that says when the next projectile is uses
		// int[] list = {4, 5}
		// have the first 4 use projectile 0, the fifth projectile uses projectile 1
		// I can't even come up with a good variable name to describe this behaviour other than list and explaining it

		//float[] timeBetweenProjectiles = { 0, 1.5f, 2f };
		// wait 0 seconds between the first and second projectile (they fire at the same time)
		// wait 1.5 seconds after the second projectile
		// wait 2 seconds after the third projectile
		// This allows several projectiles to fire at once and for projectiles to fire one after another

		// might not be able to just call Spawn()
		// because that would account for cast time each time
		// shouldn't have an array of spawnPoints because that would change the useAbility call in PlayerController
		// public Vector3[] spawnOffsets;
		// the ability should have a spot to assign different offsets for various projectiles
		// if(spawnOffsets == null)
		// 		// all projectiles spawn at the same spot
		// might have something similar to
		// int[] spawnIndexOrder = {0, 1, 0, 1} // is this name descriptive or the exact opposite of what I want?
		// first projectile spawns at the offset of spawnOffsets[0]
		// second projectile spawns at offset spawnOffset[1]
		// third projectile spawns at offset spawnOffset[0]
		// fourth projectile spawns at offset spawnOffset[1]

		// slowMovement(80)
		// endTime = startTime + castTime + timeBetweenProjectiles[0] + timeBetweenProjectiles[1] + ...
		// wait for initial cast time
		// while(Time.time < endTime)
		// get aimPoint at current moment
		// spawn first projectile
		// wait for time between projectiles
		// get new aimPoint at this time
		// spawn second projectile
		// ...
		// while ends
		// slowMovement(0)
		// EndAbility()

		yield return null;
	}
}
