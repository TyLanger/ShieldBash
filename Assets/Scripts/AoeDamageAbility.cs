using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AoeDamageAbility : Ability {




	void Start() {
		// useAbility happens first then start
		// start will happen before OnTriggerEnter happens
		//self = GetComponentInParent<PlayerController> ().gameObject;
		if(self == null)
			self = transform.parent.gameObject;

	}



	public override void useAbility()
	{
		
		if (isReady ()) {
			base.useAbility ();

			if (!GetComponent<SphereCollider>().gameObject.activeSelf) {
				// only activate if it's not already active. Else it can deal damage as fast as you press the button.
				GetComponent<SphereCollider> ().gameObject.SetActive (true);
				Invoke ("disableExplosion", 0.2f);
			}

		}
	}

	void disableExplosion()
	{
		gameObject.SetActive (false);
	}
		

	public virtual void OnTriggerEnter(Collider col)
	{
		if (col.GetComponent<Health> () != null) {
			if (col.gameObject != self) {
				col.GetComponent<Health> ().takeDamage (damage);
			}
		}
	}

}
