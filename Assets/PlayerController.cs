using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private GameObject targetPrefab;
	public float moveSpeed = 10f;

  // [SerializeField] private float rotationSpeed = 1.0f; // Velocità di rotazione

private Vector3 lastLookDirection = Vector3.forward; // Aggiungi questa variabile
[SerializeField] private GameObject playerCanvas;

// 1. Aggiungi il riferimento all'AudioSource in cima alla classe
[SerializeField] private AudioSource laserSound;


void Start()
{
    if (!IsServer) return;

    // Aggiungi questo log per vedere cosa succede
    if (targetPrefab == null)
    {
        Debug.LogError("ERRORE: targetPrefab è NULL! Controlla l'Inspector del Player.");
        return;
    }

   
        SpawnTargets(50);
    
}

private void SpawnTargets(int count)
{
    for (int i = 0; i < count; i++)
    {
        // Genera una posizione casuale nell'area di gioco
        Vector3 randomPos = new Vector3(Random.Range(-20, 20), 0.5f, Random.Range(-20, 20));
        
        // Crea l'istanza dell'oggetto
        GameObject targetInstance = Instantiate(targetPrefab, randomPos, Quaternion.identity);
        
        // Fondamentale: Spawnarlo sulla rete affinché tutti i giocatori lo vedano
        targetInstance.GetComponent<NetworkObject>().Spawn();
    }
}


void Update()
{
    if (!IsOwner) return;

    // Usiamo GetAxisRaw per avere una risposta immediata (0 o 1, senza scivolamento)
    float rotationInput = Input.GetAxisRaw("Horizontal"); 
    float forwardInput = Input.GetAxisRaw("Vertical");

    // 2. RUOTAZIONE: Ruota il cubo sul posto (Asse Y)
    // 15 gradi per secondo (moltiplicato per l'input)
    //float rotationAmount = rotationInput * rotationSpeed;
    if (rotationInput != 0) 
    {
		transform.Rotate(0, rotationInput* 1.0f, 0);
	}

    // 3. MOVIMENTO: Sposta il cubo in avanti o indietro rispetto a dove guarda
    // Usiamo transform.forward (la direzione verso cui il cubo punta ora)
    Vector3 movement = transform.forward * forwardInput * moveSpeed * Time.deltaTime;
    transform.Translate(movement, Space.World);

   // 4. SPARO: Usiamo sempre la direzione attuale del cubo
    if (Input.GetKeyDown(KeyCode.Space))
    {
        if (laserSound != null) laserSound.Play();

        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;
        Vector3 adjustedOrigin = origin + (direction * 1.5f);
        Vector3 endPoint = adjustedOrigin + (direction * 500f);

        // Simulazione locale dell'impatto (Lag = 0)
        if (Physics.Raycast(adjustedOrigin, direction, out RaycastHit hit, 50f))
        {
            endPoint = hit.point;

            // -- NOVITÀ: DISTRUZIONE SIMULATA --
            if (hit.collider.CompareTag("Target"))
            {
                // 1. Spawna le particelle sul tuo schermo all'istante
                if (impactEffectPrefab != null)
                {
                    Instantiate(impactEffectPrefab, hit.point, Quaternion.identity);
                }

                // 2. Rendi il bersaglio invisibile e "fantasma" all'istante! 
                // (Il Server lo distruggerà per davvero fra qualche millisecondo)
                if (hit.collider.TryGetComponent<MeshRenderer>(out var mesh)) mesh.enabled = false;
                if (hit.collider.TryGetComponent<Collider>(out var col)) col.enabled = false;
            }
        }

        // Il Client disegna il SUO raggio istantaneamente
        StartCoroutine(DrawLaserRoutine(adjustedOrigin, endPoint));

        // Diciamo al Server di fare i calcoli "veri"
        ShootServerRpc(origin, direction);
    }
}


[SerializeField] private LineRenderer laserLine; // Trascina qui il componente LineRenderer nell'Inspector


[SerializeField] private GameObject impactEffectPrefab; // Trascina qui il tuo Prefab di particelle

[ServerRpc]
private void ShootServerRpc(Vector3 origin, Vector3 direction)
{
    Vector3 adjustedOrigin = origin + (direction * 1.5f);
    Vector3 endPoint = adjustedOrigin + (direction * 500f);
    
    // Controlliamo il colpo vero (quello che fa i danni)
    if (Physics.Raycast(adjustedOrigin, direction, out RaycastHit hit, 50f))
    {
        endPoint = hit.point;

        // 1. Gestione colpo su altri giocatori
        if (hit.collider.TryGetComponent<NetworkObject>(out var netObj) && netObj.OwnerClientId != OwnerClientId)
        {
            NotifyHitClientRpc(netObj.OwnerClientId);
        }

        // 2. Gestione colpo su bersagli (Target)
        if (hit.collider.CompareTag("Target"))
        {
            // Diciamo a tutti gli ALTRI di far apparire l'esplosione
            ShowHitEffectClientRpc(hit.point);
            
            // Distruzione ufficiale del bersaglio sul server
            hit.collider.GetComponent<NetworkObject>().Despawn();
        }
    }

    // Chiamiamo il client per far vedere il raggio a tutti gli ALTRI giocatori
    ShowLaserClientRpc(adjustedOrigin, endPoint);
}

