namespace 皇冠娱乐
{
    /// <summary>
    /// 控制台辅助类
    /// </summary>
    public static class Log
    {
        public static void WriteLine(string text)
        {
            Console.ResetColor();
            Console.WriteLine("\r\n" + DateTime.Now + "  提示 : " + text);
            Console.ResetColor();
        }

        public static void Warning(string text)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\r\n" + DateTime.Now + "  警告 : " + text);
            Console.ResetColor();
        }

        public static void Error(string text)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\r\n" + DateTime.Now + "  出错 : " + text);
            Console.ResetColor();
        }
    }
}
