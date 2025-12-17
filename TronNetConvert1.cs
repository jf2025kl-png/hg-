
internal class TronNetConvert
{
    internal static long ToSun(decimal trxAmount)
    {
        return (long)(trxAmount * 1_000_000M);
    }
}