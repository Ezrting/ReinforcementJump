using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReinforcementJump
{
    public class Layer
    {
        public int NumberOfNodesIn;
        public int NumberOfNodesOut;
        public double[,] costGradientW; // stores cost gradients for weights(derivative of cost with respect to weight)
        public double[] costGradientB; // cost gradients for biases
        public double[,] weights;
        public double[] biases;
        LayerLearnData learnData;
        public Layer(int numberOfNodesIn, int numberOfNodesOut)
        {
            NumberOfNodesIn = numberOfNodesIn;
            NumberOfNodesOut = numberOfNodesOut;
            weights = new double[NumberOfNodesIn, NumberOfNodesOut];
            biases = new double[NumberOfNodesOut];
            costGradientW = new double[numberOfNodesIn, numberOfNodesOut];
            costGradientB = new double[numberOfNodesOut];
            learnData = new LayerLearnData(numberOfNodesOut, numberOfNodesIn);
            //Initialise random weights and biases from -1 to 1
            Random random = new Random();
            for (int i = 0; i < weights.GetLength(0) - 1; i++)
            {
                for (int j = 0; j < weights.GetLength(1); j++)
                {
                    double RandomDouble = random.NextDouble(); //Seeding time!
                    weights[i, j] = (RandomDouble * 2 - 1) / Math.Sqrt(NumberOfNodesIn);
                }
            }

            /*
            for (int a = 0; a < biases.GetLength(0); a++)
            {
                double RandomDouble = random.NextDouble(); //Seeding time!
                biases[a] = RandomDouble * 2 - 1;
            }
            */
        }
        public void ApplyGradients(double LearnRate)
        {
            double weightsum = 0;
            float Gamma = 0;
            for (int NodeOut = 0; NodeOut < NumberOfNodesOut; NodeOut++)
            {
                biases[NodeOut] -= LearnRate * costGradientB[NodeOut];
                for (int NodeIn = 0; NodeIn < NumberOfNodesIn; NodeIn++)
                {
                    weights[NodeIn, NodeOut] -= costGradientW[NodeIn, NodeOut] * LearnRate * Gamma;
                    weightsum += weights[NodeIn, NodeOut];
                }
            }
            //  Debug.WriteLine("Weight sum: "+ weightsum);
        }
        public double NodeCost(double OutputActivation, double ExpectedOutput)
        {
            double error = OutputActivation - ExpectedOutput;
            // Debug.WriteLine("Output: " + OutputActivation + " Expected: " + ExpectedOutput + " Error: " + error * error);
            return error * error; // Return the cost
        }
        public double NodeCostDerivative(double OutputActivation, double ExpectedOutput)
        {
            return 2 * (OutputActivation - ExpectedOutput); // Return the cost derivative
        }
        public double[] CalculateOutputs(double[] inputs, bool IsOutputLayer)
        {
            double[] weightedInputs = new double[NumberOfNodesOut]; //Create an array to hold the weighted inputs

            for (int NodeOutSearching = 0; NodeOutSearching < NumberOfNodesOut; NodeOutSearching++) // Loop through every node in the layer and apply a new value based on its inputs
            {
                double weightedInput = biases[NodeOutSearching];
                for (int NodeInSearching = 0; NodeInSearching < NumberOfNodesIn; NodeInSearching++) // Loop through every input being recieved to the node
                {
                    weightedInput += inputs[NodeInSearching] * weights[NodeInSearching, NodeOutSearching]; //Add the input times the respective weight to the bias
                    learnData.Inputs[NodeInSearching] = inputs[NodeInSearching];
                }
                weightedInputs[NodeOutSearching] = Sigmoid(weightedInput); //Apply the sigmoid function to the weighted input, which is now the sum of all of the weighted inputs in the node plus the bias
                learnData.WeightedInputs[NodeOutSearching] = weightedInput;
                learnData.Activations[NodeOutSearching] = weightedInputs[NodeOutSearching];
            }



            return weightedInputs;
        }
        public void ClearGradients()
        {
            costGradientB = new double[NumberOfNodesOut];
            costGradientW = new double[NumberOfNodesIn, NumberOfNodesOut];
        }
        public double[] CalculateOutputLayerNodeValues(double[] expectedOutputs)
        {
            double[] NodeValues = new double[expectedOutputs.Length];
            for (int i = 0; i < NodeValues.Length; i++)
            {
               double CostDerivative = NodeCostDerivative(learnData.Activations[i], expectedOutputs[i]);
               double ActivationDerivative = SigmoidDerivative(learnData.WeightedInputs[i]);
               NodeValues[i] = CostDerivative * ActivationDerivative;
            }
            return NodeValues;
        }
        public double[] CalculateHiddenLayerNodeValues(Layer OldLayer, double[] OldNodeValues)
        {
            //"old layer" refers to the next layer the neural network(since we are going backwards)
            double[] NewNodeValues = new double[NumberOfNodesOut];

            for (int NewNodeIndex = 0; NewNodeIndex < NewNodeValues.Length; NewNodeIndex++)
            {
                double NewNodeValue = 0; //new node value is the sum of all the old layer node values multiplied by the old layer weights
                for (int OldNodeIndex = 0; OldNodeIndex < OldNodeValues.Length; OldNodeIndex++)
                {
                    //This is the derivative of a weighted input with respect to the weight. Like 3x + 2, the derivative is 3. So here it's just equal to the weight.
                    double WeightedInputDerivative = OldLayer.weights[NewNodeIndex, OldNodeIndex];

                    NewNodeValue += WeightedInputDerivative * OldNodeValues[OldNodeIndex];
                        //When we say "Old Node Values", we are referring to a specific value produced by the cost derivative with respect to activation multiplied by the activation with respect to the weighted input
                }
                NewNodeValue = NewNodeValue * SigmoidDerivative(learnData.WeightedInputs[NewNodeIndex]);
                NewNodeValues[NewNodeIndex] = NewNodeValue;
            }

            return NewNodeValues;
        }
        public void UpdateGradients(double[] NodeValues, float Strength)
        {
            for (int nodeOut = 0; nodeOut < NumberOfNodesOut; nodeOut++)
            {
                for (int nodeIn = 0; nodeIn < NumberOfNodesIn; nodeIn++)
                {
                    double derivativeOfCostWithRespectToWeight = learnData.Inputs[nodeIn] * NodeValues[nodeOut];
                    //derivative is being added to the weight here because ultimately we want to calculate the average gradient across all data in the training batch
                    costGradientW[nodeIn, nodeOut] += derivativeOfCostWithRespectToWeight;
                }
                double derivativeOfCostWithRespectToBias = 1 * NodeValues[nodeOut];
                costGradientB[nodeOut] += derivativeOfCostWithRespectToBias;
            }
        }
        public double Sigmoid(double x) // Sigmoid function to apply to the weighted inputs
        {
            return 1 / (1 + Math.Exp(-x));
        }
        public double SigmoidDerivative(double x)  // Derivative of the sigmoid function
        {
            return Sigmoid(x) * (1 - Sigmoid(x));
        }
    }


}