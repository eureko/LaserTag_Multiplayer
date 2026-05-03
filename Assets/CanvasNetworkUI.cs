using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport.Relay;

public class CanvasNetworkUI : MonoBehaviour
{
    public static CanvasNetworkUI Instance;

    [Header("Elementi UI")]
    public TMP_InputField nameInputField;
    public TMP_InputField ipInputField;
    public Button hostButton;
    public Button clientButton;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI codiceStanzaTesto;

    [Header("Contenitori")]
    public GameObject uiPanel; 
    public GameObject menuCamera;
    public GameObject playerListPanel;

    [Header("Impostazioni Arena")]
    public GameObject targetPrefab;
    public int numeroTarget = 50;

    public string PlayerName { get; private set; } = "Anonimo";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private async void Start()
    {
        hostButton.onClick.AddListener(AvviaHost);
        clientButton.onClick.AddListener(AvviaClient);
        nameInputField.onValueChanged.AddListener(UpdateName);
        
        // All'avvio il mouse deve essere libero
        SbloccaMouse();

        statusText.text = "Inizializzazione servizi...";

        try 
        {
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            statusText.text = "LaserTag Pronto (Relay Attivo)";
            ipInputField.text = "";
        }
        catch (System.Exception e)
        {
            statusText.text = "Errore Servizi: " + e.Message;
            Debug.LogError(e);
        }

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    // --- NUOVA FUNZIONE UPDATE PER ESC ---
    private void Update()
    {
        // Se premo ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Se il menu è chiuso (sto giocando), lo riapro
            if (!uiPanel.activeSelf)
            {
                MostraMenu();
            }
            // Se il menu è già aperto, sblocchiamo comunque il mouse (sicurezza)
            else
            {
                SbloccaMouse();
            }
        }

        // Forza il mouse visibile se il menu è attivo (per evitare conflitti con altri script)
        if (uiPanel.activeSelf && Cursor.lockState != CursorLockMode.None)
        {
            SbloccaMouse();
        }
    }

    private void UpdateName(string newName)
    {
        if (!string.IsNullOrWhiteSpace(newName)) PlayerName = newName;
    }

    public async void AvviaHost()
{
    try 
    {
        statusText.text = "Generazione codice...";
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(20);
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        // --- MODIFICA QUI ---
        // Creiamo i dati del server specificando esplicitamente il protocollo "wss"
        var relayServerData = new RelayServerData(allocation, "wss");
        transport.SetRelayServerData(relayServerData);
        // --------------------

        if (NetworkManager.Singleton.StartHost())
        {
            string msg = "CODICE: " + joinCode;
            statusText.text = msg;
            if(codiceStanzaTesto != null) codiceStanzaTesto.text = msg;
            NascondiMenu();
            GeneraArena();
        }
    }
    catch (System.Exception e)
    {
        statusText.text = "Errore Host: " + e.Message;
        Debug.LogError(e);
    }
}

    public async void AvviaClient()
{
    try 
    {
        string codiceInserito = ipInputField.text;
        if (string.IsNullOrEmpty(codiceInserito)) return;

        statusText.text = "Connessione...";
        JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(codiceInserito);

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        // --- MODIFICA QUI ---
        var relayServerData = new RelayServerData(joinAllocation, "wss");
        transport.SetRelayServerData(relayServerData);
        // --------------------

        if (NetworkManager.Singleton.StartClient())
        {
            if(codiceStanzaTesto != null) codiceStanzaTesto.text = "CODICE: " + codiceInserito.ToUpper();
        }
    }
    catch (System.Exception e)
    {
        statusText.text = "Codice errato!";
        Debug.LogError(e);
    }
}

    private void GeneraArena()
    {
        if (targetPrefab == null) return;
        for (int i = 0; i < numeroTarget; i++)
        {
            Vector3 randomPos = new Vector3(Random.Range(-15f, 15f), 0.5f, Random.Range(-15f, 15f));
            GameObject target = Instantiate(targetPrefab, randomPos, Quaternion.identity);
            target.GetComponent<NetworkObject>().Spawn();
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId) NascondiMenu();
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId) MostraMenu();
    }

    private void NascondiMenu()
{
    uiPanel.SetActive(false);
    menuCamera.SetActive(false);
    playerListPanel.SetActive(true);

    // DISABILITA I TASTI per evitare doppi click accidentali
    hostButton.interactable = false;
    clientButton.interactable = false;

    // Blocca mouse per giocare
    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;
}

private void MostraMenu()
{
    uiPanel.SetActive(true);
    menuCamera.SetActive(true);
    playerListPanel.SetActive(false);

    // RIABILITA I TASTI solo se non siamo già connessi
    // Se il NetworkManager non è attivo, permettiamo di cliccare
    if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient)
    {
        hostButton.interactable = true;
        clientButton.interactable = true;
    }
    else
    {
        // Se siamo già in partita, i tasti rimangono grigi (disabilitati)
        hostButton.interactable = false;
        clientButton.interactable = false;
    }

    SbloccaMouse();
}

    private void SbloccaMouse()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnApplicationQuit() { NetworkManager.Singleton?.Shutdown(); }
    private void OnDestroy() { NetworkManager.Singleton?.Shutdown(); }
}