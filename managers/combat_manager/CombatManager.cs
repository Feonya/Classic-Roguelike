using Godot;
using System;
using System.Collections.Generic;

public partial class CombatManager : Node, IManager
{
    private List<(Character attacker,Character victim)> _combatList = new();
    public Action<Character> CharacterDied;

    public void Initialize()
    {
    }

    public void Update()
    {
        HandleCombats();
    }

    public void AddToCombatList(Character attacker, Character victim)
    {
        _combatList.Add((attacker,victim));
    }

    private void HandleCombats()
    {
        foreach (var combat in _combatList)
        {
            HandleCombat(combat.attacker, combat.victim);
        }
        CheckDeaths();
        _combatList.Clear();
    }

    private void HandleCombat(Character attacker,Character victim)
    {
        //判断是否被闪避
        if(IsVictimDodged(victim))
        {
            return;
        }

        //判断是否暴击
        var isAttackerCrited = IsAttackerCrited(attacker);

        //获得攻击者攻击力，如果暴击则翻倍攻击力
        var attackerAttack = GetAttackerAttack(attacker, isAttackerCrited);

        //获得被攻击者的防御力
        var victimDefend = GetVictimDefend(victim);

        //计算伤害
        var victimDamage = GetVictimDamage(attackerAttack,victimDefend);

        //扣除生命值
        HandleVictimDamage(victim,victimDamage);

    }

    private bool IsVictimDodged(Character victim)
    {
        var randomNumber = GD.RandRange(0f, 1f);
        if(victim.CharacterData.Dodge >= randomNumber)
        {
            GD.Print(victim.CharacterData.Name + "成功闪避");
            return true;
        }
        return false;
    }

    private bool IsAttackerCrited(Character attacker)
    {
        var randomNumber = GD.RandRange(0f, 1f);
        if(attacker.CharacterData.Crit >= randomNumber)
        {
            GD.Print(attacker.CharacterData.Name + "成功施展暴击");
            return true;
        }
        return false;
    }

    private float GetAttackerAttack(Character attacker,bool isAttackerCrited)
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

    private float GetVictimDamage(float attackerAttack,float victimDefend)
    {
        var damage = attackerAttack - victimDefend;
        if(damage <= 0)
        {
            damage = GD.RandRange(0, 1);
        }
        return damage;
    }

    private void HandleVictimDamage(Character victim,float victimDamage)
    {
        victim.CharacterData.Health -= victimDamage;
    }

    private void CheckDeaths()
    {
        foreach (var combat in _combatList)
        {
            var victim = combat.victim;
            if(victim.CharacterData.Health <= 0f)
            {
                victim.CharacterData.Health = 0f;
                CharacterDied.Invoke(victim);
            }
        }
    }

}
