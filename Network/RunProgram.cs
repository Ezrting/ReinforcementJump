using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System;

namespace ReinforcementJump
{
    internal class Program
    {
        /*
        static void RunNetwork(string[] args)
        {
            List<DataPoint> TrainingDataList = new List<DataPoint>();
            //   NeuralNetwork neuralNetwork = new NeuralNetwork(22,10, 26); // 26 outputs to represent 16 bits and 1 bit of negative or positive as well as 9 bits to represent remainders in division, there will always be 22 inputs



            NeuralNetwork neuralNetwork = new NeuralNetwork(22,10,10, 26); // 26 outputs to represent 16 bits and 1 bit of negative or positive as well as 9 bits to represent remainders in division, there will always be 22 inputs
            int Iterations = 0;
            int CorrectIterations = 0;
            while (true == true)
            {
                Thread.Sleep(2);
                DataPoint[] TrainingData = new DataPoint[3];
                for (int g = 0; g < 3; g++)
                {
                    Iterations++;
                    var (NewDataPoint, Num1, Num2, OperatorSymbol) = RecieveRandomInputs(neuralNetwork);
                    //Let's see if it works!
                    //  int MaxIndex = neuralNetwork.Classify(new double[] { 1, 1 }); // Classify the inputs 1, 1
                    // Debug.WriteLine(MaxIndex);

                    //Convert to double array
                    Debug.WriteLine("");
                    double[] outputs = neuralNetwork.Classify(NewDataPoint.Inputs);
                    double TotalDifference = 0;
                    for (int i = 0; i < outputs.Length; i++)
                    {
                        // Console.Write(outputs[i] + "-");
                        double OriginalOutput = outputs[i];
                        if (outputs[i] > 0.5)
                        {
                            outputs[i] = 1;
                        }
                        else
                        {
                            outputs[i] = 0;
                        }
                        double Difference = Math.Abs(OriginalOutput - outputs[i]);
                        TotalDifference += Difference;
                    }
                    string ArrayToString = string.Join("", outputs.Select(x => x.ToString())); // Concatnate the outputs into a string
                                                                                                 // Debug.WriteLine(ArrayToString);
                    string Answer1 = ArrayToString[..17];
                    string Remainder = ArrayToString.Substring(17, 9);
                    int PredictedResult = ConvertFromBinary(Answer1);
                    string StringToDisplay = Num1.ToString() + " " + OperatorSymbol + " " + Num2.ToString() + " = " + PredictedResult;
                    int RemainderBinaryConversion = ConvertFromBinary(Remainder);
                    string RemainderString = RemainderBinaryConversion.ToString();
                    if (OperatorSymbol == "/")
                    {
                        if (RemainderBinaryConversion == 0)
                        {
                            StringToDisplay += " with no remainder";
                        }
                        else
                        {
                            StringToDisplay += " with a remainder of " + RemainderString;
                        }
                    }
                    Debug.WriteLine(StringToDisplay);
                    int ActualRemainder = ConvertFromBinary(NewDataPoint.StringFormOutputs.Substring(17, 9));
                    //   Debug.WriteLine("Certainty: " + 100 * (outputs.Length * 1 - TotalDifference)/outputs.Length + "%");
                    int CorrectResult = ConvertFromBinary(NewDataPoint.StringFormOutputs[..17]);
                    if (CorrectResult == PredictedResult && RemainderBinaryConversion == ActualRemainder)
                    {
                        CorrectIterations++;
                    }
                    Debug.WriteLine("Accuracy: " + CorrectIterations / (double)Iterations);
                    //Display correct results


                    string CorrectStringPart1 = "Expected result: " + CorrectResult;
                    string CorrectStringPart2 = "Expected remainder: " + ActualRemainder;
                    string CompleteString = CorrectStringPart1;
                    if (OperatorSymbol == "/")
                    {
                        CompleteString += ", " + CorrectStringPart2;
                    }
                    Debug.WriteLine(CompleteString);
                    TrainingData[g] = NewDataPoint;
                    if (Iterations >= 1000)
                    {
                        Iterations = 0;
                        CorrectIterations = 0;
                    }
                }
                neuralNetwork.Learn(TrainingData);
            }
        }
        public static (DataPoint, int, int, string) RecieveRandomInputs(NeuralNetwork Network)
        {
            Random NewRandomSeed = new Random();
            int Number1 = NewRandomSeed.Next(0,17); // Random number between -128 and 127
            int Number2 = NewRandomSeed.Next(0,17); // Random number between -128 and 127ewRandomSeed.Next(-128, 127); // Random number between -128 and 127
       //     int Number1 = NewRandomSeed.Next(0,3); // Random number between -128 and 127
       //     int Number2 = NewRandomSeed.Next(0,3); // Random number between -128 and 127ewRandomSeed.Next(-128, 127); // Random number between -128 and 127
            string Operator = "Add";
            string OperatorSymbol = "+";
            Random RandomOperator = new Random();
            int OperatorID = RandomOperator.Next(1,3);
            switch (OperatorID)
            {
                case 1:
                    Operator = "Add";
                    break;
                case 2:
                    Operator = "Subtract";
                    break;
                case 3:
                    Operator = "Multiply";
                    break;
                case 4:
                    Operator = "Divide";
                    break;
            }
            switch (Operator)
            {
                case "Add":
                    Operator = "1000";
                    OperatorSymbol = "+";
                    break;
                case "Subtract":
                    Operator = "0100";
                    OperatorSymbol = "-";
                    break;
                case "Multiply":
                    Operator = "0010";
                    OperatorSymbol = "x";
                    break;
                case "Divide":
                    Operator = "0001";
                    OperatorSymbol = "/";
                    if (Number2 == 0)
                    {
                        Number2 = 1;
                    }
                    break;
            }
            string BinaryNumber1 = ConvertToBinary(Number1);
            string BinaryNumber2 = ConvertToBinary(Number2);
            string BinString = BinaryNumber1 + BinaryNumber2 + Operator;

            double[] InputsToEnter = new double[BinString.Length];
            for (int i = 0; i < BinString.Length; i++)
            {
                InputsToEnter[i] = Convert.ToDouble(BinString[i].ToString());
            }
            DataPoint NewDataPoint = new DataPoint(InputsToEnter);
            return (NewDataPoint, Number1, Number2, OperatorSymbol);
        }
        public void TrainNetwork(NeuralNetwork Network, double[] inputs)
        {

        }
        public static string ConvertToBinary(int number, int MaxBits = 8)
        {
            bool IsNegative = false; // Flag to check if the number is negative
            if (number < 0)
            {
                number = Math.Abs(number); // Convert negative numbers to positive
                IsNegative = true;
            }
            string BinaryConversion = Convert.ToString(number, 2);
            //Add leading zeros to make it 9 bits(First bit already accounted for after this loop)
            int BinDifference = MaxBits - BinaryConversion.Length;
            for (int i = 0; i < BinDifference; i++)
            {
                BinaryConversion = "0" + BinaryConversion;
            }
            if (IsNegative == true)
            {
                BinaryConversion = "1" + BinaryConversion;
            }
            else
            {
                BinaryConversion = "0" + BinaryConversion;
            }

            if (BinDifference < 0)
            {
                throw new NotImplementedException($"Number was higher than the {MaxBits} bit limit");
            }
            return BinaryConversion;
        }
        public static int ConvertFromBinary(string Binary)
        {
            //Run strings in the order with each character being twice as powerful as before.
            bool IsNegative = false; // Flag to check if the number is negative
            if (Binary[0] == '1') //chars are single-quotes, strings are double quotes
            {
                IsNegative = true;
            }
            int result = 0;
            for (int i = 1; i < Binary.Length; i++) //Start from the second character in the string to convert back to base 10
            {
                char BinChar = Binary[i];
                if (BinChar != '0' && BinChar != '1')
                {
                    Debug.WriteLine(BinChar);
                    throw new NotImplementedException("Binary string contains a digit other than 0 or 1");
                }
                try
                {
                    switch (BinChar)
                    {
                        case '0':
                            break;
                        case '1':
                            result += (int)Math.Pow(2, Binary.Length - i - 1);
                            break;
                    }
                }
                catch(Exception ex)
                {
                    Debug.WriteLine("An error occured: " + ex.Message);
                }
            }
            if (IsNegative)
            {
                result = -result;
            }
            return result;
        }
         */
    }
}
