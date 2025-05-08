using System.Collections.Generic;
using System.Linq;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using CombatDirector = On.RoR2.CombatDirector;

namespace DarknessExpansion;

public class RandomArtifact
{
    public static ArtifactDef randomArtifact;
    public static Sprite iconDisabled = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/TeamDeath/texArtifactDeathDisabled.png").WaitForCompletion();
    public static Sprite iconEnabled = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/TeamDeath/texArtifactDeathEnabled.png").WaitForCompletion();
    public RandomArtifact()
    {
        randomArtifact = ScriptableObject.CreateInstance<ArtifactDef>();
        randomArtifact.nameToken = "Artifact of Fun";
        randomArtifact.descriptionToken = "Something random happens whenever you pickup an item.";
        randomArtifact.smallIconDeselectedSprite = iconDisabled;
        randomArtifact.smallIconSelectedSprite = iconEnabled;
        ContentAddition.AddArtifactDef(randomArtifact);
        Inventory.onServerItemGiven += InventoryOnonServerItemGiven;
        SpawnCard.onSpawnedServerGlobal += SpawnCardOnonSpawnedServerGlobal;
        On.RoR2.CombatDirector.Spawn += CombatDirectorOnSpawn;
    }

    


    private void InventoryOnonServerItemGiven(Inventory arg1, ItemIndex arg2, int arg3)
    {
        if (arg1)  if (!ItemCatalog.GetItemDef(arg2).hidden)
        {
            CharacterMaster cb0 = arg1.GetComponent<CharacterMaster>();
            if (cb0) if (cb0.GetBody()) if (cb0.GetBody().teamComponent.teamIndex == TeamIndex.Player)
            {
                int optionNum = Random.RandomRangeInt(0, 25);
                int indexInInventory = arg1.itemAcquisitionOrder.IndexOf(arg2);
                ItemTier tier = ItemCatalog.GetItemDef(arg2).tier;
                if (optionNum == 0)
                {
                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage()
                        { baseToken = "<style=cEvent><color=#FFFFFF>Nothing Happens!</color></style>" });
                }

                if (optionNum == 1)
                {
                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage()
                        { baseToken = "<style=cEvent><color=#FF0000>Lose your item.</color></style>" });
                    arg1.RemoveItem(arg2, arg3);
                }

                if (optionNum == 2)
                {
                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage()
                        { baseToken = "<style=cEvent><color=#00FF00>Double your item.</color></style>" });
                    arg1.itemStacks[arg1.itemAcquisitionOrder.IndexOf(arg2)] *= 2;
                }

