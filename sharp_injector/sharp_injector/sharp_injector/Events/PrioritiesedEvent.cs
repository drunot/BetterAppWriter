using BetterAW;
using sharp_injector.Helpers;
using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sharp_injector.Events {
    public class PrioritiesedEvent<T> {

        public delegate void EventDelegate(object sender, T eventArgs);
        public struct Event : IComparer<Event> {
            public EventDelegate eventHandler;
            public int priority;

            public Event(EventDelegate EventHandler, int Priority) {
                eventHandler = EventHandler; priority = Priority;
            }


            public static bool operator <(Event first, Event second) {
                return first.priority < second.priority;
            }
            public static bool operator >(Event first, Event second) {
                return first.priority > second.priority;
            }

            public int Compare(Event first, Event second) {
                return first.priority - second.priority;
            }

            public static bool operator !=(Event first, Event second) {
                return first.eventHandler != second.eventHandler || first.priority != second.priority;
            }

            public static bool operator ==(Event first, Event second) {
                return first.eventHandler == second.eventHandler && first.priority == second.priority;
            }

            public override bool Equals(object obj) {
                if (obj.GetType() != typeof(Event)) {
                    return false;
                }
                return this == (Event)obj;
            }
            public override int GetHashCode() {

                return HashCombinator.Combine(eventHandler.GetHashCode(), priority.GetHashCode());
            }
        }

        private List<Event> events = new List<Event>();

        public static PrioritiesedEvent<T> operator +(PrioritiesedEvent<T> first, Event second) {
            try {
                var idx = first.events.BinarySearch(second);
                if (idx < 0) {
                    first.events.Insert(~idx, second);
                } else {
                    first.events.Insert(idx + 1, second);
                } 


            } catch (Exception ex) {
                Terminal.Print(string.Format("{0}\n", ex.ToString()));
            }
            return first;
        }

        public static PrioritiesedEvent<T> operator -(PrioritiesedEvent<T> first, Event second) {
            try {
                var obj = first.events.FirstOrDefault(x => x.eventHandler == second.eventHandler);
                if (obj != default(Event)) {
                    first.events.Remove(obj);
                }

            } catch (Exception ex) {
                Terminal.Print(string.Format("{0}\n", ex.ToString()));
            }
            return first;
        }

        public static PrioritiesedEvent<T> operator -(PrioritiesedEvent<T> first, EventDelegate second) {
            try {
                var obj = first.events.FirstOrDefault(x => x.eventHandler == second);
                if (obj != default(Event)) {
                    first.events.Remove(obj);
                }

            } catch (Exception ex) {
                Terminal.Print(string.Format("{0}\n", ex.ToString()));
            }
            return first;
        }

        public void Invoke(object sender, T eventArgs) {
            try {
                foreach (var e in events) {
                    e.eventHandler.Invoke(sender, eventArgs);
                }
            } catch (Exception ex) {
                Terminal.Print(string.Format("{0}\n", ex.ToString()));
            }
        }
    }
}
