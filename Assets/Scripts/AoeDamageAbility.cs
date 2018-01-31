using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AoeDamageAbility : Ability {

	public float aoeSize = 10;
	public float explosionTime = 0.2f;

	void Start() {
		// useAbility happens first then start
		// start will happen before OnTriggerEnter happens
		//self = GetComponentInParent<PlayerController> ().gameObject;
		if(self == null)
			self = transform.parent.gameObject;

		// set the collider to the right size
		setColliderSize();

	}

	void setColliderSize()
	{
		transform.localScale = Vector3.one * aoeSize;

		// this can probably all be done in start
		// only reason it's here is so I can change the statusEffect in game
		/*
		switch (additionalEffect) {
		case StatusEffect.Push:
			transform.localScale = Vector3.one * pushScale;
			break;
		case StatusEffect.Pull:
			transform.localScale = Vector3.one * pullScale;
			break;
		}*/

	}

	public override void useAbility()
	{
		
		if (isReady ()) {
			base.useAbility ();
			setColliderSize ();
			if (!GetComponent<SphereCollider>().gameObject.activeSelf) {
				// only activate if it's not already active. Else it can deal damage as fast as you press the button.
				GetComponent<SphereCollider> ().gameObject.SetActive (true);
				Invoke ("disableExplosion", explosionTime);
			}

		}
		abilityFinished ();
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
				// much better knockback
				additionalEffects(col.gameObject);


			}
		}
	}

}
