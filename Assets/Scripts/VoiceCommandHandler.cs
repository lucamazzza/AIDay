using UnityEngine;
using Neocortex;
using System.Collections.Generic;

public class VoiceCommandHandler : MonoBehaviour
{
    public Character neocortexCharacter;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (neocortexCharacter == null)
        {
            Debug.LogError("Neocortex Character not assigned.")
            return;
        }
        neocortexCharacter.OnAction.AddListener(HandleAction);
        neocortexCharacter.StartListening();
    }

    private void HandleAction(string actionName, Dictionary<string, string> entities) {
        Debug.log($"Received Action: {actionName}");
        switch (actionName) 
        {
            // TODO: add actions entries
            default:
                Debug.LogWarning($"Unknown action: {actionName}");
                break;
        }
    }
}
