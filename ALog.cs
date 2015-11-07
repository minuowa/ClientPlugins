using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class ALog
{
    private static object handler;

    public static void setHandler(object handler)
    {
        ALog.handler = handler;
    }

    public static void logWarning(string msg)
    {

    }
    public static void logInfo(string msg)
    {

    }
    public static void logError(string msg)
    {

    }
    public static void warning(string msg)
    {

    }
    public static void info(string msg)
    {

    }
    public static void error(string msg)
    {

    }
    public static void except(Exception exc)
    {

    }
    public static void debug(string msg)
    {

    }
}
