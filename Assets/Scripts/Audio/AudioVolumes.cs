using UnityEngine;

[CreateAssetMenu()]
public class AudioVolumes : ScriptableObject
{
	public float buttonTapStart;
	public float buttonTapEnd;

	public float pieceTapStart;
	public float pieceTapEnd;

	public float StartVolume(AudioSFX sfx)
	{
		switch (sfx)
		{
			case AudioSFX.ButtonClick: return buttonTapStart;
			case AudioSFX.PieceClick: return pieceTapStart;
		}

		return 0f;
	}

	public float EndVolume(AudioSFX sfx)
	{
		switch (sfx)
		{
			case AudioSFX.ButtonClick: return buttonTapEnd;
			case AudioSFX.PieceClick: return pieceTapEnd;
		}

		return 0f;
	}
}
