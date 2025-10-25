using UnityEngine;
using TMPro;

// Questo script gestisce la logICA del gioco.
public class FarmManager : MonoBehaviour
{
    // 1. Trascina i tuoi 3 oggetti Plot (dalla Hierarchy) in questo array
    public GameObject[] farmPlots;
    
    public CropData[] crops;

    // 2. Trascina il tuo oggetto StatusText (dalla Hierarchy) qui
    //public TextMeshProUGUI statusText;

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

    // Chiamato da VoiceCommandHandler
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

        // 2. Assegna il dato.
        // Usiamo una variabile temporanea per chiarezza, se necessario.
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
                //statusText.text = $"Error: Crop '{cropName}' is not recognized.";
                Debug.LogError($"Error: Crop '{cropName}' is not recognized.");
                return; // Non aggiornare il testo se il raccolto non � valido
        }

        targetCropBehaviour.SetCrop(newCropData);



        // Aggiorna il testo solo se l'azione ha successo
        string message = $"Planting {cropName} on plot {plotId}.";
        //statusText.text = message;
        Debug.Log(message);
    }

    // Chiamato da VoiceCommandHandler
    public void HarvestCrop(int plotId)
    {
        if (!PlotExists(plotId)) return;

        // Ottiene il componente SpriteRenderer
        var plotCropData = farmPlots[plotId - 1].GetComponent<CropBehaviour>().cropData;
        
        // Reimposta lo sprite a quello del campo vuoto
        plotCropData = crops[0]; 

        string message = $"Harvesting plot {plotId}.";
        //statusText.text = message;
        Debug.Log(message);
    }

    // Funzione helper per controllare se il plot esiste
    private bool PlotExists(int plotId)
    {
        if (plotId < 0 || plotId > farmPlots.Length)
        {
            //statusText.text = $"Error: Plot {plotId} does not exist.";
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
}
