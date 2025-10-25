using Neocortex; // Namespace principale
using Neocortex.Data; // <-- IMPORTANTE: Aggiungi questo per ChatResponse
using System.Collections.Generic; // Per le Dictionaries
using UnityEngine;
using System.Collections;
using System;

// Assicurati che il GameObject abbia entrambi i componenti
[RequireComponent(typeof(NeocortexSmartAgent))]
[RequireComponent(typeof(AudioReceiver))]

public class VoiceCommandHandler : MonoBehaviour
{
    // Non serve pi� trascinarli nell'Inspector, li prendiamo automaticamente
    [SerializeField] private NeocortexSmartAgent agent;
    [SerializeField] private AudioReceiver audioReceiver;


    void Start()
    {
        // 1. Ottenere i componenti e verificarli
        agent = GetComponent<NeocortexSmartAgent>();
        audioReceiver = GetComponent<AudioReceiver>();

        if (agent == null || audioReceiver == null)
        {
            Debug.LogError("Mancano i componenti richiesti (NeocortexSmartAgent o AudioReceiver).");
            return;
        }

        // 2. Iscrizione agli Eventi

        // Evento: Ricezione della Trascrizione (Utile per debug)
        agent.OnTranscriptionReceived.AddListener((message) =>
        {
            Debug.Log($"You (Transcription): {message}");
        });

        // Evento: Audio Registrato (Invia l'audio all'Agente)
        audioReceiver.OnAudioRecorded.AddListener((audioClip) =>
        {
            // Non usiamo AudioToText per avere solo la trascrizione, ma 
            // AudioToAudio per ottenere l'azione e la risposta vocale.
            agent.AudioToAudio(audioClip);
        });

        // Evento: Risposta Chat (L'AZIONE e i parametri)
        agent.OnChatResponseReceived.AddListener(HandleChatResponse);
        StartCoroutine(MyCoroutine());
    }

    IEnumerator MyCoroutine()
    {
   

        // Start recording audio for 3 seconds
        audioReceiver.StartMicrophone();
        yield return new WaitForSeconds(5f); // attende 2 secondi
        audioReceiver.StopMicrophone();

    }

    private void HandleChatResponse(ChatResponse response)
    {

        // RI-AVVIO 2: Dopo aver gestito l'azione, riavvia il microfono 
        // per essere pronto per il comando successivo
        Debug.Log($"Neocortex: {response.message}");
        StartCoroutine(MyCoroutine());
    }

    // Chiamato quando l'utente smette di parlare
    private void HandleAudioRecorded(AudioClip clip)
    {
        // Invia la clip audio all'agente per l'analisi
        agent.AudioToAudio(clip);
    }

    

    // --- I TUOI VECCHI METODI (vanno bene cos�) ---

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

    // Converte "PLANT_CROP" in "PlantCrop"
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