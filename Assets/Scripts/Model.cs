using System.Collections.Generic;

public class SignalData
{
    public int id;
    public string title;
    public string text;
}

public class ResponseData
{
    public int id;
    public int signalId;
    public string text;

    public int nextSignalId;
    public string endingTitle;

    public int connectionDelta;
    public int paranoiaDelta;
    public int batteryDelta;
    public string setFlag;
}