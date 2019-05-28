#region Namespace

using System;
using System.Collections.Generic;

#endregion

/**
Author: Aoicocoon
**/
public class AnimationType
{
    //蒙皮触发器
    public enum SkinedMeshAnimatorTrigger
    {
        trigger_attack,
        trigger_skill1,
        trigger_skill2,
        trigger_hit,
        trigger_dead,
        trigger_repel,
        trigger_reborn,
        trigger_attack1,
        trigger_attack2,
        trigger_attack3,
        trigger_win,
        trigger_Attack1Rep,
        trigger_Attack2Rep,

        bool_walk,
        bool_stun,
        bool_skill1_continue,

        trigger_count, //总数
    }

    public enum SkinedMeshSlot
    {
        Attack1,
        Attack2,
        Attack3,
        Dead,
        Hit,
        Run,
        Skill1,
        Skill2,
        Skill1_loop,
        Stun,
        Wait,
        Win,
    }

    public static string Animator_AttackChooseParameter = "choose_attack_clip";
    public static string Animator_WalkScaleParameter = "walk_speed_scale"; //移动速度
    public static string Animator_WalkSwitchParameter = "bool_walk";
    public static string Animator_LoopSkill1Parameter = "bool_loop_skill1";
    public static string Animator_Skill1ContinueParameter = "bool_skill1_continue";
    public static string Animation_AttackScaleParameter = "attack_speed_scale"; //攻击速度

    //槽位动画转换器
    private static Dictionary<string, List<string>> m_AnimatorSlotConvert;
    private static Dictionary<RoleAnimationType, SkinedMeshAnimatorTrigger> m_SkinedMeshAnimatorConvert;
    private static Dictionary<string, RoleAnimationType> m_AnimationTypeConvert;

    public static Dictionary<string, List<string>> AnimatorSlotConvert
    {
        get
        {
            if (null == m_AnimatorSlotConvert)
            {
                m_AnimatorSlotConvert = new Dictionary<string, List<string>>();
                m_AnimatorSlotConvert.Add(RoleAnimationType.Attack.ToString(),
                                          new List<string>
                                          {
                                              SkinedMeshSlot.Attack1.ToString(), SkinedMeshSlot.Attack2.ToString(),
                                              SkinedMeshSlot.Attack3.ToString()
                                          });
                m_AnimatorSlotConvert.Add(RoleAnimationType.Dead.ToString(),
                                          new List<string> {SkinedMeshSlot.Dead.ToString()});
                m_AnimatorSlotConvert.Add(RoleAnimationType.Hit.ToString(),
                                          new List<string> {SkinedMeshSlot.Hit.ToString()});
                m_AnimatorSlotConvert.Add(RoleAnimationType.Run.ToString(),
                                          new List<string> {SkinedMeshSlot.Run.ToString()});
                m_AnimatorSlotConvert.Add(RoleAnimationType.Skill1.ToString(),
                                          new List<string> {SkinedMeshSlot.Skill1.ToString()});
                m_AnimatorSlotConvert.Add(RoleAnimationType.Skill2.ToString(),
                                          new List<string> {SkinedMeshSlot.Skill2.ToString()});
                m_AnimatorSlotConvert.Add(RoleAnimationType.Wait.ToString(),
                                          new List<string> {SkinedMeshSlot.Wait.ToString()});
                m_AnimatorSlotConvert.Add(RoleAnimationType.Win.ToString(),
                                          new List<string> {SkinedMeshSlot.Win.ToString()});
                m_AnimatorSlotConvert.Add(RoleAnimationType.Attack1.ToString(), 
                                            new List<string> { SkinedMeshSlot.Attack1.ToString()});
                m_AnimatorSlotConvert.Add(RoleAnimationType.Attack2.ToString(),
                                            new List<string> { SkinedMeshSlot.Attack2.ToString() });
                m_AnimatorSlotConvert.Add(RoleAnimationType.Attack3.ToString(),
                                            new List<string> { SkinedMeshSlot.Attack3.ToString() });
                m_AnimatorSlotConvert.Add("bool_skill1_continue", new List<string> {"Skill1_Loop"});
                m_AnimatorSlotConvert.Add("Attack1Rep", new List<string> {"Attack1Rep"});
                m_AnimatorSlotConvert.Add("Attack2Rep", new List<string> {"Attack2Rep"});
            }

            return m_AnimatorSlotConvert;
        }
    }

    public static Dictionary<RoleAnimationType, SkinedMeshAnimatorTrigger> SkinedMeshAnimatorConvert
    {
        get
        {
            if (null == m_SkinedMeshAnimatorConvert)
            {
                m_SkinedMeshAnimatorConvert = new Dictionary<RoleAnimationType, SkinedMeshAnimatorTrigger>();
                m_SkinedMeshAnimatorConvert.Add(RoleAnimationType.Attack, SkinedMeshAnimatorTrigger.trigger_attack);
                m_SkinedMeshAnimatorConvert.Add(RoleAnimationType.Attack1, SkinedMeshAnimatorTrigger.trigger_attack1);
                m_SkinedMeshAnimatorConvert.Add(RoleAnimationType.Attack2, SkinedMeshAnimatorTrigger.trigger_attack2);
                m_SkinedMeshAnimatorConvert.Add(RoleAnimationType.Attack3, SkinedMeshAnimatorTrigger.trigger_attack3);
                m_SkinedMeshAnimatorConvert.Add(RoleAnimationType.Skill1, SkinedMeshAnimatorTrigger.trigger_skill1);
                m_SkinedMeshAnimatorConvert.Add(RoleAnimationType.Skill2, SkinedMeshAnimatorTrigger.trigger_skill2);
                m_SkinedMeshAnimatorConvert.Add(RoleAnimationType.Hit, SkinedMeshAnimatorTrigger.trigger_hit);
                m_SkinedMeshAnimatorConvert.Add(RoleAnimationType.Dead, SkinedMeshAnimatorTrigger.trigger_dead);
                m_SkinedMeshAnimatorConvert.Add(RoleAnimationType.Repel, SkinedMeshAnimatorTrigger.trigger_repel);
                m_SkinedMeshAnimatorConvert.Add(RoleAnimationType.Reborn, SkinedMeshAnimatorTrigger.trigger_reborn);
                m_SkinedMeshAnimatorConvert.Add(RoleAnimationType.Win, SkinedMeshAnimatorTrigger.trigger_win);
                m_SkinedMeshAnimatorConvert.Add(RoleAnimationType.Attack1Rep,
                                                SkinedMeshAnimatorTrigger.trigger_Attack1Rep);
                m_SkinedMeshAnimatorConvert.Add(RoleAnimationType.Attack2Rep,
                                                SkinedMeshAnimatorTrigger.trigger_Attack2Rep);
            }

            return m_SkinedMeshAnimatorConvert;
        }
    }

    public static Dictionary<string, RoleAnimationType> AnimationTypeConvert
    {
        get
        {
            if (null == m_AnimationTypeConvert)
            {
                m_AnimationTypeConvert = new Dictionary<string, RoleAnimationType>();

                foreach (RoleAnimationType foo in Enum.GetValues(typeof(RoleAnimationType)))
                {
                    m_AnimationTypeConvert.Add(foo.ToString(), foo);
                }
            }

            return m_AnimationTypeConvert;
        }
    }
}