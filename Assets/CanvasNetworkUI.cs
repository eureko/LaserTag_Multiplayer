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

    [Header("Impostazioni Arena (Extra)")]
    public GameObject targetPrefab;    // Trascina qui il Prefab del bersaglio
    public int numeroTarget = 50;      // Quanti target vuoi creare all'inizio

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
            // Unity capirà automaticamente di dover ascoltare su tutte le interfacce reali.
            transport.ConnectionData.Address = "127.0.0.1"; 
            transport.ConnectionData.Port = 7777;
        }

        statusText.text = "Creazione arena...";
        if (NetworkManager.Singleton.StartHost())
        {
            NascondiMenu();
            // ESEGUIAMO LO SPAWN SOLO QUI: Una volta sola, solo sul Server/Host
            GeneraArena();
        }
    }

    private void GeneraArena()
    {
        if (targetPrefab == null)
        {
            Debug.LogError("ERRORE: targetPrefab non assegnato in CanvasNetworkUI!");
            return;
        }

        for (int i = 0; i < numeroTarget; i++)
        {
            // Posizione casuale (modifica i range -15, 15 in base alla grandezza del tuo piano)
            Vector3 randomPos = new Vector3(Random.Range(-15f, 15f), 0.5f, Random.Range(-15f, 15f));
            
            // Crea l'istanza sul server
            GameObject target = Instantiate(targetPrefab, randomPos, Quaternion.identity);
            
            // Lo rende visibile a tutti i client connessi e futuri
            target.GetComponent<NetworkObject>().Spawn();
        }
        Debug.Log($"Arena popolata con {numeroTarget} bersagli dall'Host.");
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
	
    private void OnApplicationQuit()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            Debug.Log("NetworkManager spento correttamente.");
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }
}