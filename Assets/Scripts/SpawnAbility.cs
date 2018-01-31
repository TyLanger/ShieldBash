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
			var projectileController = projectileCopy.GetComponent<ProjectileController> ();
			projectileController.setDamage (damage);
			projectileController.moveTo (aimPoint, spawnPoint.gameObject);
			projectileController.setAbility (this);
		}
		abilityFinished ();
	}
}
