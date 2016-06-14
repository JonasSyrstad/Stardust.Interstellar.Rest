namespace Stardust.Interstellar.Rest.Extensions
{
    public interface IInputInterceptor
    {
        void Intercept(object[] inputs, StateDictionary getState, out bool cancel, out string cancellationMessage);

        object Intercept(object result, StateDictionary getState);
    }
}