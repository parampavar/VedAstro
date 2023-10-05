


//░█▀▀█ ─█▀▀█ ░█▄─░█ ░█▀▀▄ ░█─░█ ▀█▀ 　 ░█─░█ ─█▀▀█ ░█▀▀▄ 　 ░█──░█ ░█▀▀▀█ ░█▄─░█ 　 ▀▀█▀▀ ░█─░█ ░█▀▀▀ 
//░█─▄▄ ░█▄▄█ ░█░█░█ ░█─░█ ░█▀▀█ ░█─ 　 ░█▀▀█ ░█▄▄█ ░█─░█ 　 ░█░█░█ ░█──░█ ░█░█░█ 　 ─░█── ░█▀▀█ ░█▀▀▀ 
//░█▄▄█ ░█─░█ ░█──▀█ ░█▄▄▀ ░█─░█ ▄█▄ 　 ░█─░█ ░█─░█ ░█▄▄▀ 　 ░█▄▀▄█ ░█▄▄▄█ ░█──▀█ 　 ─░█── ░█─░█ ░█▄▄▄ 

//░█▀▀█ ░█▀▀█ ▀█▀ ▀▀█▀▀ ▀█▀ ░█▀▀▀█ ░█─░█ 　 ░█▀▀▀ ░█──░█ ░█▀▀▀ ░█▄─░█ 　 ░█▀▀█ ░█▀▀▀ ░█▀▀▀ ░█▀▀▀█ ░█▀▀█ ░█▀▀▀ 　 ▀▀█▀▀ ░█─░█ ░█▀▀▀ ░█──░█ 
//░█▀▀▄ ░█▄▄▀ ░█─ ─░█── ░█─ ─▀▀▀▄▄ ░█▀▀█ 　 ░█▀▀▀ ─░█░█─ ░█▀▀▀ ░█░█░█ 　 ░█▀▀▄ ░█▀▀▀ ░█▀▀▀ ░█──░█ ░█▄▄▀ ░█▀▀▀ 　 ─░█── ░█▀▀█ ░█▀▀▀ ░█▄▄▄█ 
//░█▄▄█ ░█─░█ ▄█▄ ─░█── ▄█▄ ░█▄▄▄█ ░█─░█ 　 ░█▄▄▄ ──▀▄▀─ ░█▄▄▄ ░█──▀█ 　 ░█▄▄█ ░█▄▄▄ ░█─── ░█▄▄▄█ ░█─░█ ░█▄▄▄ 　 ─░█── ░█─░█ ░█▄▄▄ ──░█── 

//░█─░█ ░█▀▀▀ 　 ░█─▄▀ ░█▄─░█ ░█▀▀▀ ░█──░█ 　 ░█─░█ ░█▀▀▀ 　 ░█▀▀▀ ▀▄░▄▀ ▀█▀ ░█▀▀▀█ ▀▀█▀▀ ░█▀▀▀ ░█▀▀▄ 
//░█▀▀█ ░█▀▀▀ 　 ░█▀▄─ ░█░█░█ ░█▀▀▀ ░█░█░█ 　 ░█▀▀█ ░█▀▀▀ 　 ░█▀▀▀ ─░█── ░█─ ─▀▀▀▄▄ ─░█── ░█▀▀▀ ░█─░█ 
//░█─░█ ░█▄▄▄ 　 ░█─░█ ░█──▀█ ░█▄▄▄ ░█▄▀▄█ 　 ░█─░█ ░█▄▄▄ 　 ░█▄▄▄ ▄▀░▀▄ ▄█▄ ░█▄▄▄█ ─░█── ░█▄▄▄ ░█▄▄▀




using Azure;
using Azure.Communication.Email;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Mime;
using System.Text.Json;
using System.Xml.Linq;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using SuperConvert.Extensions;
using VedAstro.Library;
using Microsoft.Bing.ImageSearch.Models;
using Microsoft.Bing.ImageSearch;
using Person = VedAstro.Library.Person;

using MimeDetective;

namespace API
{
    /// <summary>
    /// A collection of general tools used by API
    /// </summary>
    public static partial class APITools
    {
        //█░█ ▄▀█ █▀█ █▀▄   █▀▄ ▄▀█ ▀█▀ ▄▀█
        //█▀█ █▀█ █▀▄ █▄▀   █▄▀ █▀█ ░█░ █▀█

        //hard coded links to files stored in storage
        //public const string ApiDataStorageContainer = "vedastro-site-data";

        //NAMES OF FILES IN AZURE STORAGE FOR ACCESS
        public const string LiveChartHtml = "LiveChart.html";

