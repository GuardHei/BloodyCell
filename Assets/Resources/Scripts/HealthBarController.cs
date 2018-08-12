using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour {

	public Slider slider;
	public Text text;

	private readonly StringBuilder _stringBuilder = new StringBuilder();

	public void UpdateDisplay(int health, int max) {
		slider.value = 1f - health / (float) max;
		_stringBuilder.Clear();
		_stringBuilder.Append("HEALTH = ");
		_stringBuilder.Append(health);
		_stringBuilder.Append(" / ");
		_stringBuilder.Append(max);
		text.text = _stringBuilder.ToString();
	}
}