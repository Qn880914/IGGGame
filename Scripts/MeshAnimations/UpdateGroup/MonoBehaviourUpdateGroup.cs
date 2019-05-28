using IGG;
using System.Collections.Generic;

//using IGG.AI;

public class IggBehaviourUpdateGroup
{
    protected HashSet<IUpdatableIggBehaviour> iggBehaviourList;

    public IggBehaviourUpdateGroup()
    {
        iggBehaviourList = new HashSet<IUpdatableIggBehaviour>();
    }

    public void AddMonoBehaviour(IUpdatableIggBehaviour pBehaviour)
    {
        UnityEngine.Debug.LogWarning(!iggBehaviourList.Contains(pBehaviour) + "IggBehaviourUpdateGroup" +
                            "IggBehaviourGroup already contains IggBehaviour {0}" + pBehaviour);
        iggBehaviourList.Add(pBehaviour);
    }

    public void RemoveMonoBehaviour(IUpdatableIggBehaviour pBehaviour)
    {
        iggBehaviourList.Remove(pBehaviour);
    }

    public bool IsListEmpty()
    {
        return null == iggBehaviourList ? true : iggBehaviourList.Count == 0;
    }

    public int Count()
    {
        return null == iggBehaviourList ? 0 : iggBehaviourList.Count;
    }

    public virtual void Update(ITime pTime)
    {
        var itor = iggBehaviourList.GetEnumerator();
        while (itor.MoveNext())
        {
            IUpdatableIggBehaviour behavior = itor.Current;
            if (behavior.IsEnabled)
            {
                behavior.UpdateMonoBehaviour(pTime);
            }
        }
    }
}


public interface IUpdatableIggBehaviour
{
    void UpdateMonoBehaviour(ITime pTime);

    bool IsEnabled { get; }
}