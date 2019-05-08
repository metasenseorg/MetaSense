using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeLibrary
{
    class ConversionFunctionsNeuralNetTester
    {
        public static void RunTests(double coA_mV, double coW_mV, double o3A_mV, double o3W_mV,
            double no2A_mV, double no2W_mV, double temperature_mV, double humidity_pc,
            double pressure_millibar, double temp_C)
        {
            var totalFailedTests = 0;

            totalFailedTests += RunFeaturesInputTests(coA_mV, coW_mV, o3A_mV, o3W_mV, no2A_mV, no2W_mV, temperature_mV, humidity_pc,
                pressure_millibar, temp_C);

            Log.Trace($"@@@@@@@@@@@@ Failed tests: {totalFailedTests} @@@@@@@@@@@@");
        }

        public static int RunFeaturesInputTests(double coA_mV, double coW_mV, double o3A_mV, double o3W_mV,
            double no2A_mV, double no2W_mV, double temperature_mV, double humidity_pc,
            double pressure_millibar, double temp_C)
        {
            int failedTests = 0;
            StringBuilder sb = new StringBuilder();
            sb.Append(Environment.NewLine);
            sb.Append("############ Testing Input to Neural Network ############");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);

            // ignore temperature_mV since it is unused
            string inputManualTest =
                    "============ MANUAL TEST: Check that all inputs are valid ============" + Environment.NewLine +
                    $"NO2 auxiliary: {no2A_mV} mV" + Environment.NewLine +
                    $"NO2 working: {no2W_mV} mV" + Environment.NewLine +
                    $"CO auxiliary: {coA_mV} mV" + Environment.NewLine +
                    $"CO working: {coW_mV} mV" + Environment.NewLine +
                    $"O3 auxiliary: {o3A_mV} mV" + Environment.NewLine +
                    $"O3 working: {o3W_mV} mV" + Environment.NewLine +
                    $"Relative humidity: {humidity_pc * 100}%" + Environment.NewLine +
                    $"Temperature: {temp_C} °C" + Environment.NewLine +
                    $"Pressure: {pressure_millibar} mB";
            sb.Append(inputManualTest);

            string extractedFeaturesTest =
                    "============ AUTO TEST: Checks that all extracted input features are valid ============" + Environment.NewLine;

            var expectedNO2Diff = no2A_mV - no2W_mV;
            var expectedO3Diff = o3A_mV - o3W_mV;
            var expectedCODiff = coA_mV - coW_mV;
            var expectedTemp = temp_C;
            var expectedAbsHum = CalculateAbsHumidity(humidity_pc * 100, temp_C + 273.15);
            var expectedPres = pressure_millibar;

            var extractedFeatures = FeaturesInput.ExtractFeaturesInput(coA_mV, coW_mV, o3A_mV, o3W_mV, no2A_mV, no2W_mV, temperature_mV, humidity_pc,
                pressure_millibar, temp_C);

            string no2Result;
            if (extractedFeatures.NO2 == expectedNO2Diff) {
                no2Result = "~~~~ Passed NO2 voltage difference ~~~~";
            }
            else {
                no2Result = $"---- FAILED NO2 voltage difference ----" + Environment.NewLine +
                            $"Expected: {expectedNO2Diff} | Actual: {extractedFeatures.NO2}";
                failedTests++;
            }

            string o3Result;
            if (extractedFeatures.O3 == expectedO3Diff)
            {
                o3Result = "~~~~ Passed O3 voltage difference ~~~~";
            }
            else
            {
                o3Result = $"---- FAILED O3 voltage difference ----" + Environment.NewLine +
                            $"Expected: {expectedO3Diff} | Actual: {extractedFeatures.O3}";
                failedTests++;
            }

            string coResult;
            if (extractedFeatures.CO == expectedCODiff)
            {
                coResult = "~~~~ Passed CO voltage difference ~~~~";
            }
            else
            {
                coResult = $"---- FAILED CO voltage difference ----" + Environment.NewLine +
                            $"Expected: {expectedCODiff} | Actual: {extractedFeatures.CO}";
                failedTests++;
            }

            string tempResult;
            if (extractedFeatures.Temperature == expectedTemp)
            {
                tempResult = "~~~~ Passed temperature ~~~~";
            }
            else
            {
                tempResult = $"---- FAILED temperature ----" + Environment.NewLine +
                            $"Expected: {expectedTemp} | Actual: {extractedFeatures.Temperature}";
                failedTests++;
            }

            /* NOT A TRUE TEST OF ABSOLUTE HUMIDITY
             * Check that the results match the output of the absolute humidity in:
             * https://github.com/sharadmv/metasense-transfer/blob/master/metasense/data.py#L17 */
            string absHumResult;
            if (extractedFeatures.AbsoluteHumidity == expectedAbsHum)
            {
                absHumResult = "~~~~ Passed absolute humidity ~~~~";
            }
            else
            {
                absHumResult = $"---- FAILED absolute humidity ----" + Environment.NewLine +
                            $"Expected: {expectedAbsHum} | Actual: {extractedFeatures.AbsoluteHumidity}";
                failedTests++;
            }

            string pressureResult;
            if (extractedFeatures.Pressure == expectedPres)
            {
                pressureResult = "~~~~ Passed pressure ~~~~";
            }
            else
            {
                pressureResult = $"---- FAILED pressure ----" + Environment.NewLine +
                            $"Expected: {expectedPres} | Actual: {extractedFeatures.Pressure}";
                failedTests++;
            }

            sb.Append(Environment.NewLine + Environment.NewLine + extractedFeaturesTest + 
                    no2Result + Environment.NewLine + o3Result + Environment.NewLine +
                    coResult + Environment.NewLine + tempResult + Environment.NewLine +
                    absHumResult + Environment.NewLine + pressureResult + Environment.NewLine);
            Log.Trace(sb.ToString());

            return failedTests;
        }

        private static double CalculateAbsHumidity(double relHumidity, double temperatureKelvin)
        {
            return relHumidity / 100 * Math.Exp(54.842763 - 6763.22 / temperatureKelvin - 4.210 *
                Math.Log(temperatureKelvin) + 0.000367 * temperatureKelvin + Math.Tanh(0.0415 *
                (temperatureKelvin - 218.8)) * (53.878 - 1331.22 / temperatureKelvin - 9.44523 *
                Math.Log(temperatureKelvin) + 0.014025 * temperatureKelvin)) / 1000;
        }
    }
}
