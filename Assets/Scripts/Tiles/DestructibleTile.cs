using UnityEngine;
using Unity.Netcode;

public class DestructibleTile : NetworkBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 3;

    private NetworkVariable<int> netHealth = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("Crack Stages")]
    [Tooltip("Ordered crack sprites: first = light crack, last = heavy crack before break")]
    [SerializeField] private Sprite[] crackStages;
    private SpriteRenderer spriteRenderer;

    [Header("Respawn")]
    [Tooltip("Time in seconds before the tile respawns after being destroyed")]
    [SerializeField] private float respawnDelay = 5f;

    private Collider2D coll;

    public override void OnNetworkSpawn()
    {
        spriteRenderer = GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>();
        coll = GetComponent<Collider2D>();

        if (IsServer)
        {
            netHealth.Value = maxHealth;
            UpdateCrackSprite();
        }

        netHealth.OnValueChanged += OnHealthChanged;
    }

    private void OnDestroy()
    {
        netHealth.OnValueChanged -= OnHealthChanged;
    }

    // Client/other code should call this to request damage; server applies it.
    public void RequestDamage(int amount = 1)
    {
        // If running as server or Netcode is not active, apply immediately.
        if (IsServer || NetworkManager.Singleton == null)
        {
            ApplyDamageInternal(amount);
            return;
        }

        // If this NetworkBehaviour hasn't been spawned yet, avoid calling ServerRpc
        // (ServerRpc requires the object to be spawned). Fall back to local apply.
        if (!IsSpawned)
        {
            ApplyDamageInternal(amount);
            return;
        }

        // Normal networked path: request server to apply damage
        ApplyDamageServerRpc(amount);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ApplyDamageServerRpc(int amount)
    {
        ApplyDamageInternal(amount);
    }

    private void ApplyDamageInternal(int amount)
    {
        if (netHealth.Value <= 0) return;
        netHealth.Value = Mathf.Max(0, netHealth.Value - amount);

        if (netHealth.Value <= 0 && IsServer)
        {
            // notify all clients to play break visuals
            BreakClientRpc();
            // server-side also run break visuals
            BreakLocal();

            // schedule respawn
            StartCoroutine(RespawnRoutine());
        }
    }

    private void OnHealthChanged(int oldValue, int newValue)
    {
        UpdateCrackSprite();
        if (newValue <= 0 && !IsServer)
        {
            // clients handle visuals when server indicates destruction
            BreakLocal();
        }
    }

    private void UpdateCrackSprite()
    {
        int current = netHealth.Value;
        if (crackStages == null || crackStages.Length == 0 || spriteRenderer == null) return;

        float t = 1f - (float)current / Mathf.Max(1, maxHealth);
        int index = Mathf.Clamp(Mathf.FloorToInt(t * crackStages.Length), 0, crackStages.Length - 1);
        spriteRenderer.sprite = crackStages[index];
    }

    [ClientRpc]
    private void BreakClientRpc()
    {
        BreakLocal();
    }

    private void BreakLocal()
    {
        // disable visuals/collision (tile disappears)
        if (coll != null) coll.enabled = false;
        if (spriteRenderer != null) spriteRenderer.enabled = false;
    }

    private System.Collections.IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);

        // reset health on server
        netHealth.Value = maxHealth;

        // re-enable locally on server
        RespawnLocal();

        // notify clients to respawn visuals
        RespawnClientRpc();
    }

    private void RespawnLocal()
    {
        if (coll != null) coll.enabled = true;
        if (spriteRenderer != null) spriteRenderer.enabled = true;
        UpdateCrackSprite();
    }

    [ClientRpc]
    private void RespawnClientRpc()
    {
        RespawnLocal();
    }

    // Convenience editor/test method
    public void ResetTile()
    {
        if (IsServer)
        {
            netHealth.Value = maxHealth;
        }
        else if (NetworkManager.Singleton == null)
        {
            // non-networked fallback: directly update network variable locally
            netHealth.Value = maxHealth;
        }

        if (spriteRenderer != null && crackStages != null && crackStages.Length > 0)
            spriteRenderer.sprite = crackStages[0];
        if (coll != null) coll.enabled = true;
        if (spriteRenderer != null) spriteRenderer.enabled = true;
    }
}
