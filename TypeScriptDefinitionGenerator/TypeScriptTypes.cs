using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TypeScriptDefinitionGenerator
{
  internal class TypeScriptType
  {

  }

  internal class ValueType : TypeScriptType
  {
    public bool IsNullable { get; set; }
  }

  internal class NumberType : ValueType
  {

  }

  internal class StringType : TypeScriptType
  {

  }

  internal class BooleanType : ValueType
  {

  }

  internal class DateTimeType : ValueType
  {

  }

  internal class DictionaryType : TypeScriptType
  {

  }

  internal class ArrayType : TypeScriptType
  {
    public TypeScriptType ElementType { get; set; }
  }

  internal class EnumType : ValueType
  {

  }

  internal class TypeScriptProperty
  {
    public TypeScriptType Type { get; set; }
    public PropertyInfo Property { get; set; }

    public override string ToString()
    {
      return Property.ToString() + " - " + Type.GetType().ToString();
    }
  }

  internal class CustomType : TypeScriptType
  {
    public Type Type { get; set; }

    public CustomType(Type t)
    {
      this.Type = t;
      this.Properties = new List<TypeScriptProperty>();
    }

    public IList<TypeScriptProperty> Properties { get; set; }

    public TypeScriptType BaseType { get; set; }
  }

  internal class AnyType : TypeScriptType
  {

  }
}
