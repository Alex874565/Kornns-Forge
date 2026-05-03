using UnityEngine;

public interface IThrowable
{
    void ThrowSelf(Vector2 direction, float force, float angle);
}