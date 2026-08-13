using UnityEngine;

/// <summary>
/// Combines the left and right pedal values into a single rudder input (-1 to +1),
/// the way a flight model actually wants it. Optionally simulates the real-world
/// mechanical linkage: pressing one pedal visually pulls the other one back.
/// </summary>
public class RudderInputSystem : MonoBehaviour
{
    [Header("Pedals")]
    public RudderPedalController leftPedal;
    public RudderPedalController rightPedal;

    [Header("Mechanical Linkage")]
    [Tooltip("Real Cessna pedals are connected by a cable/bar - pressing one pulls the other back. " +
             "Enable this once you've replaced the cubes with a hinged model for a more authentic feel.")]
    public bool useMechanicalLinkage = false;

    [Tooltip("How much the opposite pedal is pulled back per unit of press, 0-1")]
    [Range(0f, 1f)]
    public float linkageStrength = 1f;

    [Header("Output")]
    [Tooltip("-1 = full left rudder, 0 = centered, +1 = full right rudder. Feed this into your flight/yaw controller.")]
    [Range(-1f, 1f)]
    public float rudderInput;

    void LateUpdate()
    {
        if (leftPedal == null || rightPedal == null) return;

        leftPedal.linked = useMechanicalLinkage;
        rightPedal.linked = useMechanicalLinkage;

        // Net rudder deflection for the flight model: -1 = full left, +1 = full right
        float net = Mathf.Clamp(rightPedal.currentValue - leftPedal.currentValue, -1f, 1f);
        rudderInput = net;

        if (useMechanicalLinkage)
        {
            // One connected mechanism: the pedal on the "pressed" side moves forward,
            // the other is pulled back by the same amount - like a real rudder bar.
            rightPedal.MoveToward(net * linkageStrength);
            leftPedal.MoveToward(-net * linkageStrength);
        }
    }
}
