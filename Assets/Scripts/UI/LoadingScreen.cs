using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
	[SerializeField] private Image loadingBar;

	public void SetLoadStatus(float rate)
	{
		loadingBar.fillAmount = rate;
	}
}
