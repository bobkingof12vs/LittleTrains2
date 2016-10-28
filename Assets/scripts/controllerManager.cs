// Copyright 2016 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissioßns and
// limitations under the License.

using UnityEngine;
using UnityEngine.UI;

public class controllerManager : MonoBehaviour {
	public GameObject controllerPivot, cursorMarker;
	public GameObject messageCanvas;
	public Text messageText;

	public GameObject menu;

	public Transform cameraTransform, cameraControlTransform;
	//  public Material cubeInactiveMaterial;
	//  public Material cubeHoverMaterial;
	//  public Material cubeActiveMaterial;

	public Transform snap;
	public Color orangeMarkerMaterial, highlightMarkerMaterial, redMarkerMaterial, deletedHighlightColorMaterial;
	private Renderer controllerCursorRenderer;

	// Currently selected GameObject.
	private GameObject selectedObject;
	private GameObject lastHighlight;

	// True if we are dragging the currently selected GameObject.
	private bool dragging;

	// should we be using the google daydream controller, or just some 4 button controller
	public static bool useGvr = false;

	void Update() {
		useGvr = UpdateStatusMessage ();

		if (GvrController.AppButtonUp || Input.GetButtonUp("Fire2"))
			menu.SetActive (!menu.activeSelf);
		
		if (!menu.activeSelf) {
			UpdatePointer ();

			if (useGvr) {
				if (GvrController.ClickButtonDown)
					GvrStartTouch (2);
				else if (GvrController.TouchDown)
					GvrStartTouch (1);
				else if (GvrController.ClickButtonUp)
					GvrEndTouch (2);
				else if (GvrController.TouchUp)
					GvrEndTouch (1);
				else if (GvrController.IsTouching || GvrController.ClickButton)
					GvrCheckTouch ();
			} else {
				CtlrMoveCamera ();
			}
		} else {
			cursorMarker.SetActive (false);
			snap.position = Vector3.down;
		}

	}

	//------------------//
	//  Things For Gvr  //
	//------------------//

	private int processTouch;
	private float touchStartTime;
	private Vector2 touchStartPos;
	private bool zoomed;
	private void GvrStartTouch(int TouchType){
		processTouch = TouchType;
		touchStartTime = Time.time;
		touchStartPos = GvrController.TouchPos;
		zoomed = false;
	}

	private void GvrCheckTouch(){
		float moveDist = Vector2.Distance (touchStartPos, GvrController.TouchPos);
		if (Time.time - touchStartTime < .100f && moveDist < 0.1f) {
			return;
		}
		else if (processTouch == 1 && moveDist > 0.1f) {
			UpdateCamera (
				-(touchStartPos.x - GvrController.TouchPos.x),
				0f,
				(touchStartPos.y - GvrController.TouchPos.y)
			);
		}
		else if (processTouch == 2 && moveDist > 0.1f) {
			if (Mathf.Abs (touchStartPos.x - GvrController.TouchPos.x) < Mathf.Abs (touchStartPos.y - GvrController.TouchPos.y)) {
				zoomed = true;
				UpdateCamera (0f, (touchStartPos.y - GvrController.TouchPos.y) * 0.5f, 0f);
			}
		}
	}

	private void GvrEndTouch(int touchType){
		float moveDist = Vector2.Distance (touchStartPos, GvrController.TouchPos);

		if (processTouch == 2) {
			if (Time.time - touchStartTime < 0.1f && moveDist < 0.1f) {
				Debug.Log (moveDist);
				checkClicks ();
				return;
			}
			else if (moveDist > 0.1f && !zoomed && Mathf.Abs (touchStartPos.x - GvrController.TouchPos.x) > Mathf.Abs (touchStartPos.y - GvrController.TouchPos.y)) {
				float y = snap.rotation.eulerAngles.y + ((touchStartPos.x - GvrController.TouchPos.x) < 0 ? -90f : 90f);
				y = (360 + (Mathf.RoundToInt(y) % 360) % 360);
				snap.rotation = Quaternion.Euler (0, y, 0);
				UpdatePointer ();
			}
		}

		processTouch = 0;
		touchStartTime = 0;
	}

	//--------------------------------//
	//  Things For Normal Controller  //
	//--------------------------------//

	private void CtlrMoveCamera(){

		if (Input.GetButtonUp ("Fire3") && trackUtility.utility.gameState () == "placing") {
			float y = snap.rotation.eulerAngles.y + 90f;
			snap.rotation = Quaternion.Euler (0, Mathf.RoundToInt(y) % 360, 0);
		}

		if (Input.GetButtonUp ("Fire1"))
			checkClicks ();

		//this should happen first... maybe not though?
		if (Input.GetButton ("Jump")) {
			UpdateCamera (0, Input.GetAxis ("Vertical") * 0.25f, 0);
		} else {
			UpdateCamera (Input.GetAxis ("Horizontal") * .25f, 0, Input.GetAxis ("Vertical") * .25f);
		}

		UpdatePointer ();
			
	}

	//-------------------//
	//  Things For both  //
	//-------------------//

