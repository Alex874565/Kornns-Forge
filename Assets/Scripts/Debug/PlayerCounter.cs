using UnityEngine;
using Unity.Netcode;

public class PlayerCounter : MonoBehaviour
{
    private void Update()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length > 0)
        {
            // Debug.Log($"[PlayerCounter] Found {players.Length} players.");
        }
    }
}
