namespace AsyncPipelineDemo
{
    public sealed class NullValue
    {
        public static readonly NullValue Instance = new NullValue();

        private NullValue() { }
    }
}
