namespace DualOperator.Helpers
{
    public class RawInputEventArg : EventArgs
    {
        public RawInputEventArg(KeyPressEvent arg)
        {
            KeyPressEvent = arg;
        }

        public KeyPressEvent KeyPressEvent { get; private set; }
    }
}
