﻿using Android.Media;
using Android.OS;

namespace WoWonder.Activities.Chat.Call.Tools
{
    public class AudioHelper
    {
        //determine if user connected a headset (normal or Bluetooth)
        public static bool IsHeadsetOn(AudioManager audioManager)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                return audioManager.WiredHeadsetOn || audioManager.BluetoothScoOn || audioManager.BluetoothA2dpOn;
#pragma warning restore CS0618 // Type or member is obsolete
            }

            AudioDeviceInfo[] devices = audioManager.GetDevices(GetDevicesTargets.Outputs);
            for (int i = 0; i < devices.Length; i++)
            {
                AudioDeviceInfo device = devices[i];
                if (device.Type == AudioDeviceType.WiredHeadphones || device.Type == AudioDeviceType.BluetoothA2dp || device.Type == AudioDeviceType.BluetoothSco)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsBluetoothHeadsetOn(AudioManager audioManager)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
                return audioManager.BluetoothA2dpOn || audioManager.BluetoothScoOn;
            }

            AudioDeviceInfo[] devices = audioManager.GetDevices(GetDevicesTargets.Outputs);
            for (int i = 0; i < devices.Length; i++)
            {
                AudioDeviceInfo device = devices[i];
                if (device.Type is AudioDeviceType.BluetoothA2dp or AudioDeviceType.BluetoothSco)
                {
                    return true;
                }
            }
            return false;
        }

    }
}
