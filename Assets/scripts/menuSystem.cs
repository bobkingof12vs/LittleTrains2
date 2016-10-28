using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class menuSystem : MonoBehaviour {

	public Transform cameraTransform;
	public GameObject cursorMarker;
	public trainMenu trnMenu;
	public GameObject[] menus;

	private GameObject lastHit = null;
	private string curMenuName;

	void Start (){
		foreach (GameObject g in menus)
			g.SetActive (g.name == "mainMenu");
	}
	// Update is called once per frame
	void Update () {
		if (!gameObject.activeSelf)
			return;

		if(trackUtility.utility.gameState() != "menu")
			trackUtility.utility.gameState ("menu");

		if(lastHit != null)
			lastHit.GetComponent <Image> ().enabled = false;
		

		RaycastHit hitInfo;
		Vector3 rayDirection;
		if (controllerManager.useGvr)
			rayDirection = GvrController.Orientation * Vector3.down;
		else
			rayDirection = cameraTransform.rotation * Vector3.forward;

		cursorMarker.transform.position = cameraTransform.position + rayDirection;

		if (Physics.Raycast (cameraTransform.position, rayDirection, out hitInfo, Mathf.Infinity, LayerMask.GetMask("menu")) && hitInfo.collider) {
			lastHit = hitInfo.collider.gameObject;
			lastHit.GetComponent <Image> ().enabled = true;
		} else {
			lastHit = null;
		}

		if (GvrController.ClickButtonUp || Input.GetButtonUp("Fire1")) {
			if (lastHit.name == "close") {
				trackUtility.utility.gameState ("-");
				gameObject.SetActive (false);
			} else if (lastHit.tag == "menuOption") {
				foreach (GameObject g in menus) {
					Debug.Log (g.name);
					g.SetActive (g.name == lastHit.name);
					curMenuName = lastHit.name;
				}
			} else if (lastHit.tag == "objectOption") {
				trackUtility.utility.gameState ("-");
				gameObject.SetActive (false);
				Debug.Log ("prefab/" + curMenuName + "/" + lastHit.name);
				Instantiate (Resources.Load ("prefab/" + curMenuName + "/" + lastHit.name));
			} else if (lastHit.tag == "trainAddOption") {
				Debug.Log ("prefab/" + curMenuName + "/" + lastHit.name);
				trnMenu.addCar ((GameObject)Resources.Load ("prefab/" + curMenuName + "/" + lastHit.name));
			} else if (lastHit.tag == "trainRemoveOption") {
				trnMenu.removeCar (int.Parse (lastHit.name.Substring (3)));
			} else if (lastHit.gameObject.name == "nextCar") {
				trnMenu.scrollTrains (1);
			} else if (lastHit.gameObject.name == "previousCar") {
				trnMenu.scrollTrains (-1);
			} else if (lastHit.gameObject.name == "runTrain") {
				trnMenu.runTrain ();
				gameObject.SetActive (false);
			} else if (lastHit.gameObject.name == "removeTrack") {
				trackUtility.utility.gameState ("removeTrack");
				gameObject.SetActive (false);
			} else if (lastHit.gameObject.name == "removeObject") {
				trackUtility.utility.gameState ("removeObject");
				gameObject.SetActive (false);
			} else if (lastHit.tag == "warning") {
				lastHit.gameObject.SetActive (false);
			} else {
				Debug.Log ("menu didn't catch: "+lastHit.gameObject.name);
			}

		}
	}

	void OnEnable(){
		transform.position = (cameraTransform.forward * 3.5f) + cameraTransform.position;
		transform.LookAt(2 * transform.position - cameraTransform.position);

		if (controllerManager.useGvr)
			transform.SetParent (cameraTransform);
		else
			transform.SetParent (null);
	}
}
