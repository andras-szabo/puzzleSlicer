using UnityEngine;

public static class Paths
{
	public static string TEXTURE_SAVED = "_activeTexture.png";
	public static string SAVED_TEXTURE_INFO = "_activeTexture.json";
	public static string GAME_STATE = "gameState.json";

	public static string GetFullDataPath(string file)
	{
		return System.IO.Path.Combine(Application.persistentDataPath, file);
	}

	public static string GetFullPathToGameState()
	{
		return GetFullDataPath(GAME_STATE);
	}

	public static string GetFullPathToSavedTexture()
	{
		return GetFullDataPath(TEXTURE_SAVED);
	}

	public static string GetFullPathToSavedTextureInfo()
	{
		return GetFullDataPath(SAVED_TEXTURE_INFO);
	}
}
