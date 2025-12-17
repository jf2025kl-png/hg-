internal   class TronConvert
{
    // TRX → SUN
    public static long ToSun(decimal trxAmount)
    {
        return (long)(trxAmount * 1_000_000M);
    }

    // SUN → TRX
    public static decimal ToTrx(long sunAmount)
    {
        return sunAmount / 1_000_000M;
    }
}
