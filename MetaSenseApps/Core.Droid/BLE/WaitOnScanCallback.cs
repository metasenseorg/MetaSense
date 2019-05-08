using System.Collections.Generic;
using Android.Bluetooth.LE;
using Android.Runtime;

namespace Core.Droid.BLE
{
    internal class WaitOnScanCallback : ScanCallback
    {
        public delegate void BatchScanResultsDelegate(IList<ScanResult> results);
        public BatchScanResultsDelegate HandlerBatchScanResults;
        public override void OnBatchScanResults(IList<ScanResult> results)
        {
            base.OnBatchScanResults(results);
            HandlerBatchScanResults?.Invoke(results);
        }

        public delegate void ScanResultDelegate([GeneratedEnum] ScanCallbackType callbackType, ScanResult result);
        public ScanResultDelegate HandlerScanResult;
        public override void OnScanResult([GeneratedEnum] ScanCallbackType callbackType, ScanResult result)
        {
            base.OnScanResult(callbackType, result);
            HandlerScanResult?.Invoke(callbackType, result);
        }
        public delegate void ScanFailedDelegate([GeneratedEnum] ScanFailure errorCode);
        public ScanFailedDelegate HandlerScanFailed;
        public override void OnScanFailed([GeneratedEnum] ScanFailure errorCode)
        {
            base.OnScanFailed(errorCode);
            HandlerScanFailed?.Invoke(errorCode);
        }
    }

}