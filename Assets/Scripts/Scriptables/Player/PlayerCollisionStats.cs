using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Player/CollisionStats", fileName = "CollisionStats")]
public class PlayerCollisionStats : ScriptableObject
{
    [field: SerializeField] public LayerMask GroundLayer { get; private set; }
    [field: SerializeField] public float GroundCheckDistance { get; private set; } = .25f;
    [field: SerializeField] public float HeadCheckDistance { get; private set; }
    [field: SerializeField] public float HeadSize { get; private set; } = .75f;

}