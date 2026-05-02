using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CanvasNetworkUI : MonoBehaviour
{
    public static CanvasNetworkUI Instance;

    [Header("Elementi UI")]
    public TMP_InputField nameInputField;
    public TMP_InputField ipInputField; // Trascina qui il nuovo InputField dell'IP
    public Button hostButton;           // Trascina qui il tasto "Crea"
    public Button clientButton;         // Trascina qui il tasto "Partecipa"
    public TextMeshProUGUI statusText;

    [Header("Contenitori")]
    public GameObject uiPanel; 
    public GameObject menuCamera;
    public GameObject playerListPanel;

    public string PlayerName { get; private set; } = "Anonimo";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        hostButton.onClick.AddListener(AvviaHost);
        clientButton.onClick.AddListener(AvviaClient);
        nameInputField.onValueChanged.AddListener(UpdateName);
        
        // Di default mettiamo l'indirizzo locale per i tuoi test
        ipInputField.text = "127.0.0.1";
        statusText.text = "LaserTag Multiplayer Pronto";

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void UpdateName(string newName)
    {
        if (!string.IsNullOrWhiteSpace(newName)) PlayerName = newName;
    }

    // --- LOGICA PER IL PROF ---
    public void AvviaHost()
    {
        string ipLocale = ipInputField.text; // Prende l'IP che hai scritto (es. 10.76.244.212)

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (transport != null)
        {
            // Fondamentale: impostiamo l'indirizzo dell'Host prima di partire
            transport.ConnectionData.Address = ipLocale;
            // Opzionale: se la scuola usa porte diverse, assicurati che sia 7777
            transport.ConnectionData.Port = 7777; 
        }

        statusText.text = "Creazione arena su " + ipLocale + "...";
        
        if (NetworkManager.Singleton.StartHost())
        {
            NascondiMenu();
        }
        else
        {
            statusText.text = "Errore: Impossibile creare l'arena.";
        }
    }

    // --- LOGICA PER GLI ALUNNI ---
   public void AvviaClient()
    {
        string ipDestinazione = ipInputField.text;
        
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (transport != null)
        {
            // Forziamo l'indirizzo e assicuriamoci che la porta sia quella corretta (es. 7777)
            transport.ConnectionData.Address = ipDestinazione;
            // transport.ConnectionData.Port = 7777; // De-commenta questa riga se vuoi forzarla
        }

        statusText.text = "Ricerca arena su " + ipDestinazione + "...";
        
        // Avviamo il client
        NetworkManager.Singleton.StartClient();
    }

    private void OnClientConnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            NascondiMenu();
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            MostraMenu();
            statusText.text = "Disconnesso dall'arena.";
        }
    }

    private void NascondiMenu()
    {
        uiPanel.SetActive(false);
        menuCamera.SetActive(false);
        playerListPanel.SetActive(true);
    }

    private void MostraMenu()
    {
        uiPanel.SetActive(true);
        menuCamera.SetActive(true);
        playerListPanel.SetActive(false);
    }
}