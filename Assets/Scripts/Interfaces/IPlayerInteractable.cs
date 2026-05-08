public interface IPlayerInteractable : IHighlightable
{
    //public abstract bool InteractOnlyOnce { get; set; }

    public abstract bool CanInteract(PlayerStatusController playerStatusController);
    
    public abstract void Interact(PlayerStatusController playerStatusController);
}