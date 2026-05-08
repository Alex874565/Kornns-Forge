using Unity.Netcode;
using System;

public class MaterialData : INetworkSerializable, IEquatable<MaterialData>
{
    public MaterialType Type;
    public MaterialState State;
    
    public MaterialData(MaterialType type, MaterialState state)
    {
        Type = type;
        State = state;
    }

    public MaterialData(MaterialData other)
    {
        Type = other.Type;
        State = other.State;
    }

    public MaterialData()
    {
        Type = MaterialType.None;
        State = MaterialState.Raw;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Type);
        serializer.SerializeValue(ref State);
    }

    public bool Equals(MaterialData other)
    {
        return Type == other.Type && State == other.State;
    }
}