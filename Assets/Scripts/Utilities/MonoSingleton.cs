using UnityEngine;

public class MonoSingleton<T> : MonoWithCachedTransform
								where T : class
{
	public static T Instance { get; private set; }

	public bool dontDestroyOnLoad;

	protected virtual void Awake()
	{
		EnsureSingleInstance();
		Setup();
	}

	protected virtual void OnDestroy()
	{
		Instance = null;
	}

	public virtual void Setup()
	{
	}

	public virtual void Cleanup()
	{
		Destroy(this.gameObject);
	}

	private void EnsureSingleInstance()
	{
		if (Instance != null)
		{
			Debug.LogWarningFormat("[MonoSingleton] {0} already exists ", gameObject.name);
			Destroy(this.gameObject);
		}
		else
		{
			Instance = this.GetComponent<T>();
			if (dontDestroyOnLoad)
			{
				DontDestroyOnLoad(this.gameObject);
			}
		}
	}
}
