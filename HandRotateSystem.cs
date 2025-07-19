using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Management;

public class HandDraggableFullRotation : MonoBehaviour
{
    [Header("Hand Tracking")]
    public XRNode whichXRNode = XRNode.RightHand;      // Choose LeftHand or RightHand
    public float pinchThreshold = 0.025f;              // meters
    public float rotationSensitivity = 800f;           // degrees per meter of hand travel

    private XRHandSubsystem handSubsystem;
    private bool isPinching;
    private Vector3 lastHandPos;
    private bool dragging;

    void Start()
    {
        // Find the active XRHandSubsystem
        handSubsystem = XRGeneralSettings.Instance?.Manager?.activeLoader?
            .GetLoadedSubsystem<XRHandSubsystem>();

        if (handSubsystem == null)
            Debug.LogError("HandDraggableFullRotation: No XRHandSubsystem found. Ensure XR Hands is enabled.");
    }

    void Update()
    {
        if (handSubsystem == null)
            return;

        // Select the correct hand
        XRHand hand = (whichXRNode == XRNode.RightHand)
            ? handSubsystem.rightHand
            : handSubsystem.leftHand;

        if (!hand.isTracked)
        {
            dragging = false;
            return;
        }

        // Get thumb & index joints
        XRHandJoint thumb = hand.GetJoint(XRHandJointID.ThumbTip);
        XRHandJoint index = hand.GetJoint(XRHandJointID.IndexTip);

        // Try to fetch their poses
        if (!thumb.TryGetPose(out Pose thumbPose) ||
            !index.TryGetPose(out Pose indexPose))
        {
            dragging = false;
            return;
        }

        // Determine pinch
        float pinchDist = Vector3.Distance(thumbPose.position, indexPose.position);
        isPinching = pinchDist < pinchThreshold;

        if (isPinching)
        {
            Vector3 currentHandPos = indexPose.position;

            if (!dragging)
            {
                lastHandPos = currentHandPos;
                dragging = true;
                return;
            }

            Vector3 delta = currentHandPos - lastHandPos;
            // Project movement onto camera axes for intuitive control
            float horiz = Vector3.Dot(delta, Camera.main.transform.right);
            float vert = Vector3.Dot(delta, Camera.main.transform.up);

            // Rotate around world axes
            transform.Rotate(Vector3.up, -horiz * rotationSensitivity * Time.deltaTime, Space.World);
            transform.Rotate(Camera.main.transform.right, vert * rotationSensitivity * Time.deltaTime, Space.World);

            lastHandPos = currentHandPos;
        }
        else
        {
            dragging = false;
        }
    }
}
