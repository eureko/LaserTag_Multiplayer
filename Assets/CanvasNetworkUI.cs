using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CanvasNetworkUI : MonoBehaviour
{
    public static CanvasNetworkUI Instance;

    [Header("Elementi UI")]
    public TMP_InputField nameInputField;
    public Button playButton; 
    public TextMeshProUGUI statusText;

    [Header("Contenitori e Telecamere")]
    public GameObject uiPanel; 
    public GameObject menuCamera;
    public GameObject playerListPanel;

    public string PlayerName { get; private set; } = "Anonimo";
    
    // Variabile per capire se stiamo cercando una partita o se siamo già in gioco
    private bool staSondandoLaRete = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        playButton.onClick.AddListener(AvviaMatchmaking);
        nameInputField.onValueChanged.AddListener(UpdateName);
        
        statusText.text = "Stato: Pronti a giocare!";

        // Sottoscrizione agli eventi di Netcode
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void UpdateName(string newName)
    {
        if (!string.IsNullOrWhiteSpace(newName)) 
            PlayerName = newName;
    }

    // 1. IL GIOCATORE PREME "ENTRA NELL'ARENA"
    private void AvviaMatchmaking()
    {
        statusText.text = "Ricerca arena in corso...";
        staSondandoLaRete = true;
        playButton.interactable = false;

        // Configuriamo il trasporto per un tentativo di connessione lampo
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (transport != null)
        {
            transport.MaxConnectAttempts = 1; // Prova solo una volta
            transport.ConnectTimeoutMS = 500; // Aspetta solo mezzo secondo
        }

        // Proviamo ad entrare come Client
        NetworkManager.Singleton.StartClient();
    }

    // 2. SE TROVIAMO UN HOST, CI CONNETTIAMO
    private void OnClientConnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            staSondandoLaRete = false;
            NascondiMenu();
        }
    }

    // 3. SE NON TROVIAMO NESSUNO, SCATTA IL FALLIMENTO
    private void OnClientDisconnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId || clientId == 0)
        {
            if (staSondandoLaRete)
            {
                // Abbiamo fallito la ricerca: ora resettiamo e diventiamo Host
                staSondandoLaRete = false;
                StartCoroutine(ResetEPuliziaPerHost());
            }
            else
            {
                // Disconnessione normale durante il gioco
                MostraMenu();
                statusText.text = "Sei stato disconnesso.";
                playButton.interactable = true;
            }
        }
    }

    // 4. COROUTINE DI PULIZIA: Il segreto per evitare l'errore critico
    private IEnumerator ResetEPuliziaPerHost()
    {
        statusText.text = "Nessuna arena trovata. Creazione in corso...";

        // Spegniamo il NetworkManager per chiudere i socket rimasti aperti dal tentativo Client
        NetworkManager.Singleton.Shutdown();

        // Aspettiamo che il sistema abbia finito di pulire tutto
        yield return new WaitUntil(() => !NetworkManager.Singleton.ShutdownInProgress);

        // Ripristiniamo i valori di trasporto standard per i futuri Client che si uniranno a noi
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (transport != null)
        {
            transport.MaxConnectAttempts = 60; 
            transport.ConnectTimeoutMS = 1000; 
        }

        // Ora che il terreno è pulito, avviamo l'Host
        if (NetworkManager.Singleton.StartHost())
        {
            NascondiMenu();
        }
        else
        {
            // Se fallisce ancora, mostriamo l'errore e riabilitiamo il tasto
            MostraMenu();
            statusText.text = "Errore critico di rete!";
            playButton.interactable = true;
        }
    }

    private void NascondiMenu()
    {
        if (uiPanel != null) uiPanel.SetActive(false);
        if (menuCamera != null) menuCamera.SetActive(false);
        if (playerListPanel != null) playerListPanel.SetActive(true);
    }

    private void MostraMenu()
    {
        if (uiPanel != null) uiPanel.SetActive(true);
        if (menuCamera != null) menuCamera.SetActive(true);
        if (playerListPanel != null) playerListPanel.SetActive(false);
    }
}