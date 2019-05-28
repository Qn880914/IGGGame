/********************************************************************
	created:	2017/02/10
	created:	10:2:2017   11:30
	file base:	MeshAnimatorEvent
	file ext:	cs
	author:		Horatio
	
	purpose:	
*********************************************************************/
using System;
using System.Collections.Generic;
using UnityEngine;

namespace IGG.Animation {

    [DisallowMultipleComponent]
    public class MeshAnimatorEvent : MonoBehaviour, IUpdatableIggBehaviour {
        [System.Serializable]
        public class AnimationEvent {
            public string EventName;

            public string AnimationName;

            [Range(0, 1)]
            public float NormalizedTime;
        }

        public AnimationEvent[] eventList;

        private Dictionary<string, List<AnimationEvent>> animationNameEventMap;
        private IAnimator meshAnimator;

        private List<AnimationEvent> currentEventList;
        private int currentEventListCounter;


        private Dictionary<string, Action<string, IAnimator>> eventCallbackMap = new Dictionary<string, Action<string, IAnimator>>();

        public void RegisterEventCallback(string pEventName, Action<string, IAnimator> pCallback) {
            //IGG.Debug.Assert(meshAnimator != null, "MeshAnimatorEvent was not initialized yet.");
            //IGG.Debug.Assert(eventCallbackMap.ContainsKey(pEventName), "Cannot register callback for event with name " + pEventName + ". It was not found.");

            if (eventCallbackMap.ContainsKey(pEventName)) {
                eventCallbackMap[pEventName] = eventCallbackMap[pEventName] + pCallback;
            } else {
                eventCallbackMap[pEventName] = pCallback;
            }
        }

        public void UnregisterEventCallback(string pEventName, Action<string, IAnimator> pCallback) {
            if (eventCallbackMap.ContainsKey(pEventName)) {
                eventCallbackMap[pEventName] = eventCallbackMap[pEventName] - pCallback;
            }
        }

        //call in OnSkinLoaded
        public void Init()
        {
            meshAnimator = GetComponent<IAnimator>();
            meshAnimator.AnimationStarted += AnimationStartedEventHandler;
        }

        void Awake() {
            enabled = false;

            animationNameEventMap = new Dictionary<string, List<AnimationEvent>>();

            foreach (AnimationEvent evt in eventList) {
                // Get the animation list from the map
                // Create a new list if it is not in the map, and insert it in the map.
                List<AnimationEvent> animationList;
                if (animationNameEventMap.ContainsKey(evt.AnimationName)) {
                    animationList = animationNameEventMap[evt.AnimationName];
                } else {
                    animationList = new List<AnimationEvent>();
                    animationNameEventMap[evt.AnimationName] = animationList;
                }

                // Insert in the list ordered by Normalized time
                int i = 0;
                for (; i < animationList.Count; i++) {
                    if (animationList[i].NormalizedTime > evt.NormalizedTime) {
                        break;
                    }
                }
                animationList.Insert(i, evt);
            }
        }

        void OnDestroy() {
            if (meshAnimator != null) {
                meshAnimator.AnimationStarted -= AnimationStartedEventHandler;
            }
        }

        private void AnimationStartedEventHandler(AnimationData pAnimation) {
            if (animationNameEventMap.ContainsKey(pAnimation.Name)) {
                currentEventList = animationNameEventMap[pAnimation.Name];
                currentEventListCounter = 0;
                enabled = true;
            } else {
                currentEventList = null;
                currentEventListCounter = -1;
            }
        }

        public bool IsEnabled {
            get {
                return gameObject.activeInHierarchy && enabled;
            }
        }

        public void UpdateMonoBehaviour(ITime pTime) {
            if (currentEventList != null) {
                //UnityEngine.Profiling.Profiler.BeginSample("MeshAnimatorEvent.FireEvent()", gameObject);
                //IGG.Logging.Logger.Log ("currentEventList is valid. Length is " + currentEventList.Count);
                // runs over the event list. They are ordered by normalized time
                while (currentEventList.Count > currentEventListCounter &&
                      meshAnimator.NormalizedTime >= currentEventList[currentEventListCounter].NormalizedTime) {
                    //IGG.Logging.Logger.Log ("Firing event " + currentEventList[currentEventListCounter].EventName + ". Counter is " + currentEventListCounter);
                    FireEvent(currentEventList[currentEventListCounter].EventName);
                    currentEventListCounter++;
                }
               // UnityEngine.Profiling.Profiler.EndSample();
                // just for precaution, we set the list to null.
                // and for saving some processing time, we disable the script
                if (currentEventListCounter >= currentEventList.Count) {
                    currentEventList = null;
                    currentEventListCounter = -1;
                    enabled = false;
                }
            }
        }

        private void FireEvent(string pName) {
            if (eventCallbackMap.ContainsKey(pName)) {
            //    UnityEngine.Profiling.Profiler.BeginSample(pName);
                eventCallbackMap[pName](pName, meshAnimator);
           //     UnityEngine.Profiling.Profiler.EndSample();
            }
        }
    }

}
//[RequireComponent(typeof(MeshRenderer))]


