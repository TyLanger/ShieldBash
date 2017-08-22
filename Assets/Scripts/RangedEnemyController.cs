using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemyController : EnemyController {


	public Ability projectile;

	protected override void Start()
	{
		base.Start ();
		timeBetweenAttacks = projectile.cooldown;
		// less delay time because projectiles have travel time
		damageDelayTime = 0.2f;
	}

	protected override void activateAttackArc()
	{
		// transform.forward returns something like (1.0, 0.0, 0.0)
		// Need to multiply by how far you want the projectile to go
		// then add it to the current location. Otherwise it will try to go to (1.0, 0.0, 0.0 world coords.
		projectile.useAbility (transform, transform.position + (transform.forward * GetComponent<EnemyAI> ().attackDist));
		Invoke ("disableAttackArc", 0.1f);
	}

	protected override void disableAttackArc()
	{
		attacking = false;
	}

}
