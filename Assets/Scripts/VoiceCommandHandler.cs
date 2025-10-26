using Neocortex;
using Neocortex.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(NeocortexSmartAgent))]
[RequireComponent(typeof(AudioReceiver))]
public class VoiceCommandHandler : MonoBehaviour
{
    [SerializeField] private NeocortexSmartAgent agent;
    [SerializeField] private AudioReceiver audioReceiver;
    [SerializeField] private FarmManager farmManager;
    [SerializeField] private TMP_Text text;
    [SerializeField] private AudioSource audioSource;

    void Start()
    {
        agent = GetComponent<NeocortexSmartAgent>();
        audioReceiver = GetComponent<AudioReceiver>();
        if (agent == null || audioReceiver == null)
        {
            Debug.LogError("Mancano i componenti richiesti (NeocortexSmartAgent o AudioReceiver).");
            return;
        }
        agent.OnTranscriptionReceived.AddListener(OnTranscription);
        audioReceiver.OnAudioRecorded.AddListener(OnAudioRecorded);
        agent.OnChatResponseReceived.AddListener(HandleChatResponse);
        agent.OnAudioResponseReceived.AddListener(OnAudioResponseReceived); // Corretto
        audioReceiver.StartMicrophone();
        Debug.Log("Microfono avviato. In attesa di comandi...");
    }

    private void OnAudioRecorded(AudioClip audioClip)
    {
        Debug.Log($"Audio registrato ({audioClip.samples} campioni), invio a Neocortex...");
        agent.AudioToAudio(audioClip);
    }
    private void OnTranscription(string message)
    {
        Debug.Log($"You (Transcription): {message}");
    }
    private void HandleChatResponse(ChatResponse response)
    {
        string action = response.action;
        Debug.Log($"Neocortex Action: {action}");
        Debug.Log($"Neocortex Message: {response.message}");
        if (text != null) text.text = response.message;
        string jsonString = JsonUtility.ToJson(response, true);
        Debug.LogWarning("--- INIZIO DEBUG JSON DA NEOCORTEX ---");
        Debug.Log(jsonString);
        Debug.LogWarning("--- FINE DEBUG JSON DA NEOCORTEX ---");
        if (string.IsNullOrEmpty(action))
        {
            Debug.Log("Nessuna azione rilevata.");
            return;
        }

        if (action == "PLANT_CROP")
        {
            ExtractPlantParameters(response.metadata, out string cropName, out int quantity);
            if (string.IsNullOrEmpty(cropName) || quantity == 0)
            {
                if (string.IsNullOrEmpty(cropName)) Debug.LogError("Azione PLANT_CROP: 'crop_name' non trovato.");
                if (quantity == 0) Debug.LogWarning("Azione PLANT_CROP: 'quantity' non trovata o è 0.");
                return;
            }

            Debug.Log($"Tentativo di piantare {quantity} {cropName}...");
            int plantedCount = 0;
            for (int i = 0; i < quantity; i++)
            {
                int plotId = farmManager.FindAvailablePlot();
                if (plotId != -1)
                {
                    farmManager.PlantCrop(cropName, plotId);
                    plantedCount++;
                }
                else
                {
                    Debug.LogWarning($"Non ci sono più plot liberi. Piantati {plantedCount}/{quantity} {cropName}.");
                    break;
                }
            }
            Debug.Log($"Piantati {plantedCount} {cropName} in totale.");
        }
        else if (action == "HARVEST_CROP")
        {
            ExtractPlantParameters(response.metadata, out string cropName, out int quantity);
            CropData cropToFind = null;
            if (!string.IsNullOrEmpty(cropName))
            {
                Debug.Log($"Harvesting specific crop: {cropName}");
                switch (cropName.ToLower())
                {
                    case "carrot": cropToFind = farmManager.Crops[1]; break;
                    case "corn": cropToFind = farmManager.Crops[2]; break;
                    case "pumpkin": cropToFind = farmManager.Crops[3]; break;
                    case "wheat": cropToFind = farmManager.Crops[4]; break;
                }
            }
            else
            {
                Debug.Log("Harvesting all fully grown plots.");
            }
            List<int> harvesteable = farmManager.FindFullyGrownPlots(cropToFind);
            if (harvesteable.Count == 0)
            {
                Debug.Log("Nessuna coltura pronta per il raccolto trovata.");
            }
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
    private void StartMicrophone()
    {
        audioReceiver.StartMicrophone();
        Debug.Log("Microfono riavviato per il prossimo comando.");
    }
    private void OnAudioResponseReceived(AudioClip audioClip)
    {
        if (audioSource == null)
        {
            Debug.LogError("AudioSource non assegnato! Impossibile riprodurre la risposta.");
            return;
        }
        // TODO: scommenta queste righe se vuoi effettivamente riprodurre l'audio
        //audioSource.clip = audioClip;
        //audioSource.Play();
        Invoke(nameof(StartMicrophone), audioClip.length + 0.2f); // Aggiunto un piccolo buffer
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
    private void ExtractPlantParameters(Interactable[] metadata, out string cropName, out int quantity)
    {
        cropName = null;
        quantity = 0;
        if (metadata == null || metadata.Length == 0)
        {
            Debug.LogWarning("ExtractPlantParameters fallito: array metadata è vuoto.");
            return;
        }
        foreach (Interactable item in metadata)
        {
            if (item == null || string.IsNullOrEmpty(item.name)) continue;
            if (int.TryParse(item.name, out int parsedQuantity))
            {
                quantity = parsedQuantity;
            }
            else if (IsKnownCrop(item.name))
            {
                cropName = item.name.ToLower();
            }
        }
    }
    private bool IsKnownCrop(string name)
    {
        string lowerName = name.ToLower();
        return lowerName == "wheat" ||
               lowerName == "corn" ||
               lowerName == "carrot" ||
               lowerName == "pumpkin";
    }
}
