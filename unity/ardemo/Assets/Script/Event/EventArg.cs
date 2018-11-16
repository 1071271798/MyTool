namespace Game.Event
{
	//     GBEvent.Fire(XXX, new EventArg(1, 3, 4));
	//     void OnReceive(EventArg args)
	//     {
	//         int i = (int)args[0];
	//     }
	// --------------------------------------------------------------
	public class EventArg
	{
		private object[] m_args;
		public EventArg(params object[] args)
		{
			this.m_args = args;
		}

		public object this[int index]
		{
			get 
            {
                if (null != m_args && index < m_args.Length)
                {
                    return this.m_args[index];
                }
                return null;
            }
		}

		public object[] Args
		{
			get { return m_args; }
		}
	}
}