using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// Responsible for snake specific behaviour.
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Snake : MonoBehaviour
{
	/// Tail Prefab
	[Tooltip("The prefab to be used as the tail.")]
	public GameObject TailPrefab;
	/// The name of the spawned food piece.
	[Tooltip("The name of the spawned food piece.")]
	public string FoodName;
	/// The player's movement speed.
	[Tooltip("The player's movement speed.")]
	public float Speed;
	/// Food Prefab.
	[Tooltip("The food prefab to spawn.")]
	public GameObject FoodPrefab;
	/// Game Boundaries.
	[Tooltip("The Game Boundary")]
	public Transform BoundaryTop, BoundaryBottom, BoundaryLeft, BoundaryRight;

	/// The snake's tail.
	private List<Transform> _SnakeTail = new List<Transform>();
	/// Current direction of movement.
	private Vector2 _CurrentDirection = Vector2.right;
	/// The parent object of the tail pieces.
	private GameObject _TailParent;
	/// Has the snake eaten?
	private bool _HasEaten = false;
	/// Can the player change direction?
	private bool _CanChangeDirection = true;
	/// Reference to gamecontroller component.
	private GameController _GameController;
	/// Reference to the player's Audio Source component.
	private AudioSource _AudioSource;

	void Awake ()
	{
		// Assign References.
		try 
		{
			_GameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
		} 
		catch (System.Exception) 
		{
			Debug.LogError("Please tag a gamecontroller object with a tag of \"GameController\"!");
		}

		_AudioSource = GetComponent<AudioSource>();
		// Create a new GameObject and name it tail.
		_TailParent = new GameObject();
		_TailParent.name = "Tail";
	}

	void Start ()
	{
		//InvokeRepeating ("Move", 0.5f, 0.5f);
		StartCoroutine("Move");
		// Spawn the first food item.
		Invoke("Spawn", 3f);

		Cursor.visible = false;
	}

	void Update() 
	{
		// Check for keyboard input.
		KeyboardMovement();
	}

	void OnTriggerEnter2D (Collider2D other)
	{
		if (other.name == FoodName) 
		{
			// Get longer in next Move call.
			_HasEaten = true;

			// Spawn some new food.
			Invoke("Spawn", .5f);

			// Play the collection sound.
			_AudioSource.Play();

			// Update the score.
			_GameController.UpdateScore(1);
			
			// Remove the food.
			Destroy(other.gameObject);
		}
		else 
		{
			_GameController.EndGame(true);
			StopAllCoroutines();
		}
	}

	/// Register's keyboard input and moves based upon it.
	void KeyboardMovement ()
	{
		// Can the player change direction?
		if(_CanChangeDirection)
		{
			// Is the player moving up?
			if(_CurrentDirection == Vector2.up)
			{
				if (Input.GetKey (KeyCode.RightArrow))
				{
					_CurrentDirection = Vector2.right;
					_CanChangeDirection = false;
				}
				else if (Input.GetKey (KeyCode.LeftArrow))
				{
					_CurrentDirection = -Vector2.right;
					_CanChangeDirection = false;
				}
				else if (Input.GetKey (KeyCode.UpArrow))
				{
					_CurrentDirection = Vector2.up;
					_CanChangeDirection = false;
				}
			}
			// Is the player moving down?
			else if(_CurrentDirection == -Vector2.up)
			{
				if (Input.GetKey (KeyCode.RightArrow))
				{
					_CurrentDirection = Vector2.right;
					_CanChangeDirection = false;
				}
				else if (Input.GetKey (KeyCode.DownArrow))
				{
					_CurrentDirection = -Vector2.up;
					_CanChangeDirection = false;
				}
				else if (Input.GetKey (KeyCode.LeftArrow))
				{
					_CurrentDirection = -Vector2.right;
					_CanChangeDirection = false;
				}
			}
			// Is the player moving left?
			else if(_CurrentDirection == -Vector2.right)
			{
				if (Input.GetKey (KeyCode.DownArrow))
				{
					_CurrentDirection = -Vector2.up;
					_CanChangeDirection = false;
				}
				else if (Input.GetKey (KeyCode.LeftArrow))
				{
					_CurrentDirection = -Vector2.right;
					_CanChangeDirection = false;
				}
				else if (Input.GetKey (KeyCode.UpArrow))
				{
					_CurrentDirection = Vector2.up;
					_CanChangeDirection = false;
				}
			}
			// Is the player moving right?
			else if(_CurrentDirection == Vector2.right)
			{
				if (Input.GetKey (KeyCode.RightArrow))
				{
					_CurrentDirection = Vector2.right;
					_CanChangeDirection = false;
				}
				else if (Input.GetKey (KeyCode.DownArrow))
				{
					_CurrentDirection = -Vector2.up;
					_CanChangeDirection = false;
				}
				else if (Input.GetKey (KeyCode.UpArrow))
				{
					_CurrentDirection = Vector2.up;
					_CanChangeDirection = false;
				}
			}
		}
	}

	/// Moves the snake and it's tail by the current direction.
	IEnumerator Move ()
	{
		yield return new WaitForSeconds(Speed);
		// The player can change direction again.
		_CanChangeDirection = true;
		
		// Save current position (gap will be here)
		Vector2 CurrentPosition = transform.position;
		
		// Move head into new direction (now there is a gap)
		transform.Translate (_CurrentDirection);
		
		// Ate something? Then insert new Element into gap
		if (_HasEaten) 
		{
			// Load Prefab into the world
			var tailClone =(GameObject)Instantiate (TailPrefab, CurrentPosition, Quaternion.identity);

			// Assign the tail piece a parent to clean the project hierarchy.
			tailClone.transform.parent = _TailParent.transform;
			
			// Keep track of it in our tail list
			_SnakeTail.Insert (0, tailClone.transform);
			
			// Reset the flag
			_HasEaten = false;
		}
		// Do we have a Tail?
		else if (_SnakeTail.Count > 0) 
		{
			// Move last Tail Element to where the Head was
			_SnakeTail.Last ().position = CurrentPosition;
			
			// Add to front of list, remove from the back
			_SnakeTail.Insert (0, _SnakeTail.Last());
			_SnakeTail.RemoveAt (_SnakeTail.Count - 1);
		}

		// Restart the move co-routine.
		StartCoroutine("Move");
	}

	/// Spawn one piece of food
	void Spawn()
	{
		// X position between left & right border.
		int x = (int)Random.Range (BoundaryLeft.position.x, BoundaryRight.position.x);
		
		// Y position between top & bottom border.
		int y = (int)Random.Range (BoundaryBottom.position.y, BoundaryTop.position.y);
		
		// Instantiate the food at (x, y).
		var foodClone = (GameObject)Instantiate(FoodPrefab, new Vector2 (x, y), Quaternion.identity);

		// Re-assign the spawned object's name.
		foodClone.name = FoodName;

		// Check to see if the spawn position is not within the player.
		CheckSpawnPosition (foodClone.transform);
	}

	/// Ensure's that the piece is not spawned within the snake.
	void CheckSpawnPosition (Transform positionToCheck)
	{
		// If the piece has spawned within the player's head.
		if(positionToCheck.position == this.transform.position)
		{
			// Destroy the piece and spawn a new one.
			Destroy(positionToCheck.gameObject);
			Spawn();
		}
		// If the piece has spawed in any of the snake's tail pieces.
		for(int i = 0; i < _SnakeTail.Count; i++)
		{
			if(positionToCheck.position == _SnakeTail[i].transform.position)
			{
				// Destroy it and spawn a new one.
				Destroy(positionToCheck.gameObject);
				Spawn();
			}
		}
	}
}