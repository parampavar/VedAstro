﻿using VedAstro.Library;
using System.Xml.Linq;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Newtonsoft.Json.Linq;

//A LIE IS JUST A GREAT STORY THAT SOMEONE RUINED WITH THE TRUTH -- Barnabus Stinson

namespace Website
{

    /// <summary>
    /// Encapsulates all thing to do with server (API)
    /// </summary>
    public static class ServerManager
    {

        //PUBLIC METHODS

        /// <summary>
        /// Calls a URL and returns the content of the result as XML
        /// Even if content is returned as JSON, it is converted to XML
        /// Note: if JSON auto adds "Root" as first element, unless specified
        /// for XML data root element name is ignored
        /// </summary>
        public static async Task<WebResult<XElement>> ReadFromServerXmlReply(string apiUrl, IJSRuntime? jsRuntime)
        {
            //if js runtime available & browser offline show error
            jsRuntime?.CheckInternet();

            var parsed = await Tools.ReadFromServerXmlReply(apiUrl);

            return parsed;

        }

        /// <summary>
        /// Send xml as string to server and returns stream as response
        /// </summary>
        public static async Task<Stream> WriteToServerStreamReply(string apiUrl, XElement xmlData, IJSRuntime? jsRuntime)
        {

            //if js runtime available & browser offline show error
            jsRuntime?.CheckInternet();

            try
            {
                //prepare the data to be sent
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, apiUrl);

                httpRequestMessage.Content = Tools.XmLtoHttpContent(xmlData);

                //tell sender to wait for complete reply before exiting
                var waitForContent = HttpCompletionOption.ResponseContentRead;

                //send the data on its way
                var response = await AppData.HttpClient.SendAsync(httpRequestMessage, waitForContent);

                //extract the content of the reply data
                var rawMessage = response.Content.ReadAsStreamAsync().Result;

                return rawMessage;

            }
            //rethrow specialized exception to be handled by caller
            catch (Exception e) { throw new ApiCommunicationFailed($"WriteToServerStreamReply()", e); }

        }

        /// <summary>
        /// Send xml as string to server and returns xml as response
        /// Note:
        /// - on failure payload will contain error info
        /// - xml is not checked here, just converted
        /// - No timeout! Will wait forever
        /// - failure is logged here
        /// </summary>
        public static async Task<WebResult<XElement>> WriteToServerXmlReplyDotNet(string apiUrl, XElement xmlData, IJSRuntime? jsRuntime)
        {

            WebResult<XElement> returnVal;

            //if js runtime available & browser offline show error
            jsRuntime?.CheckInternet();

            string rawMessage = "";
            var statusCode = "";

            try
            {
                //prepare the data to be sent
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                httpRequestMessage.Content = Tools.XmLtoHttpContent(xmlData);

                //tell sender to wait for complete reply before exiting
                var waitForContent = HttpCompletionOption.ResponseContentRead;

                //send the data on its way 
                var response = await AppData.HttpClient.SendAsync(httpRequestMessage, waitForContent);

                //keep for error logging if needed
                statusCode = response?.StatusCode.ToString();

                //extract the content of the reply data
                rawMessage = await response?.Content.ReadAsStringAsync() ?? "";

                //problems might occur when parsing
                //try to parse as XML
                var writeToServerXmlReply = XElement.Parse(rawMessage);
                returnVal = WebResult<XElement>.FromXml(writeToServerXmlReply);

            }
            //if internet failure let caller know immediately
            catch (HttpRequestException e)
            {
#if DEBUG
                Console.WriteLine(e.Message);
#endif
                throw new NoInternetError();
            }

            //note: other failure here could be for several very likely reasons,
            //so it is important to properly check and handled here for best UX
            //- server unexpected failure
            //- server unreachable
            catch (Exception)
            {
                returnVal = new WebResult<XElement>(false, new XElement("Root", $"Error from WriteToServerXmlReply()\n{statusCode}\n{rawMessage}"));
            }

            //if fail log it
            if (!returnVal.IsPass) { await WebLogger.Error(returnVal.Payload); }

            return returnVal;
        }

        /// <summary>
        /// HTTP Post via JS interop
        /// </summary>
        public static async Task<WebResult<XElement>> WriteToServerXmlReply(string apiUrl, XElement xmlData, int timeout = 60)
        {


        TryAgain:

            //ACT 1:
            //send data to URL, using JS for reliability & speed
            //also if call does not respond in time, we replay the call over & over
            string receivedData;
            try { receivedData = await Tools.TaskWithTimeoutAndException(Post(apiUrl, xmlData.ToString(SaveOptions.DisableFormatting)), TimeSpan.FromSeconds(timeout)); }

            //if fail replay and log it
            catch (Exception e)
            {
                var debugInfo = $"Call to \"{apiUrl}\" timeout at : {timeout}s";

                WebLogger.Data(debugInfo);
#if DEBUG
                Console.WriteLine(debugInfo);
#endif
                goto TryAgain;
            }

            //ACT 2:
            //check raw data 
            if (string.IsNullOrEmpty(receivedData))
            {
                //log it
                await WebLogger.Error($"BLZ > Call returned empty\n To:{apiUrl} with payload:\n{xmlData}");

                //send failed empty data to caller, it should know what to do with it
                return new WebResult<XElement>(false, new XElement("CallEmptyError"));
            }

            //ACT 3:
            //return data as XML
            //problems might occur when parsing
            //try to parse as XML
            var writeToServerXmlReply = XElement.Parse(receivedData);
            var returnVal = WebResult<XElement>.FromXml(writeToServerXmlReply);

            //ACT 4:
            return returnVal;
        }


        public static async Task<string> Post(string apiUrl, string stringData)
        {
            //this call will take you to NetworkThread.js
            var rawPayload = await AppData.JsRuntime.InvokeAsync<string>(JS.postWrapper, apiUrl, stringData);

            //todo proper checking of status needed
            return rawPayload;
        }

        //public static async Task<WebResult<JToken>> WriteToServerJsonReply(string apiUrl, JObject xmlData, int timeout = 60)
        //{
        //    //prepare the data to be sent
        //    var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, apiUrl);
        //    httpRequestMessage.Content = Tools.XmLtoHttpContent(xmlData);

        //    //tell sender to wait for complete reply before exiting
        //    var waitForContent = HttpCompletionOption.ResponseContentRead;

        //    //send the data on its way 
        //    var response = await AppData.HttpClient.SendAsync(httpRequestMessage, waitForContent);

        //    //keep for error logging if needed
        //    statusCode = response?.StatusCode.ToString();

        //    //extract the content of the reply data
        //    rawMessage = await response?.Content.ReadAsStringAsync() ?? "";

        //    //problems might occur when parsing
        //    //try to parse as XML
        //    var writeToServerXmlReply = XElement.Parse(rawMessage);
        //    returnVal = WebResult<XElement>.FromXml(writeToServerXmlReply);


        //    //ACT 3:
        //    //return data as XML
        //    //problems might occur when parsing
        //    //try to parse as XML
        //    var writeToServerXmlReply = JObject.Parse(receivedData);
        //    var returnVal = WebResult<JObject>.FromJson(writeToServerXmlReply);

        //    //ACT 4:
        //    return returnVal;
        //}


        //PRIVATE METHODS



    }
}