        public const string VisitorLogFile = "VisitorLog.xml";
        public const string TaskListFile = "TaskList.xml";
        public const string MessageListFile = "MessageList.xml";
        public const string SavedEventsChartListFile = "SavedChartList.xml";
        public const string SavedMatchReportList = "SavedMatchReportList.xml";
        public const string RecycleBinFile = "RecycleBin.xml";
        public const string UserDataListXml = "UserDataList.xml";

        public const string
            HoroscopeDataListFile =
                "https://vedastro.org/data/HoroscopeDataList.xml"; //todo should be getting beta version

        public static URL Url { get; set; } //instance of beta or stable URLs

        static APITools()
        {
            //make urls used here for beta or stable
            Url = new URL(GetIsBetaRuntime(), false); //obviously no debug mode
        }

        /// <summary>
        /// Default success message sent to caller
        /// - .ToString(SaveOptions.DisableFormatting); to remove make xml indented
        /// </summary>
        public static HttpResponseData PassMessage(HttpRequestData req) => PassMessage("", req);

        /// <summary>
        /// we specify xml catch error at compile time, likely to fail
        /// </summary>
        public static HttpResponseData PassMessage(XElement payload, HttpRequestData req)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/xml"); //todo check if charset is needed

            //wrap data in nice tag
            var finalXml =
                new XElement("Root", new XElement("Status", "Pass"), new XElement("Payload", payload)).ToString(
                    SaveOptions.DisableFormatting); //no XML indent

            //place in response body
            response.WriteString(finalXml);

            return response;
        }

        public static HttpResponseData PassMessage(string payload, HttpRequestData req)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            //response.Headers.Add("Content-Type", "text/xml"); //todo check if charset is needed

            //wrap data in nice tag
            var finalXml =
                new XElement("Root", new XElement("Status", "Pass"), new XElement("Payload", payload)).ToString(
                    SaveOptions.DisableFormatting); //no XML indent

            //place in response body
            response.WriteString(finalXml);

