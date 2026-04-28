using UnityEngine;

public class BedStation : ITiredness
{
    [SerializeField] Sprite sprite;
    [SerializeField] float recharge_energy;

    public float GetEnegy(float energy_points)
    {
        return recharge_energy;
    }

    public float GetTired(float energy_points)
    {
        return 0;
    }
}
