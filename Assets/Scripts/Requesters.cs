using System;
using System.IO;
using AsyncIO;
using NetMQ;
using NetMQ.Sockets;
using UnityEngine; 

public class RecordDataRequester : RunnableThread
{
    protected override void Run(object callback)
    {
        ForceDotNet.Force();
        using (RequestSocket recordDataRequester= new RequestSocket())
        {
            recordDataRequester.Connect("tcp://localhost:5555");
                recordDataRequester.SendFrame("RequestToStartRecording");
                string data = recordDataRequester.ReceiveFrameString();
                ((Action<string>)callback)(data);
        }
    }

}

public class LabelRequester: RunnableThread
{
    protected override void Run(object callback)
    {
        ForceDotNet.Force();
        using (RequestSocket labelRequester= new RequestSocket())
        {
            labelRequester.Connect("tcp://localhost:5555");
                labelRequester.SendFrame("RequestForClassifiedLabel");
                string data = labelRequester.ReceiveFrameString();
                ((Action<string>)callback)(data);
        }
    }
}


public class LabelCorrectionRequester: RunnableThread
{
    protected override void Run(object callback)
    {
        ForceDotNet.Force();
        using (RequestSocket labelCorrectionRequester= new RequestSocket())
        {
            labelCorrectionRequester.Connect("tcp://localhost:5555");
                labelCorrectionRequester.SendMoreFrame("RequestToCorrectLabel").SendFrame(correctedLabel);
                string data = labelCorrectionRequester.ReceiveFrameString();
                ((Action<string>)callback)(data);
        }
    }
}


public class RetrainModelRequester: RunnableThread
{
    protected override void Run(object callback)
    {
        ForceDotNet.Force();
        using (RequestSocket retrainModelRequester= new RequestSocket())
        {
            retrainModelRequester.Connect("tcp://localhost:5555");
                retrainModelRequester.SendFrame("RequestToRetrainModel");
                string data = retrainModelRequester.ReceiveFrameString();
                ((Action<string>)callback)(data);
        }
    }
}

public class AddSpacekeyEventRequester: RunnableThread
{
    protected override void Run(object callback)
    {
        ForceDotNet.Force();
        using (RequestSocket addSpacekeyEventRequester= new RequestSocket())
        {
            time =  ((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds).ToString();
            addSpacekeyEventRequester.Connect("tcp://localhost:5555");
                addSpacekeyEventRequester.SendMoreFrame(time).SendFrame("RequestToAddSpacekeyEvent");
                string data = addSpacekeyEventRequester.ReceiveFrameString();
                ((Action<string>)callback)(data);
        }
    }
}


public class StopRecordingRequester: RunnableThread
{
    protected override void Run(object callback)
    {
        ForceDotNet.Force();
        using (RequestSocket stopRecordingRequester= new RequestSocket())
        {
            stopRecordingRequester.Connect("tcp://localhost:5555");
                stopRecordingRequester.SendFrame("RequestToStopRecording");
                string data = stopRecordingRequester.ReceiveFrameString();
                ((Action<string>)callback)(data);
        }
    }
}
