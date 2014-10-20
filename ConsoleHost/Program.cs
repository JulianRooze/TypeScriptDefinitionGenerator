using CeyennePOS.Shared.Dto.Users;
using CeyennePOS.Shared.Messages.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeScriptDefinitionGenerator;

namespace ConsoleHost
{
  class Program
  {
    static void Main(string[] args)
    {
      var userType = typeof(UserDto);

      var createUser = typeof(GetUser);

      var generator = new Generator(new[] { userType, createUser }, t => t.Assembly == userType.Assembly, t => t.Assembly == userType.Assembly);

      generator.Generate();

    }
  }
}
