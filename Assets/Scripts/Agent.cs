using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Agent : MonoBehaviour {
	public bool hasFailed = false;
	private float distanceDelta;

	private int collidedCorner;
	private float headingAngle; //Degrees

	public NNet neuralnet;
	public RayCast raycast;
	public float l,fl,f,fr,r;

	public float MAX_ROTATION; //max rotate speed
	public float _SPEED;

	public float leftForce;
	public float rightForce;
	public float leftTheta;
	public float rightTheta;

	public float dist;

	hit hit;

	// Use this for initialization
	void Start () {
		hasFailed = false;

		neuralnet = new NNet ();
		neuralnet.CreateNet (1, 5, 8, 2);
		raycast = gameObject.GetComponent<RayCast> ();

		l = raycast.dis_l;
		fl = raycast.dis_fl;
		f = raycast.dis_f;
		fr = raycast.dis_fr;
		r = raycast.dis_fr;

		leftForce = 0.0f;
		rightForce = 0.0f;
		leftTheta = 0.0f;
		rightTheta = 0.0f;

		hit = gameObject.GetComponent<hit> ();
		
	}

	// Update is called once per frame
	void Update () {
		hasFailed = hit.crash;

		//update raycast hit distance
		l = raycast.dis_l;
		fl = raycast.dis_fl;
		f = raycast.dis_f;
		fr = raycast.dis_fr;
		r = raycast.dis_fr;
		
		
		if (!hasFailed) {
			dist += Time.deltaTime;
			List<float> inputs = new List<float> ();
			inputs.Add (Normalise (l));
			inputs.Add (Normalise (fl));
			inputs.Add (Normalise (f));
			inputs.Add (Normalise (fr));
			inputs.Add (Normalise (r));
			
			neuralnet.SetInput (inputs);
			neuralnet.refresh ();
			
			leftForce = neuralnet.GetOutput (0);
			rightForce = neuralnet.GetOutput (1);
			
			leftTheta = MAX_ROTATION * leftForce;
			rightTheta = MAX_ROTATION * rightForce;
			
			headingAngle += (leftTheta - rightTheta) * Time.deltaTime;
			
			float speed = (Mathf.Abs (leftForce + rightForce)) / 2;
			speed *= _SPEED;
			
			speed = Clamp (speed, -_SPEED, _SPEED);
			
		} else {
			dist = 0.0f;
		}
	}

	public float Normalise(float i){
		float depth = i / raycast.RayCast_Length;
		return 1 - depth;
	}

	public void Attach(NNet net){
		neuralnet = net;
	}

	public void ClearFailure(){
		hasFailed = false;
		hit.crash = false;
		dist = 0.0f;
		collidedCorner = -1;
	}

	public float Clamp (float val, float min, float max){
		if (val < min) {
			return min;
		}
		if (val > max) {
			return max;
		}
		return val;
	}
}

public class NNet {
	private int inputAmount;
	private int outputAmount;
	
	List<float> inputs = new List<float>();
	NLayer inputlayer = new NLayer();
	
	List<NLayer> hiddenLayers = new List<NLayer>();
	NLayer outputLayer = new NLayer();
	
	List<float> outputs = new List<float> ();


	public void refresh(){
		outputs.Clear ();

		for (int i=0; i < hiddenLayers.Count; i++) {
			if(i > 0){
				inputs = outputs;
			}
			hiddenLayers[i].Evaluate(inputs, ref outputs);
			
		}
		inputs = outputs;
		//Process the layeroutputs through the output layer to
		outputLayer.Evaluate (inputs, ref outputs);

	}
	
	public void SetInput(List<float> input){
		inputs = input;
	}
	
	public float GetOutput(int ID){
		if (ID >= outputAmount)
			return 0.0f;
		return outputs [ID];
	}
	
	public int GetTotalOutputs() {
		return outputAmount;
	}
	
