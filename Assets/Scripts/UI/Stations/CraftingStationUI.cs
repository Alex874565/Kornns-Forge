using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingStationUI : MonoBehaviour
{
    private CraftingStationController station;
    private PlayerStatusController player;

    [SerializeField] private List<Button> materialButtons;
    [SerializeField] private Button resultButton;
    [SerializeField] private Button craftButton;

    public void Show(CraftingStationController s, PlayerStatusController p)
    {
        station = s;
        player = p;

        station.OnCraftingChanged += Refresh;

        SetupButtons();
        Refresh();

        gameObject.SetActive(true);
    }

    private void SetupButtons()
    {
        for (int i = 0; i < materialButtons.Count; i++)
        {
            int index = i;

            materialButtons[i].onClick.RemoveAllListeners();
            materialButtons[i].onClick.AddListener(() =>
            {
                station.ToggleIngredientSlot(index, player);
            });

            AddHover(materialButtons[i],
                () => HoverMaterial(index),
                () => ClearText(materialButtons[i]));
        }

        craftButton.onClick.RemoveAllListeners();
        craftButton.onClick.AddListener(() => station.Craft());

        resultButton.onClick.RemoveAllListeners();
        resultButton.onClick.AddListener(() => station.TakeCraftedResult(player));

        AddHover(resultButton,
            HoverResult,
            () => ClearText(resultButton));
    }

    // ---------------- HOVER ----------------

    private void HoverMaterial(int index)
    {
        var text = materialButtons[index].GetComponentInChildren<TextMeshProUGUI>();

        if (text == null) return;

        var ing = station.GetIngredient(index);

        if (ing == null && player.HasIngredient())
            text.text = "Add";
        else if (ing != null && !player.HasIngredient())
            text.text = "Remove";
        else
            text.text = "";
    }

    private void HoverResult()
    {
        var text = resultButton.GetComponentInChildren<TextMeshProUGUI>();

        if (text == null) return;

        if (station.HasCrafted() && !player.HasIngredient())
            text.text = "Take";
        else
            text.text = "";
    }

    private void ClearText(Button b)
    {
        var text = b.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null) text.text = "";
    }

    private void AddHover(Button b, System.Action enter, System.Action exit)
    {
        var h = b.GetComponent<UIHoverHandler>();
        if (h == null) h = b.gameObject.AddComponent<UIHoverHandler>();

        h.OnHoverEnter += enter;
        h.OnHoverExit += exit;
    }

    // ---------------- UI REFRESH ----------------

    private void Refresh()
    {
        RefreshSlots();
        RefreshResult();
        RefreshCraft();
    }

    private void RefreshSlots()
    {
        for (int i = 0; i < materialButtons.Count; i++)
        {
            var img = materialButtons[i].GetComponent<Image>();
            var ing = station.GetIngredient(i);

            if (ing != null)
            {
                img.sprite = ing.GetIngredientSO().sprite;
                img.color = Color.white;
            }
            else
            {
                img.sprite = null;
                img.color = new Color(1, 1, 1, 0);
            }

            ClearText(materialButtons[i]);
        }
    }

    private void RefreshResult()
    {
        var img = resultButton.GetComponent<Image>();

        if (station.HasCrafted())
        {
            img.sprite = station.CraftedOrder.resultIcon;
            img.color = Color.white;
            resultButton.interactable = true;
        }
        else if (station.HasPreview())
        {
            img.sprite = station.CurrentOrderPreview.resultIcon;
            img.color = Color.white;
            resultButton.interactable = false;
        }
        else
        {
            img.sprite = null;
            img.color = new Color(1, 1, 1, 0);
            resultButton.interactable = false;
        }

        ClearText(resultButton);
    }

    private void RefreshCraft()
    {
        craftButton.interactable =
            station.HasPreview() && !station.HasCrafted();
    }

    private void OnDisable()
    {
        if (station != null)
            station.OnCraftingChanged -= Refresh;
    }
}