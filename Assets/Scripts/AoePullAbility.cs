using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AoePullAbility : AoeDamageAbility {

	float pullDistance = 2;

	public override void OnTriggerEnter(Collider col)
	{
		if (col.GetComponent<Health> () != null) {
			if (col.gameObject != self) {
				// pull the enemy to pullDistance away from you instantly
				//col.transform.position = transform.position + (col.transform.position - transform.position).normalized * pullDistance;
				col.GetComponent<Health> ().takeDamage (damage);
				col.GetComponent<EnemyController> ().setPullLocation (transform.position + (col.transform.position - transform.position).normalized * pullDistance);
			}
		}
	}
}
