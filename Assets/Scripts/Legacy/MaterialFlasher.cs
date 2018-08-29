using System;
using System.Collections;
using UnityEngine;

public class MaterialFlasher : MonoBehaviour
{
	public Material flashMaterial;

	[Range(0.2f, 1f)]
	public float _fullHighlightWeight = 0.8f;

	private Coroutine _runningCoroutine;
	private Action onCompleted;

	public void Flash(float durationSeconds, Color finalColor, bool mayInterruptPreviousFlash,
					  Action onCompleted = null)
	{
		if (_runningCoroutine != null)
		{
			if (mayInterruptPreviousFlash)
			{
				StopCoroutine(_runningCoroutine);
			}
			else
			{
				return;
			}
		}

		flashMaterial.SetColor("_Tint", finalColor);
		flashMaterial.SetFloat("_Weight", 0f);

		_runningCoroutine = StartCoroutine(DoFlash(durationSeconds, onCompleted));
	}

	private IEnumerator DoFlash(float durationSeconds, Action onCompleted = null)
	{
		float elapsedSeconds = 0f;
		var halfDuration = durationSeconds / 2f;

		while (elapsedSeconds < halfDuration)
		{
			elapsedSeconds += Time.deltaTime;
			flashMaterial.SetFloat("_Weight", elapsedSeconds / halfDuration * _fullHighlightWeight);
			yield return null;
		}

		elapsedSeconds = 0f;

		while (elapsedSeconds < halfDuration)
		{
			elapsedSeconds += Time.deltaTime;
			flashMaterial.SetFloat("_Weight", _fullHighlightWeight - (elapsedSeconds / halfDuration * _fullHighlightWeight));
			yield return null;
		}

		_runningCoroutine = null;

		if (onCompleted != null)
		{
			onCompleted();
		}
	}
}
