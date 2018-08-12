using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

	public static int Count => idleAudioSources.Count;

	private static AudioManager instance;
	private static readonly Queue<AudioSource> idleAudioSources = new Queue<AudioSource>(5);
	private static int busyCount;
	
	private void Awake() {
		instance = this;
	}

	public static void PlayAtPoint(AudioClip clip, Vector3 position, float volume = 1f) {
		if (busyCount > 100) return;
		AudioSource source = GetAudioSource();
		source.transform.position = position;
		source.clip = clip;
		source.volume = volume;
		source.Play();
		instance.StartCoroutine(ExeRecycleCoroutine(source));
	}

	private static AudioSource GetAudioSource() {
		AudioSource source;
		if (idleAudioSources.Count > 0) {
			source = idleAudioSources.Dequeue();
			source.gameObject.SetActive(true);
		} else {
			GameObject gameObject = new GameObject("Public Audio Source");
			gameObject.transform.parent = instance.transform;
			source = gameObject.AddComponent<AudioSource>();
			source.spatialBlend = 1f;
			source.loop = false;
		}

		return source;
	}

	private static IEnumerator ExeRecycleCoroutine(AudioSource source) {
		busyCount++;
		float time = source.clip.length;
		yield return new WaitForSeconds(time);
		source.Stop();
		source.gameObject.SetActive(false);
		idleAudioSources.Enqueue(source);
		busyCount--;
	}
}