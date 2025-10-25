using Neocortex;
using Neocortex.Data; 
using System.Collections.Generic; 
using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(NeocortexSmartAgent))]
[RequireComponent(typeof(AudioReceiver))]

public class VoiceCommandHandler : MonoBehaviour
{
    [SerializeField] private NeocortexSmartAgent agent;
    [SerializeField] private AudioReceiver audioReceiver;
    [SerializeField] private FarmManager farmManager;

    void Start()
    {
        agent = GetComponent<NeocortexSmartAgent>();
        audioReceiver = GetComponent<AudioReceiver>();
        if (agent == null || audioReceiver == null)
        {
            Debug.LogError("Mancano i componenti richiesti (NeocortexSmartAgent o AudioReceiver).");
            return;
        }
        agent.OnTranscriptionReceived.AddListener((message) =>
        {
            Debug.Log($"You (Transcription): {message}");
        });
        audioReceiver.OnAudioRecorded.AddListener((audioClip) =>
        {
            agent.AudioToAudio(audioClip);
        });
        agent.OnChatResponseReceived.AddListener(HandleChatResponse);
        StartCoroutine(MyCoroutine());
    }

    IEnumerator MyCoroutine()
    {
        audioReceiver.StartMicrophone();
        yield return new WaitForSeconds(5f);
        audioReceiver.StopMicrophone();
    }

    private void HandleChatResponse(ChatResponse response)
    {
        string action = response.action;
        Debug.Log($"Neocortex: {response.action}");

        if (!string.IsNullOrEmpty(action))
        {
            if (action == "PLANT_CROP")
            {
                int plotId = farmManager.FindAvailablePlot();
                if (plotId != -1)
                {
                    farmManager.PlantCrop("carrot", plotId);  
                }
            }
            else if (action == "HARVEST_CROP")
            {
                List<int> harvesteable = FarmManager.Instance.FindFullyGrownPlots(null);
                foreach (int i in harvesteable)
                {
                    farmManager.HarvestCrop(i);
                }
            }
            else
            {
                Debug.LogWarning($"Azione non riconosciuta: {action}");
            }
        }
        Debug.Log($"Neocortex: {response.message}");
        StartCoroutine(MyCoroutine());
    }

    private void HandleAudioRecorded(AudioClip clip)
    {
        agent.AudioToAudio(clip);
    }

    private void HandlePlantAction(Dictionary<string, string> entities)
    {
        if (entities == null)
        {
            Debug.LogError("HandlePlantAction fallito: Dizionario entità nullo.");
            return;
        }

        if (!entities.TryGetValue("crop_name", out string crop))
        {
            Debug.LogError("PlantCrop: 'crop_name' non trovato.");
            return;
        }

        if (!entities.TryGetValue("plot_id", out string plotString))
        {
            Debug.LogError("PlantCrop: 'plot_id' non trovato.");
            return;
        }

        int plotID = ParsePlotID(plotString);
        if (plotID == -1)
        {
            Debug.LogError($"Impossibile analizzare plot ID da: {plotString}");
            return;
        }

        FarmManager.Instance.PlantCrop(crop, plotID);
    }

    private void HandleHarvestAction(Dictionary<string, string> entities)
    {
        if (entities == null)
        {
            Debug.LogError("HandleHarvestAction fallito: Dizionario entit� � nullo.");
            return;
        }
        if (!entities.TryGetValue("plot_id", out string plotString))
        {
            Debug.LogError("HarvestCrop: 'plot_id' non trovato.");
            return;
        }
        int plotID = ParsePlotID(plotString);
        if (plotID == -1)
        {
            Debug.LogError($"Impossibile analizzare plot ID da: {plotString}");
            return;
        }
        FarmManager.Instance.HarvestCrop(plotID);
    }

    private int ParsePlotID(string plotString)
    {
        plotString = plotString.ToLower();
        if (plotString.Contains("1") || plotString.Contains("first")) return 1;
        if (plotString.Contains("2") || plotString.Contains("second")) return 2;
        if (plotString.Contains("3") || plotString.Contains("third")) return 3;
        return -1;
    }

    private string FormatActionName(string actionName)
    {
        if (string.IsNullOrEmpty(actionName)) return string.Empty;
        if (actionName.Contains("_"))
        {
            string[] parts = actionName.Split('_');
            string formattedName = "";
            foreach (string part in parts)
            {
                if (part.Length > 0)
                    formattedName += char.ToUpper(part[0]) + part.Substring(1).ToLower();
            }
            return formattedName;
        }
        return char.ToUpper(actionName[0]) + actionName.Substring(1);
    }
}