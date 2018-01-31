using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour {


	int maxHealth = 100;
	public int currentHealth;

	bool damageOverTime = false;
	float timeOfNextDamage;
	float dotInterval;
	int dotDamage;
	float dotEndTime;

	public delegate void voidDelegate();
	public voidDelegate onDeath;
	public voidDelegate onDamage;

	public RectTransform healthBar;
	float healthBarWidth;
	float maxHpToBarWidthRatio;

	public TextMesh damageText;
	Transform cameraTrans;

	// Use this for initialization
	void Start () {
		currentHealth = maxHealth;
		healthBarWidth = healthBar.rect.width;
		maxHpToBarWidthRatio = healthBarWidth / maxHealth;
		cameraTrans = FindObjectOfType<Camera> ().transform;
	}

	void FixedUpdate()
	{
		if (damageOverTime) {
			if (timeOfNextDamage < Time.time) {
				timeOfNextDamage = Time.time + dotInterval;

				if (dotEndTime < Time.time) {
					damageOverTime = false;
				} else {
					takeDamage (dotDamage);
				}

			}
		}
	}

	public void takeDamage(int damage)
	{
		if (onDamage != null) {
			onDamage ();
		}
		var textMesh = Instantiate(damageText, transform.position, cameraTrans.rotation);
		textMesh.transform.parent = null;
		textMesh.text = "-" + damage.ToString ();
		if (damage >= currentHealth) {
			currentHealth = 0;
			die ();
		} else {
			currentHealth -= damage;
		}
		updateHealthBar ();

	}

	public void takeDamageOverTime(int damageTick, float damageInterval, float totalTime)
	{
		damageOverTime = true;
		timeOfNextDamage = 0;
		dotDamage = damageTick;
		dotInterval = damageInterval;
		dotEndTime = Time.time + totalTime;
	}

	void die()
	{
		onDeath ();
		damageOverTime = false;
	}

	public void resetHealth()
	{
		currentHealth = maxHealth;
	}

	public void setMaxHealth(int max)
	{
		maxHealth = max;
		// need to reset this if the max health is ever changed
		maxHpToBarWidthRatio = healthBarWidth / maxHealth;
	}

	void updateHealthBar()
	{
		// Need to multiply by the ratio between the width of the health bar (default of 150)
		// to max health (default to 100)
		// multiply by 150/100 = 1.5
		healthBar.sizeDelta = new Vector2(currentHealth * maxHpToBarWidthRatio, healthBar.sizeDelta.y);
	}

}
