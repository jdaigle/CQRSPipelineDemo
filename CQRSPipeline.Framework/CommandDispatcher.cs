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

        public object Execute(object command, SingleInstanceFactory singleInstanceFactory)
        {
            var closure = _closureConstructor();
            foreach (var setter in _closureFieldSetters)
            {
                // we are doing dependency inject on C# closures!
                var fieldValue = singleInstanceFactory(setter.FieldType);
                setter.Execute(closure, fieldValue);
            }
            return _executor(closure, command);
        }

        private delegate object ActionExecutor(object closure, object command);
        private delegate void VoidActionExecutor(object closure, object command);
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
            ParameterExpression lambdaClosureParameter = Expression.Parameter(typeof(object), "lambdaClosure");
            UnaryExpression lambdaClosureParameterCastToClosureType = Expression.Convert(lambdaClosureParameter, methodInfo.DeclaringType);

            ParameterExpression commandParameter = Expression.Parameter(typeof(object), "command");
            UnaryExpression commandParameterCastToCommandType = Expression.Convert(commandParameter, commandType);

            MethodCallExpression methodCall = Expression.Call(lambdaClosureParameterCastToClosureType, methodInfo, commandParameterCastToCommandType);

            if (methodCall.Type == typeof(void))
            {
                Expression<VoidActionExecutor> lambda = Expression.Lambda<VoidActionExecutor>(methodCall, lambdaClosureParameter, commandParameter);
                VoidActionExecutor voidExecutor = lambda.Compile();
                return WrapVoidAction(voidExecutor);
            }
            else
            {
                UnaryExpression castMethodCallAsObject = Expression.Convert(methodCall, typeof(object));
                Expression<ActionExecutor> lambda = Expression.Lambda<ActionExecutor>(castMethodCallAsObject, lambdaClosureParameter, commandParameter);
                return lambda.Compile();
            }
        }

        private static ActionExecutor WrapVoidAction(VoidActionExecutor executor)
        {
            return delegate (object closure, object command)
            {
                executor(closure, command);
                return VoidResult.Value;
            };
        }
    }
}
