namespace devicesConnector.Common;

public interface ICommandManager
{
    public void Do(CommandsQueueRepository.CommandQueue cq);
}