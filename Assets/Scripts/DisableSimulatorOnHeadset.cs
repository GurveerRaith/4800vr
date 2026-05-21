using UnityEngine;
using UnityEngine.XR;
using System.Collections;
using System.Collections.Generic;

public class DisableSimulatorOnHeadset : MonoBehaviour
{
    IEnumerator Start()
    {
        // Wait for XR devices to fully register
        yield return new WaitForSeconds(1f);

        var headsets = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.HeadMounted, headsets);

        foreach (var device in headsets)
        {
            Debug.Log("Found headset device: " + device.name);
        }

        if (headsets.Count > 0)
        {
            Debug.Log("Real headset detected, disabling simulator");
            gameObject.SetActive(false);
        }
    }
}
