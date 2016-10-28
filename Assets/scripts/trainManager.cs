using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class trainManager : MonoBehaviour {

	public static trainManager manager;
	public List <trainScript> trains;

	void Awake(){
		if (manager == null)
			manager = this;
		else if (manager != null)
			Destroy (this);

		DontDestroyOnLoad (this);
		trains = new List<trainScript> ();
		trains.Add (new trainScript ());
	}
}