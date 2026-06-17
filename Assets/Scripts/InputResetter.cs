using UnityEngine;
using UnityEngine.InputSystem;

public static class InputResetter {
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetInput() {
        // Force the input system to reset its state
        // 1. Clear any stuck or cached states on current hardware devices
        foreach (var device in InputSystem.devices) {
            InputSystem.ResetDevice(device);
        }

        // 2. Force the update loop to flush out previous event queues
        InputSystem.Update();
    }
}