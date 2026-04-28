using UnityEngine;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CraftingStationUI : MonoBehaviour
{
    private CraftingStationController currentCraftingStation;
    private PlayerStatusController currentPlayer;
    
    [FormerlySerializedAs("iconsDatabase")] [SerializeField] private MaterialsDatabase materialsDatabase;
    [SerializeField] private List<Button> materialsButtons;
    [SerializeField] private List<Image> resultIcon;
    
    private event Action OnCraft;

    private void Start()
    {
        
    }
    
    public void Show(CraftingStationController craftingStation, PlayerStatusController player)
    {
        Initialize(craftingStation, player);
        gameObject.SetActive(true);
    }

    private void Initialize(CraftingStationController craftingStation, PlayerStatusController player)
    {
        currentCraftingStation = craftingStation;
        currentPlayer = player;

        foreach (Button button in materialsButtons)
        {
            button.GetComponent<Image>().sprite = materialsDatabase.GetMaterialIcon(currentPlayer.HeldElement.Value.Type);
        }
    }

    private void MaterialHover()
    {
        if(currentPlayer.HeldElement.Value.Type == MaterialType.None) return;
        
        
    }
}