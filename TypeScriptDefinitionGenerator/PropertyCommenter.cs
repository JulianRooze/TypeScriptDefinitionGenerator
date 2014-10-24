using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeScriptDefinitionGenerator
{
  internal class PropertyCommenter
  {
    public string GetPropertyComment(ValueType p)
    {


      return p.ClrType.Name + (p.IsNullable ? ", nullable" : "");
    }

    public string GetPropertyComment(BooleanType p)
    {
      return null;
    }

    public string GetPropertyComment(EnumType p)
    {
      return null;
    }

    public string GetPropertyComment(TypeScriptType p)
    {
      return null;
    }
  }
}
