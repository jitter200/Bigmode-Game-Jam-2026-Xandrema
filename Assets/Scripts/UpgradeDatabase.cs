using UnityEngine;
using System;
using System.Collections.Generic;

public class UpgradeDatabase : MonoBehaviour
{
    public enum UpgradeType
    {
        MoreMaxHp,
        Heal,
        MoreDamage,
        MoreAoeRadius,
        MoreWindMax,
        LessWindDrain,
        MoreMoveSpeed,
    }

    [Serializable]
    public class Upgrade
    {
        public string title;
        public UpgradeType type;
        public float value;
    }

    public List<Upgrade> upgrades = new();

    public void FillDefaultIfEmpty()
    {
        if (upgrades.Count > 0) return;

        upgrades.Add(new Upgrade { title = "+20 Max HP", type = UpgradeType.MoreMaxHp, value = 20 });
        upgrades.Add(new Upgrade { title = "Heal +30", type = UpgradeType.Heal, value = 30 });
        upgrades.Add(new Upgrade { title = "+4 Damage", type = UpgradeType.MoreDamage, value = 4 });
        upgrades.Add(new Upgrade { title = "+0.3 AOE Radius", type = UpgradeType.MoreAoeRadius, value = 0.3f });
        upgrades.Add(new Upgrade { title = "+20 Wind Max", type = UpgradeType.MoreWindMax, value = 20 });
        upgrades.Add(new Upgrade { title = "-1 Wind drain/sec", type = UpgradeType.LessWindDrain, value = 1 });
        upgrades.Add(new Upgrade { title = "+1.5 Max Speed", type = UpgradeType.MoreMoveSpeed, value = 1.5f });
    }

    public List<Upgrade> GetRandom3()
    {
        FillDefaultIfEmpty();

        List<Upgrade> pool = new List<Upgrade>(upgrades);
        List<Upgrade> pick = new List<Upgrade>();

        for (int i = 0; i < 3; i++)
        {
            int idx = UnityEngine.Random.Range(0, pool.Count);
            pick.Add(pool[idx]);
            pool.RemoveAt(idx);
        }
        return pick;
    }

    public void Apply(Player player, Upgrade u)
    {
        if (player == null) return;

        switch (u.type)
        {
            case UpgradeType.MoreMaxHp:
                player.AddMaxHp(u.value);
                break;

            case UpgradeType.Heal:
                player.Heal(u.value);
                break;

            case UpgradeType.MoreDamage:
                player.aoeCenterDamage += u.value;
                break;

            case UpgradeType.MoreAoeRadius:
                player.aoeRadius += u.value;
                break;

            case UpgradeType.MoreWindMax:
                player.AddWindMax(u.value);
                break;

            case UpgradeType.LessWindDrain:
                player.windDrainPerSec = Mathf.Max(0f, player.windDrainPerSec - u.value);
                break;

            case UpgradeType.MoreMoveSpeed:
                player.maxSpeed += u.value;
                break;
        }
    }
}
