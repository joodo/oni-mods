using HarmonyLib;
using UnityEngine;

[HarmonyPatch]
public class AutoTelescopePatch
{
    [HarmonyPatch(typeof(ScannerModuleConfig), "ConfigureBuildingTemplate")]
    public class ScannerModuleConfig_Patch
    {
        public static void Postfix(GameObject go)
        {
            go.AddOrGetDef<ScannerModule.Def>().scanRadius = 2;
        }
    }
}
