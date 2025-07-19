using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class DoorTouchTrigger : MonoBehaviour
{
    public DoorHingeRotation linkedDoor; // Assign in Inspector

    private void Awake()
    {
        XRBaseInteractable interactable = GetComponent<XRBaseInteractable>();
        if (interactable == null)
        {
            interactable = gameObject.AddComponent<XRSimpleInteractable>();
        }

        interactable.hoverEntered.AddListener(OnTouched);
    }

    private void OnTouched(HoverEnterEventArgs args)
    {
        if (linkedDoor != null)
        {
            linkedDoor.ToggleDoor();
        }
    }
}
