using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
	public const float IMG_REVEAL_DURATION_SECONDS = 1f;
	public const int TARGET_FRAMERATE = 30;

#if UNITY_EDITOR
	[SerializeField] private Texture2D testTexture;
#endif

	[SerializeField] private PiecePrefab piecePrefabPrototype;
	[SerializeField] private PuzzleContainer puzzleContainer;
	[SerializeField] private Transform pieceOutlineContainer;
	[SerializeField] private PuzzlePiecePool pool;
	[SerializeField] private LoadingScreen loadingScreen;

	[SerializeField] private StartMenuController startMenu;
	[SerializeField] private PlayFieldMover playFieldMover;
	[SerializeField] private HelpOverlay helpOverlay;
	[SerializeField] private SettingsMenu settingsMenu;
	[SerializeField] private GameObject victoryPanel;

	// [SerializeField] private RawImage playFieldBgImage;

	[SerializeField] private MaskContainer maskContainer;

	private uint _backButtonCallbackID;

	private Camera _mainCam;
	private Camera MainCam { get { return _mainCam ?? (_mainCam = Camera.main); } }

	#region Unity lifecycle
	private void Start()
	{
		SetAppSettings();
		SetupServices();

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
			puzzleContainer.ToggleHighlightsInEditor();
		}
	}
