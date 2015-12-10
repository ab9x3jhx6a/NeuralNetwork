using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NNet : MonoBehaviour {
	private int inputAmount;
	private int outputAmount;

	private List<float> inputs;
	private NLayer inputlayer;

	private List<NLayer> hiddenLayers;
	private NLayer outputLayer;

	private List<float> outputs;

	public void Update(){
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
		}
		if (outputLayer != null) {
			outputLayer = null;
		}
		for (int i=0; i<hiddenLayers.Count; i++) {
			if(hiddenLayers[i]!=null){
				hiddenLayers[i] = null;
			}
		}
		hiddenLayers.Clear ();
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

	public void FromGenome(ref Genome genome, int numofInputs, int neuronsPerHidden, int numOfOutputs){
		ReleaseNet ();

		outputAmount = numOfOutputs;
		inputAmount = numofInputs;

		int weightsForHidden = numofInputs * neuronsPerHidden;
		NLayer hidden = new NLayer ();

		List<Neuron> neurons = new List<Neuron>();
		for(int i=0; i<neuronsPerHidden; i++){
			List<float> weights = new List<float>();

			for(int j=0; j<numofInputs+1;j++){
				weights[j] = genome.weights[i*neuronsPerHidden + j];
			}
			neurons[i].Initilise(weights, numofInputs);
		}
		hidden.LoadLayer (neurons);
		this.hiddenLayers.Add (hidden);

		//Clear weights and reasign the weights to the output
		int weightsForOutput = neuronsPerHidden * numOfOutputs;
		neurons.Clear ();

		for(int i=0; i<numOfOutputs; i++){
			List<float> weights = new List<float>();

			for(int j=0; j<neuronsPerHidden + 1; j++){
				weights[j] = genome.weights[i*neuronsPerHidden + j];
			}
			neurons[i].Initilise(weights, neuronsPerHidden);
		}
		outputLayer = new NLayer ();
		this.outputLayer.LoadLayer (neurons);
	}
}


//=================================================================================================================
public class NLayer {
	
	private int totalNeurons;
	private int totalInputs;
	
	
	List<Neuron> neurons;
	
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
		//cycle over all the neurons and sum their weights against the inputs
		for (int i=0; i< totalNeurons; i++) {
			float activation = 0.0f;
			
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

	public void LoadLayer(List<Neuron> intput){
		totalNeurons = intput.Count;
		neurons = intput;
	}

	public void PopulateLayer(int numOfNeurons, int numOfInputs){
		totalInputs = numOfInputs;
		totalNeurons = numOfNeurons;
		
		for(int i=0; i<numOfNeurons; i++){
			neurons[i].Populate(numOfInputs);
		}
	}
	
	public void SetWeights(List<float> weights, int numOfNeurons, int numOfInputs){
		int index = 0;
		totalInputs = numOfInputs;
		totalNeurons = numOfNeurons;
		//Copy the weights into the neurons.
		for (int i=0; i<numOfNeurons; i++) {
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
	public List<float> weights;
	
	
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

