using DRS.HttpServers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using WatsonWebserver;
using WatsonWebserver.Core;
using HttpContextBase = WatsonWebserver.Core.HttpContextBase;
using HttpRequestBase = WatsonWebserver.Core.HttpRequestBase;

namespace DRS.WatsonWebserver
{

    public class WatsonController
    {
        private static Webserver server { set; get; }
        private static CancellationTokenSource WatsonCancell { set; get; } = null;

        /// <summary>
        /// サーバー利用ポートリスト
        /// </summary>
        private static int WatsonPorts { set; get; } = 8085;

        public WatsonController()
        {

        }

        /// <summary>
        /// サーバー起動処理
        /// </summary>
        /// <param name="port">ポート番号</param>
        /// <returns>false:起動失敗、true:正常起動</returns>
        public static DRSInfo Run(int port)
        {
            WatsonPorts = port;
            return Run();
        }

        /// <summary>
        /// サーバー起動処理
        /// </summary>
        /// <returns>false:起動失敗、true:正常起動</returns>
        private static DRSInfo Run()
        {
            StringBuilder sb = new StringBuilder();
            DRSInfo drsinfo = new DRSInfo();
            try
            {
                // ポート番号チェック
                if (WatsonPorts < 0 || WatsonPorts > 65535)
                {
                    LogWriter.LogOutput(LogLevels.Debug, "ポート番号 引数なし");
                    drsinfo.Status = DRSCodes.CHK_Port_IsNull;
                    return drsinfo;
                }
                // 利用中のポートを取得する
                IPGlobalProperties iPGlobal = IPGlobalProperties.GetIPGlobalProperties();
                var useTCPs = iPGlobal.GetActiveTcpListeners().AsEnumerable().Select(x => x.Port);

                // 引数のポート番号から、利用中のものを除外する
                var ports = new int[] { WatsonPorts }.Where(x => !useTCPs.Contains(x));


                // 利用可能なポートがない場合、Falseを返却する
                if (ports == null || ports.Count() <= 0)
                {
                    LogWriter.LogOutput(LogLevels.Debug, "利用可能ポートなし");
                    drsinfo.Status = DRSCodes.CHK_Port_NotExist;
                    return drsinfo;
                }
                var sslstr = ConfigurationManager.AppSettings["SSL"];
                bool.TryParse(sslstr, out bool sslFg);
                if (server != null)
                {
                    Stop();
                }

                WatsonCancell = new CancellationTokenSource();
                // メッセージの作成
                sb.AppendLine();
                sb.AppendLine("  サーバを起動しました。");
                sb.AppendLine("URL");

                // サーバーの準備

                // IPとポートの設定
                var usePort = ports.First();

                WebserverSettings settings = new WebserverSettings("127.0.0.1", usePort, sslFg);
                server = new Webserver(settings, MainThread);

                if (sslFg)
                {
                    sb.Append("　https");
                }
                else
                {
                    sb.Append("　http");
                }
                sb.Append("://127.0.0.1:").Append(usePort).AppendLine("");
                // サーバーの起動
                LogWriter.LogOutput(LogLevels.Debug, "Start Server Host:", server.Settings.Hostname, ":", server.Settings.Port.ToString());
                server.Start(WatsonCancell.Token);


            }
            catch (Exception ex)
            {
                sb.Clear();
                sb.AppendLine("  サーバの起動に失敗しました。").AppendLine();
                sb.AppendLine("$#$#$# 初期設定失敗 $#$#$#");
                sb.AppendLine().Append(ex.Message).AppendLine();
                DRSForm.SetMessage(sb.ToString(), true);
                LogWriter.LogOutputSystemError(ex);
                if (WatsonCancell != null)
                {
                    WatsonCancell.Cancel();
                }

                server = null;

                drsinfo.Status = DRSCodes.Exception;
                drsinfo.Exception = ex;
                return drsinfo;
            }
            DRSForm.SetMessage(sb.ToString(), true);
            drsinfo.Status = DRSCodes.Success;
            return drsinfo;
        }

