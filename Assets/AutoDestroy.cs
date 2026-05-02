using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    [Tooltip("Quanti secondi deve durare prima di essere cancellato?")]
    public float tempoDiVita = 2f; // 2 secondi di default

    void Start()
    {
        // Il comando Destroy elimina definitivamente questo oggetto dalla scena dopo 'X' secondi
        Destroy(gameObject, tempoDiVita);
    }
}