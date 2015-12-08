using UnityEngine;
using System.Collections;

public class hit : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.tag == "Checkpoint") {
			Destroy (other.gameObject);
		}
		Debug.Log ("hit");
	}
}
