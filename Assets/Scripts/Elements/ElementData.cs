using Unity.Netcode;
using System;

public class ElementData : INetworkSerializable, IEquatable<ElementData>
{
    public ElementType Type;
    public ElementState State;
    
    public ElementData(ElementType type, ElementState state)
    {
        Type = type;
        State = state;
    }

    public ElementData(ElementData other)
    {
        Type = other.Type;
        State = other.State;
    }

    public ElementData()
    {
        Type = ElementType.None;
        State = ElementState.Raw;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Type);
        serializer.SerializeValue(ref State);
    }

    public bool Equals(ElementData other)
    {
        return Type == other.Type && State == other.State;
    }
}