            return response;
        }

        /// <summary>
        /// data comes in as XML should leave as JSON ready for sending to client via HTTP
        /// </summary>
        public static HttpResponseData MessageJson<T>(string statusResult, T payload, HttpRequestData req, string contentType = MediaTypeNames.Application.Json)
        {
            //STAGE 1 : SET HTTP HEADERS
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", contentType);
            response.Headers.Add("Call-Status", statusResult); //lets caller know data is in payload
            response.Headers.Add("Access-Control-Expose-Headers", "Call-Status"); //needed by silly browser to read call-status

            //STAGE 2 : METADATA ABOUT CALL
            var finalPayloadJson = new JObject();
            finalPayloadJson["Status"] = statusResult;
            //add "Note" if specified
            if (!string.IsNullOrEmpty(APITools.ApiExtraNote)) { finalPayloadJson["Note"] = APITools.ApiExtraNote; }

            //STAGE 3 : PAYLOAD
            //if xelement than use xelement converter
            if (payload is List<XElement> payloadXmlList)
            {
                //convert XML to Json text
                var finalPayload = Tools.ListToJson(payloadXmlList);
                finalPayloadJson["Payload"] = finalPayload;
            }
            else if (payload is JProperty payloadJProperty)
            {
                //convert XML to Json text
                var temp = new JProperty("Payload", new JObject(payloadJProperty));
                finalPayloadJson.Add(temp);
            }
            else if (payload is JArray payloadJArray)
            {
                //place directly in
                finalPayloadJson["Payload"] = payloadJArray;
            }
            else if (payload is JToken payloadJToken)
            {
                //place directly in
                finalPayloadJson["Payload"] = payloadJToken;
            }
            else if (payload is JObject payloadJObject)
            {
                //place directly in
                finalPayloadJson["Payload"] = payloadJObject;
            }
            else if (payload is string payloadStr)
            {
                finalPayloadJson["Payload"] = payloadStr;
            }
            //if not special type than assign direct
            else
            {
                //if no payload just status, used for status only messages
                if (payload != null)
                {
                    finalPayloadJson["Payload"] = JToken.Parse(payload.ToString());
                }
            }

            //convert XML to Json text
            string jsonText = finalPayloadJson.ToString();

            //place in response body
            response.WriteString(jsonText);

            return response;
        }

        /// <summary>
        /// if specified will be included in open api return response to caller
        /// used to tell if call has been slowed, or other notifications
        /// </summary>
        public static string? ApiExtraNote { get; set; } = "";

        public static HttpResponseData FailMessageJson(XElement payload, HttpRequestData req) =>
            MessageJson("Fail", payload, req);

        public static HttpResponseData FailMessageJson(string payload, HttpRequestData req) =>
            MessageJson("Fail", payload, req);

        public static HttpResponseData FailMessageJson(Exception payloadException, HttpRequestData req) =>
            MessageJson("Fail", Tools.ExceptionToXML(payloadException), req);

        public static HttpResponseData PassMessageJson(object payload, HttpRequestData req) =>
            MessageJson("Pass", payload, req);

        public static HttpResponseData PassMessageJson(HttpRequestData req) => MessageJson<object>("Pass", null, req);

        public static HttpResponseData FailMessage(object payload, HttpRequestData req)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain");

            //wrap data in nice tag
            var finalXml =
                new XElement("Root", new XElement("Status", "Fail"), new XElement("Payload", payload)).ToString(
                    SaveOptions.DisableFormatting); //no XML indent

            //place in response body
            response.WriteString(finalXml);

            return response;
        }

        public static HttpResponseData FailMessage(Exception payloadException, HttpRequestData req) =>
            FailMessage(Tools.ExceptionToXML(payloadException), req);


        //----------------------------------------FUNCTIONS---------------------------------------------

        public static byte[] ExtractRawImageFromRequest(HttpRequestMessage req)
        {
            var rawStream = req.Content.ReadAsByteArrayAsync().Result;

            return rawStream;
        }

        /// <summary>
        /// Gets public IP address of client sending the http request
        /// </summary>
        public static IPAddress GetCallerIp(this HttpRequestData req)
        {
            var headerDictionary = req.Headers.ToDictionary(x => x.Key, x => x.Value, StringComparer.Ordinal);
            var key = "x-forwarded-for";
            var key2 = "x-azure-clientip";
            IPAddress? ipAddress = null;

            if (headerDictionary.ContainsKey(key) || headerDictionary.ContainsKey(key2))
            {
                var headerValues = headerDictionary[key];
                var ipn = headerValues?.FirstOrDefault()?.Split(new char[] { ',' }).FirstOrDefault()
                    ?.Split(new char[] { ':' }).FirstOrDefault();
                var key1ParseResult = IPAddress.TryParse(ipn, out ipAddress);

                //if key 1 fail , try key 2
                if (!key1ParseResult)
                {
                    headerValues = headerDictionary[key];
                    ipn = headerValues?.FirstOrDefault()?.Split(new char[] { ',' }).FirstOrDefault()
                        ?.Split(new char[] { ':' }).FirstOrDefault();
                    key1ParseResult = IPAddress.TryParse(ipn, out ipAddress);
                }
            }

            return ipAddress ?? IPAddress.None;
        }

        public static IPAddress GetCallerIp(this HttpRequestMessage request)
        {
            IPAddress result = null;
            if (request.Headers.TryGetValues("X-Forwarded-For", out IEnumerable<string> values))
            {
                var ipn = values.FirstOrDefault().Split(new char[] { ',' }).FirstOrDefault().Split(new char[] { ':' })
                    .FirstOrDefault();
                IPAddress.TryParse(ipn, out result);
            }

            return result;
        }

        /// <summary>
        /// Reads data stamped build version, if "beta" is found in that name, return true
        /// </summary>
        public static bool GetIsBetaRuntime() => ThisAssembly.BranchName.Contains("beta");
        public static async Task<JsonElement> ExtractDataFromRequestJsonNET(HttpRequestData request)
        {
            string jsonString = "";

            try
            {
                //get raw string from caller
                jsonString = (await request?.ReadAsStringAsync()) ?? @"{Root:""Empty""}";

                JsonDocument doc = JsonDocument.Parse(jsonString);
                JsonElement root = doc.RootElement;

                return root;
            }
            //todo better logging
            catch (Exception e)
            {
                APILogger.Error(e); //log it
                throw new Exception($"ExtractDataFromRequestJson : FAILED : {jsonString} \n {e.Message}");
            }
        }

        public static async Task<JObject> ExtractDataFromRequestJson(HttpRequestData request)
        {
            try
            {

                //get xml string from caller
                var readAsStringAsync = await request?.ReadAsStringAsync();
                var xmlString = readAsStringAsync ?? (new JObject()).ToString();

                //parse xml string todo needs catch here
                var parsedJson = JObject.Parse(xmlString);

                return parsedJson;
            }
            catch (Exception e)
            {
                APILogger.Error("ERROR NO DATA FROM CALLER"); //log it
                APILogger.Error(e); //log it
                return new JObject(); //null to be detected by caller
            }
        }


        /// <summary>
        /// If there is binary file in request it will take it out,
        /// not support multipart form
        /// </summary>
        public static async Task<byte[]> ExtractFileFromRequest(HttpRequestData request)
        {
            request.Body.Position = 0; //need to set to 0 else will get 0 bytes
            var x = request.Body.ToByteArray();
            return x;
        }


        /// <summary>
        /// Get all charts belonging to owner ID
        /// </summary>

        /// <summary>
        /// Gets user data, if user does
        /// not exist makes a new one & returns that
        /// Note :
        /// - email is used to find user, not hash or id (unique)
        /// - Uses UserDataList.xml
        /// </summary>
        public static async Task<UserData> GetUserData(string id, string name, string email)
        {
            //get user data list file (UserDataList.xml) Azure storage
            var userDataListXml = await Tools.GetXmlFileFromAzureStorage(UserDataListXml, Tools.BlobContainerName);

            //look for user with matching email
            var foundUserXml = userDataListXml.Root?.Elements()
                .Where(userDataXml => userDataXml.Element("Email")?.Value == email)?
                .FirstOrDefault();

            //if user found, initialize xml and send that
            if (foundUserXml != null)
            {
                return UserData.FromXml(foundUserXml);
            }

            //if no user found, make new user and send that
            else
            {
                //create new user from google's data
                var newUser = new UserData(id, name, email);

                //add new user xml to main list
                await Tools.AddXElementToXDocumentAzure(newUser.ToXml(), UserDataListXml, Tools.BlobContainerName);

                //return newly created user to caller
                return newUser;
            }
        }

        /// <summary>
        /// Given a user data it will find mathcing user email and replace the existing UserData with inputed
        /// Note :
        /// - Uses UserDataList.xml
        /// </summary>
        public static async Task<UserData> UpdateUserData(string id, string name, string email)
        {
            //get user data list file (UserDataList.xml) Azure storage
            var userDataListXml = await Tools.GetXmlFileFromAzureStorage(UserDataListXml, Tools.BlobContainerName);

            //look for user with matching email
            var foundUserXml = userDataListXml.Root?.Elements()
                .Where(userDataXml => userDataXml.Element("Email")?.Value == email)?
                .FirstOrDefault();

            //if user found, initialize xml and send that
            if (foundUserXml != null)
            {
                return UserData.FromXml(foundUserXml);
            }

            //if no user found, make new user and send that
            else
            {
                //create new user from google's data
                var newUser = new UserData(id, name, email);

                //add new user xml to main list
                await Tools.AddXElementToXDocumentAzure(newUser.ToXml(), UserDataListXml, Tools.BlobContainerName);

                //return newly created user to caller
                return newUser;
            }
        }

        /// <summary>
        /// Gets any file at given WWW url will return as bytes
        /// </summary>
        public static async Task<byte[]> GetFileHttp(string url)
        {
            try
            {
                //get the data sender
                using var client = new HttpClient();

                client.Timeout = Timeout.InfiniteTimeSpan;

                //load xml event data files before hand to be used quickly later for search
                //get main horoscope prediction file (located in wwwroot)
                var fileBytes = await client.GetByteArrayAsync(url, CancellationToken.None);

                return fileBytes;
            }
            catch (Exception e)
            {
                var msg = $"FAILED TO GET FILE:/n{url}";
                Console.WriteLine(msg);
                APILogger.Error(msg); //log it
                return new byte[] { };
            }


        }

        /// <summary>
        /// Makes a HTTP GET request and return the data as HTTP response message
        /// </summary>
        public static async Task<HttpResponseMessage> GetRequest(string receiverAddress)
        {
            //prepare the data to be sent
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, receiverAddress);

            //get the data sender
            using var client = new HttpClient() { Timeout = new TimeSpan(0, 0, 0, 0, Timeout.Infinite) }; //no timeout

            //tell sender to wait for complete reply before exiting
            var waitForContent = HttpCompletionOption.ResponseContentRead;

            //send the data on its way
            var response = await client.SendAsync(httpRequestMessage, waitForContent);

            //return the raw reply to caller
            return response;
        }

        public static async Task UpdateRecordInDoc<T>(XElement updatedPersonXml, string cloudFileName) where T : IToXml
        {
            var allListXmlDoc = await Tools.GetXmlFileFromAzureStorage(cloudFileName, Tools.BlobContainerName);

            var updatedPerson = Person.FromXml(updatedPersonXml);

            //get the person record that needs to be updated
            var personToUpdate = await Tools.FindPersonXMLById(updatedPerson.Id);

            //delete the previous person record,
            //and insert updated record in the same place
            personToUpdate?.ReplaceWith(updatedPersonXml);

            //upload modified list file to storage
            await SaveXDocumentToAzure(allListXmlDoc, cloudFileName, Tools.BlobContainerName);
        }

        public static async Task<List<Person>> GetAllPersonList()
        {
            //get all person list from storage
            var personListXml = await Tools.GetXmlFileFromAzureStorage(Tools.PersonListFile, Tools.BlobContainerName);
            var allPersonList = personListXml.Root?.Elements();

            var returnList = Person.FromXml(allPersonList);

            return returnList;
        }

        public static HttpResponseData SendHtmlToCaller(string chartContentSvg, HttpRequestData incomingRequest)
        {
            //send image back to caller
            var response = incomingRequest.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/html");
            //place in response body
            response.WriteString(chartContentSvg);
            return response;
        }

        public static HttpResponseData SendTextToCaller(string chartContentSvg, HttpRequestData incomingRequest)
        {
            //send image back to caller
            var response = incomingRequest.CreateResponse(HttpStatusCode.OK);
            //response.Headers.Add("Content-Type", "text/plain");
            //place in response body
            response.WriteString(chartContentSvg);

            return response;
        }

        public static void SendEmail(string fileName, string fileFormat, string receiverEmailAddress,
            Stream rawFileBytes)
        {
            var emailClient = getEmailClient();

            var fileNameFull = $"{fileName}.{fileFormat.ToLower()}";

            var emailTitle = $"Shared {fileFormat.ToUpper()} from VedAstro";

            // Create the email content, visible to user
            var emailContent = new EmailContent(emailTitle)
            {
                PlainText = $"Find attached your {fileName}, from VedAstro.org -> {fileNameFull}",
                Html = "<html><body>Shared file from VedAstro.org</body></html>"
            };

            var emailMessage = new EmailMessage(
                senderAddress: "contact@vedastro.org", // The email address of the domain registered with the Communication Services resource
                recipientAddress: receiverEmailAddress,
                content: emailContent);

            var attachmentName = fileNameFull;
            var contentType =
                Tools.StringToMimeType(fileFormat) ?? MediaTypeNames.Text.Plain; //if fail just plain noodle will do

            var content = BinaryData.FromStream(rawFileBytes);
            var emailAttachment = new EmailAttachment(attachmentName, contentType, content);

            emailMessage.Attachments.Add(emailAttachment);

            try
            {
                EmailSendOperation emailSendOperation = emailClient.Send(WaitUntil.Completed, emailMessage);
                Console.WriteLine($"Email Sent. Status = {emailSendOperation.Value.Status}");

                /// Get the OperationId so that it can be used for tracking the message for troubleshooting
                string operationId = emailSendOperation.Id;
                Console.WriteLine($"Email operation id = {operationId}");
            }
            catch (RequestFailedException ex)
            {
                /// OperationID is contained in the exception message and can be used for troubleshooting purposes
                Console.WriteLine(
                    $"Email send operation failed with error code: {ex.ErrorCode}, message: {ex.Message}");
            }

            //-------------LOCAL FUNCS

            EmailClient getEmailClient()
            {
                //read the connection string
                var connectionString = Secrets.AutoEmailerConnectString;


                //raise alarm if no connection string
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new Exception($"Failed to get connection string : AutoEmailerConnectString!");
                }

                //sign in to email
                return new EmailClient(connectionString);
            }
        }

        //if user made profile while logged out then logs in, transfer the profiles created with visitor id to the new user id
        //if this is not done, then when user loses the visitor ID, they also loose access to the person profile
        public static async Task<bool> SwapUserId(CallerInfo callerInfo, string cloudXmlFile)
        {
            var allListXmlDoc = await Tools.GetXmlFileFromAzureStorage(cloudXmlFile, Tools.BlobContainerName);

            //filter out record by visitor id
            var visitorIdList = Tools.FindXmlByUserId(allListXmlDoc, callerInfo.VisitorId);

            //if user made profile while logged out then logs in, transfer the profiles created with visitor id to the new user id
            //if this is not done, then when user loses the visitor ID, they also loose access to the person profile
            var loggedIn = callerInfo.UserId != "101" && !(string.IsNullOrEmpty(callerInfo.UserId)); //already logged in if true
            var visitorProfileExists = visitorIdList.Any();

            if (loggedIn && visitorProfileExists)
            {
                //convert visitor to user
                foreach (var xmlRecord in visitorIdList)
                {
                    //get existing data
                    var existingOwners = xmlRecord?.Element("UserId")?.Value ?? "";

                    //replace visitor id with user id
                    var updatedOwners = existingOwners.Replace(callerInfo.VisitorId, callerInfo.UserId);

                    //check if data is valid, should not match
                    if (updatedOwners.Equals(existingOwners))
                    {
                        throw new Exception("ID Swap Failed");
                    }

                    //pump value into local updated list
                    var cleaned = Tools.RemoveWhiteSpace(updatedOwners); //remove any space crept it
                    xmlRecord.Element("UserId")!.Value = cleaned;
                }

                //upload modified list file to storage
                await SaveXDocumentToAzure(allListXmlDoc, cloudXmlFile, Tools.BlobContainerName);

                //since heavy computation, log if happens
                #region DEBUG
                Console.WriteLine($"Profiles swapped : {visitorIdList.Count}");
                #endregion


                //clear cache
                await AzureCache.Delete(callerInfo.CallerId);

                //return true of swap was done
                return true;
            }
            //false if no swap
            else
            {
                return false;
            }
        }

        public static IEnumerable<LogItem> GetOnlineVisitors(XDocument visitorLogDocument)
        {

            //parse all logs
            var xmlRecordList = visitorLogDocument.Root?.Elements() ?? new List<XElement>();
            List<LogItem> logItemList = LogItem.FromXml(xmlRecordList);

            //last hour
            var lastHourRecords = from logItem in logItemList
                                  where Tools.IsWithinLastHour(logItem.Time, -24)
                                  select logItem;

            //unique visitors
            List<LogItem> uniqueList = lastHourRecords.DistinctBy(p => p.VisitorId).ToList();

            return uniqueList;
        }

        /// <summary>
        /// Uses name and birth year to generate human readable ID for a new person record
        /// created so that user can type ID direct into URL based on only memory of name and birth year
        /// </summary>
        public static async Task<string> GeneratePersonId(string personName, string birthYear)
        {
            //remove all space from name : Jamés Brown > JamésBrown
            var spaceLessName = Tools.RemoveWhiteSpace(personName);

            //almost done, name with birth year at the back
            var humanId = spaceLessName + birthYear;

            //check if ID is really unique, else it would need a number at the back 
            //try to find a person, if null then no new id is unique
            //jamesbrown and JamesBrown, both should by common sense work
            var idIsSafe = await CheckBothCase(humanId);

            //if id NOT safe, add nonce and try again, possible nonce has been used
            //JamésBrown > JamésBrown1
            var nonceCount = 1; //start nonce at 1
        TryAgain:
            var noncedId = humanId; //clear pre nonce if any 
            if (!idIsSafe)
            {
                //make unique
                noncedId += nonceCount;
                nonceCount++; //increment for next if needed
                              //try again
                idIsSafe = await CheckBothCase(noncedId);
                ; //anybody with same id found?
                goto TryAgain;
            }

            //once control reaches here id should be all good
            return noncedId;


            //---------------LOCAL FUNCTIONS-------------------------------

            //check both case to allow user to make mistake of adding in
            //jamesbrown and JamesBrown, both should by common sense work
            async Task<bool> CheckBothCase(string checkThis)
            {
                var x = (await Tools.FindPersonXMLById(checkThis)) == null;
                var y = (await Tools.FindPersonXMLById(checkThis.ToLower())) == null;
                return x || y;
            }




        }

        /// <summary>
        /// data comes in as XML should leave as JSON ready for sending to client via HTTP
        /// </summary>
        public static JObject AnyTypeToJson<T>(T payload)
        {
            var finalPayloadJson = new JObject();

            //if xelement than use xelement converter
            if (payload is List<XElement> payloadXmlList)
            {
                //convert XML to Json text
                var finalPayload = Tools.ListToJson(payloadXmlList);
                finalPayloadJson["Payload"] = finalPayload;
            }
            else if (payload is JProperty payloadJToken)
            {
                //convert XML to Json text
                //finalPayloadJson["Payload"] = JToken.FromObject(payloadJToken);
                var temp = new JProperty("Payload", new JObject(payloadJToken));
                finalPayloadJson.Add(temp);
            }
            else if (payload is string payloadStr)
            {
                finalPayloadJson["Payload"] = payloadStr;
            }
            //if not special type than assign direct
            else
            {
                finalPayloadJson["Payload"] = JToken.Parse(payload.ToString());
            }

            //convert XML to Json text
            // string jsonText = finalPayloadJson.ToString(); //todo can be direct aslo

            return finalPayloadJson;
        }

        public static string GetHeaderValue(HttpResponseData request, string headerName)
        {
            IEnumerable<string> list;
            return request.Headers.TryGetValues(headerName, out list) ? list.FirstOrDefault() : null;
        }

        public static object GetHeaderValue(HttpResponseMessage request, string headerName)
        {
            IEnumerable<string> list;
            return request.Headers.TryGetValues(headerName, out list) ? list.FirstOrDefault() : null;
        }

        public static HttpResponseData SendSvgToCaller(string chartContentSvg, HttpRequestData incomingRequest)
        {
            //send image back to caller
            var response = incomingRequest.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "image/svg+xml");

            //place in response body
            response.WriteString(chartContentSvg);
            return response;
        }

        public static HttpResponseData SendFileToCaller(byte[] gif, HttpRequestData incomingRequest, string mimeType)
        {
            //send image back to caller
            var response = incomingRequest.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", mimeType);
            //place in response body
            response.WriteBytes(gif);
            return response;
        }

        /// <summary>
        /// SPECIAL METHOD made to allow files straight from blob to be sent to caller
        /// as fast as possible
        /// </summary>
        public static HttpResponseData SendFileToCaller(BlobClient fileBlobClient, HttpRequestData incomingRequest, string mimeType)
        {
            //send image back to caller
            var response = incomingRequest.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", mimeType);

            //place in response body
            //NOTE: very important to pass as stream to make work
            //      if convert to byte array will not work!
            //      needs to be direct stream to response
            response.Body = fileBlobClient.OpenRead();
            return response;
        }

        public static HttpResponseData SendPassHeaderToCaller(BlobClient fileBlobClient, HttpRequestData req, string mimeType)
        {
            //send image back to caller
            //response = incomingRequest.CreateResponse(HttpStatusCode.OK);
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Call-Status", "Pass"); //lets caller know data is in payload
            response.Headers.Add("Access-Control-Expose-Headers", "Call-Status"); //needed by silly browser to read call-status
            response.Headers.Add("Content-Type", mimeType);

            //place in response body
            //NOTE: very important to pass as stream to make work
            //      if convert to byte array will not work!
            //      needs to be direct stream to response
            response.Body = fileBlobClient.OpenRead();
            return response;
        }

        public static async Task<HttpResponseData> SendPassHeaderToCaller(string dataToSend, HttpRequestData req, string mimeType)
        {
            //send image back to caller
            //response = incomingRequest.CreateResponse(HttpStatusCode.OK);
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Call-Status", "Pass"); //lets caller know data is in payload
            response.Headers.Add("Access-Control-Expose-Headers", "Call-Status"); //needed by silly browser to read call-status
            response.Headers.Add("Content-Type", mimeType);

            //place in response body
            //NOTE: very important to pass as stream to make work
            //      if convert to byte array will not work!
            //      needs to be direct stream to response
            await response.WriteStringAsync(dataToSend);

            return response;
        }

        public static string GetCallerId(string userId, string visitorId)
        {
            var IsLoggedIn = userId != "101";
            if (IsLoggedIn)
            {
                return userId;
            }
            //if user NOT logged in then take his visitor ID as caller id
            else
            {
                return visitorId;
            }

        }




        //--------------------TODO NEEDS MOVING



        /// <summary>
        /// Uses cache if available else calculates the data
        /// also auto adds the newly calculated data cache for future
        /// </summary>
        public static async Task<T> CacheExecuteTask<T>(Func<Task<T>> generateChart, string callerId, string mimeType = "")
        {
            //check if cache exist
            var isExist = await AzureCache.IsExist(callerId);

            T chart;

            if (!isExist)
            {
                //squeeze the Sky Juice!
                chart = await generateChart.Invoke();
                //save for future
                var blobClient = await AzureCache.Add<T>(callerId, chart, mimeType);
            }
            else
            {
                chart = await AzureCache.GetData<T>(callerId);
            }

            return chart;
        }



        /// <summary>
        /// Given a cache generator function and a name for the data
        /// it calculate and save data to cache Data Blob storage
        /// </summary>
        public static async Task<BlobClient> ExecuteAndSaveToCache(Func<Task<string>> cacheGenerator, string cacheName, string mimeType = "")
        {

#if DEBUG
            Console.WriteLine($"NO CACHE! RUNNING COMPUTE : {cacheName}");
#endif

            BlobClient? chartBlobClient;

            try
            {
                //lets everybody know call is running
                CallTracker.CallStart(cacheName);

                //squeeze the Sky Juice!
                var chartBytes = await cacheGenerator.Invoke();

                //save for future
                chartBlobClient = await AzureCache.Add(cacheName, chartBytes, mimeType);
                //chartBlobClient = await AzureCache.Add<byte[]>(callerId, chartBytes, mimeType);

                //chartBlobClient = await AzureCache.GetData<BlobClient>(callerId);
            }
            //always mark the call as ended
            finally
            {
                CallTracker.CallEnd(cacheName); //mark the call as ended
            }


            return chartBlobClient;
        }

        public static string JsonToCsv(string jsonData)
        {

            string csvPath = jsonData.ToCsv();


            Console.WriteLine(csvPath);

            return csvPath;
        }


        /// <summary>
        /// Searches for person's image on BING and return one most probable as result
        /// note uses thumbnail version for speed and data save
        /// </summary>
        public static async Task<byte[]> GetSearchImage(VedAstro.Library.Person personToImage)
        {

            //IMPORTANT: replace this variable with your Cognitive Services subscription key
            string subscriptionKey = Secrets.BING_IMAGE_SEARCH;
            //stores the image results returned by Bing
            Images imageResults = null;

            var client = new ImageSearchClient(new ApiKeyServiceClientCredentials(subscriptionKey));

            //make search query based on person's details
            var keywords = personToImage.DisplayName; //todo maybe location can help

            // make the search request to the Bing Image API, and get the results
            imageResults = await client.Images.SearchAsync(query: keywords); //search query


            //pick out the images that seems most suited
            var handPickedApples = imageResults.Value.Where(delegate (ImageObject x)
            {
                var isJpeg = x.EncodingFormat == "jpeg";//get only jpeg images for ease of handling down the road
                var isCorrectShape = x.Width < x.Height; //rectangle image to fit site style better
                return isJpeg && isCorrectShape;
            });


            //get 1st image in list as data
            var topImageUrl = handPickedApples.First().ThumbnailUrl;
            var imageBytes = await APITools.GetFileHttp(topImageUrl);

            //return to caller
            return imageBytes;
        }


        /// <summary>
        /// based on caller's ip address, set limit
        /// </summary>
        /// <returns></returns>
        public static async Task AutoControlOpenAPIOverload(OpenAPILogBookEntity callData)
        {
            var minute1 = 1;
            var ipAddress = callData.PartitionKey;
            var lastCallsCount = APILogger.GetAllCallsWithinLastTimeperiod(ipAddress, minute1);

            //rate set in runtime settings is multiplied
            var msDelayRate = int.Parse(Secrets.OpenAPICallDelayMs);
            var freeCallRate = 100;//allowed high speed calls per minute //int.Parse(Secrets.OpenAPICallDelayMs); TODO add to Secrets


            //if delay applied then let caller know
            //NOTE : other words allowed 1 call every 30 seconds
            var userCallRate = lastCallsCount / minute1; //calls per minute
            if (userCallRate > freeCallRate)
            {
                //every additional call within specified time limit gets slowed accordingly
                //exp: last 3 calls x 800ms = 4th call delay --> 2400ms
                var msDelay = lastCallsCount * msDelayRate;

                //todo shorten link
                APITools.ApiExtraNote = $"Donate To Increase Speed : " +
                                        $"{URL.Donate}";

                //APITools.ApiExtraNote = $"Call Slowed Down {msDelay}ms:" +
                //						$"\nSorry for now public API quota is {freeCallRate} calls/min." +
                //						$"\nYou made {lastCallsCount} calls in 1 min." +
                //						$"\nPlease buy unlimited for only $1 :" +
                //						$"https://ko-fi.com/summary/f17451bf-7509-4e59-8471-2f7ce446c9ae";


                await Task.Delay(msDelay);
#if DEBUG
                Console.WriteLine($"AUTO Throttle : IP -> {ipAddress} Delay ->{msDelay}ms");
#endif

            }
            else
            {
                //if below limit than let call run, clear message
                APITools.ApiExtraNote = "";
            }

        }

        public static TableClient GetTableClientFromTableName(string tableName)
        {
            //prepare call stuff
            var tableUlr = $"https://vedastroapistorage.table.core.windows.net/{tableName}";
            string accountName = "vedastroapistorage";
            string storageAccountKey = Secrets.VedAstroApiStorageKey;

            //get connection
            var _tableServiceClient = new TableServiceClient(new Uri(tableUlr), new TableSharedKeyCredential(accountName, storageAccountKey));
            var client = _tableServiceClient.GetTableClient(tableName);

            return client;
        }


        /// <summary>
        /// Given a file or string convertible data, send it to caller accordingly
        /// </summary>
        public static HttpResponseData SendAnyToCaller(string calculatorName, object rawPlanetData, HttpRequestData incomingRequest)
        {

            //then it is a file
            if (rawPlanetData is byte[] rawFileData)
            {
                //get correct mime type so browser or receiver knows how to present
                var mimeType = GetMimeType(rawFileData);

                return APITools.SendFileToCaller(rawFileData, incomingRequest, mimeType);
            }

            //probably data that can be sent as JSON text
            else
            {
                //4 : CONVERT TO JSON
                var payloadJson = Tools.AnyToJSON(calculatorName, rawPlanetData); //use calculator name as key

                //5 : SEND DATA
                return APITools.PassMessageJson(payloadJson, incomingRequest);
            }

        }

        public static string GetMimeType(byte[] fileBytes)
        {
            var inspector = new ContentInspectorBuilder()
            {
                Definitions = MimeDetective.Definitions.Default.All()
            }.Build();

            var fileType = inspector.Inspect(fileBytes);

            var resultsByMimeType = fileType.ByMimeType();

            // Return the MIME type
            var mimeType = resultsByMimeType[0].MimeType;
            return mimeType ?? "application/octet-stream";
        }
    }

}