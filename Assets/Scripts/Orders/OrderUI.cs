using System.Text;
using System.Linq;
using TMPro;
using UnityEngine;

public class OrderUI : MonoBehaviour
{
    public TextMeshProUGUI orderNameText;
    public TextMeshProUGUI stepsText;
    public TextMeshProUGUI itemsText;

    public void Setup(OrderProgress progress)
    {
        OrderData order = progress.order;
        orderNameText.text = order.orderName;

        StringBuilder stepsBuilder = new StringBuilder();

        foreach (var requiredStep in order.requiredSteps)
        {
            bool stepCompleted = progress.completedSteps.Contains(requiredStep);

            if (stepCompleted)
            {
                stepsBuilder.Append("<color=#59a84b>");
                stepsBuilder.Append(requiredStep.ToString());
                stepsBuilder.Append("</color>");
            }
            else
            {
                stepsBuilder.Append("<color=#999999>");
                stepsBuilder.Append(requiredStep.ToString());
                stepsBuilder.Append("</color>");
            }

            if (requiredStep != order.requiredSteps.Last())
                stepsBuilder.AppendLine();
        }

        stepsText.text = stepsBuilder.ToString();

        bool stepsCompleted = progress.HasRequiredSteps();

        StringBuilder itemsBuilder = new StringBuilder();
        foreach (var req in order.requirements)
        {
            int collected = progress.collectedItems
                .Where(item => item.itemType == req.itemType &&
                              item.materialType == req.materialType)
                .Sum(item => item.quantity);

            bool isCompleted = collected >= req.quantity;

            if (isCompleted)
            {
                itemsBuilder.Append("<color=#59a84b>");
                itemsBuilder.Append(req.quantity + "x " + req.materialType + " " + req.itemType);
                itemsBuilder.Append("</color>");
            }
            else
            {
                if (!stepsCompleted)
                {
                    itemsBuilder.Append("<color=#666666>");
                    itemsBuilder.Append(req.quantity + "x " + req.materialType + " " + req.itemType);
                    itemsBuilder.Append("</color>");
                }
                else
                {
                    itemsBuilder.Append("<color=#ca5952>");
                    itemsBuilder.Append(collected + "/" + req.quantity + "x " + req.materialType + " " + req.itemType);
                    itemsBuilder.Append("</color>");
                }
            }

            itemsBuilder.AppendLine();
        }
        itemsText.text = itemsBuilder.ToString();
    }
}