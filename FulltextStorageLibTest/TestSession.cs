// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-05-28
// Comment  
// **********************************************************************************************/
// 
using System.IO;
using System.Reflection;
using log4net;
using log4net.Config;
using NUnit.Framework;

namespace FulltextStorageLibTest;

[SetUpFixture]
public class TestSession
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    [OneTimeSetUp]
    public static void StartSession()
    {
        var log4NetConfigPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "log4net.config");
        XmlConfigurator.ConfigureAndWatch(LogManager.GetRepository(Assembly.GetExecutingAssembly()), new FileInfo(log4NetConfigPath));

        Log.Info("Starting test session");
    }
}