namespace CQRSPipeline.Framework
{
    public sealed class VoidResult
    {
        public static readonly VoidResult Value = new VoidResult();

        private VoidResult() { }
    }
}
