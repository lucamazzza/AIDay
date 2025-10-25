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
    [SerializeField] private TMP_Text text;

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
        Debug.Log($"Neocortex Action: {action}");
        Debug.Log($"Neocortex Message: {response.message}");

        string jsonString = JsonUtility.ToJson(response, true);

        // 3. Stampa il JSON nella console
        //    Uso LogWarning così appare in giallo ed è facile da trovare
        Debug.LogWarning("--- INIZIO DEBUG JSON DA NEOCORTEX ---");
        Debug.Log(jsonString);
        Debug.LogWarning("--- FINE DEBUG JSON DA NEOCORTEX ---");




        if (string.IsNullOrEmpty(action))
        {
            StartCoroutine(MyCoroutine());
            return;
        }

        if (action == "PLANT_CROP")
        {
            // 1. Estrai i parametri dall'array metadata
            ExtractPlantParameters(response.metadata, out string cropName, out int quantity);

            // 2. Controlla se abbiamo tutto
            if (string.IsNullOrEmpty(cropName) || quantity == 0)
            {
                // A volte la quantità è "una" (stringa), prova a cercarla se la quantità è 0
                if (string.IsNullOrEmpty(cropName)) Debug.LogError("Azione PLANT_CROP: 'crop_name' non trovato.");
                if (quantity == 0) Debug.LogWarning("Azione PLANT_CROP: 'quantity' non trovata o è 0.");

                StartCoroutine(MyCoroutine());
                return;
            }

            // 3. Esegui un ciclo per la quantità richiesta
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
            // 1. Estrai i parametri dai metadati.
            //    Ci interessa solo 'cropName'. 'quantity' verrà ignorato.
            ExtractPlantParameters(response.metadata, out string cropName, out int quantity);

            CropData cropToFind = null;

            // 2. Se l'IA ha estratto un nome di coltura, trovalo nel tuo array 'crops'.
            //    (Questo presume che 'farmManager.crops' sia public o accessibile)
            if (!string.IsNullOrEmpty(cropName))
            {
                Debug.Log($"Harvesting specific crop: {cropName}");

                // NOTA: Questi indici (1, 2, 3, 4) sono basati
                // sulla tua logica 'PlantCrop'. Assicurati che siano corretti!
                switch (cropName.ToLower())
                {
                    case "carrot":
                        cropToFind = farmManager.crops[1];
                        break;
                    case "corn":
                        cropToFind = farmManager.crops[2];
                        break;
                    case "pumpkin":
                        cropToFind = farmManager.crops[3];
                        break;
                    case "wheat":
                        // (Nella tua richiesta hai scritto "pumpkin" sia per 3 che per 4,
                        //  ma il tuo 'PlantCrop' usa 4 per 'wheat')
                        cropToFind = farmManager.crops[4];
                        break;
                }
            }
            else
            {
                // 3. Se 'cropName' è nullo, significa che l'utente vuole raccogliere TUTTO.
                Debug.Log("Harvesting all fully grown plots.");
                // cropToFind rimane null (come richiesto)
            }

            // 4. Chiama la funzione del FarmManager con il CropData corretto (o null)
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

    /// <summary>
    /// Estrae il nome della coltura e la quantità dall'array metadata.
    /// Questo codice è progettato per leggere il JSON che hai fornito.
    /// </summary>
    private void ExtractPlantParameters(Interactable[] metadata, out string cropName, out int quantity)
    {
        // Imposta i valori di default
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

            // Tenta di convertire il 'name' in un numero
            if (int.TryParse(item.name, out int parsedQuantity))
            {
                // È un numero! Salvalo come quantità.
                quantity = parsedQuantity;
            }
            // Controlla se è una delle nostre colture conosciute
            else if (IsKnownCrop(item.name))
            {
                // È un nome di coltura! Salvalo.
                cropName = item.name.ToLower();
            }
        }
    }

    /// <summary>
    /// Funzione helper per verificare se una stringa è una coltura valida.
    /// </summary>
    private bool IsKnownCrop(string name)
    {
        string lowerName = name.ToLower();
        return lowerName == "wheat" ||
               lowerName == "corn" ||
               lowerName == "carrot" ||
               lowerName == "pumpkin";
    }
}


