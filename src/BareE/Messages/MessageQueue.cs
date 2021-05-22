using BareE.DataStructures;
using BareE.GameDev;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace BareE.Messages
{
    /// <summary>
    /// Allows messages to be dispatched.
    /// </summary>
    public class MessageQueue : IDisposable
    {
        private static Dictionary<Type, MessageAttribute> _messageTypeData;
        internal static AliasMap<MessageAttribute> _messageAliasMap;

        public static Dictionary<Type, MessageAttribute> MessageTypeData
        {
            get
            {
                if (_messageTypeData == null)
                    LoadMessageData();
                return _messageTypeData;
            }
        }

        public object SyncRoot = new object();

        //==>>MID => bool ProcessMessage<Typeof(MID)>(object as Typeof(MID) <<==
        private Dictionary<int, List<object>> _listeners = new Dictionary<int, List<object>>();

        private Dictionary<int, ConcurrentQueue<object>> _messages = new Dictionary<int, ConcurrentQueue<object>>();

        private PriorityQueue<object> _delayedMessages_Effective = new PriorityQueue<object>();
        private PriorityQueue<object> _delayedMessages_Realtime = new PriorityQueue<object>();
        private PriorityQueue<object> _delayedMessages_Turn = new PriorityQueue<object>();

        public delegate bool ProcessMessage<T>(T msg, GameState state, Instant instant);

        /// <summary>
        /// Wait delay milliseconds from the insant provided not counting paused time then emit the specified message.
        /// </summary>
        /// <param name="delay"></param>
        /// <param name="m"></param>
        /// <param name="delayedFrom"></param>
        public void EmitEffectiveTimeDelayedMessage(long delay, object m, Instant delayedFrom)
        {
            _delayedMessages_Effective.Enqueue(m, delay + delayedFrom.EffectiveDuration);
        }
        /// <summary>
        /// Wait delay milliseconds from the instant provided including paused time, then emit the specified message.
        /// </summary>
        /// <param name="delay"></param>
        /// <param name="m"></param>
        /// <param name="delayedFrom"></param>
        public void EmitRealTimeDelayedMessage(long delay, object m, Instant delayedFrom)
        {
            _delayedMessages_Realtime.Enqueue(m, delay + delayedFrom.SessionDuration);
        }
        /// <summary>
        /// Wait delay turn from the instant provided, then emit the specified message. 
        /// </summary>
        /// <param name="delay"></param>
        /// <param name="m"></param>
        /// <param name="delayedFrom"></param>
        public void EmitTurnDelayedMessage(long delay, object m, Instant delayedFrom)
        {
            _delayedMessages_Turn.Enqueue(m, delay + delayedFrom.Turn);
        }
        /// <summary>
        /// Emit a message immediately.
        /// </summary>
        /// <param name="m"></param>
        public void EmitMsg(object m)
        {
            if (m as IMessageGenerator != null)
            {
                IMessageGenerator mg = m as IMessageGenerator;
                m = mg.GenerateMessage(null);
                if (m == null)
                    return;
            }
            var tDat = MessageTypeData[m.GetType()];
            int i = tDat.MTypeID;
            EmitMsg(i, m);
        }
        /// <summary>
        /// Emit a message immediately.
        /// </summary>
        /// <param name="m"></param>
        public void EmitMsg<T>(T msg)
        {
            var tDat = MessageTypeData[typeof(T)];
            int i = tDat.MTypeID;
            EmitMsg(i, msg);
        }
        /// <summary>
        /// Emit a message immediately.
        /// </summary>
        /// <param name="m"></param>
        private void EmitMsg(int i, object m)
        {
            if (!_messages.ContainsKey(i))
                _messages.Add(i, new ConcurrentQueue<object>());
            _messages[i].Enqueue(m);
        }
        /// <summary>
        /// Add a handler to listen for a specific type of message.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="msgresp"></param>
        public void AddListener<T>(ProcessMessage<T> msgresp)
        {
            var tDat = MessageTypeData[typeof(T)];
            int i = tDat.MTypeID;
            if (!_listeners.ContainsKey(i))
                _listeners.Add(i, new List<object>());
            _listeners[i].Add(msgresp);
        }
        /// <summary>
        /// Enumerate the currently queued messages calling the specified handlers for each.
        /// The first handler to return true will be the last handler to handle the message.
        /// </summary>
        /// <param name="snapshot"></param>
        /// <param name="state"></param>
        public void ProcessMessages(Instant snapshot, GameState state)
        {
            /*Effective time Delay*/
            while (!_delayedMessages_Effective.IsEmpty &&
                  _delayedMessages_Effective.PeekWeight() <= snapshot.EffectiveDuration)
            {
                EmitMsg(_delayedMessages_Effective.Dequeue());
                // AddMsg(new ConsoleInput() { Text = $"ED={snapshot.EffectiveDuration}" });
            }

            /*Real time Delay*/
            while (!_delayedMessages_Realtime.IsEmpty &&
                  _delayedMessages_Realtime.PeekWeight() <= snapshot.SessionDuration)
            {
                EmitMsg(_delayedMessages_Realtime.Dequeue());
                // AddMsg(new ConsoleInput() { Text = $"SD={snapshot.SessionDuration}" });
            }

            /*Turn Delay*/
            while (!_delayedMessages_Turn.IsEmpty &&
                  _delayedMessages_Turn.PeekWeight() <= snapshot.Turn)
            {
                EmitMsg(_delayedMessages_Turn.Dequeue());
                // AddMsg(new ConsoleInput() { Text = $"Turn={snapshot.Turn}" });
            }

            foreach (var kvp in _listeners)
            {
                if (!_messages.ContainsKey(kvp.Key))
                    continue;
                foreach (var msg in _messages[kvp.Key])
                {
                    foreach (var listener in kvp.Value)
                    {
                        var d = (Delegate)listener;
                        try
                        {
                            if ((bool)d.DynamicInvoke(msg, state, snapshot))
                                continue;
                        }
                        catch (Exception e)
                        {
                            Log.EmitError(e);
                        }
                    }
                }
                _messages[kvp.Key].Clear();
            }
        }

        public void Dispose()
        {
        }

        public static void LoadMessageData()
        {
            _messageTypeData = new Dictionary<Type, MessageAttribute>();
            _messageAliasMap = new AliasMap<MessageAttribute>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    var attr = type.GetCustomAttribute<MessageAttribute>(false);
                    if (attr == null)
                        continue;
                    attr.OriginatingType = type;
                    Log.EmitTrace($"Registering Message: {(type.Name)}=>{attr}");
                    _messageTypeData.Add(type, attr);
                    _messageAliasMap.Alias(attr.Name, attr);
                }
            }
        }
    }
}