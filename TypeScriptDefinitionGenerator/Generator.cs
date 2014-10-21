using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TypeScriptDefinitionGenerator
{
  public class Generator
  {
    private IList<Type> _types;

    private Func<Type, bool> _processBaseType;
    private Func<Type, bool> _processType;

    private Dictionary<Type, TypeScriptType> _processedTypes = new Dictionary<Type, TypeScriptType>();
    private Func<Type, string> _moduleNameGenerator;

    private HashSet<Type> _excludedTypes = new HashSet<Type>();

    public Generator(params Type[] types)
      : this(types, t => types.Any(x => x.Assembly == t.Assembly), t => types.Any(x => x.Assembly == t.Assembly), t => t.Namespace)
    {

    }

    public Generator(IEnumerable<Type> types, Func<Type, bool> processType, Func<Type, bool> processBaseType, Func<Type, string> moduleNameGenerator)
    {
      _types = types.ToList();
      _processBaseType = processBaseType;
      _processType = processType;
      _moduleNameGenerator = moduleNameGenerator;
    }

    private TypeScriptType ProcessType(CustomType tst)
    {
      //ProcessTypeScriptType(tst.ClrType, tst);

      BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly;

      if (tst.IncludeInheritedProperties)
      {
        flags = BindingFlags.Instance | BindingFlags.Public;
      }

      var properties = tst.ClrType.GetProperties(flags);

      foreach (var property in properties)
      {
        var propertyTst = ProcessTypeScriptType(property.PropertyType, (dynamic)GetTypeScriptType(property.PropertyType));

        tst.Properties.Add(new TypeScriptProperty
        {
          Property = property,
          Type = propertyTst
        });
      }

      return tst;
    }

    private void ProcessProperties(CustomType tst)
    {
      //ProcessTypeScriptType(tst.ClrType, tst);

      BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly;

      if (tst.IncludeInheritedProperties)
      {
        flags = BindingFlags.Instance | BindingFlags.Public;
      }

      var properties = tst.ClrType.GetProperties(flags);

      foreach (var property in properties)
      {
        var propertyTst = ProcessTypeScriptType(property.PropertyType, (dynamic)GetTypeScriptType(property.PropertyType));

        tst.Properties.Add(new TypeScriptProperty
        {
          Property = property,
          Type = propertyTst
        });
      }
    }

    private TypeScriptType ProcessTypeScriptType(Type t, ArrayType tst)
    {
      var typeInside = TypeHelper.GetTypeInsideEnumerable(t);

      var typeInsideTst = GetTypeScriptType(typeInside);

      tst.ElementType = ProcessTypeScriptType(typeInside, (dynamic)typeInsideTst);

      return tst;
    }

    private TypeScriptType ProcessTypeScriptType(Type t, DictionaryType tst)
    {
      if (tst.ClrType.IsGenericType)
      {
        var args = tst.ClrType.GetGenericArguments();

        if (typeof(IDictionary).IsAssignableFrom(tst.ClrType) && args.Length == 2)
        {
          var keyTst = GetTypeScriptType(args[0]);

          tst.ElementKeyType = ProcessTypeScriptType(args[0], (dynamic)keyTst);

          var valueTst = GetTypeScriptType(args[1]);

          tst.ElementValueType = ProcessTypeScriptType(args[1], (dynamic)valueTst);
        }
      }

      return tst;
    }

    private TypeScriptType ProcessTypeScriptType(Type t, EnumType tst)
    {
      TypeScriptType processedType;

      if (!_processedTypes.TryGetValue(tst.ClrType, out processedType))
      {
        processedType = tst;

        _processedTypes.Add(tst.ClrType, processedType);

        tst.Module = _moduleNameGenerator(t);
      }

      return processedType;
    }

    private TypeScriptType ProcessTypeScriptType(Type t, CustomType tst)
    {
      TypeScriptType processedType;

      if (!_processedTypes.TryGetValue(t, out processedType))
      {
        if (!(t.IsGenericType && !t.IsGenericTypeDefinition))
        {
          _processedTypes.Add(t, tst);
        }
        else if (t.IsGenericType)
        {
          ProcessTypeScriptType(t.GetGenericTypeDefinition(), (dynamic)GetTypeScriptType(t.GetGenericTypeDefinition()));
        }

        processedType = tst;

        //processedType = ProcessType(tst);

        bool skippedBaseType;

        var baseType = GetBaseType(t, out skippedBaseType);

        if (baseType != null)
        {
          if (_processBaseType(baseType))
          {
            var processedBaseType = ProcessTypeScriptType(baseType, (dynamic)GetTypeScriptType(baseType));

            tst.BaseType = processedBaseType;
          }

          tst.IncludeInheritedProperties = skippedBaseType;
        }

        ProcessProperties(tst);

        tst.Module = _moduleNameGenerator(t);

        if (t.IsGenericType && !t.IsGenericTypeDefinition)
        {
          var baseTypeGenericArguments = t.GetGenericArguments();
          tst.GenericArguments = new List<TypeScriptType>();

          foreach (var arg in baseTypeGenericArguments)
          {
            var baseGenericArgTst = ProcessTypeScriptType(arg, (dynamic)GetTypeScriptType(arg));

            tst.GenericArguments.Add((TypeScriptType)baseGenericArgTst);
          }
        }
      }

      return processedType;
    }

    private bool BaseTypeHasSameNameAsSubType(Type t, Type baseType)
    {
      var idx = baseType.Name.IndexOf('`');

      if (idx > 0)
      {
        var baseTypename = baseType.Name.Substring(0, idx);

        return baseTypename == t.Name;
      }

      return false;
    }

    private Type GetBaseType(Type t, out bool skippedBaseType)
    {
      skippedBaseType = false;

      var baseType = t.BaseType;

      while (baseType != null && baseType.IsGenericType)
      {
        var sameName = BaseTypeHasSameNameAsSubType(t, baseType);

        if (sameName || _excludedTypes.Contains(baseType.GetGenericTypeDefinition()))
        {
          skippedBaseType = true;

          _excludedTypes.Add(baseType.GetGenericTypeDefinition());

          baseType = baseType.BaseType;
        }
        else
        {
          break;
        }
      }

      return baseType;
    }

    private TypeScriptType ProcessTypeScriptType(Type t, TypeScriptType tst)
    {
      return tst;
    }

    private TypeScriptType GetTypeScriptType(Type type)
    {
      TypeScriptType tst;

      if (TypeHelper.Is(type, typeof(string)))
      {
        tst = new StringType();
      }
      else if (TypeHelper.Is(type, typeof(bool)))
      {
        tst = new BooleanType();
      }
      else if (TypeHelper.Is(type, typeof(int),
                                        typeof(decimal),
                                        typeof(double),
                                        typeof(long),
                                        typeof(float),
                                        typeof(short),
                                        typeof(byte)))
      {
        tst = new NumberType();
      }
      else if (TypeHelper.Is(type, typeof(DateTime)))
      {
        tst = new DateTimeType();
      }
      else if (TypeHelper.Is(type, typeof(TimeSpan)))
      {
        tst = new TimeSpanType();
      }
      else if (type.IsGenericParameter)
      {
        tst = new GenericTypeParameter();
      }
      else if (TypeHelper.IsDictionary(type))
      {
        tst = new DictionaryType();
      }
      else if (TypeHelper.IsEnumerable(type))
      {
        tst = new ArrayType();
      }
      else if (TypeHelper.IsEnum(type))
      {
        tst = new EnumType();
      }
      else
      {
        var processType = _processType(type);

        if (processType)
        {
          tst = new CustomType(type);
        }
        else
        {
          tst = new AnyType();
        }
      }

      if (TypeHelper.IsNullableValueType(type))
      {
        ((ValueType)tst).IsNullable = true;
        type = Nullable.GetUnderlyingType(type);
      }

      tst.ClrType = type;

      return tst;
    }

    public void Generate()
    {
      GenerateMapping();
    }

    internal IList<TypeScriptModule> GenerateMapping()
    {
      foreach (var type in _types)
      {
        var tst = GetTypeScriptType(type);

        ProcessTypeScriptType(type, (dynamic)tst);

        //ProcessType(tst);
      }

      var groupedByModule = _processedTypes.Values.OfType<IModuleMember>()
        .GroupBy(m => m.Module)
        .Select(m => new TypeScriptModule
        {
          Module = m.Key,
          ModuleMembers = m.ToList()
        }).ToList();

      var finalOutput = new StringBuilder();

      foreach (var module in groupedByModule)
      {
        var generator = new ModuleScriptGenerator(module);

        var result = generator.Generate();

        finalOutput.Append(result);

      }

      var finalResult = finalOutput.ToString();

      //Console.WriteLine(finalResult);

      File.WriteAllText(@"E:\data\output.d.ts", finalResult);

      return groupedByModule;
    }
  }
}
