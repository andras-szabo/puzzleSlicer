using System.Collections.Generic;
using UnityEngine;

public enum AudioSFX
{
	None,
	ButtonTapStart,
	ButtonTapEnd,
	PieceTapStart,
	PieceTapEnd
}

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoSingleton<AudioManager>
{
	public static string PP_KEY_SFX = "sfx";
	public static string GetClipForEnum(AudioSFX sfx)
	{
		switch (sfx)
		{
			case AudioSFX.ButtonTapStart:
			case AudioSFX.ButtonTapEnd:
				return "click";

			case AudioSFX.PieceTapStart:
			case AudioSFX.PieceTapEnd:
				return "click_high";
		}

		return string.Empty;
	}

	private bool _sfx;
	public bool SFX
	{
		get
		{
			return _sfx;
		}

		set
		{
			_sfx = value;
			PlayerPrefs.SetInt(PP_KEY_SFX, _sfx ? 1 : 0);
		}
	}

	private AudioSource _source;
	private AudioSource Source { get { return _source ?? (_source = GetComponent<AudioSource>()); } }

	public AudioClip[] clips;
	public AudioVolumes volumes;

	private Dictionary<string, AudioClip> _clipsByName = new Dictionary<string, AudioClip>();

	public bool ToggleSFX()
	{
		var current = SFX;
		return (SFX = !current);
	}

	public void Play(string clipName, float volumePercent = 100f)
	{
		if (SFX)
		{
			AudioClip clip;
			if (_clipsByName.TryGetValue(clipName, out clip))
			{
				Source.PlayOneShot(clip, volumePercent / 100f);
			}
		}
	}

	public void Play(AudioSFX sfx)
	{
		Play(GetClipForEnum(sfx), volumes[sfx]);
	}

	public override void Setup()
	{
		_sfx = PlayerPrefs.GetInt(PP_KEY_SFX, 1) == 1;
		SetupClipsDictionary();
	}

	private void SetupClipsDictionary()
	{
		foreach (var clip in clips)
		{
			_clipsByName.Add(clip.name, clip);
		}
	}

}
