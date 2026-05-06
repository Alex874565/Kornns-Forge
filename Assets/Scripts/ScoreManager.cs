using Unity.Netcode;

public class ScoreManager : NetworkBehaviour
{
    public static ScoreManager Instance;

    private NetworkVariable<int> score = new NetworkVariable<int>(0);

    private void Awake()
    {
        Instance = this;
    }

    public void AddScore(int points)
    {
        if (!IsServer) return;

        score.Value += points;
    }

    public int GetScore()
    {
        return score.Value;
    }

    public NetworkVariable<int> GetScoreVariable()
    {
        return score;
    }
}