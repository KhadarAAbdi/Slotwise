namespace Slotwise.Application.Abstractions
{
    public interface ICommandHandler<TCommand, TResult>
    {
        Task<TResult> Handle(TCommand command);
    }

    public interface ICommandHandler<TCommand>
    {
        Task Handle(TCommand command);
    }
}
