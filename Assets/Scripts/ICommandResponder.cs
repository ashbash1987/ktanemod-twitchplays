using System.Collections;

public interface ICommandResponder
{
    IEnumerator RespondToCommand(string message);
}
