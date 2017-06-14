using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player_Script : MonoBehaviour {

	public Maze_Generation maze;
	private Rigidbody rb;
	public float speed;
	public int num_breaks = 5;
	private int num_pages;
	public Text pgsText;
	public Text wintext;
	public Text brktext;

	void Start ()
	{
		rb = GetComponent<Rigidbody> ();
		rb.freezeRotation = true;
		num_pages = 0;
		wintext.enabled = false;
		Display_Breaks_Left ();
	}

	void FixedUpdate ()
	{
		if (Input.GetKey ("up"))
			transform.Translate (0, 3.5f * Time.deltaTime, 0);
		if (Input.GetKey ("right"))
			transform.Translate (3.5f * Time.deltaTime, 0, 0);
		if (Input.GetKey ("left"))
			transform.Translate (-(3.5f * Time.deltaTime), 0, 0);
		if (Input.GetKey ("down"))
			transform.Translate (0, -(3.5f * Time.deltaTime), 0);
		if (Input.GetKey(KeyCode.Escape))
		{
			Application.LoadLevel(0);
		}
		rb.freezeRotation = true;
	}

	void OnCollisionEnter (Collision the_object) ////////////
	{
		if (the_object.gameObject.CompareTag ("Page")) {
			the_object.gameObject.SetActive (false);
			num_pages += 1;
			num_breaks += 1;
			Display_Breaks_Left ();
			Display_Page_Count ();
		} 
		else if (the_object.gameObject.CompareTag ("Enemy")) 
		{
			StartCoroutine (End_Message (4, 1));
			GetComponent<SpriteRenderer> ().enabled = false;
			// Need to go back to main menu
		}
	}

	void OnCollisionStay (Collision wall)
	{
		rb.isKinematic = true;
		rb.isKinematic = false;
		if (num_breaks > 0 && wall.transform.name != "outer wall") 
		{
			if (Input.GetKey ("space"))
			{
				Destroy (wall.gameObject);
				num_breaks--;
				Display_Breaks_Left ();
			}
		}
	}

	void Display_Page_Count ()
	{
		pgsText.text = "Pages Collected: " + num_pages.ToString () + " / ";
		pgsText.text += Get_Total_Pages ();

		if (num_pages >= Get_Total_Pages ()) 
		{
			StartCoroutine (End_Message (4, 0));
			// Need to go back to main menu
		}
	}

	IEnumerator End_Message (int seconds, int state)
	{
		if (state == 0) 
		{
			wintext.text = "All Pages FOUND, You Survived!";
		} 
		else if (state == 1) 
		{
			wintext.text = "You have been KILLED, GAME OVER";
			//Destroy (this);
		}

		wintext.enabled = true;
		yield return new WaitForSeconds (seconds);
		wintext.enabled = false;

		Application.LoadLevel(0);
	}

	int Get_Total_Pages ()
	{
		return GameObject.Find ("Generation").GetComponent<Maze_Generation> ().total_pages;
	}

	void Display_Breaks_Left ()
	{
		brktext.text = "Wall breaks left: " + num_breaks.ToString ();
	}
}
