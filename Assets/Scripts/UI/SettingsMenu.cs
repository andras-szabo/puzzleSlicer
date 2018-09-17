using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
	public static string PP_KEY_BG_INTENSITY = "bgIntensity";
	public static string PP_KEY_BG_R = "bgR";
	public static string PP_KEY_BG_G = "bgG";
	public static string PP_KEY_BG_B = "bgB";

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
		PlayerPrefs.SetFloat(PP_KEY_BG_INTENSITY, bgHelperIntensitySlider.value);

		var pickedColor = colorPicker.CurrentColor;

		PlayerPrefs.SetFloat(PP_KEY_BG_R, pickedColor.r);
		PlayerPrefs.SetFloat(PP_KEY_BG_G, pickedColor.g);
		PlayerPrefs.SetFloat(PP_KEY_BG_B, pickedColor.b);

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
		bgHelperIntensitySlider.value = intensity;
	}

	public void OnBackgroundIntensityChanged()
	{
		gameController.ChangeHelperBackgroundIntensity(bgHelperIntensitySlider.value);
	}

	public void Hide()
	{
		this.gameObject.SetActive(false);
	}
}
