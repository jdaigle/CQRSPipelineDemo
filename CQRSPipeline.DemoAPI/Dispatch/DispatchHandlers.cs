using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CQRSPipeline.DemoAPI.Products;

namespace CQRSPipeline.DemoAPI.Dispatch
{
    public class DispatchHandlers
    {
        private readonly Dictionary<Type, ActionMethodDispatcher> delegates = new Dictionary<Type, ActionMethodDispatcher>();

        private void TryAdd(Type key, ActionMethodDispatcher dispatchInfo)
        {
            if (delegates.ContainsKey(key))
            {
                throw new InvalidOperationException("Already registered a handler for " + key.FullName);
            }
            delegates.Add(key, dispatchInfo);
        }

        public ActionMethodDispatcher FindHandler(Type type)
        {
            if (!delegates.ContainsKey(type))
            {
                throw new InvalidOperationException("Handler not found for " + type.FullName);
            }
            else
            {
                return delegates[type];
            }
        }

        public void AutoRegisterHandlers()
        {
            var types = GetTypesSafely(typeof(DispatchHandlers).Assembly);
            foreach (var type in types)
            {
                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                {
                    if (IsCommandHandlerMethod(method))
                    {
                        var commandType = method.GetParameters().Single(x => typeof(IAPICommand).IsAssignableFrom(x.ParameterType)).ParameterType;
                        TryAdd(commandType, new ActionMethodDispatcher(method));
                    }
                    if (IsQueryHandlerMethod(method))
                    {
                        var queryType = method.GetParameters().Single(x => HasQueryInterface(x.ParameterType)).ParameterType;
                        TryAdd(queryType, new ActionMethodDispatcher(method));
                    }
                }
            }
        }

        private static bool IsCommandHandlerMethod(MethodInfo method)
        {
            return method.GetCustomAttribute<CommandHandlerAttribute>() != null;
        }

        private static bool IsQueryHandlerMethod(MethodInfo method)
        {
            return method.GetCustomAttribute<QueryHandlerAttribute>() != null;
        }

        private static bool HasQueryInterface(Type type)
        {
            return type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IAPIQuery<>));
        }


        private static IEnumerable<Type> GetTypesSafely(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(x => x != null);
            }
        }
    }
}
