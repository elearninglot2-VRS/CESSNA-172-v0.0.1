using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

/// <summary>
/// Drives a single rudder pedal (placeholder cube) using an XR controller trigger's
/// analog value. Fully pressed = full travel, released = spring-returns to rest.
/// Attach one of these to each pedal object (or to a manager and assign pedalTransform).
/// </summary>
[DisallowMultipleComponent]
public class RudderPedalController : MonoBehaviour
{
    [Header("Input")]
    [Tooltip("The trigger's analog Value action, e.g. XRI LeftHand/Activate Value or a custom Rudder action bound to <XRController>/trigger")]
    public InputActionReference triggerAction;

    [Header("Pedal Transform")]
    [Tooltip("The cube/mesh representing this pedal. Defaults to this GameObject's transform.")]
    public Transform pedalTransform;

    [Header("Movement")]
    [Tooltip("Local-space direction the pedal travels when the trigger is pressed. Use Vector3.forward if the pedal's " +
             "local +Z points away from the pilot (pedal moves away when pressed, springs back toward the pilot on release). " +
             "Flip to Vector3.back if it moves the wrong way.")]
    public Vector3 pressAxis = Vector3.forward;

    [Tooltip("Max travel distance in meters when the trigger is fully pressed")]
    public float travelDistance = 0.08f;

    [Tooltip("How quickly the pedal moves toward its target position each frame. Higher = snappier/less lag.")]
    public float responseSpeed = 15f;

    [Range(0f, 1f)]
    [Tooltip("Trigger values below this are treated as 0, to avoid drift or controller jitter holding the pedal slightly off-rest")]
    public float deadzone = 0.02f;

    [Header("Optional: Haptics")]
    [Tooltip("If set, pulses the controller briefly when the pedal bottoms out (value reaches ~1)")]
    public bool hapticsOnFullPress = false;
    [Tooltip("Assign the HapticImpulsePlayer on this pedal's controller (found on the LeftHand/RightHand Controller GameObject in the XR Origin), only needed if Haptics On Full Press is checked")]
    public HapticImpulsePlayer hapticImpulsePlayer;
    [Range(0f, 1f)] public float hapticAmplitude = 0.3f;
    public float hapticDuration = 0.05f;

    /// <summary>Current trigger value after deadzone, 0-1. Read this from other scripts (e.g. flight controls).</summary>
    [HideInInspector] public float currentValue;

    /// <summary>
    /// When true, this pedal's own trigger no longer drives its position directly -
    /// a RudderInputSystem with Use Mechanical Linkage enabled is positioning it instead,
    /// based on the combined left/right value, just like the real connected pedal mechanism.
    /// </summary>
    [HideInInspector] public bool linked;

    Vector3 restPosition;
    bool firedHapticThisPress;

    void Awake()
    {
        if (pedalTransform == null) pedalTransform = transform;
        restPosition = pedalTransform.localPosition;
    }

    void OnEnable()
    {
        if (triggerAction != null && triggerAction.action != null)
            triggerAction.action.Enable();
    }

    void OnDisable()
    {
        if (triggerAction != null && triggerAction.action != null)
            triggerAction.action.Disable();
    }

    void Update()
    {
        float raw = (triggerAction != null && triggerAction.action != null)
            ? triggerAction.action.ReadValue<float>()
            : 0f;

        currentValue = raw < deadzone ? 0f : raw;

        if (!linked)
        {
            MoveToward(currentValue);
        }

        if (hapticsOnFullPress && hapticImpulsePlayer != null)
        {
            if (currentValue > 0.97f && !firedHapticThisPress)
            {
                hapticImpulsePlayer.SendHapticImpulse(hapticAmplitude, hapticDuration);
                firedHapticThisPress = true;
            }
            else if (currentValue < 0.9f)
            {
                firedHapticThisPress = false;
            }
        }
    }

    /// <summary>
    /// Smoothly moves the pedal to restPosition + pressAxis * travelDistance * normalizedValue.
    /// normalizedValue is typically 0-1 for independent mode, or -1 to 1 when driven by
    /// RudderInputSystem's mechanical linkage (negative = pulled back past rest).
    /// </summary>
    public void MoveToward(float normalizedValue)
    {
        Vector3 targetPosition = restPosition + pressAxis.normalized * (travelDistance * normalizedValue);

        // Exponential smoothing - frame-rate independent, feels like a sprung pedal
        pedalTransform.localPosition = Vector3.Lerp(
            pedalTransform.localPosition,
            targetPosition,
            1f - Mathf.Exp(-responseSpeed * Time.deltaTime));
    }
}
