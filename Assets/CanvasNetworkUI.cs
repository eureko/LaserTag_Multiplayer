using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CanvasNetworkUI : MonoBehaviour
{
    public static CanvasNetworkUI Instance;

    [Header("Elementi UI (Trascina qui dal Canvas)")]
    public TMP_InputField nameInputField;
    public Button hostButton;
    public Button clientButton;
    public TextMeshProUGUI statusText;

    [Header("Contenitori e Telecamere")]
    public GameObject uiPanel; 
    public GameObject menuCamera;

    public string PlayerName { get; private set; } = "Anonimo";
	
	[Header("Scoreboard")]
	public GameObject playerListPanel; // Trascina qui il PlayerListPanel

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        hostButton.onClick.AddListener(StartHost);
        clientButton.onClick.AddListener(StartClient);
        nameInputField.onValueChanged.AddListener(UpdateName);

        statusText.text = "Stato: In attesa...";

        // Iscrizione agli eventi di rete di Unity per intercettare successi e fallimenti
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void OnDestroy()
    {
        // Rimuoviamo l'iscrizione agli eventi quando lo script viene distrutto
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void UpdateName(string newName)
    {
        if (!string.IsNullOrWhiteSpace(newName))
        {
            PlayerName = newName;
        }
    }

    private void StartHost()
    {
        statusText.text = "Avvio Host...";
        // L'Host si connette istantaneamente al proprio computer, quindi possiamo nascondere il menu subito
        if (NetworkManager.Singleton.StartHost())
        {
            NascondiMenu();
        }
    }

    private void StartClient()
    {
        statusText.text = "Ricerca Host in corso...";
        
        // Disabilitiamo temporaneamente i bottoni per evitare che l'utente clicchi mille volte
        hostButton.interactable = false;
        clientButton.interactable = false;

        // Avviamo il client, ma NON nascondiamo ancora il menu!
        NetworkManager.Singleton.StartClient();
    }

    // Questa funzione scatta SOLO se il client riesce a trovare l'Host e si collega
    private void OnClientConnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            NascondiMenu();
        }
    }

    // Questa funzione scatta se la connessione fallisce (es: socket chiuso, nessun Host trovato) o se l'Host si disconnette
    private void OnClientDisconnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId || clientId == 0)
        {
            // Niente schermo nero: riaccendiamo tutto, segnaliamo l'errore e sblocchiamo i pulsanti
            MostraMenu();
            statusText.text = "Errore: Nessun Host trovato!";
            hostButton.interactable = true;
            clientButton.interactable = true;
        }
    }

    private void NascondiMenu()
    {
        if (uiPanel != null) uiPanel.SetActive(false);
        if (menuCamera != null) menuCamera.SetActive(false);
		if (playerListPanel != null) playerListPanel.SetActive(true);
        // NOTA: Non usiamo più "this.enabled = false;" altrimenti lo script non potrebbe 
        // più ascoltare l'evento di disconnessione!
    }

    private void MostraMenu()
    {
        if (uiPanel != null) uiPanel.SetActive(true);
        if (menuCamera != null) menuCamera.SetActive(true);
    }
}