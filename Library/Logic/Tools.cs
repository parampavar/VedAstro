using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using OfficeOpenXml;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SwissEphNet;
using Formatting = Newtonsoft.Json.Formatting;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Svg;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using static Azure.Core.HttpHeader;
using Newtonsoft.Json.Linq;
using HtmlAgilityPack;
using System.Collections.Generic;
using static System.Runtime.InteropServices.JavaScript.JSType;



//    𝕀❜𝕞 𝕟𝕠𝕥 𝕒𝕟 𝕚𝕟𝕔𝕙 𝕥𝕠𝕠 𝕗𝕒𝕣 𝕠𝕣 𝕒 𝕤𝕖𝕔𝕠𝕟𝕕 𝕥𝕠𝕠 𝕝𝕒𝕥𝕖,
//    𝕀❜𝕞 𝕖𝕩𝕒𝕔𝕥𝕝𝕪 𝕨𝕙𝕖𝕣𝕖 𝕀❜𝕞 𝕤𝕦𝕡𝕡𝕠𝕤𝕖𝕕 𝕥𝕠 𝕓𝕖 𝕒𝕝𝕨𝕒𝕪𝕤.
//    ℍ𝕖𝕣𝕖 𝕒𝕟𝕕 ℕ𝕠𝕨.

namespace VedAstro.Library
{
    /// <summary>
    /// A collection of general functions that don't have a home yet, so they live here for now.
    /// You're allowed to move them somewhere you see fit, not copy, move!! remember dear :-)
    /// </summary>
    public static class Tools
    {
        /// <summary>
        /// public static string ToJson(this MethodInfo methodInfo)
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        public static JObject ToJson(this MethodInfo methodInfo)
        {
            var jObject = new JObject
            {
                ["Name"] = methodInfo.Name,
                ["DeclaringType"] = methodInfo.DeclaringType?.FullName,
                ["Parameters"] = new JArray(methodInfo.GetParameters().Select(p => new JObject
                {
                    ["Name"] = p.Name,
                    ["ParameterType"] = p.ParameterType.FullName
                }))
            };
            return jObject;
        }

        // public static MethodInfo FromJson(this MethodInfo methodInfo, JObject jsonInput)
        public static MethodInfo MethodInfoFromJson(JObject jObject)
        {
            var methodName = (string)jObject["Name"];
            var declaringType = Type.GetType((string)jObject["DeclaringType"]);
            var parameters = ((JArray)jObject["Parameters"]).Select(jt => Type.GetType((string)jt["ParameterType"])).ToArray();
            var methodInfo = declaringType?.GetMethod(methodName, parameters);
            return methodInfo;
        }
        



        /// <summary>
        /// Converts a list of TableRow aka a full ML Data Table into CSV string 
        /// </summary>
        public static string ListToCSV(List<MLTableRow> tableRowList)
        {
            // If the list is null or empty, return an empty string.
            if (tableRowList == null || !tableRowList.Any()) { return string.Empty; }

            // Initialize a StringBuilder to build the CSV string.
            var csv = new StringBuilder();

            // Get the column names from the first row in the list.
            // The column names are the MLTableName properties of the DataColumns.
            var columnNames = tableRowList[0].DataColumns.Select(result => result.MLTableName("NOTHH!"));

            // Add the column headers to the CSV string.
            // The headers are the column names joined by commas, with "Time" as the first column.
            csv.AppendLine($"Time,{string.Join(",", columnNames)}");

            // Iterate over each row in the list.
            foreach (var row in tableRowList)
            {
                // Quote the Time value and add it as the first column of the row.
                var timeColumnData = Tools.QuoteValue(row.Time);
                // Initialize a list for the row values and add the time column data.
                var rowValues = new List<string> { timeColumnData };
                // Quote the values of the other columns and add them to the row.
                rowValues.AddRange(row.DataColumns.Select(d => Tools.QuoteValue(d.ResultAsString())));
                // Join the row values by commas to form a CSV row, and add it to the CSV string.
                csv.AppendLine(string.Join(",", rowValues));
            }
            // Convert the StringBuilder to a string and return it.
            return csv.ToString();
        }

        public static byte[] ListToExcel(List<MLTableRow> tableRowList)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Sheet1");
                // If the list is null or empty, return an empty Excel file.
                if (tableRowList == null || !tableRowList.Any())
                {
                    return package.GetAsByteArray();
                }
                // Get the column names from the first row in the list.
                var columnNames = tableRowList[0].DataColumns.Select(result => result.MLTableName("NOT YET"));
                // Add the column headers to the Excel file.
                var headerRow = new List<string> { "Time" };
                headerRow.AddRange(columnNames);
                worksheet.Cells["A1"].LoadFromArrays(new List<object[]> { headerRow.Cast<object>().ToArray() });
                // Iterate over each row in the list.
                for (int i = 0; i < tableRowList.Count; i++)
                {
                    var row = tableRowList[i];
                    var excelRow = new List<string> { row.Time.ToString() };
                    excelRow.AddRange(row.DataColumns.Select(d => d.ResultAsString()));
                    worksheet.Cells[i + 2, 1].LoadFromArrays(new List<object[]> { excelRow.Cast<object>().ToArray() });
                }
                return package.GetAsByteArray();
            }
        }

        /// <summary>
        /// Given an excel file will extract out 1 column that contains parseable time
        /// </summary>
        public static async Task<List<GeoLocation>> ExtractLocationColumnFromExcel(Stream excelBinary)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Set the license context for EPPlus
            var timeList = new List<GeoLocation>();
            int timeColumnIndex = -1;

            excelBinary.Position = 0; //important to reset else, no file error
            var excelFileStream = new MemoryStream();
            await excelBinary.CopyToAsync(excelFileStream);
            excelFileStream.Position = 0; // Reset the position of the MemoryStream to the beginning

            using (var package = new ExcelPackage(excelFileStream))
            {
                var worksheet = package.Workbook.Worksheets[0]; // Get the first worksheet
                                                                // Start from the second row (index 2) to skip the header
                for (int rowIndex = 2; rowIndex <= worksheet.Dimension.Rows; rowIndex++)
                {
                    // If we haven't found the time column yet, search for it
                    if (timeColumnIndex == -1)
                    {
                        for (int colIndex = 1; colIndex <= worksheet.Dimension.Columns; colIndex++)
                        {
                            var cellValue = worksheet.Cells[rowIndex, colIndex].Value?.ToString();
                            // If the cell value can be parsed as a Time, this is the time column
                            var tryParse = await GeoLocation.TryParse(cellValue);
                            if (tryParse.Item1) //is parsed, we no need the parsed val
                            {
                                timeColumnIndex = colIndex;
                                break;
                            }
                        }
                    }
                    // If we've found the time column, add the cell value to the list
                    if (timeColumnIndex != -1)
                    {
                        var cellValue = worksheet.Cells[rowIndex, timeColumnIndex].Value?.ToString();
                        var tryParse = await GeoLocation.TryParse(cellValue);
                        if (tryParse.Item1)
                        {
                            //add in the final parsed location into return list
                            timeList.Add(tryParse.Item2);
                        }
                    }
                }
            }

#if DEBUG
            Console.WriteLine($"File Size : {excelBinary.Length}");
            Console.WriteLine($"Rows Found : {timeList.Count}");
#endif

            return timeList;
        }


        /// <summary>
        /// Given an excel file will extract out 1 column that contains parseable time
        /// </summary>
        public static async Task<List<DateTimeOffset>> ExtractTimeColumnFromExcel(Stream excelBinary)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Set the license context for EPPlus
            var timeList = new List<DateTimeOffset>();
            int timeColumnIndex = -1;

            var excelFileStream = new MemoryStream();
            await excelBinary.CopyToAsync(excelFileStream);
            excelFileStream.Position = 0; // Reset the position of the MemoryStream to the beginning

            using (var package = new ExcelPackage(excelFileStream))
            {
                var worksheet = package.Workbook.Worksheets[0]; // Get the first worksheet
                                                                // Start from the second row (index 2) to skip the header
                for (int rowIndex = 2; rowIndex <= worksheet.Dimension.Rows; rowIndex++)
                {
                    // If we haven't found the time column yet, search for it
                    if (timeColumnIndex == -1)
                    {
                        for (int colIndex = 1; colIndex <= worksheet.Dimension.Columns; colIndex++)
                        {
                            var cellValue = worksheet.Cells[rowIndex, colIndex].Value?.ToString();
                            // If the cell value can be parsed as a Time, this is the time column
                            if (Time.TryParseStd(cellValue, out _))
                            {
                                timeColumnIndex = colIndex;
                                break;
                            }
                        }
                    }
                    // If we've found the time column, add the cell value to the list
                    if (timeColumnIndex != -1)
                    {
                        var cellValue = worksheet.Cells[rowIndex, timeColumnIndex].Value?.ToString();
                        if (Time.TryParseStd(cellValue, out var time))
                        {
                            timeList.Add(time);
                        }
                    }
                }
            }

#if DEBUG
            Console.WriteLine($"File Size : {excelBinary.Length}");
            Console.WriteLine($"Rows Found : {timeList.Count}");
