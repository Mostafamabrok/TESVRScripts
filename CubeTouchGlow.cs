using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRBaseInteractable))]
public class CubeTouchGlow : MonoBehaviour
{
    private XRBaseInteractable interactable;
    private Material materialInstance;
    private Color originalColor;
    private UserManager userManager;

    [Header("Glow Settings")]
    public Color hoverColor = Color.yellow;

    [Header("Cascade Settings")]
    public bool useCascading = true;
    public float cascadeDelay = 0.05f;

    [Header("Preview Control")]
    public bool isGroupController = false;  // one per group
    private bool isPreviewing = false;

    [Header("Hover Label")]
    public HoverLabel hoverLabel;           // assign your CanvasGroup label here

    void Awake()
    {
        interactable = GetComponent<XRBaseInteractable>();
        materialInstance = GetComponent<Renderer>().material;
        originalColor = materialInstance.color;
        userManager = FindObjectOfType<UserManager>();

        interactable.hoverEntered.AddListener(_ => HandleHover(true));
        interactable.hoverExited.AddListener(_ => HandleHover(false));
    }

    void OnDestroy()
    {
        interactable.hoverEntered.RemoveAllListeners();
        interactable.hoverExited.RemoveAllListeners();
    }

    void Start()
    {
        if (hoverLabel != null)
            hoverLabel.HideLabel();
    }

    void Update()
    {
        if (!isGroupController || userManager == null) return;

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha9))
        {
            isPreviewing = !isPreviewing;
            bool on = isPreviewing && !userManager.assembled;

            if (useCascading && transform.parent != null)
                StartCoroutine(CascadeGlow(transform.parent, on));
            else
                SetGlow(on);
        }
    }

    private void HandleHover(bool entered)
    {
        if (userManager != null && userManager.assembled) return;
        if (isPreviewing) return;

        bool on = entered;
        if (useCascading && transform.parent != null)
            StartCoroutine(CascadeGlow(transform.parent, on));
        else
            SetGlow(on);
    }

    private IEnumerator CascadeGlow(Transform parent, bool on)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            var child = parent.GetChild(i);
            var glow = child.GetComponent<CubeTouchGlow>();
            if (glow != null)
                glow.SetGlow(on);
            yield return new WaitForSeconds(cascadeDelay);
        }
    }

    /// <summary>
    /// Shows or hides glow and label, unless door is assembled.
    /// </summary>
    public void SetGlow(bool on)
    {
        if (userManager != null && userManager.assembled)
            on = false;

        materialInstance.color = on ? hoverColor : originalColor;
        if (hoverLabel != null)
        {
            if (on) hoverLabel.ShowLabel();
            else hoverLabel.HideLabel();
        }
    }
}
