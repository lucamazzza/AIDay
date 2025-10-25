using Neocortex;
using Neocortex.Data;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System;
using TMPro;

[RequireComponent(typeof(NeocortexSmartAgent))]
[RequireComponent(typeof(AudioReceiver))]
public class VoiceCommandHandler : MonoBehaviour
{
    [SerializeField] private NeocortexSmartAgent agent;
    [SerializeField] private AudioReceiver audioReceiver;
    [SerializeField] private FarmManager farmManager;
    [SerializeField] private TMP_Text text; // Assicurati di usare questa variabile 'text'

    void Start()
    {
        agent = GetComponent<NeocortexSmartAgent>();
        audioReceiver = GetComponent<AudioReceiver>();
        if (agent == null || audioReceiver == null)
        {
            Debug.LogError("Mancano i componenti richiesti (NeocortexSmartAgent o AudioReceiver).");
            return;
        }

        // --- ISCRIZIONE AGLI EVENTI ---
        agent.OnTranscriptionReceived.AddListener(OnTranscription); // Chiama una funzione nominata
        audioReceiver.OnAudioRecorded.AddListener(OnAudioRecorded); // Chiama una funzione nominata
        agent.OnChatResponseReceived.AddListener(HandleChatResponse); // Chiama la tua logica principale

        // --- AVVIO INIZIALE ---
        // Avvia l'ascolto la prima volta. Non usare una coroutine.
        audioReceiver.StartMicrophone();
        Debug.Log("Microfono avviato. In attesa di comandi...");
    }

    // Evento: L'utente ha finito di parlare e l'audio è pronto
    private void OnAudioRecorded(AudioClip audioClip)
    {
        Debug.Log($"Audio registrato ({audioClip.samples} campioni), invio a Neocortex...");
        // Invia l'audio per l'analisi (azione E audio)
        agent.AudioToAudio(audioClip);
    }

    // Evento: Neocortex ha trascritto il testo (solo per debug)
    private void OnTranscription(string message)
    {
        Debug.Log($"You (Transcription): {message}");
        if (text != null) text.text = message; // Aggiorna la UI con quello che hai detto
    }

    // Evento: Neocortex ha analizzato la richiesta e invia l'azione
    private void HandleChatResponse(ChatResponse response)
    {
        string action = response.action;
        Debug.Log($"Neocortex Action: {action}");
        Debug.Log($"Neocortex Message: {response.message}");
        if (text != null) text.text = response.message; // Aggiorna la UI con la risposta dell'IA

        // --- DEBUG JSON (mantienilo per ora) ---
        string jsonString = JsonUtility.ToJson(response, true);
        Debug.LogWarning("--- INIZIO DEBUG JSON DA NEOCORTEX ---");
        Debug.Log(jsonString);
        Debug.LogWarning("--- FINE DEBUG JSON DA NEOCORTEX ---");
        // --- FINE DEBUG ---

        if (string.IsNullOrEmpty(action))
        {
            Debug.Log("Nessuna azione rilevata.");
            // RI-AVVIA L'ASCOLTO (CICLO)
            audioReceiver.StartMicrophone();
            return;
        }

        if (action == "PLANT_CROP")
        {
            ExtractPlantParameters(response.metadata, out string cropName, out int quantity);

            if (string.IsNullOrEmpty(cropName) || quantity == 0)
            {
                if (string.IsNullOrEmpty(cropName)) Debug.LogError("Azione PLANT_CROP: 'crop_name' non trovato.");
                if (quantity == 0) Debug.LogWarning("Azione PLANT_CROP: 'quantity' non trovata o è 0.");

                // RI-AVVIA L'ASCOLTO (CICLO)
                audioReceiver.StartMicrophone();
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
            ExtractPlantParameters(response.metadata, out string cropName, out int quantity); // 'quantity' verrà ignorata qui
            CropData cropToFind = null;
            if (!string.IsNullOrEmpty(cropName))
            {
                Debug.Log($"Harvesting specific crop: {cropName}");
                switch (cropName.ToLower())
                {
                    case "carrot": cropToFind = farmManager.crops[1]; break;
                    case "corn": cropToFind = farmManager.crops[2]; break;
                    case "pumpkin": cropToFind = farmManager.crops[3]; break;
                    case "wheat": cropToFind = farmManager.crops[4]; break;
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

        // RI-AVVIA L'ASCOLTO (CICLO)
        // Chiama questo UNA SOLA VOLTA alla fine del metodo.
        audioReceiver.StartMicrophone();
    }

    // --- NON HAI PIÙ BISOGNO DI 'MyCoroutine' O 'WaitNSeconds' ---
    /*
    IEnumerator MyCoroutine()
    {
        audioReceiver.StartMicrophone();
        yield return new WaitForSeconds(5f);
        audioReceiver.StopMicrophone();
    }

    IEnumerator WaitNSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }
    */

    // --- LE TUE FUNZIONI HELPER (QUESTE SONO CORRETTE) ---
    private string FormatActionName(string actionName)
    {
        // ... (il tuo codice è corretto) ...
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
        // ... (il tuo codice è corretto) ...
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
        // ... (il tuo codice è corretto) ...
        string lowerName = name.ToLower();
        return lowerName == "wheat" ||
           lowerName == "corn" ||
           lowerName == "carrot" ||
           lowerName == "pumpkin";
    }
}