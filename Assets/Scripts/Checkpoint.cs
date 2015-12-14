using UnityEngine;
using System.Collections;

public class Checkpoint : MonoBehaviour {


	public bool passed;
	// Use this for initialization
	void Start () {
		this.passed = false;
	}
	
	// Update is called once per frame
	void Update () {

	}
	public void SetBool(bool t){
		this.passed = t;
	}
}
