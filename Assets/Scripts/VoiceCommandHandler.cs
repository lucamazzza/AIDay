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
            Debug.LogError("some components missing (NeocortexSmartAgent or AudioReceiver).");
            return;
        }
        agent.OnTranscriptionReceived.AddListener(OnTranscription);
        audioReceiver.OnAudioRecorded.AddListener(OnAudioRecorded);
        agent.OnChatResponseReceived.AddListener(HandleChatResponse);
        agent.OnAudioResponseReceived.AddListener(OnAudioResponseReceived); 
        audioReceiver.StartMicrophone();
        Debug.Log("Microphone started. Waiting for commands...");
    }

    private void OnAudioRecorded(AudioClip audioClip)
    {
        Debug.Log($"Audio recorded ({audioClip.samples} samples), sending to Neocortex...");
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
            Debug.Log("Any action found.");
            return;
        }

        if (action == "PLANT_CROP")
        {
            ExtractParameters(response.metadata, out string cropName, out int quantity);
            if (string.IsNullOrEmpty(cropName) || quantity == 0)
            {
                if (string.IsNullOrEmpty(cropName)) Debug.LogError("Action PLANT_CROP: 'crop_name' not found.");
                if (quantity == 0) Debug.LogWarning("Action PLANT_CROP: 'quantity' not found or 0.");
                return;
            }

            Debug.Log($"Trying to plant {quantity} {cropName}...");
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
                    Debug.LogWarning($"Any free plots left. Planted {plantedCount}/{quantity} {cropName}.");
                    break;
                }
            }
            Debug.Log($"Planted {plantedCount} {cropName} in total.");
        }
        else if (action == "HARVEST_CROP")
        {
            ExtractParameters(response.metadata, out string cropName, out int quantity);
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
                Debug.Log("not crops ready to harvest");
            }
            foreach (int i in harvesteable)
            {
                farmManager.HarvestCrop(i);
            }
        }
        else if (action == "BAKE_BREAD") 
        {

            ExtractParameters(response.metadata, out string itemName, out int quantity);

            if (string.IsNullOrEmpty(itemName) || quantity == 0)
            {
                Debug.LogError("Action BAKE_BREAD: 'item_name' o 'quantity' not found.");
                return;
            }

            farmManager.BakeItem(itemName, quantity);
        }
        else
        {
            Debug.LogWarning($"Action not recognized: {action}");
        }
    }
    private void StartMicrophone()
    {
        audioReceiver.StartMicrophone();
        Debug.Log("Started microphone for next command");
    }
    private void OnAudioResponseReceived(AudioClip audioClip)
    {
        if (audioSource == null)
        {
            Debug.LogError("AudioSource not assigned!");
            return;
        }
        
        audioSource.clip = audioClip;
        audioSource.Play();
        Invoke(nameof(StartMicrophone), audioClip.length + 0.2f); 
    }
    
    private void ExtractParameters(Interactable[] metadata, out string cropName, out int quantity)
    {
        cropName = null;
        quantity = 1;
        if (metadata == null || metadata.Length == 0)
        {
            Debug.LogWarning("ExtractPlantParameters failed: array metadata is empty.");
            return;
        }
        foreach (Interactable item in metadata)
        {
            if (item == null || string.IsNullOrEmpty(item.name)) continue;
            if (int.TryParse(item.name, out int parsedQuantity))
            {
                quantity = parsedQuantity;
            }
            else if (IsKnownItem(item.name))
            {
                cropName = item.name.ToLower();
            }
        }
    }
    private bool IsKnownItem(string name)
    {
        string lowerName = name.ToLower();
        return lowerName == "wheat" ||
               lowerName == "corn" ||
               lowerName == "carrot" ||
               lowerName == "pumpkin" ||
               lowerName == "bread";
    }
}
