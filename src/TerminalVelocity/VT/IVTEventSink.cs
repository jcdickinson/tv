namespace TerminalVelocity.VT
{
    public interface IVTEventSink
    {
        void OnPrint(in VTPrintAction print);
        void OnExecute(in VTExecuteAction execute);
        void OnHook(in VTHookAction hook);
        void OnPut(in VTPutAction put);
        void OnUnhook(in VTUnhookAction unhook);
        void OnCsiDispatch(in VTCsiDispatchAction csiDispatch);
        void OnOscDispatch(in VTOscDispatchAction oscDispatch);
        void OnEscDispatch(in VTEscDispatchAction escDispatch);
    }
}