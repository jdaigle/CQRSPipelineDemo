using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CQRSPipeline.DemoAPI.Products;

namespace CQRSPipeline.DemoAPI.API
{
    public class DispatchHandlers
    {
        private readonly Dictionary<Type, DispatchInfo> delegates = new Dictionary<Type, DispatchInfo>();

        private void TryAdd(Type key, DispatchInfo dispatchInfo)
        {
            if (delegates.ContainsKey(key))
            {
                throw new InvalidOperationException("Already registered a handler for " + key.FullName);
            }
            delegates.Add(key, dispatchInfo);
        }

        public DispatchInfo FindHandler(Type type)
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
                    if (IsCommandHandlerMethod(method) || IsQueryHandlerMethod(method))
                    {
                        var parameters = method.GetParameters();
                        TryAdd(parameters[0].ParameterType, GetDispatchInfo(method));
                    }
                }
            }
        }

        private static DispatchInfo GetDispatchInfo(MethodInfo method)
        {
            var isVoidReturn = method.ReturnType == typeof(void);
            var parameters = method.GetParameters();
            if (parameters.Length == 1)
            {
                if (isVoidReturn)
                {
                    return GetActionDispatchInfo(method, typeof(ActionDispatchInfo<>));
                }
                return GetFuncDispatchInfo(method, typeof(FuncDispatchInfo<,>));
            }
            else if (parameters.Length == 2)
            {
                if (isVoidReturn)
                {
                    return GetActionDispatchInfo(method, typeof(ActionDispatchInfo<,>));
                }
                return GetFuncDispatchInfo(method, typeof(FuncDispatchInfo<,,>));
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private static DispatchInfo GetActionDispatchInfo(MethodInfo method, Type dispatchInfoGenericType)
        {
            var parameters = method.GetParameters();
            return Activator.CreateInstance(dispatchInfoGenericType.MakeGenericType(parameters.Select(x => x.ParameterType).ToArray()), method) as DispatchInfo;
        }

        private static DispatchInfo GetFuncDispatchInfo(MethodInfo method, Type dispatchInfoGenericType)
        {
            var parameters = method.GetParameters();
            return Activator.CreateInstance(dispatchInfoGenericType.MakeGenericType(parameters.Select(x => x.ParameterType).Concat(new[] { method.ReturnType }).ToArray()), method) as DispatchInfo;
        }

        private static bool IsCommandHandlerMethod(MethodInfo method)
        {
            var parameters = method.GetParameters();
            if (parameters.Length == 2)
            {
                return HasCommandInterface(parameters[0].ParameterType) &&
                       typeof(CommandContext).IsAssignableFrom(parameters[1].ParameterType);
            }
            return false;
        }

        private static bool HasCommandInterface(Type type)
        {
            return type.GetInterfaces().Any(i => i == typeof(IAPICommand));
        }

        private static bool IsQueryHandlerMethod(MethodInfo method)
        {
            var parameters = method.GetParameters();
            if (parameters.Length == 2)
            {
                return HasQueryInterface(parameters[0].ParameterType) &&
                       typeof(QueryContext).IsAssignableFrom(parameters[1].ParameterType);
            }
            return false;
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
