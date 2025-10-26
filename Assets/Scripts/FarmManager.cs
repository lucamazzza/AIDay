using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class FarmManager : MonoBehaviour
{
    public GameObject[] FarmPlots;
    public CropData[] Crops;
    public CraftingRecipe[] recipes;
    public Bakery bakery;
    public Dictionary<string, int> Inventory = new Dictionary<string, int>();
    public static FarmManager Instance { get; private set; }
    
    private string[] PlotStates;
    [SerializeField] private TMP_Text CarrotAmountText;
    [SerializeField] private TMP_Text CornAmountText;
    [SerializeField] private TMP_Text PumpkinAmountText;
    [SerializeField] private TMP_Text WheatAmountText;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        PlotStates = new string[FarmPlots.Length];
        for (int i = 0; i < PlotStates.Length; i++)
        {
            PlotStates[i] = string.Empty;
        }
        UpdateInventoryUI();
    }

    public void PlantCrop(string cropName, int plotId)
    {
        int arrayIndex = plotId;
        if (!PlotExists(plotId)) return;
        CropBehaviour targetCropBehaviour = FarmPlots[arrayIndex].GetComponent<CropBehaviour>();
        if (targetCropBehaviour == null)
        {
            Debug.LogError($"FATALE: Componente CropBehaviour mancante sul plot {plotId}.");
            return;
        }
        CropData newCropData = null;
        switch (cropName.ToLower())
        {
            case "wheat":
                newCropData = Crops[4]; 
                break;
            case "corn":
                newCropData = Crops[2];
                break;
            case "carrot":
                newCropData = Crops[1];
                break;
            case "pumpkin":
                newCropData = Crops[3];
                break;
            default:
                Debug.LogError($"Error: Crop '{cropName}' is not recognized.");
                return; 
        }
        targetCropBehaviour.SetCrop(newCropData);
        PlotStates[plotId] = cropName;
        string message = $"Planting {cropName} on plot {plotId}.";
        Debug.Log(message);
    }
    public void HarvestCrop(int plotId)
    {
        if (!PlotExists(plotId)) return;
        CropBehaviour targetCropBehaviour = FarmPlots[plotId].GetComponent<CropBehaviour>();
        if (!targetCropBehaviour.isFullyGrown) return;
        targetCropBehaviour.SetCrop(Crops[0]); 
        string message = $"Harvesting plot {plotId}.";
        Debug.Log(message);
        string cropName = PlotStates[plotId];
        if (string.IsNullOrEmpty(cropName))
        {
            Debug.LogWarning($"Nothing to harvest on plot {plotId}");
            return;
        }
        if (Inventory.ContainsKey(cropName)) 
        {
            Inventory[cropName]++;
        }
        else
        {
            Inventory[cropName] = 1;
        }
        PlotStates[plotId] = string.Empty;
        UpdateInventoryUI();
    }
    private bool PlotExists(int plotId)
    {
        if (plotId < 0 || plotId > FarmPlots.Length)
        {
            Debug.LogError($"Error: Plot {plotId} does not exist.");
            return false;
        }
        return true;
    }
    public int FindAvailablePlot()
    {
        for (int i = 0; i< FarmPlots.Length; i++)
        {
            var plot = FarmPlots[i];
            var plotCropData = plot.GetComponent<CropBehaviour>().cropData;
            if (plotCropData == Crops[0])
            {
                return i;
            }
        }
        Debug.Log("No available plots found.");
        return -1;
    }
    public List<int> FindFullyGrownPlots(CropData crop) 
    {
        List<int> fullyGrown = new List<int>();
        for (int i = 0; i < FarmPlots.Length; i++)
        {
            if (crop == null)
            {
                if (FarmPlots[i].GetComponent<CropBehaviour>().isFullyGrown) 
                {
                    fullyGrown.Add(i);
                }
            }
            else
            {
                if (FarmPlots[i].GetComponent<CropBehaviour>().isFullyGrown && FarmPlots[i].GetComponent<CropBehaviour>().cropData == crop) 
                {
                    fullyGrown.Add(i);
                }
            }
        }
        return fullyGrown;
    }
    void UpdateInventoryUI()
    {
        if (Inventory.ContainsKey("carrot"))
            CarrotAmountText.text = Inventory["carrot"] + "";
        if (Inventory.ContainsKey("corn"))
            CornAmountText.text = Inventory["corn"] + "";
        if (Inventory.ContainsKey("pumpkin"))
            PumpkinAmountText.text = Inventory["pumpkin"] + "";
        if (Inventory.ContainsKey("wheat"))
            WheatAmountText.text = Inventory["wheat"] + "";
    }

  

    public void BakeItem(string recipeName, int quantity)
    {
        CraftingRecipe recipeToBake = null;
        foreach (var r in recipes)
        {
            if (r.recipeName.ToLower() == recipeName.ToLower())
            {
                recipeToBake = r;
                break;
            }
        }

        if (recipeToBake == null)
        {
            Debug.LogError($"Recipe not found: {recipeName}");
            return;
        }

     

        
        if (!(Inventory.ContainsKey("wheat") && Inventory["wheat"] >= 3))
        {
            Debug.LogError($"Not enough ingredients for {recipeName}");
            return;
        }

        
        if (bakery.StartBaking(recipeToBake))
        {
            Inventory["wheat"] -= 3;
            Debug.Log($"Sent one {recipeName} to bakery.");
            UpdateInventoryUI();
        }
        else
        {
            Debug.Log("Busy bakery, can't bake now.");
        }
    }
}
