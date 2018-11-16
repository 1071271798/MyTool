using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Game.Platform;

namespace Game
{
	delegate void TimerCallback1();
	delegate void TimerCallback2(params object[] args);

	static class Timer
	{
		// --------------------------------------------------------------------
		// Timer 回调封装
		// --------------------------------------------------------------------
		#region Timer 回调基类
		public abstract class Callback
		{
			private bool m_disposed;				// 是否已经废弃
			private readonly float m_interval;		// 隔多长时间触发一次 
			private float m_nextTime;				// 临时记录下一次应该执行的时间
			private readonly int m_count;			// 总共要执行的次数
			private int m_currCount;				// 临时记录当前已经执行过的次数

			public Callback(float start, float interval, int count)
			{
				this.m_disposed = false;
				this.m_interval = interval < 0 ? 0 : interval;
				this.m_nextTime = Time.time + start;
				this.m_count = count;
				this.m_currCount = 0;
			}

			public virtual void Dispose()
			{
				this.m_disposed = true;
			}

			public bool Disposed
			{
				get { return this.m_disposed; }
			}

			public float NextTime
			{
				get { return this.m_nextTime; }
			}

			protected abstract void Call();

			// 如果执行完本次回调，Timer 将无效的话，返回 true
			public bool TryCall()
			{
				if (this.m_nextTime > Time.time) 
					return false;
                try
                {
                    if (!this.Disposed)
                        this.Call();
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                    PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
                }
                
				if (this.m_interval == 0)						// 如果没指定时间间隔，则只执行一次
					return true;

				if (this.m_count > 0)							// 有次数限制
				{
					if(++this.m_currCount >= this.m_count)		// 执行次数已经达到上限
						return true;
				}
			
				this.m_nextTime = Time.time + this.m_interval;
				return false;
			}
		}

		#endregion

		#region 不带参数的 Timer 回调
		private class Callback1 : Callback
		{
			private TimerCallback1 m_cb;

			public Callback1(float start, float interval, int count, TimerCallback1 cb)
				: base(start, interval, count)

			{
				this.m_cb = cb;
			}

			protected override void Call()
			{
				this.m_cb();
			}
		}
		#endregion

		#region 带不定额外参数的 Timer 回调
		private class Callback2 : Callback
		{
			private TimerCallback2 m_cb;
			private object[] m_args;

			public Callback2(float start, float interval, int count, TimerCallback2 cb, object[] args)
				: base(start, interval, count)
			{
				this.m_cb = cb;
				this.m_args = args;
			}

			protected override void Call()
			{
				this.m_cb(this.m_args);
			}
		}
		#endregion

		// --------------------------------------------------------------------
		// 实现 Timer
		// --------------------------------------------------------------------
		private static Dictionary<Int64, Callback> sm_cbs;	// 回调列表
		private static Int64 sm_handle;						// 当前 TimerID 值
		private static float sm_nextTime;					// 下次回调时间

		static Timer()
		{
			sm_cbs = new Dictionary<Int64, Callback>();
			sm_handle = 1;
			sm_nextTime = -1;
		}

		// ----------------------------------------------------------
		// private
		// ----------------------------------------------------------
		private static Int64 GenTimerID()
		{
			if (sm_handle < Int64.MaxValue)
				return ++sm_handle;
			Int64 id = 0;
			while (++id < Int64.MaxValue)
			{
				if (!sm_cbs.ContainsKey(id))
					return id;
			}
            return -1;
		}

		// ----------------------------------------------------------
		// internal
		// ----------------------------------------------------------
		internal static void Update()
		{
			float now = Time.time;
			if (now < sm_nextTime) return;							// 时间还没到最近一个 Timer（防止频繁检索执行列表）
			if (sm_cbs.Count == 0) return;
			//sm_nextTime = now + 100.0f;								// 随便给个大点的下次执行时间
			List<Int64> invalids = new List<Int64>();
			foreach(Int64 id in sm_cbs.Keys.ToArray<Int64>())
			{
                if (!sm_cbs.ContainsKey(id)) 
					continue;
                Callback cb = sm_cbs[id];
                if (cb.TryCall())
                    invalids.Add(id);
                else if (!cb.Disposed && sm_nextTime > cb.NextTime)
                    sm_nextTime = cb.NextTime;						// 搜索最近的下一个 Timer 的执行时间（防止频繁检索执行列表）
			}
			foreach(Int64 timerID in invalids)						// 去除已经无效的 Timer
				sm_cbs.Remove(timerID);
            if (sm_cbs.Count == 0)
            {
                sm_nextTime = -1;
            }
		}

		// ----------------------------------------------------------
		// public
		// ----------------------------------------------------------
		// ----------------------------------------------------------
		// 添加一个 Timer
		// 参数：
		//     start   : 多长时间后开启回调，如果为 0，则在下一帧时立刻调用
		//     interval: 隔多长时间回调一次，如果为 0，则只调用一次
		//     count   : 回调次数（超过指定次数，则结束 Timer，如果小于或等于 0，则无限次，直到调用 Cancel）
		//     cb      : 时间到达时回调
		//     args    : 为额外参数（下面有不带额外参数的版本）
		// 返回：
		//     返回一个长整形数值，表示 TimerID，通过调用 Timer.Cancel(TimerID) 能取消整个 Timer
		// ----------------------------------------------------------
		public static Int64 Add(float start, float interval, TimerCallback2 cb, params object[] args)
		{
			return Add(start, interval, 0, cb, args);
		}

		public static Int64 Add(float start, float interval, int count, TimerCallback2 cb, params object[] args)
		{
			Int64 timerID = Timer.GenTimerID();
            if (sm_nextTime < 0)
            {
                sm_nextTime = Time.time + start;
            }
            else
            {
                sm_nextTime = Math.Min(sm_nextTime, Time.time + start);
            }
			
			sm_cbs[timerID] = new Callback2(start, interval, count, cb, args); 
			return timerID;
		}

		public static Int64 Add(float start, float interval, TimerCallback1 cb)
		{
			return Add(start, interval, 0, cb);
		}

		public static Int64 Add(float start, float interval, int count, TimerCallback1 cb)
		{
			Int64 timerID = Timer.GenTimerID();
            if (sm_nextTime < 0)
            {
                sm_nextTime = Time.time + start;
            }
            else
            {
                sm_nextTime = Math.Min(sm_nextTime, Time.time + start);
            }
			sm_cbs[timerID] = new Callback1(start, interval, count, cb);
            Debug.Log(string.Format("Timer Add Index = {0} methodName = {1}", timerID, cb.Method.Name));
			return timerID;
		}

		// ----------------------------------------------------------
		// 取消一个 Timer
		// 参数：
		//     timerID: 要取消的 Timer
		// 返回：
		//     如果 Timer 存在，则返回 true，否则返回 false
		// ----------------------------------------------------------
		public static bool Cancel(Int64 timerID)
		{
			if (sm_cbs.ContainsKey(timerID))
			{
                Debug.Log(string.Format("Timer Cancel Index = {0}", timerID));
                sm_cbs[timerID].Dispose();
				sm_cbs.Remove(timerID);
                if (sm_cbs.Count > 0)
                {
                    sm_nextTime = long.MaxValue;
                    foreach (KeyValuePair<Int64, Callback> kvp in sm_cbs)
                    {
                        sm_nextTime = Math.Min(sm_nextTime, kvp.Value.NextTime);
                    }
                }
                else
                {
                    sm_nextTime = -1;
                }
                
                return true;
			}
			return false;
		}
	}
}
