using UnityEngine;
using System;
using Unity.Netcode;

[Serializable]
public class StationElementData : INetworkSerializable, IEquatable<StationElementData>
{
    public MaterialType Type;
    public int ProcessingTime;
    public MaterialState AcceptedState;
    public MaterialState FinishedState;

    public StationElementData()
    {
        Type = MaterialType.None;
    }
    
    public StationElementData(MaterialType type, int processingTime, MaterialState acceptedState, MaterialState finishedState)
    {
        Type = type;
        ProcessingTime = processingTime;
        AcceptedState = acceptedState;
        FinishedState = finishedState;
    }

    public StationElementData(StationElementData stationElementData)
    {
        Type = stationElementData.Type;
        ProcessingTime = stationElementData.ProcessingTime;
        AcceptedState = stationElementData.AcceptedState;
        FinishedState = stationElementData.FinishedState;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Type);
        serializer.SerializeValue(ref ProcessingTime);
        serializer.SerializeValue(ref AcceptedState);
        serializer.SerializeValue(ref FinishedState);
    }

    public bool Equals(StationElementData other)
    {
        return Type == other.Type &&
               ProcessingTime == other.ProcessingTime &&
               AcceptedState == other.AcceptedState &&
               FinishedState == other.FinishedState;
    }
}