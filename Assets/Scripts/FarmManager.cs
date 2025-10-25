using UnityEngine;
using TMPro;

// Questo script gestisce la logICA del gioco.
public class FarmManager : MonoBehaviour
{
    // 1. Trascina i tuoi 3 oggetti Plot (dalla Hierarchy) in questo array
    public GameObject[] farmPlots;

    // 2. Trascina il tuo oggetto StatusText (dalla Hierarchy) qui
    public TextMeshProUGUI statusText;

    // 3. Trascina i tuoi asset Sprite (dalla cartella Project) qui
    public Sprite emptyPlotSprite; // L'immagine del campo vuoto
    public Sprite wheatSprite;     // L'immagine del grano
    public Sprite cornSprite;      // L'immagine del mais
    public Sprite carrotSprite;    // L'immagine della carota
    // (Aggiungine altri se necessario)

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
        var plotRenderer = farmPlots[plotId - 1].GetComponent<SpriteRenderer>();

        switch (cropName.ToLower())
        {
            case "wheat":
                plotRenderer.sprite = wheatSprite; // Assegna il nuovo sprite
                break;
            case "corn":
                plotRenderer.sprite = cornSprite;
                break;
            case "carrot":
                plotRenderer.sprite = carrotSprite;
                break;
            default:
                statusText.text = $"Error: Crop '{cropName}' is not recognized.";
                Debug.LogError($"Error: Crop '{cropName}' is not recognized.");
                return; // Non aggiornare il testo se il raccolto non è valido
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
        var plotRenderer = farmPlots[plotId - 1].GetComponent<SpriteRenderer>();

        // Reimposta lo sprite a quello del campo vuoto
        plotRenderer.sprite = emptyPlotSprite;

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