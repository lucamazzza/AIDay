using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class FarmManager : MonoBehaviour
{
    public GameObject[] farmPlots;
    public CropData[] crops;

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
        int arrayIndex = plotId;
        if (!PlotExists(plotId)) return;
        CropBehaviour targetCropBehaviour = farmPlots[arrayIndex].GetComponent<CropBehaviour>();
        if (targetCropBehaviour == null)
        {
            Debug.LogError($"FATALE: Componente CropBehaviour mancante sul plot {plotId}.");
            return;
        }
        CropData newCropData = null;
        switch (cropName.ToLower())
        {
            case "wheat":
                newCropData = crops[4]; 
                break;
            case "corn":
                newCropData = crops[2];
                break;
            case "carrot":
                newCropData = crops[1];
                break;
            case "pumpkin":
                newCropData = crops[3];
                break;
            default:
                Debug.LogError($"Error: Crop '{cropName}' is not recognized.");
                return; 
        }
        targetCropBehaviour.SetCrop(newCropData);
        string message = $"Planting {cropName} on plot {plotId}.";
        Debug.Log(message);
    }

    public void HarvestCrop(int plotId)
    {
        if (!PlotExists(plotId)) return;
        CropBehaviour targetCropBehaviour = farmPlots[plotId].GetComponent<CropBehaviour>();
        if (!targetCropBehaviour.isFullyGrown) return;
        targetCropBehaviour.SetCrop(crops[0]); 
        string message = $"Harvesting plot {plotId}.";
        Debug.Log(message);
    }

    private bool PlotExists(int plotId)
    {
        if (plotId < 0 || plotId > farmPlots.Length)
        {
            Debug.LogError($"Error: Plot {plotId} does not exist.");
            return false;
        }
        return true;
    }

    public int FindAvailablePlot()
    {
        for (int i = 0; i< farmPlots.Length; i++)
        {
            var plot = farmPlots[i];
            var plotCropData = plot.GetComponent<CropBehaviour>().cropData;
            if (plotCropData == crops[0])
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
        for (int i = 0; i < farmPlots.Length; i++)
        {
            if (crop == null)
            {
                if (farmPlots[i].GetComponent<CropBehaviour>().isFullyGrown) 
                {
                    fullyGrown.Add(i);
                }
            }
            else
            {
                if (farmPlots[i].GetComponent<CropBehaviour>().isFullyGrown && farmPlots[i].GetComponent<CropBehaviour>().cropData == crop) 
                {
                    fullyGrown.Add(i);
                }
            }
        }
        return fullyGrown;
    }
}
