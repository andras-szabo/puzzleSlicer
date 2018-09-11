using UnityEngine;

public class QuitConfirmPopup : MonoBehaviour
{
	private uint _backButtonCallbackID;

	private void OnEnable()
	{
		_backButtonCallbackID = BackButtonManager.Instance.PushAndGetBackButtonCallbackID(Close);
	}

	private void OnDisable()
	{
		BackButtonManager.Instance.Pop(_backButtonCallbackID);
	}

	public void TrySaveAndQuit()
	{
		ServiceLocator.Get<IGameStateService>().Save();
		Application.Quit();
	}

	public void Close()
	{
		gameObject.SetActive(false);
	}
}
