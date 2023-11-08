using System;
using System.IO;
using System.Reflection;
using log4net;
using Mono.Options;
using MySqlConnector;
using Pim.CommonLib;
//using PimIdentity;

namespace Pim.Utils;

class MyArguments
{
    //const string OperationResetAdminPassword = "ResetAdmin";

    public string Operation { get; set; }

    public string NewPassword { get; set; }

    public string IdentityDatabasePath { get; set; } = string.Empty;

    public string DatabasePassword { get; set; } = string.Empty;

    //public bool IsOperationResetAdmin => OperationResetAdminPassword.Equals(Operation, StringComparison.OrdinalIgnoreCase);

    public bool IsEmptyCommand => true;
    
    public OptionSet OptionSet { get; set; }
}

class MyArgumentException : Exception
{
    /// <inheritdoc />
    public MyArgumentException(string message) : base(message)
    {
    }
}

class Program
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


    static void Main(string[] args)
    {
        var logConfigFileinfo = new FileInfo(GetFullPathNextToExecutable("log4net.config"));
        if (logConfigFileinfo.Exists)
            log4net.Config.XmlConfigurator.ConfigureAndWatch(LogManager.GetRepository(Assembly.GetExecutingAssembly()), logConfigFileinfo);
        else
            log4net.Config.XmlConfigurator.Configure(LogManager.GetRepository(Assembly.GetExecutingAssembly()));

        MyArguments myArgs = null;

        try
        {
            myArgs = ParseArguments(args);

            ProcessCommand(myArgs);
        }
        catch (OptionException exception) when(myArgs != null)
        {
            myArgs.OptionSet.WriteOptionDescriptions(Console.Out);
            Console.WriteLine(exception.Message);
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"Exception: {exception}");
        }

    }
    public static string GetFullPathNextToExecutable(string relativePath)
    {
        Check.DoRequireArgumentNotNull(relativePath, nameof(relativePath));

        var exeDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        return exeDirectory == null
            ? relativePath
            : Path.Combine(exeDirectory, relativePath);
    }

    private static void ProcessCommand(MyArguments arguments)
    {
        Check.DoRequireArgumentNotNull(arguments, nameof(arguments));

        if (arguments.IsEmptyCommand)
        {
            //CheckCommandLineArgument(!string.IsNullOrWhiteSpace(arguments.IdentityDatabasePath), "Identity database path is required.");
            //CheckCommandLineArgument(!string.IsNullOrWhiteSpace(arguments.NewPassword), "New password is required.");
            using var connection = new MySqlConnection("Server=127.0.0.1;Database=local");
            connection.Open();
            using var command = new MySqlCommand("insert into pim (id, text) values (3, 'Combined arms warfare');", connection);
        }
        else
        {
            throw new MyArgumentException($"Operation not recognized: '{arguments.Operation}'; nothing to do.");
        }
    }

    private static void CheckCommandLineArgument(bool assertion, string messageForUser)
    {
        if (!assertion)
            throw new MyArgumentException(messageForUser);
    }

    static MyArguments ParseArguments(string[] args)
    {
        var result = new MyArguments();

        var optionSet = new OptionSet()
        {
            {"op|Operation=", "Operation to perform (ResetAdmin)", o => result.Operation = o },
            { "np|NewPassword=", "New password (optional, defaults to 'password')", o => result.NewPassword = o},
            { "db|DbFilePath=", "Database file path", o => result.IdentityDatabasePath = o},
            { "pw|DatabasePassword=", "Database password", o => result.DatabasePassword = o}
        };

        optionSet.Parse(args);
        result.OptionSet = optionSet;

        return result;
    }
}