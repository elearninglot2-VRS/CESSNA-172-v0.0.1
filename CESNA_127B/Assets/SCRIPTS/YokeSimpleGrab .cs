using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(Rigidbody))]
public class YokeSimpleGrab : XRBaseInteractable
{
    [Header("Push / Pull Limit")]
    [SerializeField] private float maxPushPullDistance = 0.12f;
    [Header("Roll (rotation) Limit")]
    [SerializeField] private float maxRollAngle = 90f;
    [Tooltip("Stable reference for measuring roll/push axis. Defaults to this object's parent (YokeBase) if left empty.")]
    [SerializeField] private Transform axisReference;

    private IXRSelectInteractor currentInteractor;
    private Vector3 previousInteractorPosition;
    private Vector3 startPosition;
    private Quaternion startLocalRotation; // rotation of this object relative to axisReference, captured at Awake
    private float currentPushPull;
    private float currentRoll;
    private float rollOffset;

    // ---- NEW: normalized outputs for driving control surfaces ----
    // Range: -1 .. 1. Roll: negative = left, positive = right (matches your -currentRoll sign below).
    // Pitch: negative = pushed in, positive = pulled back.
    public float NormalizedRoll => maxRollAngle > 0f ? Mathf.Clamp(currentRoll / maxRollAngle, -1f, 1f) : 0f;
    public float NormalizedPushPull => maxPushPullDistance > 0f ? Mathf.Clamp(currentPushPull / maxPushPullDistance, -1f, 1f) : 0f;

    protected override void Awake()
    {
        base.Awake();
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        startPosition = transform.position;
        if (axisReference == null)
            axisReference = transform.parent; // YokeBase
        startLocalRotation = Quaternion.Inverse(axisReference.rotation) * transform.rotation;
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        currentInteractor = args.interactorObject;
        Transform attach = currentInteractor.GetAttachTransform(this);
        previousInteractorPosition = attach.position;
        float rawAngle = GetRawAngle(attach.position);
        rollOffset = rawAngle - currentRoll;
        Debug.Log("[YokeSimpleGrab] Grabbed yoke");
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        currentInteractor = null;
        Debug.Log("[YokeSimpleGrab] Released yoke");
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);
        if (updatePhase != XRInteractionUpdateOrder.UpdatePhase.Dynamic)
            return;
        if (currentInteractor == null)
            return;

        Transform attach = currentInteractor.GetAttachTransform(this);

        // --- Push / Pull ---
        Vector3 handDelta = attach.position - previousInteractorPosition;
        float movementAlongShaft = Vector3.Dot(handDelta, transform.forward);
        currentPushPull += movementAlongShaft;
        currentPushPull = Mathf.Clamp(currentPushPull, -maxPushPullDistance, maxPushPullDistance);
        transform.position = startPosition + transform.forward * currentPushPull;

        // --- Roll ---
        float rawAngle = GetRawAngle(attach.position);
        currentRoll = Mathf.Clamp(rawAngle - rollOffset, -maxRollAngle, maxRollAngle);
        transform.rotation = axisReference.rotation * startLocalRotation * Quaternion.Euler(0f, 0f, -currentRoll);

        previousInteractorPosition = attach.position;
    }

    private float GetRawAngle(Vector3 worldPos)
    {
        Vector3 local = axisReference.InverseTransformPoint(worldPos);
        return Mathf.Atan2(local.x, local.y) * Mathf.Rad2Deg;
    }
}