                if (optionNum == 3)
                {
                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage()
                        { baseToken = "<style=cEvent><color=#0000FF>Randomize your item.</color></style>" });
                    List<ItemIndex> choices = getItems(tier);
                    arg1.itemAcquisitionOrder[indexInInventory] = choices[Random.RandomRangeInt(0, choices.Count)];
                }

                if (optionNum == 4)
                {
                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage()
                        { baseToken = "<style=cEvent><color=#00FF00>Upgrade your item.</color></style>" });
                    ItemTier newTier = ItemTier.Boss;
                    if (tier == ItemTier.Tier1)
                    {
                        newTier = ItemTier.Tier2;
                    }
                    else if (tier == ItemTier.Tier2)
                    {
                        newTier = ItemTier.Tier3;
                    }

                    List<ItemIndex> choices = getItems(newTier);
                    arg1.itemAcquisitionOrder[indexInInventory] = choices[Random.RandomRangeInt(0, choices.Count)];
                    if (newTier == ItemTier.Boss)
                    {
                        arg1.itemStacks[indexInInventory] *= 2;
                    }
                }

                if (optionNum == 5)
                {
                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage()
                        { baseToken = "<style=cEvent><color=#FF0000>Downgrade your item.</color></style>" });
                    ItemTier newTier = ItemTier.Boss;
                    if (tier == ItemTier.Tier2)
                    {
                        newTier = ItemTier.Tier1;
                    }
                    else if (tier == ItemTier.Tier3)
                    {
                        newTier = ItemTier.Tier2;
                    }

                    List<ItemIndex> choices = getItems(newTier);
                    arg1.itemAcquisitionOrder[indexInInventory] = choices[Random.RandomRangeInt(0, choices.Count)];
                    if (newTier == ItemTier.Boss)
                    {
                        arg1.itemStacks[indexInInventory] = Mathf.Max(1, arg1.itemStacks[indexInInventory] / 2);
                    }
                }

                if (optionNum == 6)
                {
                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage()
                        { baseToken = "<style=cEvent><color=#00FFFF>Get Bigger!</color></style>" });
                    arg1.gameObject.transform.localScale *= 1.25f;
                }

                if (optionNum == 7)
                {
                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage()
                        { baseToken = "<style=cEvent><color=#FF00FF>Get Smaller!</color></style>" });
                    arg1.gameObject.transform.localScale *= .75f;
                }

                if (optionNum == 8)
                {
                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage()
                        { baseToken = "<style=cEvent><color=#FF0000>Lose your Bead Stats.</color></style>" });
                    arg1.beadAppliedDamage = 0;
                    arg1.beadAppliedHealth = 0;
                    arg1.beadAppliedRegen = 0;
                    arg1.beadAppliedShield = 0;
                }

                if (optionNum == 9)
                {
                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage()
                        { baseToken = "<style=cEvent><color=#00FF00>Get some Bonus Health!</color></style>" });
                    arg1.beadAppliedHealth += 100f;
                }

                if (optionNum == 10)
                {
                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage()
                        { baseToken = "<style=cEvent><color=#FFFF00>Get some Bonus Damage!</color></style>" });
                    arg1.beadAppliedDamage += 5f;
                }

                if (optionNum == 11)
                {
                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage()
                        { baseToken = "<style=cEvent><color=#00FFFF>Big Enemies Incoming!</color></style>" });
                    bigEnemiesLeft += 10;
                }

                if (optionNum == 12)
                {
                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage()
                        { baseToken = "<style=cEvent><color=#FF00FF>Small Enemies Incoming!</color></style>" });
                    smallEnemiesLeft += 10;
                }

                if (optionNum == 13)
                {
                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage()
                        { baseToken = "<style=cEvent><color=#00FF00>Heal to Full.</color></style>" });
                    CharacterBody cb = arg1.GetComponent<CharacterBody>();
                    cb.healthComponent.Heal((int)cb.maxHealth, new ProcChainMask());
                }

                if (optionNum == 14)
                {
                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage()
                        { baseToken = "<style=cEvent><color=#FF0000>Damage!!!!</color></style>" });
                    CharacterBody cb = arg1.GetComponent<CharacterBody>();
                    cb.healthComponent.TakeDamage(new DamageInfo { damage = cb.maxHealth * Random.value });
                }

                if (optionNum == 15)
                {
                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage()
                        { baseToken = "<style=cEvent><color=#000000>Spinning the death roulette.</color></style>" });

                    if (Random.RandomRangeInt(0, 100) == 0)
                    {
                        Chat.SendBroadcastChat(new Chat.SimpleChatMessage()
                        {
                            baseToken = "<style=cEvent><color=#000000>YOU HAVE BEEN DOOMED TO DIE!!!!</color></style>"
                        });
                        CharacterBody cb = arg1.GetComponent<CharacterBody>();
                        for (int i = 0; i < 10; i++)
                        {
                            cb.healthComponent.TakeDamage(new DamageInfo { damage = cb.maxHealth * 10f });
                        }

                        cb.healthComponent.Die();
                    }
                    else
                    {
                        Chat.SendBroadcastChat(new Chat.SimpleChatMessage()
                            { baseToken = "<style=cEvent><color=#000000>You survive this time.</color></style>" });
                    }
                }

                if (optionNum == 16)
                {
                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage()
                        { baseToken = "<style=cEvent><color=#FF0000>An enemy equips this item.</color></style>" });
                    itemToEquip = arg2;
                }

                if (optionNum == 17)
                {
                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage()
                    {
                        baseToken =
                            "<style=cEvent><color=#00FF00>The next 5 elites will become normal enemies.</color></style>"
                    });
                    removeElites = 5;
                }

                if (optionNum == 18)
                {
                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage()
                    {
                        baseToken =
                            "<style=cEvent><color=#0000FF>The next 5 normal enemies will become a random elite.</color></style>"
                    });
                    removeElites = 5;
                }

                if (optionNum == 19)
                {
                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage()
                        { baseToken = "<style=cEvent><color=#FF00FF>Enable a random artifact.</color></style>" });
                    RunArtifactManager.instance.SetArtifactEnabled(
                        ArtifactCatalog.artifactDefs[(int)(ArtifactCatalog.artifactDefs.Length * Random.value)], true);
                }

                if (optionNum == 20)
                {
                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage()
                        { baseToken = "<style=cEvent><color=#00FFFF>Gain a barrier!</color></style>" });
                    var cb20 = arg1.GetComponent<CharacterBody>();
                    cb20.healthComponent.AddBarrier(100f);
                }

                if (optionNum == 21)
                {
                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage()
                        { baseToken = "<style=cEvent><color=#FFAA00>Random teleport!</color></style>" });
                    // Teleport 5–10 units in a random direction
                    Vector3 randDir = Random.onUnitSphere;
                    randDir.y = 0;
                    arg1.gameObject.transform.position += randDir.normalized * Random.Range(5f, 10f);
                }

                if (optionNum == 22)
                {
                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage()
                        { baseToken = "<style=cEvent><color=#8888FF>Random Buff for 20 seconds!</color></style>" });
                    var cb22 = arg1.GetComponent<CharacterBody>();
                    cb22.AddTimedBuff(BuffCatalog.buffDefs[(int)(BuffCatalog.buffDefs.Length * Random.value)], 20f);
                }

                if (optionNum == 23)
                {
                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage()
                    {
                        baseToken = "<style=cEvent><color=#FFFF00>A Meteor strikes the nearest foe!</color></style>"
                    });
                    var cb23 = arg1.GetComponent<CharacterBody>();
                    CharacterBody nearest =
                        Util.GetEnemyEasyTarget(cb23, new Ray() { direction = cb23.transform.forward }, 100, 100F);
                    if (nearest)
                    {
                        nearest.healthComponent.TakeDamage(new DamageInfo
                        {
                            damage = 150f,
                            position = nearest.transform.position,
                            attacker = cb23.gameObject,
                            inflictor = null,
                            crit = false,
                            damageType = DamageType.Shock5s
                        });
                        EffectManager.SpawnEffect(GlobalEventManager.CommonAssets.runicMeteorEffect,
                            new EffectData { origin = nearest.transform.position }, true);
                    }
                }

                if (optionNum == 24)
                {
                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage()
                    {
                        baseToken = "<style=cEvent><color=#FFFF00>Swap health with a random enemy!</color></style>"
                    });
                    var cb29 = arg1.GetComponent<CharacterBody>();
                    var enemies = CharacterBody.instancesList.Where(b => b.teamComponent.teamIndex == TeamIndex.Monster)
                        .ToList();
                    if (enemies.Count > 0)
                    {
                        var target = enemies[Random.Range(0, enemies.Count)];
                        float playerHp = cb29.healthComponent.health;
                        float enemyHp = target.healthComponent.health;
                        cb29.healthComponent.health = Mathf.Clamp(enemyHp, 1f, cb29.healthComponent.fullHealth);
                        target.healthComponent.health = Mathf.Clamp(playerHp, 1f, target.healthComponent.fullHealth);
                    }
                }
            }
        }
    }

    public ItemIndex itemToEquip = ItemIndex.None;
    public int bigEnemiesLeft = 0;
    public int smallEnemiesLeft = 0;
    public int removeElites = 0;
    public int gainElites = 0;
    private void SpawnCardOnonSpawnedServerGlobal(SpawnCard.SpawnResult obj)
    {
        if (obj.spawnedInstance)
        {
            if (bigEnemiesLeft > 0)
            {
                obj.spawnedInstance.transform.localScale *= 2;
                CharacterMaster cm = obj.spawnedInstance.GetComponent<CharacterMaster>();
                if (cm)
                {
                    if (cm.GetBody())
                    {
                        cm.GetBody().transform.localScale *= 2;
                        bigEnemiesLeft--;
                    }
                }
            }

            if (smallEnemiesLeft > 0)
            {
                obj.spawnedInstance.transform.localScale *= 0.5f;
                CharacterMaster cm = obj.spawnedInstance.GetComponent<CharacterMaster>();
                if (cm)
                {
                    if (cm.GetBody())
                    {
                        cm.GetBody().transform.localScale *= 0.5f;
                        smallEnemiesLeft--;
                    }
                }
            }

            if (itemToEquip != ItemIndex.None)
            {
                obj.spawnedInstance.GetComponent<Inventory>().GiveItem(itemToEquip);
                itemToEquip = ItemIndex.None;
            }
        }
    }
    private bool CombatDirectorOnSpawn(CombatDirector.orig_Spawn origSpawn, RoR2.CombatDirector self, SpawnCard spawncard, EliteDef elitedef, Transform spawntarget, DirectorCore.MonsterSpawnDistance spawndistance, bool preventoverhead, float valuemultiplier, DirectorPlacementRule.PlacementMode placementmode)
    {
        if (removeElites > 0 && elitedef != null)
        {
            elitedef = null;
            removeElites--;
        }

        if (elitedef == null && gainElites > 0)
        {
            elitedef = EliteCatalog.eliteDefs[(int)(EliteCatalog.eliteDefs.Length * Random.value)];
            gainElites--;
        }
        return origSpawn(self, spawncard, elitedef, spawntarget, spawndistance, preventoverhead, valuemultiplier, placementmode);
    }
    public List<ItemIndex> getItems(ItemTier tier)
    {
        if (tier == ItemTier.Tier1)
        {
            return ItemCatalog.tier1ItemList;
        }
        if (tier == ItemTier.Tier2)
        {
            return ItemCatalog.tier2ItemList;
        }
        if (tier == ItemTier.Tier3)
        {
            return ItemCatalog.tier3ItemList;
        }
        if (tier == ItemTier.Lunar)
        {
            return ItemCatalog.lunarItemList;
        }
        return ItemCatalog.itemNameToIndex.Values.ToList();
    }
}