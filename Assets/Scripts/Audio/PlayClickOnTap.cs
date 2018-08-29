using UnityEngine;
using UnityEngine.EventSystems;

public class PlayClickOnTap : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
	public AudioVolumes volumeConfig;
	public AudioSFX sfxToPlay;

	private float _startVolume;
	private float _endVolume;

	private void Start()
	{
		_startVolume = volumeConfig.StartVolume(sfxToPlay);
		_endVolume = volumeConfig.EndVolume(sfxToPlay);
	}

	private void PlayClickSFX(float volumePercent)
	{
		AudioManager.Instance.Play(sfxToPlay, volumePercent);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		PlayClickSFX(_startVolume);
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		PlayClickSFX(_endVolume);
	}
}

public enum AudioSFX
{
	None,
	ButtonClick,
	PieceClick
}
