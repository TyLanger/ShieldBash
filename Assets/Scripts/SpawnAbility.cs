using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnAbility : Ability {


	public ProjectileController projectile;
	public GameObject spawnedObject;




	public override void useAbility(Transform spawnPoint, Vector3 aimPoint, bool move)
	{
		if (isReady ()) {
			base.useAbility ();

			if (move) {
				var projectileCopy = Instantiate (projectile.gameObject, spawnPoint.position, spawnPoint.rotation);
				projectileCopy.GetComponent<ProjectileController> ().setDamage (damage);
				projectileCopy.GetComponent<ProjectileController> ().moveTo (aimPoint, spawnPoint.gameObject);
			} else {
				Instantiate (spawnedObject, aimPoint, spawnPoint.rotation);
			}
		}
	}
}