	public void CreateNet(int numOfHIddenLayers, int numOfInputs, int NeuronsPerHidden, int numOfOutputs){
		inputAmount = numOfInputs;
		outputAmount = numOfOutputs;
		
		for(int i=0; i<numOfHIddenLayers; i++){
			NLayer layer = new NLayer();
			layer.PopulateLayer(NeuronsPerHidden, numOfInputs);
			hiddenLayers.Add (layer);
		}
		
		outputLayer = new NLayer ();
		outputLayer.PopulateLayer (numOfOutputs, NeuronsPerHidden);
	}
	
	public void ReleaseNet(){
		if (inputlayer != null) {
			inputlayer = null;
			inputlayer = new NLayer();
		}
		if (outputLayer != null) {
			outputLayer = null;
			outputLayer = new NLayer();
		}
		for (int i=0; i<hiddenLayers.Count; i++) {
			if(hiddenLayers[i]!=null){
				hiddenLayers[i] = null;
			}
		}
		hiddenLayers.Clear ();
		hiddenLayers = new List<NLayer> ();
	}
	
	public int GetNumofHIddenLayers(){
		return hiddenLayers.Count;
	}
	
	public Genome ToGenome(){
		Genome genome = new Genome ();
		
		for (int i=0; i<this.hiddenLayers.Count; i++) {
			List<float> weights = new List<float> ();
			hiddenLayers[i].GetWeights(ref weights);
			for(int j=0; j<weights.Count;j++){
				genome.weights.Add (weights[j]);
			}
		}
		
		List<float> outweights = new List<float> ();
		outputLayer.GetWeights(ref outweights);
		for (int i=0; i<outweights.Count; i++) {
			genome.weights.Add (outweights[i]);
		}
		
		return genome;
	}
	
	public void FromGenome(Genome genome, int numofInputs, int neuronsPerHidden, int numOfOutputs){
		ReleaseNet ();
		
		outputAmount = numOfOutputs;
		inputAmount = numofInputs;
		
		int weightsForHidden = numofInputs * neuronsPerHidden;
		NLayer hidden = new NLayer ();
		
		List<Neuron> neurons = new List<Neuron>();

		for(int i=0; i<neuronsPerHidden; i++){
			//init
			neurons.Add(new Neuron());
			List<float> weights = new List<float>();
			//init
			
			for(int j=0; j<numofInputs+1;j++){
				weights.Add(0.0f);
				weights[j] = genome.weights[i*neuronsPerHidden + j];
			}
			neurons[i].weights = new List<float>();
			neurons[i].Initilise(weights, numofInputs);
		}
		hidden.LoadLayer (neurons);
		//Debug.Log ("fromgenome, hiddenlayer neruons#: " + neurons.Count);
		//Debug.Log ("fromgenome, hiddenlayer numInput: " + neurons [0].numInputs);
		this.hiddenLayers.Add (hidden);
		
		//Clear weights and reasign the weights to the output
		int weightsForOutput = neuronsPerHidden * numOfOutputs;
		List<Neuron> outneurons = new List<Neuron> ();

		for(int i=0; i<numOfOutputs; i++){
			outneurons.Add(new Neuron());

			List<float> weights = new List<float>();
			
			for(int j=0; j<neuronsPerHidden + 1; j++){
				weights.Add (0.0f);
				weights[j] = genome.weights[i*neuronsPerHidden + j];
			}
			outneurons[i].weights = new List<float>();
			outneurons[i].Initilise(weights, neuronsPerHidden);
		}
		this.outputLayer = new NLayer ();
		this.outputLayer.LoadLayer (outneurons);
		//Debug.Log ("fromgenome, outputlayer neruons#: " + outneurons.Count);
		//Debug.Log ("fromgenome, outputlayer numInput: " + outneurons [0].numInputs);
	}
}


//=================================================================================================================
public class NLayer {
	
	private int totalNeurons;
	private int totalInputs;
	
	
	List<Neuron> neurons = new List<Neuron>();
	
	public float Sigmoid(float a, float p) {
		float ap = (-a) / p;
		return (1 / (1 + Mathf.Exp (ap)));
	}
	
	public float BiPolarSigmoid(float a, float p){
		float ap = (-a) / p;
		return (2 / (1 + Mathf.Exp (ap)) - 1);
	}
	
