using UnityEngine;
using UnityEngine.EventSystems;

public class PlayFieldMover : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	public PuzzleContainer container;

	private int _currentTouchCount;
	private float _startingPinchDistance;
	private Vector3 _originalPlayFieldScale = new Vector3(-1f, -1f, -1f);
	private Vector2 _originalPieceSize = new Vector2(-1f, -1f);

	public void Reset()
	{
		_originalPlayFieldScale = new Vector3(-1f, -1f, -1f);
		_originalPieceSize = new Vector2(-1f, -1f);
	}

	public void ResetPlayFieldZoomAndPosition()
	{
		if (_originalPlayFieldScale.x > 0f)
		{
			container.transform.localScale = _originalPlayFieldScale;
			PuzzleService.pieceWidth = _originalPieceSize.x;
			PuzzleService.pieceHeight = _originalPieceSize.y;
		}

		container.transform.localPosition = Vector3.zero;
	}

	private void Update() 
	{
		UpdateTouchCount();
	}

	private void UpdateTouchCount()
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

		PuzzleService.pieceWidth *= increment;
		PuzzleService.pieceHeight *= increment;
	}

	private void RecordOriginalScale(Vector3 scale)
	{
		_originalPlayFieldScale = scale;
		_originalPieceSize = new Vector2(PuzzleService.pieceWidth, PuzzleService.pieceHeight);
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
