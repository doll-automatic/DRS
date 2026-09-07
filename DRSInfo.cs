using System;
using System.Text;

namespace DRS
{
    public enum DRSCodes
    {
        /// <summary>
        /// 初期値
        /// </summary>
        None = -1,
        /// <summary>
        /// 正常
        /// </summary>
        Success,
        /// <summary>
        /// チェックエラー:ポート未設定
        /// </summary>
        CHK_Port_IsNull,
        /// <summary>
        /// チェックエラー:数値変換エラー
        /// </summary>
        CHK_Port_NotNumber,
        /// <summary>
        /// チェックエラー:ポート範囲外エラー
        /// </summary>
        CHK_Port_OutOfRange,
        /// <summary>
        /// チェックエラー:ポート番号利用中
        /// </summary>
        CHK_Port_NotExist,

        /// <summary>
        /// サーバー停止失敗
        /// </summary>
        Exception

    }

    public class DRSInfo
    {
        /// <summary>
        /// 発生したエクセプションを保持
        /// </summary>
        public Exception Exception { get; set; } = null;
        /// <summary>
        /// 状態コードの設定
        /// </summary>
        public DRSCodes Status { set; get; } = DRSCodes.None;
        /// <summary>
        /// メッセージの取得
        /// </summary>
        /// <returns></returns>
        public string GetMessage()
        {
            switch (Status)
            {
                case DRSCodes.None:
                    return "初期状態";
                case DRSCodes.Success:
                    return "正常";
                case DRSCodes.CHK_Port_IsNull:
                    return "ポート番号が未設定です。";
                case DRSCodes.CHK_Port_NotNumber:
                    return "ポート番号に数値以外が設定されています。";
                case DRSCodes.CHK_Port_NotExist:
                    return "利用可能なポート番号が見つかりませんでした。";
                case DRSCodes.CHK_Port_OutOfRange:
                    return "ポート番号が有効な範囲内ではありません。(1～65535の範囲内で設定してください)";

                default:
                    return "--No Message --";
            }
        }

        /// <summary>
        /// ステータス、メッセージを表示する、エラーがある場合そのメッセージも出力する
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("Status:").Append((int)Status);
            sb.Append(" Message:").Append(GetMessage());
            if (Exception != null)
            {
                sb.Append(" Exception:").AppendLine(Exception.Message);
                sb.Append(Exception);
            }
            return sb.ToString();
        }
    }
}
