using UnityEngine;
using System.Collections;

public class trackScript : MonoBehaviour {
	public Transform snap;

	public bool placing = true;
	public int overlap = 0, occupied = 0;
	public string folder;

	public Material redMaterial;
	public Renderer rend;
	private Material[] realMats, redMats;

	// Use this for initialization
	void Start () {
		//set the parent for ease of applying functions to all children of type
		transform.SetParent (GameObject.Find (folder).transform);

		//snap controls where object is placed
		snap = GameObject.Find ("gridSnap").transform;

		//objects turn red when they cannot be placed, 
		redMats = new Material[rend.materials.Length];
		for(int i = 0; i < rend.materials.Length; i++)
			redMats[i] = redMaterial;
		//this makes sure we can set them back to their normal color 
		realMats = new Material[rend.materials.Length];
		rend.materials.CopyTo(realMats, 0);

		//change the gamestate to placing (probably overwriting "menu")
		trackUtility.utility.gameState ("placing");
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (gameObject.isStatic || !placing) {
			//we want to keep this game object, but ignore the update usually...
			return;
		}
		else if (trackUtility.utility.gameState () == "menu") {
			//if gamestate goes back to the menu, delete this object
			Debug.Log ("deleted");
			Destroy (gameObject);
		} else if (trackUtility.utility.gameState () == "placing") {
			//while we are placing, update this objects position and rotation based on the snap
			transform.position = snap.position;
			transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, snap.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
		}
	}

	void OnTriggerEnter (Collider other){
		if (placing && other.gameObject.tag == "objects") {
			overlap++;
			rend.materials = redMats;
		} else if (!placing && other.gameObject.tag == "train") {
			occupied++;
			rend.materials = redMats;
		}
	}

	void OnTriggerExit (Collider other){
		if (placing && other.gameObject.tag == "objects") {
			overlap--;
			if (overlap == 0)
				rend.materials = realMats;
		} else if (!placing && other.gameObject.tag == "train") {
			occupied--;
			if (occupied == 0)
				rend.materials = realMats;
		}
	}

	void clicked(){
		if (
			(trackUtility.utility.gameState () == "removeObject" && gameObject.transform.parent.gameObject.name == "objects")
			|| (trackUtility.utility.gameState () == "removeTrack" && gameObject.transform.parent.gameObject.name == "track")) {
			foreach (GameObject pago in GameObject.FindGameObjectsWithTag ("pathPoint")) {
				foreach (pathPoint pa in pago.GetComponents<pathPoint> ()) {
					if (pa.getID () == GetInstanceID ()) {
						pa.removePoint ();
					}
				}
			}
			Destroy (gameObject);
		} else if (trackUtility.utility.gameState () == "placing" && placing) {
			Debug.Log ("got here");
			if (overlap > 0)
				return;
			
			//make sure position and location are up to date
			transform.position = snap.position;
			transform.rotation = Quaternion.Euler (transform.rotation.eulerAngles.x, snap.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);

			//then stop placing the object
			placing = false;

			//and make sure its defaults are set
			overlap = 0;
			rend.materials = realMats;
			foreach (pathPoint p in gameObject.GetComponentsInChildren<pathPoint> ())
				p.runConnection ();

			//and that it is static 
			gameObject.isStatic = true;

			trackUtility.utility.gameState ("-");
		}
	}

	void highlightObject(){
	}

	void removeHighlight(){
	}
}