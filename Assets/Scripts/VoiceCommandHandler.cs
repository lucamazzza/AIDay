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
    // Non serve più trascinarli nell'Inspector, li prendiamo automaticamente
    [SerializeField] private NeocortexSmartAgent agent;
    [SerializeField] private AudioReceiver audioReceiver;
    private String response = "";

    void Start()
    {



        agent.OnTranscriptionReceived.AddListener((message) =>
        {
            Debug.Log($"You: {message}");
            response = message;
        });

        var audioReceiver = GetComponent<NeocortexAudioReceiver>();
        audioReceiver.OnAudioRecorded.AddListener((audioClip) =>
        {
            Debug.Log($"Audio Data Length: {audioClip.samples}");
            agent.AudioToText(audioClip);
        });


        StartCoroutine(MyCoroutine());
        if (response.Length > 0)
            agent.TextToText(response);

        agent.OnChatResponseReceived.AddListener((response) =>
        {
            Debug.Log($"Message: {response.message}");
        });
    }

        IEnumerator MyCoroutine()
    {
   

        // Start recording audio for 3 seconds
        audioReceiver.StartMicrophone();
        yield return new WaitForSeconds(5f); // attende 2 secondi
        audioReceiver.StopMicrophone();

    }

    // Chiamato quando l'utente smette di parlare
    private void HandleAudioRecorded(AudioClip clip)
    {
        Debug.Log("Audio registrato, invio a Neocortex...");
        // Invia la clip audio all'agente per l'analisi
        agent.AudioToAudio(clip);
    }

    // Chiamato quando Neocortex ha analizzato l'audio e invia una risposta
    private void HandleChatResponse(ChatResponse response)
    {
        // Estrai l'azione (es. "PLANT_CROP")
        string actionName = response.action;

        // Estrai le entità (es. {"crop_name": "wheat", "plot_id": "1"})
        // Questo è il nome più probabile per la variabile, basato sul tuo tutorial
        //Dictionary<string, string> entities = response.parameters;

        // Se non c'è nessuna azione, era solo una chiacchiera. Riavviamo il microfono.
        if (string.IsNullOrEmpty(actionName))
        {
            Debug.Log("Nessuna azione rilevata, riavvio l'ascolto.");
            audioReceiver.StartMicrophone();
            return;
        }

        // --- Da qui, il tuo codice originale funziona quasi perfettamente ---

        Debug.Log($"Azione Ricevuta: {actionName}");

        // Converti "PLANT_CROP" in "PlantCrop" per farlo funzionare con lo switch
        //string formattedActionName = FormatActionName(actionName);

        //switch (formattedActionName)
        //{
        //    case "PlantCrop":
        //        HandlePlantAction(entities);
        //        break;

        //    case "HarvestCrop":
        //        HandleHarvestAction(entities);
        //        break;

        //    default:
        //        Debug.LogWarning($"Azione Sconosciuta: {actionName}");
        //        break;
        //}

        // IMPORTANTE: Dopo aver gestito l'azione, riavvia il microfono
        // Il sample lo fa dopo che la clip audio di risposta è finita.
        // Noi lo facciamo subito per semplicità.
        audioReceiver.StartMicrophone();
    }

    // --- I TUOI VECCHI METODI (vanno bene così) ---

    private void HandlePlantAction(Dictionary<string, string> entities)
    {
        if (entities == null)
        {
            Debug.LogError("HandlePlantAction fallito: Dizionario entità è nullo.");
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
            Debug.LogError("HandleHarvestAction fallito: Dizionario entità è nullo.");
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