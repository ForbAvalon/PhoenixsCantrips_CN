using BlueprintCore.Blueprints.CustomConfigurators.UnitLogic.Abilities;
using BlueprintCore.Blueprints.CustomConfigurators.UnitLogic.Buffs;
using BlueprintCore.Utils;
using Kingmaker.Blueprints;

using Kingmaker.Blueprints.Classes;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using PhoenixsCantrips.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TabletopTweaks.Core.Utilities;

namespace PhoenixsCantrips.Spells
{
    class ModifyCantrips
    {
        public static void Do()
        {
            if (!Settings.IsEnabled("scaling"))
                return;
            Main.Context.Logger.Log("Adding cantrip scaling");
            foreach (var v in RegisterCantrips.rangedCantrips)
            {
                ModifyAttackCantrip(v.Value);
            }
           foreach(var v in RegisterCantrips.meleeCantrips)
            {
                ModifyAttackCantrip(v.Value);
            }

         

            ModifyAttackCantrip(BlueprintTool.GetRef<BlueprintAbilityReference>("DivineZap"));//Divine Zap
            ModifyAttackCantrip(BlueprintTool.GetRef<BlueprintAbilityReference>("DisruptUndead"));//Disrupt Undead
            ModifyVirtue();
            
            // plus another die per two caster levels past first (maximum 6 dice).
            //return;
            Main.Context.Logger.Log("Updating cantrip descriptions");
            SetDesc("DivineZap", "你对一个目标释放神圣力量。目标受到{g|Encyclopedia:Dice}1d3{/g}点神力{g|Encyclopedia:Damage}伤害{/g}, 每两个施法者等级伤害额外增加1d3（最高到达6d3）。{g|Encyclopedia:Saving_Throw}豁免检定{/g}成功可减半此伤害。");//Divine Zap
            SetDesc("Jolt", "你制造出一串电火花，并需要成功进行远程{g|Encyclopedia:TouchAttack}接触攻击{/g}检定来击中目标。此{g|Encyclopedia:Spell}法术{/g}造成{g|Encyclopedia:Dice}1d3{/g}点{g|Encyclopedia:Energy_Damage}电击伤害{/g}，每两个施法者等级伤害额外增加1d3（最高到达6d3）。");//Jolt
            SetDesc("RayOfFrost", "你从指尖射出一道寒气与冰霜。你必须成功进行远程{g|Encyclopedia:TouchAttack}接触攻击{/g}才可对目标造成{g|Encyclopedia:Damage}伤害{/g}。射线造成{g|Encyclopedia:Dice}1d3{/g}点{g|Encyclopedia:Energy_Damage}寒冷伤害{/g}，每两个施法者等级伤害额外增加1d3（最高到达6d3）。");//Jolt
            SetDesc("AcidSplash", "你向目标发射一小团酸液球。你必须成功进行远程{g|Encyclopedia:TouchAttack}接触攻击{/g}才可命中目标。酸液球造成1到3（{g|Encyclopedia:Dice}1d3{/g}）点{g|Encyclopedia:Energy_Damage}酸蚀伤害{/g}，每两个施法者等级伤害额外增加1d3（最高到达6d3）。");//Acit Splash
            SetDesc("DisruptUndead", "你引导一束正向能量。你必须成功进行远程{g|Encyclopedia:TouchAttack}接触攻击{/g}才可命中目标，若该射线命中不死生物，便可对其造成{g|Encyclopedia:Dice}1d6{/g}加上每两个施法者等级1d6点{g|Encyclopedia:Damage}伤害{/g}，最高到达6d6。");//Jolt
            SetDesc("d3a852385ba4cd740992d1970170301a", "你通过{g|Encyclopedia:TouchAttack}接触{/g}向受术生物注入一股微小的生命涌动，使其获得1点临时生命值，每两个施法者等级额外+1，最高到达10点。");//Jolt
            SetBuffDesc("a13ad2502d9e4904082868eb71efb0c5", "你通过{g|Encyclopedia:TouchAttack}接触{/g}向受术生物注入一股微小的生命涌动，使其获得1点临时生命值，每两个施法者等级额外+1，最高到达10点。");//Jolt


        }

        private static void SetDesc(string guid, string newDesc)
        {
            var spell = BlueprintTool.Get<BlueprintAbility>(guid);
            
            AbilityConfigurator.For(spell).SetDescription(LocalizationTool.CreateString(spell.name + "Phoenix.Desc", newDesc, false)).Configure();
            Main.Context.Logger.LogPatch("Patched description on", spell);
        }

        private static void SetBuffDesc(string guid, string newDesc)
        {
            var spell = BlueprintTool.Get<BlueprintBuff>(guid);
            

            BuffConfigurator.For(spell).SetDescription(LocalizationTool.CreateString(spell.name + "Phoenix.Desc", newDesc, false)).Configure();
            Main.Context.Logger.LogPatch("Patched description on", spell);
        }

        private static void ModifyAttackCantrip(BlueprintAbilityReference reference)
        {


            var spell = reference.Get();
            AbilityEffectStickyTouch sticky = spell.GetComponent<AbilityEffectStickyTouch>();
            if ( sticky != null)
            {
                spell = sticky.TouchDeliveryAbility;
            }

            var dmg = spell.GetComponent<AbilityEffectRunAction>().Actions.Actions.FirstOrDefault(x => x is ContextActionDealDamage) as ContextActionDealDamage;
            dmg.Value.DiceCountValue.ValueType = Kingmaker.UnitLogic.Mechanics.ContextValueType.Rank;
            dmg.Value.DiceCountValue.ValueRank = Kingmaker.Enums.AbilityRankType.DamageDice;
            
            spell.AddContextRankConfig(x =>
            {
                x.m_Type = Kingmaker.Enums.AbilityRankType.DamageDice;
                x.m_BaseValueType = ContextRankBaseValueType.CasterLevel;
                x.m_Progression = ContextRankProgression.OnePlusDiv2;
                x.m_StepLevel = 0;
                x.m_StartLevel = 0;
                x.m_UseMax = true;
                x.m_Max = 6;
            });
            Main.Context.Logger.LogPatch("Patched damage on", spell);
        }

        private static void ModifyVirtue()
        {
            var buff = BlueprintTool.Get<BlueprintBuff>("a13ad2502d9e4904082868eb71efb0c5");
            var temphp = buff.GetComponent<TemporaryHitPointsFromAbilityValue>();
            temphp.Value.ValueType = Kingmaker.UnitLogic.Mechanics.ContextValueType.Rank;
            temphp.Value.ValueRank = Kingmaker.Enums.AbilityRankType.StatBonus;
            buff.AddContextRankConfig(x =>
            {
                x.m_Type = Kingmaker.Enums.AbilityRankType.StatBonus;
                x.m_BaseValueType = ContextRankBaseValueType.CasterLevel;
                x.m_Progression = ContextRankProgression.OnePlusDiv2;
                x.m_UseMin = true;
                x.m_Min = 1;
                x.m_UseMax = true;
                x.m_Max = 10;
            });
            Main.Context.Logger.LogPatch("Patched scaling on", buff);
        }
    }
}
