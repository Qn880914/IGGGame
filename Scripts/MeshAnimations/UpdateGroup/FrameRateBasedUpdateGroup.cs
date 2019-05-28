#region Namespace

using IGG;
using IGG.Animation;

#endregion

//using IGG.AI;

// The FrameRateBasedUpdateGroup updates the MonoBehaviour based on a informed delta time.
// if the delta time is too high (low frame rate), half of the monobehaviours is updated in on frame,
// and the other half is updated next frame. However, bear in mind that it will NOT WORK for MonoBehavious
// that are updated based on Time.deltaTime. It will work with MonoBehaviours that tracks its own delta time
// or works with the absolute time value (i.e. Time.time, Time.timeSinceStartup, etc.)
public class FrameRateBasedUpdateGroup<T> : IggBehaviourUpdateGroup where T : IUpdatableIggBehaviour
{
    private float m_frameDeltaTime;

    public FrameRateBasedUpdateGroup(float pFrameDeltaTime)
    {
        m_frameDeltaTime = pFrameDeltaTime;
    }

    public override void Update(ITime pTime)
    {
        //int framerate = UnityEngine.Time.captureFramerate;
        int start = 0;
        int increment = 1;

        // If framerate is going bad.
        if (pTime.DeltaTime > m_frameDeltaTime)
        {
            start = pTime.CurrentFrame & 0x1;
            increment = 2;
        }

        int i = 0;
        int nextUpdate = start;

        var itor = iggBehaviourList.GetEnumerator();
        while (itor.MoveNext())
        {
            IUpdatableIggBehaviour behavior = itor.Current;

            if (nextUpdate == i && behavior != null)
            {
                if (behavior.IsEnabled)
                {
                    behavior.UpdateMonoBehaviour(pTime);
                }

                nextUpdate += increment;
            }

            i++;
        }

        itor.Dispose();
    }
}