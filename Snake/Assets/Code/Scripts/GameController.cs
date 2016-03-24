using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[System.Serializable]
public class UIComponents
{
	/// The text elements to update.
	[Tooltip("The text elements to update.")]
	public Text ScoreTxt, TimerTxt, FinalScoreShadowTxt, FinalScoreTxt;
	/// The collection particle system.
	[Tooltip("The collection particle system.")]
	public ParticleSystem CollectionBurst;
	/// The UI screen's to control.
	[Tooltip("The UI screen's to control.")]
	public GameObject MainScreen, GameScreen, PauseScreen, GameOverScreen;
}

/// Responsible for game wide events and behaviour.
public class GameController : MonoBehaviour
{
	public enum GameState {Main, Game, Pause, GameOver};
	/// The current state the game is in.
	[Tooltip("The current state the game is in.")]
	public GameState CurrentGameState;
	
	public UIComponents UserInterfaceComponents;


	/// The score text elements Animator component reference.
	private Animator _ScoreAnim;
	/// The total score accumalated in-game.
	private int _TotalScore = 0;
	/// The amount of seconds that have passed.
	private int _SecondsElapsed = 0;
	/// The amount of minutes that have passed.
	private int _MinutesElapsed = 0;
	/// The amount of hours that have passed.
	private int _HoursElapsed = 0;
	/// is the game currently paused.
	private bool _IsPaused = false;

	void Awake ()
	{
		// Assign references.
		_ScoreAnim = UserInterfaceComponents.ScoreTxt.gameObject.GetComponent<Animator>();
	}

	void Start ()
	{
		// Cycle through all states to ensure there are no UI hangs.
		UpdateGameState(GameState.GameOver);
		UpdateGameState(GameState.Pause);
		UpdateGameState(GameState.Game);
		UpdateGameState(GameState.Main);
		// Disable the score text.
		EndScore();
		// Begin the game in the main screen.
		MainScreen(true);
		// Begin counting the game's elasped time.
		StartCoroutine("Timer");
	}

	void Update ()
	{
		// Check for keyboard input.
		KeyboardInput ();

		if(_TotalScore == 126)
		{
			EndGame (true);
		}
	}

	void KeyboardInput ()
	{
		if(CurrentGameState == GameState.Main)
		{
			if(Input.GetButtonDown("Fire1"))
			{
				UpdateGameState(GameState.Game);
				MainScreen(false);
			}
		}

		if(CurrentGameState == GameState.Game || CurrentGameState == GameState.Pause)
		{
			// Has the player pressed pause?
			if(Input.GetButtonDown("Cancel"))
			{
				// Switch between pause states.
				_IsPaused = !_IsPaused;
				// Pause the game based on the pause state.
				PauseGame(_IsPaused);
			}
		}
		
		// Has the game ended?
		if(CurrentGameState == GameState.GameOver)
		{
			// Has the player pressed space?
			if(Input.GetButton("Jump"))
			{
				// Restart the level.
				Application.LoadLevel(Application.loadedLevel);
			}
			// Has the player pressed enter/return?
			else if(Input.GetKeyDown(KeyCode.Return))
			{
				// Quit the game.
				Application.Quit();
			}
		}
	}

	void UpdateGameState (GameState stateToSwitchTo)
	{
		// Change the current game's state.
		CurrentGameState = stateToSwitchTo;

		// Evaluate which state to change to.
		switch(stateToSwitchTo)
		{
		case GameState.Main:
			DisplayScreen(UserInterfaceComponents.MainScreen);
			break;
		case GameState.Game:
			DisplayScreen(UserInterfaceComponents.GameScreen);
			break;
		case GameState.Pause:
			DisplayScreen(UserInterfaceComponents.PauseScreen);
			break;
		case GameState.GameOver:
			DisplayScreen(UserInterfaceComponents.GameOverScreen);
			break;
		}
	}

