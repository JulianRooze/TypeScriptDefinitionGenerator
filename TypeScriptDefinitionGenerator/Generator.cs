using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeScriptDefinitionGenerator
{

  public class Generator
  {
    private IList<Type> _types;

    private Func<Type, bool> _processBaseType;
    private Func<Type, bool> _processType;

    private Dictionary<Type, TypeScriptType> _processedTypes = new Dictionary<Type, TypeScriptType>();

    public Generator(IEnumerable<Type> types, Func<Type, bool> processType, Func<Type, bool> processBaseType)
    {
      _types = types.ToList();
      _processBaseType = processBaseType;
      _processType = processType;
    }


    private TypeScriptType ProcessType(CustomType tst)
    {
      var properties = tst.Type.GetProperties();

      foreach (var property in properties)
      {
        var propertyTst = GetTypeScriptType(property.PropertyType);

        ProcessTypeScriptType(property.PropertyType, (dynamic)propertyTst);

        tst.Properties.Add(new TypeScriptProperty
        {
          Property = property,
          Type = propertyTst
        });
      }

      _processedTypes.Add(tst.Type, tst);

      ProcessTypeScriptType(tst.Type, tst);

      return tst;
    }

    private void ProcessTypeScriptType(Type t, ArrayType tst)
    {
      var typeInside = TypeHelper.GetTypeInsideEnumerable(t);

      var typeInsideTst = GetTypeScriptType(typeInside);

      tst.ElementType = ProcessTypeScriptType(typeInside, (dynamic)typeInsideTst);
    }

    private TypeScriptType ProcessTypeScriptType(Type t, CustomType tst)
    {
      TypeScriptType existing;

      if (!_processedTypes.TryGetValue(t, out existing))
      {
        ProcessType(tst);
      }

      if (_processBaseType(t.BaseType))
      {
        if (!_processedTypes.TryGetValue(t.BaseType, out existing))
        {
          var baseTst = new CustomType(t.BaseType);

          ProcessType(baseTst);

          tst.BaseType = baseTst;
        }
      }

      return existing;
    }

    private void ProcessTypeScriptType(Type t, TypeScriptType tst)
    {

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
      else if (TypeHelper.IsEnumerable(type))
      {
        tst = new ArrayType();
      }
      else if (TypeHelper.IsDictionary(type))
      {
        tst = new DictionaryType();
      }
      else if (type.IsEnum)
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
      }

      return tst;
    }

    public void Generate()
    {
      foreach (var type in _types)
      {
        var tst = new CustomType(type);

        ProcessType(tst);
      }

      Console.WriteLine();
    }


  }
}