#endif

            return timeList;
        }

        /// <summary>
        /// Given a JSON version of a Table will convert to HTML table in string
        /// </summary>
        public static string ConvertJsonToHtmlTable(JToken jObject)
        {
            var data = jObject.ToObject<List<Dictionary<string, string>>>();
            var sb = new StringBuilder();
            sb.Append("<table>");
            // Add header row
            sb.Append("<tr>");
            foreach (var key in data[0].Keys)
            {
                sb.AppendFormat("<th>{0}</th>", key);
            }
            sb.Append("</tr>");
            // Add data rows
            foreach (var row in data)
            {
                sb.Append("<tr>");
                foreach (var value in row.Values)
                {
                    sb.AppendFormat("<td>{0}</td>", value);
                }
                sb.Append("</tr>");
            }
            sb.Append("</table>");
            return sb.ToString();
        }


        /// <summary>
        /// Given a HTML table in string will convert to JSON version
        /// </summary>
        public static JObject ConvertHtmlTableToJson(string htmlTable)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlTable);
            var table = doc.DocumentNode.SelectSingleNode("//table");
            var rows = table.SelectNodes("tr").Skip(1); // Skip header row
            var header = table.SelectSingleNode("tr"); // Header row
            var data = new List<Dictionary<string, string>>();
            foreach (var row in rows)
            {
                var rowData = new Dictionary<string, string>();
                var cells = row.SelectNodes("td");
                for (int i = 0; i < cells.Count; i++)
                {
                    var cellText = cells[i].InnerText;
                    var headerText = header.SelectNodes("th")[i].InnerText;
                    rowData.Add(headerText, cellText);
                }
                data.Add(rowData);
            }
            return JObject.FromObject(data);
        }


        /// <summary>
        /// Given any data will try to print it as data readable json
        /// Note: made for beautiful code use in python 
        /// </summary>
        public static void Print(dynamic input)
        {
            JToken jsonData = Tools.AnyToJSON("", input);
            var stringJson = jsonData.ToString();
            Console.WriteLine(stringJson);
        }

        //gets the exact width of a text based on Font size & type
        //used to generate nicely fitting background for text
        public static double GetTextWidthPx(string textInput)
        {
            //TODO handle max & min
            //set max & min width background
            //const int maxWidth = 70;
            //backgroundWidth = backgroundWidth > maxWidth ? maxWidth : backgroundWidth;
            //const int minWidth = 30;
            //backgroundWidth = backgroundWidth > minWidth ? minWidth : backgroundWidth;


            SizeF size;
            using (var graphics = Graphics.FromHwnd(IntPtr.Zero))
            {
                size = graphics.MeasureString(textInput,
                    new Font("Calibri", 12, FontStyle.Regular, GraphicsUnit.Pixel));
            }

            var widthPx = Math.Round(size.Width);

            return widthPx;
        }


        /// <summary>
        /// Gets any file at given WWW url will return as string
        /// used for SVG
        /// </summary>
        public static async Task<string> GetStringFileHttp(string url)
        {
            try
            {

                //get the data sender
                using var client = new HttpClient();

                client.Timeout = Timeout.InfiniteTimeSpan;

                //load xml event data files before hand to be used quickly later for search
                //get main horoscope prediction file (located in wwwroot)
                var fileString = await client.GetStringAsync(url, CancellationToken.None);

                return fileString;
            }
            catch (Exception e)
            {
                var msg = $"FAILED TO GET FILE:/n{url}";
                Console.WriteLine(msg);
                LibLogger.Error(msg); //log it
                return "";
            }


        }


        /// <summary>
        /// Gets a SVG icon file direct from Illustrator, removes not needed
        /// attributes and makes it ready to be injected into another SVG
        /// no file return nothing
        /// </summary>
        public static async Task<string> GetSvgIconHttp(string svgFileUrl, double width, double height)
        {

            //get raw icon as SVG (if exist)
            var svgIconString = await Tools.GetStringFileHttp(svgFileUrl);
            if (!string.IsNullOrEmpty(svgIconString))
            {
                //remove XML file header

                var parsedIcon = Svg.SvgDocument.FromSvg<Svg.SvgDocument>(svgIconString);

                //set custom width & height
                parsedIcon.Height = (SvgUnit)height;
                parsedIcon.Width = (SvgUnit)width;
                //parsedIcon.ViewBox = new SvgViewBox(0, 0, (float)width, (float)height);

                var final = parsedIcon.GetXML();

                //<?xml version="1.0" encoding="utf-8"?>
                final = final.Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>", "");
                //<!DOCTYPE svg PUBLIC "-//W3C//DTD SVG 1.1//EN" "http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd">
                final = final.Replace("<!DOCTYPE svg PUBLIC \"-//W3C//DTD SVG 1.1//EN\" \"http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd\">", "");

                return final;
            }

            //if control reaches here than no file
            return "";

        }

        public static Image Svg2Png(string svg, int width, int height)
        {

            byte[] png_bytes;
            string png_base64;
            byte[] byte_array;
            Stream stream;
            SvgDocument svg_document;
            Bitmap bitmap;
            string base64_string;


            //convert svg string to byte array
            //NOTE : proper encoding needed else will shown funny values when render
            byte_array = Encoding.UTF8.GetBytes(svg);

            //convert byte array to stream
            stream = new MemoryStream(byte_array);

            Svg.SvgDocument.EnsureSystemIsGdiPlusCapable();


            //generate svg doc from stream
            svg_document = SvgDocument.Open<Svg.SvgDocument>(stream);

            //convert svg doc to bitmap with specified width & height
            bitmap = svg_document.Draw(width, height);

            return bitmap;

            //png_bytes = ImageToByte2(bitmap);

            //base64_string = Convert.ToBase64String(png_bytes, 0, png_bytes.Length);

            //png_base64 = "data:image/png;base64," + base64_string;

            //return png_bytes;

        }

        public static byte[] ImageToByte2(Image img)
        {
            using (var stream = new MemoryStream())
            {
                img.Save(stream, ImageFormat.Png);
                return stream.ToArray();
            }
        }


        /// <summary>
        /// Extension method to get null when out of range, instead of exception
        /// </summary>
        public static T GetValueOrNull<T>(this List<T> list, int index)
        {
            if (index >= 0 && index < list.Count)
            {
                return list[index];
            }
            else
            {
                return default(T); // Returns null for reference types and zero for numeric value types
            }
        }


        /// <summary>
        /// gets last day of any month at any time
        /// input : 01/1981
        /// </summary>
        public static int GetLastDay(string monthYearText)
        {
            //split month and year
            string[] splited = monthYearText.Split('/');
            var month = int.Parse(splited[0]);
            var year = int.Parse(splited[1]);

            int daysInMonth = DateTime.DaysInMonth(year: year, month: month);
            return daysInMonth;

        }

        /// <summary>
        /// Converts raw call from API via URL to parsed Time
        /// </summary>
        public static async Task<Time> ParseTime(string locationName,
            string hhmmStr,
            string dateStr,
            string monthStr,
            string yearStr,
            string offsetStr)
        {
            WebResult<GeoLocation>? geoLocationResult = await Tools.AddressToGeoLocation(locationName);
            var geoLocation = geoLocationResult.Payload;

            //clean time text
            var timeStr = $"{hhmmStr} {dateStr}/{monthStr}/{yearStr} {offsetStr}";
            var parsedTime = new Time(timeStr, geoLocation);

            return parsedTime;
        }

        /// <summary>
        /// deletes old person record by ID and saves in new one as updated
        /// </summary>
        public static async Task UpdatePersonRecord(Person updatedPersonData)
        {
            //get the person record that needs to be updated
            var originalPerson = await Tools.FindPersonXMLById(updatedPersonData.Id);

            //NOTE:
            //only way it works is to
            //delete the previous person record
            //add new record at bottom

            //delete the old person record,
            await Tools.DeleteXElementFromXDocumentAzure(originalPerson, Tools.PersonListFile, Tools.BlobContainerName);

            //and insert updated record in the updated as new
            //add new person to main list
            await Tools.AddXElementToXDocumentAzure(updatedPersonData.ToXml(), Tools.PersonListFile, Tools.BlobContainerName);

        }

        /// <summary>
        /// Adds an XML element to XML document in by file & container name
        /// and saves files directly to Azure blob store
        /// </summary>
        public static async Task AddXElementToXDocumentAzure(XElement dataXml, string fileName, string containerName)
        {
            //get user data list file (UserDataList.xml) Azure storage
            var fileClient = await Tools.GetBlobClientAzure(fileName, containerName);

            //add new log to main list
            var updatedListXml = await AddXElementToXDocument(fileClient, dataXml);

            //upload modified list to storage
            await OverwriteBlobData(fileClient, updatedListXml);
        }
        public static void AddXElementToXDocumentAzure(XElement dataXml, ref XDocument xDocument)
        {

            //add new log to main list
            AddXElementToXDocument(dataXml, ref xDocument);

        }


        /// <summary>
        /// Overwrites new XML data to a blob file
        /// </summary>
        public static async Task OverwriteBlobData(BlobClient blobClient, XDocument newData)
        {
            //convert xml data to string
            var dataString = newData.ToString();

            //convert xml string to stream
            var dataStream = GenerateStreamFromString(dataString);

            var blobUploadOptions = new BlobUploadOptions();
            blobUploadOptions.AccessTier = AccessTier.Cool; //save money!

            //upload stream to blob
            //note: no override needed because specifying BlobUploadOptions, is auto override
            await blobClient.UploadAsync(dataStream, options: blobUploadOptions);

            //auto correct content type from wrongly set "octet/stream"
            var blobHttpHeaders = new BlobHttpHeaders { ContentType = "text/xml" };
            await blobClient.SetHttpHeadersAsync(blobHttpHeaders);
        }
        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }


        /// <summary>
        /// Adds an XML element to XML document in blob form
        /// </summary>
        public static async Task<XDocument> AddXElementToXDocument(BlobClient xDocuBlobClient, XElement newElement)
        {
            //get person list from storage
            var xDocument = await Tools.DownloadToXDoc(xDocuBlobClient);

            //add new person to list
            xDocument.Root.Add(newElement);

            return xDocument;
        }
        public static void AddXElementToXDocument(XElement newElement, ref XDocument xDocument)
        {
            //add new person to list
            xDocument.Root.Add(newElement);
        }

        /// <summary>
        /// Deletes an XML element from an XML document in by file & container name
        /// and saves files directly to Azure blob store
        /// </summary>
        public static async Task DeleteXElementFromXDocumentAzure(XElement dataXmlToDelete, string fileName, string containerName)
        {
            //access to file
            var fileClient = await Tools.GetBlobClientAzure(fileName, containerName);

            //get xml file
            var xmlDocFile = await Tools.DownloadToXDoc(fileClient);

            //check if record to delete exists
            //if not found, raise alarm
            var xmlRecordList = xmlDocFile.Root.Elements();
            var personToDelete = Person.FromXml(dataXmlToDelete);
            var foundRecords = xmlRecordList.Where(x => Person.FromXml(x).Id == personToDelete.Id);
            if (!foundRecords.Any()) { throw new Exception("Could not find XML record to delete in main list!"); }

            //continue with delete
            foundRecords.First().Remove();

            //upload modified list to storage
            await OverwriteBlobData(fileClient, xmlDocFile);
        }

        /// <summary>
        /// deletes by using same reference to limit unnecessary calls to storage
        /// used in maintenance scripts
        /// deletes by ID not HASH
        /// </summary>
        public static void DeleteXElementFromXDocumentAzure(XElement dataXmlToDelete, ref XDocument xmlDocFile)
        {

            //check if record to delete exists
            //if not found, raise alarm
            var xmlRecordList = xmlDocFile.Root.Elements();
            var personToDelete = Person.FromXml(dataXmlToDelete);
            var foundRecords = xmlRecordList.Where(x => Person.FromXml(x).Id == personToDelete.Id);
            if (!foundRecords.Any()) { throw new Exception("Could not find XML record to delete in main list!"); }

            //continue with delete
            foundRecords.First().Remove();

        }



        /// <summary>
        /// used for finding uncertain time in certain birth day
        /// split a person's day into precision based slices of possible birth times
        /// </summary>
        public static List<Time> GetTimeSlicesOnBirthDay(Person person, double precisionInHours)
        {
            //start of day till end of day
            var dayStart = new Time($"00:00 {person.BirthDateMonthYear} {person.BirthTimeZone}", person.GetBirthLocation());
            var dayEnd = new Time($"23:59 {person.BirthDateMonthYear} {person.BirthTimeZone}", person.GetBirthLocation());

            var finalList = Time.GetTimeListFromRange(dayStart, dayEnd, precisionInHours);

            return finalList;
        }

        public static async Task<T> DelayedResultTask<T>(TimeSpan delay, Func<T> fallbackMaker)
        {
            await Task.Delay(delay);
            return fallbackMaker();
        }

        public static async Task<T> DelayedTimeoutExceptionTask<T>(TimeSpan delay)
        {
            await Task.Delay(delay);
            throw new TimeoutException();
        }

        public static async Task<T> TaskWithTimeoutAndException<T>(Task<T> task, TimeSpan timeout)
        {
            //two task are fired at once, real task and countdown
            return await await Task.WhenAny(task, DelayedTimeoutExceptionTask<T>(timeout));
        }

        public static List<HoroscopeData> SavedHoroscopeDataList { get; set; } = null; //null used for checking empty

        /// <summary>
        /// Get parsed HoroscopeDataList.xml from wwwroot file / static site
        /// Note: auto caching is used
        /// </summary>
        public static async Task<List<HoroscopeData>> GetHoroscopeDataList(string fileUrl)
        {
            //if prediction list already loaded use that instead
            if (SavedHoroscopeDataList != null) { return SavedHoroscopeDataList; }

            //get data list from Static Website storage
            //always get from STABLE for reliability, and also no URL instance here
            var horoscopeDataListXml = await Tools.GetXmlFileHttp(fileUrl);

            //parse each raw event data in list
            var horoscopeDataList = new List<HoroscopeData>();
            foreach (var predictionDataXml in horoscopeDataListXml)
            {
                //add it to the return list
                horoscopeDataList.Add(HoroscopeData.FromXml(predictionDataXml));
            }

            //make a copy to be used later if needed (speed improve)
            SavedHoroscopeDataList = horoscopeDataList;

            return horoscopeDataList;
        }

        /// <summary>
        /// Gets all horoscope predictions for a person
        /// </summary>
        public static async Task<List<HoroscopePrediction>> GetHoroscopePrediction(Time birthTime, string fileUrl)
        {
            //get list of horoscope data (file from wwwroot)
            var horoscopeDataList = await GetHoroscopeDataList(fileUrl);

            //start calculating predictions (mix with time by person's birth date)
            var predictionList = calculate(birthTime, horoscopeDataList);

            return predictionList;

            /// <summary>
            /// Get list of predictions occurring in a time period for all the
            /// inputed prediction types aka "prediction data"
            /// </summary>
            List<HoroscopePrediction> calculate(Time birthTime, List<HoroscopeData> horoscopeDataList)
            {
                //get data to instantiate muhurtha time period
                //get start & end times

                //initialize empty list of event to return
                List<HoroscopePrediction> horoscopeList = new();

                try
                {
                    foreach (var horoscopeData in horoscopeDataList)
                    {
                        //only add if occuring
                        var isOccuring = horoscopeData.IsEventOccuring(birthTime);
                        if (isOccuring)
                        {
                            var newHoroscopePrediction = new HoroscopePrediction(horoscopeData.Name, horoscopeData.Description, horoscopeData.RelatedBody);
                            //add events to main list of event
                            horoscopeList.Add(newHoroscopePrediction);
                        }
                    }
                }
                //catches only exceptions that indicates that user canceled the calculation (caller lost interest in the result)
                catch (Exception)
                {
                    //return empty list
                    return new List<HoroscopePrediction>();
                }

                //return calculated event list
                return horoscopeList;
            }
        }


        /// <summary>
        /// "H1N1" -> ["H", "1", "N", "1"]
        /// "H" -> ["H"]
        /// "GH1N12" -> ["GH", "1", "N", "12"]
        /// "OS234" -> ["OS", "234"]
        /// </summary>
        public static List<string> SplitAlpha(string input)
        {
            var words = new List<string> { string.Empty };
            for (var i = 0; i < input.Length; i++)
            {
                words[words.Count - 1] += input[i];
                if (i + 1 < input.Length && char.IsLetter(input[i]) != char.IsLetter(input[i + 1]))
                {
                    words.Add(string.Empty);
                }
            }
            return words;
        }

        /// <summary>
        /// Converts xml element instance to string properly
        /// </summary>
        public static string XmlToString(XElement xml)
        {
            //remove all formatting, for clean xml as string
            return xml.ToString(SaveOptions.DisableFormatting);
        }

        /// <summary>
        /// Gets XML file from any URL and parses it into xelement list
        /// </summary>
        public static async Task<List<XElement>> GetXmlFileHttp(string url)
        {
            //get the data sender
            using var client = new HttpClient();

            //load xml event data files before hand to be used quickly later for search
            //get main horoscope prediction file (located in wwwroot)
            var fileStream = await client.GetStreamAsync(url);

            //parse raw file to xml doc
            var document = XDocument.Load(fileStream);

            //get all records in document
            return document.Root.Elements().ToList();
        }

        /// <summary>
        /// Converts any type to XML, it will use Type's own ToXml() converter if available
        /// else ToString is called and placed inside element with Type's full name
        /// Note, used to transfer data via internet Client to API Server
        /// Example:
        /// <TypeName>
        ///     DataValue
        /// </TypeName>
        /// </summary>
        public static XElement AnyTypeToXml<T>(T value)
        {
            //check if type has own ToXml method
            //use the Type's own converter if available
            if (value is IToXml hasToXml)
            {
                var betterXml = hasToXml.ToXml();
                return betterXml;
            }

            //gets enum value as string to place inside XML
            //note: value can be null hence ?, fails quietly
            var enumValueStr = value?.ToString();

            //get the name of the Enum
            //Note: This is the name that will be used
            //later to instantiate the class from string
            var typeName = typeof(T).FullName;

            return new XElement(typeName, enumValueStr);
        }

        /// <summary>
        /// will convert inputed type to xelement via .net serializer
        /// </summary>
        public static XElement AnyTypeToXElement(object o)
        {
            var doc = new XDocument();
            using (XmlWriter writer = doc.CreateWriter())
            {
                XmlSerializer serializer = new XmlSerializer(o.GetType());
                serializer.Serialize(writer, o);
            }

            return doc.Root;
        }

        /// <summary>
        /// Converts any type that implements IToXml to XML, it will use Type's own ToXml() converter
        /// Note, used to transfer data via internet Client to API Server
        /// Placed inside "Root" xml
        /// Default name for root element is Root
        /// </summary>
        public static XElement AnyTypeToXmlList<T>(List<T> xmlList, string rootElementName = "Root") where T : IToXml
        {
            var rootXml = new XElement(rootElementName);
            foreach (var xmlItem in xmlList)
            {
                rootXml.Add(AnyTypeToXml(xmlItem));
            }
            return rootXml;
        }

        /// <summary>
        /// Simple override for XML, to skip parsing to type before sorting
        /// </summary>
        public static XElement AnyTypeToXmlList(List<XElement> xmlList, string rootElementName = "Root")
        {
            var rootXml = new XElement(rootElementName);
            foreach (var xmlItem in xmlList)
            {
                rootXml.Add(xmlItem);
            }
            return rootXml;
        }

        /// <summary>
        /// Given the URL of a standard VedAstro XML file, like "http://...PersonList.xml",
        /// will convert to the specified type and return in nice list, with time to be home for dinner
        /// </summary>
        public static async Task<List<T>> ConvertXmlListFileToInstanceList<T>(string httpUrl) where T : IToXml, new()
        {
            //get data list from Static Website storage
            //note : done so that any updates to that live file will be instantly reflected in API results
            var eventDataListXml = await Tools.GetXmlFileHttp(httpUrl);

            var finalInstance = await ConvertXmlListFileToInstanceList<T>(eventDataListXml);

            return finalInstance;
        }

        /// <summary>
        /// Given an XML file will auto convert it to an instance using ToXml() method
        /// </summary>
        public static async Task<List<T>> ConvertXmlListFileToInstanceList<T>(List<XElement> eventDataListXml) where T : IToXml, new()
        {
            //parse each raw event data in list
            var eventDataList = new List<T>();
            foreach (var eventDataXml in eventDataListXml)
            {
                //add it to the return list
                var x = new T();
                eventDataList.Add(x.FromXml<T>(eventDataXml));
            }

            return eventDataList;

        }



        /// <summary>
        /// Extracts data from an Exception puts it in a nice XML
        /// </summary>
        public static XElement ExceptionToXML(Exception e)
        {
            //place to store the exception data
            string fileName;
            string methodName;
            int line;
            int columnNumber;
            string message;
            string source;

            //get the exception that started it all
            var originalException = e.GetBaseException();

            //extract the data from the error
            StackTrace st = new StackTrace(e, true);

            //Get the first stack frame
            StackFrame frame = st.GetFrame(st.FrameCount - 1);

            //Get the file name
            fileName = frame?.GetFileName();

            //Get the method name
            methodName = frame.GetMethod()?.Name;

            //Get the line number from the stack frame
            line = frame.GetFileLineNumber();

            //Get the column number
            columnNumber = frame.GetFileColumnNumber();

            message = originalException.ToString();

            source = originalException.Source;
            //todo include inner exception data
            var stackTrace = originalException.StackTrace;


            //put together the new error record
            var newRecord = new XElement("Error",
                new XElement("Message", message),
                new XElement("Source", source),
                new XElement("FileName", fileName),
                new XElement("SourceLineNumber", line),
                new XElement("SourceColNumber", columnNumber),
                new XElement("MethodName", methodName),
                new XElement("MethodName", methodName)
            );


            return newRecord;
        }

        /// <summary>
        /// - Type is a value typ
        /// - Enum
        /// </summary>
        public static dynamic XmlToAnyType<T>(XElement xml) // where T : //IToXml, new()
        {
            //get the name of the Enum
            var typeNameFullName = typeof(T).FullName;
            var typeNameShortName = typeof(T).FullName;

#if DEBUG
            Console.WriteLine(xml.ToString());
#endif

            //type name inside XML
            var xmlElementName = xml?.Name;

            //get the value for parsing later
            var rawVal = xml.Value;


            //make sure the XML enclosing type has the same name
            //check both full class name, and short class name
            var isSameName = xmlElementName == typeNameFullName || xmlElementName == typeof(T).GetShortTypeName();

            //if not same name raise error
            if (!isSameName)
            {
                throw new Exception($"Can't parse XML {xmlElementName} to {typeNameFullName}");
            }

            //implements ToXml()
            var typeImplementsToXml = typeof(T).GetInterfaces().Any(x =>
                x.IsGenericType &&
                x.GetGenericTypeDefinition() == typeof(IToXml));

            //type has owm ToXml method
            if (typeImplementsToXml)
            {
                dynamic inputTypeInstance = GetInstance(typeof(T).FullName);

                return inputTypeInstance.FromXml(xml);

            }

            //if type is an Enum process differently
            if (typeof(T).IsEnum)
            {
                var parsedEnum = (T)Enum.Parse(typeof(T), rawVal);

                return parsedEnum;
            }

            //else it is a value type
            if (typeof(T) == typeof(string))
            {
                return rawVal;
            }

            if (typeof(T) == typeof(double))
            {
                return Double.Parse(rawVal);
            }

            if (typeof(T) == typeof(int))
            {
                return int.Parse(rawVal);
            }

            //raise error since converter not implemented
            throw new NotImplementedException($"XML converter for {typeNameFullName}, not implemented!");
        }

        /// <summary>
        /// Gets only the name of the Class, without assembly
        /// </summary>
        public static string GetShortTypeName(this Type type)
        {
            var sb = new StringBuilder();
            var name = type.Name;
            if (!type.IsGenericType) return name;
            sb.Append(name.Substring(0, name.IndexOf('`')));
            sb.Append("<");
            sb.Append(string.Join(", ", type.GetGenericArguments()
                .Select(t => t.GetShortTypeName())));
            sb.Append(">");
            return sb.ToString();
        }

        public static bool Implements<I>(this Type type, I @interface) where I : class
        {
            if (((@interface as Type) == null) || !(@interface as Type).IsInterface)
                throw new ArgumentException("Only interfaces can be 'implemented'.");

            return (@interface as Type).IsAssignableFrom(type);
        }

        /// <summary>
        /// For converting value types, String, Double, etc.
        /// </summary>
        //public static dynamic XmlToValueType<T>(XElement xml) 
        //{
        //    //get the name of the Enum
        //    var typeName = nameof(T);


        //    //raise error since not XML type and Input type mismatch
        //    throw new Exception($"Can't parse XML to {typeName}");
        //}


        /// <summary>
        /// Gets an instance of Class from string name
        /// </summary>
        public static object GetInstance(string strFullyQualifiedName)
        {
            Type type = Type.GetType(strFullyQualifiedName);
            if (type != null)
                return Activator.CreateInstance(type);
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = asm.GetType(strFullyQualifiedName);
                if (type != null)
                    return Activator.CreateInstance(type);
            }

            return null;
        }


        /// <summary>
        /// Converts days to hours
        /// </summary>
        /// <returns></returns>
        public static double DaysToHours(double days) => days * 24.0;

        public static double MinutesToHours(double minutes) => minutes / 60.0;

        public static double MinutesToYears(double minutes) => minutes / 525600.0;

        public static double MinutesToDays(double minutes) => minutes / 1440.0;

        /// <summary>
        /// Given a date it will count the days to the end of that year
        /// </summary>
        public static double GetDaysToNextYear(Time getBirthDateTime)
        {
            //get start of next year
            var standardTime = getBirthDateTime.GetStdDateTimeOffset();
            var nextYear = standardTime.Year + 1;
            var startOfNextYear = new DateTimeOffset(nextYear, 1, 1, 0, 0, 0, 0, standardTime.Offset);

            //calculate difference of days between 2 dates
            var diffDays = (startOfNextYear - standardTime).TotalDays;

            return diffDays;
        }

        /// <summary>
        /// Gets the time now in the system in text form
        /// formatted with standard style (HH:mm dd/MM/yyyy zzz) 
        /// </summary>
        public static string GetNowSystemTimeText() => DateTimeOffset.Now.ToString(Time.DateTimeFormat);

        /// <summary>
        /// Gets the time now in the system in text form with seconds (HH:mm:ss dd/MM/yyyy zzz) 
        /// </summary>
        public static string GetNowSystemTimeSecondsText() => DateTimeOffset.Now.ToString(Time.DateTimeFormatSeconds);

        /// <summary>
        /// Gets the time now in the Server (+8:00) in text form with seconds (HH:mm:ss dd/MM/yyyy zzz) 
        /// </summary>
        public static string GetNowServerTimeSecondsText() => DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(8)).ToString(Time.DateTimeFormatSeconds);

        /// <summary>
        /// Custom hash generator for Strings. Returns consistent/deterministic values
        /// If null returns 0
        /// Note: MD5 (System.Security.Cryptography) not used because not supported in Blazor WASM
        /// </summary>
        public static int GetStringHashCode(string stringToHash)
        {
            if (stringToHash == null)
            {
                return 0;
            }

            unchecked
            {
                int hash1 = (5381 << 16) + 5381;
                int hash2 = hash1;

                for (int i = 0; i < stringToHash.Length; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ stringToHash[i];
                    if (i == stringToHash.Length - 1)
                        break;
                    hash2 = ((hash2 << 5) + hash2) ^ stringToHash[i + 1];
                }

                return hash1 + (hash2 * 1566083941);
            }


            //MD5 md5Hasher = MD5.Create();
            //var hashedByte = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(stringToHash));
            //return BitConverter.ToInt32(hashedByte, 0);

        }

        /// <summary>
        /// Gets random unique ID
        /// </summary>
        public static string GenerateId() => Guid.NewGuid().ToString("N");


        /// <summary>
        /// Converts any list to comma separated string
        /// Note: calls ToString();
        /// </summary>
        public static string ListToString<T>(List<T> list)
        {
            var combinedNames = "";

            for (int i = 0; i < list.Count; i++)
            {
                //when last in row, don't add comma
                var isLastItem = i == (list.Count - 1);
                var ending = isLastItem ? "" : ", ";

                //combine to together based on type
                var item = list[i];
                if (item is IToJson iToJson)
                {
                    //todo can wrap into jobject if needed
                    combinedNames += iToJson.ToJson() + ending;
                }
                else
                {
                    combinedNames += item.ToString() + ending;
                }

            }

            return combinedNames;
        }







        //█▀▀ █░█ ▀▀█▀▀ █▀▀ █▀▀▄ █▀▀ ░▀░ █▀▀█ █▀▀▄ 　 █▀▄▀█ █▀▀ ▀▀█▀▀ █░░█ █▀▀█ █▀▀▄ █▀▀ 
        //█▀▀ ▄▀▄ ░░█░░ █▀▀ █░░█ ▀▀█ ▀█▀ █░░█ █░░█ 　 █░▀░█ █▀▀ ░░█░░ █▀▀█ █░░█ █░░█ ▀▀█ 
        //▀▀▀ ▀░▀ ░░▀░░ ▀▀▀ ▀░░▀ ▀▀▀ ▀▀▀ ▀▀▀▀ ▀░░▀ 　 ▀░░░▀ ▀▀▀ ░░▀░░ ▀░░▀ ▀▀▀▀ ▀▀▀░ ▀▀▀


        public static string? Truncate(this string? value, int maxLength, string truncationSuffix = "…")
        {
            return value?.Length > maxLength
                ? value.Substring(0, maxLength) + truncationSuffix
                : value;
        }

        /// <summary>
        /// Find the first offset in the string that might contain the characters
        /// in `needle`, in any order. Returns -1 if not found.
        /// <para>This function can return false positives</para>
        /// </summary>
        public static bool FindCluster(this string haystack, string needle)
        {
            if (haystack == null) return false;
            if (needle == null) return false;

            if (haystack.Length < needle.Length) return false;

            long sum = needle.ToCharArray().Sum(c => c);
            long rolling = haystack.ToCharArray().Take(needle.Length).Sum(c => c);

            var idx = 0;
            var head = needle.Length;
            while (rolling != sum)
            {
                if (head >= haystack.Length) return false;
                rolling -= haystack[idx];
                rolling += haystack[head];
                head++;
                idx++;
            }

            return true;
        }

        /// <summary>
        /// Remap from 1 range to another
        /// </summary>
        public static float Remap(this float from, float fromMin, float fromMax, float toMin, float toMax)
        {
            var fromAbs = from - fromMin;
            var fromMaxAbs = fromMax - fromMin;

            var normal = fromAbs / fromMaxAbs;

            var toMaxAbs = toMax - toMin;
            var toAbs = toMaxAbs * normal;

            var to = toAbs + toMin;

            return to;
        }

        /// <summary>
        /// Remap from 1 range to another
        /// </summary>
        public static double Remap(this double from, double fromMin, double fromMax, double toMin, double toMax)
        {
            var fromAbs = from - fromMin;
            var fromMaxAbs = fromMax - fromMin;

            var normal = fromAbs / fromMaxAbs;

            var toMaxAbs = toMax - toMin;
            var toAbs = toMaxAbs * normal;

            var to = toAbs + toMin;

            return to;
        }

        public static string StreamToString(Stream stream)
        {
            StreamReader reader = new StreamReader(stream);
            string text = reader.ReadToEnd();

            return text;
        }

        /// <summary>
        /// Converts a timezone (+08:00) in string form to parsed timespan 
        /// </summary>
        public static TimeSpan StringToTimezone(string timezoneRaw)
        {
            return DateTimeOffset.ParseExact(timezoneRaw, "zzz", CultureInfo.InvariantCulture).Offset;
        }

        /// <summary>
        /// Returns system timezone offset as TimeSpan
        /// </summary>
        public static string GetSystemTimezoneStr() => DateTimeOffset.Now.ToString("zzz");

        /// <summary>
        /// Returns system timezone offset as TimeSpan
        /// </summary>
        public static TimeSpan GetSystemTimezone() => DateTimeOffset.Now.Offset;


        /// <summary>
        /// Given a place's name, using API will get location
        /// </summary>
        public static async Task<WebResult<GeoLocation>> AddressToGeoLocation(string address)
        {
            //get location data from VedAstro API
            var webResult = await Tools.ReadFromServerXmlReply(URL.AddressToGeoLocationAPIStable + $"/{address}");

            //if fail to make call, end here
            if (!webResult.IsPass) { return new WebResult<GeoLocation>(false, GeoLocation.Empty); }

            //if success, get the reply data out
            var rootXml = webResult.Payload;
            Console.WriteLine(rootXml);
            var parsed = GeoLocation.FromXml(rootXml);

            //return to caller pass
            return new WebResult<GeoLocation>(true, parsed);
        }


        /// <summary>
        /// Given a location & time, will use Google Timezone API
        /// to get accurate time zone that was/is used
        /// Must input valid geo location 
        /// NOTE:
        /// - offset of timeAtLocation not important
        /// - googleGeoLocationApiKey needed to work
        /// </summary>
        public static async Task<TimeSpan> GetTimezoneOffset(string locationName, DateTimeOffset timeAtLocation)
        {
            //get geo location first then call underlying method
            var geoLocation = await GeoLocation.FromName(locationName);
            return Tools.StringToTimezone(await GetTimezoneOffsetApi(geoLocation, timeAtLocation));
        }

        public static async Task<string> GetTimezoneOffsetString(string locationName, DateTime timeAtLocation)
        {
            //get geo location first then call underlying method
            var geoLocation = await GeoLocation.FromName(locationName);
            return await GetTimezoneOffsetApi(geoLocation, timeAtLocation);
        }

        public static async Task<string> GetTimezoneOffsetString(string location, string dateTime)
        {
            //get timezone from Google API
            var lifeEvtTimeNoTimezone = DateTime.ParseExact(dateTime, Time.DateTimeFormatNoTimezone, null);
            var timezone = await Tools.GetTimezoneOffsetString(location, lifeEvtTimeNoTimezone);

            return timezone;

            //get start time of life event and find the position of it in slices (same as now line)
            //so that this life event line can be placed exactly on the report where it happened
            //var lifeEvtTimeStr = $"{dateTime} {timezone}"; //add offset 0 only for parsing, not used by API to get timezone
            //var lifeEvtTime = DateTimeOffset.ParseExact(lifeEvtTimeStr, Time.DateTimeFormat, null);

            //return lifeEvtTime;
        }

        /// <summary>
        /// Given a location & time, will use Google Timezone API
        /// to get accurate time zone that was/is used, if Google fail,
        /// then auto default to system timezone
        /// NOTE:
        /// - sometimes unexpected failure to call google by some clients only
        /// - offset of timeAtLocation not important
        /// - googleGeoLocationApiKey needed to work
        /// </summary>
        public static async Task<WebResult<string>> GetTimezoneOffsetApi(GeoLocation geoLocation, DateTimeOffset timeAtLocation)
        {

            try
            {
                //get location data from VedAstro API
                var timePackage = new Time(timeAtLocation, geoLocation);
                var url = URL.GeoLocationToTimezoneAPIStable + timePackage.ToUrl();
                var webResult = await Tools.ReadFromServerXmlReply(url);

                //if fail to make call, end here
                if (!webResult.IsPass) { return new WebResult<string>(false, ""); }

                //if success, get the reply data out
                var data = webResult.Payload.Value;

                //return to caller pass
                return new WebResult<string>(true, data);

            }
            catch (Exception e)
            {
                LibLogger.Error(e);
                //return to caller pass
                return new WebResult<string>(false, "");
            }
        }

        /// <summary>
        /// Given a timespan instance converts to string timezone +08:00
        /// </summary>
        public static string TimeSpanToUTCTimezoneString(TimeSpan offsetMinutes)
        {
            var x = DateTimeOffset.UtcNow.ToOffset(offsetMinutes).ToString("zzz");
            return x;
        }

        /// <summary>
        /// When using google api to get timezone data, the API returns a reply in XML similar to one below
        /// This function parses this raw XML data from google to TimeSpan data we need
        /// It also checks for other failures like wrong location name
        /// Failing when parsing this TimeZoneResponse XML has occurred enough times, for its own method
        /// </summary>
        public static bool TryParseGoogleTimeZoneResponse(XElement timeZoneResponseXml, out TimeSpan offsetMinutes)
        {
            //<?xml version="1.0" encoding="UTF-8"?>
            //<TimeZoneResponse>
            //    <status>INVALID_REQUEST </ status >
            //    < error_message > Invalid request.Invalid 'location' parameter.</ error_message >
            //</ TimeZoneResponse >

            //extract out the data from google's reply timezone offset
            var status = timeZoneResponseXml?.Element("status")?.Value ?? "";
            var failed = status.Contains("INVALID_REQUEST");

            //try process data if did NOT fail so far
            if (!failed)
            {
                double offsetSeconds;

                //get raw data from XML
                var rawOffsetData = timeZoneResponseXml?.Element("raw_offset")?.Value;

                //at times google api returns no valid data, but call is replied as normal
                //so check for that here, if fail end here
                if (string.IsNullOrEmpty(rawOffsetData)) { goto Fail; }

                //try to parse what ever value there is, should be number
                else
                {
                    var isNumber = double.TryParse(rawOffsetData, out offsetSeconds);
                    if (!isNumber) { goto Fail; } //if not number end here
                }

                //offset needs to be "whole" minutes, else fail
                //purposely hard cast to int to remove not whole minutes
                var notWhole = TimeSpan.FromSeconds(offsetSeconds).TotalMinutes;
                offsetMinutes = TimeSpan.FromMinutes((int)Math.Round(notWhole)); //set

                //let caller know valid data
                return true;
            }

        //if fail let caller know something went wrong & set to 0s
        Fail:
            LibLogger.Error(timeZoneResponseXml);
            offsetMinutes = TimeSpan.Zero;
            return false;


        }

        /// <summary>
        /// Calls a URL and returns the content of the result as XML
        /// Even if content is returned as JSON, it is converted to XML
        /// Note:
        /// - if JSON auto adds "Root" as first element, unless specified
        /// for XML data root element name is ignored
        /// </summary>
        public static async Task<WebResult<XElement>> ReadFromServerXmlReply(string apiUrl, string rootElementName = "Root")
        {
            //send request to API server
            var result = await RequestServer(apiUrl);

            //get raw reply from the server response
            var rawMessage = await result.Content?.ReadAsStringAsync() ?? "";

            //only good reply from server is accepted, anything else is marked invalid
            //stops invalid replies from being passed as valid
            if (!result.IsSuccessStatusCode) { return new WebResult<XElement>(false, new("RawErrorData", rawMessage)); }

            //tries to parse the raw data received into XML or JSON
            //if all fail will return raw data with fail status
            var parsed = ParseData(rawMessage);


            return parsed;




            //----------------------------------------------------------
            // FUNCTIONS

            WebResult<XElement> ParseData(string inputRawString)
            {
                var exceptionList = new List<Exception>();

                try
                {
                    //OPTION 1 : xml with VedAstro standard reply
                    var parsedXml = XElement.Parse(inputRawString);
                    var returnVal = WebResult<XElement>.FromXml(parsedXml);
                    //if can't parse, raise exception so can check other methods
                    if (!returnVal.IsPass) { throw new InvalidOperationException(); }
                    return returnVal;
                }
                catch (Exception e1)
                {
                    try
                    {
                        //OPTION 2 : xml 3rd party reply (google)
                        var parsedXml = XElement.Parse(inputRawString);
                        return new WebResult<XElement>(true, parsedXml);
                    }
                    catch (Exception e2) { exceptionList.Add(e2); }

                    try
                    {
                        //OPTION 3 : json 3rd party reply
                        var parsedJson = JsonConvert.DeserializeXmlNode(inputRawString, "LocationData");
                        var wrappedXml = XElement.Parse(parsedJson.InnerXml); //expected to fail if not right
                        return new WebResult<XElement>(true, wrappedXml);
                    }
                    catch (Exception e3) { exceptionList.Add(e3); } //if fail just void print

                    exceptionList.Add(e1);

                    //send all exception data to server
                    foreach (var exception in exceptionList) { LibLogger.Error(exception, inputRawString); }

                    //if control reaches here all has failed
                    return new WebResult<XElement>(false, new XElement("Failed"));
                }
            }

            async Task<HttpResponseMessage> RequestServer(string receiverAddress)
            {
                //prepare the data to be sent
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, receiverAddress);

                //get the data sender 
                using var client = new HttpClient() { Timeout = new TimeSpan(0, 0, 0, 0, Timeout.Infinite) }; //no timeout

                //tell sender to wait for complete reply before exiting
                var waitForContent = HttpCompletionOption.ResponseContentRead;

                //send the data on its way
                var response = await client.SendAsync(httpRequestMessage, waitForContent);

                //return the raw reply to caller
                return response;
            }


        }

        /// <summary>
        /// Given a list of strings will return one by random
        /// Used to make dynamic user error & info messages
        /// </summary>
        public static string RandomSelect(string[] msgList)
        {
            // Create a Random object  
            Random rand = new Random();

            // Generate a random index less than the size of the array.  
            int randomIndexNumber = rand.Next(msgList.Length);

            //return random text from list to caller
            return msgList[randomIndexNumber];
        }

        public static int GetRandomNumber(int maxNumber)
        {
            // Create a Random object  
            Random rand = new Random();

            // Generate a random index less than the size of the array.  
            int randomIndexNumber = rand.Next(maxNumber);

            //return random text from list to caller
            return randomIndexNumber;
        }

        /// <summary>
        /// Split string by character count
        /// </summary>
        public static IEnumerable<string> SplitByCharCount(string str, int maxChunkSize)
        {
            for (int i = 0; i < str.Length; i += maxChunkSize)
                yield return str.Substring(i, Math.Min(maxChunkSize, str.Length - i));
        }

        /// <summary>
        /// Inputed event name has be space separated
        /// </summary>
        public static List<PlanetName> GetPlanetFromName(string eventName)
        {
            var returnList = new List<PlanetName>();

            //lower case it
            var lowerCased = eventName.ToLower();

            //split into words
            var splited = lowerCased.Split(' ');

            //check if any be parsed into planet name
            foreach (var word in splited)
            {
                var result = PlanetName.TryParse(word, out var planetParsed);
                if (result)
                {
                    //add list if parsed
                    returnList.Add(planetParsed);
                }
            }


            //return list to caller
            return returnList;
        }

        /// <summary>
        /// Packages the data into ready form for the HTTP client to use in final sending stage
        /// </summary>
        public static StringContent XmLtoHttpContent(XElement data)
        {
            //gets the main XML data as a string
            var dataString = Tools.XmlToString(data);

            //specify the data encoding
            var encoding = Encoding.UTF8;

            //specify the type of the data sent
            //plain text, stops auto formatting
            var mediaType = "plain/text";

            //return packaged data to caller
            return new StringContent(dataString, encoding, mediaType);
        }

        /// <summary>
        /// Packages the data into ready form for the HTTP client to use in final sending stage
        /// </summary>
        public static StringContent JsontoHttpContent(JToken data)
        {
            //gets the main XML data as a string
            var dataString = data.ToString();

            //specify the data encoding todo es mui nesasito?
            var encoding = Encoding.UTF8;

            //specify the type of the data sent
            //plain text, stops auto formatting
            var mediaType = "application/json";

            //return packaged data to caller
            return new StringContent(dataString, encoding, mediaType);
        }



        /// <summary>
        /// Extracts data from an Exception puts it in a nice JSON
        /// </summary>
        public static JObject ExceptionToJSON(Exception e)
        {
            //place to store the exception data
            string fileName;
            string methodName;
            int line;
            int columnNumber;
            string message;
            string source;

            //get the exception that started it all
            var originalException = e.GetBaseException();

            //extract the data from the error
            StackTrace st = new StackTrace(e, true);

            //Get the first stack frame
            StackFrame frame = st.GetFrame(st.FrameCount - 1);

            //Get the file name
            fileName = frame?.GetFileName();

            //Get the method name
            methodName = frame.GetMethod()?.Name;

            //Get the line number from the stack frame
            line = frame.GetFileLineNumber();

            //Get the column number
            columnNumber = frame.GetFileColumnNumber();

            message = originalException.ToString();

            source = originalException.Source;
            //todo include inner exception data
            var stackTrace = originalException.StackTrace;


            //put together the new error record
            var newRecord = new JObject(
                new JProperty("Error", new JObject(
                    new JProperty("Message", message),
                    new JProperty("Source", source),
                    new JProperty("FileName", fileName),
                    new JProperty("SourceLineNumber", line),
                    new JProperty("SourceColNumber", columnNumber),
                    new JProperty("MethodName", methodName)
                ))
            );


            return newRecord;
        }

        /// <summary>
        /// Gets now time with seconds in wrapped in xml element
        /// used for logging
        /// </summary>
        public static XElement TimeStampSystemXml => new("TimeStamp", Tools.GetNowSystemTimeSecondsText());

        /// <summary>
        /// Gets now time at server location (+8:00) with seconds in wrapped in xml element
        /// used for logging
        /// </summary>
        public static XElement TimeStampServerXml => new("TimeStampServer", Tools.GetNowServerTimeSecondsText());

        /// <summary>
        /// Converts any Enum from URL epx : ../EnumName/EnumValue
        /// </summary>
        public static async Task<dynamic> EnumFromUrl(string url)
        {
            string[] parts = url.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            //enum has simple structure ../ZodiacName/Leo
            var enumName = parts[0];
            var enumValue = parts[1];

            //get the type of the enum
            var enumType = Type.GetType("VedAstro.Library." + enumName);

            object parsedZodiac;
            try
            {
                //parse the string to an enum value
                parsedZodiac = Enum.Parse(enumType, enumValue);
            }
            catch (Exception e)
            {
                //enum value could is a number
                parsedZodiac = double.Parse(enumValue);
            }

            return parsedZodiac;
        }

        private static readonly Random Random = new Random();

        /// <summary>
        /// we get all the values of the enum using Enum.GetValues,
        /// generate a random index, and return the enum value at that index
        /// </summary>
        public static object GetRandomEnumValue(Type enumType)
        {
            if (!enumType.IsEnum)
            {
                throw new ArgumentException("Type must be an enumeration");
            }
            var values = Enum.GetValues(enumType);
            var index = Random.Next(values.Length);
            return values.GetValue(index);
        }

        /// <summary>
        /// Gets now time in UTC +8:00
        /// Because server time is uncertain, all change to UTC8
        /// </summary>
        public static string GetNow()
        {
            //create utc 8
            var utc8 = new TimeSpan(8, 0, 0);
            //get now time in utc 0
            var nowTime = DateTimeOffset.Now.ToUniversalTime();
            //convert time utc 0 to utc 8
            var utc8Time = nowTime.ToOffset(utc8);

            //return converted time to caller
            return utc8Time.ToString(Time.DateTimeFormatSeconds);
        }

        /// <summary>
        /// Removes all invalid characters for an person name
        /// used to clean name field user input
        /// allowed chars : periods (.) and hyphens (-), space ( )
        /// SRC:https://learn.microsoft.com/en-us/dotnet/standard/base-types/how-to-strip-invalid-characters-from-a-string
        /// </summary>
        public static string CleanAndFormatNameText(string nameInput)
        {
            // Replace invalid characters with empty strings.
            try
            {
                //remove invalid
                var cleanText = Regex.Replace(nameInput, @"[^\w\.\s*-]", "", RegexOptions.None, TimeSpan.FromSeconds(2));

                var textinfo = new CultureInfo("en-US", false).TextInfo;

                //tit le case it!, needs all small else will fail when some nut puts all as capital 
                cleanText = cleanText.ToLower(); //lower
                cleanText = textinfo.ToTitleCase(cleanText); //convert

                return cleanText;
            }
            // If we timeout when replacing invalid characters,
            // we should return Empty.
            catch (RegexMatchTimeoutException)
            {
                return string.Empty;
            }
        }


        /// <summary>
        /// Given a parsed XML element will convert to Json string
        /// Note:
        /// - auto removes Root from XML (since XML needs it & JSON does not)
        /// - this light weight only uses Newtownsoft
        /// - Newtownsoft here because the converter is better than .net's
        /// </summary>
        public static string XmlToJsonString(XElement xElement)
        {
            //no XML indent
            var finalXml = xElement.ToString(SaveOptions.DisableFormatting);

            //convert to JSON
            XmlDocument doc = new XmlDocument(); //NOTE: different xDOC from .Net's
            doc.LoadXml(finalXml);

            //removes "Root" from xml
            string jsonText = JsonConvert.SerializeXmlNode(doc, Formatting.None, true);

            return jsonText;
        }

        ///// <summary>
        ///// Parses from XML > string > .Net JSON
        ///// NOTE:
        ///// - compute heavier than just string, use wisely
        ///// </summary>
        //public static JsonElement XmlToJson(XElement xElement)
        //{
        //    //convert xml to JSON string
        //    var jsonStr = XmlToJsonString(xElement);

        //    //convert string 
        //    using JsonDocument doc = JsonDocument.Parse(jsonStr);
        //    JsonElement root = doc.RootElement;

        //    return root;
        //}
        public static JObject XmlToJson(XElement xElement)
        {
            var x = XmlToJsonString(xElement);

            var y = JObject.Parse(x);

            return y;
        }

        /// <summary>
        /// Converts VedAstro planet name to Swiss Eph planet
        /// </summary>
        /// <returns></returns>
        public static int VedAstroToSwissEph(PlanetName planetName)
        {
            int planet = 0;

            //Convert PlanetName to SE_PLANET type
            if (planetName == PlanetName.Sun)
                planet = SwissEph.SE_SUN;
            else if (planetName == PlanetName.Moon)
            {
                planet = SwissEph.SE_MOON;
            }
            else if (planetName == PlanetName.Mars)
            {
                planet = SwissEph.SE_MARS;
            }
            else if (planetName == PlanetName.Mercury)
            {
                planet = SwissEph.SE_MERCURY;
            }
            else if (planetName == PlanetName.Jupiter)
            {
                planet = SwissEph.SE_JUPITER;
            }
            else if (planetName == PlanetName.Venus)
            {
                planet = SwissEph.SE_VENUS;
            }
            else if (planetName == PlanetName.Saturn)
            {
                planet = SwissEph.SE_SATURN;
            }
            else if (planetName == PlanetName.Rahu)
            {
                //set based on user preference
                planet = Calculate.UseMeanRahuKetu ? SwissEph.SE_MEAN_NODE : SwissEph.SE_TRUE_NODE;
            }
            else if (planetName == PlanetName.Ketu)
            {
                //NOTES:
                //the true node, which is the point where the Moon's orbit crosses the ecliptic plane
                //can also be SE_OSCU_APOG, but no need to add 180

                //set based on user preference, ask for rahu values then add 180 later
                planet = Calculate.UseMeanRahuKetu ? SwissEph.SE_MEAN_NODE : SwissEph.SE_TRUE_NODE;
            }

            return planet;
        }

        /// <summary>
        /// Converts string name of planets, all case to swiss type
        /// </summary>
        public static int StringToSwissEph(string planetName)
        {
            int planet = 0;

            //make small case, best reliability
            planetName = planetName.ToLower();

            //Convert PlanetName to SE_PLANET type
            if (planetName == "sun")
                planet = SwissEph.SE_SUN;
            else if (planetName == "moon")
            {
                planet = SwissEph.SE_MOON;
            }
            else if (planetName == "mars")
            {
                planet = SwissEph.SE_MARS;
            }
            else if (planetName == "Mercury")
            {
                planet = SwissEph.SE_MERCURY;
            }
            else if (planetName == "Jupiter")
            {
                planet = SwissEph.SE_JUPITER;
            }
            else if (planetName == "Venus")
            {
                planet = SwissEph.SE_VENUS;
            }
            else if (planetName == "Saturn")
            {
                planet = SwissEph.SE_SATURN;
            }
            else if (planetName == "Rahu")
            {
                //set based on user preference
                planet = Calculate.UseMeanRahuKetu ? SwissEph.SE_MEAN_NODE : SwissEph.SE_TRUE_NODE;
            }
            else if (planetName == "Ketu")
            {
                //NOTES:
                //the true node, which is the point where the Moon's orbit crosses the ecliptic plane
                //can also be SE_OSCU_APOG, but no need to add 180

                //set based on user preference, ask for rahu values then add 180 later
                planet = Calculate.UseMeanRahuKetu ? SwissEph.SE_MEAN_NODE : SwissEph.SE_TRUE_NODE;
            }

            return planet;
        }


        /// <summary>
        /// All possible for all celestial body types
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> ApiDataPropertyCallList()
        {
            var returnList = new List<string>();

            //get all possible calls for API
            //get all calculators that can work with the inputed data
            var calculatorClass = typeof(Calculate);

            foreach (var methodInfo in calculatorClass.GetMethods())
            {
                //get special API name
                returnList.Add(methodInfo.Name);
            }

            return returnList;

        }


        /// <summary>
        /// Given any string will remove the white spaces
        /// </summary>
        public static string RemoveWhiteSpace(string stringWithSpace)
        {
            var removed = string.Join("", stringWithSpace.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));

            return removed;
        }


        /// <summary>
        /// Get all methods that is available to 2 params
        /// this is the list that will appear on the fly in API Builder dropdown
        /// </summary>
        /// <returns></returns>
        public static List<OpenAPIMetadata> GetPlanetApiCallList<T1, T2>()
        {
            //get all the same methods gotten by Open api func
            var calcList = GetCalculatorListByParam<T1, T2>();

            //extract needed data out in convenient form
            var finalList = OpenAPIMetadata.FromMethodInfoList(calcList);

            return finalList;
        }


        /// <summary>
        /// Gets a unique string to repent the methods name and signature
        /// this method has to match output with one in Generate tools
        /// </summary>
        public static string GetMethodSignature(this MethodInfo methodInfo)
        {
            var parameters = methodInfo.GetParameters();
            var parameterDescriptions = parameters.Select(param => $"{GetGenericTypeName(param.ParameterType)} {param.Name}");
            return GetMethodSignature(methodInfo.Name, parameterDescriptions);
        }

        /// <summary>
        /// EXAMPLE: VedAstro.Library.Calculate.List<Avasta> PlanetAvasta(PlanetName planetName, Time time)
        /// </summary>
        public static string GetMethodSignature(this MethodDeclarationSyntax methodDeclaration, SemanticModel semanticModel)
        {
            var parameters = methodDeclaration.ParameterList.Parameters;
            var parameterDescriptions = parameters.Select(param => $"{param.Type.ToString()} {param.Identifier.Text}");
            return GetMethodSignature(methodDeclaration.Identifier.Text, parameterDescriptions);
        }


        /// <summary>
        /// Main purpose is to get unified method signature from Code and Reflection
        /// </summary>
        public static string GetMethodSignature(string methodName, IEnumerable<string> parameterDescriptions)
        {
            //unify type names, one is coming from Reflection and another direct from C# file
            var listString = parameterDescriptions.Select(p => p.Replace("Int32", "int"));
            listString = listString.Select(p => p.Replace("Double", "double"));
            listString = listString.Select(p => p.Replace("String", "string"));
            listString = listString.Select(p => p.Replace("Boolean", "bool"));

            var methodSignature = $"{methodName}({string.Join(", ", listString)})";

            //remove all space, else won't match
            methodSignature = methodSignature.Replace(" ", "");

            return methodSignature;
        }

        public static string GetGenericTypeName(Type type)
        {
            if (!type.IsGenericType)
                return type.Name;
            var genericTypeName = type.GetGenericTypeDefinition().Name;
            // Remove the `1 at the end of the name
            genericTypeName = genericTypeName.Substring(0, genericTypeName.IndexOf('`'));
            var genericArgs = string.Join(",", type.GetGenericArguments().Select(t => t.Name).ToArray());
            var finalReturn = genericTypeName + "<" + genericArgs + ">";

            return finalReturn;
        }

        /// <summary>
        /// Searches text no caps and no space
        /// </summary>
        public static bool SearchText(this string text, string keyword)
        {
            // Remove spaces from the text and the keyword
            string textWithoutSpaces = text.Replace(" ", "");
            string keywordWithoutSpaces = keyword.Replace(" ", "");
            return textWithoutSpaces.Contains(keywordWithoutSpaces, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets name, params and return type as string from method info
        /// </summary>
        public static string GetAllDataAsText(this MethodInfo methodInfo)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{methodInfo.Name}");
            sb.AppendLine($"{methodInfo.ReturnType.Name}");
            ParameterInfo[] parameters = methodInfo.GetParameters();
            foreach (ParameterInfo parameter in parameters)
            {
                sb.AppendLine($"\t{parameter.ParameterType.Name} {parameter.Name}");
            }

            var allDataAsText = sb.ToString();

            return allDataAsText;
        }

        /// <summary>
        /// gets all sample params needed to invoke method
        /// </summary>
        public static List<object> GetInitializedSampleParameters(this MethodInfo methodInfo)
        {
            List<object> parameters = new List<object>();

            foreach (ParameterInfo parameter in methodInfo.GetParameters())
            {
                // Get the underlying type of the parameter
                var parameterType = parameter.ParameterType;

                //final sample initialized data
                object sampleData = null;

                //HACK to handle custom types, since can't extend class
                if (parameterType == typeof(object)) { sampleData = new object(); }
                if (parameterType == typeof(ZodiacSign)) { sampleData = new ZodiacSign(ZodiacName.Aquarius, Angle.FromDegrees(15)); }
                if (parameterType == typeof(Time)) { sampleData = Time.StandardHoroscope(); }
                if (parameterType == typeof(Angle)) { sampleData = Angle.Degrees180; }
                if (parameterType == typeof(PlanetName)) { sampleData = PlanetName.Sun; }
                if (parameterType == typeof(PlanetConstellation)) { sampleData = new PlanetConstellation(1, 1, Angle.FromDegrees(13)); }
                if (parameterType == typeof(Person)) { sampleData = new Person("", "Juliet", Time.StandardHoroscope(), Gender.Female, new[] { "" }); }
                if (parameterType == typeof(HouseName)) { sampleData = HouseName.House4; }
                if (parameterType == typeof(TimeSpan)) { sampleData = new TimeSpan(1, 0, 0); }
                if (parameterType == typeof(List<HouseName>)) { sampleData = new List<HouseName>() { HouseName.House1, HouseName.House4 }; }
                if (parameterType == typeof(List<PlanetName>)) { sampleData = new List<PlanetName>() { PlanetName.Moon, PlanetName.Mars }; }
                if (parameterType == typeof(int)) { sampleData = 5; }
                if (parameterType == typeof(double)) { sampleData = 2415018.5; } //julian days
                if (parameterType == typeof(string)) { sampleData = "sun"; }
                if (parameterType == typeof(bool)) { sampleData = false; }
                if (parameterType == typeof(DateTimeOffset)) { sampleData = DateTimeOffset.Now; }
                if (parameterType == typeof(PlanetName[])) { sampleData = new[] { PlanetName.Mars, PlanetName.Jupiter }; }
                if (parameterType == typeof(int[])) { sampleData = new[] { 3, 5 }; }
                if (parameterType == typeof(Dictionary<PlanetName, Shashtiamsa>))
                {
                    sampleData = new Dictionary<PlanetName, Shashtiamsa>()
                    {
                        { PlanetName.Sun, new Shashtiamsa(103.244) },
                        { PlanetName.Moon, new Shashtiamsa(195.338) },
                        { PlanetName.Mars, new Shashtiamsa(28.665) },
                        { PlanetName.Mercury, new Shashtiamsa(191.879) },
                        { PlanetName.Jupiter, new Shashtiamsa(210.36) },
                        { PlanetName.Venus, new Shashtiamsa(117.7177) },
                        { PlanetName.Saturn, new Shashtiamsa(114.849) },
                    };
                }

                //if not found then probably Enum, so use special Enum converter
                if (sampleData == null) { sampleData = Tools.GetRandomEnumValue(parameterType); }

                parameters.Add(sampleData);
            }

            return parameters;

        }


        /// <summary>
        /// Gets input params of methods nicely formatted string for display
        /// </summary>
        public static List<string> GetParametersStringList(this MethodInfo methodInfo)
        {
            List<string> parameters = new List<string>();
            foreach (ParameterInfo parameter in methodInfo.GetParameters())
            {
                parameters.Add($"{parameter.ParameterType.Name}");
            }
            return parameters;
        }

        /// <summary>
        /// Gets calculators by param type and count
        /// Gets all calculated data in nice JSON with matching param signature
        /// used to create a dynamic API call list
        /// </summary>
        public static JObject ExecuteCalculatorByParam<T1, T2>(T1 inputedPram1, T2 inputedPram2)
        {
            //get reference to all the calculators that can be used with the inputed param types
            var finalList = GetCalculatorListByParam<T1, T2>();

            //sort alphabetically so easier to eye data point
            var aToZOrder = finalList.OrderBy(method => method.Name).ToList();


            //place the data from all possible methods nicely in JSON
            var rootPayloadJson = new JObject(); //each call below adds to this root
            object[] paramList = new object[] { inputedPram1, inputedPram2 };
            foreach (var methodInfo in aToZOrder)
            {
                var resultParse1 = ExecuteAPICalculator(methodInfo, paramList);
                //done to get JSON formatting right
                var resultParse2 = JToken.FromObject(resultParse1); //jprop needs to be wrapped in JToken
                rootPayloadJson.Add(resultParse2);
            }

            return rootPayloadJson;

        }

        public static JObject ExecuteCalculatorByParam<T1>(T1 inputedPram1)
        {
            //get reference to all the calculators that can be used with the inputed param types
            var finalList = GetCalculatorListByParam<T1>();

            //sort alphabetically so easier to eye data point
            var aToZOrder = finalList.OrderBy(method => method.Name).ToList();


            //place the data from all possible methods nicely in JSON
            var rootPayloadJson = new JObject(); //each call below adds to this root
            object[] paramList = new object[] { inputedPram1 };
            foreach (var methodInfo in aToZOrder)
            {
                var resultParse1 = ExecuteAPICalculator(methodInfo, paramList);
                //done to get JSON formatting right
                var resultParse2 = JToken.FromObject(resultParse1); //jprop needs to be wrapped in JToken
                rootPayloadJson.Add(resultParse2);
            }

            return rootPayloadJson;

        }


        /// <summary>
        /// Given an API name, will find the calc and try to call and wrap it in JSON
        /// </summary>
        public static JProperty ExecuteCalculatorByApiName<T1, T2>(string methodName, T1 param1, T2 param2)
        {
            var calculatorClass = typeof(Calculate);
            var foundMethod = calculatorClass.GetMethods().Where(x => x.Name == methodName).FirstOrDefault();

            //if method not found, possible outdated API call link, end call here
            if (foundMethod == null)
            {
                //let caller know that method not found
                var msg = $"Call not found, make sure API link is latest version : {methodName} ";
                return new JProperty(methodName, $"ERROR:{msg}");
            }

            //pass to main function
            return ExecuteCalculatorByApiName(foundMethod, param1, param2);
        }

        /// <summary>
        /// Given an API name, will find the calc and try to call and wrap it in JSON
        /// </summary>
        public static JProperty ExecuteCalculatorByApiName<T1, T2>(MethodInfo foundMethod, T1 param1, T2 param2)
        {
            //get methods 1st param
            var param1Type = foundMethod.GetParameters()[0].ParameterType;
            object[] paramOrder1 = new object[] { param1, param2 };
            object[] paramOrder2 = new object[] { param2, param1 };

            //if first param match type, then use that
            var finalParamOrder = param1Type == param1.GetType() ? paramOrder1 : paramOrder2;

#if DEBUG
            //print out which order is used more, helps to clean code
            Console.WriteLine(param1Type == param1.GetType() ? "paramOrder1" : "paramOrder2");
#endif

            //based on what type it is we process accordingly, converts better to JSON
            var rawResult = foundMethod?.Invoke(null, finalParamOrder);

            //get correct name for this method, API friendly
            var apiSpecialName = foundMethod.Name;

            //process list differently
            JProperty rootPayloadJson;
            if (rawResult is IList iList) //handles results that have many props from 1 call, exp : SwissEphemeris
            {
                //convert list to comma separated string
                var parsedList = iList.Cast<object>().ToList();
                var stringComma = Tools.ListToString(parsedList);

                rootPayloadJson = new JProperty(apiSpecialName, stringComma);
            }
            //custom JSON converter available
            else if (rawResult is IToJson iToJson)
            {
                rootPayloadJson = new JProperty(apiSpecialName, iToJson.ToJson());
            }
            //normal conversion via to string
            else
            {
                rootPayloadJson = new JProperty(apiSpecialName, rawResult?.ToString());
            }


            return rootPayloadJson;
        }

        public static JProperty ExecuteCalculatorByApiName<T1>(MethodInfo foundMethod, T1 param1)
        {
            //get methods 1st param
            var param1Type = foundMethod.GetParameters()[0].ParameterType;
            object[] paramOrder1 = new object[] { param1 };


            //based on what type it is we process accordingly, converts better to JSON
            var rawResult = foundMethod?.Invoke(null, paramOrder1);

            //get correct name for this method, API friendly
            var apiSpecialName = foundMethod.Name;

            //process list differently
            JProperty rootPayloadJson;
            if (rawResult is IList iList) //handles results that have many props from 1 call, exp : SwissEphemeris
            {
                //convert list to comma separated string
                var parsedList = iList.Cast<object>().ToList();
                var stringComma = Tools.ListToString(parsedList);

                rootPayloadJson = new JProperty(apiSpecialName, stringComma);
            }
            //custom JSON converter available
            else if (rawResult is IToJson iToJson)
            {
                rootPayloadJson = new JProperty(apiSpecialName, iToJson.ToJson());
            }
            //normal conversion via to string
            else
            {
                rootPayloadJson = new JProperty(apiSpecialName, rawResult?.ToString());
            }


            return rootPayloadJson;
        }

        /// <summary>
        /// Executes all calculators for API based on input param type only
        /// Wraps return data in JSON
        /// </summary>
        public static JProperty ExecuteAPICalculator(MethodInfo methodInfo1, object[] param)
        {

            //likely to fail during call, as such just ignore and move along
            try
            {
                JProperty outputResult;
                //execute based on param count
                if (param.Length == 1)
                {
                    outputResult = ExecuteCalculatorByApiName(methodInfo1, param[0]);
                }
                else if (param.Length == 2)
                {
                    outputResult = ExecuteCalculatorByApiName(methodInfo1, param[0], param[1]);
                }
                else
                {
                    //if not filled than not accounted for
                    throw new Exception("END OF THE LINE!");
                }


                return outputResult;
            }
            catch (Exception e)
            {
                try
                {
#if DEBUG
                    Console.WriteLine($"Trying again in reverse! {methodInfo1.Name}:\n{e.Message}\n{e.StackTrace}");
#endif
                    //try again in reverse
                    if (param.Length == 2)
                    {
                        var outputResult3 = ExecuteCalculatorByApiName(methodInfo1, param[1], param[0]);
                        return outputResult3;
                    }

                    var jsonPacked = new JProperty(methodInfo1.Name, $"ERROR: {e.Message}");
                    return jsonPacked;

                }
                //if fail put error in data for easy detection
                catch (Exception e2)
                {
                    //save it nicely in json format
                    var jsonPacked = new JProperty(methodInfo1.Name, $"ERROR: {e2.Message}");
                    return jsonPacked;
                }
            }
        }

        /// <summary>
        /// Gets all methods in Astronomical calculator that has the pram types inputed
        /// Note : also gets when order is reversed
        /// </summary>
        public static IEnumerable<MethodInfo> GetCalculatorListByParam<T1, T2>()
        {
            var inputedParamType1 = typeof(T1);
            var inputedParamType2 = typeof(T2);

            //get all calculators that can work with the inputed data
            var calculatorClass = typeof(Calculate);

            var finalList = new List<MethodInfo>();

            var calculators1 = from calculatorInfo in calculatorClass.GetMethods()
                               let parameter = calculatorInfo.GetParameters()
                               where parameter.Length == 2 //only 2 params
                                     && parameter[0].ParameterType == inputedParamType1
                                     && parameter[1].ParameterType == inputedParamType2
                               select calculatorInfo;

            finalList.AddRange(calculators1);

            //reverse order
            //second possible order, technically should be aligned todo
            var calculators2 = from calculatorInfo in calculatorClass.GetMethods()
                               let parameter = calculatorInfo.GetParameters()
                               where parameter.Length == 2 //only 2 params
                                     && parameter[0].ParameterType == inputedParamType2
                                     && parameter[1].ParameterType == inputedParamType1
                               select calculatorInfo;

            finalList.AddRange(calculators2);

#if true
            //PRINT DEBUG DATA
            Console.WriteLine($"Calculators Type 1 : {calculators1?.Count()}");
            Console.WriteLine($"Calculators Type 2 : {calculators2?.Count()}");
#endif

            return finalList;
        }


        public static IEnumerable<MethodInfo> GetCalculatorListByParam<T1>()
        {
            var inputedParamType1 = typeof(T1);

            //get all calculators that can work with the inputed data
            var calculatorClass = typeof(Calculate);

            var finalList = new List<MethodInfo>();

            var calculators1 = from calculatorInfo in calculatorClass.GetMethods()
                               let parameter = calculatorInfo.GetParameters()
                               where parameter.Length == 1 //only 2 params
                                     && parameter[0].ParameterType == inputedParamType1
                               select calculatorInfo;

            finalList.AddRange(calculators1);


#if true
            //PRINT DEBUG DATA
            Console.WriteLine($"Calculators with 1 param : {calculators1?.Count()}");
#endif

            return finalList;
        }

        /// <summary>
        /// Gets all possible API calculators from code method info
        /// used to make list to show user
        /// </summary>
        public static List<MethodInfo> GetAllApiCalculatorsMethodInfo()
        {

            //get all calculators that can work with the inputed data
            var calculatorClass = typeof(Calculate);

            //fine tune, what methods gets set as calculators
            //remove auto properties methods and base methods
            var finalList = calculatorClass.GetMethods()
                .Where(m => !m.Name.StartsWith("get_") && !m.Name.StartsWith("set_") && m.DeclaringType != typeof(object))
                .ToList();
            return finalList;

        }

        /// <summary>
        /// Given a type will convert to json
        /// used for parsing results from all OPEN API calcs
        /// </summary>
        public static JToken AnyToJSON(string dataName, dynamic anyTypeData)
        {
            //process list differently
            JProperty rootPayloadJson;
            switch (anyTypeData)
            {
                //handles results that have many props from 1 call, exp : SwissEphemeris
                case List<APIFunctionResult> apiList:
                    {
                        //converts into JSON list with property names
                        //NOTE: uses back this AnyToJSON to convert nested types
                        var parsed = APIFunctionResult.ToJsonList(apiList);
                        return parsed;
                    }
                //handles results that have many props from 1 call, exp : SwissEphemeris
                case IList iList:
                    {
                        //convert list to comma separated string
                        var parsedList = iList.Cast<object>().ToList();
                        var stringComma = Tools.ListToString(parsedList);

                        rootPayloadJson = new JProperty(dataName, stringComma);
                        break;
                    }
                //handles results that have many props from 1 call, exp : SwissEphemeris
                case Dictionary<PlanetName, ZodiacSign> dictionary:
                    {
                        //convert list to comma separated string
                        var parsedList = dictionary.Cast<object>().ToList();
                        var stringComma = Tools.ListToString(parsedList);

                        rootPayloadJson = new JProperty(dataName, stringComma);
                        break;
                    }
                case Dictionary<ZodiacName, int> dictionary:
                    {
                        //convert list to comma separated string
                        var parsedList = dictionary.Cast<object>().ToList();
                        var stringComma = Tools.ListToString(parsedList);

                        rootPayloadJson = new JProperty(dataName, stringComma);
                        break;
                    }
                case Dictionary<PlanetName, PlanetConstellation> dictionary:
                    {
                        //convert list to comma separated string
                        var parsedList = dictionary.Cast<object>().ToList();
                        var stringComma = Tools.ListToString(parsedList);

                        rootPayloadJson = new JProperty(dataName, stringComma);
                        break;
                    }
                //custom JSON converter available
                case IToJson iToJson:
                    rootPayloadJson = new JProperty(dataName, iToJson.ToJson());
                    break;
                //normal conversion via "ToString"
                default:
                    rootPayloadJson = new JProperty(dataName, anyTypeData?.ToString());
                    break;
            }

            return rootPayloadJson;

        }

        /// <summary>
        /// Given any type tries best to convert to string
        /// note: used in ML Table Generator
        /// </summary>
        public static string AnyToString(object result)
        {
            // Use StringBuilder for efficient string concatenation
            var sb = new StringBuilder();
            switch (result)
            {
                // Use 'is' for pattern matching
                case IList iList:
                    sb.Append(string.Join(", ", iList.Cast<object>()));
                    break;
                case Enum enumResult:
                    sb.Append(enumResult.ToString());
                    break;
                case Dictionary<PlanetName, ZodiacName> dictPZodiacName:
                    AppendDictionary(sb, dictPZodiacName);
                    break;
                case Dictionary<PlanetName, ZodiacSign> dictPZodiacSign:
                    AppendDictionary(sb, dictPZodiacSign);
                    break;
                case Dictionary<PlanetName, PlanetConstellation> dictPConstellation:
                    AppendDictionary(sb, dictPConstellation);
                    break;
                case Dictionary<PlanetName, Dictionary<ZodiacName, int>> dictPDZ:
                    {
                        foreach (var kv in dictPDZ)
                        {
                            sb.Append($"{kv.Key}: {AnyToString(kv.Value)}, ");
                        }

                        break;
                    }
                case Dictionary<ZodiacName, int> dictZI:
                    AppendDictionary(sb, dictZI);
                    break;
                case Dictionary<PlanetName, double> dictPD:
                    AppendDictionary(sb, dictPD);
                    break;
                default:
                    sb.Append(result.ToString());
                    break;
            }
            return sb.ToString();
        }
        private static void AppendDictionary<TKey, TValue>(StringBuilder sb, Dictionary<TKey, TValue> dict)
        {
            foreach (var kv in dict)
            {
                sb.Append($"{kv.Key}: {kv.Value}, ");
            }
        }


        /// <summary>
        /// QuoteValue method takes care of escaping double quotes and enclosing values in double quotes.
        /// This should handle cases where property values contain commas, newlines, or double quotes.
        /// If a value contains a double quote, you can escape it by doubling it. 
        /// </summary>
        public static string QuoteValue(object value)
        {
            var stringValue = value?.ToString() ?? string.Empty;
            stringValue = stringValue.Replace("\"", "\"\""); // escape double quotes
            return $"\"{stringValue}\""; // enclose in double quotes
        }

        public static string StringToMimeType(string fileFormat)
        {
            switch (fileFormat.ToLower())
            {
                case "pdf": return MediaTypeNames.Application.Pdf;
                case "xml": return MediaTypeNames.Application.Xml;
                case "gif": return MediaTypeNames.Image.Gif;
                case "jpeg": return MediaTypeNames.Image.Jpeg;
                case "jpg": return MediaTypeNames.Image.Jpeg;
                case "tiff": return MediaTypeNames.Image.Tiff;
            }

            throw new Exception("END OF LINE");

        }

        /// <summary>
        /// Given a list of object will make into JSON
        /// </summary>
        public static JArray ListToJson<T>(List<T> itemList)
        {
            //get all as converted to basic string

            JArray arrayJson = new JArray();
            foreach (var item in itemList)
            {
                if (item is XElement personXml)
                {
                    var personJson = Tools.XmlToJson(personXml);
                    arrayJson.Add(personJson);
                }
                else if (item is IToJson toJson)
                {
                    arrayJson.Add(toJson.ToJson());
                }
                //do it normal string way
                else
                {
                    arrayJson.Add(item.ToString());
                }

            }

            return arrayJson;
        }

        /// <summary>
        /// Used for Person, Match Report, Chart and all things made by end user
        /// </summary>
        public static string[] GetUserIdFromData(XElement inputXml)
        {
            var userIdRaw = inputXml.Element("UserId")?.Value ?? "";
            //clean, remove white space & new line if any
            userIdRaw = userIdRaw.Replace("\n", "");
            userIdRaw = userIdRaw.Replace(" ", "");

            var userId = userIdRaw.Split(',');//split by comma

            return userId;
        }

        public static string[] GetUserIdFromData(JToken input)
        {
            var userIdRaw = input["UserId"].Value<string>();
            //clean, remove white space & new line if any
            userIdRaw = userIdRaw.Replace("\n", "");
            userIdRaw = userIdRaw.Replace(" ", "");

            var userId = userIdRaw.Split(',');//split by comma

            return userId;
        }

        /// <summary>
        /// Given a doc os records will find by user ID , owners
        /// used by to get stuff created by end user
        /// </summary>
        public static List<XElement> FindXmlByUserId(XDocument allListXmlDoc, string inputUserId)
        {
            var returnList = new List<XElement>();

            //add all  profiles that have the given user ID
            var allItems = allListXmlDoc.Root?.Elements();
            foreach (var itemXml in allItems)
            {
                var allOwnerId = itemXml.Element("UserId")?.Value ?? "";

                //check if inputed ID is found in list, add to return list
                var match = IsUserIdMatch(allOwnerId, inputUserId);
                if (match) { returnList.Add(itemXml); }
            }

            return returnList;
        }

        /// <summary>
        /// check if 2 user id strings match, can't just use contains since 101 can be anywhere
        /// split by comma, and check by direct equality lower case
        /// </summary>
        public static bool IsUserIdMatch(string userIdStringA, string userIdStringB)
        {

            //must be split before can be used
            var userListA = userIdStringA.Split(',');
            var userListB = userIdStringB.Split(',');

            foreach (var userIdA in userListA)
            {
                foreach (var userIdB in userListB)
                {
                    //check direct match with lower case todo maybe lower case not needed since user ID, not person ID
                    var match = userIdA.ToLower() == userIdB.ToLower();

                    //if found even 1 match then return
                    if (match) { return true; }
                }
            }

            //if control reaches here than confirm no match
            return false;
        }

        /// <summary>
        /// Check if inputed time was within last hour
        /// </summary>
        public static bool IsWithinLastHour(Time logItemTime, double hours)
        {
            //get time 1 hour ago
            var time1HourAgo = DateTimeOffset.Now.AddHours(hours);

            //check if inputed time is after this 1 ago mark
            var isAfter = logItemTime.GetStdDateTimeOffset() >= time1HourAgo;

            return isAfter;
        }


        /// <summary>
        /// sends a simple head request to check if file exists (low cost)
        /// </summary>
        public static async Task<bool> DoesFileExist(string url)
        {
            var client = new HttpClient();

            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Head, url))
            {
                using (HttpResponseMessage response = await client.SendAsync(request))
                {
                    return response.StatusCode == System.Net.HttpStatusCode.OK;
                }
            }
        }

        /// <summary>
        /// given a time zone will return famous cities using said timezone
        /// </summary>
        public static string TimeZoneToLocation(string timeZone)
        {
            return "Earth";
            //switch (timeZone)
            //{

            //}
        }


        /// <summary>
        /// No parsing direct from horses mouth
        /// </summary>
        public static async Task<T> ReadServerRaw<T>(string receiverAddress)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, receiverAddress);
            var waitForContent = HttpCompletionOption.ResponseContentRead;
            using var client = new HttpClient();
            client.Timeout = Timeout.InfiniteTimeSpan;
            var response = await client.SendAsync(httpRequestMessage, waitForContent);
            if (typeof(T) == typeof(byte[]))
            {
                var dataReturned = await response.Content.ReadAsByteArrayAsync();
                return (T)(object)dataReturned;
            }
            else
            {
                var dataReturned = await response.Content.ReadAsStringAsync();
                if (typeof(T) == typeof(string))
                {
                    return (T)(object)dataReturned;
                }
                else if (typeof(T) == typeof(JObject))
                {
                    return (T)(object)JObject.Parse(dataReturned);
                }
                else
                {
                    throw new InvalidOperationException("Unsupported type parameter");
                }
            }
        }


        /// <summary>
        /// makes HTTP call to address using .NET
        /// </summary>
        public static async Task<T> WriteServer<T, Z>(HttpMethod method, string receiverAddress, Z payload = default)
        {
            //prepare the data to be sent
            var httpRequestMessage = new HttpRequestMessage(method, receiverAddress);

            //tell sender to wait for complete reply before exiting
            var waitForContent = HttpCompletionOption.ResponseContentRead;

            //add in payload if specified
            if (payload != null)
            {
                if (payload is JToken payloadJson)
                {
                    httpRequestMessage.Content = JsontoHttpContent(payloadJson);
                }
                else if (payload is byte[] payloadBinary)
                {
                    httpRequestMessage.Content = new ByteArrayContent(payloadBinary);
                }
                else
                {
                    throw new ArgumentException("Payload must be either a JToken or a byte array.", nameof(payload));
                }
            }

            //send the data on its way (wait forever no timeout)
            using var client = new HttpClient();
            client.Timeout = Timeout.InfiniteTimeSpan;

            //send the data on its way
            var response = await client.SendAsync(httpRequestMessage, waitForContent);
            if (typeof(T) == typeof(byte[]))
            {
                var dataReturned = await response.Content.ReadAsByteArrayAsync();
                return (T)(object)dataReturned;
            }
            else
            {
                var dataReturned = await response.Content.ReadAsStringAsync();
                if (typeof(T) == typeof(string))
                {
                    return (T)(object)dataReturned;
                }
                else if (typeof(T) == typeof(JObject))
                {
                    //return data as JSON as expected from API 
                    return (T)(object)JObject.Parse(dataReturned);
                }
                else
                {
                    throw new InvalidOperationException("Unsupported type parameter");
                }
            }

        }

        /// <summary>
        /// Given a method name in string form, will get it's reference to code
        /// gets from Calculate.cs class
        /// </summary>
        public static MethodInfo MethodNameToMethodInfo(string methodName)
        {
            var calculatorClass = typeof(Calculate);
            var foundList = calculatorClass.GetMethods().Where(x => x.Name == methodName);
            var foundMethod = foundList.FirstOrDefault();

            //if more than 1 method found major internal error, crash it!
            if (foundList.Count() > 1)
            {
                Console.WriteLine($"POTENTIAL ERROR: Duplicate API Names : {methodName}");
            }

            return foundMethod;

        }


        /// <summary>
        /// Given a id will return parsed person from main list
        /// Returns empty person if, no person found
        /// </summary>
        public static async Task<Person> GetPersonById(string personId)
        {
            //get the raw data of person
            var foundPersonXml = await FindPersonXMLById(personId);

            if (foundPersonXml == null) { return Person.Empty; }

            var foundPerson = Person.FromXml(foundPersonXml);

            return foundPerson;
        }

        /// <summary>
        /// Gets person XML given ID direct from storage
        /// </summary>
        public static async Task<XElement?> FindPersonXMLById(string personIdToFind)
        {
            try
            {
                //get latest file from server
                //note how this creates & destroys per call to method
                //might cost little extra cycles but it's a functionality
                //to always get the latest list
                var personListXmlDoc = await GetPersonListFile();

                //list of person XMLs
                var personXmlList = personListXmlDoc?.Root?.Elements() ?? new List<XElement>();

                //do the finding (default empty)
                var foundPerson = personXmlList?.Where(MatchPersonId)?.First();

                //log it (should not occur all the time)
                if (foundPerson == null)
                {
                    await LibLogger.Error($"No person found with ID : {personIdToFind}");
                    //return empty value so caller will know
                    foundPerson = null;
                }

                return foundPerson;
            }
            catch (Exception e)
            {
                //if fail log it and return empty value so caller will know
                await LibLogger.Error(e);
                return null;
            }

            //--------
            //do the finding, for id both case should match, but stored in upper case because looks nice
            //but user might pump in with mixed case, who knows, so compensate.
            bool MatchPersonId(XElement personXml)
            {
                if (personXml == null) { return false; }

                var inputPersonId = personXml?.Element("PersonId")?.Value ?? ""; //todo PersonId has to be just Id

                //lower case it before checking
                var isMatch = inputPersonId == personIdToFind; //hoisting alert

                return isMatch;
            }
        }

        /// <summary>
        /// Reference version of above method
        /// used in scripts
        /// </summary>
        public static XElement FindPersonXMLById(string personIdToFind, ref XDocument personListXmlDoc)
        {
            try
            {
                //list of person XMLs
                var personXmlList = personListXmlDoc?.Root?.Elements() ?? new List<XElement>();

                //do the finding (default empty)
                var foundPerson = personXmlList?.Where(MatchPersonId)?.First();

                //log it (should not occur all the time)
                if (foundPerson == null)
                {
                    //return empty value so caller will know
                    foundPerson = null;
                }

                return foundPerson;
            }
            catch (Exception e)
            {
                //if fail log it and return empty value so caller will know
                return null;
            }

            //--------
            //do the finding, for id both case should match, but stored in upper case because looks nice
            //but user might pump in with mixed case, who knows, so compensate.
            bool MatchPersonId(XElement personXml)
            {
                if (personXml == null) { return false; }

                var inputPersonId = personXml?.Element("PersonId")?.Value ?? ""; //todo PersonId has to be just Id

                //lower case it before checking
                var isMatch = inputPersonId == personIdToFind; //hoisting alert

                return isMatch;
            }
        }


        public const string BlobContainerName = "vedastro-site-data";

        public const string PersonListFile = "PersonList.xml";

        /// <summary>
        /// Gets main person list xml doc file
        /// </summary>
        /// <returns></returns>
        public static async Task<XDocument> GetPersonListFile()
        {
            var personListXml = await GetXmlFileFromAzureStorage(PersonListFile, BlobContainerName);

            return personListXml;
        }

        /// <summary>
        /// Gets XML file from Azure blob storage
        /// </summary>
        public static async Task<XDocument> GetXmlFileFromAzureStorage(string fileName, string blobContainerName)
        {
            var fileClient = await GetBlobClientAzure(fileName, blobContainerName);
            var xmlFile = await DownloadToXDoc(fileClient);

            return xmlFile;
        }

        /// <summary>
        /// Converts a blob client of a file to an XML document
        /// </summary>
        public static async Task<XDocument> DownloadToXDoc(BlobClient blobClient)
        {
            var isFileExist = (await blobClient.ExistsAsync()).Value;

            if (isFileExist)
            {
                XDocument xDoc;
                await using (var stream = (await blobClient.DownloadStreamingAsync()).Value.Content)
                {
                    xDoc = await XDocument.LoadAsync(stream, LoadOptions.None, CancellationToken.None);
                }

#if DEBUG
                Console.WriteLine($"Downloaded: {blobClient.Name}");
#endif

                return xDoc;
            }
            else
            {
                //will be logged by caller
                throw new Exception($"No File in Cloud! : {blobClient.Name}");
            }

        }


        /// <summary>
        /// Gets file blob client from azure storage by name
        /// </summary>
        public static async Task<BlobClient> GetBlobClientAzure(string fileName, string blobContainerName)
        {
            //get the connection string stored separately (for security reasons)
            //note: dark art secrets are in local.settings.json
            var storageConnectionString = Secrets.API_STORAGE;

            //get image from storage
            var blobContainerClient = new BlobContainerClient(storageConnectionString, blobContainerName);
            var fileBlobClient = blobContainerClient.GetBlobClient(fileName);

            return fileBlobClient;

            //var returnStream = new MemoryStream();
            //await fileBlobClient.DownloadToAsync(returnStream);

            //return returnStream;
        }

        /// <summary>
        /// INPUT:
        /// /Singapore/Time/23:59/31/12/2000/+08:00/Planet/Sun/Sign/
        /// OUTPUT:
        /// "/Singapore/Time/23:59/"
        /// NOTE:
        /// In this example, if cutCount is 3, the CutString method will return
        /// the first 3 substrings ("Singapore", "Time", "23:59")
        /// from the input string. The result will be "/Singapore/Time/23:59/".
        /// </summary>
        public static string CutOutString(string input, int cutCount)
        {
            var parts = input.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var firstParts = parts.Take(cutCount);
            return "/" + string.Join("/", firstParts) + "/";
        }

        /// <summary>
        /// removes the what is within count, returns rest
        /// </summary>
        public static string CutRemoveString(string input, int cutCount)
        {
            var parts = input.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var remainingParts = parts.Skip(cutCount);
            return "/" + string.Join("/", remainingParts) + "/";
        }


        /// <summary>
        /// This algorithm calculates the minimum number of single-character edits (insertions, deletions, or substitutions) required to change one word into the other. It's a good measure of how dissimilar two strings are.
        /// </summary>
        public static int LevenshteinDistance(string a, string b)
        {
            var matrix = new int[a.Length + 1, b.Length + 1];
            for (int i = 0; i <= a.Length; i++)
                matrix[i, 0] = i;
            for (int j = 0; j <= b.Length; j++)
                matrix[0, j] = j;
            for (int i = 1; i <= a.Length; i++)
            {
                for (int j = 1; j <= b.Length; j++)
                {
                    int cost = (a[i - 1] == b[j - 1]) ? 0 : 1;
                    matrix[i, j] = Math.Min(Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1), matrix[i - 1, j - 1] + cost);
                }
            }
            return matrix[a.Length, b.Length];
        }


        /// <summary>
        /// Given a MethodInfo will generate Python method stub declaration code (Made by AI in 30s)
        /// EXP: def HousePlanetIsIn(time: Time, planet_name: PlanetName) -> HouseName: 
        /// </summary>
        public static string GeneratePythonDef(MethodInfo methodInfo)
        {
            var sb = new StringBuilder();
            // Get the method name
            string methodName = methodInfo.Name;
            sb.Append("def ");
            sb.Append(methodName);
            sb.Append("(");
            // Get the parameter types and names
            var parameters = methodInfo.GetParameters();
            if (parameters.Length == 0)
            {
                sb.Append("cls");
            }
            else
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    var param = parameters[i];
                    sb.Append(param.Name);
                    sb.Append(": ");
                    //meaning it is a Task, List or Dictionary
                    var cleanedTypeName = param.ParameterType.Name.Contains("`") ? "Any" : param.ParameterType.Name;
                    cleanedTypeName = cleanedTypeName.Replace("[", "").Replace("]", ""); // Remove square brackets
                    sb.Append(cleanedTypeName);
                    if (i < parameters.Length - 1)
                    {
                        sb.Append(", ");
                    }
                }
            }
            sb.Append(")");
            // Get the return type
            var returnType = methodInfo.ReturnType;
            if (returnType.Name.Contains("`")) //meaning it is a Task, List or Dictionary
            {
                sb.Append(" -> Any");
            }
            else
            {
                sb.Append(" -> ");
                var cleanedReturnTypeName = returnType.Name.Replace("[", "").Replace("]", ""); // Remove square brackets
                sb.Append(cleanedReturnTypeName);
            }
            sb.Append(":");
            return sb.ToString();
        }

        /// <summary>
        /// Given a meta data will give name params stacked
        /// exp : IsPlanetBenefic_Sun
        /// </summary>
        public static string GetSpecialMLTableName(dynamic openApiMetadata, object resultOverride = null)
        {
            if (openApiMetadata.SelectedParams == null)
            {
                //when using methods like All, need to dig out column name
                return $"{openApiMetadata.Name}_{resultOverride?.ToString() ?? "PLEASE INJECT VALUE"}";
            }
            //stack the param values next to each other exp: Sun_House1
            var paramCombined = "";
            foreach (var selectedParam in openApiMetadata.SelectedParams)
            {
                //if time no need to add into column name, since its in the row
                if (selectedParam is Time)
                {
                    continue;
                }
                else if (selectedParam is IList ccc)
                {
                    foreach (object xxx in ccc)
                    {
                        if (xxx is Time)
                        {
                            continue;
                        }
                        var strData = Tools.AnyToString(xxx);
                        paramCombined += "_" + strData;

                    }
                }
                else
                {
                    var strData = Tools.AnyToString(selectedParam);
                    paramCombined += "_" + strData;

                }


            }

            return $"{openApiMetadata.Name}{paramCombined}";
        }

        public static dynamic ephemeris_swe_calc(Time time, int swissPlanet)
        {
            //Converts LMT to UTC (GMT)
            int iflag = 2;//SwissEph.SEFLG_SWIEPH;  //+ SwissEph.SEFLG_SPEED;
            double[] results = new double[6];
            string err_msg = "";
            double jul_day_ET;
            SwissEph ephemeris = new SwissEph();

            // Convert DOB to ET
            jul_day_ET = Calculate.TimeToEphemerisTime(time);

            //Get planet long
            int ret_flag = ephemeris.swe_calc(jul_day_ET, swissPlanet, iflag, results, ref err_msg);

            //data in results at index 0 is longitude
            var sweCalcResults = new
            {
                Longitude = results[0],
                Latitude = results[1],
                DistanceAU = results[2],
                SpeedLongitude = results[3],
                SpeedLatitude = results[4],
                SpeedDistance = results[5]
            };

            return sweCalcResults;
        }

    }


}