#endif

	private void OnApplicationPause(bool pause)
	{
		if (pause)
		{
			ServiceLocator.Get<IGameStateService>().Save();
		}
	}

	private void OnDestroy()
	{
		ServiceLocator.Shutdown();
	}

	#endregion

	public void OnHelpTapped()
	{
		helpOverlay.gameObject.SetActive(true);
	}

	public void OnBringPiecesToFrontTapped()
	{
		ServiceLocator.Get<IBoardService>().BringFreeStandingPiecesToTheFront();
	}

	public void OnSettingsMenuTapped()
	{
		settingsMenu.Setup(puzzleContainer.helperBackgroundImg.color.a, MainCam.backgroundColor);
		settingsMenu.gameObject.SetActive(true);
	}

	public void OnExitTapped()
	{
		BackButtonManager.Instance.Pop(_backButtonCallbackID);
		startMenu.gameObject.SetActive(true);
		startMenu.ShowCloseAndResumeButton();
	}

	public void ChangeHelperBackgroundIntensity(float value)
	{
		puzzleContainer.helperBackgroundImg.color = new Color(1f, 1f, 1f, value);
	}

	public void ChangePlayfieldBgColor(Color color)
	{
		MainCam.backgroundColor = color;
	}

	private void SetupServices()
	{
		ServiceLocator.Register<IBoardService>(new BoardService());
		ServiceLocator.Register<IGameStateService>(new GameStateService());

		ServiceLocator.Get<IBoardService>().OnPlayerHasWon += HandlePlayerWon;
	}

	private void SetAppSettings()
	{
		Application.targetFrameRate = TARGET_FRAMERATE;
		Screen.sleepTimeout = SleepTimeout.SystemSetting;
	}

	private void HandlePlayerWon()
	{
		playFieldMover.ResetPlayFieldZoomAndPosition();
		ReplacePiecesWithBackgroundOnVictory(playFieldMover.camResetDurationSeconds, IMG_REVEAL_DURATION_SECONDS);
	}

	private void ReplacePiecesWithBackgroundOnVictory(float camResetDurationSeconds, float imgRevealDurationSeconds)
	{
		victoryPanel.gameObject.SetActive(true);
		puzzleContainer.ReplacePiecesWithBackgroundOnVictory(camResetDurationSeconds,
															imgRevealDurationSeconds);
	}

	private void TryLoadSavedGame()
	{
		SlicedTextureInfo savedTextureInfo;

		if (TextureUtility.TryGetSavedTextureInfo(out savedTextureInfo))
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
		StartCoroutine(TryLoadTextureToSliceRoutine());
#else
		StartCoroutine(DoSetupRoutine(testTexture, ""));
#endif
	}

	private IEnumerator TryLoadTextureToSliceRoutine()
	{
		PrepareUIForSetup();

		yield return null;

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

	private IEnumerator DoSetupRoutine(Texture2D originalTexture, string originalTexturePath,
									   SlicedTextureInfo savedTextureInfo = null)
	{
		BackButtonManager.Instance.Suspend();
		PrepareUIForSetup();

		var originalSize = new Vector2(originalTexture.width, originalTexture.height);
		var slicingInfo = savedTextureInfo == null ? ImgSlicer.GetSliceInfo(originalSize, startMenu.SelectedDifficulty)
												   : new SlicingInfo(savedTextureInfo);

		InitPlayFieldAndContainers(slicingInfo, originalTexture);

		Texture2D slicedTexture;
		var couldLoadSavedTexture = TextureUtility.TryLoadSavedTexture(originalSize, slicingInfo.rows, slicingInfo.columns,
													 savedTextureInfo, originalTexturePath, out slicedTexture);

		if (!couldLoadSavedTexture)
		{
			var pieceConnections = ImgSlicer.SetupConnections(slicingInfo);
			yield return ImgSlicer.CreateAndSaveSlicedTextureRoutine(slicingInfo, originalTexture, slicedTexture,
																	 originalTexturePath, maskContainer, pieceConnections,
																	 loadingScreen.SetLoadStatus);
		}

		slicedTexture.wrapMode = TextureWrapMode.Clamp;
		slicedTexture.Apply(true, true);

		SetupPrefabs(slicingInfo.rows, slicingInfo.columns, slicedTexture, couldLoadSavedTexture);

		Resources.UnloadUnusedAssets();
		DoStartGame();
	}

	private void InitPlayFieldAndContainers(SlicingInfo slicingInfo, Texture2D originalTexture)
	{
		ServiceLocator.InitAllServices();

		puzzleContainer.Init(slicingInfo, originalTexture);
		pool.Init(slicingInfo);
		playFieldMover.Init();
	}

	private void DoStartGame()
	{
		BackButtonManager.Instance.Resume();
		HandleBackButtonUntilFurtherNotice();
		pool.ScrollToTop();
		loadingScreen.gameObject.SetActive(false);
	}

	private void PrepareUIForSetup()
	{
		startMenu.playButton.interactable = true;
		startMenu.gameObject.SetActive(false);

		loadingScreen.gameObject.SetActive(true);
		loadingScreen.SetLoadStatus(0f);

		victoryPanel.gameObject.SetActive(false);

		playFieldMover.ResetPlayFieldZoomAndPosition(lerpOverTime: false);
	}

	private void SetupPrefabs(int rows, int columns, Texture2D slicedTexture, bool tryLoadGameState)
	{
		var boardService = ServiceLocator.Get<IBoardService>();

		boardService.Setup(rows * columns,
					   pieceOutlineContainer,
					   puzzleContainer.transform);

		var pieceScaleFactor = 1f + (ImgSlicer.PADDING_RATIO * 2f);

		BoardContext.Instance.Setup(pieceScaleFactor, puzzleContainer.topRightBounds.position, 
									puzzleContainer.bottomLeftBounds.position);

		var gameStateService = ServiceLocator.Get<IGameStateService>();
		gameStateService.TryLoad(expectedPieceCount: rows * columns);

		for (int col = 0; col < columns; ++col)
		{
			for (int row = 0; row < rows; ++row)
			{
				var pieceAnchor = puzzleContainer.GetPieceAnchor(row, col);
				var anchorInPool = pool.GetPieceAnchor(row, col, columns);

				var isOnPlayField = gameStateService.IsPieceOnPlayField(col, row);
				var parent = isOnPlayField ? pieceAnchor : anchorInPool;

				var newPrefab = Instantiate<PiecePrefab>(piecePrefabPrototype, parent, false);

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
				newPrefab.pieceOutlineContainer = pieceOutlineContainer;

				if (isOnPlayField)
				{
					boardService.MarkPieceOnPlayField(newPrefab);
					newPrefab.transform.position = gameStateService.GetWorldPosition(col, row);
					anchorInPool.gameObject.SetActive(false);
					newPrefab.TryMoveBackgroundToBackgroundDisplay();
				}
			}
		}

		if (tryLoadGameState)
		{
			if (gameStateService.HasWon)
			{
				ReplacePiecesWithBackgroundOnVictory(0f, 0f);
			}
			else
			{
				boardService.SnapAndConnectLoadedPieces();
			}
		}
	}
}
