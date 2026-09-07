using System.Collections.Concurrent;
using System.Data;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DRS.HttpServers
{
    /// <summary>
    /// レスポンス内容保持クラス
    /// </summary>
    [DataContract]
    class ParamJSON
    {
        /// <summary>
        /// 要求URL
        /// </summary>
        [DataMember] public string URL { get; set; }
        /// <summary>
        /// 応答内容
        /// </summary>
        [DataMember] public string Values { get; set; }
        /// <summary>
        /// 要求のHttpMethod
        /// ANY,GET,POST
        /// </summary>
        [DataMember] public string Type { get; set; }
        /// <summary>
        /// ステータスコード(数値)
        /// </summary>
        public int Status_Int { private set; get; }
        /// <summary>
        /// ステータスコード(文字列)
        /// </summary>
        public string Status_Str { private set; get; }
        /// <summary>
        /// ステータスコード
        /// </summary>
        [DataMember]
        public string Status
        {
            private get => Status_Str;
            set
            {
                int code = (int)HttpStatusCode.InternalServerError;
                string _st = "500";

                if (!string.IsNullOrWhiteSpace(value))
                {
                    _st = value;
                }
                if (int.TryParse(_st, out int tmp))
                {
                    code = tmp;
                }

                Status_Str = _st;
                Status_Int = code;
            }
        }
        /// <summary>
        /// 応答の文字コード
        /// </summary>
        [DataMember] public string Enc { get; set; }
        /// <summary>
        /// 処理の待機時間
        /// </summary>
        [DataMember] public string WaitTime { get; set; }
        /// <summary>
        /// 応答時のコンテンツタイプ
        /// </summary>
        [DataMember] public string ContentType { get; set; }
        /// <summary>
        /// ボディー内容チェック
        /// </summary>
        [DataMember] public string CheckBody { set; get; }

        private static readonly Encoding utf8 = new UTF8Encoding(false);
        private static readonly Encoding utf8Bom = new UTF8Encoding(true);
        /// <summary>
        /// エンコード取得
        /// </summary>
        /// <returns>Encoding</returns>
        public Encoding GetEncoding()
        {
            return GetEncoding(Enc);
        }

        /// <summary>
        /// エンコード取得
        /// </summary>
        /// <returns>Encoding</returns>
        public static Encoding GetEncoding(string encName)
        {
            Encoding enc;
            switch (encName.ToUpper().Trim().Replace("-", ""))
            {
                case "UTF8":
                case "UTF8 BOM":
                case "UTF8BOM":
                    enc = utf8;
                    break;
                case "SJIS":
                    enc = Encoding.GetEncoding("Shift-JIS");
                    break;
                case "JIS":
                    enc = Encoding.GetEncoding("iso-2022-jp");
                    break;
                case "ASCII":
                default:
                    enc = Encoding.ASCII;
                    break;
            }
            return enc;
        }
    }

    /// <summary>
    /// 設定JSONの変換処理クラス
    /// </summary>
    class ParamConv
    {
        /// <summary>
        /// DataTableからConcurrentBagに変換する処理
        /// </summary>
        /// <param name="dt">応答内容テーブル</param>
        /// <returns>応答内容データ</returns>
        internal static ConcurrentBag<ParamJSON> GetList(DataTable dt)
        {
            ConcurrentBag<ParamJSON> lst = new ConcurrentBag<ParamJSON>();
            Parallel.ForEach(dt.AsEnumerable(), row =>
            {
                ParamJSON param = new ParamJSON
                {
                    Status = row["Status"].ToString(),
                    Type = row["Type"].ToString(),
                    URL = row["URL"].ToString(),
                    Values = row["Values"].ToString(),
                    Enc = row["Enc"].ToString(),
                    WaitTime = row["WaitTime"].ToString(),
                    ContentType = row["ContentType"].ToString(),
                    CheckBody = row["CheckBody"].ToString(),
                };
                lst.Add(param);
            });
            return lst;
        }

        /// <summary>
        /// データテーブル変換処理
        /// </summary>
        /// <param name="lst">応答内容リスト</param>
        /// <returns>応答内容テーブル</returns>
        internal static DataTable GetDataTable(ConcurrentBag<ParamJSON> lst)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("URL", typeof(string));
            dt.Columns.Add("Type", typeof(string));
            dt.Columns.Add("Status", typeof(string));
            dt.Columns.Add("WaitTime", typeof(string));
            dt.Columns.Add("Values", typeof(string));
            dt.Columns.Add("Enc", typeof(string));
            dt.Columns.Add("ContentType", typeof(string));
            dt.Columns.Add("CheckBody", typeof(string));
            if (lst != null)
            {
                foreach (var param in lst)
                {
                    DataRow row = dt.NewRow();
                    row["Status"] = param.Status_Str;
                    row["Enc"] = param.GetEncoding().BodyName;
                    row["URL"] = param.URL.Trim();
                    row["Values"] = param.Values;
                    row["Type"] = param.Type.Trim().ToUpper();
                    row["WaitTime"] = param.WaitTime;
                    row["ContentType"] = param.ContentType;
                    row["CheckBody"] = param.CheckBody;
                    dt.Rows.Add(row);
                }
            }
            return dt;
        }
    }
}
