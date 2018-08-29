using System.Collections.Generic;
using UnityEngine;

public class MaskContainer : MonoBehaviour 
{
	public Texture2D flatEdge;
	public Texture2D hook;
	public Texture2D eye;

	private Dictionary<Connection, Color[]> _cachedMaskTextures = new Dictionary<Connection, Color[]>();

	public Color[] GetTextureAsArray(Connection connection)
	{
		Color[] textureAsArray;
		if (_cachedMaskTextures.TryGetValue(connection, out textureAsArray))
		{
			return textureAsArray;
		}

		var mask = GetMask(connection);

		if (mask != null)
		{
			_cachedMaskTextures.Add(connection, mask.GetPixels());
			return _cachedMaskTextures[connection];
		}

		return null;
	}

	public Texture2D GetMask(Connection connection)
	{
		switch (connection)
		{
			case Connection.None: return flatEdge;
			case Connection.Hook: return hook;
			case Connection.Eye: return eye;
		}

		return null;
	}
}
