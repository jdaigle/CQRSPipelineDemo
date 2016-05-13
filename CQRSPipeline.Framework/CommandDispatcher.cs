using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace CQRSPipeline.Framework
{
    public sealed class CommandDispatcher
    {
        public static CommandDispatcher CreateCommandDispatcher<TCommand, TResult>(Func<TCommand, TResult> func, Type commandType, Type moduleType)
            where TCommand : ICommand<TResult>
        {
            return new CommandDispatcher(func.Method, commandType, moduleType);
        }

        public static CommandDispatcher CreateCommandDispatcher<TCommand>(Action<TCommand> action, Type commandType, Type moduleType)
            where TCommand : ICommand<VoidResult>
        {
            return new CommandDispatcher(action.Method, commandType, moduleType);
        }

        private readonly ActionExecutor _executor;
        private readonly LambdaClosureConstructor _closureConstructor;
        private readonly List<ClosureFieldSetter> _closureFieldSetters;

        public CommandDispatcher(MethodInfo methodInfo, Type commandType, Type moduleType)
        {
            _executor = GetExecutor(methodInfo, commandType);
            _closureConstructor = GetClosureConstructor(methodInfo.DeclaringType);
            _closureFieldSetters = GetClosureFieldSetters(methodInfo.DeclaringType);
            ModuleType = moduleType;
            CommandType = commandType;
        }

        public Type ModuleType { get; }
        public Type CommandType { get; }

        public void Execute(CommandContext commandContext)
        {
            var closure = _closureConstructor();
            foreach (var setter in _closureFieldSetters)
            {
                // we are doing dependency inject on C# closures!
                var fieldValue = commandContext.SingleInstanceFactory(setter.FieldType);
                setter.Execute(closure, fieldValue);
            }
            _executor(closure, commandContext);
        }

        private delegate void ActionExecutor(object closure, CommandContext commandContext);
        private delegate object LambdaClosureConstructor();
        private delegate void SetClosureFieldValue(object closure, object value);

        private static LambdaClosureConstructor GetClosureConstructor(Type type)
        {
            NewExpression constructorCall = Expression.New(type.GetConstructors()[0]);
            UnaryExpression castClosureAsObject = Expression.Convert(constructorCall, typeof(object));
            Expression<LambdaClosureConstructor> lambda = Expression.Lambda<LambdaClosureConstructor>(castClosureAsObject);
            return lambda.Compile();
        }

        private static List<ClosureFieldSetter> GetClosureFieldSetters(Type type)
        {
            var setters = new List<ClosureFieldSetter>();
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                var fieldType = field.FieldType;

                ParameterExpression lambdaClosureParameter = Expression.Parameter(typeof(object), "lambdaClosure");
                UnaryExpression lambdaClosureParameterCastToClosureType = Expression.Convert(lambdaClosureParameter, type);

                ParameterExpression valueParameter = Expression.Parameter(typeof(object), "value");
                UnaryExpression valueParameterCastToValueType = Expression.Convert(valueParameter, fieldType);

                MemberExpression fieldExpression = Expression.Field(lambdaClosureParameterCastToClosureType, field);
                BinaryExpression fieldAssignment = Expression.Assign(fieldExpression, valueParameterCastToValueType);

                Expression<SetClosureFieldValue> lambda = Expression.Lambda<SetClosureFieldValue>(fieldAssignment, lambdaClosureParameter, valueParameter);
                var lambdaCompiled = lambda.Compile();

                ;

                setters.Add(new ClosureFieldSetter(fieldType, lambdaCompiled));
            }
            return setters;
        }
        
        private sealed class ClosureFieldSetter
        {
            public ClosureFieldSetter(Type fieldType, SetClosureFieldValue setter)
            {
                FieldType = fieldType;
                Execute = setter;
            }

            public Type FieldType { get; }
            public SetClosureFieldValue Execute { get; }
        }

        private static ActionExecutor GetExecutor(MethodInfo methodInfo, Type commandType)
        {
            Type commandContextType = methodInfo.ReturnType == typeof(void)
                ? typeof(CommandContext<,>).MakeGenericType(commandType, typeof(VoidResult))
                : typeof(CommandContext<,>).MakeGenericType(commandType, methodInfo.ReturnType);

            ParameterExpression lambdaClosureParameter = Expression.Parameter(typeof(object), "lambdaClosure");
            UnaryExpression lambdaClosureParameterCastToClosureType = Expression.Convert(lambdaClosureParameter, methodInfo.DeclaringType);

            ParameterExpression commandContextParameter = Expression.Parameter(typeof(CommandContext), "commandContext");
            UnaryExpression commandContextCastToCommandContextType = Expression.Convert(commandContextParameter, commandContextType);

            // commandContext<,>.Command
            var getCommandContextCommand = Expression.MakeMemberAccess(commandContextCastToCommandContextType, commandContextType.GetProperty("Command"));

            // _closure.Execute(commandContext<,>.Command)
            MethodCallExpression methodCall = Expression.Call(lambdaClosureParameterCastToClosureType, methodInfo, getCommandContextCommand);

            if (methodCall.Type == typeof(void))
            {
                // _closure.Execute(commandContext<,>.Command)
                return Expression.Lambda<ActionExecutor>(methodCall, lambdaClosureParameter, commandContextParameter).Compile();
            }

            // commandContext<,>.Result
            var setCommandContextResult = Expression.MakeMemberAccess(commandContextCastToCommandContextType, commandContextType.GetProperty("Result"));
            // commandContext<,>.Result = _closure.Execute(commandContext<,>.Command)
            var assignedCommandContextResult = Expression.Assign(setCommandContextResult, methodCall);

            Expression<ActionExecutor> lambda = Expression.Lambda<ActionExecutor>(assignedCommandContextResult, lambdaClosureParameter, commandContextParameter);
            return lambda.Compile();
        }
    }
}
