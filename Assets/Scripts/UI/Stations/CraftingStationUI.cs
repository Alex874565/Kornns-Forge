using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingStationUI : MonoBehaviour
{
    private CraftingStationController station;
    private PlayerStatusController player;
    private PlayerInputController input;

    [SerializeField] private List<Button> materialButtons;
    [SerializeField] private Button resultButton;
    [SerializeField] private Button craftButton;
    [SerializeField] private Button closeButton;
    
    public void Show(CraftingStationController s, PlayerStatusController p)
    {
        if (station != null)
            station.OnCraftingChanged -= Refresh;

        station = s;
        player = p;
        input = p.gameObject.GetComponent<PlayerInputController>();

        station.OnCraftingChanged += Refresh;

        SetupButtons();
        Refresh();

        gameObject.SetActive(true);

        if (input != null)
            input.OnInteract += Hide;
    }
    
    public void Hide()
    {
        gameObject.SetActive(false);
        if(input != null)
            input.OnInteract -= Hide;
    }
    
    private void SetupButtons()
    {
        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(Hide);
        
        for (int i = 0; i < materialButtons.Count; i++)
        {
            int index = i;
            Button button = materialButtons[index];

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                station.RequestToggleIngredientSlot(index, player);
            });

            AddHover(
                button,
                () => HoverMaterial(index),
                () => ClearText(button)
            );
        }

        craftButton.onClick.RemoveAllListeners();
        craftButton.onClick.AddListener(() =>
        {
            station.RequestCraft(player);
        });
        resultButton.onClick.RemoveAllListeners();
        resultButton.onClick.AddListener(() =>
        {
            station.RequestTakeCraftedResult(player);
        });

        AddHover(
            resultButton,
            HoverResult,
            () => ClearText(resultButton)
        );
    }

    private void HoverMaterial(int index)
    {
        TextMeshProUGUI text = materialButtons[index].GetComponentInChildren<TextMeshProUGUI>();
        if (text == null) return;

        Ingredient ing = station.GetIngredient(index);

        if (ing == null && player.HasIngredient())
            text.text = "Add";
        else if (ing != null && !player.HasIngredient())
            text.text = "Remove";
        else
            text.text = "";
    }

    private void HoverResult()
    {
        TextMeshProUGUI text = resultButton.GetComponentInChildren<TextMeshProUGUI>();
        if (text == null) return;

        if (station.HasCrafted() && !player.IsHoldingSomething())
            text.text = "Take";
        else
            text.text = "";
    }

    private void ClearText(Button button)
    {
        TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
            text.text = "";
    }

    private void AddHover(Button button, System.Action enter, System.Action exit)
    {
        UIHoverHandler hover = button.GetComponent<UIHoverHandler>();

        if (hover == null)
            hover = button.gameObject.AddComponent<UIHoverHandler>();

        hover.OnHoverEnter += enter;
        hover.OnHoverExit += exit;
    }

    private void Refresh()
    {
        if (station == null) return;

        RefreshSlots();
        RefreshResult();
        RefreshCraft();
    }

    private void RefreshSlots()
    {
        for (int i = 0; i < materialButtons.Count; i++)
        {
            Image image = materialButtons[i].GetComponent<Image>();
            Ingredient ing = station.GetIngredient(i);

            if (ing != null)
            {
                image.sprite = ing.GetIngredientSO().sprite;
            }
            else
            {
                image.sprite = null; // or keep a default sprite (see below)
            }

            image.color = Color.white; // ✅ always visible

            ClearText(materialButtons[i]);
        }
    }

    private void RefreshResult()
    {
        Image image = resultButton.GetComponent<Image>();

        if (station.HasCrafted())
        {
            image.sprite = station.CraftedOrder.sprite;
            resultButton.interactable = true;
        }
        else if (station.HasPreview())
        {
            image.sprite = station.OrderPreview.sprite;
            image.color = Color.white;
            resultButton.interactable = false;
        }
        else
        {
            image.sprite = null;
            image.color = new Color(1, 1, 1, 0);
            resultButton.interactable = false;
        }

        ClearText(resultButton);
    }

    private void RefreshCraft()
    {
        craftButton.interactable =
            station.HasPreview() &&
            !station.HasCrafted();
    }
    
    public void Open(CraftingStationController station, PlayerStatusController player)
    {
        Show(station, player);
    }

    private void OnDisable()
    {
        if (station != null)
            station.OnCraftingChanged -= Refresh;
        
        if(input != null)
            input.OnInteract -= Hide;
    }
}