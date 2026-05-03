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
    UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
    if (transport != null)
    {
        // IMPORTANTE: Per l'Host usa "127.0.0.1" o lascia vuoto. 
        // Unity capirà automaticamente di dover ascoltare su tutte le interfacce reali.
        transport.ConnectionData.Address = "127.0.0.1"; 
        transport.ConnectionData.Port = 7777;
    }

    statusText.text = "Creazione arena...";
    if (NetworkManager.Singleton.StartHost())
    {
        NascondiMenu();
    }
}

public void AvviaClient()
{
    string ipDestinazione = ipInputField.text; // Qui scriverai l'IP reale (es. 192.168.1.50)
    UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
    
    if (transport != null)
    {
        transport.ConnectionData.Address = ipDestinazione;
        transport.ConnectionData.Port = 7777;
    }

    statusText.text = "Connessione a " + ipDestinazione + "...";
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