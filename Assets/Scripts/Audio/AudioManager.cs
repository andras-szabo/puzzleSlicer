using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoSingleton<AudioManager>
{
	public static string PP_KEY_SFX = "sfx";
	public static string GetClipForEnum(AudioSFX sfx)
	{
		switch (sfx)
		{
			case AudioSFX.ButtonClick: return "click";
			case AudioSFX.PieceClick: return "click_high";
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

	private Dictionary<string, AudioClip> _clipsByName = new Dictionary<string, AudioClip>();

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

	public void Play(AudioSFX sfx, float volumePercent = 100f)
	{
		Play(GetClipForEnum(sfx), volumePercent);
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