[ClientRpc]
private void ShowHitEffectClientRpc(Vector3 position)
{
	// Se ho sparato IO, ignoro il comando perché ho già creato l'effetto nell'Update!
	if (IsOwner) return;

	// Se sono un altro giocatore, faccio nascere le particelle nel punto dell'impatto
	if (impactEffectPrefab != null)
	{
		Instantiate(impactEffectPrefab, position, Quaternion.identity);
	}
}
	

[ClientRpc]
private void ShowLaserClientRpc(Vector3 start, Vector3 end)
{
    // IMPORTANTISSIMO: Se sono io il proprietario del cubo che ha sparato, 
    // ignoro questo messaggio perché ho GIA' disegnato il laser nell'Update!
    if (IsOwner) return;

    // Se sono un altro giocatore che osserva, disegno il laser di chi ha sparato
    StartCoroutine(DrawLaserRoutine(start, end));
}

private System.Collections.IEnumerator DrawLaserRoutine(Vector3 start, Vector3 end)
{
    laserLine.enabled = true;
    
    // Aggiungiamo un piccolo offset verso l'alto (0.1f) 
    // per evitare che si sovrapponga al pavimento
    Vector3 offset = Vector3.up * 0.1f; 
    
    laserLine.SetPosition(0, start + offset);
    laserLine.SetPosition(1, end + offset);
    
    yield return new WaitForSeconds(0.1f);
    laserLine.enabled = false;
}

    [ClientRpc]
    private void NotifyHitClientRpc(ulong victimClientId)
    {
        if (NetworkManager.Singleton.LocalClientId == victimClientId)
        {
            Debug.Log("Sono stato colpito!");
            // Qui chiamerai la Coroutine per il feedback visivo
        }
    }
	public override void OnNetworkSpawn()
	{
		// Recuperiamo la camera che abbiamo appena messo dentro il Prefab
		Camera miaCamera = GetComponentInChildren<Camera>();

		if (IsOwner)
		{
			// Se questo è il MIO cubo, attiva la camera
			miaCamera.enabled = true;
			
			// Se la camera ha un AudioListener, attiva anche quello solo per me
			if(miaCamera.TryGetComponent<AudioListener>(out var listener))
				listener.enabled = true;
			
			// ATTIVA IL MIRINO
			if (playerCanvas != null) playerCanvas.SetActive(true);
			
			Cursor.lockState = CursorLockMode.Locked;

				// 1. Diamo un nome di base per sicurezza
			string nomeDaInviare = "Player_" + OwnerClientId; 

			// 2. Chiediamo al nuovo script il nome inserito dall'utente
			if (CanvasNetworkUI.Instance != null)
			{
				nomeDaInviare = CanvasNetworkUI.Instance.PlayerName;
			}

			// 3. Lo inviamo al server!
			AnnounceJoinServerRpc(nomeDaInviare);

		}
		else
		{
			// Se è il cubo di un altro, spegni la sua camera sul mio schermo
			miaCamera.enabled = false;
			
			if(miaCamera.TryGetComponent<AudioListener>(out var listener))
				listener.enabled = false;
			
			// DISATTIVA IL MIRINO PER GLI ALTRI
			if (playerCanvas != null) playerCanvas.SetActive(false);
		}
	}
	
	[ServerRpc]
    public void AnnounceJoinServerRpc(string nome)
    {
        // Il giocatore appena entrato invia il suo nome al Server.
        // Il Server chiama subito il ClientRpc per avvisare tutti gli altri.
        AnnounceJoinClientRpc(nome);
    }

    [ClientRpc]
    private void AnnounceJoinClientRpc(string nome)
    {
        // Questo pezzo di codice scatta in contemporanea sui PC di TUTTI i giocatori.
        
        // 1. Lo scriviamo in console per sicurezza
        Debug.Log("<color=green>NUOVO GIOCATORE: </color>" + nome + " è entrato nell'arena!");

        // 2. Se hai mantenuto il tuo NotificationManager, togli i commenti (//) 
        // dalle righe qui sotto per far apparire la scritta a schermo:
        
        // if (NotificationManager.Instance != null)
        // {
        //     NotificationManager.Instance.ShowNotification(nome + " è entrato in partita!");
        // }
    }
	
	
}