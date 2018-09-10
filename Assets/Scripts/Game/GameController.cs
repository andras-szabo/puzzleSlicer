using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
#if UNITY_EDITOR
	public Texture2D testTexture;
#endif

	public float imageRevealDuration = 1f;

	public PiecePrefab piecePrefab;

	public PuzzleContainer pContainer;
	public PuzzlePiecePool pool;

	public GameObject loadingScreen;
	public Image loadingBar;

	public StartMenuController startMenu;
	public PlayFieldMover playFieldMover;
	public HelpOverlay helpOverlay;
	public GameObject victoryPanel;

	public MaskContainer maskContainer;

	private Dictionary<IntVector2, PieceInfo> _pieceInfos = new Dictionary<IntVector2, PieceInfo>();
	private uint _backButtonCallbackID;

	private void Start()
	{
		SetAppSettings();

#if !UNITY_EDITOR
		TextureUtility.CleanupLegacyTextureData();
		TryLoadSavedGame();
#endif
	}

#if UNITY_EDITOR
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.H))
		{
			pContainer.ToggleHighlightsInEditor();
		}
	}
#endif

	private void SetAppSettings()
	{
		Application.targetFrameRate = 30;
		Screen.sleepTimeout = SleepTimeout.SystemSetting;
		PuzzleService.Instance.OnPlayerHasWon += HandlePlayerWon;
	}

	private void OnDestroy()
	{
		var ps = PuzzleService.Instance;
		if (ps != null)
		{
			ps.OnPlayerHasWon -= HandlePlayerWon;
		}
	}

	private void HandlePlayerWon()
	{
		playFieldMover.ResetPlayFieldZoomAndPosition();
		ReplacePiecesWithBackgroundOnVictory(playFieldMover.camResetDurationSeconds, imageRevealDuration);
	}

	private void ReplacePiecesWithBackgroundOnVictory(float camResetDurationSeconds, float imgRevealDurationSeconds)
	{
		victoryPanel.gameObject.SetActive(true);
		pContainer.ReplacePiecesWithBackgroundOnVictory(camResetDurationSeconds, 
														imgRevealDurationSeconds);
	}

	public void HelpTapped()
	{
		helpOverlay.gameObject.SetActive(true);
	}

	public void OnExitTapped()
	{
		BackButtonManager.Instance.Pop(_backButtonCallbackID);
		startMenu.gameObject.SetActive(true);
		startMenu.ShowCloseAndResumeButton();
	}

	private void TryLoadSavedGame()
	{
		var savedTextureInfo = TextureUtility.TryGetSavedTextureInfo();
		if (savedTextureInfo != null)
		{
			var textureToSlice = NativeGallery.LoadImageAtPath(savedTextureInfo.originalPath,
															   markTextureNonReadable: false,
															   generateMipmaps: false);
			if (textureToSlice != null)
			{
				StartCoroutine(DoSetupRoutine(textureToSlice, savedTextureInfo.originalPath, savedTextureInfo));
			}
		}
	}

	public void HandleBackButtonUntilFurtherNotice()
	{
		_backButtonCallbackID = BackButtonManager.Instance.PushAndGetBackButtonCallbackID(OnExitTapped);
	}

	public void TryStartNewGame()
	{
#if !UNITY_EDITOR
		TryLoadTextureToSlice();
#else
		StartCoroutine(DoSetupRoutine(testTexture, ""));
#endif
	}

	private void TryLoadTextureToSlice()
	{
		var permission = NativeGallery.GetImageFromGallery(OnImagePicked, "Please pick an image");
		Debug.Log(permission);
	}

	private void OnImagePicked(string imgPath)
	{
		if (!string.IsNullOrEmpty(imgPath))
		{
			var tex = NativeGallery.LoadImageAtPath(imgPath, maxSize: -1,
													markTextureNonReadable: false,
													generateMipmaps: false);

			if (tex != null)
			{
				StartCoroutine(DoSetupRoutine(tex, imgPath));
			}
		}

		startMenu.playButton.interactable = true;
	}

	private void SetLoadStatus(float rate)
	{
		loadingBar.fillAmount = rate;
	}

	private void ShowAndResetLoadingScreen()
	{
		startMenu.playButton.interactable = true;
		startMenu.gameObject.SetActive(false);

		loadingScreen.gameObject.SetActive(true);
		loadingBar.fillAmount = 0f;
	}

	private IEnumerator DoSetupRoutine(Texture2D originalTexture, string originalTexturePath,
									   SlicedTextureInfo savedTextureInfo = null)
	{
		victoryPanel.gameObject.SetActive(false);
		ShowAndResetLoadingScreen();
		BackButtonManager.Instance.Suspend();

		playFieldMover.ResetPlayFieldZoomAndPosition(lerpOverTime: false);

		_pieceInfos.Clear();

		var originalSize = new Vector2(originalTexture.width, originalTexture.height);

		var sliceInfo = savedTextureInfo == null ? ImgSlicer.GetSliceInfo(originalSize, startMenu.SelectedDifficulty)
												 : new SlicingInfo(savedTextureInfo);

		originalTexture.wrapMode = TextureWrapMode.Clamp;
		ImgSlicer.SetupConnections(sliceInfo, _pieceInfos);

		PuzzleService.Instance.Reset();
		pContainer.Setup(sliceInfo, originalTexture);
		pool.Setup(sliceInfo);
		playFieldMover.Reset();

		Texture2D slicedTexture;
		var couldLoadSavedGame = TextureUtility.TryLoadSavedTexture(originalSize, sliceInfo.rows, sliceInfo.columns,
													 savedTextureInfo, originalTexturePath, out slicedTexture);

		if (!couldLoadSavedGame)
		{
			yield return ImgSlicer.CreateAndSaveSlicedTextureRoutine(sliceInfo.rows, sliceInfo.columns,
																	 originalTexture, originalSize, slicedTexture,
																	 originalTexturePath,
																	 maskContainer,
																	 _pieceInfos,
																	 SetLoadStatus);
		}

		slicedTexture.wrapMode = TextureWrapMode.Clamp;
		slicedTexture.Apply(true, true);

		SetupPrefabs(sliceInfo.rows, sliceInfo.columns, slicedTexture, couldLoadSavedGame);

		Resources.UnloadUnusedAssets();
		pool.ScrollToTop();

		DoStartGame();
	}

	private void DoStartGame()
	{
		BackButtonManager.Instance.Resume();
		HandleBackButtonUntilFurtherNotice();
		loadingScreen.gameObject.SetActive(false);
	}

	private void SetupPrefabs(int rows, int columns, Texture2D slicedTexture, bool tryLoadGameState)
	{
		PuzzleService.Instance.Setup(rows * columns, pContainer.topRightBounds.position,
											pContainer.bottomLeftBounds.position);

		var pieceScaleFactor = 1f + (ImgSlicer.PADDING_RATIO * 2f);
		PuzzleService.pieceScaleFactor = pieceScaleFactor;

		GameState gameState = tryLoadGameState ? PuzzleService.Instance.TryLoadSavedGameState() : null;

		for (int col = 0; col < columns; ++col)
		{
			for (int row = 0; row < rows; ++row)
			{
				var pieceAnchor = pContainer.GetPieceAnchor(row, col);
				var anchorInPool = pool.GetPieceAnchor(row, col, columns);

				var isOnPlayField = gameState != null && gameState.IsOnPlayField(col, row);

				var parent = isOnPlayField ? pieceAnchor : anchorInPool;

				var newPrefab = Instantiate<PiecePrefab>(piecePrefab, parent, false);

				var uv0x = (float)col / (float)columns;
				var uv0y = (float)row / (float)rows;

				var uv3x = (float)(col + 1) / (float)columns;
				var uv3y = (float)(row + 1) / (float)rows;

				newPrefab.Setup(slicedTexture, new Vector2(uv0x, uv0y), new Vector2(uv3x - uv0x, uv3y - uv0y),
								col, row, isOnPlayField);

				newPrefab.transform.localPosition = Vector3.zero;

				newPrefab.transform.localScale = new Vector3(pieceScaleFactor, pieceScaleFactor);
				newPrefab.pieceAnchor = pieceAnchor;
				newPrefab.anchorInPool = anchorInPool;

				if (isOnPlayField)
				{
					PuzzleService.Instance.MarkPieceOnPlayingField(newPrefab);
					newPrefab.transform.position = gameState.GetWorldPosition(col, row);
					anchorInPool.gameObject.SetActive(false);
					newPrefab.TryMoveBackgroundToBackgroundDisplay();
				}
			}
		}

		if (tryLoadGameState)
		{
			if (gameState.HasWon)
			{
				ReplacePiecesWithBackgroundOnVictory(0f, 0f);
			}
			else
			{
				PuzzleService.Instance.ConnectLoadedPieces();
			}
		}
	}
}
