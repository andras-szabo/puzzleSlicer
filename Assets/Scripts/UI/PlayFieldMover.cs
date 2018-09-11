using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayFieldMover : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	public PuzzleContainer container;

#if UNITY_EDITOR
	[Range(0.1f, 2f)]
	public float wheelZoomSensitivity = 0.5f;
#endif

	private int _currentTouchCount;
	private float _startingPinchDistance;
	private Vector3 _originalPlayFieldScale = new Vector3(-1f, -1f, -1f);
	private Vector2 _originalPieceSize = new Vector2(-1f, -1f);

	private Coroutine _scaleLerpRoutine;
	private Coroutine _containerPositionResetRoutine;
	public float camResetDurationSeconds;

	public void Init()
	{
		_originalPlayFieldScale = new Vector3(-1f, -1f, -1f);
		_originalPieceSize = new Vector2(-1f, -1f);
	}

	public void ResetPlayFieldZoomAndPosition(bool lerpOverTime = true)
	{
		if (_originalPlayFieldScale.x > 0f)
		{
			BoardContext.Instance.SetPieceDimensions(_originalPieceSize);

			if (_scaleLerpRoutine == null)
			{
				if (lerpOverTime)
				{
					_scaleLerpRoutine = StartCoroutine(LerpScaleToOriginalRoutine());
				}
				else
				{
					container.transform.localScale = _originalPlayFieldScale;
				}
			}
		}

		if (_containerPositionResetRoutine == null)
		{
			if (lerpOverTime)
			{
				_containerPositionResetRoutine = StartCoroutine(LerpPositionToZeroRoutine());
			}
			else
			{
				container.transform.localPosition = Vector3.zero;
			}
		}
	}

	private IEnumerator LerpPositionToZeroRoutine()
	{
		var startingPosition = container.transform.localPosition;
		var elapsedTime = 0f;

		while (elapsedTime < camResetDurationSeconds)
		{
			var positionThisFrame = Vector3.Lerp(startingPosition, Vector3.zero, elapsedTime / camResetDurationSeconds);
			container.transform.localPosition = positionThisFrame;
			elapsedTime += Time.deltaTime;

			yield return null;
		}

		container.transform.localPosition = Vector3.zero;
		_containerPositionResetRoutine = null;
	}

	private IEnumerator LerpScaleToOriginalRoutine()
	{
		var targetScale = _originalPlayFieldScale;
		var startingScale = container.transform.localScale;
		var elapsedTime = 0f;

		while (elapsedTime < camResetDurationSeconds)
		{
			var scaleThisFrame = Vector3.Lerp(startingScale, targetScale, elapsedTime / camResetDurationSeconds);
			container.transform.localScale = scaleThisFrame;
			elapsedTime += Time.deltaTime;

			yield return null;
		}

		container.transform.localScale = targetScale;
		_scaleLerpRoutine = null;
	}

	private void Update()
	{
		UpdateTouchCountAndTryToZoom();
#if UNITY_EDITOR
		CheckMouseWheelZoom();
#endif
	}

#if UNITY_EDITOR
	private void CheckMouseWheelZoom()
	{
		var wheelZoom = Input.GetAxis("Mouse ScrollWheel");

		if (!Mathf.Approximately(wheelZoom, 0f))
		{
			TryZoomBy(1f + (wheelZoom * wheelZoomSensitivity));
		}
	}
#endif

	private void UpdateTouchCountAndTryToZoom()
	{
		var pastTouchCount = _currentTouchCount;
		_currentTouchCount = Input.touchCount;

		if (pastTouchCount != _currentTouchCount)
		{
			if (_currentTouchCount == 2)
			{
				InitializePinchZoom();
			}
		}
		else
		{
			if (_currentTouchCount == 2)
			{
				TryPinchZoom();
			}
		}
	}

	private void TryPinchZoom()
	{
		var currentPinchDistance = GetCurrentPinchDistance();
		var pinchFactor = currentPinchDistance / _startingPinchDistance;
		TryZoomBy(pinchFactor);
		_startingPinchDistance = currentPinchDistance;
	}

	private void InitializePinchZoom()
	{
		_startingPinchDistance = GetCurrentPinchDistance();
	}

	private float GetCurrentPinchDistance()
	{
		var touch0 = Input.GetTouch(0).position;
		var touch1 = Input.GetTouch(1).position;
		return Vector2.SqrMagnitude(touch0 - touch1);
	}

	private void TryZoomBy(float increment)
	{
		var currentScale = container.transform.localScale;

		if (_originalPlayFieldScale.x < 0f)
		{
			RecordOriginalScale(currentScale);
		}

		container.transform.localScale = new Vector3(currentScale.x * increment, currentScale.y * increment, 0f);

		BoardContext.Instance.AdjustPieceDimensions(increment);
	}

	private void RecordOriginalScale(Vector3 scale)
	{
		_originalPlayFieldScale = scale;
		_originalPieceSize = new Vector2(BoardContext.PieceWidthInWorldUnits, 
										 BoardContext.PieceHeightInWorldUnits);
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
	}

	public void OnDrag(PointerEventData eventData)
	{
		container.transform.Translate(new Vector3(eventData.delta.x, eventData.delta.y));
	}

	public void OnEndDrag(PointerEventData eventData)
	{
	}

#if UNITY_EDITOR
	private void TryZoomIn()
	{
		TryZoomBy(1.1f);
	}

	private void TryZoomOut()
	{
		TryZoomBy(0.9f);
	}
#endif
}
