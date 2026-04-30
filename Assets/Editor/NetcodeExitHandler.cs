using UnityEditor;
using Unity.Netcode;

[InitializeOnLoad]
public static class NetcodeExitHandler
{
    static NetcodeExitHandler()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        // Se l'utente sta premendo "STOP" (uscendo dal Play Mode)
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            {
                // Spegniamo il NetworkManager in modo pulito PRIMA che Unity distrugga gli oggetti
                NetworkManager.Singleton.Shutdown();
            }
        }
    }
}