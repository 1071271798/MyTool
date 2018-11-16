using System;
using System.Collections;
using System.Collections.Generic;
using Game.Platform;

namespace Game.Event
{
	public delegate void EventDelegate(EventArg arg);

	class MyEventDelegates
	{
		private EventID m_id;
		private List<Delegate> m_delegate;
	    private List<Delegate> m_currDelegate; 

		public MyEventDelegates(EventID eid)
		{
			this.m_id = eid;
			this.m_delegate = new List<Delegate>();
            this.m_currDelegate = new List<Delegate>();
		}

		public void Add(Delegate edlg)
		{
			this.m_delegate.Add(edlg);
		}

		public bool Remove(Delegate edlg)
		{
			if (this.m_delegate.Contains(edlg))
			{
				this.m_delegate.Remove(edlg);
			}
			return this.m_delegate.Count == 0;
		}

		public void Call(EventArg earg)
		{
            m_currDelegate.Clear();
            m_currDelegate.AddRange( m_delegate );

            foreach (Delegate edlg in this.m_currDelegate)
			{
                try
                {
                    if (null != edlg && edlg.GetType() == typeof(EventDelegate))
                        ((EventDelegate)edlg)(earg);
                }
                catch (System.Exception ex)
                {
                    PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, ex.ToString());
                    continue;
                }
				
			}
		}
	}

	public class EventMgr
	{
		private static EventMgr sm_inst;
		private Dictionary<EventID, MyEventDelegates> m_events;
		
		private EventMgr()
		{
			this.m_events = new Dictionary<EventID, MyEventDelegates>();
		}

		public static EventMgr Inst
		{
			get
			{
				if (sm_inst == null)
					sm_inst = new EventMgr();
				return sm_inst;
			}
		}


		public void Regist(EventID eid, EventDelegate edlg)
		{
			if (!this.m_events.ContainsKey(eid))
			{
				this.m_events[eid] = new MyEventDelegates(eid);
			}
			this.m_events[eid].Add(edlg);
		}

		public void UnRegist(EventID eid, EventDelegate edlg)
		{
			if (this.m_events.ContainsKey(eid) && this.m_events[eid].Remove(edlg))
			{
				this.m_events.Remove(eid);
			}
		}

		public void Fire(EventID eid, EventArg arg)
		{
			MyEventDelegates edlgs;
			if (this.m_events.TryGetValue(eid, out edlgs))
			{
				edlgs.Call(arg);
			}
		}

		public void Fire(EventID eid)
		{
			this.Fire(eid, null);
		}
	}
}