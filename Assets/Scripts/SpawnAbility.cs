using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnAbility : Ability {


	public ProjectileController projectile;


	public override void useAbility(Transform spawnPoint, Vector3 aimPoint)
	{
		if (isReady ()) {
			base.useAbility ();

			var projectileCopy = Instantiate (projectile.gameObject, spawnPoint.position, spawnPoint.rotation);
			projectileCopy.GetComponent<ProjectileController> ().setDamage (damage);
			projectileCopy.GetComponent<ProjectileController> ().moveTo (aimPoint, spawnPoint.gameObject);
		}
	}
}
