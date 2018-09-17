using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
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
