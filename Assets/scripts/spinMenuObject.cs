using UnityEngine;
using System.Collections;

public class spinMenuObject : MonoBehaviour {

	public GameObject menu;
	void Update () {
		if (!menu.activeSelf)
			return;

		//this whole class should be self explanitory...
		transform.RotateAround (transform.position, transform.up, (360f / 8f) * Time.deltaTime);
	}
}
