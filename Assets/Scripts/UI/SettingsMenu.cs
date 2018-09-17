using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
	[SerializeField] private Slider bgHelperIntensitySlider;
	[SerializeField] private GameController gameController;
	[SerializeField] private ColorPicker colorPicker;

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
		colorPicker.Setup(bgColor, HandleColorPicked);
	}

	private void HandleColorPicked(Color color)
	{
		gameController.ChangePlayfieldBgColor(color);
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
