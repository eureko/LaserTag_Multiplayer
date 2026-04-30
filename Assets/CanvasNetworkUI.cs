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
    public GameObject menuCamera; // <-- Ecco la variabile che deve apparire!

    public string PlayerName { get; private set; } = "Anonimo";

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
        NetworkManager.Singleton.StartHost();
        NascondiMenu();
    }

    private void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        NascondiMenu();
    }

    private void NascondiMenu()
    {
        // 1. Spegne l'interfaccia visiva
        if (uiPanel != null) uiPanel.SetActive(false);
        
        // 2. Spegne finalmente la telecamera doppia!
        if (menuCamera != null) menuCamera.SetActive(false);
        
        // 3. Disattiva questo script per far respirare la CPU
        this.enabled = false; 
    }
}