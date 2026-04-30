using UnityEngine;
using TMPro;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance;
    public TextMeshProUGUI notificationText;
    public TMP_InputField nameInputField; // Trascina qui il PlayerNameInput
    
    [HideInInspector] public string localPlayerName;

    void Awake() 
    { 
        Instance = this; 
        DontDestroyOnLoad(gameObject); // Opzionale: mantiene il nome tra le scene
    }

    // Chiameremo questo metodo quando il giocatore preme "Start" o "Join"
    public void SaveName()
    {
        if (!string.IsNullOrEmpty(nameInputField.text))
        {
            localPlayerName = nameInputField.text;
        }
        else
        {
            localPlayerName = "Player_" + Random.Range(10, 99);
        }
    }

    public void ShowNotification(string message)
    {
        notificationText.text = message;
        CancelInvoke();
        Invoke("ClearText", 5f);
    }

    void ClearText() { notificationText.text = ""; }
}