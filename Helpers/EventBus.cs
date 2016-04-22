using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chatter.Helpers
{
    public class EventBus
    {
        public static EventBus Instance { get { return instance ?? (instance = new EventBus()); } }

        public void Register(object listener)
        {
            if (listeners.All(l => l.Listener.GetType() != listener.GetType()))
                listeners.Add(new EventListenerWrapper(listener));
        }

        public void Unregister(object listener)
        {
            listeners.RemoveAll(l => l.Listener == listener);
        }

        public void PostEvent(object e)
        {
            listeners.Where(l => l.EventType == e.GetType()).ToList().ForEach(l => l.PostEvent(e));
        }

        private static EventBus instance;

        private EventBus() { }

        private List<EventListenerWrapper> listeners = new List<EventListenerWrapper>();

        private class EventListenerWrapper
        {
            public object Listener { get; private set; }
            public Type EventType { get; private set; }

            private MethodBase method;

            public EventListenerWrapper(object listener)
            {
                Listener = listener;

                Type type = listener.GetType();

                method = type.GetMethod("EventBusReceiveEvent");
                if (method == null)
                    throw new ArgumentException("Class " + type.Name + " does not containt method OnEvent");

                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length != 1)
                    throw new ArgumentException("Method EventBusReceiveEvent of class " + type.Name + " have invalid number of parameters (should be one)");

                EventType = parameters[0].ParameterType;
            }

            public void PostEvent(object e)
            {
                method.Invoke(Listener, new[] { e });
            }
        }
    }
}
