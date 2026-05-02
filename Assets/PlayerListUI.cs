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
        string currentPlayers = "GIOCATORI:\n\n";

        PlayerController[] allPlayers = Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        foreach (var player in allPlayers)
        {
            string nome = "Anonimo";
            if (player.TryGetComponent<PlayerNameSync>(out var nameSync))
            {
                nome = nameSync.playerName.Value.ToString();
            }
			
			if (player.OwnerClientId == 0)
            {
                nome += " <color=#FFA500>[HOST]</color>"; // Aggiunge l'etichetta arancione
            }

            // 1. Controlliamo prima se ha vinto
            if (player.isWinner.Value)
            {
                currentPlayers += $"<color=yellow><b>{nome} - !!! HAI VINTO LA PARTITA !!!</b></color>\n";
            }
            // 2. Poi controlliamo se è morto
            else if (player.isDead)
            {
                currentPlayers += $"<color=red><s>{nome}</s></color> - ELIMINATO\n";
            }
            // 3. Altrimenti mostriamo il punteggio normale
            else
            {
                currentPlayers += $"{nome} - <color=#00FF00>Vite: {player.score.Value}</color>\n";
            }
        }

        listText.text = currentPlayers;
    }
}