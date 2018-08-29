using UnityEngine;

public class MonoWithCachedTransform : MonoBehaviour
{
	private Transform _transform;
	public Transform CachedTransform
	{
		get
		{
			return _transform ?? (_transform = this.transform);
		}
	}
}
