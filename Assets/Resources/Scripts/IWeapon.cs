using System;
using UnityEngine;

public interface IWeapon {

	void OnSwitchOn();
	void OnSwitchOff();
	void ReceiveAttackCommand();
	void FinishAttackCommand();
}

[Serializable]
public class WeaponBaseSettings {
	
	public bool doesHit;
	public bool doesStun;
	public bool doesDamage;

	public float hitVelocity;
	public float stunDuration;
	public int damage;

	public void Attack(EnemyController enemy, Vector3 attackerPosition) {
		if (doesHit) enemy.GetHit((enemy.transform.position - attackerPosition).normalized * hitVelocity);
		if (doesStun) enemy.GetStunned(stunDuration);
		if (doesDamage) enemy.GetDamaged(damage);
	}

	public void Attack(CancerController cancer) {
		if (doesDamage) cancer.GetDamaged(damage);
	}
}