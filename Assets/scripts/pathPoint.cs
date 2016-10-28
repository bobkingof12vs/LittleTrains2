using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;

public class pathPoint : MonoBehaviour {

	//set these on prefab
	public GameObject[] pathMeshs;
	public bool followPathForward;
	public bool hasSwitch;
	public sbyte switchPosition = 0;

	//don't set
	private int id;
	private Vector3[][] pathVertices;
	private pathPoint[] connectsTo;
	private pathPoint connectsFrom;
	private Vector3 pathStart;
	private Vector3[] pathEnds;
	public Vector3 trainDropPoint;
	private float totalLength;
	private float[] lengths;

	void Awake(){
		id = gameObject.transform.parent.gameObject.GetInstanceID();

		//initialize variables
		pathEnds = new Vector3[pathMeshs.Length];
		pathVertices = new Vector3[pathMeshs.Length][];
	}

	public void runConnection () {
		
		connectsTo = new pathPoint[pathMeshs.Length];

		for (int i = 0; i < pathMeshs.Length; i++) {

			//generate our list of path vertices from the armature children
			List<Vector3> path = new List<Vector3> ();
			Transform t = pathMeshs [i].transform.GetChild (0);
			while (true) {
				//add the position
				path.Add (t.position);

				//find next point
				if (t.childCount == 1)
					t = t.GetChild (0);
				else if (t.childCount > 1)
					Debug.LogError ("too many children");
				else
					break;
			}
			//if the path is followed backward, simply reverse this array
			if (!followPathForward)
				path.Reverse ();

			//finally set variables based on aboce
			pathVertices [i] = path.ToArray<Vector3> ();
			pathStart = pathVertices [i] [0];
			pathEnds [i] = pathVertices [i].Last ();

			foreach (GameObject go in GameObject.FindGameObjectsWithTag ("pathPoint")) {
				//get the pathPoint from other object
				pathPoint pa = go.GetComponent<pathPoint> ();

				//make sure it is not this point
				if (pa.getID () == gameObject.transform.parent.gameObject.GetInstanceID())
					continue;

				//check if that point meets this endpoint
				if(Vector3.Distance (pa.getStartPoint(), pathEnds [i]) < 0.01f)
					connectsTo [i] = pa;

				//check if that start point meets this startpoint 
				//(this is checked more than necessary, but makes for a more understandable program)
				if(Vector3.Distance (pa.getStartPoint(), pathStart) < 0.01f)
					connectsFrom = pa;
				
				//check if this point meets that point
				pa.checkEnds (this);
			}
		}

		int p = pathVertices[0].Length - 3;
		trainDropPoint = trackUtility.utility.lerp2 (pathVertices [0] [p], pathVertices [0] [p+1], pathVertices [0] [p+2], 0.5f);
		totalLength = trackUtility.utility.pathDistance (this);
		lengths = trackUtility.utility.secDistance (this);
	}

	public Vector3[] getPath(){
		return pathVertices [switchPosition];
	}

	public List<Vector3> getExtendedPath(int startPoint, float distance){
		List<Vector3> path = new List<Vector3> ();

		float curDist = 0;
		pathPoint pa;
		if (distance > 0) {
			//start from startpoint and go forward
			for(int i = startPoint; i < pathVertices.Length; i++)
				path.Add(pathVertices [switchPosition][i]);
			
			pa = getNextPath ();
			while (curDist < distance && pa != null) {
				path.AddRange (pa.getPath ());
				curDist += pa.getLength ();
				pa = pa.getNextPath ();
			}
		} else {
			//add from startPoint back to beggining
			for(int i = startPoint; i > 0; i--)
				path.Add(pathVertices [switchPosition][i]);
			
			pa = getPrevPath ();
			while (curDist > distance && pa != null) {
				path.AddRange (pa.getPath ());
				curDist -= pa.getLength ();
				pa = pa.getNextPath ();
			}
		}

		return path;

	}

	public pathPoint getNextPath(){
		return connectsTo [switchPosition];
	}

	public pathPoint getPrevPath(){
		return connectsFrom;
	}

	public Vector3 getStartPoint(){
		return pathStart;
	}

	public Vector3 getTrainDropPoint(){
		return trainDropPoint;
	}

	public float getLength(){
		return totalLength;
	}

	public float[] getPathDists(){
		return lengths;
	}

	public void checkEnds(pathPoint pa){
		if (pa.getID () == id)
			return;

		//check if that point meets this endpoint
		for (int i = 0; i < pathEnds.Length; i++) {
			if(Vector3.Distance (pa.getStartPoint(), pathEnds [i]) < 0.01f)
				connectsTo [i] = pa;
		}

		//check if that start point meets this startpoint 
		//(this is checked more than necessary, but makes for a more understandable program)
		if(Vector3.Distance (pa.getStartPoint(), pathStart) < 0.01f)
			connectsFrom = pa;
	}

	public int getID(){
		return gameObject.transform.parent.gameObject.GetInstanceID();
	}

	public void clicked(){

		//this function handles switchs, so if no switch, return
		if (!hasSwitch || trackUtility.utility.gameState() != "-")
			return;

		//if we go over the number of posisble switches, go back to the start
		//allows for eventual 3+ way switches
		if (++switchPosition >= connectsTo.Length)
			switchPosition = 0;

		Debug.Log (trackUtility.utility.gameState () + ": yep : "+switchPosition);
		//update the animator to show the position
		GetComponentInParent<Animator>().SetInteger("switch", switchPosition);
	}
	public void removePoint(){
		//set start all points to a vector no track will have
		pathStart = Vector3.down;
		for (int i = 0; i < pathEnds.Length; i++)
			pathEnds [i] = Vector3.down;

		//then run the connection for all points this touches (should find nothing and return null)
		connectsFrom.runConnection ();
		for (int i = 0; i < connectsTo.Length; i++)
			connectsTo[i].runConnection ();
	}
}

