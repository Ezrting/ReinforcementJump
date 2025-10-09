

using System.Linq;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Threading;
using System;
namespace ReinforcementJump
{
    public class NeuralNetwork
    {
        public Layer[] Layers;
        public  List<double> PastIterations = new();
        public double[] PastOutput;
        public double TotalCost = 0; //Total cost of the network, used for debugging and learning purposes
        public int AccumulatedRewards = 0;
        public NeuralNetwork(params int[] layerSizes) //params keyword allows for a variable number of arguments to be passed to the method
        {
            Layers = new Layer[layerSizes.Length - 1];
            for (int i = 0; i < layerSizes.Length - 1; i++)
            {
                Layers[i] = new Layer(layerSizes[i], layerSizes[i + 1]);
                //Above, the layerSizes[i] refers to the nodes in(the size of the previous layer) whilst layerSizes[i+1] refers to the current layer being iterated
            }
            PastOutput = new double[layerSizes[layerSizes.Length - 1]];

        }
        public double Cost(DataPoint dataPoint)
        {
            double[] outputs = CalculateOutputs(dataPoint.Inputs);
            Layer OutputLayer = Layers[Layers.Length - 1];
            double cost = 0;
            for (int i = 0; i < outputs.Length; i++)
            {
                double ExpectedOutput = dataPoint.ExpectedOutputs[i];
              //  Debug.WriteLine(ExpectedOutput);
                double CostToAdd = OutputLayer.NodeCost(outputs[i], ExpectedOutput);
                string ResultBinary = "";
                for (int a = 0; a < outputs.Length; a++)
                {
                    ResultBinary += outputs[a].ToString();
                    //  Console.Write(ResultConversion[i]);
                }
                cost += CostToAdd;
   
            }
            return cost;
        }
        public double Cost(DataPoint[] dataPoints)
        {
            double TotalCost = 0;
            foreach (DataPoint DP in dataPoints)
            {
                TotalCost += Cost(DP);

            }
            return TotalCost;
        }
        public double[] CalculateOutputs(double[] inputs)
        {
            for(int i = 0; i < Layers.Length; i++)
            {
                Layer layer = Layers[i];
                bool IsOutputLayer = i == Layers.Length - 1;
                inputs = layer.CalculateOutputs(inputs, IsOutputLayer);
            }
            return inputs; // At the end of the loop, inputs will be the output of the last layer(output layer)
        }
        public void ApplyAllGradients(double LearnRate)
        {
            foreach (Layer layer in Layers)
            {
                layer.ApplyGradients(LearnRate);

            }
        }
        void UpdateAllGradients(DataPoint dataPoint)
        {
            CalculateOutputs(dataPoint.Inputs);
            Layer OutputLayer = Layers[Layers.Length - 1];
            float Strength = dataPoint.Strength;
            double[] NodeValues = OutputLayer.CalculateOutputLayerNodeValues(dataPoint.ExpectedOutputs);
            //NodeValues are the values of the nodes in the output layer

            OutputLayer.UpdateGradients(NodeValues, Strength);
            for (int HiddenLayerIndex = Layers.Length - 2; HiddenLayerIndex >= 0; HiddenLayerIndex--)
            {
                Layer CurrentLayer = Layers[HiddenLayerIndex];
                NodeValues = CurrentLayer.CalculateHiddenLayerNodeValues(Layers[HiddenLayerIndex + 1], NodeValues);
                CurrentLayer.UpdateGradients(NodeValues, Strength);
            }
        }
        public void ClearAllGradients()
        {
            foreach (Layer layer in Layers)
            {
                layer.ClearGradients();
            }
        }
        public void Learn(DataPoint[] TrainingData, double LearnRate = 0.2)
        {
            foreach (DataPoint dataPoint in TrainingData)
            {
            //    UpdateAllGradients(dataPoint);
            }
            System.Threading.Tasks.Parallel.For(0, TrainingData.Length, (i) =>
            {
                UpdateAllGradients(TrainingData[i]);
            });

            ApplyAllGradients(LearnRate / TrainingData.Length);

            ClearAllGradients();
            /*
            const double h = 0.0001;
            double OriginalCost = Cost(TrainingData);
            foreach (Layer layer in Layers)
            {
                //Calculate cost gradient for weights
                for (int NodeIn = 0; NodeIn < layer.NumberOfNodesIn; NodeIn++)
                {
                    for (int NodeOut = 0; NodeOut < layer.NumberOfNodesOut; NodeOut++)
                    {
                        layer.weights[NodeIn, NodeOut] += h; // go check out sebastian lague
                        double deltaCost = Cost(TrainingData) - OriginalCost; // this is the (often infinitesimal) difference that adding h has made to the cost
                        layer.weights[NodeIn, NodeOut] -= h;
                        layer.costGradientW[NodeIn, NodeOut] = deltaCost / h; //Set the cost gradient to the rate of change(with respect to the weight)
                                                                              //Debug.WriteLine(layer.weights[NodeIn, NodeOut]);
                       // Debug.WriteLine(deltaCost / h);
                    }
                }
                for (int biasIndex = 0; biasIndex < layer.biases.Length; biasIndex++)
                {
                    layer.biases[biasIndex] += h; // go check out sebastian lague
                    double deltaCost = Cost(TrainingData) - OriginalCost; // this is the difference that adding h has made to the cost
                    layer.biases[biasIndex] -= h;
                    layer.costGradientB[biasIndex] = deltaCost / h;
                }
                //Calculate cost gradient for biases
            }
            ApplyAllGradients(LearnRate);
            double TheCost = Cost(TrainingData);
            PastIterations.Add(TheCost);
            while (PastIterations.Count > 100)
            {
                PastIterations.RemoveAt(0);
            }
          //  Debug.WriteLine(PastIterations.Sum() / PastIterations.Count);
            */
        }

        public (double[], int) Classify(double[] inputs)
        {
            double[] outputs = CalculateOutputs(inputs); //Simply find the outputs
            int MaxIndex = outputs.ToList().IndexOf(outputs.Max());
            return (outputs, MaxIndex); //Find the index of the maximum output, which is the classification
        }
        public double[] CreateExpectedOutputs(DataPoint dataPointToChange, float Strength)
        {
            double[] DataPointEP = dataPointToChange.ExpectedOutputs;
            int ChosenAction = dataPointToChange.ChosenAction;
            double[] FakeEP = new double[DataPointEP.Length];
            dataPointToChange.Strength = Strength * Math.Abs(AccumulatedRewards);
            if (AccumulatedRewards > 0)
            {
                for (int i = 0; i < FakeEP.Length; i++)
                {
                    if (i != ChosenAction)
                    {
                        FakeEP[i] = 0;
                    }
                    else
                    {
                        FakeEP[ChosenAction] = 1;
                    }
                }
            }
            else if (AccumulatedRewards <= 0)
            {
                for (int i = 0; i < FakeEP.Length; i++)
                {
                    if (i != ChosenAction)
                    {
                        //All other outputs stay the same because we don't know what effect they have had yet
                        FakeEP[i] = DataPointEP[i];
                    }
                    else
                    {
                        FakeEP[ChosenAction] = AccumulatedRewards;
                    }
                }
            }
            dataPointToChange.ExpectedOutputs = FakeEP;
            return FakeEP;
        }
    }
    public class LayerLearnData
    {
        public double[] WeightedInputs;
        public double[] Activations;
        public double[] Inputs;
        public LayerLearnData(int NodesOut, int NodesIn)
        {
            WeightedInputs = new double[NodesOut];
            Activations = new double[NodesOut];
            Inputs = new double[NodesIn];
        }

    }
}
