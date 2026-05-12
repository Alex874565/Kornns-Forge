using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingStationUI : MonoBehaviour
{
    private CraftingStationController station;
    private PlayerStatusController player;
    private PlayerInputController input;

    [Header("Buttons")]
    [SerializeField] private List<Button> materialButtons;
    [SerializeField] private Button resultButton;
    [SerializeField] private Button craftButton;
    [SerializeField] private Button closeButton;

    public void Show(CraftingStationController s, PlayerStatusController p)
    {
        Unsubscribe();

        station = s;
        player = p;
        input = player != null
            ? player.GetComponent<PlayerInputController>()
            : null;

        if (station != null)
            station.OnCraftingChanged += Refresh;

        if (input != null)
            input.OnInteract += Hide;

        SetupButtons();

        gameObject.SetActive(true);

        Refresh();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        Unsubscribe();
    }

    private void Unsubscribe()
    {
        if (station != null)
            station.OnCraftingChanged -= Refresh;

        if (input != null)
            input.OnInteract -= Hide;
    }

    // ---------------- SETUP ----------------

    private void SetupButtons()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Hide);
        }

        for (int i = 0; i < materialButtons.Count; i++)
        {
            int index = i;
            Button button = materialButtons[index];

            if (button == null) continue;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                if (station != null && player != null)
                    station.RequestToggleIngredientSlot(index, player);
            });

            AddHover(
                button,
                () => HoverMaterial(index),
                () => ClearText(button)
            );
        }

        if (craftButton != null)
        {
            craftButton.onClick.RemoveAllListeners();
            craftButton.onClick.AddListener(() =>
            {
                if (station != null && player != null)
                    station.RequestCraft(player);
            });
        }

        if (resultButton != null)
        {
            resultButton.onClick.RemoveAllListeners();
            resultButton.onClick.AddListener(() =>
            {
                if (station != null && player != null)
                    station.RequestTakeCraftedResult(player);
            });

            AddHover(
                resultButton,
                HoverResult,
                () => ClearText(resultButton)
            );
        }
    }

    // ---------------- REFRESH ----------------

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
            Button button = materialButtons[i];
            if (button == null) continue;

            Image image = button.GetComponent<Image>();
            IngredientSO ingredientSO = station.GetIngredient(i);

            if (image == null) continue;

            if (ingredientSO != null)
            {
                image.sprite = ingredientSO.sprite;
                image.color = Color.white;
            }
            else
            {
                image.sprite = null;
            }

            ClearText(button);
        }
    }

    private void RefreshResult()
    {
        if (resultButton == null) return;

        Image image = resultButton.GetComponent<Image>();
        if (image == null) return;

        OrderData craftedOrder = station.GetCraftedOrder();
        OrderData previewOrder = station.GetOrderPreview();

        if (craftedOrder != null)
        {
            image.sprite = craftedOrder.sprite;
            image.color = Color.white;
            resultButton.interactable = true;
        }
        else if (previewOrder != null)
        {
            image.sprite = previewOrder.sprite;
            image.color = Color.white;
            resultButton.interactable = false;
        }
        else
        {
            image.sprite = null;
            image.color = new Color(1f, 1f, 1f, 0f);
            resultButton.interactable = false;
        }

        ClearText(resultButton);
    }

    private void RefreshCraft()
    {
        if (craftButton == null) return;

        craftButton.interactable =
            station.HasPreview() &&
            !station.HasCrafted();
    }

    // ---------------- HOVER ----------------

    private void HoverMaterial(int index)
    {
        if (station == null || player == null) return;

        TextMeshProUGUI text =
            materialButtons[index].GetComponentInChildren<TextMeshProUGUI>();

        if (text == null) return;

        IngredientSO ingredientSO = station.GetIngredient(index);

        if (ingredientSO == null && player.HasIngredientNetworked())
            text.text = "Add";
        else if (ingredientSO != null && !player.IsHoldingSomethingNetworked())
            text.text = "Remove";
        else
            text.text = "";
    }

    private void HoverResult()
    {
        if (station == null || player == null || resultButton == null) return;

        TextMeshProUGUI text =
            resultButton.GetComponentInChildren<TextMeshProUGUI>();

        if (text == null) return;

        if (station.HasCrafted() && !player.IsHoldingSomethingNetworked())
            text.text = "Take";
        else
            text.text = "";
    }

    private void ClearText(Button button)
    {
        if (button == null) return;

        TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>();

        if (text != null)
            text.text = "";
    }

    private void AddHover(Button button, System.Action enter, System.Action exit)
    {
        if (button == null) return;

        UIHoverHandler hover = button.GetComponent<UIHoverHandler>();

        if (hover == null)
            hover = button.gameObject.AddComponent<UIHoverHandler>();

        hover.Clear();

        hover.OnHoverEnter += enter;
        hover.OnHoverExit += exit;
    }

    private void OnDisable()
    {
        Unsubscribe();
    }
}