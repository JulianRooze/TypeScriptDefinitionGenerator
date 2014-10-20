using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeScriptDefinitionGenerator
{
  internal class TypeHelper
  {
    public static bool IsNullableValueType(Type type)
    {
      return (type.IsGenericType && type.
        GetGenericTypeDefinition().Equals
        (typeof(Nullable<>)));
    }

    public static bool IsEnumerable(Type t)
    {
      return typeof(IEnumerable).IsAssignableFrom(t) && t != typeof(string);
    }
    
    public static bool IsDictionary(Type t)
    {
      if (typeof(IDictionary).IsAssignableFrom(t))
      {
        return true;
      }

      return false;
    }

    public static Type GetTypeInsideEnumerable(Type type)
    {
      var getEnumeratorMethod = type.GetMethod("GetEnumerator", Type.EmptyTypes);

      if (getEnumeratorMethod == null)
      {
        getEnumeratorMethod = (from i in type.GetInterfaces()
                               from m in i.GetMethods()
                               where m.Name == "GetEnumerator"
                               orderby m.ReturnType.IsGenericType descending
                               select m).FirstOrDefault();

      }

      if (getEnumeratorMethod == null) return null;

      if (getEnumeratorMethod.ReturnType.IsGenericType)
      {
        var args = getEnumeratorMethod.ReturnType.GetGenericArguments();

        if (typeof(IDictionary).IsAssignableFrom(type) && args.Length == 2)
        {
          return typeof(KeyValuePair<,>).MakeGenericType(args[0], args[1]);
        }

        return args.First();
      }
      else if (type.IsArray)
      {
        return type.GetElementType();
      }

      return typeof(object);

    }

    public static bool Is(Type type, params Type[] types)
    {
      var nullableType = Nullable.GetUnderlyingType(type);

      return types.Any(t => t.IsAssignableFrom(nullableType ?? type));
    }

  }
}
