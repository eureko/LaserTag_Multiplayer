using Unity.Netcode;
using UnityEngine;

public class NetworkUI : MonoBehaviour
{
    private void OnGUI()
    {
        // 1. Guard Clause per prevenire NullReferenceException
        if (NetworkManager.Singleton == null) return;

        float boxWidth = 200f;
        float boxHeight = 100f;
        float centerX = Screen.width / 2f - boxWidth / 2f;
        float centerY = Screen.height / 2f - boxHeight / 2f;

        GUILayout.BeginArea(new Rect(centerX, centerY, boxWidth, boxHeight));

        // 2. Disabilitiamo i tasti se la rete è già attiva per evitare chiamate multiple
        bool isNetworkActive = NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient;

        if (!isNetworkActive)
        {
            if (GUILayout.Button("Avvia come Host"))
            {
                NetworkManager.Singleton.StartHost();
                Debug.Log("Avvio del mondo 3D, come host");
            }

            if (GUILayout.Button("Avvia come Client"))
            {
                NetworkManager.Singleton.StartClient();
                Debug.Log("Avvio del mondo 3D, come client");
            }
        }

        // 3. UI di stato semplificata
        string status = "In attesa...";
        if (NetworkManager.Singleton.IsServer) status = "Host Attivo";
        else if (NetworkManager.Singleton.IsClient) status = "Client Attivo. In attesa del server....";
        
        GUILayout.Label("Stato: " + status);

        GUILayout.EndArea();
    }

    private void OnApplicationQuit()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }
}