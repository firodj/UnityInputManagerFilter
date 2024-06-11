using BepInEx;
using HarmonyLib;
using System;
using System.Reflection;
using BepInEx.Logging;

namespace MyFirstPlugin
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {          
        static ManualLogSource logger;

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            var harmony = new Harmony("com.example.patch");

            Type inputManager = AccessTools.TypeByName("UnityEngine.InputSystem.InputManager");
            Logger.LogInfo($"inputManager {inputManager.FullName} is loaded!");

            MethodInfo original = AccessTools.Method(inputManager, "AddDevice", new[] { typeof(UnityEngine.InputSystem.InputDevice) });
            Logger.LogInfo($"original {original.Name}:{original.ReturnType}");

            MethodInfo prefixPatch = AccessTools.Method(typeof(Plugin), nameof(PrefixAddDevice));
            Logger.LogInfo($"prefixPatch {prefixPatch.Name}:{prefixPatch.ReturnType}");

            harmony.Patch(original, new HarmonyMethod(prefixPatch));
            logger = Logger;
        }

        static bool PrefixAddDevice(object __instance, UnityEngine.InputSystem.InputDevice device)
        {            
            logger.LogInfo($"Hook InputManager.AddDevice {device.name}");
            if (device.name == "Keyboard" || device.name == "Mouse" || device.name == "Pen") return true;
            bool supported = true;
            if (device.name == "DualShock3GamepadHID")
            {
                supported = false;
            }
            if (device.name == "DualShock4GamepadHID" && device.description.manufacturer != "Sony Computer Entertainment")
            {
                supported = false;
            }
            if (!supported)
            {
                logger.LogError($"Unsupported InputManager.AddDevice {device.deviceId} path={device.path} description='{device.description.ToJson()}'");                
                return false;
            }
            return true;
        }
    }        
}
