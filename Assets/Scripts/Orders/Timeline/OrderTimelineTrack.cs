using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackClipType(typeof(OrderTimelineClip))]
[TrackBindingType(typeof(OrderTimelineReceiver))]
public class OrderTimelineTrack : TrackAsset
{
    public override Playable CreateTrackMixer(
        PlayableGraph graph,
        GameObject go,
        int inputCount)
    {
        return ScriptPlayable<OrderTimelineMixer>.Create(graph, inputCount);
    }
}