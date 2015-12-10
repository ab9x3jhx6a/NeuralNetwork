using UnityEngine;
using System.Collections;

public class hit : MonoBehaviour {
	public int checkpoints;
	// Use this for initialization
	void Start () {
		checkpoints = -1;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.tag == "Checkpoint") {
			Destroy (other.gameObject);
		}
		checkpoints++;
		Debug.Log ("hit");
	}
}
