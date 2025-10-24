using UnityEngine;
using TMPro;

public class FarmManager : MonoBehaviour
{
    public GameObject[] farmPlots;
    public TextMeshProUGUI statusText;

    public static FarmManager Instance { get; private set; } 
    
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

    public void PlantCrop(string cropName, int plotId)
    {
        if (!plotExists(plotId)) return;
        var PlotRenderer = farmPlots[plotId - 1].GetComponent<Renderer>();
        switch (cropName.ToLower()) 
        {
            // TODO: add crops here as switch cases
            default:
                statusText.text = $"Error: Crop '{cropName}' is not recognized.";
                Debug.LogError($"Error: Crop '{cropName}' is not recognized.");
                break;
        }
    }
    
    public void HarvestCrop(int plotId)
    {
        if (!plotExists(plotId)) return;
        // TODO: implement harvesting logic 
    }


    private bool plotExists(int plotId) 
    {
        if (plotId < 1 || plotId > farmPlots.Length)
        {
            statusText.text = $"Error: Plot {plotId} does not exist.";
            Debug.LogError($"Error: Plot {plotId} does not exist.");
            return false;
        }
        return true;
    }
}
