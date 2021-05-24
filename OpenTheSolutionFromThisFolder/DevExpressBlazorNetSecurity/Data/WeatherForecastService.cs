using BusinessObjectsLibrary.EFCore.BusinessObjects;
using DevExpress.EntityFrameworkCore.Security;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DevExpressBlazorNetSecurity.Data
{
  public class WeatherForecastService
  {
    readonly IConfiguration configuration;
    public WeatherForecastService(IConfiguration configuration)
    {
      this.configuration = configuration;
    }

    public Task<Employee[]> GetForecastAsync(DateTime startDate)
    {

      // ## Step 1. Initialization. Create a Secured Data Store and Set Authentication Options
      AuthenticationStandard authentication = new AuthenticationStandard();
      SecurityStrategyComplex security = new SecurityStrategyComplex(
          typeof(PermissionPolicyUser), typeof(PermissionPolicyRole),
          authentication
      );
      string connectionString = configuration.GetConnectionString("ConnectionString");
      SecuredEFCoreObjectSpaceProvider objectSpaceProvider = new SecuredEFCoreObjectSpaceProvider(security, typeof(ApplicationDbContext),
          (builder, _) => builder.UseSqlServer(connectionString));

      // ## Step 2. Authentication. Log in as a 'User' with an Empty Password
      authentication.SetLogonParameters(new AuthenticationStandardLogonParameters(userName: "User", password: string.Empty));
      IObjectSpace loginObjectSpace = objectSpaceProvider.CreateNonsecuredObjectSpace();
      try
      {
        security.Logon(loginObjectSpace);
      }
      catch (SqlException sqlEx)
      {
        if (sqlEx.Number == 4060)
        {
          throw new Exception(sqlEx.Message + Environment.NewLine + ApplicationDbContext.DatabaseConnectionFailedMessage, sqlEx);
        }
      }

      // ## Step 3. Authorization. Access and Manipulate Data/UI Based on User/Role Rights
      Console.WriteLine($"{"Full Name",-40}{"Department",-40}");
      return objectSpaceProvider.CreateObjectSpace().GetObjectsQuery<Employee>().ToArrayAsync();
    }
  }
}
