using UnityEngine;
using System.Collections;

public class trainData : MonoBehaviour {
	public GameObject go;
	public float backWheelDistance = 0;
	public float distanceBehind = 0;
	public float[] lengths = {0,0,0};
	public float topSpeed;
	public bool isEngine;
	public float accel;
	public float decel;

	void Awake(){
		go = gameObject;
	}
}
