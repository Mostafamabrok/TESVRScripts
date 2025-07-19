using UnityEngine;
using UnityEngine.SceneManagement;

public class UserManager : MonoBehaviour
{
    public bool assembled = true;
    private bool doorOpen = false;
    private FadeController fadeController;
    private HierarchicalLightActivator lightActivator;

    void Start()
    {
        fadeController = FindObjectOfType<FadeController>();
        lightActivator = FindObjectOfType<HierarchicalLightActivator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            assembled = !assembled;
            TriggerAssemblePhase();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            doorOpen = !doorOpen;
            TriggerDoorSwing();
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            if (fadeController != null)
                fadeController.FadeAndReloadScene();
            else
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SkyAssembleOrdered.TriggerAssembleAll();
            Debug.Log("[SkyAssemble] Triggered sky‐assembly sequence.");
        }

        // 🔆 Trigger light sequence with Key 6
        if (Input.GetKeyDown(KeyCode.Alpha6) && lightActivator != null)
        {
            lightActivator.StartCoroutine("ActivateLightsInOrder");
            Debug.Log("[LightSequence] Light activation triggered.");
        }
    }

    void TriggerAssemblePhase()
    {
        AssemblePart[] parts = FindObjectsOfType<AssemblePart>();
        foreach (var part in parts)
            part.Run(assembled);

        Debug.Log($"[Assemble] {(assembled ? "Assembling" : "Disassembling")} parts.");
    }

    void TriggerDoorSwing()
    {
        DoorHingeRotation[] doors = FindObjectsOfType<DoorHingeRotation>();
        foreach (var door in doors)
            door.ToggleDoor();

        Debug.Log($"[Door] {(doorOpen ? "Opened" : "Closed")} doors.");
    }
}
