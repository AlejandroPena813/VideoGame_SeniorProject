using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E_C : MonoBehaviour {

	//------------------------------------------Linked List Stack

	public class Node
	{
		public Vector2 spot;
		public int dir_choice;
		public string came_from;
		public Node next;
		public Node prev;
	}

	public class Linked_Stack
	{
		public Node head;
		public Node curr;
		public Node tail;
		public int size;

		public Linked_Stack ()
		{
			head = null;
			tail = null;
			size = 0;

		}

		public void Push (Node new_node)
		{
			size++;
			new_node.dir_choice = 0;
			new_node.next = null;

			if (head != null)
			{
				tail.next = new_node;
				new_node.prev = tail;
				tail = new_node;
			}
			else
			{
				head = new_node;
				curr = head;
				tail = new_node;
				new_node.prev = null;
			}
		}

		public void Pop ()
		{
			tail = tail.prev;
			tail.next = null;
		}

		public Vector2 Top_Vec ()
		{
			return tail.spot;
		}

		public string Top_from ()
		{
			return tail.came_from;
		}

		public int Top_dir_choice ()
		{
			return tail.dir_choice;
		}

		public void Update_Top_dir_choice (int dir)
		{
			tail.dir_choice = dir;
		}

		public void Clear_Stack ()
		{
			head = null;
			tail = null;	// added after works
			size = 0;
		}
	}

	//---------------------------------------------------------------Public Variables

	private Rigidbody rb;
	List<Vector2> real_path = new List<Vector2> ();
	Linked_Stack Path = new Linked_Stack ();
	List <Vector2> visited = new List <Vector2> ();
	int action = 0;
	Vector2 destination;
	int current = 0;
	bool new_path_found = false;
	bool returned_to_start = false;
	bool arrived = false;
	bool arrived_at_start = false;
	bool no_where_to_go = false;
	bool found_player = false;						// added
	bool move_faster = false;
	Vector2 player_spot;
	int count = 0;
	bool moving = false;

	//--------------------------------------------------------------------------START

	void Start () {
		rb = GetComponent<Rigidbody> ();
		// Spawn anywhere on left wall of map
		this.transform.position = new Vector2 (-19.5f, Random.Range (-15, 14));

		// Make node of current spot, add to all lists
		Node node = new Node ();
		node.spot = transform.position;
		node.came_from = "START";
		Path.Push (node);
		real_path.Add (node.spot);
		visited.Add (transform.position);
	}
	
	//-----------------------------------------------------------------------------------------------------------------------------------UPDATE
	void Update () {

		if (!found_player && Player_Search (transform.position))
		{
			found_player = true;
		}

		// Choose waypint to approach
		while (action == 0) 
		{
			Debug.Log (action);
			destination = Choose_Waypoint ();
			Debug.Log (Path.head.spot);
			Debug.Log (Path.head.came_from);
			action = 1;
		}

		// Approach waypoint by filling in stack and real path
		while (action == 1) 
		{
			Debug.Log (action + " " + Path.Top_Vec ());
			while (Path.Top_Vec () != destination) {
				Node new_node = new Node ();
				Vector2 next_spot = new Vector2 ();
				next_spot = Path.Top_Vec ();

				if (check_dir (next_spot, Vector3.up) == 0 && Path.Top_from () != "NORTH" && !Check_If_Visited (next_spot, Vector3.up)) {
					Update_Lists (Vector3.up, next_spot, new_node);
				} else if (check_dir (next_spot, Vector3.left) == 0 && Path.Top_from () != "WEST" && !Check_If_Visited (next_spot, Vector3.left)) {
					Path.Update_Top_dir_choice (1);
					Update_Lists (Vector3.left, next_spot, new_node);
				} else if (check_dir (next_spot, Vector3.down) == 0 && Path.Top_from () != "SOUTH" && !Check_If_Visited (next_spot, Vector3.down)) {
					Path.Update_Top_dir_choice (2);
					Update_Lists (Vector3.down, next_spot, new_node);
				} else if (check_dir (next_spot, Vector3.right) == 0 && Path.Top_from () != "EAST" && !Check_If_Visited (next_spot, Vector3.right)) {
					Path.Update_Top_dir_choice (3);
					Update_Lists (Vector3.right, next_spot, new_node);
				} else { // Last node of stack has no where to go, START BACKTRACKING
					new_node = null;
					no_where_to_go = true;
					action = 2;
				}
					
				if (no_where_to_go == true) {
					no_where_to_go = false;
					Debug.Log (next_spot);
					break;
				}
			}

			if (Path.Top_Vec () == destination) {
				visited.Clear ();
				Path.Clear_Stack ();
				action = 5;
			}

		}

		// BACKTRACK until new path found
		while (action == 2)
		{
			Debug.Log (action);
			while (!new_path_found) {

				// Pops nodes with all directions checked, add to real path
				while (Path.Top_dir_choice () == 3 && Path.Top_from () != "START") {
					Path.Pop ();
					real_path.Add (Path.Top_Vec ());
				}

				// Instantiate node to reference Top Vector
				Node new_node = new Node ();
				Vector2 next_spot = new Vector2 ();
				next_spot = Path.Top_Vec ();


				if (Path.Top_dir_choice () == 0) {
					// If only top_node-UP checked, check last 3 directions
					if (check_dir (next_spot, Vector3.left) == 0 && Path.Top_from () != "WEST" && !Check_If_Visited (next_spot, Vector3.left)) {
						Path.Update_Top_dir_choice (1);
						Update_Lists (Vector3.left, next_spot, new_node);
						new_path_found = true;
					} else if (check_dir (next_spot, Vector3.down) == 0 && Path.Top_from () != "SOUTH" && !Check_If_Visited (next_spot, Vector3.down)) {
						Path.Update_Top_dir_choice (2);
						Update_Lists (Vector3.down, next_spot, new_node);
						new_path_found = true;
					} else if (check_dir (next_spot, Vector3.right) == 0 && Path.Top_from () != "EAST" && !Check_If_Visited (next_spot, Vector3.right)) {
						Path.Update_Top_dir_choice (3);
						Update_Lists (Vector3.right, next_spot, new_node);
						new_path_found = true;
					} else {
						Path.Update_Top_dir_choice (3);
						new_node = null;
					}
				} else if (Path.Top_dir_choice () == 1) {
					// If top_node-left checked, check last 2 directions
					if (check_dir (next_spot, Vector3.down) == 0 && Path.Top_from () != "SOUTH" && !Check_If_Visited (next_spot, Vector3.down)) {
						Path.Update_Top_dir_choice (2);
						Update_Lists (Vector3.down, next_spot, new_node);
						new_path_found = true;
					} else if (check_dir (next_spot, Vector3.right) == 0 && Path.Top_from () != "EAST" && !Check_If_Visited (next_spot, Vector3.right)) {
						Path.Update_Top_dir_choice (3);
						Update_Lists (Vector3.right, next_spot, new_node);
						new_path_found = true;
					} else {
						Path.Update_Top_dir_choice (3);
						new_node = null;
					}
				} else if (Path.Top_dir_choice () == 2) {
					// If  top_node-down checked, check last direction
					if (check_dir (next_spot, Vector3.right) == 0 && Path.Top_from () != "EAST" && !Check_If_Visited (next_spot, Vector3.right)) {
						Path.Update_Top_dir_choice (3);
						Update_Lists (Vector3.right, next_spot, new_node);
						new_path_found = true;
					} else {
						Path.Update_Top_dir_choice (3);
						new_node = null;
					}
				} else if (Path.Top_from () == "START") {
					// If top_node is START NODE & all directions checked, action = 3, no new path found
					returned_to_start = true;
					new_node = null;
					// remove last node of stack, path stack now empty
					Path.Pop ();
					break;
				}
			}

			if (returned_to_start == true) {
				returned_to_start = false;
				action = 3;
			} else
				action = 1;
			new_path_found = false;
		}

		// Add vectors to real path until a wall is broken, then return to action 1
		while (action == 3 && arrived_at_start == true)
		{
			Debug.Log (action);
			if (Move_Until_Break (Vector3.right) == 0) 
				break;
			else if (Move_Until_Break (Vector3.down) == 0) 
				break;
			else if (Move_Until_Break (Vector3.left) == 0) 
				break;
			else if (Move_Until_Break (Vector3.up) == 0) 
				break;
		}

		if (current < real_path.Count)
		{
			if (!move_faster) 														//added
				transform.position = Vector2.MoveTowards (transform.position, real_path[current], Time.deltaTime *4.0f);
			else 
				transform.position = Vector2.MoveTowards (transform.position, real_path[current], Time.deltaTime *4.5f);

			if ((Vector2)transform.position == real_path[current]) 
			{
				if (found_player && count < 4) {													//added
					if (count == 0) {
						Debug.Log ("chase " + count);
						Debug.Log (player_spot);
						Path.Clear_Stack ();
						visited.Clear ();
						real_path.Clear ();
						current = 0;
						real_path.Add (transform.position);
						Vector2 new_spot = round_to_reachable_vector (player_spot); 			//changed
						Debug.Log (new_spot);
						move_faster = true;
						action = 5;
						real_path.Add (new_spot);
						count++;
					} else {
						Player_Search (transform.position);
						Debug.Log (count);
						Vector2 new_spot = round_to_reachable_vector (player_spot);				//changed
						Debug.Log (new_spot);
						real_path.Add (new_spot);
						count++;
					} 
				}
				if (count == 4) {
					Debug.Log (count);
					count = 0;
					move_faster = false;
					found_player = false;
					GetComponent<SpriteRenderer> ().enabled = false;
					GetComponent<SphereCollider> ().enabled = false;
					StartCoroutine (Waiting());
					/**/
				}
				current++;
			}
		}

		if (current == real_path.Count-1 && real_path.Count != 0 && !found_player) {
			real_path.Clear ();
			current = 0;

		}

		rb.freezeRotation = true;
		rb.isKinematic = true;
		rb.isKinematic = false;
	}

	//-------------------------------------------------------------------------------------------------------------------------------FUNCTIONS

	Vector2 Choose_Waypoint ()
	{
		Vector2 waypoint = new Vector2 (Random.Range (-20, 19) + 0.5f, Random.Range (-15, 14));
		Debug.Log ("waypoint: " + waypoint);
		Node new_node = new Node ();
		new_node.spot = transform.position;
		new_node.came_from = "START";
		Path.Push (new_node);
		Path.size++;
		visited.Add ((Vector2)transform.position);
		real_path.Add ((Vector2)transform.position);
		Debug.Log ("node: " + new_node.spot);
		Debug.Log ("Path: " + Path.head.spot);
		return waypoint;
	}

	IEnumerator Waiting ()
	{
		yield return new WaitForSeconds (4);
		GetComponent<SpriteRenderer> ().enabled = true;
		GetComponent<SphereCollider> ().enabled = true;
		action = 0;
	}
		
	public int check_dir (Vector3 spot, Vector3 dir)
	{
		RaycastHit contact;
		int layer_mask = LayerMask.GetMask ("Default");
		Ray line = new Ray (spot, dir);

		if (Physics.Raycast (line, out contact, 0.5f, layer_mask)) 
		{
			if (contact.collider.transform.name != "outer wall")
				// If collider in this direction and NOT OUTER WALL... return 1
				return 1;
			else
				return 11; // else return 11 if OUTER WALL
		}
		return 0; // else return 0 if no collider
	}

	public bool Player_Search (Vector3 spot)											// added
	{
		int layer_mask = LayerMask.GetMask ("Player Layer");

		Collider[] hits = Physics.OverlapSphere (spot, 4.5f, layer_mask);
		for (int i = 0; i < hits.Length; i++)
		{
			if (hits[i].tag == "Player") {
				player_spot = hits[i].transform.position;
				return true;
			}
		}
		return false;
	}

	public Vector2 round_to_reachable_vector (Vector2 orig_spot)
	{
		Vector2 the_spot;

		float x = orig_spot.x * 2;
		x = Mathf.Round (x);
		x /= 2;

		if (x % 1 == 0 && x != 20)
			x += 0.5f;
		else if (x % 1 == 0)
			x -= 0.5f;

		float y = orig_spot.y;
		y = Mathf.Round (y);

		the_spot.x = x;
		the_spot.y = y;

		return the_spot;
	}

	bool Check_If_Visited (Vector2 next_spot, Vector3 direction)
	{
		if (direction == Vector3.up)
			next_spot.y += 1.0f;
		else if (direction == Vector3.left) 
			next_spot.x -= 1.0f;
		else if (direction == Vector3.down) 
			next_spot.y -= 1.0f;
		else if (direction == Vector3.right) 
			next_spot.x += 1.0f;

		for (int i = 0; i < visited.Count; i++)
		{
			if (visited[i] == next_spot)
			{
				return true;
			}
		}
		return false;
	}

	void Update_Lists (Vector3 direction, Vector2 next_spot, Node new_node)
	{
		if (direction == Vector3.up) {
			next_spot.y += 1.0f;
			new_node.came_from = "SOUTH";
		} else if (direction == Vector3.left) {
			next_spot.x -= 1.0f;
			new_node.came_from = "EAST";
		} else if (direction == Vector3.down) {
			next_spot.y -= 1.0f;
			new_node.came_from = "NORTH";
		} else if (direction == Vector3.right) {
			next_spot.x += 1.0f;
			new_node.came_from = "WEST";
		}

		new_node.spot = next_spot;
		Path.Push (new_node);
		Path.size++;
		visited.Add (next_spot);
		real_path.Add (next_spot);
	}

	int Move_Until_Break (Vector3 direction)
	{
		bool normal_wall = false;
		Vector3 start = (Vector3)real_path [real_path.Count-1];

		if (direction == Vector3.right) {
			
			while (check_dir (start, Vector3.right) == 0) {
				start.x += 1.0f;
				real_path.Add ((Vector2)start);
			}

			if (check_dir (start, Vector3.right) == 1) {
				start.x += 1.0f;
				real_path.Add ((Vector2)start);
				normal_wall = true;
			}
				
		} else if (direction == Vector3.down) {
			
			while (check_dir (start, Vector3.down) == 0) {
				start.y -= 1.0f;
				real_path.Add ((Vector2)start);
			}

			if (check_dir (start, Vector3.down) == 1) {
				start.y -= 1.0f;
				real_path.Add ((Vector2)start);
				normal_wall = true;
			}

		} else if (direction == Vector3.left) {
			
			while (check_dir (start, Vector3.left) == 0) {
				start.x -= 1.0f;
				real_path.Add ((Vector2)start);
			}

			if (check_dir (start, Vector3.left) == 1) {
				start.x -= 1.0f;
				real_path.Add ((Vector2)start);
				normal_wall = true;
			}

		} else if (direction == Vector3.up) {
			
			while (check_dir (start, Vector3.up) == 0) {
				start.y += 1.0f;
				real_path.Add ((Vector2)start);
			}

			if (check_dir (start, Vector3.up) == 1) {
				start.y += 1.0f;
				real_path.Add ((Vector2)start);
				normal_wall = true;
			}
		}

		if (normal_wall == true) {
			Node new_node = new Node ();
			new_node.spot = (Vector2)start;
			new_node.came_from = "START";
			Path.Push (new_node);
			arrived_at_start = false;
			action = 1;
			return 0;

		} else
			return 1;
	}
		
	void OnCollisionStay (Collision wall)
	{
		if (wall.transform.tag == "Wall" && wall.transform.name != "outer wall")
		{	Debug.Log ("on collisin log");
			Destroy (wall.gameObject);
		}

		if (wall.transform.tag == "Page")
		{
			Physics.IgnoreCollision (wall.collider, GetComponent<Collider> ());
		}

		rb.freezeRotation = true;
	}
}
