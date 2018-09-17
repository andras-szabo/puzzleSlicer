using UnityEngine;
using UnityEngine.UI;

public class StartMenuController : MonoBehaviour
{
	public Button closeButton;
	public Button resumeButton;
	public DifficultySelector diffSelector;
	public Button playButton;
	public Text versionLabel;

	public QuitConfirmPopup quitConfirmPopup;
	public GameController gameController;
	public Difficulty SelectedDifficulty { get { return diffSelector.SelectedDifficulty; } }

	private uint _backButtonCallbackID;

	private void Start()
	{
		UpdateVersionLabel();
	}

	private void UpdateVersionLabel()
	{
		versionLabel.text = string.Format("v {0}", Application.version);
	}

	private void OnEnable()
	{
		_backButtonCallbackID = BackButtonManager.Instance.PushAndGetBackButtonCallbackID(ConfirmQuit, removeFromStackAfterCalled: false);
	}

	private void OnDisable()
	{
		var bbManager = BackButtonManager.Instance;
		if (bbManager != null)
		{
			bbManager.Pop(_backButtonCallbackID);
		}
	}

	private void ConfirmQuit()
	{
		quitConfirmPopup.gameObject.SetActive(true);	
	}

	public void OnPlayTapped()
	{
		playButton.interactable = false;
		gameController.TryStartNewGame();
	}

	public void OnCloseTapped()
	{
		this.gameObject.SetActive(false);
		gameController.HandleBackButtonUntilFurtherNotice();
	}

	public void ShowCloseAndResumeButton()
	{
		closeButton.gameObject.SetActive(true);
		resumeButton.gameObject.SetActive(true);
	}
}