	private void checkClicks(){

		if (trackUtility.utility.gameState () == "placing") {
			GameObject.Find ("track").BroadcastMessage ("clicked");
			return;
		}

		RaycastHit hitInfo;
		Vector3 rayDirection;
		if (useGvr)
			rayDirection = GvrController.Orientation * Vector3.down;
		else
			rayDirection = cameraTransform.rotation * Vector3.forward;
		
		if (Physics.Raycast (new Ray (cameraTransform.position, rayDirection), out hitInfo, Mathf.Infinity, LayerMask.GetMask ("objects"))) {
			if (hitInfo.distance > 1 && hitInfo.collider) {
				Debug.Log ("clicked "+hitInfo.collider.gameObject.name);
				if(trackUtility.utility.gameState() == "menu")
					hitInfo.collider.gameObject.transform.parent.gameObject.BroadcastMessage ("clicked", hitInfo, SendMessageOptions.DontRequireReceiver);
				if (trackUtility.utility.gameState () == "dropTrain")
					GameObject.Find ("trains").BroadcastMessage ("clicked");
				if (trackUtility.utility.gameState () == "removeTrack" || trackUtility.utility.gameState () == "removeObject" || trackUtility.utility.gameState () == "-")
					hitInfo.collider.gameObject.transform.parent.gameObject.BroadcastMessage ("clicked", hitInfo, SendMessageOptions.DontRequireReceiver);
				if (trackUtility.utility.gameState () == "dropTrain")
					GameObject.Find ("trains").BroadcastMessage ("clicked");
			}
		}
	}

	private void UpdateCamera (float dX, float dY, float dZ){
		cameraControlTransform.rotation = Quaternion.Euler (0, cameraTransform.rotation.eulerAngles.y, 0);
		cameraControlTransform.position += (cameraControlTransform.right * dX);
		cameraControlTransform.position += (cameraControlTransform.forward * dZ);
		cameraControlTransform.position = new Vector3 (
			Mathf.Clamp(cameraControlTransform.position.x, -32f, 32f),
			Mathf.Clamp(cameraControlTransform.position.y + dY,   1f, 64f),
			Mathf.Clamp(cameraControlTransform.position.z, -32f, 32f)
		);
		cameraTransform.position = cameraControlTransform.position;
	}

	private bool highlightedObject = false;
	private void UpdatePointer() {

		//bool showPointer = true;
		if (useGvr && GvrController.State != GvrConnectionState.Connected) {
			cursorMarker.SetActive (false);
			snap.position = Vector3.down;
		} 
		else {
			cursorMarker.SetActive (true);
			//controllerPivot.transform.rotation = GvrController.Orientation;

			RaycastHit hitInfo;
			Vector3 rayDirection;
			if (useGvr)
				rayDirection = GvrController.Orientation * Vector3.down;
			else
				rayDirection = cameraTransform.rotation * Vector3.forward;
			
			int layer = 0;
			if(trackUtility.utility.gameState() == "placing")
				layer = LayerMask.GetMask("yard");
			else
				layer = LayerMask.GetMask("yard", "objects");
			if (Physics.Raycast (new Ray(cameraTransform.position, rayDirection), out hitInfo, Mathf.Infinity, layer)) {

//				Debug.Log (hitInfo.collider.gameObject.layer);
//				Debug.Log (LayerMask.GetMask("objects"));
				if (LayerMask.LayerToName (hitInfo.collider.gameObject.layer) == "objects") {
					highlightedObject = true;
				} else if (highlightedObject) {
					highlightedObject = false;
				}
				
				
				//set the marker to where the ray hit
				cursorMarker.transform.position = hitInfo.point;

				//update the snap position 
				if (hitInfo.distance > 1 && hitInfo.collider) {
					//rounded to the nearest 2
					float newX = (Mathf.Floor (hitInfo.point.x / 2f) * 2f) + 1;
					float newZ = (Mathf.Floor (hitInfo.point.z / 2f) * 2f) + 1;
					//offset by 1 in some direction
					if (Mathf.RoundToInt(snap.rotation.eulerAngles.y) == 90)
						newZ -= 1f;
					else if (Mathf.RoundToInt(snap.rotation.eulerAngles.y) == 270)
						newZ += 1f;
					else if (Mathf.RoundToInt(snap.rotation.eulerAngles.y) == 0)
						newX += 1f;
					else if (Mathf.RoundToInt(snap.rotation.eulerAngles.y) == 180)
						newX -= 1f;
					
					//actually update the snap position
					snap.position = new Vector3 (newX, 0, newZ);
				} else {
					//otherwise set it to a value that won't be seen
					snap.position = Vector3.down;
					cursorMarker.SetActive (false);
				}
			} else {
				//snap.position = Vector3.down;
				cursorMarker.SetActive (false);

				//ball back to orange
				if (highlightedObject)
					highlightedObject = false;
			}
		}

		//ball back to orange
		if (trackUtility.utility.gameState () == "removeTrack" || trackUtility.utility.gameState () == "removeObject") {
			if (highlightedObject)
				cursorMarker.gameObject.GetComponent<Renderer> ().material.color = deletedHighlightColorMaterial;
			else
				cursorMarker.gameObject.GetComponent<Renderer> ().material.color = redMarkerMaterial;
		} else {
			if (highlightedObject)
				cursorMarker.gameObject.GetComponent<Renderer> ().material.color = highlightMarkerMaterial;
			else
				cursorMarker.gameObject.GetComponent<Renderer> ().material.color = orangeMarkerMaterial;
		}
	}

  private bool UpdateStatusMessage() {
		// This is an example of how to process the controller's state to display a status message.

		switch (GvrController.State) {
		case GvrConnectionState.Connected:
			messageCanvas.SetActive (false);
			return true;
		case GvrConnectionState.Disconnected:
			break;
		case GvrConnectionState.Error:
			messageText.text = "ERROR: " + GvrController.ErrorDetails;
			messageText.color = Color.red;
			messageCanvas.SetActive (true);
			break;
		}

		if (Input.GetJoystickNames ().Length > 0) {
			messageCanvas.SetActive (false);
			return false;
		}
			
		if (Input.mousePresent) {
			messageCanvas.SetActive (false);
			return false;
		}
		messageText.text = "ERROR: No Controller Found";
		messageText.color = Color.red;
		messageCanvas.SetActive (true);

		return false;

	}
}
