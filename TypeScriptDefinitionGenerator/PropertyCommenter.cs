using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeScriptDefinitionGenerator
{
  internal class PropertyCommenter
  {
    public string GetPropertyComment(DateTimeType p)
    {
      return p.ClrType.Name;
    }

    public string GetPropertyComment(TimeSpanType p)
    {
      return p.ClrType.Name;
    }

    public string GetPropertyComment(NumberType p)
    {
      return p.ClrType.Name;
    }

    public string GetPropertyComment(TypeScriptType p)
    {
      return null;
    }
  }
}
