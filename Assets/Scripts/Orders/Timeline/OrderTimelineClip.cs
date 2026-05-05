using UnityEngine;
using UnityEngine.Playables;

[System.Serializable]
public class OrderTimelineClip : PlayableAsset
{
    public OrderData order;
    public float orderTimer = 30f;
    public int points = 100;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<OrderTimelineBehaviour>.Create(graph);
        var behaviour = playable.GetBehaviour();

        behaviour.order = order;
        behaviour.orderTimer = orderTimer;
        behaviour.points = points;

        return playable;
    }
}