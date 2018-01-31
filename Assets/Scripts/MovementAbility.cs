using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementAbility : Ability {

	public bool alwaysMoveToMax = true;
	public float maxMoveDistance = 3;
	public AoeDamageAbility aoeAbility;


	public override void useAbility (Transform spawnPoint, Vector3 aimPoint)
	{
		if (self == null) {
			setSelf (transform.parent.gameObject);
		}
		// check if cooldown is up
		if (isReady ()) {
			// sets the cooldown
			base.useAbility ();
			if (alwaysMoveToMax) {
				Vector3 lineToAimPoint = (aimPoint - spawnPoint.position).normalized;
				Vector3 clampedAimPoint = spawnPoint.position + lineToAimPoint * maxMoveDistance;
				self.GetComponent<MovementController> ().setDisplacementLocation (clampedAimPoint);
			}
			if (aoeAbility != null) {
				aoeAbility.useAbility (spawnPoint, aimPoint);
			}
		}
		// maybe the ability should be over when the dash ends (i.e. you reach the end of it)
		abilityFinished ();
	}
}
