using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability : MonoBehaviour {

	// additional effects that can be applied to the ability in addition to damage
	public enum StatusEffect{None, Push, Pull, Knockdown, Knockup, Stun, Slow, DamageOverTime};


	// HideInInspector so they don't get drawn twice
	// The inspector gets drawn as normal (without these)
	// then these get drawn based on what additionalEffects is set to
	[HideInInspector]
	public StatusEffect additionalEffect;

	[HideInInspector]
	public float pushDistance = 8;
	//[HideInInspector]
	//public float pushScale = 7;
	[HideInInspector]
	public float pullDistance = 2;
	//[HideInInspector]
	//public float pullScale = 15;
	[HideInInspector]
	public float stunDuration = 2;

	// DoT
	[HideInInspector]
	public int dotDamageTick;
	[HideInInspector]
	public float dotTimeInterval, dotTotalTime;

	// Slow
	[HideInInspector]
	public float slowPercent, slowDuration;
	[HideInInspector]
	public bool decayingSlow;

	public int damage = 0;
	public float cooldown = 0;
	float timeNextReady = 0;
	protected GameObject self;


	public virtual void useAbility()
	{
		timeNextReady = Time.time + cooldown;
	}

	public virtual void useAbility(Transform spawnPoint, Vector3 aimPoint)
	{

	}

	public void additionalEffects(GameObject g)
	{
		switch(additionalEffect)
		{
		case StatusEffect.Push:
			g.GetComponent<MovementController> ().setDisplacementLocation (transform.position + (g.transform.position - transform.position).normalized * pushDistance);
			break;
		case StatusEffect.Pull:
			g.GetComponent<MovementController> ().setDisplacementLocation (transform.position + (g.transform.position - transform.position).normalized * pullDistance);
			break;
		case StatusEffect.Stun:
			g.GetComponent<MovementController> ().stun (stunDuration);
			break;
		case StatusEffect.DamageOverTime:
			g.GetComponent<Health> ().takeDamageOverTime(dotDamageTick, dotTimeInterval, dotTotalTime);
			break;

		case StatusEffect.Slow:
			g.GetComponent<MovementController> ().slow (slowPercent, slowDuration);
			break;
		}
	}

	public void setSelf(GameObject _self)
	{
		self = _self;
	}

	protected bool isReady()
	{
		// if cooldown is finished
		if (Time.time > timeNextReady) {
			
			return true;
		}

		return false;	
	}
}
