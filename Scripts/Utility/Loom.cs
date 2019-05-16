using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Linq;

public class LoomBase : System.Object
{
    public LoomCallBack mAction;
}

public delegate void LoomCallBack(LoomBase para);

public class Loom : MonoBehaviour
{
	public static int maxThreads = 10;
	static int numThreads;

	private static Loom _current;
	public static Loom Current
	{
		get
		{
			Initialize();
			return _current;
		}
	}

	static bool initialized;

	private List<LoomBase> _actions = new List<LoomBase>();
	private List<LoomBase> _currentActions = new List<LoomBase>();

	public struct DelayedQueueItem
	{
		public float time;
		public LoomBase Para;
	}
	private List<DelayedQueueItem> _delayed = new List<DelayedQueueItem>();
	List<DelayedQueueItem> _currentDelayed = new List<DelayedQueueItem>();

	void Awake()
	{
		_current = this;
		initialized = true;
	}

	static void Initialize()
	{
		if (!initialized)
		{

			if (!Application.isPlaying)
				return;
			initialized = true;
			var g = new GameObject("Loom");
			DontDestroyOnLoad(g);
			_current = g.AddComponent<Loom>();
		}

	}

	public static void QueueOnMainThread(LoomBase para, LoomCallBack action)
	{
		QueueOnMainThread(para, action, 0f);
	}

	public static void QueueOnMainThread(LoomBase para, LoomCallBack action, float time)
	{
		para.mAction = action;

		if (time != 0)
		{
			lock (Current._delayed)
			{
				Current._delayed.Add(new DelayedQueueItem { time = Time.time + time, Para = para });
			}
		}
		else
		{
			lock (Current._actions)
			{
				Current._actions.Add(para);
			}
		}
	}

	public static Thread RunAsync(LoomBase para, LoomCallBack a)
	{
		para.mAction = a;

		Initialize();
		while (numThreads >= maxThreads)
		{
			Thread.Sleep(1);
		}
		Interlocked.Increment(ref numThreads);
		ThreadPool.QueueUserWorkItem(RunAction, para);
		return null;
	}

	private static void RunAction(object para)
	{
		try
		{
			LoomBase loomObj = (LoomBase)para;
			loomObj.mAction(loomObj);
		}
		catch
		{
		}
		finally
		{
			Interlocked.Decrement(ref numThreads);
		}

	}

	void OnDisable()
	{
		if (_current == this)
		{

			_current = null;
		}
	}

	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{
		lock (_actions)
		{
			_currentActions.Clear();
			_currentActions.AddRange(_actions);
			_actions.Clear();
		}
		foreach (var a in _currentActions)
		{
			a.mAction(a);
		}
		lock (_delayed)
		{
			_currentDelayed.Clear();
			_currentDelayed.AddRange(_delayed.Where(d => d.time <= Time.time));
			foreach (var item in _currentDelayed)
				_delayed.Remove(item);
		}
		foreach (var delayed in _currentDelayed)
		{
			delayed.Para.mAction(delayed.Para);
		}
	}
}