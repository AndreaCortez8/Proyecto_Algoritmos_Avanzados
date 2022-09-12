using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;


namespace PerceptronVideo
{
    class Program
    {
        static List<double[]> input = new List<double[]>();
        static List<double[]> output = new List<double[]>();

        static int inputCount = 5;
        static int outputCount = 1;

        static double inputMax = 3000;
        static double inputMin = 0.4;

        static double outputMax = 1;
        static double outputMin = 0;

        static bool loadNetwork = false;
        static bool saveNetwork = true;

        static string dataPath = @"D:\pruebas\Multi-layer-perceptron-master\Project\PerceptronMejorado\pruebaOctubre.txt";
        static string outputPath = @"D:\pruebas\Multi-layer-perceptron-master\Project\PerceptronMejorado\pruebaOctubreOutput.csv";
        static string neuronPath = @"D:\pruebas\Multi-layer-perceptron-master\Project\PerceptronMejorado\neuralNetwork.bin";        


        static void ReadData()
        {
            string data = System.IO.File.ReadAllText(dataPath).Replace("\r", "");

            string[] row = data.Split(Environment.NewLine.ToCharArray());
            for (int i = 0; i < row.Length; i++)
            {
                string[] rowData = row[i].Split(';');

                double[] inputs = new double[inputCount];
                double[] outputs = new double[outputCount];

                for (int j = 0; j < rowData.Length; j++)
                {
                    if (j < inputCount)
                    {
                        inputs[j] = Normalize(double.Parse(rowData[j]), inputMin, inputMax);
                    }
                    else
                    {
                        outputs[j - inputCount] = Normalize(double.Parse(rowData[j]), outputMin, outputMax);
                    }
                }

                input.Add(inputs);
                output.Add(outputs);
            }

        }

        static double Normalize(double value, double min, double max)
        {
            return (value - min) / (max - min);
        }
        static double InverseNormalize(double value, double min, double max)
        {
            return value * (max - min) + min;
        }

        static void outputRequest(Perceptron p)
        {
            while (true)
            {
                double[] val = new double[inputCount];
                for (int i = 0; i < inputCount; i++)
                {
                    Console.WriteLine("Inserte valor " + i + ": ");
                    val[i] = Normalize(double.Parse(Console.ReadLine()), inputMin, inputMax);
                }

                double[] sal = p.Activate(val);
                for (int i = 0; i < outputCount; i++)
                {
                    Console.Write("Respuesta " + i + ": " + InverseNormalize(sal[i], outputMin, outputMax) + " ");
                }

                Console.WriteLine("");
            }
        }

        static void Evaluate(Perceptron p, double from, double to, double step)
        {
            string output = "";

            for (double i = from; i < to; i += step)
            {
                double res = p.Activate(new double[] { Normalize(i, inputMin, inputMax) })[0];
                output += i + ";" + InverseNormalize(res, outputMin, outputMax) + "\n";
                Console.WriteLine(i + ";" + res + "\n");
            }

            System.IO.File.WriteAllText(outputPath, output);
        }

