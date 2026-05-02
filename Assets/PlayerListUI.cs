using UnityEngine;
using TMPro;
using Unity.Netcode;

public class PlayerListUI : MonoBehaviour
{
    public TextMeshProUGUI listText;
    
    // Variabili per il timer
    private float timer = 0f;
    private float updateInterval = 0.5f; // Aggiorna la lista ogni 0.5 secondi

    private void Update()
    {
        // Se non siamo connessi, non facciamo nulla
        if (!NetworkManager.Singleton || (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer))
            return;

        // Facciamo scorrere il tempo
        timer += Time.deltaTime;

        // Quando passa mezzo secondo, aggiorniamo la lista e azzeriamo il timer
        if (timer >= updateInterval)
        {
            AggiornaLista();
            timer = 0f;
        }
    }

    private void AggiornaLista()
    {
        string currentPlayers = "GIOCATORI:\n";

        // Questa operazione pesante ora avviene solo 2 volte al secondo, non 60!
        PlayerNameSync[] allPlayers = Object.FindObjectsByType<PlayerNameSync>(FindObjectsSortMode.None);

        foreach (var player in allPlayers)
        {
            currentPlayers += player.playerName.Value.ToString() + "\n";
        }

        listText.text = currentPlayers;
    }
}