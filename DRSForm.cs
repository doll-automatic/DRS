using DRS.HttpServers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DRS
{
    public partial class DRSForm : Form
    {
        /// <summary>
        /// ボタン Start
        /// </summary>
        private readonly string BTN_START = "Start";
        /// <summary>
        /// ボタン Strop
        /// </summary>
        private readonly string BTN_STOP = "Stop";
        /// <summary>
        /// static参照用オブジェクト
        /// </summary>
        private static DRSForm fm = null;
        /// <summary>
        /// メッセージに表示する線の文字数
        /// </summary>
        internal static int Txt_Cnt { get; private set; } = 0;
        /// <summary>
        /// 応答内容
        /// </summary>
        private static ConcurrentBag<ParamJSON> ParamJSON { set; get; }

        public DRSForm()
        {
            InitializeComponent();
            fm = this; // staticメソッド用
        }

        /// <summary>
        /// Formロード処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DmSVForm_Load(object sender, EventArgs e)
        {
            // 起動時のサイズで調整
            DmSVForm_Resize(null, EventArgs.Empty);

            ConcurrentBag<ParamJSON> paramJSONs = new ConcurrentBag<ParamJSON>();
            if (File.Exists(@".\Response.json"))
            {
                try
                {
                    // 応答内容を取得し、登録する
                    foreach (var pm in JsonSerializer.Deserialize<List<ParamJSON>>(File.ReadAllText(@".\Response.json")))
                    {
                        paramJSONs.Add(pm);
                    }

                }
                catch (Exception ex)
                {
                    LogWriter.LogOutputSystemError(ex);
                    SetMessage("  読込失敗 Response.json " + ex.Message);
                    SetMessage("", true);
                }
            }

            // 追加の応答内容
            var addConf = GetConfValue("EXResFiles", null);
            if (!string.IsNullOrWhiteSpace(addConf) && Directory.Exists(addConf))
            {
                foreach (string fl in Directory.GetFiles(addConf))
                {
                    if (!fl.ToUpper().EndsWith(".JSON"))
                    {
                        continue;
                    }

                    try
                    {
                        // 応答内容を取得し、登録する
                        foreach (var pm in JsonSerializer.Deserialize<List<ParamJSON>>(File.ReadAllText(fl)))
                        {
                            paramJSONs.Add(pm);
                        }
                        SetMessage("  追加読込 " + fl);
                    }
                    catch (Exception ex)
                    {
                        SetMessage("  追加読込失敗 " + fl + " " + ex.Message);
                    }
                }
            }

            ParamJSON = paramJSONs;
            // Formに関連付ける
            dataGridView1.DataSource = ParamConv.GetDataTable(ParamJSON);

            // 画面表示内容の調整
            dataGridView1.Columns["Status"].Width = 50;
            dataGridView1.Columns["Type"].Width = 60;
            dataGridView1.Columns["URL"].Width = 300;
            dataGridView1.Columns["Values"].Width = 350;
            dataGridView1.Columns["WaitTime"].Width = 80;
            dataGridView1.Columns["Enc"].Width = 80;
            dataGridView1.Columns["Enc"].HeaderText = "Charset";
            dataGridView1.Columns["ContentType"].Width = 150;
            int n = 0;
            dataGridView1.Columns["URL"].DisplayIndex = n++;
            dataGridView1.Columns["Type"].DisplayIndex = n++;
            dataGridView1.Columns["Status"].DisplayIndex = n++;
            dataGridView1.Columns["Values"].DisplayIndex = n++;
            dataGridView1.Columns["WaitTime"].DisplayIndex = n++;
            dataGridView1.Columns["Enc"].DisplayIndex = n++;
            dataGridView1.Columns["ContentType"].DisplayIndex = n++;
        }

        /// <summary>
        /// Formサイズ変更時処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DmSVForm_Resize(object sender, EventArgs e)
        {
            Txt_Cnt = (Txt_Message.Width - 30) / 6;
        }

        /// <summary>
        /// サーバの開始、停止処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_STSP_Click(object sender, EventArgs e)
        {

            try
            {
                if (Btn_STSP.Text == BTN_START)
                {
                    // Start

                    // 応答情報の更新
                    ParamJSON = ParamConv.GetList((DataTable)fm.dataGridView1.DataSource);
                    var code = RunServer(PortNum.Value);
                    switch (code.Status)
                    {
                        case DRSCodes.Success: // 起動成功

                            PortNum.Enabled = false;
                            Btn_STSP.Text = BTN_STOP;
                            // 起動状態が分かるようにライトグリーンにする
                            splitContainer1.BackColor = Color.LightGreen;
                            panel1.BackColor = Color.LightGreen;
                            break;

                        case DRSCodes.CHK_Port_IsNull: // ポート未入力
                            MessageBox.Show(code.GetMessage(), "ポート番号未設定", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            break;

                        case DRSCodes.CHK_Port_NotNumber: // 数値以外
                            MessageBox.Show(code.GetMessage(), "ポート番号不正", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            break;

                        case DRSCodes.CHK_Port_OutOfRange: // ポート範囲外
                            MessageBox.Show(code.GetMessage(), "ポート番号不正", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            break;

                        case DRSCodes.CHK_Port_NotExist: // ポート利用不可
                            MessageBox.Show(code.GetMessage(), "ポート利用不可", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            break;

                        // エラーまたは想定外のコード値
                        case DRSCodes.Exception:
                            LogWriter.LogOutput(LogLevels.Error, code.ToString());
                            throw code.Exception;
                        default:
                            throw new Exception("想定外の状態");
                    }

                }
                else
                {
                    // Stop
                    WatsonWebserver.WatsonController.Stop();

                    Btn_STSP.Text = BTN_START;
                    PortNum.Enabled = true;

                    splitContainer1.BackColor = Color.LightGray;
                    panel1.BackColor = Color.WhiteSmoke;
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogOutputSystemError(ex);
                SetMessage(ex.Message);
                PortNum.Enabled = true;
                string type;
                if (Btn_STSP.Text == BTN_START)
                {
                    type = "起動";
                }
                else
                {
                    type = "停止";
                }
                MessageBox.Show($"サーバの{type}に失敗しました。", $"{type}処理失敗", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                splitContainer1.BackColor = Color.Red;
                panel1.BackColor = Color.Red;
            }
        }

        /// <summary>
        /// ポート番号をチェックし、サーバーを起動する
        /// </summary>
        /// <param name="strPort">ポート番号</param>
        /// <returns></returns>
        private static DRSInfo RunServer(decimal port)
        {
            DRSInfo dmSv = new DRSInfo();
            if (port <= 0 || 65535 < port)
            {
                dmSv.Status = DRSCodes.CHK_Port_OutOfRange;
                return dmSv;
            }
            // サーバーの起動
            return WatsonWebserver.WatsonController.Run((int)port);
        }

        #region Staticメソッド
        /// <summary>
        /// メッセージの表示処理
        /// </summary>
        /// <param name="msg">出力メッセージ</param>
        /// <param name="lineFg">区切り線フラグ</param>

        public static void SetMessage(string msg, bool lineFg = false)
        {
            // FormがNull、または処理終了後の場合、処理を抜ける
            if (fm == null || fm.IsDisposed)
            {
                return;
            }

            if (fm.InvokeRequired)
            {
                fm.BeginInvoke(new Action(() => SetMessage(msg, lineFg)));
                return;
            }

            // メッセージの最大文字数
            const int maxLen = 50000;

            // 末尾に追記する（既存テキストを StringBuilder に積み直さない）
            fm.Txt_Message.AppendText(msg + Environment.NewLine);
            if (lineFg)
            {
                fm.Txt_Message.AppendText(new string('=', Txt_Cnt) + Environment.NewLine);
            }

            // 最大文字数を超えたら先頭（古い側）を削る
            if (fm.Txt_Message.TextLength > maxLen)
            {
                fm.Txt_Message.Text = fm.Txt_Message.Text.Substring(fm.Txt_Message.TextLength - maxLen);
                // キャレットを末尾へ
                fm.Txt_Message.SelectionStart = fm.Txt_Message.TextLength;
                fm.Txt_Message.ScrollToCaret();
            }
        }

        /// <summary>
        /// 要求情報の表示フラグ
        /// </summary>
        /// <returns></returns>
        public static bool GetInfo()
        {
            if (fm == null || fm.IsDisposed)
            {
                return false;
            }

            return fm.chk_Info.Checked;
        }

        /// <summary>
        /// ログの折返し状態変更処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Chk_TxtReturn_CheckedChanged(object sender, EventArgs e)
        {
            if (chk_TxtReturn.Checked)
            {
                Txt_Message.WordWrap = true;
            }
            else
            {
                Txt_Message.WordWrap = false;
            }
        }

        /// <summary>
        /// Formのテーブルデータを返却する
        /// </summary>
        /// <returns>応答内容リスト</returns>
        internal static ConcurrentBag<ParamJSON> GetResponsJsons()
        {
            return ParamJSON;
        }

        #endregion

        /// <summary>
        /// フォーム終了処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DmSVForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                WatsonWebserver.WatsonController.Stop();
            }
            catch (Exception ex)
            {
                LogWriter.LogOutputSystemError(ex);
            }
        }

        /// <summary>
        /// テーブルの変更時に応答内容を更新する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            ParamJSON = ParamConv.GetList((DataTable)fm.dataGridView1.DataSource);
        }


        /// <summary>
        /// App.configからキー項目の内容を取得します。
        /// </summary>
        /// <param name="key">キー</param>
        /// <returns></returns>
        private string GetConfValue(string key)
        {
            try
            {
                return ConfigurationManager.AppSettings[key];
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// App.configからキー項目の内容を取得します。
        /// ブランク、Nullの場合、引数の初期値を設定します。
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="defValue">初期値</param>
        /// <returns></returns>
        private string GetConfValue(string key, string defValue)
        {
            string value = GetConfValue(key);
            return string.IsNullOrWhiteSpace(value) ? defValue : value;
        }
    }
}
