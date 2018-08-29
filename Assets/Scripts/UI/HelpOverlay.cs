using UnityEngine;

public class HelpOverlay : MonoBehaviour
{
	private uint _backButtonCallbackID;

	private void OnEnable()
	{
		_backButtonCallbackID = BackButtonManager.Instance.PushAndGetBackButtonCallbackID(Hide);
	}

	private void OnDisable()
	{
		BackButtonManager.Instance.Pop(_backButtonCallbackID);
	}

	public void Hide()
	{
		this.gameObject.SetActive(false);
	}
}
