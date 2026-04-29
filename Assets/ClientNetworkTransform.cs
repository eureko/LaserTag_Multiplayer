using Unity.Netcode.Components;
using UnityEngine;

// Questo script eredita dal NetworkTransform di base
// ma sovrascrive il metodo di verifica dell'autorità.
public class ClientNetworkTransform : NetworkTransform
{
    protected override bool OnIsServerAuthoritative()
    {
        return false; // Diciamo al sistema: "Non essere server-authoritative"
    }
}