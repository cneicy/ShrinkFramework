//------------------------------------------------------------
// Shrink Framework
// Author Eicy.
// Homepage: https://github.com/cneicy/ShrinkFramework
// Feedback: mailto:im@crash.work
//------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Reflection;
using Singleton;
using UnityEngine;

namespace Event
{
    /*
     * 订阅事件的方法一定要是public且返回值为object
     */
    public class EventManager : Singleton<EventManager>
    {
        private readonly Dictionary<string, Delegate> _eventHandlers = new();
        
        public void RegisterEvent<T>(string eventName, Func<T, object> handler)
        {
            if (!_eventHandlers.TryAdd(eventName, handler))
            {
                _eventHandlers[eventName] = Delegate.Combine(_eventHandlers[eventName], handler);
            }
        }
        
        public void UnregisterEvent<T>(string eventName, Func<T, object> handler)
        {
            if (_eventHandlers.ContainsKey(eventName))
            {
                _eventHandlers[eventName] = Delegate.Remove(_eventHandlers[eventName], handler);
            }
        }
        
        public List<object> TriggerEvent<T>(string eventName, T args)
        {
            if (!_eventHandlers.TryGetValue(eventName, out var eventHandler))
                return new List<object>();

            var results = new List<object>();
            foreach (var handler in eventHandler.GetInvocationList())
            {
                if (handler is Func<T, object> typedHandler)
                {
                    try 
                    {
                        results.Add(typedHandler(args));
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"执行事件 {eventName} 时发生异常: {ex}");
                    }
                }
                else
                {
                    Debug.LogError($"事件 {eventName} 的处理程序类型不匹配");
                }
            }
            return results;
        }

        
        public void CancelEvent(string eventName)
        {
            if (!_eventHandlers.ContainsKey(eventName)) return;
            _eventHandlers[eventName] = null;
            Debug.Log($"事件 {eventName} 已取消");
        }

        public void UnregisterAllEventsForObject(object targetObject)
        {
            var eventNames = new List<string>(_eventHandlers.Keys);

            foreach (var eventName in eventNames)
            {
                var eventHandler = _eventHandlers[eventName];
                if (eventHandler == null) continue;

                var handlers = eventHandler.GetInvocationList();
                
                foreach (var handler in handlers)
                {
                    if (handler.Target == targetObject)
                    {
                        _eventHandlers[eventName] = Delegate.Remove(_eventHandlers[eventName], handler);
                    }
                }
                
                if (_eventHandlers[eventName] == null || _eventHandlers[eventName].GetInvocationList().Length == 0)
                {
                    _eventHandlers.Remove(eventName);
                }
            }

            Debug.Log($"已为 {targetObject} 注销所有事件订阅");
        }

        
        public void UnregisterAllEvents()
        {
            _eventHandlers.Clear();
            Debug.Log("所有事件订阅已被注销");
        }
        
        public void RegisterEventHandlersFromAttributes(object target)
        {
            var methods = target.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (var method in methods)
            {
                var attributes = method.GetCustomAttributes(typeof(EventSubscribeAttribute), false);
                foreach (var attribute in attributes)
                {
                    if (attribute is not EventSubscribeAttribute eventSubscribe) continue;
                    var eventName = eventSubscribe.EventName;
                    var handler = Delegate.CreateDelegate(
                        typeof(Func<,>).MakeGenericType(method.GetParameters()[0].ParameterType, method.ReturnType),
                        target, method);
                    if (!_eventHandlers.TryAdd(eventName, handler))
                    {
                        _eventHandlers[eventName] = Delegate.Combine(_eventHandlers[eventName], handler);
                    }
                }
            }
        }
    }
    
    [AttributeUsage(AttributeTargets.Method)]
    public class EventSubscribeAttribute : Attribute
    {
        public string EventName { get; }

        public EventSubscribeAttribute(string eventName)
        {
            EventName = eventName;
        }
    }
}