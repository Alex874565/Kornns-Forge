using UnityEngine;

public class OptionsMenu : MonoBehaviour
{
    public GameObject mainMenu;

    public void OnBackButton()
    {
        SoundManager.PlaySound(SoundType.ButtonClick);

        mainMenu.SetActive(true); 
        gameObject.SetActive(false);
    }
}