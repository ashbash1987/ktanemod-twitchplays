using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum CommandResponse
{
    Start,
    EndNotComplete,
    EndComplete,
    EndError,
    NoResponse
}

public interface ICommandResponseNotifier
{
    void ProcessResponse(CommandResponse response, int value);
}

