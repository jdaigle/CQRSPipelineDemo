using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CQRSPipeline.Framework
{
    public sealed class CommandModuleCatalog
    {
        private static readonly Type commandModuleAbstractType = typeof(CommandModule);

        private Dictionary<Type, CommandDispatcher> commandDispatchers = new Dictionary<Type, CommandDispatcher>();

        public void ScanAssembly(Assembly assembly)
        {
            var typesToScan = GetTypesSafely(assembly);
            foreach (var candidateModuleType in typesToScan)
            {
                if (candidateModuleType.IsAbstract || !candidateModuleType.IsClass || !commandModuleAbstractType.IsAssignableFrom(candidateModuleType))
                {
                    continue;
                }

                var ctors = candidateModuleType.GetConstructors();
                if (ctors.Length != 1)
                {
                    throw new Exception($"{candidateModuleType.FullName} has {ctors.Length} constructors. CommandModules should only have a single constructor.");
                }

                var ctor = ctors.Single();
                var nullParameters = ctor.GetParameters().Select(x => (object)null).ToArray();

                var commandModule = ctor.Invoke(nullParameters) as CommandModule;

                RegisterDispatchers(candidateModuleType, commandModule.CommandDispatchers);
            }
        }

        public CommandDispatcher GetDispatcher(Type commandType)
        {
            if (commandDispatchers.ContainsKey(commandType))
            {
                return commandDispatchers[commandType];
            }
            throw new Exception($"Could not find registered command dispatcher for {commandType.FullName}");
        }

        private void RegisterDispatchers(Type moduleType, IReadOnlyList<CommandDispatcher> commandDispatchers)
        {
            foreach (var commandDispatcher in commandDispatchers)
            {
                if (this.commandDispatchers.ContainsKey(commandDispatcher.CommandType))
                {
                    var existingCommandDispatcher = this.commandDispatchers[commandDispatcher.CommandType];
                    throw new Exception($"Cannot register handler for {commandDispatcher.CommandType.FullName} in {commandDispatcher.ModuleType.FullName}. Already registered in {existingCommandDispatcher.ModuleType.FullName}. Commands may only be handled once.");
                }
                this.commandDispatchers[commandDispatcher.CommandType] = commandDispatcher;
            }
        }

        // TODO: abstract to common extension method
        public static IEnumerable<Type> GetTypesSafely(Assembly assembly)
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
