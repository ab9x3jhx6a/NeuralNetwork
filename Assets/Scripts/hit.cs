using UnityEngine;
using System.Collections;

public class hit : MonoBehaviour {
	public int checkpoints;
	public Material Passed;
	public Vector3 init_pos;
	public Quaternion init_rotation;
	public bool crash;

	Agent agent;
	// Use this for initialization
	void Start () {
		agent = gameObject.GetComponent<Agent> ();
		crash = false;
		checkpoints = 0;
		init_pos = transform.position;
		init_rotation = transform.rotation;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.tag == "Checkpoint") {
			Renderer tmp = other.gameObject.GetComponent<Renderer> ();
			tmp.material = Passed;
			checkpoints++;
			agent.dist += 1.0f;
		} else {
			//hit a wall considered fail
			//checkpoints = 0;
			crash = true;
		}
	}
}
