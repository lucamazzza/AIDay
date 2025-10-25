using UnityEngine;
using TMPro;

// Questo script gestisce la logICA del gioco.
public class FarmManager : MonoBehaviour
{
    // 1. Trascina i tuoi 3 oggetti Plot (dalla Hierarchy) in questo array
    public GameObject[] farmPlots;
    
    public CropData[] crops;

    // 2. Trascina il tuo oggetto StatusText (dalla Hierarchy) qui
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

    // Chiamato da VoiceCommandHandler
    public void PlantCrop(string cropName, int plotId)
    {
        if (!plotExists(plotId)) return;

        // Ottiene il componente SpriteRenderer (per il 2D)
        var plotCropData = farmPlots[plotId - 1].GetComponent<CropData>();

        switch (cropName.ToLower())
        {
            case "wheat":
                plotCropData = crops[4]; 
                break;
            case "corn":
                plotCropData = crops[2];
                break;
            case "carrot":
                plotCropData = crops[1];
                break;
            case "pumpkin":
                plotCropData = crops[3];
                break;
            default:
                statusText.text = $"Error: Crop '{cropName}' is not recognized.";
                Debug.LogError($"Error: Crop '{cropName}' is not recognized.");
                return; // Non aggiornare il testo se il raccolto non � valido
        }

        // Aggiorna il testo solo se l'azione ha successo
        string message = $"Planting {cropName} on plot {plotId}.";
        statusText.text = message;
        Debug.Log(message);
    }

    // Chiamato da VoiceCommandHandler
    public void HarvestCrop(int plotId)
    {
        if (!plotExists(plotId)) return;

        // Ottiene il componente SpriteRenderer
        var plotCropData = farmPlots[plotId - 1].GetComponent<CropData>();
        
        // Reimposta lo sprite a quello del campo vuoto
        plotCropData = crops[0]; 

        string message = $"Harvesting plot {plotId}.";
        statusText.text = message;
        Debug.Log(message);
    }

    // Funzione helper per controllare se il plot esiste
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