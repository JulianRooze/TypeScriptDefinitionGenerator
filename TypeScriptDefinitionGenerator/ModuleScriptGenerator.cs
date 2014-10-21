using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TypeScriptDefinitionGenerator
{

  internal class ModuleScriptGenerator
  {
    private TypeScriptModule _module;
    private IndentedStringBuilder _sb;

    public ModuleScriptGenerator(TypeScriptModule module)
    {
      _module = module;
      _sb = new IndentedStringBuilder(module.ModuleMembers.Count * 256);
    }

    public string Generate()
    {
      _sb.AppendLine("declare module {0} {{", _module.Module);
      _sb.IncreaseIndentation();
      _sb.AppendLine("");

      foreach (var type in _module.ModuleMembers)
      {
        Render((dynamic)type);
      }

      _sb.DecreaseIndentation();
      _sb.AppendLine("}}");
      _sb.AppendLine("");

      return _sb.ToString();
    }

    private Regex _genericTypeReplacer = new Regex("`(\\d+)");

    private string GetTypeName(NumberType tst)
    {
      return "number";
    }

    private string GetTypeName(EnumType tst)
    {
      return tst.ClrType.Name;
    }

    private string GetTypeName(StringType tst)
    {
      return "string";
    }

    private string GetTypeName(DateTimeType tst)
    {
      return "string";
    }

    private string GetTypeName(BooleanType tst)
    {
      return "boolean";
    }

    private string GetTypeName(GenericTypeParameter tst)
    {
      return tst.ClrType.Name;
    }

    private string GetTypeName(DictionaryType tst)
    {
      return string.Format("{{ [ key : {2}{0} ] : {3}{1} }}",
        GetTypeName((dynamic)tst.ElementKeyType), GetTypeName((dynamic)tst.ElementValueType),
        GetModuleName((dynamic)tst.ElementKeyType), GetModuleName((dynamic)tst.ElementValueType));
    }

    private string GetTypeName(ArrayType tst)
    {
      return GetTypeName((dynamic)tst.ElementType) + "[]";
    }

    private string GetTypeName(TypeScriptType tst)
    {
      return "any";
    }

    private string GetTypeName(CustomType tst)
    {
      var type = tst.ClrType;

      if (type.IsGenericTypeDefinition)
      {
        var genericParams = type.GetGenericArguments();

        return string.Format("{0}<{1}>", _genericTypeReplacer.Replace(type.Name, ""), string.Join(", ", genericParams.Select(p => p.Name)));
      }
      else if (type.IsGenericType)
      {
        var genericParams = tst.GenericArguments;

        return string.Format("{0}<{1}>", _genericTypeReplacer.Replace(type.Name, ""), string.Join(", ", genericParams.Select(p => GetModuleName((dynamic)p) + GetTypeName((dynamic)p))));
      }

      return type.Name;
    }

    private void Render(CustomType type)
    {
      _sb.AppendLine("interface {0}{1} {{", GetTypeName(type), RenderBaseType(type));
      _sb.IncreaseIndentation();

      foreach (var p in type.Properties)
      {
        Render(p);
      }

      _sb.DecreaseIndentation();
      _sb.AppendLine("}}");
      _sb.AppendLine("");
    }

    private string GetPropertyComment(TypeScriptProperty p)
    {
      var obsolete = p.Property.GetCustomAttributes(typeof(ObsoleteAttribute), false).FirstOrDefault() as ObsoleteAttribute;

      var comments = new List<string>();

      if (obsolete != null)
      {
        comments.Add(string.Format("obsolete {0}", string.IsNullOrEmpty(obsolete.Message) ? "" : ": " + obsolete.Message));
      }

      var propertyComment = GetPropertyComment((dynamic)p.Type);

      if (propertyComment != null)
      {
        comments.Add(propertyComment);
      }

      if (comments.Any())
      {
        return "// " + string.Join(", ", comments);
      }

      return "";
    }

    private string GetPropertyComment(DateTimeType p)
    {
      return p.ClrType.Name;
    }

    private string GetPropertyComment(TimeSpanType p)
    {
      return p.ClrType.Name;
    }

    private string GetPropertyComment(NumberType p)
    {
      return p.ClrType.Name;
    }

    private string GetPropertyComment(TypeScriptType p)
    {
      return null;
    }

    private void Render(TypeScriptProperty p)
    {
      _sb.AppendLine("{0}{3} : {1}{2}; {4}", p.Property.Name, GetModuleName((dynamic)p.Type), GetTypeName((dynamic)p.Type), HandleOptional(p.Type), GetPropertyComment(p));
    }

    private string HandleOptional(TypeScriptType typeScriptType)
    {
      var vt = typeScriptType as ValueType;

      if (vt != null)
      {
        return vt.IsNullable ? "?" : "";
      }

      return "";
    }

    private string GetModuleName(ArrayType type)
    {
      return GetModuleName((dynamic)type.ElementType);
    }

    private string GetModuleName(TypeScriptType type)
    {
      return "";
    }

    private string GetModuleName(EnumType type)
    {
      return type.Module + ".";
    }

    private string GetModuleName(CustomType type)
    {
      return type.Module + ".";
    }

    private string RenderBaseType(CustomType type)
    {
      if (type.BaseType == null)
      {
        return "";
      }

      var baseType = string.Format(" extends {0}{1}", GetModuleName((dynamic)type.BaseType), GetTypeName((dynamic)type.BaseType));

      return baseType;

    }

    private void Render(EnumType type)
    {
      _sb.AppendLine("enum {0} {{", type.ClrType.Name);
      _sb.IncreaseIndentation();

      var values = Enum.GetValues(type.ClrType);
      var names = Enum.GetNames(type.ClrType);

      int i = 0;

      foreach (var val in values)
      {
        var name = names[i];
        i++;

        _sb.AppendLine("{0} = {1},", name, (int)val);
      }

      _sb.DecreaseIndentation();
      _sb.AppendLine("}}");
      _sb.AppendLine("");
    }
  }
}
