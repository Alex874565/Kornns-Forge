using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Tutorial : MonoBehaviour
{
    // List of slide GameObjects (empty objects that contain text/images).
    // Assign in the inspector in the desired order.
    public List<GameObject> slides = new List<GameObject>();

    // Optional: name of the scene to load after the tutorial finishes.
    // If empty, the next build-index scene will be used when available.
    public string nextSceneName;

    // The transform under which the active slide will be parented for display.
    // If null, this GameObject's transform will be used.
    public Transform displayParent;

    [SerializeField] private GameObject initialButton;
    
    // Keep original parents so we can restore them when switching slides.
    private List<Transform> originalParents = new List<Transform>();

    private int currentIndex = -1;
    
    private void OnEnable()
    {
        // Capture original parents for every slide so we can restore them later.
        originalParents.Clear();
        foreach (var s in slides)
        {
            originalParents.Add(s != null ? s.transform.parent : null);
            if (s != null) s.SetActive(false);
        }

        if (slides.Count > 0)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                KornnGameManager.Instance.IsPaused.Value = true;
            }
            
            ShowSlide(0);
        }
    }

    // Call this from a UI Button onClick to advance the tutorial.
    public void Next()
    {
        if (slides.Count == 0) return;
        int next = currentIndex + 1;
        if (next >= slides.Count)
        {
            KornnGameManager.Instance.FinishTutorialServerRpc();
            return;
        }

        ShowSlide(next);
    }
    
    // Optional: call to go back one slide
    public void Previous()
    {
        if (slides.Count == 0) return;
        int prev = currentIndex - 1;
        if (prev < 0) return;
        ShowSlide(prev);
    }

    // Allow advancing with Space key, controller, arrows, or WASD
    void Update()
    {
        // Next: Space / Submit / primary joystick button / right, down, D, S
        if (Input.GetKeyDown(KeyCode.Space)
            || Input.GetButtonDown("Submit")
            || Input.GetKeyDown(KeyCode.JoystickButton0)
            || Input.GetKeyDown(KeyCode.RightArrow)
            || Input.GetKeyDown(KeyCode.DownArrow)
            || Input.GetKeyDown(KeyCode.D)
            || Input.GetKeyDown(KeyCode.S))
        {
            Next();
        }

        // Previous: Left, Up, A, W
        if (Input.GetKeyDown(KeyCode.LeftArrow)
            || Input.GetKeyDown(KeyCode.UpArrow)
            || Input.GetKeyDown(KeyCode.A)
            || Input.GetKeyDown(KeyCode.W))
        {
            Previous();
        }
    }

    // Reparents slides and toggles active state so only the current slide is visible.
    public void ShowSlide(int index)
    {
        if (index < 0 || index >= slides.Count) return;

        // Hide and restore previous slide
        if (currentIndex >= 0 && currentIndex < slides.Count)
        {
            var prev = slides[currentIndex];
            if (prev != null)
            {
                prev.SetActive(false);
                var orig = originalParents[currentIndex];
                prev.transform.SetParent(orig, false);
            }
        }

        // Show and parent the new slide under displayParent
        var slide = slides[index];
        if (slide != null)
        {
            var parent = displayParent != null ? displayParent : transform;
            slide.transform.SetParent(parent, false);
            slide.SetActive(true);
        }

        currentIndex = index;
    }
}
