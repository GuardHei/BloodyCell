using System;
using System.Collections;
using UnityEngine;

public class ShooterController : MonoBehaviour, IWeapon {

	public string bulletName;
	public int commandBufferLength;
	public WeaponBaseSettings baseSettings;
	public float firePower;
	public float lifespan;
	public float destroyDelay;
	public AudioClip fireSound;
	public FireOrigin[] fireOrigins;
	public float recoilTime;
	public float recoilDistance;
	public float recoilAngle;
	public Vector3 recoilShake;
	public float recoilRumble;
	
	private bool _isFiring;
	private bool _isRecoiling;
	private int _commandBuffer;
	private Coroutine _recoilCoroutine;

	private void OnAwake() {
		
	}
	
	public void OnSwitchOn() {
		
	}

	public void OnSwitchOff() {
		
	}
	
	public void ReceiveAttackCommand() {
		if (_commandBuffer < commandBufferLength) _commandBuffer++;
		_isFiring = true;
	}

	public void FinishAttackCommand() {
		if (commandBufferLength < 0) _isFiring = false;
	}

	private void Update() {
		if (_isFiring) {
			if (!_isRecoiling) Fire();
		}
	}

	private void Fire() {
		if (fireSound) AudioManager.PlayAtPoint(fireSound, transform.position);
		CameraShaker.ShakeAt(transform.position, recoilShake);
		foreach (var fireOrigin in fireOrigins) {
			BulletController bullet = BulletManager.Get(bulletName);
			bullet.lifespan = lifespan;
			bullet.destroyDelay = destroyDelay;
			bullet.baseSettings = baseSettings;
			bullet.transform.position = transform.position + transform.right * fireOrigin.offset.x + transform.up * fireOrigin.offset.y + transform.forward * fireOrigin.offset.z;
			bullet.transform.rotation = transform.rotation * Quaternion.Euler(0, -fireOrigin.rotation, 0);
			float rad = bullet.transform.eulerAngles.y * Mathf.Deg2Rad;
			bullet.Fire(new Vector3(Mathf.Cos(rad) * firePower, 0, -Mathf.Sin(rad) * firePower));
		}
		
		_recoilCoroutine = StartCoroutine(ExeRecoilCoroutine());
	}

	private IEnumerator ExeRecoilCoroutine() {
		_isRecoiling = true;
		float startTime = Time.time;
		Quaternion targetRot = Quaternion.Euler(0, 0, recoilAngle);
		
		while (true) {
			yield return null;
			float timeDiff = Time.time - startTime;
			float lerpCoefficient = timeDiff / recoilTime;
			transform.localPosition = new Vector3(Mathf.Lerp(0, -recoilDistance, lerpCoefficient), 0, 0);
			transform.localRotation = Quaternion.Lerp(Quaternion.identity, targetRot, lerpCoefficient);
			if (timeDiff >= recoilTime) break;
		}

		startTime = Time.time;
		while (true) {
			yield return null;
			float timeDiff = Time.time - startTime;
			float lerpCoefficient = timeDiff / recoilTime;
			transform.localPosition = new Vector3(Mathf.Lerp(-recoilDistance, 0, timeDiff / recoilTime), 0, 0);
			transform.localRotation = Quaternion.Lerp(targetRot, Quaternion.identity, lerpCoefficient);
			if (timeDiff >= recoilTime) break;
		}
		
		_isRecoiling = false;
		_recoilCoroutine = null;

		if (commandBufferLength < 0) yield break;
		_commandBuffer--;
		if (_commandBuffer > 0) Fire();
		else _isFiring = false;
	}
}

[Serializable]
public class FireOrigin {
	public Vector3 offset;
	public float rotation;
}