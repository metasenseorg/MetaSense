using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeLibrary
{
    /// <summary>
    /// The input for the features of the neural network.
    /// </summary>
    public class FeaturesInput
    {
        private const double CELSUIS_TO_KELVIN_CONSTANT = 273.15;

        public double NO2 { get; set; }
        public double O3 { get; set; }
        public double CO { get; set; }
        public double Temperature { get; set; }
        public double AbsoluteHumidity { get; set; }
        public double Pressure { get; set; }

        /// <summary>
        /// Initializes the inputs for the features.
        /// </summary>
        /// <param name="no2">The voltage difference in mV of the NO2 electrodes
        ///     (auxiliary minus working)</param>
        /// <param name="o3">The voltage difference of the O3 electrodes
        ///     (auxiliary minus working)</param>
        /// <param name="co">The voltage difference of the CO electrodes
        ///     (auxiliary minus working)</param>
        /// <param name="temp">Temperature (degrees Celsius)</param>
        /// <param name="absHumidity">Absolute humidity</param>
        /// <param name="pres">Pressure (mbar)</param>
        public FeaturesInput(double no2, double o3, double co, double temp, double absHumidity,
                             double pres)
        {
            NO2 = no2;
            O3 = o3;
            CO = co;
            Temperature = temp;
            AbsoluteHumidity = absHumidity;
            Pressure = pres;
        }

        /// <summary>
        /// Returns a 2D array containing all the inputs for the features.
        /// </summary>
        /// <returns>A 2D array containing all the inputs for the features in the order of:
        ///     NO2 voltage difference (mV), O3 voltage difference (mV),
        ///     CO voltage difference (mV), temperature (degrees Celsius),
        ///     absolute humidity, pressure (mbar)</returns>
        public double[,] To2dArray()
        {
            return new double[1, 6] { { NO2, O3, CO, Temperature, AbsoluteHumidity, Pressure } };
        }

        /// <summary>
        /// Returns a 2D array containing all the voltage differences.
        /// </summary>
        /// <returns>A 2D array containing all the voltage differences in the order of:
        ///     NO2 voltage difference (mV), O3 voltage difference (mV),
        ///     CO voltage difference (mV)</returns>
        public double[,] VoltDiffsTo2dArray()
        {
            return new double[1, 3] { { NO2, O3, CO } };
        }

        /// <summary>
        /// Determines whether two objects are equal.
        /// </summary>
        /// <param name="obj">The object to compare to the current object.</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var comparison = obj as FeaturesInput;

            if (comparison == null)
            {
                return false;
            }

            return NO2 == comparison.NO2 && O3 == comparison.O3 && CO == comparison.CO &&
                Temperature == comparison.Temperature &&
                AbsoluteHumidity == comparison.AbsoluteHumidity && Pressure == comparison.Pressure;
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Create the input for the features of the neural network from the sensor measurements.
        /// </summary>
        /// <param name="coA_mV">CO auxiliary electrode reading (mV)</param>
        /// <param name="coW_mV">CO working electrode reading (mV)</param>
        /// <param name="o3A_mV">O3 auxiliary electrode reading (mV)</param>
        /// <param name="o3W_mV">O3 working electrode reading (mV)</param>
        /// <param name="no2A_mV">NO2 auxiliary electrode reading (mV)</param>
        /// <param name="no2W_mV">NO2 working electrode reading (mV)</param>
        /// <param name="temperature_mV">PT temperature sensor reading (mV)</param>
        /// <param name="humidity_pc">Relative humidity percent</param>
        /// <param name="pressure_millibar">Pressure (mbar)</param>
        /// <param name="temp_C">Temperature (degrees Celsius)</param>
        /// <returns>An FeaturesInput with inputs created from the given sensor
        ///     measurements.</returns>
        public static FeaturesInput ExtractFeaturesInput(double coA_mV, double coW_mV,
            double o3A_mV, double o3W_mV, double no2A_mV, double no2W_mV, double temperature_mV,
            double humidity_pc, double pressure_millibar, double temp_C)
        {
            var no2 = no2A_mV - no2W_mV;
            var o3 = o3A_mV - o3W_mV;
            var co = coA_mV - coW_mV;
            // multiply humidity_pc by 100 to get percent from 0 to 100
            // (not converted decimal percent version from 0 to 1)
            var absoluteHumidity = CalculateAbsHumidity(humidity_pc * 100,
                temp_C + CELSUIS_TO_KELVIN_CONSTANT);

            return new FeaturesInput(no2, o3, co, temp_C, absoluteHumidity, pressure_millibar);
        }

        /// <summary>
        /// Calculate the absolute humidity mathematically.
        /// </summary>
        /// <param name="relHumidity">Relative humidity percent</param>
        /// <param name="temperatureKelvin">Temperature (degrees kelvin)</param>
        /// <returns>A double representing the absolute humidity.</returns>
        private static double CalculateAbsHumidity(double relHumidity, double temperatureKelvin)
        {
            return relHumidity / 100 * Math.Exp(54.842763 - 6763.22 / temperatureKelvin - 4.210 *
                Math.Log(temperatureKelvin) + 0.000367 * temperatureKelvin + Math.Tanh(0.0415 *
                (temperatureKelvin - 218.8)) * (53.878 - 1331.22 / temperatureKelvin - 9.44523 *
                Math.Log(temperatureKelvin) + 0.014025 * temperatureKelvin)) / 1000;
        }
    }

    /// <summary>
    /// The inputs and outputs for a neural network conversion.
    /// </summary>
    public class ConversionInputOutput
    {
        public FeaturesInput Input { get; set; }
        public GasReading Output { get; set; }

        /// <summary>
        /// Initializes the input and output to the given objects.
        /// </summary>
        /// <param name="input">The input to the neural network</param>
        /// <param name="output">The output of the neural network</param>
        public ConversionInputOutput(FeaturesInput input, GasReading output)
        {
            Input = input;
            Output = output;
        }
    }

    /// <summary>
    /// A neural network that has activation functions, weights, and biases and is run using
    /// matrices. 
    /// </summary>
    public class NeuralNetwork
    {
        List<string> _activationFunctions;
        List<List<List<double>>> _weights;
        List<List<double>> _biases;
        Matrix<double>[] _weightsMatrices;
        Matrix<double>[] _biasesMatrices;
        bool _hasInitializedMatrices;

        /// <summary>
        /// Initializes the activation functions, weights, and biases.
        /// </summary>
        /// <param name="activationFunctions">A list of strings containing the activation
        ///     function for each layer starting with the first layer.</param>
        /// <param name="weights">A list of lists of lists of doubles containing the weights
        ///     for each node of each layer starting with the first layer.</param>
        /// <param name="biases">A list of lists of doubles containing the biases for each node
        ///     of each layer starting with the first layer.</param>
        public NeuralNetwork(List<string> activationFunctions, List<List<List<double>>> weights,
                             List<List<double>> biases)
        {
            _activationFunctions = activationFunctions;
            _weights = weights;
            _biases = biases;
            _weightsMatrices = new Matrix<double>[weights.Count()];
            _biasesMatrices = new Matrix<double>[biases.Count()];
            _hasInitializedMatrices = false;
        }

        /// <summary>
        /// Runs the neural network with the specified inputs.
        /// 
        /// Note: in order to understand what is output by the neural network, you must understand
        /// the models that were serialized into JSON and then deserialized in the appropriate
        /// conversion functions neural network class.
        /// </summary>
        /// <param name="inputFeatureMat">A matrix of doubles that contains the inputs for the
        ///     neural network</param>
        /// <returns>A list of doubles representing the output of the neural network.</returns>
        public List<double> Convert(Matrix<double> inputFeatureMat)
        {
            if (!_hasInitializedMatrices)
            {
                InitializeWeightsAndBiases();
            }

            // start with 'output' variable as the input vector
            Matrix<double> output = inputFeatureMat;
            var i = 0;
            int numWeightsBiasesMatrices = _weights.Count();

            while (i < numWeightsBiasesMatrices)
            {
                // calculate the input to this layer using the appropriate weights and biases
                output = output.Multiply(_weightsMatrices[i]).Add(_biasesMatrices[i]);

                var actFunction = _activationFunctions[i];
                output = ApplyActivationFunction(output, actFunction);

                i++;
            }

            return output.ToRowArrays()[0].ToList();
        }

        /// <summary>
        /// Apply the specified activation function on the input.
        /// </summary>
        /// <param name="input">The input to the layer of nodes</param>
        /// <param name="func">The activation function for this layer of nodes</param>
        /// <returns>A matrix of doubles representing the output of the layer of nodes.</returns>
        private Matrix<double> ApplyActivationFunction(Matrix<double> input, string func)
        {
            if (func.Equals("relu"))
            {
                input.Map((x) => Math.Max(0, x), input, Zeros.AllowSkip);
            }

            return input;
        }

        /// <summary>
        /// Create matrices for the weights and biases for each layer of the neural network.
        /// </summary>
        private void InitializeWeightsAndBiases()
        {
            var i = 0;

            while (i < _weightsMatrices.Length && i < _biasesMatrices.Length)
            {
                var biasesArr = _biases[i].ToArray();
                _biasesMatrices[i] = Matrix<double>.Build.DenseOfRowArrays(biasesArr);
                var weightsArr = _weights[i].Select(w => w.ToArray()).ToArray();
                _weightsMatrices[i] = Matrix<double>.Build.DenseOfRowArrays(weightsArr);

                i++;
            }

            _hasInitializedMatrices = true;
        }
    }
}
