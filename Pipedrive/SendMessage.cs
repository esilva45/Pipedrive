using Newtonsoft.Json;
using Npgsql;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace ServiceIntegration {
    class SendMessage {
        public static string urlligacao = null;
        public static string urlservice = null;
        public static string token = null;
        public static string domain = null;
        public static string company_id = null;
        public static string extension_start = null;
        public static string extension_end = null;
        public static string extensions = null;

        public static void Message() {
            NpgsqlConnection conn = Connection.GetConnection();
            XElement configXml = XElement.Load(AppDomain.CurrentDomain.BaseDirectory + @"\config.xml");
            urlligacao = configXml.Element("UrlLigacao").Value.ToString();
            urlservice = configXml.Element("UrlService").Value.ToString();
            token = configXml.Element("Token").Value.ToString();
            domain = configXml.Element("Domain").Value.ToString();
            company_id = configXml.Element("CompanyId").Value.ToString();
            extension_start = configXml.Element("ExtensionStart").Value.ToString();
            extension_end = configXml.Element("ExtensionEnd").Value.ToString();
            extensions = configXml.Element("Extensions").Value.ToString();

            Regex r = new Regex(@"\[[^\}]+?\]");

            CallModel call = new CallModel();
            string call_id = "";
            string result = "";
            string callfile = "";
            int range_start = 0;
            int range_end = 0;
            int extension = 0;
            int restriction = 0;
            bool a = false;
            string[] extensionsList = null;
            string ramal = "";

            try {
                if (!extension_start.Equals("0")) {
                    restriction = 1;

                    try {
                        range_start = Int32.Parse(configXml.Element("ExtensionStart").Value.ToString());
                    }
                    catch (Exception) {
                        range_start = 0;
                    }

                    try {
                        range_end = Int32.Parse(configXml.Element("ExtensionEnd").Value.ToString());

                        if (range_end == 0) {
                            range_end = 999999999;
                        }
                    }
                    catch (Exception) {
                        range_end = 999999999;
                    }
                } else if (!extensions.Equals("0")) {
                    restriction = 2;
                    extensionsList = extensions.Split(',');
                }

                string query = "select myphone_callhistory_v14.call_id, dnowner, party_name, " +
                    "CASE  " +
                    "WHEN calltype = 2 THEN 'Inbound'  " +
                    "WHEN calltype = 3 THEN 'Outbound'  " +
                    "ELSE 'Invalid' END as calltype, " +
                    "party_callerid, to_char(start_time  - interval '3 hours', 'YYYY-MM-DD HH24:MI:SS') as start_time, " +
                    "to_char(end_time  - interval '3 hours', 'YYYY-MM-DD HH24:MI:SS') as end_time, " +
                    "(date_part('second', end_time - start_time) + (date_part('minute', end_time - start_time) * 60)) as tempo, " +
                    "to_char(established_time, 'YYYYMMDDHH24MISS') as established " +
                    "from myphone_callhistory_v14 " +
                    "inner join crm_integration on myphone_callhistory_v14.call_id = crm_integration.call_id " +
                    "where crm_integration.processed = false";
               
                NpgsqlCommand command = new NpgsqlCommand(query, conn);
                NpgsqlDataReader rd = command.ExecuteReader();

                while (rd.Read()) {
                    if (restriction.Equals(0)) {
                        Util.Log("Filtro: " + restriction);
                        result = "";
                        callfile = Util.FindRecording(rd["call_id"].ToString());

                        Match m = r.Match(callfile);

                        call.UrlLigacao = urlligacao + callfile;
                        call.CallName = m.ToString().Replace("[", "").Replace("]", "");
                        call.TipoChamada = rd["calltype"].ToString();
                        call.Domain = domain;
                        call.CompanyId = company_id;
                        call.OrigemTel = rd["dnowner"].ToString();
                        call.DestinoTel = rd["party_callerid"].ToString();
                        call.DtInicioChamada = rd["start_time"].ToString();
                        call.DtFimChamada = rd["end_time"].ToString();
                        call.TempoConversacao = rd["tempo"].ToString();
                        result = Send(JsonConvert.SerializeObject(call), rd["call_id"].ToString());

                        if (result.Equals("OK")) {
                            call_id += rd["call_id"] + " ";
                        }
                    } else if (restriction.Equals(1)) {
                        try {
                            extension = Int32.Parse(rd["dnowner"].ToString());
                        }
                        catch (Exception) {
                            extension = 0;
                        }

                        Util.Log("Filtro: " + restriction + " Ramal: [" + extension + "] Inicio/fim: " + range_start + " " + range_end);

                        if ((extension >= range_start) && (extension <= range_end)) {
                            result = "";
                            callfile = Util.FindRecording(rd["call_id"].ToString());

                            Match m = r.Match(callfile);

                            call.UrlLigacao = urlligacao + callfile;
                            call.CallName = m.ToString().Replace("[", "").Replace("]", "");
                            call.TipoChamada = rd["calltype"].ToString();
                            call.Domain = domain;
                            call.CompanyId = company_id;
                            call.OrigemTel = rd["dnowner"].ToString();
                            call.DestinoTel = rd["party_callerid"].ToString();
                            call.DtInicioChamada = rd["start_time"].ToString();
                            call.DtFimChamada = rd["end_time"].ToString();
                            call.TempoConversacao = rd["tempo"].ToString();
                            result = Send(JsonConvert.SerializeObject(call), rd["call_id"].ToString());

                            if (result.Equals("OK")) {
                                call_id += rd["call_id"] + " ";
                            }
                        } else {
                            call_id += rd["call_id"] + " ";
                        }
                    } else if (restriction.Equals(2)) {
                        ramal = rd["dnowner"].ToString();
                        a = Array.Exists(extensionsList, element => element == ramal);

                        Util.Log("Filtro: " + restriction + " Ramal: [" + ramal + "] Teste: " + a);

                        if (a) {
                            result = "";
                            callfile = Util.FindRecording(rd["call_id"].ToString());

                            Match m = r.Match(callfile);

                            call.UrlLigacao = urlligacao + callfile;
                            call.CallName = m.ToString().Replace("[", "").Replace("]", "");
                            call.TipoChamada = rd["calltype"].ToString();
                            call.Domain = domain;
                            call.CompanyId = company_id;
                            call.OrigemTel = rd["dnowner"].ToString();
                            call.DestinoTel = rd["party_callerid"].ToString();
                            call.DtInicioChamada = rd["start_time"].ToString();
                            call.DtFimChamada = rd["end_time"].ToString();
                            call.TempoConversacao = rd["tempo"].ToString();
                            result = Send(JsonConvert.SerializeObject(call), rd["call_id"].ToString());

                            if (result.Equals("OK")) {
                                call_id += rd["call_id"] + " ";
                            }
                        } else {
                            call_id += rd["call_id"] + " ";
                        }
                    }
                }

                rd.Close();

                if (call_id != "") {
                    call_id = call_id.Trim().Replace(" ", ",");
                    NpgsqlCommand cmd = new NpgsqlCommand("update crm_integration set processed = true where call_id in (" + call_id + ")", conn);
                    cmd.ExecuteReader();
                }
            }
            catch (Exception ex) {
                Util.Log(ex.ToString());
            }
            finally {
                if (conn != null) {
                    conn.Close();
                }
            }
        }

        private static string Send(string json, string call_id) {
            string code = "ERROR";

            try {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(urlservice);
                httpWebRequest.ContentType = "application/json";

                if (token != null || token != "") {
                    httpWebRequest.Headers["Pipedrive"] = token;
                }

                httpWebRequest.Method = "POST";

                byte[] data = Encoding.UTF8.GetBytes(json);
                httpWebRequest.ContentLength = data.Length;

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream())) {
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                HttpStatusCode respStatusCode = httpResponse.StatusCode;

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream())) {
                    HttpStatusCode statusCode = ((HttpWebResponse)httpResponse).StatusCode;
                    //var result = streamReader.ReadToEnd();
                    code = statusCode.ToString();
                    Util.Log("ID: " + call_id + " code: " + code);
                    Util.Log(json);
                }
            }
            catch (Exception ex) {
                Util.Log(ex.ToString());
            }

            return code;
        }
    }
}
