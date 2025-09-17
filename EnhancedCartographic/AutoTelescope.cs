using KSerialization;
using TUNING;
using UnityEngine;

[SerializationConfig(MemberSerialization.OptIn)]
public class AutoTelescope : KMonoBehaviour, ISim200ms
{
    // 每周期要揭示的完整格子数量（可调节）
    [SerializeField]
    public float tilesPerCycle = 4f; // 每个周期揭示 4 格

    private float revealPointsPerSecond;

    // 当前目标
    private AxialI currentTarget;
    private ClusterFogOfWarManager.Instance fogManager;

    protected override void OnSpawn()
    {
        Debug.Log("AutoTelescope OnSpawn");
        base.OnSpawn();
        fogManager = SaveGame.Instance.GetSMI<ClusterFogOfWarManager.Instance>();

        // 计算每秒增加多少点数
        // ONI 的定义：ROCKETRY.CLUSTER_FOW.POINTS_TO_REVEAL 是揭示一格需要的点数
        // 600f = 每周期 600 秒
        revealPointsPerSecond = ROCKETRY.CLUSTER_FOW.POINTS_TO_REVEAL * tilesPerCycle / 600f;

        PickNextTarget();
    }

    public void Sim200ms(float dt)
    {
        Debug.Log($"AutoTelescope Sim200ms, target ({currentTarget.r}, {currentTarget.q}),revealed {fogManager.GetRevealCompleteFraction(currentTarget)}");
        if (fogManager == null) return;

        if (!fogManager.IsLocationRevealed(currentTarget))
        {
            // 每 200ms 给当前格子增加点数
            fogManager.EarnRevealPointsForLocation(currentTarget, dt * revealPointsPerSecond);
        }
        else
        {
            // 当前格子已完成，切换到下一个
            Debug.Log("Finished revealing current target, picking next");
            PickNextTarget();
        }
    }

    private void PickNextTarget()
    {
        if (fogManager == null) return;

        if (currentTarget != null && !fogManager.IsLocationRevealed(currentTarget))
            return;
        return;

        AxialI myWorldLocation = this.GetMyWorldLocation();
        Debug.Log($"Picking next target from my location ({myWorldLocation.r}, {myWorldLocation.q})");

        fogManager.GetUnrevealedLocationWithinRadius(myWorldLocation, 4, out currentTarget);
    }
}