	/// Disable's all active screen's and display's the passed in screen.
	void DisplayScreen (GameObject screenToDisplay)
	{
		// Disable all active screens.
		UserInterfaceComponents.MainScreen.SetActive(false);
		UserInterfaceComponents.GameScreen.SetActive(false);
		UserInterfaceComponents.PauseScreen.SetActive(false);
		UserInterfaceComponents.GameOverScreen.SetActive(false);
		// Display the parameter screen .
		screenToDisplay.SetActive(true);
	}

	/// Updates the timer and manages the seconds, minutes and hours.
	IEnumerator Timer ()
	{
		// Increase the elapsed amount of seconds.
		_SecondsElapsed++;

		// Has a minute passed?
		if(_SecondsElapsed == 60)
		{
			// Reset the seconds to 0.
			_SecondsElapsed = 0;
			// Increase the elasped amount of minutes.
			_MinutesElapsed++;
			// Has an hour passed?
			if(_MinutesElapsed == 60)
			{
				// Reset the minutes to 0.
				_MinutesElapsed = 0;
				// Increase the elasped amount of hours.
				_HoursElapsed++;
			}
		}

		// Update the timer text to reflect the time changes.
		UserInterfaceComponents.TimerTxt.text = _HoursElapsed.ToString("#00:") + _MinutesElapsed.ToString("#00:") + _SecondsElapsed.ToString("#00") + " Time Elapsed";

		// Wait for 1 second.
		yield return new WaitForSeconds(1f);

		// Restart the co-routine.
		StartCoroutine("Timer");
	}

	/// Adds to the score with the value passed in.
	public void UpdateScore (int score)
	{
		// Add to the total score.
		_TotalScore += score;
		// Display the new score.
		DisplayScore ();
	}

	/// Display's the current score and emit's particles.
	void DisplayScore ()
	{
		// Activate the score text.
		UserInterfaceComponents.ScoreTxt.gameObject.SetActive(true);
		// Display the new score.
		UserInterfaceComponents.ScoreTxt.text = _TotalScore.ToString();
		// Trigger the score text's animation.
		_ScoreAnim.SetTrigger("Collected");
		// Emit some particles.
		UserInterfaceComponents.CollectionBurst.Emit (20);
		// Begin the countdown to disable the score.
		Invoke("EndScore", 1f);
	}

	/// Disable's the score and stop's the particle emission.
	void EndScore ()
	{
		// Disable the score text.
		UserInterfaceComponents.ScoreTxt.gameObject.SetActive (false);
	}

	/// Stops the instance of the game.
	public void PauseGame (bool shouldPause)
	{
		if(shouldPause)
		{
			// Pause the game.
			Time.timeScale = 0.0f;
			// Change the game's state to paused.
			UpdateGameState(GameState.Pause);
		}
		else
		{
			// Unpause the game.
			Time.timeScale = 1.0f;
			// Change the game's state to the main loop.
			UpdateGameState(GameState.Game);
		}
	}

	/// End's the current game instance and switches to the game over state.
	public void EndGame (bool shouldGameEnd)
	{
		if(shouldGameEnd)
		{
			// Stop the game.
			Time.timeScale = 0.0f;
			// Change the game's state to game over.
			UpdateGameState(GameState.GameOver);
			// Display a prompt to the player.
			DisplayGameOverPrompt ();
		}
		else
		{
			// Resume the game.
			Time.timeScale = 1.0f;
			// Change the game's state to the main loop.
			UpdateGameState(GameState.Game);
		}
	}

	/// Display's the end game statistics.
	void DisplayGameOverPrompt ()
	{
		// Update the final score text elements.
		UserInterfaceComponents.FinalScoreTxt.text = _TotalScore.ToString();
		UserInterfaceComponents.FinalScoreShadowTxt.text = _TotalScore.ToString();
	}

	void MainScreen (bool isInMainScreen)
	{
		if(isInMainScreen)
		{
			Time.timeScale = 0.0f;
		}
		else
		{
			Time.timeScale = 1.0f;
		}
	}
}

