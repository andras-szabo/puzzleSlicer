using UnityEngine;
using UnityEngine.EventSystems;

public class PlayClickOnTap : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
	public bool isUI;

	private void PlaySFX(AudioSFX sfx)
	{
		AudioManager.Instance.Play(sfx);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		PlaySFX(isUI ? AudioSFX.ButtonTapStart : AudioSFX.PieceTapStart);
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		PlaySFX(isUI ? AudioSFX.ButtonTapEnd : AudioSFX.PieceTapEnd);
	}
}
