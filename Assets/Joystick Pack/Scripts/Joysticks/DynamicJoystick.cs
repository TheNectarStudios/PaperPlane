using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DynamicJoystick : Joystick
{
    public float MoveThreshold { get { return moveThreshold; } set { moveThreshold = Mathf.Abs(value); } }

    [SerializeField] private float moveThreshold = 1;

    protected override void Start()
    {
        MoveThreshold = moveThreshold;
        base.Start();
        background.gameObject.SetActive(false); // Hide joystick background initially
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        // Set the background position once when the player touches the screen
        background.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);
        background.gameObject.SetActive(true); // Show joystick background
        base.OnPointerDown(eventData); // Call the base function
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        background.gameObject.SetActive(false); // Hide joystick background when touch ends
        base.OnPointerUp(eventData);
    }

    protected override void HandleInput(float magnitude, Vector2 normalised, Vector2 radius, Camera cam)
    {
        // Keep the joystick background in place without adjusting its position
        base.HandleInput(magnitude, normalised, radius, cam);
    }
}
