using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

[HarmonyPatch]
public class AutoTelescopePatch
{
    [HarmonyPatch(typeof(ScannerModuleConfig), "ConfigureBuildingTemplate")]
    public class ScannerModuleConfig_Patch
    {
        public static void Postfix(GameObject go)
        {
            // 给 ScannerModule 添加自动望远镜功能
            go.AddOrGet<AutoTelescope>();
            go.AddOrGetDef<ScannerModule.Def>().scanRadius = 2;
            //AxialI position = go.GetMyWorldLocation();
            //Debug.Log($"Rocket location: {position.q}, {position.r}");
        }
    }
}
