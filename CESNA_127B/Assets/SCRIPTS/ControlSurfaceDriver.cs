using UnityEngine;

/// <summary>
/// Drives 4 control-surface cubes (left/right aileron, left/right elevator)
/// from a YokeSimpleGrab's normalized roll (-1..1) and pitch (-1..1) values.
///
/// Ailerons deflect OPPOSITE each other (roll right -> right aileron up, left aileron down).
/// Elevators deflect TOGETHER (pull back -> both elevators up / nose up).
///
/// Each surface cube must be parented under a pivot Transform placed at its
/// real-world hinge line, so rotating the pivot swings the cube like a hinged panel.
/// </summary>
public class ControlSurfaceDriver : MonoBehaviour
{
    [Header("Source")]
    [SerializeField] private YokeSimpleGrab yoke;

    [Header("Ailerons (Roll)")]
    [Tooltip("Pivot transform at the aileron's hinge line, cube parented underneath.")]
    [SerializeField] private Transform leftAileronPivot;
    [SerializeField] private Transform rightAileronPivot;
    [SerializeField] private float maxAileronDeflection = 20f;
    [Tooltip("Local axis the pivot rotates around. Usually the hinge line direction (often local X or Z).")]
    [SerializeField] private Vector3 aileronRotationAxis = Vector3.right;
    [Tooltip("Flip if aileron moves the wrong way for a given roll direction.")]
    [SerializeField] private bool invertAilerons = false;

    [Header("Elevators (Pitch)")]
    [SerializeField] private Transform leftElevatorPivot;
    [SerializeField] private Transform rightElevatorPivot;
    [SerializeField] private float maxElevatorDeflection = 20f;
    [SerializeField] private Vector3 elevatorRotationAxis = Vector3.right;
    [Tooltip("Flip if elevator moves the wrong way when pulling/pushing the yoke.")]
    [SerializeField] private bool invertElevators = false;

    private Quaternion leftAileronStart, rightAileronStart, leftElevatorStart, rightElevatorStart;

    private void Awake()
    {
        if (leftAileronPivot) leftAileronStart = leftAileronPivot.localRotation;
        if (rightAileronPivot) rightAileronStart = rightAileronPivot.localRotation;
        if (leftElevatorPivot) leftElevatorStart = leftElevatorPivot.localRotation;
        if (rightElevatorPivot) rightElevatorStart = rightElevatorPivot.localRotation;
    }

    private void LateUpdate()
    {
        if (yoke == null) return;

        float roll = yoke.NormalizedRoll * (invertAilerons ? -1f : 1f);
        float pitch = yoke.NormalizedPushPull * (invertElevators ? -1f : 1f);

        if (leftAileronPivot)
            leftAileronPivot.localRotation = leftAileronStart * Quaternion.AngleAxis(roll * maxAileronDeflection, aileronRotationAxis);
        if (rightAileronPivot)
            rightAileronPivot.localRotation = rightAileronStart * Quaternion.AngleAxis(-roll * maxAileronDeflection, aileronRotationAxis);

        if (leftElevatorPivot)
            leftElevatorPivot.localRotation = leftElevatorStart * Quaternion.AngleAxis(-pitch * maxElevatorDeflection, elevatorRotationAxis);
        if (rightElevatorPivot)
            rightElevatorPivot.localRotation = rightElevatorStart * Quaternion.AngleAxis(-pitch * maxElevatorDeflection, elevatorRotationAxis);
    }
}
