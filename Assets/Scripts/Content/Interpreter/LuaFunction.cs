using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KingdomsGame.Interpreter
{
    public class LuaFunction
    {
        public class Context
        {
            public ScriptExecutionContext scriptContext;
            public CallbackArguments args;
        }

        Func<Context, DynValue> execution;

        public LuaFunction(Func<Context, DynValue> execution)
        {
            this.execution = execution;
        }

        public DynValue Invoke(ScriptExecutionContext scriptContext, CallbackArguments args)
        {
            return execution.Invoke(new Context()
            {
                scriptContext = scriptContext,
                args = args
            });
        }
    }

    public static class LuaFunctionExtensions
    {
        public static void Set(this Table table, string property, LuaFunction function)
        {
            table.Set(property, DynValue.NewCallback(new Func<ScriptExecutionContext, CallbackArguments, DynValue>(
                (context, arguments) =>
                {
                    return function.Invoke(context, arguments);
                }
            )));
        }
    }
}
