#region Namespace

using IGG.Game;

#endregion

public enum RoleAnimationType
{
    Attack,
    Skill1,
    Skill2,
    Hit,
    Dead,
    Repel, //击退
    Reborn, //复活
    Wait,
    Walk,
    Run,
    Attack1Rep,
    Attack2Rep,

    CustomAtkStart,
    Wait1,
    Wait2,
    Attack1,
    Attack2,
    Attack3,
    CustomAtkEnd,

    Win,

    //附加
    Skill1Continue,
    Bron
}

//骨骼点标识
public enum RoleBonePosType
{
    None,
    Attack, //攻击点
    Head, //头
    Chest //胸
}

public delegate void OnActorSkinLoaded(IActorSkin skin);

public interface IActorSkin
{/*
    bool SkinLoaded { get; }

    bool GenerateNormal { get; set; }
    int RecastId { set; }
    void SetCamp(BattleCamp camp);
    void SetScale(float scale);
    void OnLoadFromPool();
    void SetModelOffset(float x, float z);
    void SetAdditiveColors(float r, float g, float b, float a);
    void PlayAnimation(RoleAnimationType type);
    void PlayAnimation(string boolKey, bool boolValue);
    void SetAnimationLoopable(RoleAnimationType type, bool loop);
    void LockAnimation(bool isLock);
    void SetMoveSpeedScale(float speedScale);
    void SetAttackSpeedScale(float speedScale);
    void FaceLocalPos(float x, float y, float z, bool imme);
    void FaceWorldPos(float x, float y, float z, bool imme);
    void HighLight(bool turnOn, float r, float g, float b);
    void SetStyle(ActorStyleType type);
    void ResetNormalAttackTrigger();

    void LoadSkin(uint nSkinId, int nLevel, OnActorSkinLoaded realSkinLoadedCallBack, object userData,
                  bool async = false, bool insert = false, bool useSceneScale = false, bool noScale = false);

    void ChangeSkin(uint nSkinId, int nLevel, OnActorSkinLoaded realSkinLoadedCallBack, object userData);

    //冻结模型动画
    void Freeze();

    //解除冻结模型动画
    void UnFreeze();

    void ModifySkinSpeed(double speedScale);

    RoleAnimationType GetRoleAnimationType();*/
}