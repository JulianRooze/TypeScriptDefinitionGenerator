using CeyennePOS.Shared.Dto.Users;
using CeyennePOS.Shared.Messages.Cart;
using CeyennePOS.Shared.Messages.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeScriptDefinitionGenerator;

namespace ConsoleHost
{
  public class Test<T>
  {
    public T Blaat { get; set; }
  }

  public class Test : Test<string>
  {

  }

  class Program
  {
    static void Main(string[] args)
    {
      var userType = typeof(UserDto);

      var createUser = typeof(GetUser);

      //var testType = typeof(Test);

      //var generator = new Generator(new[] { typeof(Test) }, t => t.Assembly == testType.Assembly, t => t.Assembly == testType.Assembly, t => t.Namespace);

      var testType = typeof(Test);

      //var generator = new Generator(new[] { userType, createUser, typeof(RequestPasswordResetToken), typeof(GetShoppingCart), typeof(ShoppingCartResponse) }, t => t.Assembly == userType.Assembly, t => t.Assembly == userType.Assembly, t => t.Namespace);
      var generator = new Generator(userType.Assembly.GetExportedTypes() , t => t.Assembly == userType.Assembly, t => t.Assembly == userType.Assembly, t => t.Namespace);
      generator.Generate();

    }
  }
}