	public void Evaluate(List<float> input, ref List<float> output){
		int inputIndex = 0;
		//Debug.Log ("input.count " + input.Count);
		//Debug.Log ("totalneuron " + totalNeurons);
		//cycle over all the neurons and sum their weights against the inputs
		for (int i=0; i< totalNeurons; i++) {
			float activation = 0.0f;

			//Debug.Log ("numInputs " + (neurons[i].numInputs - 1));

			//sum the weights to the activation value
			//we do the sizeof the weights - 1 so that we can add in the bias to the activation afterwards.
			for(int j=0; j< neurons[i].numInputs - 1; j++){

				activation += input[inputIndex] * neurons[i].weights[j];
				inputIndex++;
			}
			
			//add the bias
			//the bias will act as a threshold value to
			activation += neurons[i].weights[neurons[i].numInputs] * (-1.0f);//BIAS == -1.0f
			
			output.Add(Sigmoid(activation, 1.0f));
			inputIndex = 0;
		}
	}
	
	public void LoadLayer(List<Neuron> input){
		totalNeurons = input.Count;
		neurons = input;
	}
	
	public void PopulateLayer(int numOfNeurons, int numOfInputs){
		totalInputs = numOfInputs;
		totalNeurons = numOfNeurons;

		if (neurons.Count < numOfNeurons) {
			for(int i=0; i<numOfNeurons; i++){
				neurons.Add(new Neuron());
			}
		}

		for(int i=0; i<numOfNeurons; i++){
			neurons[i].Populate(numOfInputs);
		}
	}
	
	public void SetWeights(List<float> weights, int numOfNeurons, int numOfInputs){
		int index = 0;
		totalInputs = numOfInputs;
		totalNeurons = numOfNeurons;

		if (neurons.Count < numOfNeurons) {
			for (int i=0; i<numOfNeurons - neurons.Count; i++){
				neurons.Add(new Neuron());
			}
		}
		//Copy the weights into the neurons.
		for (int i=0; i<numOfNeurons; i++) {
			if(neurons[i].weights.Count < numOfInputs){
				for(int k=0; k<numOfInputs-neurons[i].weights.Count; k++){
					neurons[i].weights.Add (0.0f);
				}
			}
			for(int j=0; j<numOfInputs; j++){
				neurons[i].weights[j] = weights[index];
				index++;
			}
		}
	}
	
	public void GetWeights(ref List<float> output){
		//Calculate the size of the output list by calculating the amount of weights in each neurons.
		output.Clear ();
		
		for (int i=0; i<this.totalNeurons; i++) {
			for(int j=0; j<neurons[i].weights.Count; j++){
				output[totalNeurons*i + j] = neurons[i].weights[j];
			}
		}
	}
	
	public void SetNeurons(List<Neuron> neurons, int numOfNeurons, int numOfInputs){
		totalInputs = numOfInputs;
		totalNeurons = numOfNeurons;
		this.neurons = neurons;
	}
}


//=============================================================
public class Neuron {
	public int numInputs;
	public List<float> weights = new List<float>();
	
	
	public float RandomFloat()
	{
		float rand = (float)Random.Range (0.0f, 32767.0f);
		return rand / 32767.0f/*32767*/ + 1.0f;
	}
	
	public float RandomClamped()
	{
		return RandomFloat() - RandomFloat();
	}
	
	public float Clamp (float val, float min, float max){
		if (val < min) {
			return min;
		}
		if (val > max) {
			return max;
		}
		return val;
	}
	
	public void Populate(int num){
		this.numInputs = num;
		
		//Initilise the weights
		for (int i=0; i < num; i++){
			weights.Add(RandomClamped());
		}
		
		//add an extra weight as the bias (the value that acts as a threshold in a step activation).
		weights.Add (RandomClamped ());
	}
	
	public void Initilise(List<float> weightsIn, int num){
		this.numInputs = num;
		weights = weightsIn;
	}
}

//===================================
public class Genome{
	public float fitness;
	public int ID;
	public List<float> weights;
	
}


