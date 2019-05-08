using System;
using System.ComponentModel;
using NodeLibrary;
using Receiver.ViewModels;

namespace Receiver
{
    public enum ConversionType
    {
        none,
        alphasense,
        // DM: add neural network and multi-sensor
        sharad,
        neural,
        multisensor
        // DM: end
    }

    public interface IAppProperties: INotifyPropertyChanged
    {
        ConversionType ConversionType { get; set; }
        string AlphasenseJson { get; set; }
        string SharadJson { get; set; }
        // DM: add neural network and multi-sensor
        string NeuralNetworkJson { get; set; }
        string MultiSensorJson { get; set; }
        // DM: end
        bool IsAlphasenseJsonValid(string json);
        bool IsSharadJsonValid(string json);
        // DM: add neural network and multi-sensor
        bool IsNeuralNetworkJsonValid(string json);
        bool IsMultiSensorJsonValid(string json);
        // DM: end
        IConversionFunctions Conversion { get; set; }

        event EventHandler<MetaSenseMessage> MessageReceived;
        event EventHandler<MetaSenseNodeViewModel> NodeSelectedChanged;
        MetaSenseMessage LastMessageReceived { get; }
        bool Connected { get; }
        MetaSenseNodeViewModel NodeViewModel { get; set; }
        MetaSenseNode Node { get; }
        DateTime? LastMessageReceivedAt { get; }
        DateTime? LastTimestampReceived { get; }
        void ConnectSelectedNode();
        void DiconnectSelectedNode();
    }
}