public interface IReceiveElement
{
    void ReceiveElement(MaterialData material);
    
    bool CanReceiveElement(MaterialData material);
}