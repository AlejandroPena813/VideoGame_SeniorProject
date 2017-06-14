using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Maze_Generation : MonoBehaviour {
	[System.Serializable]

	// Represents single 1-by-1 area of the initial grid
	public class Cell
	{
		// Keeps track of all 4 walls of each cell
		public GameObject top;
		public GameObject bottom;
		public GameObject left;
		public GameObject right;
		// Keeps track of whether the cell has been visited 
		public bool check;
	}


	public float wall_length = 1.0f;

	// 2D game - all walls are a 2-vector object
	public GameObject Wall_Object;
	private Vector2 init;
	private Vector2 spot;

	// Holds all generated walls
	private GameObject holder;

	// Holds all created Cells
	private Cell[] all_cells;

	// Represents number of current cell
	public int curr = 0;

	// Tells if maze is currently being built or not
	private bool maze_under_maintenance = false;

	// Represents current neighboring cell
	private int curr_neigh = 0;

	// Counts the cells visited in Mazify
	private int cells_checked = 0;

	// Stack of cells
	private List<int> last;

	// Holds position in pathway when backing up
	private int back = 0;

	// Helps return chosen wall number to destroy
	private int chosen_wall = 0;

	public GameObject pgs;

	public int total_pages;

	// Initialize game
	void Start () 
	{
		total_pages = 5;
		// Generate new maze
		Start_Creation ();
	}

	void Start_Creation ()
	{
		// Prepare new wall holder named MAZE
		holder = new GameObject ();
		holder.name = "MAZE";


		GameObject opposite_walls;

		// Number of cells on x-axis divided by 2 for center of the screen
		// Add wall_legnth (1) divided by 2 to find left side of init
		// Same for y-axis
		init = new Vector2 ((-40/2) + wall_length / 2, (-30/2) + wall_length / 2);

		// Set new vetcor at initial spot
		Vector2 my_spot = init;


		// Creates x-by-y grid of given sizes
		for (int i = 0; i < 30; i++)				// X-Axis
		{
			for (int j = 0; j <= 40; j++)		// (j <= ...) because need one extra wall to close grid on opposite end
			{
				// Increment spot to spawn new wall depending on input size
				my_spot = new Vector2(init.x + (j*wall_length)- wall_length/2, init.y + (i*wall_length) - wall_length/2);

				// Spawn wall object at spot indicated
				opposite_walls = Instantiate(Wall_Object, my_spot, Quaternion.identity) as GameObject;

				// Let game know this is an outer wall (cannot be broken)

				if (j == 0 || j == 40)
				{
					opposite_walls.name = "outer wall";
				}
				// Child of wall holder
				opposite_walls.transform.parent = holder.transform;

			}
		}

		for (int i = 0; i <= 30; i++)			// Y-Axis
		{
			for (int j = 0; j < 40; j++)
			{
				// Increment spot to spawn new wall
				my_spot = new Vector2(init.x + (j*wall_length), init.y + (i*wall_length) - wall_length);
				// Spawn the wall object at spot indicated
				opposite_walls = Instantiate(Wall_Object, my_spot, Quaternion.Euler(0.0f, 0.0f, 90.0f)) as GameObject;

				// Let game know this is an outer wall (cannot be broken)
				if (i == 0 || i == 30)
				{
					opposite_walls.name = "outer wall";
				}

				// Child of wall holder
				opposite_walls.transform.parent = holder.transform;
			}
		}

		// Convert spaces in grid to cells
		Cells ();
	}

	// Instantiate cells after all walls are generated and placed in the holder
	void Cells ()
	{
		// Array of wall objects
		GameObject[] every_wall;

		// Instantiate stack
		last = new List<int> ();
		last.Clear ();

		// Number of children in the holder of walls
		int num_children = holder.transform.childCount;

		every_wall = new GameObject[num_children];
		all_cells = new Cell[1200];
		int LR_assign = 0, child_assign = 0, count = 0;

		// Gets all children (wall objects) into array from holder
		for (int i = 0; i < num_children; i++) 
		{
			every_wall [i] = holder.transform.GetChild (i).gameObject;
		}


		for (int assignment = 0; assignment < all_cells.Length; assignment++)
		{
			// Creates new Cell in array of Cells
			all_cells [assignment] = new Cell ();

			// Assigns left wall of cell depending on incremented LR_assign value
			all_cells [assignment].left = every_wall [LR_assign];

			// Assigns bottom wall of cell
			all_cells [assignment].bottom = every_wall [child_assign + (40+1) * 30];

			// Since there is nothing next to 5th cell of each row,...
			// count must be reset
			if (count == 40) {
				LR_assign = LR_assign + 2;
				count = 0;
			}
			else 
			{
				LR_assign++;
			}

			count++;
			child_assign++;

			// Assigns top and right walls of cell
			all_cells [assignment].right = every_wall [LR_assign];
			all_cells [assignment].top = every_wall [(child_assign + (40+1) * 30) + 40-1];
		}

		Mazify ();
	}

	void Mazify ()
	{
		while (cells_checked < 1200) 
		{
			if (maze_under_maintenance) {		// If start wall chosen...
				Choose_Destroy ();
				if (all_cells [curr_neigh].check == false && all_cells [curr].check == true) 
				{
					// Destroy wall if neighboring cell has not been checked yet
					Destroy_Wall ();

					// Notify the program that this cell has been checked
					all_cells [curr_neigh].check = true;
					cells_checked = cells_checked + 1;

					// Add cell to stack of cells
					last.Add (curr);

					// Set current cell to this cell's neighbor in which wall was destroyed
					curr = curr_neigh;

					// Decrement backing up value if stack exists
					if (last.Count > 0) 
					{
						back = last.Count - 1;
					}
				}
			}
			else 
			{
				// Chooses random start cell to begin destroying walls
				curr = Random.Range (0, 1200);
				all_cells [curr].check = true;
				cells_checked++;
				maze_under_maintenance = true;
			}
		}
		Spawn_Pages ();
	}

	void Spawn_Pages ()
	{
		for (int number = 0; number < total_pages; number++) 
		{
			spot = new Vector2 (Random.Range (-20, 19) + 0.5f, Random.Range (-15, 14));
			Instantiate (pgs, spot, Quaternion.identity);
		}
	}

	// Destroys wall based on the number chosen in Choose_Destroy ()
	void Destroy_Wall ()
	{
		if (chosen_wall == 1) 
		{
			Destroy (all_cells [curr].top);
		} 
		else if (chosen_wall == 2) 
		{
			Destroy (all_cells [curr].left);
		} 
		else if (chosen_wall == 3) 
		{
			Destroy (all_cells [curr].right);
		} 
		else if (chosen_wall == 4) 
		{
			Destroy (all_cells [curr].bottom);
		} 
		else {}
	}

	void Choose_Destroy ()
	{
		// Variable to check if we are cornering the cell
		int corner = ((((curr + 1) / 40) - 1) * 40) + 40;
		// 1 added to current cell number, div the number of columns minus 1
		// then multiply and add by the total number of columns.
		// Note that this is integer division, so the answer will not always be equivalent to (current cell # + 1)
		// This ensures that any number that is one less than a multitude of # of columns = (current cell number + 1)
		// For example: for a 5 column grid, cell numbers that will not have a right cell are 4, 9, 14, 19, 24

		// List of 4 surrounding cells of current cell
		int[] cells_on_sides = new int[4];

		int[] shared_wall = new int[4];

		// Value to increment through cells_on_sides array
		int i = 0;

		// Finds cell on the RIGHT of current cell
		if (curr + 1 < 1200 && (curr + 1) != corner) 
		{
			// Fill array of cells_on_sides with RIGHT cell if the cell number is 
			if (all_cells[curr + 1].check == false)
			{
				cells_on_sides [i] = curr + 1;
				shared_wall [i] = 3;
				i++;
			}

		}

		// Finds cell on the LEFT of current cell
		if (curr - 1 >= 0 && curr != corner) 
		{
			if (all_cells[curr - 1].check == false)
			{
				cells_on_sides [i] = curr - 1;
				shared_wall [i] = 2;
				i++;
			}
		}

		// Finds cell on the TOP of current cell
		if (curr + 40 < 1200) 
		{
			if (all_cells[curr + 40].check == false)
			{
				cells_on_sides [i] = curr + 40;
				shared_wall [i] = 1;
				i++;
			}
		}

		// Finds cell on the BOTTOM of current cell
		if (curr - 40 >= 0) 
		{
			if (all_cells[curr - 40].check == false)
			{
				cells_on_sides [i] = curr - 40;
				shared_wall [i] = 4;
				i++;
			}
		}

		// If there are neighbors, randomly pick one of the walls around cell
		if (i != 0) {
			int pick;
			pick = Random.Range (0, i);
			curr_neigh = cells_on_sides [pick];
			chosen_wall = shared_wall [pick];
		} 
		else 
		{
			// If neighbors not found, backup and choose cell before current
			if (back > 0) 
			{
				curr = last [back];
				back = back - 1;
			}
		}
	}

	// Update is called once per frame
	void Update () 
	{

	}
}
