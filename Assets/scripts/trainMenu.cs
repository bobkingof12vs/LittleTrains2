using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class trainMenu : MonoBehaviour {

	public int showTrain = 0;
	public Transform[] menuPositions;
	public List<GameObject> carList = new List<GameObject> ();
	public GameObject engineWarning, noTrackWarning, mainMenu;
	public Transform trainObjectTransform, tracks;

	private int scrollPos = 0;
	private bool hasEngine = false;

	void OnEnable(){
		if (tracks.childCount == 0) {
			noTrackWarning.SetActive (true);
			mainMenu.SetActive (true);
			gameObject.SetActive (false);
		}
	}

	public void clearTrains(){
		carList.ForEach (delegate (GameObject t) {
			DestroyImmediate(t);
		});
		carList.Clear ();
		hasEngine = false;
	}

	public void addCar (GameObject car) {

		trainData td = car.GetComponent<trainData> ();

		if (td.isEngine) {
			if (hasEngine) {
				DestroyImmediate (carList [0]);
				carList [0] = ((GameObject)Instantiate (car));
			} else {
				carList.Add ((GameObject)Instantiate (car));
				hasEngine = true;
			}
			scrollPos = 0;
		} else if (hasEngine) {
			carList.Add ((GameObject)Instantiate (car));
			if (carList.Count > 6) {
				scrollPos = carList.Count - 6;
			}
		} else {
			engineWarning.SetActive(true);
		}

		updateTrainMenu ();
	}

	public void updateTrainMenu(){
		int tPos = 0;
		carList.ForEach (delegate (GameObject t) {
			if (tPos >= scrollPos && tPos < (scrollPos + 6)) {
				t.transform.SetParent (menuPositions [tPos - scrollPos]);
				t.transform.localScale = Vector3.one;
				t.transform.localPosition = Vector3.zero;
				t.SetActive(true);
			} else {
				t.transform.SetParent (null);
				t.SetActive(false);
			}
			tPos++;
		});
	}

	public void scrollTrains(int i){
		scrollPos += i;
		if (scrollPos < 0)
			scrollPos = 0;
		if (scrollPos > carList.Count - 6)
			scrollPos = carList.Count - 6;
		updateTrainMenu ();
	}
			
	public void removeCar(int index){
		index += scrollPos;
		DestroyImmediate (carList [index]);
		carList.RemoveAt(index);
		updateTrainMenu ();
	}

	public void runTrain(){
		if (!hasEngine) {
			engineWarning.SetActive (true);
			return;
		}

		GameObject newTrain = (GameObject)Instantiate (Resources.Load ("prefab/trainPrefab"));
		newTrain.GetComponent<trainScript> ().setTrain (carList);
		newTrain.transform.SetParent(trainObjectTransform);

		trackUtility.utility.gameState ("dropTrain");
	}
}
