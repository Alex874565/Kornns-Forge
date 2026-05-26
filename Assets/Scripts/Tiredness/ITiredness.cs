using UnityEngine;

public interface ITiredness
{
   float GetTired(float energy_points);
   float GetEnegy(float energy_points);
}

public abstract class Tiredness : MonoBehaviour, ITiredness
{
   [SerializeField] protected PlayerStatusController playerStatusController;
   [SerializeField] protected float energy = 10f;
   [SerializeField] protected bool consume = true;

   public abstract void ConsumeEnergy(float amount);
   public abstract void RechargeEnergy(float amount);

   protected void ApplyConsume(float amount)
   {
      if (playerStatusController == null) return;
      playerStatusController.GetTired(amount);
   }

   protected void ApplyRecharge(float amount)
   {
      if (playerStatusController == null) return;
      playerStatusController.GetEnergy(amount);
   }

   // Interface compatibility for existing station classes that expect float-returning methods
   public float GetTired(float energy_points)
   {
      ApplyConsume(energy_points);
      return playerStatusController != null ? playerStatusController.GetEnergyLevel() : -1f;
   }

   public float GetEnegy(float energy_points)
   {
      ApplyRecharge(energy_points);
      return playerStatusController != null ? playerStatusController.GetEnergyLevel() : -1f;
   }
}
