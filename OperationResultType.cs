namespace AudioTagger
{
    public enum OperationResultType
    {
        Neutral,
        Success,
        Failure,
        Cancelled,
        Unknown
    }

    public class ResultSymbols
    {
        public static string Neutral { get { return "- " ; } }
        public static string Success { get { return "◯ "; } }
        public static string Failure { get { return "× "; } }
        public static string Cancelled { get { return "＊ "; } }
        public static string Unknown { get { return "? "; } }
    }
}