        public static void Stop()
        {
            if (server != null && WatsonCancell != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine();
                sb.AppendLine("  サーバを停止しました。");
                sb.AppendLine();
                try
                {
                    WatsonCancell.Cancel();

                    server.Stop();
                    server.Dispose();

                    WatsonCancell.Dispose();
                }
                catch (Exception ex)
                {
                    LogWriter.LogOutputSystemError(ex);
                }
                DRSForm.SetMessage(sb.ToString(), true);
            }
            server = null;
        }

        /// <summary>
        /// HTTP要求応答処理
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        private static async Task MainThread(HttpContextBase ctx)
        {
            StringBuilder formLog = new StringBuilder();
            formLog.Append("受信時刻:");
            formLog.AppendLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"));

            ctx.Response.Headers.Add("Server", "DmSVForm");
            ctx.Response.Headers.Add("Accept-Language", "ja, en");

            // faviconの要求
            if (ctx.Request.Url.Full.EndsWith("favicon.ico"))
            {

                if (File.Exists(".\\favicon.ico"))
                {
                    ctx.Response.StatusCode = 200;
                    formLog.AppendLine("favicon.icoの応答");
                    ctx.Response.Headers.Add("Content-Type", "image/x-icon");
                    await ctx.Response.Send(File.ReadAllBytes(".\\favicon.ico"));
                }
                else
                {
                    ctx.Response.StatusCode = 404;
                    formLog.AppendLine("favicon.icoの要求がありましたがファイルがありません。");
                    await ctx.Response.Send("");
                }
                DRSForm.SetMessage(formLog.ToString(), true);
                return;
            }

            // 要求の取得
            formLog.Append(GetRequestParam(ctx, out string reqBody, out string reqContType));
            // 応答内容の取得
            ParamJSON resParamjson = GetResponse(ctx, reqBody, reqContType);

            // 応答情報なし
            if (resParamjson == null || resParamjson.Values == "@null")
            {
                // ステータスコードはGetResponseで設定する
                formLog.AppendLine("応答情報が存在しません。");
                await ctx.Response.Send("");

                DRSForm.SetMessage(formLog.ToString(), true);
                return;
            }

            // 応答内容(バイト配列)
            byte[] res = null;
            // 応答内容 ログに表示するのでここで宣言する
            string resFileValue = string.Empty;
            // 処理が重いものがあるので、別タスクで実行する
            var setResThread = Task.Run(() =>
            {
                try
                {
                    // 応答の読込
                    resFileValue = GetFile(resParamjson.Values, ctx, reqBody);
                    // 応答に設定
                    res = resParamjson.GetEncoding().GetBytes(resFileValue);

                    // 応答情報の設定

                    // ステータスの設定
                    ctx.Response.StatusCode = resParamjson.Status_Int;

                    // ヘッダー情報のカスタマイズ
                    string mtType = resParamjson.Type;
                    if (mtType == "ANY" || string.IsNullOrEmpty(mtType))
                    {
                        mtType = "*";
                    }
                    ctx.Response.Headers.Add("Access-Control-Allow-Methods", string.Join(",", mtType));
                    // エンコード指定があれば
                    if (!string.IsNullOrWhiteSpace(resParamjson.Enc))
                    {
                        try
                        {
                            // エンコードの正式名称を取得する
                            var enc = Encoding.GetEncoding(resParamjson.Enc);
                            ctx.Response.Headers.Add("Accept-Charset", enc.BodyName);
                        }
                        catch (Exception e)
                        {
                            // 変換失敗時は、記載されている名称のまま利用する
                            LogWriter.LogOutputSystemError(e);
                            ctx.Response.Headers.Add("Accept-Charset", resParamjson.Enc);
                        }
                    }
                    // コンテンツタイプ
                    if (!string.IsNullOrWhiteSpace(resParamjson.ContentType))
                    {
                        ctx.Response.Headers.Add("Content-Type", resParamjson.ContentType);
                    }

                }
                catch (Exception ex)
                {
                    // タスク内でのエラー時
                    LogWriter.LogOutputSystemError(ex);
                    ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    resFileValue = "Response build NG " + ex.Message;
                    res = Encoding.UTF8.GetBytes(resFileValue);
                }

            });

            // 待機時間を取得する
            string wait = resParamjson.WaitTime;
            int waitTime = -1;
            if (!string.IsNullOrWhiteSpace(wait) && wait != "0")
            {
                wait = wait.ToUpper().Trim();
                if (wait.EndsWith("MS") || wait.EndsWith("ミリ秒"))
                {
                    // Nms Nミリ秒
                    int.TryParse(wait.Replace("MS", "").Replace("ミリ秒", ""), out waitTime);
                }
                else if (wait.EndsWith("S") || wait.EndsWith("秒"))
                {
                    // Ns N秒
                    int.TryParse(wait.Replace("S", "").Replace("秒", ""), out waitTime);
                    // ミリ秒に直す
                    waitTime *= 1000;
                }
                else
                {
                    // 上記以外 mとかhは無視して変換に失敗させる
                    int.TryParse(wait, out waitTime);
                }
                // 0より大きければ待機する
                if (waitTime > 0)
                {
                    await Task.Delay(waitTime);
                }
            }

            formLog.Append("応答時刻:");
            formLog.Append(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"));
            if (waitTime > 0)
            {
                formLog.Append(" ");
                formLog.Append(waitTime);
                formLog.Append("ms待機しました。");
            }

            await setResThread;
            // 念のためのフォールバック
            if (res == null)
            {
                res = Array.Empty<byte>();
            }
            formLog.AppendLine();
            formLog.Append("#### 応答 ####  Status:").Append(ctx.Response.StatusCode).AppendLine();

            // 情報表示の場合、応答のヘッダーを表示する
            if (DRSForm.GetInfo())
            {
                // ヘッダー情報表示する場合
                formLog.AppendLine(" Headers:");
                try
                {
                    var headers = ctx.Response.Headers;
                    if (headers != null)
                    {
                        int hedCnt = headers.Count;
                        for (int i = 0; i < hedCnt; i++)
                        {
                            var headKey = headers.Keys[i];
                            formLog.Append(i + 1 != hedCnt ? " ├ " : " └ ");
                            formLog.Append(headKey);
                            formLog.Append(":");
                            formLog.AppendLine(headers.Get(headKey));
                        }
                    }
                }
                catch (Exception e)
                {
                    LogWriter.LogOutputSystemError(e);
                }
                formLog.AppendLine();
            }
            formLog.AppendLine(resFileValue);

            await ctx.Response.Send(res);

            DRSForm.SetMessage(formLog.ToString(), true);
        }

        /// <summary>
        /// 戻り値の取得
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="reqBody"></param>
        /// <returns>
        /// null:応答データ無し
        /// ParamJSON:応答データ
        /// </returns>
        private static ParamJSON GetResponse(HttpContextBase ctx, string reqBody, string contentType)
        {
            // メソッドタイプ
            string Type = ctx.Request.Method.ToString().ToUpper();
            try
            {
                // URLの取得
                string reqURL = ctx.Request.Url.RawWithoutQuery;
                // リクエストメソッド
                string httpMethod = ctx.Request.Method.ToString().ToUpper();

                // URLをデコードする
                reqURL = HttpUtility.UrlDecode(reqURL);

                // faviconの要求の場合処理を抜ける
                if (reqURL.EndsWith("favicon.ico"))
                {
                    return null;
                }
                // Bodyの内容を変換する
                var convBodyTask = Task.Run(() =>
                {
                    return ConvBody2Dic(ctx.Request, contentType, reqBody);
                });

                // 応答内容一覧の取得
                ConcurrentBag<ParamJSON> paramJSONs = DRSForm.GetResponsJsons();
                // 処理結果格納用
                ConcurrentQueue<ParamJSON> resParams = new ConcurrentQueue<ParamJSON>();

                // Bodyの変換結果を取得する
                ConcurrentDictionary<string, string> body = convBodyTask.Result;

                // 応答内容からスレッド処理で検索する
                Parallel.ForEach(paramJSONs, param => GetParamJSON(param, Type, body, reqURL, resParams));

                // 先頭から順に応答内容の候補を確認する
                // 候補がなくなった場合、最後のデータを返す
                ParamJSON res = null;
                while (!resParams.IsEmpty)
                {
                    if (!resParams.TryDequeue(out res))
                    {
                        continue;
                    }

                    // ANY以外、Bodyチェックがある場合処理を抜ける
                    if (res.Type != "ANY" || !string.IsNullOrWhiteSpace(res.CheckBody))
                    {
                        break;
                    }
                }

                // 候補のデータがある場合、保持している内容を返却する
                if (res != null && res.Values != "@null")
                {
                    LogWriter.LogOutput(LogLevels.Info, "応答情報あり Status:", res.Status_Str);

                    return res;
                }
                else
                {
                    LogWriter.LogOutput(LogLevels.Info, "応答情報なし Status:404");
                    ctx.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }
            }
            catch (Exception ex)
            {
                // 例外が発生した場合
                LogWriter.LogOutput(LogLevels.Info, "応答内容 Status:500");
                ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                LogWriter.LogOutputSystemError(ex);
            }
            // データ無し、エラー発生時
            return null;
        }

        private static void GetParamJSON(ParamJSON param, string Type, ConcurrentDictionary<string, string> body, string reqURL, ConcurrentQueue<ParamJSON> resParams)
        {
            if (param == null)
            {
                return;
            }

            var _type = param.Type;
            // 応答と同じタイプ以外の場合、次のデータへ
            if (_type != "ANY" && Type != _type)
            {
                return;
            }

            var _url = param.URL;

            if (!string.IsNullOrWhiteSpace(param.CheckBody))
            {
                // Bodyの内容を検証する場合
                if (body == null) return;
                if (reqURL != _url && reqURL != _url + "/" && reqURL != _url + "?") return;

                // 分割して配列に格納する
                ConcurrentDictionary<string, string> checkBodys = new ConcurrentDictionary<string, string>();
                Parallel.ForEach(param.CheckBody.Split(','), s =>
                {
                    var ss = s.Trim().Split('=');
                    if (ss == null || ss.Length <= 1)
                    {
                        return;
                    }
                    else if (ss.Length >= 3)
                    {
                        checkBodys.TryAdd(ss[0], string.Join("=", ss, 1, ss.Count()));
                    }
                    else
                    {
                        checkBodys.TryAdd(ss[0], ss[1]);
                    }
                });

                // 内容の確認（全キー一致を判定する）
                bool fg = checkBodys.Count > 0; // チェック項目が無ければ不一致扱い
                foreach (var chb in checkBodys)
                {
                    if (body.ContainsKey(chb.Key) && body.TryGetValue(chb.Key, out string vl) && vl == chb.Value)
                    {
                        continue; // このキーは一致、次へ
                    }
                    // 1 つでも不一致ならその時点で不成立
                    fg = false;
                    break;
                }
                // 全て一致した場合、候補に格納する
                if (fg)
                {
                    resParams.Enqueue(param);
                }

                // チェックが必要な応答はここで処理を抜ける
                return;
            }

            // 要求URLとデータのURLが同じ場合
            if (reqURL == _url || reqURL == _url + "/")
            {
                resParams.Enqueue(param);
            }
            else if (_url.Split('?')[0] == reqURL)
            {
                // 部分一致する場合、一時保持する
                resParams.Enqueue(param);
                LogWriter.LogOutput(LogLevels.Debug, "類似データ保持 URL:", _url);
            }
        }

        /// <summary>
        /// リクエストBodyの内容をDictionaryに変換する
        /// </summary>
        /// <param name="request">リクエストベース</param>
        /// <param name="contentType">コンテンツタイプ</param>
        /// <param name="reqBody">リクエストBody</param>
        /// <returns>
        /// null:変換失敗又はデータ無し
        /// ConcurrentDictionary:Body変換後連想配列
        /// </returns>
        private static ConcurrentDictionary<string, string> ConvBody2Dic(HttpRequestBase request, string contentType, string reqBody)
        {
            try
            {
                contentType = contentType.ToUpper().Trim();
                // form-dataの場合
                if (contentType.IndexOf("FORM-DATA") != -1)
                {
                    contentType = "FORM";
                }
                switch (contentType)
                {
                    case "APPLICATION/JSON":
                        return new ConcurrentDictionary<string, string>(JsonSerializer.Deserialize<Dictionary<string, string>>(reqBody));
                    case "APPLICATION/X-WWW-FORM-URLENCODED":
                    case "FORM":
                    case "TEXT/PLAIN":
                        string tgBody = string.Empty;
                        // 変換対象を設定する
                        if (request.HeaderExists("Content-Disposition") && contentType == "FORM")
                        {
                            // ヘッダーから情報を取得する
                            tgBody = string.Join("&", request.Headers["Content-Disposition"].Split(';'));
                        }
                        else
                        {
                            tgBody = reqBody;
                        }
                        // 変換後Body格納用
                        var resDictionary = new ConcurrentDictionary<string, string>();
                        // CSV(改行)、http(&区切り)
                        var bodySplit = tgBody.Replace("\r", "").Replace("\n", "&").Split('&');

                        void addDic(string[] sArray, string separator)
                        {
                            if (sArray == null || sArray.Length <= 1)
                            {
                                // 有効データ無し
                                return;
                            }
                            else if (sArray.Length >= 3)
                            {
                                // 3個以上に分割された場合、キー以外を結合する
                                resDictionary.TryAdd(sArray[0], string.Join(separator, sArray, 1, sArray.Count()));
                            }
                            else
                            {
                                // 2個に分割された場合、キーと値として登録する
                                resDictionary.TryAdd(sArray[0], sArray[1]);
                            }
                        }
                        ;
                        // 分割して配列に格納する
                        Parallel.ForEach(bodySplit, bodystr =>
                        {
                            if (bodystr.IndexOf("=") != -1)
                            {
                                // イコール区切り
                                var ss = bodystr.Trim().Split('=');
                                addDic(ss, "=");
                            }
                            else if (bodystr.IndexOf(",") != -1)
                            {
                                // カンマ区切り
                                var ss = bodystr.Trim().Split(',');
                                addDic(ss, ",");
                            }
                        });

                        if (resDictionary.Count != 0)
                        {
                            return resDictionary;
                        }

                        return null;
                    // 対象外 
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                LogWriter.LogOutputSystemError(e);
            }
            return null;

        }

        /// <summary>
        /// 応答内容を返却する
        /// ファイルパスの場合、ファイルの内容を取得する
        /// </summary>
        /// <param name="resValue">応答値/ファイルパス</param>
        /// <param name="ctx">連携内容</param>
        /// <param name="msg">メッセージ</param>
        /// <returns>文字列</returns>
        private static string GetFile(string resValue, HttpContextBase ctx, string reqBody)
        {
            LogWriter.LogOutput(LogLevels.Debug, "応答内容取得 Value:", resValue);
            string dllClassName = "";
            // DLL用のパス分割
            if (resValue.Contains("@"))
            {
                // ファイルパス@ネームスペース.クラス名
                var work = resValue.Split('@');
                dllClassName = work[1];
                resValue = work[0];
            }
            // 文字列に\が含まれ、ファイルが存在する場合
            if (resValue.Contains("\\") && File.Exists(resValue))
            {

                // ファイルとして処理をする
                if (Path.GetExtension(resValue) == ".dll")
                {
                    // 【注意】ここでは応答に設定されている任意のDLLを実行する。
                    // 実質的に任意コード実行となるため、信頼できない DLL を
                    // 読み込ませない運用を前提とすること（社内検証用途限定）。
                    try
                    {
                        // DLLを実行する
                        Assembly asm = Assembly.LoadFrom(resValue);
                        dynamic instance = Activator.CreateInstance(asm.GetType(dllClassName));
                        var full = Path.GetFullPath(".\\");
                        if (string.IsNullOrWhiteSpace(reqBody))
                        {
                            reqBody = "";
                        }

                        resValue = instance.Main(new string[] { reqBody, resValue, full });
                    }
                    catch (Exception ex)
                    {
                        LogWriter.LogOutputSystemError(ex);
                        // 読み込みに失敗した場合、エラメッセージを選択する
                        resValue = "Dll exec NG " + ex.Message;
                        ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    }
                }
                else
                {
                    try
                    {
                        // ファイル内容を取得する
                        resValue = File.ReadAllText(resValue);
                    }
                    catch (Exception ex)
                    {
                        LogWriter.LogOutputSystemError(ex);
                        // 読み込みに失敗した場合、エラメッセージを選択する
                        resValue = "File Read NG " + ex.Message;
                        ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    }
                }
            }
            LogWriter.LogOutput(LogLevels.Info, "応答内容 Value:", resValue.Replace("\r\n", ""));
            return resValue;
        }

        /// <summary>
        /// 内容表示
        /// </summary>
        /// <param name="ctx"></param>
        private static string GetRequestParam(HttpContextBase ctx, out string reqBody, out string reqContType)
        {
            // 要求内容
            string result = "nothing";
            reqContType = "text/plain";

            // 情報表示フラグ
            bool getinfoFg = DRSForm.GetInfo();
            Encoding enc = null;

            StringBuilder reqLog = new StringBuilder();
            reqLog.AppendLine("#### 要求 ####");
            reqLog.Append(getinfoFg ? " ├ " : " └ ");
            reqLog.Append("URL:").AppendLine(ctx.Request.Url.Full);
            if (getinfoFg)
            {
                reqLog.AppendLine(" ├ Headers:");
            }
            // ヘッダー情報の取得処理
            try
            {
                var headers = ctx.Request.Headers;
                if (headers != null)
                {
                    int hedCnt = headers.Count;
                    for (int i = 0; i < hedCnt; i++)
                    {
                        var headKey = headers.Keys[i];
                        if (getinfoFg)
                        {
                            // 応答情報表示の場合
                            reqLog.Append(i + 1 != hedCnt ? " │ ├ " : " │ └ ");
                            reqLog.Append(headKey);
                            reqLog.Append(":");
                            reqLog.AppendLine(headers.Get(headKey));
                        }

                        if (enc == null && headKey == "Content-Type")
                        {
                            // エンコード情報を取得する
                            string[] ctypes = headers[i].Split(';');
                            string charset = "utf-8";
                            var wk = ctypes.Where(x => x.ToUpper().Contains("CHARSET"));
                            if (wk.Any())
                            {
                                charset = wk.First().Split('=')[1];
                            }

                            enc = ParamJSON.GetEncoding(charset);
                            reqContType = ctypes[0];
                            LogWriter.LogOutput(LogLevels.Debug, "コンテンツタイプ:", ctypes[0], " エンコード:", enc.BodyName);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogWriter.LogOutputSystemError(e);
            }
            if (enc == null)
            {
                enc = Encoding.UTF8;
            }

            if (getinfoFg)
            {
                // 応答情報表示の場合
                reqLog.AppendLine(" │");
                reqLog.Append(" └ Type:");
                reqLog.AppendLine(ctx.Request.Method.ToString());
            }
            // リクエスト内容の取得
            using (Stream reqStream = ctx.Request.Data)
            {
                using (StreamReader sr = new StreamReader(reqStream, enc))
                {
                    result = sr.ReadToEnd();
                    sr.Close();
                }
                reqStream.Close();
            }
            string url = HttpUtility.UrlDecode(ctx.Request.Url.RawWithQuery.ToString());
            string Type = ctx.Request.Method.ToString().ToUpper();
            if (Type == "GET" && url.IndexOf('?') > 0)
            {
                // GETでURLにパラメータがある場合、リクエスト内容を書き換える
                result = url.Split('?')[1];
            }
            // 要求内容がURLエンコードされている場合の対策
            reqBody = HttpUtility.UrlDecode(result);
            reqLog.AppendLine("# Value");
            reqLog.AppendLine(reqBody);
            reqLog.AppendLine(new string('-', (int)(DRSForm.Txt_Cnt * 0.8)));
            return reqLog.ToString();
        }
    }
}
