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
        var plotRenderer = farmPlots[plotId - 1].GetComponent<Renderer>();
        switch (cropName.ToLower()) 
        {
            //TODO: Change the assets to represent different crops
            //TODO: Add more crops as needed
            case "wheat":
                plotRenderer.material.color = Color.yellow;
                break;
            case "corn":
                plotRenderer.material.color = Color.green;
                break;
            case "carrot":
                plotRenderer.material.color = new Color(1.0f, 0.5f, 0.0f); // Orange
                break;
            default:
                statusText.text = $"Error: Crop '{cropName}' is not recognized.";
                Debug.LogError($"Error: Crop '{cropName}' is not recognized.");
                break;
        }
    }
    
    public void HarvestCrop(int plotId)
    {
        if (!plotExists(plotId)) return;

        // TODO: Change the asset to empty field
        var plotRenderer = farmPlots[plotId - 1].GetComponent<Renderer>();
        plotRenderer.material.color = Color.white; // White = empty

        string message = $"Harvesting plot {plotId}.";
        statusText.text = message;
        Debug.Log(message);
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
