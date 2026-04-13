using UnityEngine;
using UnityEngine.InputSystem;

public class GamepadChecker : MonoBehaviour
{
    void Start()
    {
        Invoke("CheckForGamepad", 1f); // Delay to ensure devices are initialized
    }

    void CheckForGamepad()
    {
        Gamepad gamepad = Gamepad.current;

        if (gamepad != null)
        {
            Debug.Log("Controller found: " + gamepad.name);
        }
        else
        {
            Debug.Log("No controller connected after delay.");
        }
    }

    void OnEnable()
    {
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is Gamepad)
        {
            Debug.Log($"Gamepad {device.name} was {change}.");
            CheckForGamepad(); // Recheck when devices are added
        }
    }
}