using UnityEngine;
using System.Collections;

public class Motor : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 newpos = transform.position + Vector3.left/100;
		transform.position = newpos;
	}
}
