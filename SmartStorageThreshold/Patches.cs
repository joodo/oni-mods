using System.Collections.Generic;
using HarmonyLib;
using STRINGS;
using UnityEngine;

namespace SmartStorageThreshold
{
    [HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
    public static class PatchDbInitialize
    {
        public static void Postfix()
        {
            STRINGS.BUILDINGS.PREFABS.STORAGELOCKERSMART.EFFECT =
                "Stores the "
                + UI.FormatAsLink("Solid Materials", "ELEMENTS_SOLID")
                + " of your choosing.";
        }
    }

    [HarmonyPatch(typeof(StorageLockerSmart))]
    public static class PatchStorageLockerSmart
    {
        [HarmonyPatch("UpdateLogicAndActiveState")]
        [HarmonyPostfix]
        public static void UpdateLogicAndActiveState_Postfix(StorageLockerSmart __instance)
        {
            if (__instance == null)
                return;

            // try to find our threshold component on the same GameObject
            var threshold = __instance.gameObject.GetComponent<StorageLockerSmartThreshold>();
            if (threshold == null)
                return;

            float percentFull =
                __instance.GetComponent<Storage>().MassStored() / __instance.UserMaxCapacity;
            bool signal = threshold.EvaluateMass(percentFull);

            // send signal to ports to override original behavior
            try
            {
                var portsField = __instance
                    .GetType()
                    .GetField(
                        "ports",
                        System.Reflection.BindingFlags.Instance
                            | System.Reflection.BindingFlags.NonPublic
                    );
                if (portsField != null)
                {
                    var ports = portsField.GetValue(__instance) as LogicPorts;
                    ports?.SendSignal(FilteredStorage.FULL_PORT_ID, signal ? 1 : 0);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error sending overridden signal: {e}");
            }
        }
    }

    [HarmonyPatch(typeof(StorageLockerSmartConfig))]
    public static class PatchStorageLockerSmartConfig
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(StorageLockerSmartConfig.DoPostConfigureComplete))]
        public static void DoPostConfigureComplete_Postfix(GameObject go)
        {
            if (go == null)
                return;

            // add our threshold component so the side screen can bind to it
            if (go.GetComponent<StorageLockerSmartThreshold>() == null)
            {
                go.AddComponent<StorageLockerSmartThreshold>();
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(StorageLockerSmartConfig.CreateBuildingDef))]
        public static void CreateBuildingDef_Postfix(ref BuildingDef __result)
        {
            if (__result == null)
                return;

            __result.LogicOutputPorts = new List<LogicPorts.Port>
            {
                LogicPorts.Port.OutputPort(
                    FilteredStorage.FULL_PORT_ID,
                    new CellOffset(0, 1),
                    STRINGS.BUILDINGS.PREFABS.SMARTRESERVOIR.LOGIC_PORT,
                    STRINGS.BUILDINGS.PREFABS.SMARTRESERVOIR.LOGIC_PORT_ACTIVE,
                    STRINGS.BUILDINGS.PREFABS.SMARTRESERVOIR.LOGIC_PORT_INACTIVE,
                    show_wire_missing_icon: true
                ),
            };
        }
    }
}
