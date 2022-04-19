using System.Text;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;

namespace devicesConnector.Helpers;


public class LogHelper
{
    private static NLog.Logger _logger;


    private static NLog.Logger Logger
    {
        get
        {
            if (_logger == null)
            {
                ActivateConfig();
            }

            return _logger;
        }
    }

    public static void Write(string msg, Exception? e=null)
    {
        if (e == null)
        {
            Logger.Trace(msg);
        }

        else
        {
            Logger.Error(e, msg);
        }
    }

    private static void ActivateConfig()
    {
        var cfg = new LoggingConfiguration();

        var logPath =  @".\Logs\${date:format=yyyy-MM-dd}\";

        var layout = new LayoutWithHeaderAndFooter //отдельно, т.к. из-за использования дважды одного лайаута не пишет callsite
        {
            Header =
                @"----------NLog Starting---------${newline}" +
                @"POS Devices Connector ver.: ${assembly-version}${newline}${newline}",
            Layout =
                @"${date:format=dd.MM.yyyy HH\:mm\:ss.ffff} | ${level:uppercase=true} | ${CallSite:skipFrames=1:includeNamespace=false:fileName=false:includeSourcePath=true} | ${message} | ${exception:toString,Data}",
            Footer = @"----------NLog  Ending-----------${newline}${newline}"
        };


        var target = new FileTarget(@"traceFileTarget")
        {
            FileName = logPath + @"log.log",
            Layout = layout,
            Encoding = Encoding.UTF8
        };

        cfg.AddTarget(target);


        var loggerName = "main logger";
        cfg.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, target, loggerName);

        LogManager.Configuration = cfg;

        _logger = LogManager.GetLogger(loggerName);

    }
}