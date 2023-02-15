namespace DualOperator.Models
{
    public class RunningApp
    {
        public string ApplicationPath { get; set; }
        public string Keyboard { get; set; }
        public IntPtr? WindowHandle { get; set; }
        public string WindowTitle { get; set; }
    }
}
