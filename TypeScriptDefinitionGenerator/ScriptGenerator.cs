using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeScriptDefinitionGenerator
{
  internal class ScriptGenerator
  {
    private IEnumerable<TypeScriptType> _types;

    internal ScriptGenerator(IEnumerable<TypeScriptType> types)
    {
      _types = types;
    }

    public void Generate()
    {
      foreach (var t in _types)
      {
        Generate(t);
      }
    }

    private void Generate(TypeScriptType tst)
    {

    }

    private void Test(CustomType tst)
    {

    }
  }
}
