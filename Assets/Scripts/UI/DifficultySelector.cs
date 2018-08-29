using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DifficultySelector : MonoBehaviour
{
	public Button[] difficultyButtons;
	public Color selectedButtonTint;
	public Color notSelectedButtonTint;

	private Dictionary<Difficulty, int> _buttonIndicesByDifficulty = new Dictionary<Difficulty, int>();
	public Difficulty SelectedDifficulty { get; private set; }

	private void Awake()
	{
		SelectedDifficulty = Difficulty.Medium;
		MapDifficultiesToButtons();
	}

	private void Start()
	{
		UpdateViews();	
	}

	public void OnDifficultyButtonTapped(int difficulty)
	{
		SelectedDifficulty = (Difficulty)difficulty;
		UpdateViews();
	}

	private void UpdateViews()
	{
		var selectedIndex = _buttonIndicesByDifficulty[SelectedDifficulty];

		for (int i = 0; i < difficultyButtons.Length; ++i)
		{
			var buttonTint = i == selectedIndex ? selectedButtonTint : notSelectedButtonTint;
			difficultyButtons[i].targetGraphic.color = buttonTint;
		}
	}

	private void MapDifficultiesToButtons()
	{
		var values = System.Enum.GetValues(typeof(Difficulty));

		// Start from 1 because of ignoring "None" at 0
		for (int i = 1; i < values.Length && i <= difficultyButtons.Length; ++i)
		{
			_buttonIndicesByDifficulty[(Difficulty)values.GetValue(i)] = i - 1;
		}
	}
}
