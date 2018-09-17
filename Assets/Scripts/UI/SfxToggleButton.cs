using UnityEngine;
using UnityEngine.UI;

public class SfxToggleButton : MonoBehaviour
{
	[SerializeField] Text sfxLabel;

	private void OnEnable()
	{
		TryInitSfxState();
	}

	private void Start()
	{
		TryInitSfxState();
	}

	private void TryInitSfxState()
	{
		var audioManager = AudioManager.Instance;
		if (audioManager != null)
		{
			UpdateSfxStateLabel(AudioManager.Instance.SFX);
		}
	}

	public void ToggleSfxState()
	{
		var sfxState = AudioManager.Instance.ToggleSFX();
		UpdateSfxStateLabel(sfxState);
	}

	private void UpdateSfxStateLabel(bool isOn)
	{
		sfxLabel.text = string.Format("Sound {0}", isOn ? "ON" : "OFF");
	}
}
