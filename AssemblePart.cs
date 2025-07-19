using UnityEngine;

public class AssemblePart : MonoBehaviour
{
    [Header("Animation Settings")]
    public Vector3 offset = new Vector3(0, 50f, -100f);
    public float delay = 0.2f;
    public float duration = 2f;
    public float pitchVariance = 0.1f;

    [Header("Hierarchy Offset (optional)")]
    public bool useHierarchyOffset = false;
    public float hierarchyOffsetIncrement = 0.2f;

    [Header("Startup")]
    [Tooltip("If true, this part begins in the assembled position")]
    public bool startAssembled = false;

    private Vector3 startPos;
    private Vector3 endPos;
    private float timer;
    private bool animating;
    private bool assembleTarget;
    private bool triggered;
    private bool isAssembled;
    private AudioSource audioSource;

    void Start()
    {
        // 1) Record the assembled (end) position in local space
        endPos = transform.localPosition;

        // 2) Calculate the disassembled position
        Vector3 effOffset = offset;
        if (useHierarchyOffset)
        {
            Vector3 dir = offset.normalized;
            float extra = hierarchyOffsetIncrement * transform.GetSiblingIndex();
            effOffset += dir * extra;
        }
        startPos = endPos + effOffset;

        // 3) Place according to startAssembled
        isAssembled = startAssembled;
        assembleTarget = startAssembled;
        transform.localPosition = isAssembled ? endPos : startPos;

        // 4) Cache audio
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (!animating) return;

        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / duration);
        float eased = t * t * (3f - 2f * t);

        Vector3 from = assembleTarget ? startPos : endPos;
        Vector3 to = assembleTarget ? endPos : startPos;
        transform.localPosition = Vector3.Lerp(from, to, eased);

        if (t >= 1f)
        {
            animating = false;
            triggered = false;
            isAssembled = assembleTarget;

            // Play sound only when we just assembled
            if (assembleTarget && audioSource != null)
            {
                audioSource.pitch = 1f + Random.Range(-pitchVariance, pitchVariance);
                audioSource.Play();
            }
        }
    }

    /// <summary>
    /// Call with true to assemble, false to disassemble.
    /// Skips if already in that state.
    /// </summary>
    public void Run(bool assemble)
    {
        // 1) Do nothing if we're already animating or in the target state
        if (triggered || animating || assemble == isAssembled)
            return;

        // 2) Schedule the animation
        triggered = true;
        assembleTarget = assemble;
        Invoke(nameof(Begin), delay);
    }

    private void Begin()
    {
        timer = 0f;
        animating = true;
    }
}
