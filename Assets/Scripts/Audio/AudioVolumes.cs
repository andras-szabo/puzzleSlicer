using UnityEngine;

[CreateAssetMenu()]
public class AudioVolumes : ScriptableObject
{
	public float buttonTapStart;
	public float buttonTapEnd;

	public float pieceTapStart;
	public float pieceTapEnd;

	public float this[AudioSFX sfx]
	{
		get
		{
			switch (sfx)
			{
				case AudioSFX.ButtonTapStart: return buttonTapStart;
				case AudioSFX.ButtonTapEnd: return buttonTapEnd;
				case AudioSFX.PieceTapStart: return pieceTapStart;
				case AudioSFX.PieceTapEnd: return pieceTapEnd;
				default:
					return 0f;
			}
		}
	}
}
