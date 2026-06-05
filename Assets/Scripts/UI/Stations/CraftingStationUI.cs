using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CraftingStationUI : MonoBehaviour
{
    private CraftingStationController station;
    private PlayerStatusController player;
    private PlayerInputController input;
    
    [Header("Images")]
    [SerializeField] private List<Image> materialIconImages;
    [SerializeField] private Image resultImage;
    
    [Header("Buttons")]
    [SerializeField] private List<Button> materialButtons;
    [SerializeField] private Button craftButton;
    [SerializeField] private Button closeButton;

    private EventSystem eventSystem;

    private int clickedMaterialIndex = 0;

    public void Show(CraftingStationController s, PlayerStatusController p)
    {
        Unsubscribe();

        station = s;
        player = p;
        input = player != null
            ? player.GetComponent<PlayerInputController>()
            : null;

        gameObject.SetActive(true);

        EnterUIMode();
        SetupButtons();
        Refresh();
    }

    private void EnterUIMode()
    {
        if (eventSystem == null)
        {
            eventSystem = EventSystem.current;
        }

        input.SetUIMode(true, materialButtons[0].gameObject);
    }

    public void Hide()
    {
        if (input != null)
            input.SetUIMode(false);

        gameObject.SetActive(false);
        Unsubscribe();
    }

    private void Subscribe()
    {
        if (station != null)
            station.OnCraftingChanged += Refresh;

        if (input != null)
        {
            input.OnCancel += Hide;
            input.OnInteractAlternateUI += HandleInteractAlternateUI;
            input.OnInputDeviceChanged += HandleInputDeviceChanged;
        }
    }

    private void Unsubscribe()
    {
        if (station != null)
            station.OnCraftingChanged -= Refresh;

        if (input != null)
        {
            input.OnCancel -= Hide;
            input.OnInteractAlternateUI -= HandleInteractAlternateUI;
            input.OnInputDeviceChanged -= HandleInputDeviceChanged;
        }
    }

    private void HandleInputDeviceChanged(PlayerInputController.PlayerInputDeviceType device)
    {
        Refresh();
    }

    // ---------------- SETUP ----------------

    private void SetupButtons()
    {
        clickedMaterialIndex = 0;

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Hide);
        }

        for (int i = 0; i < materialButtons.Count; i++)
        {
            int index = i;
            Button button = materialButtons[index];

            if (button == null)
                continue;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                if (station != null && player != null)
                    station.RequestToggleIngredientSlot(index, player);

                clickedMaterialIndex = index;
            });

            AddHover(
                button,
                () =>
                {
                    if (eventSystem != null)
                        eventSystem.SetSelectedGameObject(button.gameObject);

                    HoverMaterial(index);
                },
                () => ClearText(button)
            );

            AddSelection(button,
                () =>
                {
                    clickedMaterialIndex = index;
                    HoverMaterial(index);
                },
                () => ClearText(button)
            );
        }

        HoverMaterial(0);

        if (craftButton != null)
        {
            craftButton.onClick.RemoveAllListeners();
            craftButton.onClick.AddListener(() =>
            {
                if (station != null && player != null)
                    station.RequestCraft(player);
            });

            AddHover(
                craftButton,
                () =>
                {
                    if (eventSystem != null)
                        eventSystem.SetSelectedGameObject(craftButton.gameObject);

                    HoverCraft();
                },
                () => {}
            );

            AddSelection(
                craftButton,
                HoverCraft,
                () => { }
            );
        }
        Subscribe();
    }

    private void Refresh()
    {
        if (station == null)
            return;

        RefreshSlots();
        RefreshResult();
        RefreshCraft();
        RefreshCloseText();
    }

    private void RefreshSlots()
    {
        for (int i = 0; i < materialButtons.Count; i++)
        {
            if (i >= materialIconImages.Count)
                continue;

            Image iconImage = materialIconImages[i];
            if (iconImage == null)
                continue;

            IngredientSO ingredientSO = station.GetIngredient(i);

            if (ingredientSO != null)
            {
                iconImage.sprite = ingredientSO.sprite;
                iconImage.enabled = true;
            }
            else
            {
                iconImage.sprite = null;
                iconImage.enabled = false;
            }

            ClearText(materialButtons[i]);
        }

        HoverMaterial(clickedMaterialIndex);
    }

    private void RefreshResult()
    {
        if (resultImage == null)
            return;

        Image image = resultImage.GetComponent<Image>();
        if (image == null)
            return;

        OrderData craftedOrder = station.GetCraftedOrder();
        OrderData previewOrder = station.GetOrderPreview();

        if (craftedOrder != null)
        {
            image.sprite = craftedOrder.sprite;
            image.color = Color.white;
        }
        else if (previewOrder != null)
        {
            image.sprite = previewOrder.sprite;
            image.color = Color.white;
        }
        else
        {
            // Keep the configured sprite and only adjust visibility if needed.
            // Avoid setting the sprite to null which removes the prefab image.
            image.color = new Color(1f, 1f, 1f, 0f);
        }

        if (eventSystem != null &&
            eventSystem.currentSelectedGameObject == resultImage.gameObject)
        {
            HoverResult();
        }
    }

    private void RefreshCraft()
    {
        if (craftButton == null)
            return;

        craftButton.interactable =
            station.HasPreview() &&
            !station.HasCrafted();

        if (eventSystem != null &&
            eventSystem.currentSelectedGameObject == craftButton.gameObject)
        {
            HoverCraft();
        }
        
        TextMeshProUGUI text =
            craftButton.GetComponentInChildren<TextMeshProUGUI>();
        string craftPrompt = input != null
            ? input.GetAlternateInteractPrompt()
            : "R";

        text.text = FormatPrompt("Craft", craftPrompt);
    }

    private void RefreshCloseText()
    {
        if (closeButton == null)
            return;

        TextMeshProUGUI closeText =
            closeButton.GetComponentInChildren<TextMeshProUGUI>();

        if (closeText == null)
            return;

        string cancelPrompt = input != null
            ? input.GetCancelPrompt()
            : "Esc";

        closeText.text = FormatPrompt("Close", cancelPrompt);
    }

    private void HandleInteractAlternateUI()
    {
        if (station != null && player != null)
            station.RequestCraft(player);
    }

    private void HoverMaterial(int index)
    {
        if (station == null || player == null)
            return;

        if (index < 0 || index >= materialButtons.Count)
            return;

        Button button = materialButtons[index];
        if (button == null)
            return;

        TextMeshProUGUI text =
            button.GetComponentInChildren<TextMeshProUGUI>();

        if (text == null)
            return;

        IngredientSO ingredientSO = station.GetIngredient(index);

        string interactPrompt = input != null
            ? input.GetInteractPrompt()
            : "E";

        if (ingredientSO == null && player.HasIngredient())
            text.text = FormatPrompt("Add", interactPrompt);
        else if (ingredientSO != null && !player.IsHoldingSomething())
            text.text = FormatPrompt("Remove", interactPrompt);
        else
            text.text = "";

        UpdateNavigation(index);
    }

    private void HoverCraft()
    {
        if (craftButton == null)
            return;

        TextMeshProUGUI text =
            craftButton.GetComponentInChildren<TextMeshProUGUI>();

        if (text == null)
            return;
    }

    private void HoverResult()
    {
        if (station == null || player == null || resultImage == null)
            return;

        TextMeshProUGUI text =
            resultImage.GetComponentInChildren<TextMeshProUGUI>();

        if (text == null)
            return;

        string interactPrompt = input != null
            ? input.GetInteractPrompt()
            : "E";

        if (station.HasCrafted() && !player.IsHoldingSomething())
            text.text = FormatPrompt("Take", interactPrompt);
        else
            text.text = "";
    }

    private string FormatPrompt(string text, string prompt)
    {
        if (input != null && input.IsUsingMouse())
            return text;

        if (string.IsNullOrEmpty(prompt))
            return text;

        return $"{text} ({prompt})";
    }

    private void UpdateNavigation(int index)
    {
        if (index < 0 || index >= materialButtons.Count)
            return;

        Button selectedMaterialButton = materialButtons[index];

        if (selectedMaterialButton == null)
            return;

        if (craftButton != null)
        {
            Navigation craftNav = craftButton.navigation;
            craftNav.mode = Navigation.Mode.Explicit;
            craftNav.selectOnUp = selectedMaterialButton;
            craftButton.navigation = craftNav;
        }

        if (closeButton != null)
        {
            Navigation closeNav = closeButton.navigation;
            closeNav.mode = Navigation.Mode.Explicit;
            closeNav.selectOnDown = selectedMaterialButton;
            closeButton.navigation = closeNav;
        }
    }

    private void ClearText(Button button)
    {
        if (button == null)
            return;

        if (button == closeButton)
            return;

        TextMeshProUGUI text =
            button.GetComponentInChildren<TextMeshProUGUI>();

        if (text != null)
            text.text = "";
    }

    private void AddHover(Button button, System.Action enter, System.Action exit)
    {
        if (button == null)
            return;

        UIButtonHandler btnHandler = button.GetComponent<UIButtonHandler>();

        if (btnHandler == null)
            btnHandler = button.gameObject.AddComponent<UIButtonHandler>();

        btnHandler.ClearHover();

        btnHandler.OnHoverEnter += enter;
        btnHandler.OnHoverExit += exit;
    }

    private void AddSelection(Button button, System.Action select, System.Action deselect)
    {
        if (button == null)
            return;

        UIButtonHandler btnHandler = button.GetComponent<UIButtonHandler>();

        if (btnHandler == null)
            btnHandler = button.gameObject.AddComponent<UIButtonHandler>();

        btnHandler.ClearSelection();

        btnHandler.OnSelectAction += select;
        btnHandler.OnDeselectAction += deselect;
    }

    private void OnDisable()
    {
        Unsubscribe();
    }
}