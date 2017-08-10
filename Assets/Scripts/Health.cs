using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour {

	int maxHealth = 100;
	public int currentHealth;

	public delegate void deathDelegate();
	public deathDelegate onDeath;

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
	

	public void takeDamage(int damage)
	{
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

	void die()
	{
		onDeath ();
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
