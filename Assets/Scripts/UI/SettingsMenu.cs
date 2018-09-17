using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
	[SerializeField] Slider bgHelperIntensitySlider;
	[SerializeField] GameController gameController;

	private uint _backButtonCallbackID;

	private void OnEnable()
	{
		_backButtonCallbackID = BackButtonManager.Instance.PushAndGetBackButtonCallbackID(Hide);
	}

	private void OnDisable()
	{
		BackButtonManager.Instance.Pop(_backButtonCallbackID);
	}

	public void Setup(float bgHelperIntensity, Color bgColor)
	{
		SetupBgHelperIntensitySlider(bgHelperIntensity);
	}

	public void SetupBgHelperIntensitySlider(float intensity)
	{
		bgHelperIntensitySlider.normalizedValue = intensity;
	}

	public void OnBackgroundIntensityChanged()
	{
		gameController.ChangeHelperBackgroundIntensity(bgHelperIntensitySlider.normalizedValue);
	}

	public void Hide()
	{
		this.gameObject.SetActive(false);
	}
}
