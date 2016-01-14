using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SquirrelDeploy
{
    public class ArgumentParser
    {

        public T Parse<T>(string[] args) where T : new()
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            T instance = new T();
            var properties = typeof(T).GetProperties().ToList();

            foreach (var arg in args)
            {
                var index = arg.IndexOf("=");

                if (index == -1)
                    throw new InvalidOperationException($"Invalid argument: {arg}");

                var key = arg.Substring(0, index);
                var value = arg.Substring(index + 1);

                var prop = properties.SingleOrDefault(p => p.Name == key);

                if (prop == null)
                    throw new InvalidOperationException($"Invalid argument: {arg}");

                if (prop.PropertyType == typeof(int))
                {
                    int intValue;

                    if (!int.TryParse(value, out intValue))
                        throw new InvalidOperationException($"Invalid argument type. Integer expected: {arg}");

                    prop.SetValue(instance, intValue);
                }
                else
                    prop.SetValue(instance, value);

                properties.Remove(prop);
            }

            if (properties.Any())
                throw new InvalidOperationException($"Missing argument: {properties[0].Name}");
            
            return instance;
        }
    }
}
