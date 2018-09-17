using System;
using UnityEngine;
using UnityEngine.UI;

public class ColorPicker : MonoBehaviour
{
	public bool showAlpha;
	public RawImage previewImage;

	[SerializeField] private Slider redSlider;
	[SerializeField] private Slider greenSlider;
	[SerializeField] private Slider blueSlider;
	[SerializeField] private Slider alphaSlider;
	[SerializeField] private Text alphaLabel;

	[SerializeField] private Color defaultColor;

	public Color CurrentColor { get; private set; }

	private Action<Color> _callback;

	private void OnEnable()
	{
		alphaSlider.gameObject.SetActive(showAlpha);
		alphaLabel.gameObject.SetActive(showAlpha);
	}

	public void Setup(Color color, Action<Color> callback)
	{
		SetSlidersTo(color);
		_callback = callback;
	}

	public void OnDefaultColorPicked()
	{
		SetSlidersTo(defaultColor);
	}

	private void SetSlidersTo(Color color)
	{
		CurrentColor = color;

		redSlider.normalizedValue = color.r;
		greenSlider.normalizedValue = color.g;
		blueSlider.normalizedValue = color.b;
		alphaSlider.normalizedValue = color.a;
	}

	private void HandleNewColorPicked(Color pickedColor)
	{
		CurrentColor = pickedColor;

		previewImage.color = pickedColor;
		if (_callback != null)
		{
			_callback(pickedColor);
		}
	}

	public void OnColorChange()
	{
		var pickedColor = new Color(redSlider.normalizedValue, greenSlider.normalizedValue, 
									blueSlider.normalizedValue, alphaSlider.normalizedValue);

		HandleNewColorPicked(pickedColor);
	}

	private void OnDestroy()
	{
		_callback = null;
	}
}
