using Neocortex; // Import the Neocortex namespace (check docs for exact name)
using System.Collections.Generic; // To use Dictionaries
using UnityEngine;

// This component will listen for Neocortex events
public class VoiceCommandHandler : MonoBehaviour
{
    // Assign your Neocortex Character component in the Inspector
    public Character neocortexCharacter;

    void Start()
    {
        if (neocortexCharacter == null)
        {
            Debug.LogError("Neocortex Character not assigned!");
            return;
        }

        // Subscribe to the "On Action" event.
        // This event fires when Neocortex detects an Action (intent) you defined.
        neocortexCharacter.OnAction.AddListener(HandleAction);

        // Tell Neocortex to start listening.
        // You might tie this to a UI button, but for a hackathon, just start it.
        neocortexCharacter.StartListening();
    }

    // This function is called every time Neocortex sends an Action
    private void HandleAction(string actionName, Dictionary<string, string> entities)
    {
        Debug.Log($"Received Action: {actionName}");

        // Use a switch to decide which game function to call
        switch (actionName)
        {
            case "PlantCrop":
                HandlePlantAction(entities);
                break;

            case "HarvestCrop":
                HandleHarvestAction(entities);
                break;

            default:
                Debug.LogWarning($"Unknown action: {actionName}");
                break;
        }
    }

    private void HandlePlantAction(Dictionary<string, string> entities)
    {
        // Try to get the "crop_name" entity
        if (!entities.TryGetValue("crop_name", out string crop))
        {
            // Handle missing entity. For now, just log an error.
            Debug.LogError("PlantCrop action called, but no 'crop_name' entity found.");
            return;
        }

        // Try to get the "plot_id" entity
        if (!entities.TryGetValue("plot_id", out string plotString))
        {
            Debug.LogError("PlantCrop action called, but no 'plot_id' entity found.");
            return;
        }

        // Convert the string "plot 1" or "1" into an integer
        int plotID = ParsePlotID(plotString);

        if (plotID == -1)
        {
            Debug.LogError($"Could not parse plot ID from: {plotString}");
            return;
        }

        // We have all the info! Call the FarmManager.
        FarmManager.Instance.PlantCrop(crop, plotID);
    }

    private void HandleHarvestAction(Dictionary<string, string> entities)
    {
        if (!entities.TryGetValue("plot_id", out string plotString))
        {
            Debug.LogError("HarvestCrop action called, but no 'plot_id' entity found.");
            return;
        }

        int plotID = ParsePlotID(plotString);
        if (plotID == -1)
        {
            Debug.LogError($"Could not parse plot ID from: {plotString}");
            return;
        }

        // Call the FarmManager
        FarmManager.Instance.HarvestCrop(plotID);
    }

    // Helper function to convert "plot 1", "first plot", "2" into a number
    private int ParsePlotID(string plotString)
    {
        plotString = plotString.ToLower();

        // Simple parsing. You can make this smarter.
        if (plotString.Contains("1") || plotString.Contains("first")) return 1;
        if (plotString.Contains("2") || plotString.Contains("second")) return 2;
        if (plotString.Contains("3") || plotString.Contains("third")) return 3;

        return -1; // Not found
    }
}