using UnityEngine;

public class UIWarning : MonoBehaviour
{
    private float timer;
    private float duration;

    public void Show(float seconds)
    {
        duration = seconds;
        timer = 0f;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= duration)
        {
            ObjectPooler.Instance.ReturnToPool("UIWarning", gameObject);
        }
    }
}
