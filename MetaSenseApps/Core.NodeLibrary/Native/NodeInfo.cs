using System;
using System.Threading.Tasks;

namespace NodeLibrary.Native
{
    public abstract class NodeInfo
    {
        public event EventHandler Updated;
        protected void OnUpdated()
        {
            Updated?.Invoke(this, null);
        }
        public string SensorId { get; protected set; }
        public string AfeSerial { get; protected set; }
        public object Id { get; }
        public NodeTransport Transport { get; protected set; }
        public string Name { get; protected set; }
        public string MacAddress { get; protected set; }
        public double? SignalStrength { get; protected set; }
        public bool? Paired { get; protected set; }

        protected NodeInfo(object id, string name, string mac, NodeTransport transport)
        {
            Id = id;
            Transport = transport;
            Name = name;
            MacAddress = mac;
        }
        public bool Update(NodeInfo node)
        {
            if (node == null) return false;
            var ret = false;
            if (node.SensorId != null && !node.SensorId.Equals(SensorId))
            {
                SensorId = node.SensorId;
                ret = true;
            }
            if (node.AfeSerial == null || node.AfeSerial.Equals(AfeSerial)) return ret;
            AfeSerial = node.AfeSerial;
            return true;
        }
        public override int GetHashCode()
        {
            return Id?.GetHashCode() ?? 0;
        }
        public override bool Equals(object obj)
        {
            var info = obj as NodeInfo;
            return info != null && Id.Equals(info.Id);
        }

        public abstract Task<MetaSenseNode> NodeFactory();
    }
}
