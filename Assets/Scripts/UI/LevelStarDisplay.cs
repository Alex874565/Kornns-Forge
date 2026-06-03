using UnityEngine;
using UnityEngine.UI;

public class LevelStarDisplay : MonoBehaviour
{
    [SerializeField] private Image[] stars;

    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color inactiveColor = new Color(1,1,1,0.25f);


    public void SetStars(int amount)
    {
        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].color =
                i < amount ? activeColor : inactiveColor;
        }
    }
}