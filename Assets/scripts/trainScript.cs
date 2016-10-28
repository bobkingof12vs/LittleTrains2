using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class trainScript : MonoBehaviour {

	public trainData engine;
	public List <trainData> cars;
	public Transform cursorMarker;
	public Transform track;

	//variables to check if we are going to hit anything
	public Transform leadPoint, tailPoint;
	public float leadPointAheadDistance, tailPointDistanceBehind;

	private float bufferBetweenCars = 0.2f;

	//states
	private bool placing = true;
	private bool braking = false;

	//path variables
	public float curSpeed = 0;
	private pathPoint curPath;
	private int pathSection;
	public float curDist;

	void Update () {
		if (placing) {
			if (cursorMarker == null) {
				cursorMarker = GameObject.Find ("cursorMarker").transform;
				track = GameObject.Find ("track").transform;
			}

			curPath = trackUtility.utility.findCloseTrainDrop(cursorMarker.position, 3f);
			Vector3 startPoint;
			if (curPath != null) {
				startPoint = curPath.getTrainDropPoint ();
				transform.position = new Vector3 (startPoint.x, 0.35f, startPoint.z);
			}
			else if (cursorMarker.position.y > -0.001f) {
				transform.position = new Vector3 (cursorMarker.position.x, 0.35f, cursorMarker.position.z);
			}
		} else {

			//determine speed
			if (!braking) {
				curSpeed += (engine.accel * Time.deltaTime);
				if (curSpeed > engine.topSpeed)
					curSpeed = engine.topSpeed;
				if (curSpeed < -engine.topSpeed)
					curSpeed = -engine.topSpeed;
			} else {
				curSpeed -= (engine.decel * Time.deltaTime);
				if (curSpeed < .001)
					curSpeed = 0;
			}

			//we need to know if any point moves to a new path
			//as we start going forward on a new path
			float direction = -1f;

			//find the engines next point
			//this is the point that this object's variables follow
			Vector3 nextPoint = trackUtility.utility.moveDistance (curSpeed * Time.deltaTime, engine.go.transform.position, ref curPath, ref pathSection, ref curDist, ref direction);

			//we copy those variables and use them elsewhere
			pathPoint curPathCopy = curPath;
			int pathSectionCopy = pathSection;
			float curDistCopy = curDist;

			//ever car will follow the pattern of move the car, point towards the lookAt
			Vector3 lookAt = trackUtility.utility.moveDistance ((direction * engine.lengths[1]), nextPoint, ref curPathCopy, ref pathSectionCopy, ref curDistCopy, ref direction);

			if (engine.go.transform.position == nextPoint || nextPoint == lookAt)
				return;
			engine.go.transform.position = nextPoint;
			engine.go.transform.LookAt (lookAt);
			engine.go.transform.RotateAround (nextPoint, Vector3.up, -90);

			//determine the point behind it
			float nextDistance = (direction * (engine.lengths[2] + bufferBetweenCars));

			//loop through cars
			for (int i = 0; i < cars.Count; i++) {
				//find its point
				nextPoint = trackUtility.utility.moveDistance (nextDistance + (direction * cars[i].lengths[0]), lookAt, ref curPathCopy, ref pathSectionCopy, ref curDistCopy, ref direction);

				//find its tail point
				lookAt = trackUtility.utility.moveDistance (direction * cars[i].lengths[1], nextPoint, ref curPathCopy, ref pathSectionCopy, ref curDistCopy, ref direction);

				if (cars[i].go.transform.position == nextPoint || nextPoint == lookAt)
					return;

				cars[i].go.transform.position = nextPoint;
				cars[i].go.transform.LookAt (lookAt);
				cars[i].go.transform.RotateAround (nextPoint, Vector3.up, -90);

				nextDistance = (direction * (cars [i].lengths [2] + bufferBetweenCars));
			}
		}
	}

	public void setTrain(List<GameObject> newCars){

		engine = newCars [0].GetComponent<trainData> ();
		engine.go.transform.SetParent (transform);
		engine.go.transform.localPosition = Vector3.zero;
		engine.go.transform.localScale = Vector3.one;

		float distanceBehind = engine.lengths [2];
		for (int i = 1; i < newCars.Count; i++) {
			trainData newCar = newCars [i].GetComponent<trainData> ();
			newCar.gameObject.transform.SetParent (transform);
			newCar.go.transform.localPosition = Vector3.zero;
			newCar.go.transform.localScale = Vector3.one;

			newCar.backWheelDistance = newCar.lengths [1];
			newCar.distanceBehind = distanceBehind + newCar.lengths [0] + bufferBetweenCars;
			distanceBehind = newCar.lengths [2];

			newCar.go.SetActive (false);
			cars.Add (newCar);
		}

		placing = true;
	}

	public void clicked(){
		if(placing){
			placing = false;
			pathSection =  curPath.getPathDists().Length - 1;
			curDist = 0.5f;

			foreach (trainData td in cars)
				td.go.SetActive (true);
			
			trackUtility.utility.gameState ("-");

		}
	}
}