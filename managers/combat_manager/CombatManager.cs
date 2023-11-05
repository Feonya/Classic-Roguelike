using System;
using System.Collections.Generic;
using Godot;

public partial class CombatManager : Node, IManager
{
    public event Action<Character> CharacterDied;

    private List<
        KeyValuePair<Character/*attacker*/, Character/*victim*/>
    > _combatList = new();

    public void Initialize()
    {
    }

    public void Update(double delta)
    {
        HandleCombats();
    }

    public void AddToCombatList(Character attacker, Character victim)
    {
        _combatList.Add(KeyValuePair.Create(attacker, victim));
    }

    private void HandleCombats()
    {
        foreach (var combat in _combatList)
        {
            HandleCombat(combat.Key, combat.Value);
        }

        CheckDeaths();

        _combatList.Clear();
    }

    private void HandleCombat(Character attacker, Character victim)
    {
        if (IsVictimDodged(victim)) { return; }

        var isAttackerCrited = IsAttackerCrited(attacker);

        var attackerAttack = GetAttackerAttack(attacker, isAttackerCrited);

        var victimDefend = GetVictimDefend(victim);

        var victimDamage = GetVictimDamage(attackerAttack, victimDefend);

        HandleVictimDamage(victim, victimDamage);

        GD.Print(
            attacker.CharacterData.Name +
            "对" +
            victim.CharacterData.Name +
            "造成" +
            victimDamage +
            "点伤害！"
        );
    }

    private bool IsVictimDodged(Character victim)
    {
        var randomNumber = GD.RandRange(0f, 1f);

        if (victim.CharacterData.Dodge >= randomNumber)
        {
            GD.Print(victim.CharacterData.Name + "成功闪避！");
            return true;
        }

        return false;
    }

    private bool IsAttackerCrited(Character attacker)
    {
        var randomNumber = GD.RandRange(0f, 1f);

        if (attacker.CharacterData.Crit >= randomNumber)
        {
            GD.Print(attacker.CharacterData.Name + "成功施展暴击！");
            return true;
        }

        return false;
    }

    private float GetAttackerAttack(Character attacker, bool isAttackerCrited)
    {
        return
            isAttackerCrited ?
            attacker.CharacterData.Attack * 2f :
            attacker.CharacterData.Attack;
    }

    private float GetVictimDefend(Character victim)
    {
        return victim.CharacterData.Defend;
    }

    private float GetVictimDamage(float attackerAttack, float victimDefend)
    {
        var damage = attackerAttack - victimDefend;
        if (damage <= 0)
        {
            damage = GD.RandRange(0, 1);
        }

        return damage;
    }

    private void HandleVictimDamage(Character victim, float victimDamage)
    {
        victim.CharacterData.Health -= victimDamage;
    }

    private void CheckDeaths()
    {
        foreach (var combat in _combatList)
        {
            var victim = combat.Value;
            if (victim.CharacterData.Health <= 0f)
            {
                victim.CharacterData.Health = 0f;

                CharacterDied.Invoke(victim);
            }
        }
    }
}
