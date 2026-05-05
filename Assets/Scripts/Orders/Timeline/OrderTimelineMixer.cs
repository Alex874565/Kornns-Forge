using UnityEngine;
using UnityEngine.Playables;

public class OrderTimelineMixer : PlayableBehaviour
{
    public override void ProcessFrame(
        Playable playable,
        FrameData info,
        object playerData)
    {
        OrderTimelineReceiver receiver = playerData as OrderTimelineReceiver;

        if (receiver == null)
            return;

        int inputCount = playable.GetInputCount();

        for (int i = 0; i < inputCount; i++)
        {
            float weight = playable.GetInputWeight(i);

            if (weight <= 0f)
                continue;

            var inputPlayable =
                (ScriptPlayable<OrderTimelineBehaviour>)playable.GetInput(i);

            OrderTimelineBehaviour behaviour = inputPlayable.GetBehaviour();

            if (behaviour.hasSpawned)
                continue;

            behaviour.hasSpawned = true;

            receiver.SpawnOrder(
                behaviour.order,
                behaviour.orderTimer,
                behaviour.points
            );
        }
    }
}