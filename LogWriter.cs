using log4net;
using System;
using System.Diagnostics;
using System.Text;

namespace DRS
{
    internal enum LogLevels
    {

        /// <summary>
        /// デバッグ、レベルが DEBUG の場合のみ出力されます。
        /// </summary>
        Debug,

        /// <summary>
        /// 確認、レベルが DEBUG、INFO の場合のみ出力されます。
        /// </summary>
        Info,

        /// <summary>
        /// 警告、レベルが DEBUG、INFO、WARN の場合のみ出力されます。
        /// </summary>
        Warn,

        /// <summary>
        /// エラー、レベルが DEBUG、INFO、WARN、ERROR の場合のみ出力されます。
        /// </summary>
        Error,

        /// <summary>
        /// 致命的なエラー、全てのレベルで出力されます。
        /// </summary>
        Fatal,

    }

    static class LogWriter
    {
        /// <summary>
        /// ロガー
        /// </summary>
        private static readonly ILog logger = LogManager.GetLogger("DRSLog");

        /// <summary>
        /// エラーログ出力クラス
        /// </summary>
        /// <param name="ex"></param>
        public static void LogOutputSystemError(Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            string className = "----";
            string methodName = "----";
            try
            {
                // StackFrameクラスをインスタンス化する
                StackFrame objStackFrame = new StackFrame(1);// フレーム数(1なら直接呼び出したメソッド)
                className = objStackFrame.GetMethod().ReflectedType.FullName;
                methodName = objStackFrame.GetMethod().Name;
            }
            catch { }
            // 呼び出し元のクラス名を取得する
            sb.Append("[").Append(className).Append("]");
            // 呼び出し元のメソッド名を取得する
            sb.Append("[").Append(methodName).Append("]");
            sb.Append("システムエラーが発生しました:");
            sb.Append(ex);

            Console.Error.WriteLine(sb.ToString());

            logger.Error(sb);
            sb.Clear();
        }

        /// <summary>
        /// ログファイルへの出力処理
        /// </summary>
        /// <param name="logLevels">ログレベル</param>
        /// <param name="args">出力文字列の配列</param>
        public static void LogOutput(LogLevels logLevels, params string[] args)
        {
            if (args == null || args.Length == 0) return;
            StringBuilder sb = new StringBuilder();
            string strMethodName = "-----";
            string[] spClass = { "-----" };
            try
            {
                // StackFrameクラスをインスタンス化する
                StackFrame objStackFrame = new StackFrame(1);// フレーム数(1なら直接呼び出したメソッド)
                                                             // 呼び出し元のメソッド名を取得する
                strMethodName = objStackFrame.GetMethod().Name;
                // 呼び出し元のクラス名を取得する
                string strClassName = objStackFrame.GetMethod().ReflectedType.Name;
                spClass = strClassName.Split('.');

            }
            catch { }

            // クラス名、メソッド名を追加する
            sb.Append("[").Append(spClass[spClass.Length - 1]).Append("]");
            sb.Append("[").Append(strMethodName).Append("]");
            for (int i = 0, max = args.Length; i < max; i++) sb.Append(args[i]);
            LogOutput(sb.ToString(), logLevels);
        }

        /// <summary>
        /// ログファイルへの出力処理
        /// </summary>
        /// <param name="message">ログ出力内容</param>
        /// <param name="logLevel">ログレベル</param>
        private static void LogOutput(string message, LogLevels logLevel)
        {
            try
            {
#if DEBUG
                Console.WriteLine(message);
#endif
                switch (logLevel)
                {
                    case LogLevels.Debug:
                        logger.Debug(message);
                        break;
                    case LogLevels.Info:
                        logger.Info(message);
                        break;
                    case LogLevels.Warn:
                        logger.Warn(message);
                        break;
                    case LogLevels.Error:
                        logger.Error(message);
                        break;
                    case LogLevels.Fatal:
                        logger.Fatal(message);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("LogWriteでのエラー");
                throw;
            }
        }
    }
}
