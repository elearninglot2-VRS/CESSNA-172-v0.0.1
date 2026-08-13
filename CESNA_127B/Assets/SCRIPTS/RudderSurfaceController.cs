using UnityEngine;

/// <summary>
/// Rotates the rudder control surface (the hinged trailing-edge panel on the vertical
/// stabilizer) based on the combined pedal input from RudderInputSystem.
///
/// Real Cessna 172 rudder travel is roughly +/-16-17 degrees from neutral, so that's
/// the default here - tune maxDeflectionDegrees if your model/POH reference differs.
///
/// Attach this to the rudder's PIVOT object (an empty GameObject sitting exactly on the
/// hinge line, with the rudder mesh parented under it) - see setup notes below.
/// </summary>
[DisallowMultipleComponent]
public class RudderSurfaceController : MonoBehaviour
{
    [Header("Input Source")]
    [Tooltip("The RudderInputSystem that combines left/right pedal values into -1..+1")]
    public RudderInputSystem rudderInputSystem;

    [Header("Hinge")]
    [Tooltip("Local axis this pivot rotates around. The rudder hinges about a roughly " +
             "vertical line at the back of the fin, so this is normally Vector3.up. " +
             "If your model's pivot is oriented differently, change this instead of the mesh.")]
    public Vector3 hingeAxis = Vector3.up;

    [Tooltip("Flip if right pedal visually swings the rudder the wrong way")]
    public bool invert = false;

    [Header("Deflection")]
    [Tooltip("Max rudder deflection in degrees at full pedal, one side. C172 POH-ish default: ~16-17 deg.")]
    public float maxDeflectionDegrees = 16.5f;

    [Header("Feel")]
    [Tooltip("How quickly the surface tracks the target angle. Higher = snappier/less lag. " +
             "Keep this a bit slower than the pedal responseSpeed - the actual control surface " +
             "has aerodynamic/cable lag the pedal cube doesn't need to bother simulating.")]
    public float responseSpeed = 10f;

    /// <summary>Current commanded deflection in degrees, -max..+max. Positive = right rudder.</summary>
    [HideInInspector] public float currentDeflection;

    Quaternion restLocalRotation;
    float smoothedInput;

    void Awake()
    {
        restLocalRotation = transform.localRotation;
    }

    void LateUpdate()
    {
        if (rudderInputSystem == null) return;

        // rudderInput is -1 (full left) .. +1 (full right) from RudderInputSystem
        float target = rudderInputSystem.rudderInput * (invert ? -1f : 1f);

        smoothedInput = Mathf.Lerp(smoothedInput, target, 1f - Mathf.Exp(-responseSpeed * Time.deltaTime));

        currentDeflection = smoothedInput * maxDeflectionDegrees;

        // Rotate away from rest pose about the hinge axis. Positive deflection = right rudder,
        // which is a positive rotation about +Y in Unity's left-handed system when viewed from above.
        Quaternion deflectionRotation = Quaternion.AngleAxis(currentDeflection, hingeAxis.normalized);
        transform.localRotation = restLocalRotation * deflectionRotation;
    }
}