        static void Main(string[] args)
        {

            Perceptron p;

            if (!loadNetwork)
            {
                ReadData();
                p = new Perceptron(new int[] { input[0].Length, 20, 10, output[0].Length });

                while (!p.Learn(input, output, 0.02, 0.5, 25000))
                {
                    p = new Perceptron(new int[] { input[0].Length, 20, 10, output[0].Length });
                }
                if (saveNetwork)
                {
                    FileStream fs = new FileStream(neuronPath, FileMode.Create);
                    BinaryFormatter formatter = new BinaryFormatter();
                    try
                    {
                        formatter.Serialize(fs, p);
                    }
                    catch (SerializationException e)
                    {
                        Console.WriteLine("Failed to serialize. Reason: " + e.Message);
                        throw;
                    }
                    finally
                    {
                        fs.Close();
                    }
                }
            }
            else
            {
                FileStream fs = new FileStream(neuronPath, FileMode.Open);
                try
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    p = (Perceptron)formatter.Deserialize(fs);
                }
                catch (SerializationException e)
                {
                    Console.WriteLine("Failed to deserialize. Reason: " + e.Message);
                    throw;
                }
                finally
                {
                    fs.Close();
                }
            }
            outputRequest(p);

        }
    }

    [Serializable]
    class Perceptron
    {
        List<Layer> layers;

        public Perceptron(int[] neuronsPerlayer)
        {
            layers = new List<Layer>();
            Random r = new Random();

            for (int i = 0; i < neuronsPerlayer.Length; i++)
            {
                layers.Add(new Layer(neuronsPerlayer[i], i == 0 ? neuronsPerlayer[i] : neuronsPerlayer[i - 1], r));
            }
        }
        public double[] Activate(double[] inputs)
        {
            double[] outputs = new double[0];
            for (int i = 1; i < layers.Count; i++)
            {
                outputs = layers[i].Activate(inputs);
                inputs = outputs;
            }
            return outputs;
        }
        double IndividualError(double[] realOutput, double[] desiredOutput)
        {
            double err = 0;
            for (int i = 0; i < realOutput.Length; i++)
            {
                err += Math.Pow(realOutput[i] - desiredOutput[i], 2);
            }
            return err;
        }
        double GeneralError(List<double[]> input, List<double[]> desiredOutput)
        {
            double err = 0;
            for (int i = 0; i < input.Count; i++)
            {
                err += IndividualError(Activate(input[i]), desiredOutput[i]);
            }
            return err;
        }
        List<string> log;
        public bool Learn(List<double[]> input, List<double[]> desiredOutput, double alpha, double maxError, int maxIterations)
        {
            double err = 99999;
            log = new List<string>();
            while (err > maxError)
            {
                maxIterations--;
                if (maxIterations <= 0)
                {
                    Console.WriteLine("---------------------Minimo local-------------------------");
                    return false;
                }

                if (Console.KeyAvailable)
                {
                    Console.WriteLine("---------------------Boton de escape-------------------------");
                    System.IO.File.WriteAllLines(@"D:\pruebas\Multi-layer-perceptron-master\log\LogTail.txt", log.ToArray());
                    return true;
                }

                ApplyBackPropagation(input, desiredOutput, alpha);
                err = GeneralError(input, desiredOutput);
                log.Add(err.ToString());
                if (err < 30)
                {
                    Console.WriteLine(err);
                }
                
            }
            System.IO.File.WriteAllLines(@"D:\pruebas\Nueva carpeta\LogTail.txt", log.ToArray());
            return true;
        }

        List<double[]> sigmas;
        List<double[,]> deltas;

        void SetSigmas(double[] desiredOutput)
        {
            sigmas = new List<double[]>();
            for (int i = 0; i < layers.Count; i++)
            {
                sigmas.Add(new double[layers[i].numberOfNeurons]);
            }
            for (int i = layers.Count - 1; i >= 0; i--)
            {
                for (int j = 0; j < layers[i].numberOfNeurons; j++)
                {
                    if (i == layers.Count - 1)
                    {
                        double y = layers[i].neurons[j].lastActivation;
                        sigmas[i][j] = (Neuron.Sigmoid(y) - desiredOutput[j]) * Neuron.SigmoidDerivated(y);
                    }
                    else
                    {
                        double sum = 0;
                        for (int k = 0; k < layers[i + 1].numberOfNeurons; k++)
                        {
                            sum += layers[i + 1].neurons[k].weights[j] * sigmas[i + 1][k];
                        }
                        sigmas[i][j] = Neuron.SigmoidDerivated(layers[i].neurons[j].lastActivation) * sum;
                    }
                }
            }
        }
        void SetDeltas()
        {
            deltas = new List<double[,]>();
            for (int i = 0; i < layers.Count; i++)
            {
                deltas.Add(new double[layers[i].numberOfNeurons, layers[i].neurons[0].weights.Length]);
            }
        }
        void AddDelta()
        {
            for (int i = 1; i < layers.Count; i++)
            {
                for (int j = 0; j < layers[i].numberOfNeurons; j++)
                {
                    for (int k = 0; k < layers[i].neurons[j].weights.Length; k++)
                    {
                        deltas[i][j, k] += sigmas[i][j] * Neuron.Sigmoid(layers[i - 1].neurons[k].lastActivation);
                    }
                }
            }
        }
        void UpdateBias(double alpha)
        {
            for (int i = 0; i < layers.Count; i++)
            {
                for (int j = 0; j < layers[i].numberOfNeurons; j++)
                {
                    layers[i].neurons[j].bias -= alpha * sigmas[i][j];
                }
            }
        }
        void UpdateWeights(double alpha)
        {
            for (int i = 0; i < layers.Count; i++)
            {
                for (int j = 0; j < layers[i].numberOfNeurons; j++)
                {
                    for (int k = 0; k < layers[i].neurons[j].weights.Length; k++)
                    {
                        layers[i].neurons[j].weights[k] -= alpha * deltas[i][j, k];
                    }
                }
            }
        }
        void ApplyBackPropagation(List<double[]> input, List<double[]> desiredOutput, double alpha)
        {
            SetDeltas();
            for (int i = 0; i < input.Count; i++)
            {
                Activate(input[i]);
                SetSigmas(desiredOutput[i]);
                UpdateBias(alpha);
                AddDelta();
            }
            UpdateWeights(alpha);

        }
    }
    [Serializable]
    class Layer
    {
        public List<Neuron> neurons;
        public int numberOfNeurons;
        public double[] output;

        public Layer(int _numberOfNeurons, int numberOfInputs, Random r)
        {
            numberOfNeurons = _numberOfNeurons;
            neurons = new List<Neuron>();
            for (int i = 0; i < numberOfNeurons; i++)
            {
                neurons.Add(new Neuron(numberOfInputs, r));
            }
        }

        public double[] Activate(double[] inputs)
        {
            List<double> outputs = new List<double>();
            for (int i = 0; i < numberOfNeurons; i++)
            {
                outputs.Add(neurons[i].Activate(inputs));
            }
            output = outputs.ToArray();
            return outputs.ToArray();
        }

    }
    [Serializable]
    class Neuron
    {
        public double[] weights;
        public double lastActivation;
        public double bias;

        public Neuron(int numberOfInputs, Random r)
        {
            bias = 10 * r.NextDouble() - 5;
            weights = new double[numberOfInputs];
            for (int i = 0; i < numberOfInputs; i++)
            {
                weights[i] = 10 * r.NextDouble() - 5;
            }
        }
        public double Activate(double[] inputs)
        {
            double activation = bias;

            for (int i = 0; i < weights.Length; i++)
            {
                activation += weights[i] * inputs[i];
            }

            lastActivation = activation;
            return Sigmoid(activation);
        }
        public static double Sigmoid(double input)
        {
            return 1 / (1 + Math.Exp(-input));
        }
        public static double SigmoidDerivated(double input)
        {
            double y = Sigmoid(input);
            return y * (1 - y);
        }

    }
}
