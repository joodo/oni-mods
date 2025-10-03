using HarmonyLib;
using UnityEngine;

namespace EnhancedCartographic
{
    [HarmonyPatch]
    public class AutoTelescopePatch
    {
        [HarmonyPatch(
            typeof(ScannerModuleConfig),
            nameof(ScannerModuleConfig.ConfigureBuildingTemplate)
        )]
        public class PatchScannerModuleConfig
        {
            public static void Postfix(GameObject go)
            {
                go.AddOrGetDef<ScannerModule.Def>().scanRadius = 2;
            }
        }
    }
}
