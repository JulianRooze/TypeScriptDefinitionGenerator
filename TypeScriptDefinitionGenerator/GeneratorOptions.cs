using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeScriptDefinitionGenerator
{
  public class GeneratorOptions
  {
    public IEnumerable<Type> Types { get; set; }

    public Func<Type, bool> TypeFilter { get; set; }

    public Func<Type, bool> BaseTypeFilter { get; set; }

    public Func<Type, string> ModuleNameGenerator { get; set; }
  }
}
