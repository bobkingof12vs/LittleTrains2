using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class trackUtility : MonoBehaviour {
	
	public static trackUtility utility;
	public LineRenderer line;

	private List<pathPoint> switches;

	void Awake(){
		if (utility == null)
			utility = this;
		else if (utility != null)
			Destroy (this);

		DontDestroyOnLoad (this);

		line = GetComponent<LineRenderer> ();
	}

	public pathPoint findClosePath(Vector3 cursor, float maxDistance = 0.5f){

		pathPoint closestPoint = null;
		float smallestDistance = maxDistance;

		foreach (GameObject go in GameObject.FindGameObjectsWithTag("pathPoint")) {
			
			pathPoint pa = go.GetComponent <pathPoint> ();
			float dist = Vector3.Distance (pa.getStartPoint(), cursor);

			if (dist <= smallestDistance) {
				closestPoint = pa;
				smallestDistance = dist;
			}
		}

		return closestPoint;
	}

	public pathPoint findCloseTrainDrop(Vector3 cursor, float maxDistance = 2.5f){
		pathPoint closestPoint = null;
		float smallestDistance = maxDistance;

		foreach (GameObject go in GameObject.FindGameObjectsWithTag("pathPoint")) {

			pathPoint pa = go.GetComponent <pathPoint> ();
			float dist = Vector3.Distance (pa.getTrainDropPoint(), cursor);

			if (dist <= smallestDistance) {
				closestPoint = pa;
				smallestDistance = dist;
			}
		}

		return closestPoint;
	} 

	public void drawPath(pathPoint curPath, bool drawNext = false){


		Vector3[] path;
		if (drawNext && curPath.getNextPath () != null) {
			Vector3[] path1 = curPath.getPath ();
			Vector3[] path2 = curPath.getNextPath ().getPath ();
			path = new Vector3[path1.Length + path2.Length - 1];
			Array.Copy (path1, path, path1.Length - 1);
			Array.Copy (path2, 0, path, path1.Length - 1, path2.Length);
		}
		else {
			path = curPath.getPath ();
		}

		line.SetVertexCount(path.Length);
		line.SetPositions (path);
	}

	public string state;
	public string gameState(string newState = null){
		if(newState != null)
			state = newState;
		return state;
	}

	public Vector3 lerp2 (Vector3 p1, Vector3 p2, Vector3 p3, float t){
		return Vector3.Lerp(Vector3.Lerp(p1, p2, t), Vector3.Lerp(p2, p3, t), t);
	}
		
	public Vector3 moveDistance(float deltaDistance, Vector3 curPoint, ref pathPoint pa, ref int pathSection, ref float curDist, ref float direction){
		
		//no path, no moving.
		if (pa == null)
			return curPoint;

		Vector3[] path = pa.getPath ();
		float[] pathDists = pa.getPathDists ();

		int i = (pathSection * 2) + 0;
		int j = (pathSection * 2) + 1;
		int k = (pathSection * 2) + 2;
		
		//set the current distance
		curDist += deltaDistance;

		//check if that distance is out of range
		if (curDist > pathDists [pathSection]) {
			curDist -= pathDists [pathSection];
			pathSection++; 
			if (pathSection > pathDists.Length - 1) {
				pa = pa.getNextPath ();
				pathSection = 0;
			}
			return moveDistance (0, curPoint, ref pa, ref pathSection, ref curDist, ref direction); 
		} else if (curDist < 0) {

			pathSection--;
			if (pathSection < 0) {
				pa = pa.getPrevPath ();
				pathSection = 0;
				deltaDistance = Mathf.Abs(curDist);
				curDist = 0;
				direction = 1f;
			} else {
				deltaDistance = curDist;
				curDist = pathDists [pathSection];
			}
			return moveDistance (deltaDistance, curPoint, ref pa, ref pathSection, ref curDist, ref direction); 
		}

		//at some point, we will reach here and return some a new point
		return lerp2 (path [i], path [j], path [k], curDist / pathDists [pathSection]);

	}

	public float pathDistance(pathPoint pa){
		int accuracy = 100;

		Vector3[] path = pa.getPath ();

		float totalDistance = 0;
		Vector3 lastPoint;
		for (int i = 0; i < path.Length - 1; i += 2) {
			lastPoint = path [i];
			for (int j = 1; j <= 1; j++) {
				Vector3 nextPoint = lerp2 (path [i + 0], path [i + 1], path [i + 2], (j / accuracy));
				totalDistance += Vector3.Distance (lastPoint, nextPoint);

				lastPoint = nextPoint;
			}
		}

		return totalDistance;
	}

	public float[] secDistance(pathPoint pa){
		int accuracy = 100;

		Vector3[] path = pa.getPath ();
		List<float> distance = new List<float> ();

		Vector3 lastPoint;
		for (int i = 0; i < path.Length - 1; i += 2) {
			lastPoint = path [i];
			distance.Add(0f);
			for (int j = 1; j <= accuracy; j++) {
				Vector3 nextPoint = lerp2 (path [i + 0], path [i + 1], path [i + 2], (j / accuracy));
				distance[distance.Count - 1] += Vector3.Distance (lastPoint, nextPoint);
				lastPoint = nextPoint;
			}
		}

		return distance.ToArray();
	}

}
