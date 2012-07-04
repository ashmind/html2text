using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Linq;
using System.Reflection;
using System.Text;

namespace HtmlToText.ConsoleTools {
    public class CommandEngine {
        public string LongPrefix { get; set; }

        public CommandEngine() {
            this.LongPrefix = "--";
        }

        public void Execute(MethodInfo method, string[] args)
        {
            var parameters = method.GetParameters();
            var indices = parameters.Select((parameter, index) => new { parameter, index }).ToDictionary(x => x.parameter.Name, x => x.index);
            var values = new object[parameters.Length];

            var currentParameter = (ParameterInfo)null;
            var currentIndex = 0;

            foreach (var arg in args) {
                var isParameterName = arg.StartsWith(LongPrefix);
                if (isParameterName) {
                    if (currentParameter != null)
                        HandleParameterWithoutValue(currentParameter, x => values[indices[currentParameter.Name]] = x);

                    var name = arg.Substring(LongPrefix.Length, arg.Length - LongPrefix.Length);
                    currentParameter = parameters.First(p => p.Name == name);
                    continue;
                }

                var parameter = currentParameter;
                if (parameter == null) {
                    parameter = parameters[currentIndex];
                    currentIndex += 1;
                }

                var value = TypeDescriptor.GetConverter(parameter.ParameterType).ConvertFromInvariantString(arg);
                values[indices[parameter.Name]] = value;
                currentParameter = null;
            }

            if (currentParameter != null)
                HandleParameterWithoutValue(currentParameter, x => values[indices[currentParameter.Name]] = x);

            AssignDefaultValues(values, parameters);

            method.Invoke(null, values);
        }

        private static void HandleParameterWithoutValue(ParameterInfo currentParameter, Action<object> setValue) {
            if (currentParameter.ParameterType == typeof(bool)) {
                setValue(true);
            }
            else {
                throw new FormatException("Value not provided for parameter " + currentParameter.Name);
            }
        }

        private void AssignDefaultValues(object[] values, ParameterInfo[] parameters) {
            for (int i = 0; i < values.Length; i++) {
                if (values[i] != null)
                    continue;

                var @default = (DefaultParameterValueAttribute)parameters[i].GetCustomAttributes(typeof(DefaultParameterValueAttribute), false).SingleOrDefault();
                values[i] = @default != null ? @default.Value : GetDefaultValue(parameters[i].GetType());
            }
        }

        private object GetDefaultValue(Type type) {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
    }
}
