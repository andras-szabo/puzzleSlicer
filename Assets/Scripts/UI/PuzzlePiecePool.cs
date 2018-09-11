using UnityEngine;
using UnityEngine.UI;

public class PuzzlePiecePool : MonoSingleton<PuzzlePiecePool>
{
	public Transform poolAnchor;
	public Transform scrollRectContent;
	public ScrollRect scrollRect;

	private Transform[] anchors;

	public override void Cleanup()
	{
		if (anchors != null)
		{
			for (int i = 0; i < anchors.Length; ++i)
			{
				Destroy(anchors[i].gameObject);
			}

			anchors = null;
		}
	}

	public void Init(SlicingInfo sliceInfo)
	{
		Cleanup();

		var pieceCount = sliceInfo.rows * sliceInfo.columns;
		anchors = new Transform[pieceCount];

		poolAnchor.gameObject.SetActive(true);

		for (int i = 0; i < pieceCount; ++i)
		{
			anchors[i] = Instantiate<Transform>(poolAnchor, scrollRectContent);	
		}

		Shuffle(anchors);

		poolAnchor.gameObject.SetActive(false);
	}

	private void Shuffle(Transform[] anchors)
	{
		for (int i = 0; i < anchors.Length - 1; ++i)
		{
			var sourceIndex = Random.Range(0, i + 1);
			var destIndex = Random.Range(i, anchors.Length);

			if (sourceIndex != destIndex)
			{
				var tmp = anchors[sourceIndex];
				anchors[sourceIndex] = anchors[destIndex];
				anchors[destIndex] = tmp;
			}
		}
	}

	public Transform GetPieceAnchor(int row, int column, int columns)
	{
		var index = column + (row * columns);
		return anchors[index];
	}

	public void ScrollToTop()
	{
		scrollRect.verticalNormalizedPosition = 1f;
	}
}
