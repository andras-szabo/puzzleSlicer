using UnityEngine;

public class FlashTester : MonoBehaviour
{
	public MaterialFlasher flasher;

	[Range(0.2f, 5f)]
	public float flashDurationSeconds;

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			DoRandomTest();
		}
	}

	private void DoRandomTest()
	{
		var targetColor = new Color(Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), 1f);
		flasher.Flash(flashDurationSeconds, targetColor, true);
	}
}
