using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability : MonoBehaviour {

	// additional effects that can be applied to the ability in addition to damage
	public enum StatusEffect{None, Push, Pull, Knockdown, Knockup, Stun, Slow, DamageOverTime};

	public StatusEffect additionalEffect;

	public float pushDistance = 8;
	public float pushScale = 7;
	public float pullDistance = 2;
	public float pullScale = 15;
	public float stunDuration = 2;

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
			g.GetComponent<EnemyController> ().setPullLocation (transform.position + (g.transform.position - transform.position).normalized * pushDistance);
			break;
		case StatusEffect.Pull:
			g.GetComponent<EnemyController> ().setPullLocation (transform.position + (g.transform.position - transform.position).normalized * pullDistance);
			break;
		case StatusEffect.Stun:
			g.GetComponent<EnemyController> ().stun (stunDuration);
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
