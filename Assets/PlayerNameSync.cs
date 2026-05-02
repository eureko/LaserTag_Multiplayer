using Unity.Netcode;
using Unity.Collections; // Necessario per le stringhe speciali

public class PlayerNameSync : NetworkBehaviour
{
    // Variabile di rete sincronizzata: tutti possono leggerla, solo il proprietario può scriverla
    public NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>(
        "Anonimo", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // Quando nasciamo, prendiamo il nome salvato nel nostro UIManager e lo scriviamo in rete
            playerName.Value = CanvasNetworkUI.Instance.PlayerName;
        }
    }
}