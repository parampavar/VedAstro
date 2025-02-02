
using Newtonsoft.Json.Linq;
using SwissEphNet;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static VedAstro.Library.PlanetName;
using Exception = System.Exception;
using System.Text;
using ExCSS;
using ScottPlot.Drawing.Colormaps;
using Azure;
// Note: The Azure OpenAI client library for .NET is in preview.
// Install the .NET library via NuGet: dotnet add package Azure.AI.OpenAI --version 1.0.0-beta.5
using Azure;
using Azure.AI.OpenAI;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Web;
using System.IO;
using MimeDetective.Storage.Xml.v2;
using System.Net;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using System.Net.Mime;
using Microsoft.Extensions.Hosting;
using System.Xml.Linq;
using Microsoft.Bing.ImageSearch.Models;

namespace VedAstro.Library
{

    //█▀▄▀█ █▀▀ ▀▀█▀▀ █░░█ █▀▀█ █▀▀▄ 　 ▀▀█▀▀ █▀▀█ 　 ▀▀█▀▀ █░░█ █▀▀ 　 █▀▄▀█ █▀▀█ █▀▀▄ █▀▀▄ █▀▀ █▀▀ █▀▀ 
    //█░▀░█ █▀▀ ░░█░░ █▀▀█ █░░█ █░░█ 　 ░░█░░ █░░█ 　 ░░█░░ █▀▀█ █▀▀ 　 █░▀░█ █▄▄█ █░░█ █░░█ █▀▀ ▀▀█ ▀▀█ 
    //▀░░░▀ ▀▀▀ ░░▀░░ ▀░░▀ ▀▀▀▀ ▀▀▀░ 　 ░░▀░░ ▀▀▀▀ 　 ░░▀░░ ▀░░▀ ▀▀▀ 　 ▀░░░▀ ▀░░▀ ▀▀▀░ ▀░░▀ ▀▀▀ ▀▀▀ ▀▀▀ 

    //█▀▀█ █▀▀█ █▀▀▄ █▀▀ █▀▀█ 　 ▀▀█▀▀ █▀▀█ 　 ▀▀█▀▀ █░░█ █▀▀ 　 █▀▀ █░░█ █▀▀█ █▀▀█ █▀▀ 
    //█░░█ █▄▄▀ █░░█ █▀▀ █▄▄▀ 　 ░░█░░ █░░█ 　 ░░█░░ █▀▀█ █▀▀ 　 █░░ █▀▀█ █▄▄█ █░░█ ▀▀█ 
    //▀▀▀▀ ▀░▀▀ ▀▀▀░ ▀▀▀ ▀░▀▀ 　 ░░▀░░ ▀▀▀▀ 　 ░░▀░░ ▀░░▀ ▀▀▀ 　 ▀▀▀ ▀░░▀ ▀░░▀ ▀▀▀▀ ▀▀▀

    /// <summary>
    /// Collection of astronomical calculator functions
    /// Note : Many of the functions here use cacheing machanism
    /// </summary>
    public partial class Calculate
    {

        #region SETTINGS


        /// <summary>
        /// Defaults to RAMAN, but can be set before calling any funcs,
        /// NOTE: remember not to change mid instance, because "GetAyanamsa" & others are cached per instance
        /// </summary>
        public static int Ayanamsa { get; set; } = (int)Library.Ayanamsa.LAHIRI;

        /// <summary>
        /// Number of days in a year. Used for dasa related calculations.
        /// much debate on this number. Tests prove Raman's 360 is accurate.
        /// 365.25 is used by 3rd party astrology software like LoKPA
        /// default to 365.25 for the sake of DEAR CP JOIS 🙈
        /// 365.2564 True Sidereal Solar Year
        /// </summary>
        public static double SolarYearTimeSpan { get; set; } = 365.35;

        /// <summary>
        /// If set true, will not include gochara that was obstructed by "Vedhanka Point" calculation
        /// Enabled by default, recommend only disabled for research & debugging.
        /// Vedhanka needed for accuracy, recommended leave true
        /// </summary>
        public static bool UseVedhankaInGochara { get; set; } = true;

        /// <summary>
        /// Defaults to mean Rahu & Ketu positions for a more even value,
        /// set to false to use true node.
        /// Correlates to Swiss Ephemeris, SE_TRUE_NODE & SE_MEAN_NODE
        /// </summary>
        public static bool UseMeanRahuKetu { get; set; } = true;


        #endregion


        //----------------------------------------CORE CODE---------------------------------------------

        #region BIRTH TIME FINDER

        public static JObject FindBirthTimeByAnimal(Time possibleBirthTime, double precisionHours = 1)
        {
            //get list of possible birth time slice in the current birth day
            var timeSlices = Tools.GetTimeSlicesOnBirthDay(possibleBirthTime, 1);

            //get predictions for each slice and place in out going list  
            var compiledObj = new JObject();
            foreach (var timeSlice in timeSlices)
            {
                //get the animal prediction for possible birth time
                var newBirthConstellation = Calculate.MoonConstellation(timeSlice).GetConstellationName();
                var animal = Calculate.YoniKutaAnimalFromConstellation(newBirthConstellation);

                //nicely packed 📦
                var named = new JProperty(timeSlice.ToString(), animal.ToString());
                compiledObj.Add(named);
            }

            //send data back to caller
            return compiledObj;
        }

        public static JObject FindBirthTimeByRisingSign(Time possibleBirthTime, double precisionHours = 1)
        {
            //get list of possible birth time slice in the current birth day
            var timeSlices = Tools.GetTimeSlicesOnBirthDay(possibleBirthTime, 1);

            //get predictions for each slice and place in out going list  
            var compiledObj = new JObject();
            foreach (var timeSlice in timeSlices)
            {
                //get all predictions for person
                var allPredictions = Tools.GetHoroscopePrediction(timeSlice);
                //select only rising sign
                var risingSignPredict = allPredictions.Where(x => x.FormattedName.Contains("Rising")).FirstOrDefault();

                //nicely packed 📦
                var named = new JProperty(timeSlice.ToString(), risingSignPredict.ToString());
                compiledObj.Add(named);
            }

            //send data back to caller
            return compiledObj;
        }

        public static JObject FindBirthTimeHouseStrengthPerson(Time possibleBirthTime, double precisionHours = 1)
        {
            //get list of possible birth time slice in the current birth day
            var timeSlices = Tools.GetTimeSlicesOnBirthDay(possibleBirthTime, 1);

            //get predictions for each slice and place in out going list  
            var compiledObj = new JObject();
            foreach (var timeSlice in timeSlices)
            {
                //compile all house strengths into a nice presentable string
                var finalString = "";
                foreach (var house in House.AllHouses)
                {
                    //get house strength
                    var strength = Calculate.HouseStrength(house, timeSlice).ToDouble(2);

                    //add to compiled string
                    var thisHouse = $"{house} {strength},";
                    finalString += thisHouse;
                }

                //nicely packed with TIME next to variable data
                var named = new JProperty(timeSlice.ToString(), finalString);
                compiledObj.Add(named);
            }

            //send data back to caller
            return compiledObj;
        }


        #endregion



        #region MAINTAINANCE

        /// <summary>
        /// Special debug function
        /// </summary>
        public static string BouncBackInputPlanet(PlanetName planetName, Time time) => planetName.ToString();

        /// <summary>
        /// Basic bounce back data to confirm validity or ML table needs
        /// </summary>
        public static GeoLocation BouncBackInputGeoLocation(Time time) => time.GetGeoLocation();

        /// <summary>
        /// Basic bounce back data to confirm validity or ML table needs
        /// </summary>
        public static string BouncBackInputTime(Time time) => time.ToString();

        /// <summary>
        /// Returns list of all API calls for fun, why not
        /// </summary>
        /// <returns></returns>
        public static JArray ListAPICalls()
        {
            var allApiCalculatorsMethodInfo = Tools.GetAllApiCalculatorsMethodInfo();

            var returnList = new JArray();
            foreach (var openApiCalc in allApiCalculatorsMethodInfo)
            {
                //get special signature to find the correct description from list
                var signature = openApiCalc.GetMethodSignature();
                returnList.Add(signature);
            }

            return returnList;
        }

        #endregion

        #region GEO LOCATION


        /// <summary>
        /// Given an address will convert to it's geo location equivelant
        /// http://localhost:7071/api/Calculate/AddressToGeoLocation/Address/Gaithersburg
        /// </summary>
        /// <param name="address">can be any location name or coordinates like -3.9571599,103.8723379</param>
        /// <returns></returns>
        public static GeoLocation AddressToGeoLocation(string address)
        {
            //CACHE MECHANISM
            return CacheManager.GetCache<GeoLocation>(new CacheKey(nameof(AddressToGeoLocation), address), _AddressToGeoLocation);

            //UNDERLYING FUNCTION
            GeoLocation _AddressToGeoLocation()
            {
                //inject api key from parent
                var locationProvider = new LocationManager();

                //do calculation using API and cache inteligently
                var returnVal = locationProvider.AddressToGeoLocation(address).Result;

                return returnVal;
            }
        }

        public static async Task<List<GeoLocation>> SearchLocation(string address)
        {
            //CACHE MECHANISM
            return await CacheManager.GetCache(new CacheKey(nameof(SearchLocation), address), async () => await _SearchLocation(address));

            //UNDERLYING FUNCTION
            async Task<List<GeoLocation>> _SearchLocation(string address)
            {

                //return all searches with less than 2 chars as pre name typing search
                if (address.Length <= 2) { return new List<GeoLocation>(); }

                //inject api key from parent
                var locationProvider = new LocationManager();

                //do calculation using API and cache inteligently
                var returnVal = await locationProvider.SearchAddressToGeoLocation(address);

                return returnVal;
            }

        }

        /// <summary>
        /// Given coordinates will convert to it's geo location equivalent
        /// http://localhost:7071/api/Calculate/CoordinatesToGeoLocation/Latitude/35.6764/Longitude/139.6500
        /// </summary>
        public static async Task<GeoLocation> CoordinatesToGeoLocation(string latitude, string longitude)
        {
            //CACHE MECHANISM
            return await CacheManager.GetCache(new CacheKey(nameof(CoordinatesToGeoLocation), latitude, longitude), async () => await _CoordinatesToGeoLocation(latitude, longitude));

            //UNDERLYING FUNCTION
            async Task<GeoLocation> _CoordinatesToGeoLocation(string latitude, string longitude)
            {
                //inject api key from parent
                var locationProvider = new LocationManager();

                //do calculation using API and cache inteligently
                var returnVal = await locationProvider.CoordinatesToGeoLocation(latitude, longitude);

                return returnVal;
            }
        }

        /// <summary>
        /// Gets all timezone given a location, accounts for Daylight savings & historical changes
        /// Note : location name is not mandatory, it is there because location names can change, but coordinates are essential 
        /// ...../api/Calculate/GeoLocationToTimezone/Location/Tokyo, Japan/Coordinates/35.65,139.83/Time/14:02/09/11/1977/+00:00
        /// </summary>
        public static async Task<string> GeoLocationToTimezone(GeoLocation geoLocation, DateTimeOffset timeAtLocation)
        {
            //CACHE MECHANISM
            return await CacheManager.GetCache(new CacheKey(nameof(GeoLocationToTimezone), geoLocation, timeAtLocation), async () => await _GeoLocationToTimezone(geoLocation, timeAtLocation));

            //UNDERLYING FUNCTION
            async Task<string> _GeoLocationToTimezone(GeoLocation geoLocation, DateTimeOffset timeAtLocation)
            {
                //inject api key from parent
                var locationProvider = new LocationManager();

                //do calculation using API and cache inteligently
                var returnVal = await locationProvider.GeoLocationToTimezone(geoLocation, timeAtLocation);

                return returnVal;
            }

        }

        /// <summary>
        /// ...../api/Calculate/IpAddressToGeoLocation/IpAddress/180.89.33.89
        /// </summary>
        public static async Task<GeoLocation> IpAddressToGeoLocation(string ipAddress)
        {
            //CACHE MECHANISM
            return await CacheManager.GetCache(new CacheKey(nameof(IpAddressToGeoLocation), ipAddress), async () => await _IpAddressToGeoLocation(ipAddress));

            //UNDERLYING FUNCTION
            async Task<GeoLocation> _IpAddressToGeoLocation(string ipAddress)
            {

                //inject api key from parent
                var locationProvider = new LocationManager();

                //do calculation using API and cache inteligently
                var returnVal = await locationProvider.IpAddressToGeoLocation(ipAddress);

                return returnVal;
            }
        }


        #endregion

        #region EVENTS

        /// <summary>
        /// Gets all events occuring at given time. Basically a slice from "Events Chart"
        /// Can be used by LLM to interprate final prediction. Also known as Muhurtha
        /// </summary>
        /// <param name="birthTime">DOB of person involded in event</param>
        /// <param name="checkTime">time event will occur</param>
        /// <param name="eventTagList">tags to select events</param>
        public static List<Event> EventsAtTime(Time birthTime, Time checkTime, List<EventTag> eventTagList)
        {
            // TEMP hack to place time in Person (wrapped) 
            var johnDoe = new Person("", birthTime, Gender.Empty);

            var xx = EventManager.CalculateEvents(1, checkTime, checkTime, johnDoe, eventTagList);

            return xx;
        }

        public static List<Event> EventsAtRange(Time birthTime, Time startTime, Time endTime, List<EventTag> eventTagList, int precisionHours = 100)
        {
            // TEMP hack to place time in Person (wrapped) 
            var johnDoe = new Person("", birthTime, Gender.Empty);

            //do calculation (heavy computation)
            List<Event> eventList = EventManager.CalculateEvents(precisionHours,
                startTime,
                endTime,
                johnDoe,
                eventTagList);

            return eventList;
        }


        /// <summary>
        /// Given a birth time, current time and event name, gets the event data occuring at current time
        /// Easy way to check if Gochara is occuring at given time, with start and end time calculated
        /// Precision hard set to 1 hour TODO
        /// </summary>
        /// <param name="birthTime">birth time of native</param>
        /// <param name="checkTime">time to base calculation on</param>
        public static Event EventStartEndTime(Time birthTime, Time checkTime, EventName nameOfEvent)
        {
            //from event name, get full event data
            EventData eventData = EventDataListStatic.Rows.Where(x => x.Name == nameOfEvent).FirstOrDefault();

            //TODO should be changeable for fine events
            var precisionInHours = 1;

            //check if event is occuring
            //NOTE: hack to enter birth time with existing code
            var birthTimeWrapped = new Person("", birthTime, Gender.Male);
            var isOccuringAtCheckTime = EventManager.ConvertToEventSlice(checkTime, eventData, birthTimeWrapped)?.IsOccuring ?? false; //not found default to false

            //if occuring, start scanning for start & end times
            if (isOccuringAtCheckTime)
            {
                //scan for start time given event data
                var eventStartTime = Calculate.EventStartTime(birthTime, checkTime, eventData, precisionInHours);

                //scan for end time given event data
                var eventEndTime = Calculate.EventEndTime(birthTime, checkTime, eventData, precisionInHours);

                //TODO: temp
                var tags = EventManager.GetTagsByEventName(eventData.Name);
                var finalEvent = new Event(eventData.Name, eventData.Nature, eventData.Description, eventData.SpecializedSummary, eventStartTime, eventEndTime, tags);

                return finalEvent;
            }

            //if not occuring, let user know with empty event
            else { return Event.Empty; }

        }

        public static Time EventStartTime(Time birthTime, Time checkTime, EventData eventData, int precisionInHours)
        {
            //NOTE: hack to enter birth time with existing code
            var birthTimeWrapped = new Person("", birthTime, Gender.Male);

            //check time will be used as possible start time
            var possibleStartTime = checkTime;
            var previousPossibleStartTime = possibleStartTime;

            //start as not found
            var isFound = false;
            while (!isFound) //run while not found
            {
                //check possible start time if event occurring (heavy computation)
                var updatedEventData = EventManager.ConvertToEventSlice(possibleStartTime, eventData, birthTimeWrapped);

                //if occuring than continue to next, start time not found
                if (updatedEventData != null && updatedEventData.IsOccuring)
                {
                    //save a copy of possible time, to be used when we go too far
                    previousPossibleStartTime = possibleStartTime;

                    //decrement entered time, to check next possible start time in the past
                    possibleStartTime = possibleStartTime.SubtractHours(precisionInHours);
                }
                //start time found!, event has stopped occuring (too far)
                else
                {
                    //return possible start time as confirmed!
                    possibleStartTime = previousPossibleStartTime;
                    isFound = true; //stop looking
                }
            }

            //if control reaches here than start time found
            return possibleStartTime;

        }

        public static Time EventEndTime(Time birthTime, Time checkTime, EventData eventData, int precisionInHours)
        {
            //NOTE: hack to enter birth time with existing code
            var birthTimeWrapped = new Person("", birthTime, Gender.Male);

            //check time will be used as possible end time
            var possibleEndTime = checkTime;
            var previousPossibleEndTime = possibleEndTime;

            //end as not found
            var isFound = false;
            while (!isFound) //run while not found
            {
                //check possible end time if event occurring (heavy computation)
                var updatedEventData = EventManager.ConvertToEventSlice(possibleEndTime, eventData, birthTimeWrapped);

                //if occuring than continue to next, end time not found
                if (updatedEventData != null && updatedEventData.IsOccuring)
                {
                    //save a copy of possible time, to be used when we go too far
                    previousPossibleEndTime = possibleEndTime;

                    //increment possible end time, to check next possible end time in the future
                    possibleEndTime = possibleEndTime.AddHours(precisionInHours);
                }
                //end time found!, event has stopped occuring (too far)
                else
                {
                    //return possible end time as confirmed!
                    possibleEndTime = previousPossibleEndTime;
                    isFound = true; //stop looking
                }
            }

            //if control reaches here than end time found
            return possibleEndTime;

        }


        #endregion

        #region MATCH CHECK KUTA SCORE

        /// <summary>
        /// Get full kuta match data for 2 horoscopes
        /// </summary>
        public static MatchReport MatchReport(Time maleBirthTime, Time femaleBirthTime)
        {
            //get 1st and 2nd only for now (todo support more)
            var male = new Person("", maleBirthTime, Gender.Male);
            var female = new Person("", femaleBirthTime, Gender.Female);

            //generate compatibility report
            var compatibilityReport = MatchReportFactory.GetNewMatchReport(male, female, "101");

            return compatibilityReport;
        }

        public static async Task<string> BirthTimeLocationAutoAIFill(string personFullName)
        {
            //get birth time as compatible text
            var birthTime = await BirthTimeAutoAIFill(personFullName);

            //get birth location as compatible text, without comma
            var birthLocation = await BirthLocationAutoAIFill(personFullName);

            //get birth location as compatible text, without comma
            var marriagePartnerName = await MarriagePartnerNameAutoAIFill(personFullName);

            //get partner data
            var marriagePartnerBirthTime = await BirthTimeAutoAIFill(marriagePartnerName);
            var marriagePartnerBirthLocation = await BirthLocationAutoAIFill(marriagePartnerName);

            //get marriage data
            var marriageTags = await MarriageTagsAutoAIFill(personFullName, marriagePartnerName);

            return $"{marriageTags},{personFullName},{birthTime},{birthLocation},{marriagePartnerName},{marriagePartnerBirthTime},{marriagePartnerBirthLocation}";
        }

        /// <summary>
        /// Given a famous person name will auto find birth time using LLM AI
        /// </summary>
        public static async Task<string> BirthTimeAutoAIFill(string personFullName)
        {
            string anyScaleAPIKey = Environment.GetEnvironmentVariable("AnyScaleAPIKey");

            using (var client = new HttpClient())
            {
                var requestBodyObject = new
                {
                    model = "meta-llama/Meta-Llama-3-70B-Instruct",
                    messages = new List<object>
                    {
                        new { role = "system", content = "given person name output birth time as {HH:mm DD/MM/YYYY zzz}" },
                        new { role = "user", content = "Monroe, Marilyn" },
                        new { role = "assistant", content = "09:30 01/06/1926 -08:00" },
                        new { role = "user", content = personFullName }
                    }
                };

                string requestBody = JsonConvert.SerializeObject(requestBodyObject);

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", anyScaleAPIKey);
                client.BaseAddress = new Uri("https://api.endpoints.anyscale.com/v1/chat/completions");

                var content = new StringContent(requestBody);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpResponseMessage response = await client.PostAsync("", content);

                string responseContent = await response.Content.ReadAsStringAsync();
                var fullResponse = JObject.Parse(responseContent);
                var message = fullResponse["choices"][0]["message"]["content"].Value<string>();

                return message;
            }
        }

        public static async Task<string> MarriageTagsAutoAIFill(string personA, string personB)
        {
            string anyScaleAPIKey = Environment.GetEnvironmentVariable("AnyScaleAPIKey");

            using (var client = new HttpClient())
            {
                var requestBodyObject = new
                {
                    model = "meta-llama/Meta-Llama-3-70B-Instruct",
                    messages = new List<object>
                    {
                        new { role = "system", content = "given married couple name output marriage duration in years" },
                        new { role = "user", content = "Brad Pitt & Angelina Jolie" },
                        new { role = "assistant", content = "#2Years" },
                        new { role = "user", content = "Napoleon Bonaparte & Joséphine" },
                        new { role = "assistant", content = "#14Years" },
                        new { role = "user", content = "Dax Shepard & Kristen Bell" },
                        new { role = "assistant", content = "#StillMarried" },
                        new { role = "user", content = $"{personA} & {personB}" }
                    }
                };

                string requestBody = JsonConvert.SerializeObject(requestBodyObject);

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", anyScaleAPIKey);
                client.BaseAddress = new Uri("https://api.endpoints.anyscale.com/v1/chat/completions");

                var content = new StringContent(requestBody);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpResponseMessage response = await client.PostAsync("", content);

                string responseContent = await response.Content.ReadAsStringAsync();
                var fullResponse = JObject.Parse(responseContent);
                var message = fullResponse["choices"][0]["message"]["content"].Value<string>();

                return message;
            }
        }

        /// <summary>
        /// Given a famous person name will auto find birth location using LLM AI
        /// </summary>
        public static async Task<string> BirthLocationAutoAIFill(string personFullName)
        {
            string anyScaleAPIKey = Environment.GetEnvironmentVariable("AnyScaleAPIKey");

            using (var client = new HttpClient())
            {
                var requestBodyObject = new
                {
                    model = "meta-llama/Meta-Llama-3-70B-Instruct",
                    messages = new List<object>
                    {
                        new { role = "system", content = "given person name output birth location as {city state country}" },
                        new { role = "user", content = "Monroe, Marilyn" },
                        new { role = "assistant", content = "Los Angeles California USA" },
                        new { role = "user", content = personFullName }
                    }
                };

                string requestBody = JsonConvert.SerializeObject(requestBodyObject);

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", anyScaleAPIKey);
                client.BaseAddress = new Uri("https://api.endpoints.anyscale.com/v1/chat/completions");

                var content = new StringContent(requestBody);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpResponseMessage response = await client.PostAsync("", content);

                string responseContent = await response.Content.ReadAsStringAsync();
                var fullResponse = JObject.Parse(responseContent);
                var message = fullResponse["choices"][0]["message"]["content"].Value<string>(); ;

                return message;
            }
        }

        /// <summary>
        /// Given a famous person name will auto find marriage partner using LLM AI
        /// </summary>
        public static async Task<string> MarriagePartnerNameAutoAIFill(string personFullName)
        {
            string anyScaleAPIKey = Environment.GetEnvironmentVariable("AnyScaleAPIKey");

            using (var client = new HttpClient())
            {
                var requestBodyObject = new
                {
                    model = "meta-llama/Meta-Llama-3-70B-Instruct",
                    messages = new List<object>
                    {
                        new { role = "system", content = "given person name output first marriage partner name" },
                        new { role = "user", content = "Monroe, Marilyn" },
                        new { role = "assistant", content = "James Dougherty" },
                        new { role = "user", content = personFullName }
                    }
                };

                string requestBody = JsonConvert.SerializeObject(requestBodyObject);

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", anyScaleAPIKey);
                client.BaseAddress = new Uri("https://api.endpoints.anyscale.com/v1/chat/completions");

                var content = new StringContent(requestBody);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpResponseMessage response = await client.PostAsync("", content);

                string responseContent = await response.Content.ReadAsStringAsync();
                var fullResponse = JObject.Parse(responseContent);
                var message = fullResponse["choices"][0]["message"]["content"].Value<string>(); ;

                return message;
            }
        }

        #endregion

        #region CHAT API & MACHINE LEARNING

        /// <summary>
        /// Ask questions to AI astrologer about life horoscope predictions
        /// </summary>
        /// <param name="birthTime">time of person persons horoscope to check</param>
        /// <param name="userQuestion">question related horoscope</param>
        /// <param name="chatSession"></param>
        /// <returns></returns>
        public static async Task<JObject> HoroscopeChat(Time birthTime, string userQuestion, string userId, string sessionId = "")
        {
            return await ChatAPI.SendMessageHoroscope(birthTime, userQuestion, sessionId, userId);
        }

        public static async Task HoroscopeChat2(Time birthTime, string userQuestion, string userId, string sessionId = "")
        {
            //await ChatAPI.CreatePresetQuestionEmbeddings_CohereEmbed();

            await ChatAPI.LLMSearchAPICall_CohereEmbed(userQuestion);
            //var foundQuestions = await ChatAPI.FindPresetQuestionEmbeddings_CohereEmbed(userQuestion);

            //throw new NotImplementedException();
            //return foundQuestions.Select(jv => jv.ToJson()).ToList();



            //return await ChatAPI.SendMessageHoroscope(birthTime, userQuestion, sessionId, userId);

        }

        public static async Task<JObject> HoroscopeChatFeedback(string answerHash, int feedbackScore)
        {
            return await ChatAPI.HoroscopeChatFeedback(answerHash, feedbackScore);
        }

        public static async Task<JObject> HoroscopeFollowUpChat(Time birthTime, string followUpQuestion, string primaryAnswerHash, string userId,
            string sessionId)
        {
            return await ChatAPI.SendMessageHoroscopeFollowUp(birthTime, followUpQuestion, primaryAnswerHash, userId, sessionId);
        }

        /// <summary>
        /// Ask questions to AI astrologer about life horoscope predictions
        /// </summary>
        /// <param name="birthTime">time of person hprtson horoscope to check</param>
        /// <param name="userQuestion">question related horoscope</param>
        /// <param name="chatSession"></param>
        /// <returns></returns>
        public static async Task<JObject> MatchChat(Time maleBirthTime, Time femaleBirthTime, string userQuestion, string chatSession = "")
        {
            return await ChatAPI.SendMessageMatch(maleBirthTime, femaleBirthTime, userQuestion, chatSession);
        }

        /// <summary>
        /// Searches all horoscopes predictions with LLM
        /// </summary>
        public static async Task<List<HoroscopePrediction>> HoroscopeLLMSearch(Time birthTime, string textInput)
        {
            //make http call to python api server
            var timeUrl = birthTime.ToUrl();
            var callUrl = $"https://vedastrocontainer.delightfulground-a2445e4b.westus2.azurecontainerapps.io/HoroscopeLLMSearch";
            var jsonString = $@"{{""query"":""{textInput}"",
                                ""birth_time"":""{timeUrl}"",
                                ""llm_model_name"":""sentence-transformers/all-MiniLM-L6-v2"",
                                ""search_type"" : ""similarity""
                            }}";

            //result is an array of found
            var rawReply = await Tools.MakePostRequest(callUrl, jsonString);

            //convert to nice nice format
            var finalList = new List<HoroscopePrediction>();
            foreach (var eachPrediction in rawReply)
            {
                finalList.Add(HoroscopePrediction.FromLLMJson(eachPrediction));
            }

            return finalList;
        }

        /// <summary>
        /// Given a start time & end time and space in hours between.
        /// Will generate massive CSV tables for ML & Data Science
        /// Will contain 3 columns, "Name","Time","Location"
        /// this can then be fed into ML Table Generator to make datasets worthy of HuggingFace
        /// </summary>
        public static string GenerateTimeListCSV(Time startTime, Time endTime, double hoursBetween)
        {
            //make slices to fill list
            var timeSlices = Time.GetTimeListFromRange(startTime, endTime, hoursBetween);

            //generate CSV string from above time slices
            StringBuilder csv = new StringBuilder();
            csv.AppendLine("Name,Time,Location");

            for (var index = 0; index < timeSlices.Count; index++)
            {
                var time = timeSlices[index];
                var locationNameCSVSafe = time.GetGeoLocation().Name().Replace(",", ""); //remove comma, since CSV reserved
                csv.AppendLine($"row{index},{time.GetStdDateTimeOffsetText()},{locationNameCSVSafe}");
            }

            return csv.ToString();
        }


        #endregion

        #region VARGAS OR SUBTLE DIVISIONS

        /// <summary>
        /// Checks if the inputed sign was the sign of the house during the inputed time
        /// </summary>
        public static bool IsHouseSignName(HouseName house, ZodiacName sign, Time time) => HouseSignName(house, time) == sign;

        /// <summary>
        /// Gets only the the zodiac sign name at middle longitude of the house.
        /// </summary>
        public static ZodiacName HouseSignName(HouseName houseNumber, Time time)
        {
            //get full sign data
            var zodiacSign = Calculate.HouseZodiacSign(houseNumber, time);

            //only return name
            return zodiacSign.GetSignName();
        }

        /// <summary>
        /// Gets the zodiac sign at middle longitude of the house with degrees data (Bhava Chalit)
        /// </summary>
        public static ZodiacSign HouseZodiacSign(HouseName houseNumber, Time time)
        {
            //get all houses
            var allHouses = AllHouseLongitudes(time);

            //get the house specified 
            var specifiedHouse = allHouses.Find(house => house.GetHouseName() == houseNumber);

            //get sign of the specified house
            var middleLongitude = specifiedHouse.GetMiddleLongitude();
            var houseSign = ZodiacSignAtLongitude(middleLongitude);

            //return the name of house sign
            return houseSign;
        }

        /// <summary>
        /// Gets zodiac sign for a given house counted from lagna
        /// </summary>
        public static ZodiacSign HouseRasiSign(HouseName houseNumber, Time time)
        {
            //get all houses
            var allHouses = AllHouseLongitudes(time);

            //get sign of the first house (lagna)
            var house1 = allHouses.Find(house => house.GetHouseName() == HouseName.House1);
            var house1MiddleLongitude = house1.GetMiddleLongitude();
            var house1Sign = ZodiacSignAtLongitude(house1MiddleLongitude);

            //count to specified house sign from 1st house sign
            var houseNumberInt = (int)houseNumber;
            var houseSign = Calculate.SignCountedFromInputSign(house1Sign.GetSignName(), houseNumberInt);

            //return the name of house sign
            return new ZodiacSign(houseSign, house1Sign.GetDegreesInSign());

        }

        /// <summary>
        /// Gets the zodiac sign at middle longitude of the house.
        /// </summary>
        public static Dictionary<HouseName, ZodiacSign> AllHouseZodiacSigns(Time time) => House.AllHouses.ToDictionary(houseName => houseName, houseName => Calculate.HouseZodiacSign(houseName, time));
        public static Dictionary<HouseName, ZodiacSign> AllHouseRasiSigns(Time time) => House.AllHouses.ToDictionary(houseName => houseName, houseName => Calculate.HouseRasiSign(houseName, time));
        public static Dictionary<HouseName, ZodiacSign> AllHouseHoraSign(Time time) => House.AllHouses.ToDictionary(houseName => houseName, houseName => Calculate.HouseHoraD2Sign(houseName, time));
        public static Dictionary<HouseName, ZodiacSign> AllHouseDrekkanaSign(Time time) => House.AllHouses.ToDictionary(houseName => houseName, houseName => Calculate.HouseDrekkanaD3Sign(houseName, time));
        public static Dictionary<HouseName, ZodiacSign> AllHouseChaturthamsaSign(Time time) => House.AllHouses.ToDictionary(houseName => houseName, houseName => Calculate.HouseChaturthamshaD4Sign(houseName, time));
        public static Dictionary<HouseName, ZodiacSign> AllHouseSaptamshaSign(Time time) => House.AllHouses.ToDictionary(houseName => houseName, houseName => Calculate.HouseSaptamshaD7Sign(houseName, time));
        public static Dictionary<HouseName, ZodiacSign> AllHouseNavamshaSign(Time time) => House.AllHouses.ToDictionary(houseName => houseName, houseName => Calculate.HouseNavamshaD9Sign(houseName, time));
        public static Dictionary<HouseName, ZodiacSign> AllHouseDashamamshaSign(Time time) => House.AllHouses.ToDictionary(houseName => houseName, houseName => Calculate.HouseDashamamshaD10Sign(houseName, time));
        public static Dictionary<HouseName, ZodiacSign> AllHouseDwadashamshaSign(Time time) => House.AllHouses.ToDictionary(houseName => houseName, houseName => Calculate.HouseDwadashamshaD12Sign(houseName, time));
        public static Dictionary<HouseName, ZodiacSign> AllHouseShodashamshaSign(Time time) => House.AllHouses.ToDictionary(houseName => houseName, houseName => Calculate.HouseShodashamshaD16Sign(houseName, time));
        public static Dictionary<HouseName, ZodiacSign> AllHouseVimshamshaSign(Time time) => House.AllHouses.ToDictionary(houseName => houseName, houseName => Calculate.HouseVimshamshaD20Sign(houseName, time));
        public static Dictionary<HouseName, ZodiacSign> AllHouseChaturvimshamshaSign(Time time) => House.AllHouses.ToDictionary(houseName => houseName, houseName => Calculate.HouseChaturvimshamshaD24Sign(houseName, time));
        public static Dictionary<HouseName, ZodiacSign> AllHouseBhamshaSign(Time time) => House.AllHouses.ToDictionary(houseName => houseName, houseName => Calculate.HouseBhamshaD27Sign(houseName, time));
        public static Dictionary<HouseName, ZodiacSign> AllHouseTrimshamshaSign(Time time) => House.AllHouses.ToDictionary(houseName => houseName, houseName => Calculate.HouseTrimshamshaD30Sign(houseName, time));
        public static Dictionary<HouseName, ZodiacSign> AllHouseKhavedamshaSign(Time time) => House.AllHouses.ToDictionary(houseName => houseName, houseName => Calculate.HouseKhavedamshaD40Sign(houseName, time));
        public static Dictionary<HouseName, ZodiacSign> AllHouseAkshavedamshaSign(Time time) => House.AllHouses.ToDictionary(houseName => houseName, houseName => Calculate.HouseAkshavedamshaD45Sign(houseName, time));
        public static Dictionary<HouseName, ZodiacSign> AllHouseShashtyamshaSign(Time time) => House.AllHouses.ToDictionary(houseName => houseName, houseName => Calculate.HouseShashtyamshaD60Sign(houseName, time));



        /// <summary>
        /// Calculates the divisional longitude of a planet in a D-chart (divisional chart) in Vedic Astrology.
        /// written by AI & Human 
        /// </summary>
        /// <param name="planetName">The name of the planet.</param>
        /// <param name="inputTime">The time for which the calculation is to be made.</param>
        /// <param name="divisionalNo">The number of the D-chart (e.g., 2, 3, 4, 7, 9, etc.).</param>
        /// <returns>The divisional longitude of the planet in the specified D-chart.</returns>
        public static Angle PlanetDivisionalLongitude(PlanetName planetName, Time inputTime, int divisionalNo)
        {
            // Step 1: Get the Nirayana (sidereal) longitude of the planet at the given time.
            var planet_degrees = Calculate.PlanetNirayanaLongitude(planetName, inputTime).TotalDegrees;

            // Multiply the planet's longitude by the D-chart number to get the raw divisional longitude.
            var total_degrees = planet_degrees * divisionalNo;

            // Step 2: Normalize the raw divisional longitude to the range [0, 60) degrees.
            // This is done by subtracting 60 (the number of degrees in a zodiac sign) until the result is less than 60.
            while (total_degrees >= 60)
            {
                total_degrees -= 60;
            }

            // The remaining value is the longitude of the planet in the D-chart.
            var finalLong = Angle.FromDegrees(total_degrees);
            return finalLong;
        }

        public static Angle DivisionalLongitude(double totalDegrees, int divisionalNo)
        {
            // Step 1: Get the Nirayana (sidereal) longitude of the planet at the given time.
            //var planet_degrees = Calculate.PlanetNirayanaLongitude(planetName, inputTime).TotalDegrees;

            // Multiply the planet's longitude by the D-chart number to get the raw divisional longitude.
            var total_degrees = totalDegrees * divisionalNo;

            // Step 2: Normalize the raw divisional longitude to the range [0, 30) degrees.
            // This is done by subtracting 30 (the number of degrees in a zodiac sign) until the result is less than 30.
            while (total_degrees >= 30)
            {
                total_degrees -= 30;
            }

            // The remaining value is the longitude of the planet in the D-chart.
            var finalLong = Angle.FromDegrees(total_degrees);
            return finalLong;
        }

        //------------ D0 : Bhava ------------

        /// <summary>
        /// Get zodiac sign planet is in based on house longitudes
        /// basically the sign of the house the planet is in based on longitudes
        /// D0 Bhava chart
        /// </summary>
        public static ZodiacSign PlanetZodiacSignBasedOnHouseLongitudes(PlanetName planetName, Time time)
        {
            //get house of planet
            var planetHouse = Calculate.HousePlanetOccupiesBasedOnLongitudes(planetName, time);

            //get zodiac sign at middle longitude of house
            var houseZodiacSign = Calculate.HouseZodiacSign(planetHouse, time);

            //return
            return houseZodiacSign;
        }


        //------------ D1 : Rashi ------------
        /// <summary>
        /// Get zodiac sign planet is in.
        /// D1
        /// </summary>
        public static ZodiacSign PlanetRasiD1Sign(PlanetName planetName, Time time)
        {
            //CACHE MECHANISM
            return CacheManager.GetCache(new CacheKey(nameof(PlanetRasiD1Sign), planetName, time, Ayanamsa), _planetRasiD1Sign);

            //UNDERLYING FUNCTION
            ZodiacSign _planetRasiD1Sign()
            {
                //get longitude of planet
                var longitudeOfPlanet = PlanetNirayanaLongitude(planetName, time);

                //get sign planet is in
                var signPlanetIsIn = ZodiacSignAtLongitude(longitudeOfPlanet);

                //return
                return signPlanetIsIn;

            }

        }


        //------------ D2 : Hora ------------

        /// <summary>
        /// Gets Hora (D2) zodiac sign of a planet
        /// </summary>
        public static ZodiacSign PlanetHoraD2Signs(PlanetName planetName, Time time) => Calculate.DrekkanaSignName(Calculate.PlanetRasiD1Sign(planetName, time));

        /// <summary>
        /// D2 chart
        /// </summary>
        public static ZodiacSign HoraSignName(ZodiacSign zodiacSign) => Vargas.VargasCoreCalculator(zodiacSign, Vargas.HoraTable, 2);

        /// <summary>
        /// Given a longitude will return Hora (D2) sign at that longitude
        /// </summary>
        public static ZodiacSign HoraSignAtLongitude(Angle longitude) => HoraSignName(ZodiacSignAtLongitude(longitude));

        /// <summary>
        /// Gets the zodiac sign at middle longitude of the house with degrees data
        /// </summary>
        public static ZodiacSign HouseHoraD2Sign(HouseName houseNumber, Time time)
        {
            //get all houses
            var allHouses = AllHouseLongitudes(time);

            //get sign of the first house (lagna)
            var house1 = allHouses.Find(house => house.GetHouseName() == HouseName.House1);
            var house1MiddleLongitude = house1.GetMiddleLongitude();
            var house1Sign = HoraSignAtLongitude(house1MiddleLongitude);

            //count to specified house sign from 1st house sign
            var houseSign = Calculate.SignCountedFromInputSign(house1Sign.GetSignName(), (int)houseNumber);

            //return the name of house sign
            return new ZodiacSign(houseSign, house1Sign.GetDegreesInSign());
        }


        //------------ D3 : Drekkana ------------

        /// <summary>
        /// Gets the Drekkana sign the planet is in
        /// D3
        /// </summary>
        public static ZodiacSign PlanetDrekkanaD3Sign(PlanetName planetName, Time time) => Calculate.DrekkanaSignName(Calculate.PlanetRasiD1Sign(planetName, time));

        /// <summary>
        /// Given a zodiac sign will convert to drekkana
        /// D3
        /// </summary>
        public static ZodiacSign DrekkanaSignName(ZodiacSign zodiacSign) => Vargas.VargasCoreCalculator(zodiacSign, Vargas.DrekkanaTable, 3);

        /// <summary>
        /// Given a longitude will return Drekkana (D3) sign at that longitude
        /// </summary>
        public static ZodiacSign DrekkanaSignAtLongitude(Angle longitude) => DrekkanaSignName(ZodiacSignAtLongitude(longitude));

        /// <summary>
        /// Gets the Drekkana sign at middle longitude of the house with degrees data
        /// </summary>
        public static ZodiacSign HouseDrekkanaD3Sign(HouseName houseNumber, Time time)
        {
            //get all houses
            var allHouses = AllHouseLongitudes(time);

            //get sign of the first house (lagna)
            var house1 = allHouses.Find(house => house.GetHouseName() == HouseName.House1);
            var house1MiddleLongitude = house1.GetMiddleLongitude();
            var house1Sign = DrekkanaSignAtLongitude(house1MiddleLongitude);

            //count to specified house sign from 1st house sign
            var houseSign = Calculate.SignCountedFromInputSign(house1Sign.GetSignName(), (int)houseNumber);

            //return the name of house sign
            return new ZodiacSign(houseSign, house1Sign.GetDegreesInSign());
        }



        //------------ D4 : Chaturthamsha ------------

        /// <summary>
        /// D4 chart
        /// </summary>
        public static ZodiacSign PlanetChaturthamshaD4Sign(PlanetName planetName, Time time) => Calculate.ChaturthamshaSignName(Calculate.PlanetRasiD1Sign(planetName, time));

        /// <summary>
        /// D4 chart
        /// </summary>
        public static ZodiacSign ChaturthamshaSignName(ZodiacSign zodiacSign) => Vargas.VargasCoreCalculator(zodiacSign, Vargas.ChaturthamshaTable, 4);

        /// <summary>
        /// Given a longitude will return Hora (D4) sign at that longitude
        /// </summary>
        public static ZodiacSign ChaturthamshaSignAtLongitude(Angle longitude) => ChaturthamshaSignName(ZodiacSignAtLongitude(longitude));

        /// <summary>
        /// Gets the Chaturthamsha D4 sign at middle longitude of the house with degrees data
        /// </summary>
        public static ZodiacSign HouseChaturthamshaD4Sign(HouseName houseNumber, Time time)
        {
            //get all houses
            var allHouses = AllHouseLongitudes(time);

            //get sign of the first house (lagna)
            var house1 = allHouses.Find(house => house.GetHouseName() == HouseName.House1);
            var house1MiddleLongitude = house1.GetMiddleLongitude();
            var house1Sign = ChaturthamshaSignAtLongitude(house1MiddleLongitude);

            //count to specified house sign from 1st house sign
            var houseSign = Calculate.SignCountedFromInputSign(house1Sign.GetSignName(), (int)houseNumber);

            //return the name of house sign
            return new ZodiacSign(houseSign, house1Sign.GetDegreesInSign());
        }



        //------------ D7 : Saptamsha ------------
        /// <summary>
        /// D7 chart
        /// </summary>
        public static ZodiacSign PlanetSaptamshaD7Sign(PlanetName planetName, Time time) => Calculate.SaptamshaSignName(Calculate.PlanetRasiD1Sign(planetName, time));

        /// <summary>
        /// D7 chart
        /// </summary>
        public static ZodiacSign SaptamshaSignName(ZodiacSign zodiacSign) => Vargas.VargasCoreCalculator(zodiacSign, Vargas.SaptamshaTable, 7);

        /// <summary>
        /// Given a longitude will return Saptamsha (D7) sign at that longitude
        /// </summary>
        public static ZodiacSign SaptamshaSignAtLongitude(Angle longitude) => SaptamshaSignName(ZodiacSignAtLongitude(longitude));

        /// <summary>
        /// Gets the Saptamsha (D7) sign at middle longitude of the house with degrees data
        /// </summary>
        public static ZodiacSign HouseSaptamshaD7Sign(HouseName houseNumber, Time time)
        {
            //get all houses
            var allHouses = AllHouseLongitudes(time);

            //get sign of the first house (lagna)
            var house1 = allHouses.Find(house => house.GetHouseName() == HouseName.House1);
            var house1MiddleLongitude = house1.GetMiddleLongitude();
            var house1Sign = SaptamshaSignAtLongitude(house1MiddleLongitude);

            //count to specified house sign from 1st house sign
            var houseSign = Calculate.SignCountedFromInputSign(house1Sign.GetSignName(), (int)houseNumber);

            //return the name of house sign
            return new ZodiacSign(houseSign, house1Sign.GetDegreesInSign());
        }

        /// <summary>
        /// Saptamsa (D7) and measures 4.28 degrees
        /// TODO : BV RAMAN method, OLD MARKED FOR OBLIVION, NEEDS TESTING AGAINST NEW METHOD
        /// </summary>
        public static ZodiacName PlanetSaptamshaSignOLD(PlanetName planetName, Time time)
        {
            //get sign planet is in
            var planetSign = PlanetRasiD1Sign(planetName, time);

            //get planet sign name
            var planetSignName = planetSign.GetSignName();

            //get degrees in sign 
            var degreesInSign = planetSign.GetDegreesInSign().TotalDegrees;

            //declare const number for saptamsa calculation
            const double maxSaptamsaDegrees = 4.285714285714286; // 30/7
            const double maxSignDegrees = 30.0;

            //get rough saptamsa number
            double roughSaptamsaNumber = (degreesInSign % maxSignDegrees) / maxSaptamsaDegrees;

            //get rounded saptamsa number
            var saptamsaNumber = (int)Math.Ceiling(roughSaptamsaNumber);

            //2.0 Get even or odd sign

            //if planet is in odd sign
            if (IsOddSign(planetSignName))
            {
                //convert saptamsa number to zodiac name
                return SignCountedFromInputSign(planetSignName, saptamsaNumber);
            }

            //if planet is in even sign
            if (IsEvenSign(planetSignName))
            {
                var countToNextSign = saptamsaNumber + 6;
                return SignCountedFromInputSign(planetSignName, countToNextSign);
            }


            throw new Exception("Saptamsa not found, error!");
        }


        //------------ D9 : Navamsha ------------

        /// <summary>
        /// D9 chart
        /// </summary>
        public static ZodiacSign PlanetNavamshaD9Sign(PlanetName planetName, Time time) => Calculate.NavamshaSignName(Calculate.PlanetRasiD1Sign(planetName, time));

        /// <summary>
        /// D9 chart
        /// </summary>
        public static ZodiacSign NavamshaSignName(ZodiacSign zodiacSign) => Vargas.VargasCoreCalculator(zodiacSign, Vargas.NavamshaTable, 9);

        /// <summary>
        /// Gets Navamsa (D9) sign given a longitude
        /// </summary>
        public static ZodiacSign NavamshaSignAtLongitude(Angle longitude) => Calculate.NavamshaSignName(ZodiacSignAtLongitude(longitude));

        /// <summary>
        /// Get Navamsa (D9) sign of house (mid point)
        /// </summary>
        public static ZodiacSign HouseNavamshaD9Sign(HouseName houseNumber, Time time)
        {
            //get all houses
            var allHouses = AllHouseLongitudes(time);

            //get sign of the first house (lagna)
            var house1 = allHouses.Find(house => house.GetHouseName() == HouseName.House1);
            var house1MiddleLongitude = house1.GetMiddleLongitude();
            var house1Sign = NavamshaSignAtLongitude(house1MiddleLongitude);

            //count to specified house sign from 1st house sign
            var houseSign = Calculate.SignCountedFromInputSign(house1Sign.GetSignName(), (int)houseNumber);

            //return the name of house sign
            return new ZodiacSign(houseSign, house1Sign.GetDegreesInSign());
        }

        /// <summary>
        /// Gets Navamsa (D9) sign given a longitude
        /// TODO : BV RAMAN method, OLD MARKED FOR OBLIVION, NEEDS TESTING AGAINST NEW METHOD
        /// </summary>
        public static ZodiacSign NavamshaSignAtLongitudeOLD(Angle longitude)
        {
            //1.0 Get ordinary zodiac sign name
            //get ordinary zodiac sign
            var ordinarySign = ZodiacSignAtLongitude(longitude);

            //get name of ordinary sign
            var ordinarySignName = ordinarySign.GetSignName();

            //2.0 Get first navamsa sign
            ZodiacName firstNavamsa;

            switch (ordinarySignName)
            {
                //Aries, Leo, Sagittarius - from Aries.
                case ZodiacName.Aries:
                case ZodiacName.Leo:
                case ZodiacName.Sagittarius:
                    firstNavamsa = ZodiacName.Aries;
                    break;
                //Taurus, Capricorn, Virgo - from Capricorn.
                case ZodiacName.Taurus:
                case ZodiacName.Capricorn:
                case ZodiacName.Virgo:
                    firstNavamsa = ZodiacName.Capricorn;
                    break;
                //Gemini, Libra, Aquarius - from Libra.
                case ZodiacName.Gemini:
                case ZodiacName.Libra:
                case ZodiacName.Aquarius:
                    firstNavamsa = ZodiacName.Libra;
                    break;
                //Cancer, Scorpio, Pisces - from Cancer.
                case ZodiacName.Cancer:
                case ZodiacName.Scorpio:
                case ZodiacName.Pisces:
                    firstNavamsa = ZodiacName.Cancer;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            //3.0 Get the number of the navamsa currently in
            //get degrees in ordinary sign
            var degreesInOrdinarySign = ordinarySign.GetDegreesInSign();

            //declare length of a navamsa in the ecliptic arc
            const double navamsaLenghtInDegrees = 3.333333333;

            //divide total degrees in current sign to get raw navamsa number
            var rawNavamsaNumber = degreesInOrdinarySign.TotalDegrees / navamsaLenghtInDegrees;

            //round the raw number to get current navamsa number
            var navamsaNumber = (int)Math.Ceiling(rawNavamsaNumber);

            //4.0 Get navamsa sign
            //count from first navamsa sign
            ZodiacName signAtNavamsa = SignCountedFromInputSign(firstNavamsa, navamsaNumber);

            return new ZodiacSign(signAtNavamsa, Angle.Zero);

        }



        //------------ D10 : Dashamamsha ------------
        /// <summary>
        /// D10 chart
        /// </summary>
        public static ZodiacSign PlanetDashamamshaD10Sign(PlanetName planetName, Time time) => Calculate.DashamamshaSignName(Calculate.PlanetRasiD1Sign(planetName, time));

        /// <summary>
        /// D10 chart
        /// </summary>
        public static ZodiacSign DashamamshaSignName(ZodiacSign zodiacSign) => Vargas.VargasCoreCalculator(zodiacSign, Vargas.DashamamshaTable, 10);

        /// <summary>
        /// Given a longitude will return Dashamamsha (D10) sign at that longitude
        /// </summary>
        public static ZodiacSign DashamamshaSignAtLongitude(Angle longitude) => DashamamshaSignName(ZodiacSignAtLongitude(longitude));

        /// <summary>
        /// Gets the Dashamamsha (D10) sign at middle longitude of the house with degrees data
        /// </summary>
        public static ZodiacSign HouseDashamamshaD10Sign(HouseName houseNumber, Time time)
        {
            //get all houses
            var allHouses = AllHouseLongitudes(time);

            //get sign of the first house (lagna)
            var house1 = allHouses.Find(house => house.GetHouseName() == HouseName.House1);
            var house1MiddleLongitude = house1.GetMiddleLongitude();
            var house1Sign = DashamamshaSignAtLongitude(house1MiddleLongitude);

            //count to specified house sign from 1st house sign
            var houseSign = Calculate.SignCountedFromInputSign(house1Sign.GetSignName(), (int)houseNumber);

            //return the name of house sign
            return new ZodiacSign(houseSign, house1Sign.GetDegreesInSign());
        }




        //------------ D12 : Dwadashamsha ------------

        /// <summary>
        /// D12 chart
        /// </summary>
        public static ZodiacSign PlanetDwadashamshaD12Sign(PlanetName planetName, Time time) => Calculate.DwadashamshaSignName(Calculate.PlanetRasiD1Sign(planetName, time));

        /// <summary>
        /// D12 chart
        /// </summary>
        public static ZodiacSign DwadashamshaSignName(ZodiacSign zodiacSign) => Vargas.VargasCoreCalculator(zodiacSign, Vargas.DwadashamshaTable, 12);

        /// <summary>
        /// Given a longitude will return Dwadashamsha (D12) sign at that longitude
        /// </summary>
        public static ZodiacSign DwadashamshaSignAtLongitude(Angle longitude) => DwadashamshaSignName(ZodiacSignAtLongitude(longitude));

        /// <summary>
        /// Gets the Dwadashamsha (D12) sign at middle longitude of the house with degrees data
        /// </summary>
        public static ZodiacSign HouseDwadashamshaD12Sign(HouseName houseNumber, Time time)
        {
            //get all houses
            var allHouses = AllHouseLongitudes(time);

            //get sign of the first house (lagna)
            var house1 = allHouses.Find(house => house.GetHouseName() == HouseName.House1);
            var house1MiddleLongitude = house1.GetMiddleLongitude();
            var house1Sign = DwadashamshaSignAtLongitude(house1MiddleLongitude);

            //count to specified house sign from 1st house sign
            var houseSign = Calculate.SignCountedFromInputSign(house1Sign.GetSignName(), (int)houseNumber);

            //return the name of house sign
            return new ZodiacSign(houseSign, house1Sign.GetDegreesInSign());
        }

        /// <summary>
        /// When a sign is divided into 12 equal parts each is called a Dwadasamsa (D12) and measures 2.5 degrees.
        /// The Bhachakra can thus he said to contain 12x12=144 Dwadasamsas. The lords of the 12
        /// Dwadasamsas in a sign are the lords of the 12 signs from it, i.e.,
        /// the lord of the first Dwadasamsa in Mesha is Kuja, that of the second Sukra and so on.
        /// 
        /// TODO : BV RAMAN method, OLD MARKED FOR OBLIVION, NEEDS TESTING AGAINST NEW METHOD
        /// </summary>
        public static ZodiacName PlanetDwadashamshaSignOLD(PlanetName planetName, Time time)
        {
            //get sign planet is in
            var planetSign = PlanetRasiD1Sign(planetName, time);

            //get planet sign name
            var planetSignName = planetSign.GetSignName();

            //get degrees in sign 
            var degreesInSign = planetSign.GetDegreesInSign().TotalDegrees;

            //declare const number for Dwadasamsa calculation
            const double maxDwadasamsaDegrees = 2.5; // 30/12
            const double maxSignDegrees = 30.0;

            //get rough Dwadasamsa number
            double roughDwadasamsaNumber = (degreesInSign % maxSignDegrees) / maxDwadasamsaDegrees;

            //get rounded Dwadasamsa number
            var dwadasamsaNumber = (int)Math.Ceiling(roughDwadasamsaNumber);

            //get Dwadasamsa sign from counting with Dwadasamsa number
            var dwadasamsaSign = SignCountedFromInputSign(planetSignName, dwadasamsaNumber);

            return dwadasamsaSign;
        }


        //------------ D16 : Shodashamsha ------------

        /// <summary>
        /// D16 chart
        /// </summary>
        public static ZodiacSign PlanetShodashamshaD16Sign(PlanetName planetName, Time time) => Calculate.ShodashamshaSignName(Calculate.PlanetRasiD1Sign(planetName, time));

        /// <summary>
        /// D16 chart
        /// </summary>
        public static ZodiacSign ShodashamshaSignName(ZodiacSign zodiacSign) => Vargas.VargasCoreCalculator(zodiacSign, Vargas.ShodashamshaTable, 16);

        /// <summary>
        /// Given a longitude will return Shodashamsha (D16) sign at that longitude
        /// </summary>
        public static ZodiacSign ShodashamshaSignAtLongitude(Angle longitude) => ShodashamshaSignName(ZodiacSignAtLongitude(longitude));

        /// <summary>
        /// Gets the Shodashamsha (D16) sign at middle longitude of the house with degrees data
        /// </summary>
        public static ZodiacSign HouseShodashamshaD16Sign(HouseName houseNumber, Time time)
        {
            //get all houses
            var allHouses = AllHouseLongitudes(time);

            //get sign of the first house (lagna)
            var house1 = allHouses.Find(house => house.GetHouseName() == HouseName.House1);
            var house1MiddleLongitude = house1.GetMiddleLongitude();
            var house1Sign = ShodashamshaSignAtLongitude(house1MiddleLongitude);

            //count to specified house sign from 1st house sign
            var houseSign = Calculate.SignCountedFromInputSign(house1Sign.GetSignName(), (int)houseNumber);

            //return the name of house sign
            return new ZodiacSign(houseSign, house1Sign.GetDegreesInSign());
        }



        //------------ D20 : Vimshamsha ------------
        /// <summary>
        /// D20 chart
        /// </summary>
        public static ZodiacSign PlanetVimshamshaD20Sign(PlanetName planetName, Time time) => Calculate.VimshamshaSignName(Calculate.PlanetRasiD1Sign(planetName, time));

        /// <summary>
        /// D20 chart
        /// </summary>
        public static ZodiacSign VimshamshaSignName(ZodiacSign zodiacSign) => Vargas.VargasCoreCalculator(zodiacSign, Vargas.VimshamshaTable, 20);

        /// <summary>
        /// Given a longitude will return Vimshamsha (D20) sign at that longitude
        /// </summary>
        public static ZodiacSign VimshamshaSignAtLongitude(Angle longitude) => VimshamshaSignName(ZodiacSignAtLongitude(longitude));

        /// <summary>
        /// Gets the Vimshamsha (D20) sign at middle longitude of the house with degrees data
        /// </summary>
        public static ZodiacSign HouseVimshamshaD20Sign(HouseName houseNumber, Time time)
        {
            //get all houses
            var allHouses = AllHouseLongitudes(time);

            //get sign of the first house (lagna)
            var house1 = allHouses.Find(house => house.GetHouseName() == HouseName.House1);
            var house1MiddleLongitude = house1.GetMiddleLongitude();
            var house1Sign = VimshamshaSignAtLongitude(house1MiddleLongitude);

            //count to specified house sign from 1st house sign
            var houseSign = Calculate.SignCountedFromInputSign(house1Sign.GetSignName(), (int)houseNumber);

            //return the name of house sign
            return new ZodiacSign(houseSign, house1Sign.GetDegreesInSign());
        }



        //------------ D24 : Chaturvimshamsha ------------
        /// <summary>
        /// D24 chart
        /// </summary>
        public static ZodiacSign PlanetChaturvimshamshaD24Sign(PlanetName planetName, Time time) => Calculate.ChaturvimshamshaSignName(Calculate.PlanetRasiD1Sign(planetName, time));

        /// <summary>
        /// D24 chart
        /// </summary>
        public static ZodiacSign ChaturvimshamshaSignName(ZodiacSign zodiacSign) => Vargas.VargasCoreCalculator(zodiacSign, Vargas.ChaturvimshamshaTable, 24);

        /// <summary>
        /// Given a longitude will return Chaturvimshamsha (D24) sign at that longitude
        /// </summary>
        public static ZodiacSign ChaturvimshamshaSignAtLongitude(Angle longitude) => ChaturvimshamshaSignName(ZodiacSignAtLongitude(longitude));

        /// <summary>
        /// Gets the Chaturvimshamsha (D24) sign at middle longitude of the house with degrees data
        /// </summary>
        public static ZodiacSign HouseChaturvimshamshaD24Sign(HouseName houseNumber, Time time)
        {
            //get all houses
            var allHouses = AllHouseLongitudes(time);

            //get sign of the first house (lagna)
            var house1 = allHouses.Find(house => house.GetHouseName() == HouseName.House1);
            var house1MiddleLongitude = house1.GetMiddleLongitude();
            var house1Sign = ChaturvimshamshaSignAtLongitude(house1MiddleLongitude);

            //count to specified house sign from 1st house sign
            var houseSign = Calculate.SignCountedFromInputSign(house1Sign.GetSignName(), (int)houseNumber);

            //return the name of house sign
            return new ZodiacSign(houseSign, house1Sign.GetDegreesInSign());
        }



        //------------ D27 : Bhamsha / Sapta-vimshamsha ------------
        /// <summary>
        /// D27 chart
        /// </summary>
        public static ZodiacSign PlanetBhamshaD27Sign(PlanetName planetName, Time time) => Calculate.BhamshaSignName(Calculate.PlanetRasiD1Sign(planetName, time));

        /// <summary>
        /// D27 chart
        /// </summary>
        public static ZodiacSign BhamshaSignName(ZodiacSign zodiacSign) => Vargas.VargasCoreCalculator(zodiacSign, Vargas.BhamshaTable, 27);

        /// <summary>
        /// Given a longitude will return Bhamsha (D27) sign at that longitude
        /// </summary>
        public static ZodiacSign BhamshaSignAtLongitude(Angle longitude) => BhamshaSignName(ZodiacSignAtLongitude(longitude));

        /// <summary>
        /// Gets the Bhamsha (D27) sign at middle longitude of the house with degrees data
        /// </summary>
        public static ZodiacSign HouseBhamshaD27Sign(HouseName houseNumber, Time time)
        {
            //get all houses
            var allHouses = AllHouseLongitudes(time);

            //get sign of the first house (lagna)
            var house1 = allHouses.Find(house => house.GetHouseName() == HouseName.House1);
            var house1MiddleLongitude = house1.GetMiddleLongitude();
            var house1Sign = BhamshaSignAtLongitude(house1MiddleLongitude);

            //count to specified house sign from 1st house sign
            var houseSign = Calculate.SignCountedFromInputSign(house1Sign.GetSignName(), (int)houseNumber);

            //return the name of house sign
            return new ZodiacSign(houseSign, house1Sign.GetDegreesInSign());
        }



        //------------ D30 : Trimshamsha ------------

        /// <summary>
        /// Get Thrimsamsa (D30) sign of planet
        /// Trimshamsha or one-thirtieth of a sign
        ///
        /// Reference (Elements of Astrology) : 
        /// Trimshamsha (Table X-12): Literally speaking, it is considered as one-
        /// thirtieth division of a sign. Actually, however, each sign is divided into five
        /// unequal parts, each part belonging to one of the five planets from Mars to
        /// Saturn. In odd signs, the first five degrees belong to Mars, the next five
        /// degrees to Saturn, the next eight degrees to Jupiter, the subsequent seven
        /// degrees to Mercury, and the last five degrees to Venus. This order gets
        /// reversed in case of even signs where the planets Venus, Mercury, Jupiter,
        /// Saturn and Mars respectively own five degrees, seven degrees, eight
        /// degrees, five degrees and five degrees, in a sign.
        ///
        /// </summary>

        public static ZodiacSign PlanetTrimshamshaD30Sign(PlanetName planetName, Time time) => Calculate.TrimshamshaSignName(Calculate.PlanetRasiD1Sign(planetName, time));

        /// <summary>
        /// D30 chart
        /// </summary>
        public static ZodiacSign TrimshamshaSignName(ZodiacSign zodiacSign)
        {
            //get planet sign name
            var zodiacSignName = zodiacSign.GetSignName();

            //get degrees in sign 
            var degreesInSign = zodiacSign.GetDegreesInSign().TotalDegrees;

            //declare const number for Thrimsamsa calculation
            const double maxThrimsamsaDegrees = 1; // 30/1
            const double maxSignDegrees = 30.0;

            //get rough Thrimsamsa number
            double roughThrimsamsaNumber = (degreesInSign % maxSignDegrees) / maxThrimsamsaDegrees;

            //get rounded saptamsa number
            var thrimsamsaNumber = (int)Math.Ceiling(roughThrimsamsaNumber);

            var signName = ZodiacName.Empty;

            //if planet is in odd sign
            if (IsOddSign(zodiacSignName))
            {
                //1,2,3,4,5 - Mars
                if (thrimsamsaNumber >= 0 && thrimsamsaNumber <= 5)
                {
                    //Aries and Scorpio are ruled by Mars
                    signName = ZodiacName.Scorpio;
                }
                //6,7,8,9,10 - saturn
                if (thrimsamsaNumber >= 6 && thrimsamsaNumber <= 10)
                {
                    //Capricorn and Aquarius by Saturn.
                    signName = ZodiacName.Capricorn;

                }
                //11,12,13,14,15,16,17,18 - jupiter
                if (thrimsamsaNumber >= 11 && thrimsamsaNumber <= 18)
                {
                    //Sagittarius and Pisces by Jupiter
                    signName = ZodiacName.Sagittarius;

                }
                //19,20,21,22,23,24,25 - mercury
                if (thrimsamsaNumber >= 19 && thrimsamsaNumber <= 25)
                {
                    //Gemini and Virgo by Mercury
                    signName = ZodiacName.Gemini;
                }
                //26,27,28,29,30 - venus
                if (thrimsamsaNumber >= 26 && thrimsamsaNumber <= 30)
                {
                    //Taurus and Libra by Venus;
                    signName = ZodiacName.Taurus;
                }

            }

            //if planet is in even sign
            if (IsEvenSign(zodiacSignName))
            {
                //1,2,3,4,5 - venus
                if (thrimsamsaNumber >= 0 && thrimsamsaNumber <= 5)
                {
                    //Taurus and Libra by Venus;
                    signName = ZodiacName.Taurus;
                }
                //6,7,8,9,10,11,12 - mercury
                if (thrimsamsaNumber >= 6 && thrimsamsaNumber <= 12)
                {
                    //Gemini and Virgo by Mercury
                    signName = ZodiacName.Gemini;
                }
                //13,14,15,16,17,18,19,20 - jupiter
                if (thrimsamsaNumber >= 13 && thrimsamsaNumber <= 20)
                {
                    //Sagittarius and Pisces by Jupiter
                    signName = ZodiacName.Sagittarius;

                }
                //21,22,23,24,25 - saturn
                if (thrimsamsaNumber >= 21 && thrimsamsaNumber <= 25)
                {
                    //Capricorn and Aquarius by Saturn.
                    signName = ZodiacName.Capricorn;

                }
                //26,27,28,29,30 - Mars
                if (thrimsamsaNumber >= 26 && thrimsamsaNumber <= 30)
                {
                    //Aries and Scorpio are ruled by Mars
                    signName = ZodiacName.Scorpio;
                }

            }

            //NOTE : degrees in sign have to be converted (special logic) for specific division type
            var divisionalDegreesInSign = Calculate.DivisionalLongitude(degreesInSign, 30);
            return new ZodiacSign(signName, divisionalDegreesInSign);

        }

        /// <summary>
        /// Given a longitude will return Trimshamsha (D30) sign at that longitude
        /// </summary>
        public static ZodiacSign TrimshamshaSignAtLongitude(Angle longitude) => TrimshamshaSignName(ZodiacSignAtLongitude(longitude));

        /// <summary>
        /// Gets the Trimshamsha (D30) sign at middle longitude of the house with degrees data
        /// </summary>
        public static ZodiacSign HouseTrimshamshaD30Sign(HouseName houseNumber, Time time)
        {
            //get all houses
            var allHouses = AllHouseLongitudes(time);

            //get sign of the first house (lagna)
            var house1 = allHouses.Find(house => house.GetHouseName() == HouseName.House1);
            var house1MiddleLongitude = house1.GetMiddleLongitude();
            var house1Sign = TrimshamshaSignAtLongitude(house1MiddleLongitude);

            //count to specified house sign from 1st house sign
            var houseSign = Calculate.SignCountedFromInputSign(house1Sign.GetSignName(), (int)houseNumber);

            //return the name of house sign
            return new ZodiacSign(houseSign, house1Sign.GetDegreesInSign());
        }



        //------------ D40 : Khavedamsha ------------
        /// <summary>
        /// D40 chart
        /// </summary>
        public static ZodiacSign PlanetKhavedamshaD40Sign(PlanetName planetName, Time time) => Calculate.KhavedamshaSignName(Calculate.PlanetRasiD1Sign(planetName, time));

        /// <summary>
        /// D40 chart
        /// </summary>
        public static ZodiacSign KhavedamshaSignName(ZodiacSign zodiacSign) => Vargas.VargasCoreCalculator(zodiacSign, Vargas.KhavedamshaTable, 40);

        /// <summary>
        /// Given a longitude will return Khavedamsha (D40) sign at that longitude
        /// </summary>
        public static ZodiacSign KhavedamshaSignAtLongitude(Angle longitude) => KhavedamshaSignName(ZodiacSignAtLongitude(longitude));

        /// <summary>
        /// Gets the Khavedamsha (D40) sign at middle longitude of the house with degrees data
        /// </summary>
        public static ZodiacSign HouseKhavedamshaD40Sign(HouseName houseNumber, Time time)
        {
            //get all houses
            var allHouses = AllHouseLongitudes(time);

            //get sign of the first house (lagna)
            var house1 = allHouses.Find(house => house.GetHouseName() == HouseName.House1);
            var house1MiddleLongitude = house1.GetMiddleLongitude();
            var house1Sign = KhavedamshaSignAtLongitude(house1MiddleLongitude);

            //count to specified house sign from 1st house sign
            var houseSign = Calculate.SignCountedFromInputSign(house1Sign.GetSignName(), (int)houseNumber);

            //return the name of house sign
            return new ZodiacSign(houseSign, house1Sign.GetDegreesInSign());
        }



        //------------ D45 : Aksha-vedamsha ------------
        /// <summary>
        /// D45 chart
        /// </summary>
        public static ZodiacSign PlanetAkshavedamshaD45Sign(PlanetName planetName, Time time) => Calculate.AkshavedamshaSignName(Calculate.PlanetRasiD1Sign(planetName, time));

        /// <summary>
        /// D45 chart
        /// </summary>
        public static ZodiacSign AkshavedamshaSignName(ZodiacSign zodiacSign) => Vargas.VargasCoreCalculator(zodiacSign, Vargas.AkshavedamshaTable, 45);

        /// <summary>
        /// Given a longitude will return Akshavedamsha (D45) sign at that longitude
        /// </summary>
        public static ZodiacSign AkshavedamshaSignAtLongitude(Angle longitude) => AkshavedamshaSignName(ZodiacSignAtLongitude(longitude));

        /// <summary>
        /// Gets the Akshavedamsha (D45) sign at middle longitude of the house with degrees data
        /// </summary>
        public static ZodiacSign HouseAkshavedamshaD45Sign(HouseName houseNumber, Time time)
        {
            //get all houses
            var allHouses = AllHouseLongitudes(time);

            //get sign of the first house (lagna)
            var house1 = allHouses.Find(house => house.GetHouseName() == HouseName.House1);
            var house1MiddleLongitude = house1.GetMiddleLongitude();
            var house1Sign = AkshavedamshaSignAtLongitude(house1MiddleLongitude);

            //count to specified house sign from 1st house sign
            var houseSign = Calculate.SignCountedFromInputSign(house1Sign.GetSignName(), (int)houseNumber);

            //return the name of house sign
            return new ZodiacSign(houseSign, house1Sign.GetDegreesInSign());
        }



        //------------ D60 : Shashtyamsha ------------
        /// <summary>
        /// D60 chart
        /// </summary>
        public static ZodiacSign PlanetShashtyamshaD60Sign(PlanetName planetName, Time time) => Calculate.ShashtyamshaSignName(Calculate.PlanetRasiD1Sign(planetName, time));

        /// <summary>
        /// D60 chart
        /// </summary>
        public static ZodiacSign ShashtyamshaSignName(ZodiacSign zodiacSign) => Vargas.VargasCoreCalculator(zodiacSign, Vargas.ShashtyamshaTable, 60);

        /// <summary>
        /// Given a longitude will return Shashtyamsha (D60) sign at that longitude
        /// </summary>
        public static ZodiacSign ShashtyamshaSignAtLongitude(Angle longitude) => ShashtyamshaSignName(ZodiacSignAtLongitude(longitude));

        /// <summary>
        /// Gets the Shashtyamsha (D60) sign at middle longitude of the house with degrees data
        /// </summary>
        public static ZodiacSign HouseShashtyamshaD60Sign(HouseName houseNumber, Time time)
        {
            //get all houses
            var allHouses = AllHouseLongitudes(time);

            //get sign of the first house (lagna)
            var house1 = allHouses.Find(house => house.GetHouseName() == HouseName.House1);
            var house1MiddleLongitude = house1.GetMiddleLongitude();
            var house1Sign = ShashtyamshaSignAtLongitude(house1MiddleLongitude);

            //count to specified house sign from 1st house sign
            var houseSign = Calculate.SignCountedFromInputSign(house1Sign.GetSignName(), (int)houseNumber);

            //return the name of house sign
            return new ZodiacSign(houseSign, house1Sign.GetDegreesInSign());
        }

        /// <summary>
        /// Gets list of all planets and the zodiac signs they are in based on house longitudes
        /// </summary>
        public static Dictionary<PlanetName, ZodiacSign> AllPlanetSignsBasedOnHouseLongitudes(Time time) => All9Planets.ToDictionary(planet => planet, planet => PlanetZodiacSignBasedOnHouseLongitudes(planet, time));

        /// <summary>
        /// Gets list of all planets and the zodiac signs they are in
        /// </summary>
        public static Dictionary<PlanetName, ZodiacSign> AllPlanetRasiSigns(Time time) => All9Planets.ToDictionary(planet => planet, planet => PlanetRasiD1Sign(planet, time));
        public static Dictionary<PlanetName, ZodiacSign> AllPlanetHoraSign(Time time) => All9Planets.ToDictionary(planet => planet, planet => PlanetHoraD2Signs(planet, time));
        public static Dictionary<PlanetName, ZodiacSign> AllPlanetDrekkanaSign(Time time) => All9Planets.ToDictionary(planet => planet, planet => PlanetDrekkanaD3Sign(planet, time));
        public static Dictionary<PlanetName, ZodiacSign> AllPlanetChaturthamsaSign(Time time) => All9Planets.ToDictionary(planet => planet, planet => PlanetChaturthamshaD4Sign(planet, time));
        public static Dictionary<PlanetName, ZodiacSign> AllPlanetSaptamshaSign(Time time) => All9Planets.ToDictionary(planet => planet, planet => PlanetSaptamshaD7Sign(planet, time));
        public static Dictionary<PlanetName, ZodiacSign> AllPlanetNavamshaSign(Time time) => All9Planets.ToDictionary(planet => planet, planet => PlanetNavamshaD9Sign(planet, time));
        public static Dictionary<PlanetName, ZodiacSign> AllPlanetDashamamshaSign(Time time) => All9Planets.ToDictionary(planet => planet, planet => PlanetDashamamshaD10Sign(planet, time));
        public static Dictionary<PlanetName, ZodiacSign> AllPlanetDwadashamshaSign(Time time) => All9Planets.ToDictionary(planet => planet, planet => PlanetDwadashamshaD12Sign(planet, time));
        public static Dictionary<PlanetName, ZodiacSign> AllPlanetShodashamshaSign(Time time) => All9Planets.ToDictionary(planet => planet, planet => PlanetShodashamshaD16Sign(planet, time));
        public static Dictionary<PlanetName, ZodiacSign> AllPlanetVimshamshaSign(Time time) => All9Planets.ToDictionary(planet => planet, planet => PlanetVimshamshaD20Sign(planet, time));
        public static Dictionary<PlanetName, ZodiacSign> AllPlanetChaturvimshamshaSign(Time time) => All9Planets.ToDictionary(planet => planet, planet => PlanetChaturvimshamshaD24Sign(planet, time));
        public static Dictionary<PlanetName, ZodiacSign> AllPlanetBhamshaSign(Time time) => All9Planets.ToDictionary(planet => planet, planet => PlanetBhamshaD27Sign(planet, time));
        public static Dictionary<PlanetName, ZodiacSign> AllPlanetTrimshamshaSign(Time time) => All9Planets.ToDictionary(planet => planet, planet => PlanetTrimshamshaD30Sign(planet, time));
        public static Dictionary<PlanetName, ZodiacSign> AllPlanetKhavedamshaSign(Time time) => All9Planets.ToDictionary(planet => planet, planet => PlanetKhavedamshaD40Sign(planet, time));
        public static Dictionary<PlanetName, ZodiacSign> AllPlanetAkshavedamshaSign(Time time) => All9Planets.ToDictionary(planet => planet, planet => PlanetAkshavedamshaD45Sign(planet, time));
        public static Dictionary<PlanetName, ZodiacSign> AllPlanetShashtyamshaSign(Time time) => All9Planets.ToDictionary(planet => planet, planet => PlanetShashtyamshaD60Sign(planet, time));



        #endregion

        #region TAJIKA

        /// <summary>
        /// Gets a given planet's Tajika Longitude
        /// </summary>
        /// <param name="scanYear">4 digit year number</param>
        public static Angle PlanetTajikaLongitude(PlanetName planetName, Time birthTime, int scanYear)
        {
            //based on birth sun sign find next date with exact sign for given year
            var possibleTajika = Calculate.TajikaDateForYear(birthTime, scanYear);

            //once found, use that date to get niryana longitude for asked for planet
            var tajikaLongitude = Calculate.PlanetNirayanaLongitude(planetName, possibleTajika);

            return tajikaLongitude;
        }

        /// <summary>
        /// Gets a given planet's Tajika constellation
        /// </summary>
        /// <param name="scanYear">4 digit year number</param>
        public static Constellation PlanetTajikaConstellation(PlanetName planetName, Time birthTime, int scanYear)
        {
            //get position of planet in longitude
            var planetLongitude = PlanetTajikaLongitude(planetName, birthTime, scanYear);

            //return the constellation behind the planet
            return ConstellationAtLongitude(planetLongitude);

        }

        /// <summary>
        /// Gets a given planet's Tajika zodiac sign
        /// </summary>
        /// <param name="scanYear">4 digit year number</param>
        public static ZodiacSign PlanetTajikaZodiacSign(PlanetName planetName, Time birthTime, int scanYear)
        {
            //get position of planet in longitude
            var planetLongitude = PlanetTajikaLongitude(planetName, birthTime, scanYear);

            //return the constellation behind the planet
            return ZodiacSignAtLongitude(planetLongitude);
        }

        /// <summary>
        /// Annual or Progressed Horoscope
        /// The annual
        /// or progressed horoscope (sidereal solar return according to Western astrology) is cast the same way as the
        /// birth horoscope. The time of the commencement of
        /// the anniversary, known as Varsharambha, is said to
        /// begin at the exact moment when the Sun comes to
        /// the same position he was in at the time of birth. In
        /// other words the individual's New Year begins when
        /// the Sun comes back to the same point he heJd at the·
        /// time of birth. 
        /// Given a birth time and scan year, will return exact time for tajika chart
        /// The tājika system attempts to predict in detail the likely happenings in one year of
        /// an individual's life. The system goes to such details as to predict events even on a
        /// day-by-day basis or even half-a-day. On account of this,
        /// this system is also called the varṣaphala system.
        /// </summary>
        /// <param name="scanYear">4 digit year number</param>
        public static Time TajikaDateForYear2(Time birthTime, int scanYear)
        {

            //CACHE MECHANISM
            return CacheManager.GetCache(new CacheKey(nameof(TajikaDateForYear2), birthTime, scanYear, Ayanamsa), _tajikaDateForYear);


            //UNDERLYING FUNCTION

            Time _tajikaDateForYear()
            {
                //get position of sun on birth
                var sunBirthSign = SunSign(birthTime);

                //scan to find next time sun will be in same sign as birth for given year
                //(not overall longitude only same sign and degree in sign)
                var tajikaDateFound = false;

                //NOTE: to speed up computation time, only start scan 5 days before birth date
                //      this assumes that all tajika dates will only occure +/-5 days from birthday
                var birthDateYear = new Time($"00:00 {birthTime.StdDateText()}/{birthTime.StdMonthText()}/{scanYear} {birthTime.StdTimezoneText}", birthTime.GetGeoLocation());
                var possibleTajika = birthDateYear.SubtractHours(Tools.DaysToHours(5));

                while (!tajikaDateFound)
                {
                    //get sun sign at possible date
                    var possibleSunSign = Calculate.SunSign(possibleTajika);

                    //if found
                    var nameMatch = sunBirthSign.GetSignName() == possibleSunSign.GetSignName();
                    var degreesInSign = sunBirthSign.GetDegreesInSign().TotalDegrees;
                    var inSign = possibleSunSign.GetDegreesInSign().TotalDegrees;
                    var tolerance = 0.008; // Tolerance in degrees
                    var degreesMatch = Math.Abs(degreesInSign - inSign) <= tolerance;

                    //date found, can stop looking
                    if (nameMatch && degreesMatch)
                    {
                        tajikaDateFound = true;
                    }

                    //not found, keep looking
                    else
                    {
                        //NOTE : The sun moves across the zodiac at a rate of approximately 0.3 hours per minute.
                        //as such to be optimal we scan every 0.3 hours, to achive "DMS" minute level accuracy match
                        possibleTajika = possibleTajika.AddHours(0.3);
                    }

                }

                //possible date confirmed as correct date
                return possibleTajika;
            }

        }

        /// <summary>
        /// Annual or Progressed Horoscope
        /// The annual
        /// or progressed horoscope (sidereal solar return according to Western astrology) is cast the same way as the
        /// birth horoscope. The time of the commencement of
        /// the anniversary, known as Varsharambha, is said to
        /// begin at the exact moment when the Sun comes to
        /// the same position he was in at the time of birth. In
        /// other words the individual's New Year begins when
        /// the Sun comes back to the same point he heJd at the·
        /// time of birth. 
        /// Calculated based on method in BV Raman book "Varshaphala" 
        /// </summary>
        /// <param name="scanYear">4 digit year number</param>
        public static Time TajikaDateForYear(Time birthTime, int scanYear)
        {
            //This method of calculating the
            // Varshaphal horoscope (also called sidereal solar return
            // chart) is based on the modern value of the duration of
            // the sidereal year, viz., 365.256374 days or roughJy
            // 365 days, 6 hours, 9 minutes and 12 seconds differing from. the Hindu sidereal year by ~.5 vighatis or
            // 3 minutes and 24 seconds. A study· of a number of
            // annual charts for over 30 years has convinced me
            // that the modern value of the sidereal year would yield
            // better results. H

            //CACHE MECHANISM
            return CacheManager.GetCache(new CacheKey(nameof(TajikaDateForYear), birthTime, scanYear, Ayanamsa), _tajikaDateForYear);


            //UNDERLYING FUNCTION

            Time _tajikaDateForYear()
            {

                //Below data was culled out from pg 22 of Varshaphala-Hindu Progressed Horoscope - BV. RAMAN
                var records = new Dictionary<int, Dictionary<string, int>>()
                {
                    { 1, new Dictionary<string, int>(){ {"Days", 1}, {"Hrs", 6}, {"Mts", 9}, {"Secs", 12} }},
                    { 2, new Dictionary<string, int>(){ {"Days", 2}, {"Hrs", 12}, {"Mts", 18}, {"Secs", 18} }},
                    { 3, new Dictionary<string, int>(){ {"Days", 3}, {"Hrs", 18}, {"Mts", 27}, {"Secs", 30} }},
                    { 4, new Dictionary<string, int>(){ {"Days", 5}, {"Hrs", 0}, {"Mts", 36}, {"Secs", 36} }},
                    { 5, new Dictionary<string, int>(){ {"Days", 6}, {"Hrs", 6}, {"Mts", 45}, {"Secs", 48} }},
                    { 6, new Dictionary<string, int>(){ {"Days", 0}, {"Hrs", 12}, {"Mts", 55}, {"Secs", 0} }},
                    { 7, new Dictionary<string, int>(){ {"Days", 1}, {"Hrs", 19}, {"Mts", 4}, { "Secs", 6} }},
                    { 8, new Dictionary<string, int>(){ {"Days", 3}, {"Hrs", 1}, {"Mts", 13}, { "Secs", 18} }},
                    { 9, new Dictionary<string, int>(){ {"Days", 4}, {"Hrs", 7}, {"Mts", 22}, { "Secs", 30} }},
                    { 10, new Dictionary<string, int>(){ {"Days", 5}, {"Hrs", 13}, {"Mts", 31}, { "Secs", 36} }},
                    { 20, new Dictionary<string, int>(){ {"Days", 4}, {"Hrs", 13}, {"Mts", 3}, { "Secs", 12} }},
                    { 30, new Dictionary<string, int>(){ {"Days", 2}, {"Hrs", 16}, {"Mts", 34}, { "Secs", 54} }},
                    { 40, new Dictionary<string, int>(){ {"Days", 1}, {"Hrs", 6}, {"Mts", 6}, { "Secs", 30} }},
                    { 50, new Dictionary<string, int>(){ {"Days", 6}, {"Hrs", 19}, {"Mts", 38}, { "Secs", 6} }},
                    { 60, new Dictionary<string, int>(){ {"Days", 5}, {"Hrs", 9}, {"Mts", 9}, { "Secs", 42} }},
                    { 70, new Dictionary<string, int>(){ {"Days", 3}, {"Hrs", 22}, {"Mts", 41}, { "Secs", 24} }},
                    { 80, new Dictionary<string, int>(){ {"Days", 2}, {"Hrs", 12}, {"Mts", 13}, { "Secs", 00} }},
                    { 90, new Dictionary<string, int>(){ {"Days", 1}, {"Hrs", 1}, { "Mts", 44 }, { "Secs", 36} }},
                    { 100, new Dictionary<string, int>(){ {"Days", 6}, {"Hrs", 15}, { "Mts", 13 }, { "Secs", 12} }}
                };

                throw new Exception();
            }

        }


        #endregion

        #region TRANSITS

        public static HouseName TransitHouseFromLagna(PlanetName transitPlanet, Time checkTime, Time birthTime)
        {
            //Note the Lagna Rashi.
            var lagnaRasi = Calculate.LagnaSignName(birthTime);

            //Choose the planet transit result for which predictions to be made.

            //Note the transit position of the Moon with reference to
            //Natal Moon(Janma Rashi) when the chosen planet enters a new sign.
            var transitRasi = PlanetRasiD1Sign(transitPlanet, checkTime);
            var count = Calculate.CountFromSignToSign(lagnaRasi, transitRasi.GetSignName());

            return (HouseName)count;

        }

        public static HouseName TransitHouseFromNavamsaLagna(PlanetName transitPlanet, Time checkTime, Time birthTime)
        {
            //Note the Lagna Rashi.
            var navamsaLagnaRasi = Calculate.HouseNavamshaD9Sign(HouseName.House1, birthTime).GetSignName();

            //Choose the planet transit result for which predictions to be made.

            //Note the transit position of the Moon with reference to
            //Natal Moon(Janma Rashi) when the chosen planet enters a new sign.
            var transitRasi = PlanetRasiD1Sign(transitPlanet, checkTime);
            var count = Calculate.CountFromSignToSign(navamsaLagnaRasi, transitRasi.GetSignName());

            return (HouseName)count;

        }

        public static HouseName TransitHouseFromMoon(PlanetName transitPlanet, Time checkTime, Time birthTime)
        {
            //Note the Janma Rashi.
            var janmaRasi = PlanetRasiD1Sign(Moon, birthTime);

            //Choose the planet transit result for which predictions to be made.
            //Note the transit position of the Moon with reference to
            //Natal Moon(Janma Rashi) when the chosen planet enters a new sign.
            var transitRasi = PlanetRasiD1Sign(transitPlanet, checkTime);
            var count = Calculate.CountFromSignToSign(janmaRasi.GetSignName(), transitRasi.GetSignName());

            return (HouseName)count;
        }

        public static HouseName TransitHouseFromNavamsaMoon(PlanetName transitPlanet, Time checkTime, Time birthTime)
        {
            //Note the Janma Rashi.
            var janmaRasi = Calculate.PlanetNavamshaD9Sign(Moon, birthTime).GetSignName();

            //Choose the planet transit result for which predictions to be made.
            //Note the transit position of the Moon with reference to
            //Natal Moon(Janma Rashi) when the chosen planet enters a new sign.
            var transitRasi = PlanetRasiD1Sign(transitPlanet, checkTime);
            var count = Calculate.CountFromSignToSign(janmaRasi, transitRasi.GetSignName());

            return (HouseName)count;
        }

        public static string Murthi(PlanetName transitPlanet, Time checkTime, Time birthTime)
        {
            return "";

            //if moon retun no murthi
            if (transitPlanet == Moon) { return ""; }

            //Note the Janma Rashi.
            var janmaRasi = PlanetRasiD1Sign(Moon, birthTime);

            //Choose the planet transit result for which predictions to be made.

            //Note the transit position of the Moon with reference to
            //Natal Moon(Janma Rashi) when the chosen planet enters a new sign.
            var transitRasi = PlanetRasiD1Sign(transitPlanet, checkTime);
            var count = Calculate.CountFromSignToSign(janmaRasi.GetSignName(), transitRasi.GetSignName());

            //Name the Moorti as follows:- •

            //If the transit Moon is in 1st, 6th or 11th from Natal Moon – Swarna(Golden) Moorti.
            //If it is in 2nd, 5th or 9th – Rajata(Silver) Moorti.
            //If it is in 3rd, 7th or 10th – Tamra(Copper) Moorti.
            //If it is in 4th, 8th or 12th – Loha(Iron) Moorti.

            throw new Exception("");

        }

        #endregion

        #region PANCHA PAKSHI

        /// <summary>
        /// In each of the main activities, the other four activities also occur as
        /// abstract sub-activity for short duration of time gaps covering the complete
        /// duration of the main activity, the period being 2 hrs. 24 min
        /// for Pancha Pakshi
        /// </summary>
        public static BirdActivity AbstractActivity(Time checkTime)
        {
            //start counting from start of current Yama
            var yamaStartTime = BirthYama(checkTime).YamaStartTime;

            //based on day or night birth start checking
            if (IsDayBirth(checkTime))
            {
                //total minutes is 2h 24min
                var daySubTimings = new Dictionary<BirdActivity, double>()
                {
                    {BirdActivity.Eating, 30},
                    {BirdActivity.Walking, 36},
                    {BirdActivity.Ruling, 48},
                    {BirdActivity.Sleeping, 18},
                    {BirdActivity.Dying, 12}
                };

                //find which sub activity given time falls under
                foreach (var timing in daySubTimings)
                {
                    //calculate end time for this yama
                    var subYamaSpanMin = timing.Value;
                    var yamaEndTime = yamaStartTime.AddHours(Tools.MinutesToHours(subYamaSpanMin));

                    //if birth time is in this sub-yama, found! end here.
                    //(start time must be smaller and time must be bigger)
                    if (yamaStartTime < checkTime && yamaEndTime > checkTime)
                    {
                        return timing.Key; //bird name
                    }

                    //since not found
                    //keep looking, end of this yama begins next
                    yamaStartTime = yamaEndTime;
                }
            }
            //night birth
            else
            {
                //total minutes is 2h 24min
                var nightSubTimings = new Dictionary<BirdActivity, double>()
                {
                    {BirdActivity.Eating, 30},
                    {BirdActivity.Ruling, 24},
                    {BirdActivity.Dying, 36},
                    {BirdActivity.Walking, 30},
                    {BirdActivity.Sleeping, 24}
                };

                //find which sub activity given time falls under this sub yama
                foreach (var timing in nightSubTimings)
                {
                    //calculate end time for this yama
                    var subYamaSpanMin = timing.Value;
                    var yamaEndTime = yamaStartTime.AddHours(Tools.MinutesToHours(subYamaSpanMin));

                    //if birth time is in this sub-yama, found! end here.
                    //(start time must be smaller and time must be bigger)
                    if (yamaStartTime < checkTime && yamaEndTime > checkTime)
                    {
                        return timing.Key; //bird name
                    }

                    //since not found
                    //keep looking, end of this yama begins next
                    yamaStartTime = yamaEndTime;
                }
            }


            throw new Exception("END OF LINE!");
        }

        /// <summary>
        /// Each bird performs these five activities during each day
        /// and in night over the week days and during waxing and
        /// waning Moon cycles during the 5 YAMAS in day and 5
        /// YAMAS in night in a stipulated order
        /// for Pancha Pakshi
        /// </summary>
        public static BirdActivity MainActivity(Time birthTime, Time checkTime)
        {

            // Determine the bird's type and its current main and abstract activities.
            var birthBird = PanchaPakshiBirthBird(birthTime);
            var timeOfDay = IsDayBirth(checkTime) ? PanchaPakshi.TimeOfDay.Day : PanchaPakshi.TimeOfDay.Night;
            var dayOfWeek = DayOfWeek(checkTime);
            var yamaNumber = BirthYama(checkTime).YamaCount;

            // Retrieve the strength of the bird's abstract activity from the pre-initialized dictionary.
            var mainActivity = PanchaPakshi.TableData[timeOfDay][dayOfWeek][yamaNumber][birthBird];
            return mainActivity;

        }

        /// <summary>
        /// These 5 elemental vibrations act in 5 gradations offaculties for stipulated time
        /// intervals called (YAMAS) consisting of
        /// 2 hrs. 24 mits. each (6 Ghatikas each) over the 5 YAMAS in
        /// the day and 5 YAMAS in the night, thus spread over evenly in
        /// 24 hours.
        /// </summary>
        public static BirthYama BirthYama(Time inputTime)
        {
            //get the vedic day start time for given input time (aka sunrise)
            var dayStartVedic = Calculate.VedicDayStartTime(inputTime);

            //get start of vedic day and start checking 1 yama range at a time
            var isFound = false;
            var yamaCount = 1;
            var yamaStartTime = dayStartVedic;
            while (!isFound)
            {
                //calculate yama end time based on yama count (2h 24min = 2.4h)
                //var minFromStart = 2.4 * yamaCount;
                var yamaEndTime = yamaStartTime.AddHours(2.4); // Changed this line

                //if birth time is in this yama, found! end here.
                //(start time must be smaller or equal and end time must be bigger or equal)
                if (yamaStartTime <= inputTime && yamaEndTime >= inputTime)
                {
                    //if above 5, restart Yama count for night cycle
                    if (yamaCount > 5) { yamaCount = yamaCount - 5; }

                    return new BirthYama(yamaCount, yamaStartTime, yamaEndTime);
                }

                //keep looking, end of this yama begins next
                yamaStartTime = yamaEndTime;

                yamaCount++;
            }

            throw new Exception("END OF LINE!");

        }

        /// <summary>
        /// Given a time, it will find out the start time of for that vedic day
        /// If time is before sunrise, the previous day
        /// </summary>
        public static Time VedicDayStartTime(Time inputTime)
        {
            var sunrise = Calculate.SunriseTime(inputTime);

            //#PREVIOUS DAY
            //time should be before sunrise (sunrise time will be bigger)
            if (sunrise > inputTime)
            {
                var previousDay = inputTime.SubtractHours(23); //todo proper method
                var yamaStartTime = Calculate.SunriseTime(previousDay);
                return yamaStartTime;
            }
            //else return sunrise as is
            else
            {
                return sunrise;
            }


        }



        /// <summary>
        /// yama works out to 2 hrs. 24 mts. of our modern time.
        /// It is to be noted that the beginning of the day is
        /// reckoned from Sun rise to Sun set in Hindu system. Similarly
        /// night is reckoned from Sun set to Sun rise on the following
        /// day, thus consisting of 24 hours for one day.
        /// The timings of the five Yamas are the same during day
        /// and night
        /// for Pancha Pakshi
        /// </summary>


        /// <summary>
        /// Calculates the strength of a bird's "Abstract" activity (sub activity) based on its birth time.
        /// for pancha pakshi bird
        /// </summary>
        /// <param name="birthTime">The bird's birth time :D</param>
        /// <returns>The strength of the bird's activity.</returns>
        public static double AbstractActivityStrength(Time birthTime, Time checkTime)
        {
            // Determine the bird's type and its current main and abstract activities.
            var birthBird = PanchaPakshiBirthBird(birthTime);
            var mainActivity = MainActivity(birthTime, checkTime);
            var abstractActivity = AbstractActivity(checkTime);

            // Retrieve the strength of the bird's abstract activity from the pre-initialized dictionary.
            return PanchaPakshi.AbstractActivityStrengthTable[birthBird][mainActivity][abstractActivity];
        }

        /// <summary>
        /// Gets "birth bird" for a birth time.
        /// Sidhas have personified the elements as birds identifying each element under
        /// which an individual is born, when these elements are all functioning differentially
        /// during each time gap. These 5 elemental vibrations are personified as PAKSHIS or BIRDS and the
        /// gradations of their faculities are named as 5 activities.
        /// This bird is called his birth Stellar Lunar bird.
        /// </summary>
        public static BirdName PanchaPakshiBirthBird(Time birthTime)
        {
            //get rulling constellation
            var rullingConst = Calculate.MoonConstellation(birthTime);
            var rullingConstNumber = (int)rullingConst.GetConstellationName();

            //based on waxing or waning assign bird accordingly
            var isWaxing = Calculate.IsWaxingMoon(birthTime);
            if (isWaxing)
            {
                switch (rullingConstNumber)
                {
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                        return BirdName.Vulture;
                    case 6:
                    case 7:
                    case 8:
                    case 9:
                    case 10:
                    case 11:
                        return BirdName.Owl;
                    case 12:
                    case 13:
                    case 14:
                    case 15:
                    case 16:
                        return BirdName.Crow;
                    case 17:
                    case 18:
                    case 19:
                    case 20:
                    case 21:
                        return BirdName.Cock;
                    case 22:
                    case 23:
                    case 24:
                    case 25:
                    case 26:
                    case 27:
                        return BirdName.Peacock;
                }
            }
            //else must be wanning
            else
            {
                switch (rullingConstNumber)
                {
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                        return BirdName.Peacock;
                    case 6:
                    case 7:
                    case 8:
                    case 9:
                    case 10:
                    case 11:
                        return BirdName.Cock;
                    case 12:
                    case 13:
                    case 14:
                    case 15:
                    case 16:
                        return BirdName.Crow;
                    case 17:
                    case 18:
                    case 19:
                    case 20:
                    case 21:
                        return BirdName.Owl;
                    case 22:
                    case 23:
                    case 24:
                    case 25:
                    case 26:
                    case 27:
                        return BirdName.Vulture;
                }
            }

            throw new Exception("END OF LINE!");
        }

        /// <summary>
        /// Ancients have evolved a method of identifying the birth bird of
        /// other individuals by recognising the first
        /// vowel sound that shoots out while uttering the name of such
        /// individual. Here, we have to be
        /// very careful in identifying the first vowel sound (and not the
        /// first vowel letter) ofthe other man's name. In this system, the
        /// vowels referred to are ofthe Dravidian Origin TAMIL and do
        /// not indicate the English vowel sounds. This should always be
        /// borne in mind.
        /// It should
        /// be remembered that the eleven vowels of Dravidian Tamil
        /// language are distributed among the 5 birds. These vowels and
        /// consonants which contain them are to be identified from the
        /// first sound of the name. Virtually, these eleven vowel sounds
        /// are to be equated and sounded by the five English vowels A, E,
        /// I, O and U. In this language "U" is uttered as "V + U = VU",
        /// to project the Dravidian sound. Except the sound "I", all
        /// other sounds have short and long vowels.
        ///
        /// From what has been explained so far, it can be understood
        /// that for the same name, the birds are different during bright
        /// half and dark halfperiods of Moon where we do not know the
        /// birth data of the other person and for such persons only we
        /// should use this system
        /// </summary>
        /// <param name="name">a popular name and known by that name only</param>
        public static BirdName PanchaPakshiBirthBirdFromName(string name)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Given a time will return true if it is on
        /// "Waxing moon" or "Shukla Paksha" or "Bright half"
        /// </summary>
        public static bool IsWaxingMoon(Time birthTime)
        {
            var lunarDay = LunarDay(birthTime);

            return lunarDay.GetMoonPhase() == MoonPhase.BrightHalf;
        }

        /// <summary>
        /// Given a time will return true if it is on
        /// "Waning moon" or "Krishna Paksha" or "Dark half"
        /// </summary>
        public static bool IsWaningMoon(Time birthTime)
        {
            var lunarDay = LunarDay(birthTime);

            return lunarDay.GetMoonPhase() == MoonPhase.DarkHalf;
        }

        /// <summary>
        /// Given a name will extract out the 1st vowel sound.
        /// Used to get Pancha Pakshi bird when birth date not known
        /// </summary>
        public static string FirstVowelSound(string word)
        {
            HashSet<char> Vowels = new HashSet<char>("aeiouAEIOU");
            Dictionary<string, string> ConsecutiveVowelMap = new Dictionary<string, string>()
            {
                { "AI", "I" },
                { "AE", "A" },
                { "AO", "A" },
                { "AU", "A" },
                { "EZ", "EA" },
                { "JA", "EA" },
                { "PE", "EA" },
                { "ES", "EA" },
                { "EI", "E" },
                { "MI", "E" },
                { "EA", "E" },
                { "EO", "E" },
                { "EU", "E" },
                { "IA", "I" },
                { "IE", "I" },
                { "IO", "I" },
                { "IU", "I" },
                { "OA", "O" },
                { "OE", "O" },
                { "OI", "O" },
                { "OU", "OW" },
                { "OP", "O" },
                { "UA", "U" },
                { "UE", "U" },
                { "UI", "U" },
                { "UO", "U" }
            };


            // Remove non-letter characters from the input word
            var cleanedWord = new string(word.Where(Char.IsLetter).ToArray());

            // Split the cleaned word into syllables
            var syllables = Syllables(cleanedWord);

            for (int i = 0; i < syllables.Count; i++)
            {
                var syllableToCheck = syllables[i];

                // If the syllable starts with a vowel or contains consecutive vowels at its end and beginning, handle it accordingly
                if (IsVowel(syllableToCheck) || (i < syllables.Count - 1 && IsVowel(syllableToCheck[^1].ToString()) && IsVowel(syllables[i + 1][0].ToString())))
                {
                    // Use the lookup table if there are two consecutive vowels at the end and beginning of adjacent syllables
                    if (i < syllables.Count - 1 && IsVowel(syllableToCheck[^1].ToString()) && IsVowel(syllables[i + 1][0].ToString()))
                    {
                        var firstVowelSound = syllableToCheck[^1].ToString() + syllables[i + 1][0];

                        if (ConsecutiveVowelMap.TryGetValue(firstVowelSound.ToUpper(), out string mappedValue2))
                        {
                            return mappedValue2;
                        }

                        return firstVowelSound;
                    }

                    // Use the lookup table for syllables that only contain vowels
                    if (IsVowel(syllableToCheck) && ConsecutiveVowelMap.TryGetValue(syllableToCheck.ToUpper(), out string mappedValue))
                    {
                        return mappedValue;
                    }

                    // Return the last vowel sound if it's not followed by another vowel in the next syllable
                    if (i < syllables.Count - 1 && IsVowel(syllableToCheck[^1].ToString()) && !IsVowel(syllables[i + 1][0].ToString()))
                    {
                        return syllableToCheck[^1].ToString();
                    }

                    // Otherwise, just return the syllable itself
                    return syllableToCheck;
                }
            }

            return "";


            List<string> Syllables(string word)
            {
                var vowels = new HashSet<char> { 'a', 'e', 'i', 'o', 'u' };
                var syllables = new List<string>();
                var currentSyllable = new StringBuilder();

                for (int i = 0; i < word.Length; i++)
                {
                    currentSyllable.Append(word[i]);
                    if (vowels.Contains(char.ToLower(word[i])))
                    {
                        syllables.Add(currentSyllable.ToString());
                        currentSyllable.Clear();

                        if (i < word.Length - 1 && !vowels.Contains(char.ToLower(word[i + 1])))
                        {
                            currentSyllable.Append(word[i + 1]);
                            i++;
                        }
                    }
                }

                if (currentSyllable.Length > 0)
                {
                    syllables.Add(currentSyllable.ToString());
                }

                // Combine single character vowel syllables
                for (int i = 0; i < syllables.Count - 1; i++)
                {
                    if (syllables[i].Length == 1 && syllables[i + 1].Length == 1 && vowels.Contains(char.ToLower(syllables[i][0])) && vowels.Contains(char.ToLower(syllables[i + 1][0])))
                    {
                        syllables[i] += syllables[i + 1];
                        syllables.RemoveAt(i + 1);
                        i--;
                    }
                }

                return syllables;
            }

            bool IsVowel(string syllable)
            {
                return syllable.Any(c => "aeiou".Contains(char.ToLower(c)));
            }

        }



        #endregion

        #region PANCHANGA

        /// <summary>
        /// It’s used to determine auspicious times and rituals.
        /// It includes multiple attributes such as,
        /// Tithi (lunar day),
        /// Lunar Month
        /// Vara (weekday),
        /// Nakshatra (constellation),
        /// Yoga (luni-solar day) and Karana (half of a Tithi).
        /// Disha Shool
        /// </summary>
        public static PanchangaTable PanchangaTable(Time inputTime)
        {
            //Ayanamsa
            var ayanamsaDegree = Calculate.AyanamsaDegree(inputTime).DegreesMinutesSecondsText;

            //Tithi (lunar day)
            var tithi = Calculate.LunarDay(inputTime);

            //lunar month
            var lunarMonth = Calculate.LunarMonth(inputTime);

            //Vara (weekday)
            var weekDay = Calculate.DayOfWeek(inputTime);

            //Nakshatra (constellation)
            var constellation = Calculate.MoonConstellation(inputTime);

            //Yoga (luni-solar day) 
            var yoga = Calculate.NithyaYoga(inputTime);

            //Karana (half of a Tithi)
            var karana = Calculate.Karana(inputTime);

            //Hora Lord
            var horaLord = Calculate.LordOfHoraFromTime(inputTime);

            //Disha Shool
            var dishaShool = Calculate.DishaShool(inputTime);

            //Sunrise
            var sunrise = Calculate.SunriseTime(inputTime);

            //Sunset
            var sunset = Calculate.SunsetTime(inputTime);

            //Ishta Kaala
            var ishtaKaala = Calculate.IshtaKaala(inputTime);

            return new PanchangaTable(ayanamsaDegree, tithi, lunarMonth, weekDay, constellation, yoga, karana, horaLord, dishaShool, sunrise, sunset, ishtaKaala);
        }

        /// <summary>
        /// Here are the following Disha shool days and the directions that are considered as
        /// inauspicious or Disha shool. Check the Disha Shool chart to find the inauspicious direction to travel 
        /// </summary>
        public static string DishaShool(Time inputTime)
        {
            //vedic day
            var vedicWeekDay = Calculate.DayOfWeek(inputTime);

            switch (vedicWeekDay)
            {
                case Library.DayOfWeek.Monday: return "East";
                case Library.DayOfWeek.Tuesday: return "North";
                case Library.DayOfWeek.Wednesday: return "North";
                case Library.DayOfWeek.Thursday: return "South";
                case Library.DayOfWeek.Friday: return "West";
                case Library.DayOfWeek.Saturday: return "East";
                case Library.DayOfWeek.Sunday: return "West";
            }

            throw new Exception("END OF LINE!");
        }

        /// <summary>
        /// Also know as Chandramana or Hindu Month.
        /// Each Hindu month begins with the New Moon.
        /// These lunar months go by special names. The name of a lunar month is
        /// decided by the rasi in which Sun-Moon conjunction takes place.
        /// These names come from the constellation that Moon is most likely to
        /// occupy on the full Moon day.
        /// Names are Chaitra, Vaisaakha, Jyeshtha, Aashaadha, Sraavana etc...
        /// </summary>
        public static LunarMonth LunarMonth(Time inputTime, bool ignoreLeapMonth = false)
        {
            //TODO JAN 2024
            //needs further validation, the month before
            //Adhika is also shown as Adhika
            //most test cases pass, but some closser to change dates fail

            //based on vedic start of day (sunrise time)
            //scan and get dates when new moon last occured
            var sunriseTime = Calculate.SunriseTime(inputTime);
            var lastNewMoonRaw = Calculate.PreviousNewMoon(sunriseTime); //for this moon month
            var nextNewMoon = Calculate.NextNewMoon(sunriseTime); //for next moon month

            //get sign as number
            var thisMonthSign = (int)Calculate.MoonSignName(lastNewMoonRaw);
            var nextMonthSign = (int)Calculate.MoonSignName(nextNewMoon);

            //detect leap month if 2 months are same name
            var isLeapMonth = (thisMonthSign == nextMonthSign);

            //increment 1 to convert from rasi to solar month number
            var monthNumber = thisMonthSign + 1;

            //if exceed 12 than loop back to 1
            if (monthNumber > 12) { monthNumber = monthNumber % 12; }

            //verify if really leap month (rescursive)
            //NOTE: this was added later as hack (remove if needed)
            if (isLeapMonth && !ignoreLeapMonth)
            {
                var ccc = Calculate.NextNewMoon(nextNewMoon.AddHours(24));
                var nextNewMoonxx = Calculate.LunarMonth(ccc, true); //NOTE:turn off recursive
                var possibleLeapMonth = ((LunarMonth)monthNumber).ToString();

                //checks if month name is in the next months name (Jyeshtha -> JyeshthaAdhika)
                var vvv = nextNewMoonxx.ToString().Contains(possibleLeapMonth);
                if (!vvv)
                {
                    isLeapMonth = false;
                }
            }

            //based on month number (NOT sign number or constellation)
            //set the name of the lunar month also based on if leap month
            var monthName = Library.LunarMonth.Empty;
            switch (monthNumber)
            {
                case 1: monthName = isLeapMonth ? Library.LunarMonth.ChaitraAdhika : Library.LunarMonth.Chaitra; break;
                case 2: monthName = isLeapMonth ? Library.LunarMonth.VaisaakhaAdhika : Library.LunarMonth.Vaisaakha; break;
                case 3: monthName = isLeapMonth ? Library.LunarMonth.JyeshthaAdhika : Library.LunarMonth.Jyeshtha; break;
                case 4: monthName = isLeapMonth ? Library.LunarMonth.AashaadhaAdhika : Library.LunarMonth.Aashaadha; break;
                case 5: monthName = isLeapMonth ? Library.LunarMonth.SraavanaAdhika : Library.LunarMonth.Sraavana; break;
                case 6: monthName = isLeapMonth ? Library.LunarMonth.BhaadrapadaAdhika : Library.LunarMonth.Bhaadrapada; break;
                case 7: monthName = isLeapMonth ? Library.LunarMonth.AaswayujaAdhika : Library.LunarMonth.Aaswayuja; break;
                case 8: monthName = isLeapMonth ? Library.LunarMonth.KaarteekaAdhika : Library.LunarMonth.Kaarteeka; break;
                case 9: monthName = isLeapMonth ? Library.LunarMonth.MaargasiraAdhika : Library.LunarMonth.Maargasira; break;
                case 10: monthName = isLeapMonth ? Library.LunarMonth.PushyaAdhika : Library.LunarMonth.Pushya; break;
                case 11: monthName = isLeapMonth ? Library.LunarMonth.MaaghaAdhika : Library.LunarMonth.Maagha; break;
                case 12: monthName = isLeapMonth ? Library.LunarMonth.PhaalgunaAdhika : Library.LunarMonth.Phaalguna; break;
            }

            return monthName;
        }

        /// <summary>
        /// Gets next future New Moon date, when tithi will be 1.
        /// Uses conjunctions angle to calculate with accuracy of ~30min
        /// Includes start time in scan
        /// </summary>
        public static Time NextNewMoon(Time inputTime)
        {
            //scan till find
            //start with input time
            var newMoonTime = inputTime;
            while (true)
            {
                //if conjunction, than new moon dectected
                var conjunctAngle = SunMoonConjunctionAngle(newMoonTime);

                //When Sun and Moon are at the same longitude, a new lunar month of 30 tithis starts
                //which conjunction 0 degrees
                if (conjunctAngle.TotalDegrees < 1)
                {
                    return newMoonTime;
                }

                //go foward in time since did not find 0 degree conjunction
                newMoonTime = newMoonTime.AddHours(0.5);
            }

            return newMoonTime;
        }

        /// <summary>
        /// Gets last occured New Moon date, when tithi will be 1.
        /// Uses conjunctions angle to calculate with accuracy of ~30min
        /// Includes start time in scan
        /// </summary>
        public static Time PreviousNewMoon(Time inputTime)
        {
            //scan till find
            //start with input time
            var newMoonTime = inputTime;
            while (true)
            {
                //if conjunction, than new moon dectected
                var conjunctAngle = SunMoonConjunctionAngle(newMoonTime);

                //When Sun and Moon are at the same longitude, a new lunar month of 30 tithis starts
                //which conjunction 0 degrees
                if (conjunctAngle.TotalDegrees < 1)
                {
                    return newMoonTime;
                }

                //go backward in time since did not find 0 degree conjunction
                newMoonTime = newMoonTime.SubtractHours(0.5);
            }
        }

        /// <summary>
        /// Gets the distance in degrees between Sun & Moon at a given time
        /// Used to calculate lunar months.
        /// </summary>
        public static Angle SunMoonConjunctionAngle(Time ccc)
        {
            //longitudes of the sun & moon
            Angle sunLong = PlanetNirayanaLongitude(Sun, ccc);
            Angle moonLong = PlanetNirayanaLongitude(Moon, ccc);

            //get non negative difference, expunge 360 if needed
            var cleanedDifference = moonLong.GetDifference(sunLong).Normalize360();

            return cleanedDifference;
        }

        #endregion

        #region EVENTS CHART

        #endregion

        #region AVASTA

        /// <summary>
        /// Gets all the Avastas for a planet, Lajjita, Garvita, Kshudita, etc...
        /// </summary>
        /// <param name="time">time to base calculation on</param>
        public static List<Avasta> PlanetAvasta(PlanetName planetName, Time time)
        {
            var finalList = new Avasta?[6]; //total 6 avasta

            //add in each avasta that matches
            finalList[0] = IsPlanetInLajjitaAvasta(planetName, time) ? Avasta.LajjitaShamed : null;
            finalList[1] = IsPlanetInGarvitaAvasta(planetName, time) ? Avasta.GarvitaProud : null;
            finalList[2] = IsPlanetInKshuditaAvasta(planetName, time) ? Avasta.KshuditaStarved : null;
            finalList[3] = IsPlanetInTrashitaAvasta(planetName, time) ? Avasta.TrishitaThirst : null;
            finalList[4] = IsPlanetInMuditaAvasta(planetName, time) ? Avasta.MuditaDelighted : null;
            finalList[5] = IsPlanetInKshobhitaAvasta(planetName, time) ? Avasta.KshobitaAgitated : null;

            // Convert array to List<Avasta> and remove nulls
            var resultList = finalList.OfType<Avasta>().ToList();
            return resultList;

        }

        /// <summary>
        /// Lajjita / humiliated : Planet in the 5th house in conjunction with rahu or ketu, Saturn or mars.
        /// </summary>
        /// <param name="time">time to base calculation on</param>
        public static bool IsPlanetInLajjitaAvasta(PlanetName planetName, Time time)
        {
            //check if input planet is in 5th
            var isPlanetIn5thHouse = IsPlanetInHouse(planetName, HouseName.House5, time);

            //check if any negative planets is in 5th (conjunct)
            var planetNames = new List<PlanetName>() { Rahu, Ketu, Saturn, Mars };
            var rahuKetuSaturnMarsIn5th = IsAllPlanetsInHouse(planetNames, HouseName.House5, time);

            //check if all conditions are met Lajjita
            var isLajjita = isPlanetIn5thHouse && rahuKetuSaturnMarsIn5th;

            return isLajjita;

        }

        /// <summary>
        /// Garvita, proud : Planet in exaltation sign or moolatrikona zone, happiness and gains
        /// </summary>
        /// <param name="time">time to base calculation on</param>
        public static bool IsPlanetInGarvitaAvasta(PlanetName planetName, Time time)
        {
            //Planet in exaltation sign
            var planetExalted = IsPlanetExaltedDegree(planetName, time);

            //moolatrikona zone
            var planetInMoolatrikona = IsPlanetInMoolatrikona(planetName, time);

            //check if all conditions are met for Garvita
            var isGarvita = planetExalted || planetInMoolatrikona;

            return isGarvita;
        }

        /// <summary>
        /// Kshudita, hungry : Planet in enemy’s sign or conjoined with enemy or aspected by enemy, Grief
        /// </summary>
        public static bool IsPlanetInKshuditaAvasta(PlanetName planetName, Time time)
        {
            //Planet in enemy’s sign 
            var planetExalted = IsPlanetInEnemyHouse(planetName, time);

            //conjoined with enemy (same house)
            var conjunctWithMalefic = IsPlanetConjunctWithEnemyPlanets(planetName, time);

            //aspected by enemy
            var aspectedByMalefic = IsPlanetAspectedByEnemyPlanets(planetName, time);

            //check if all conditions are met for Kshudita
            var isKshudita = planetExalted || conjunctWithMalefic || aspectedByMalefic;

            return isKshudita;
        }

        /// <summary>
        /// Trashita, thirsty – Planet in a watery sign, aspected by a enemy and is without the aspect of benefic Planets
        /// 
        /// The Planet who being conjoined or aspected by a Malefic or his enemy Planet is situated,
        /// without the aspect of a benefic Planet, in the 4th House is Trashita.
        /// 
        /// Another version
        /// 
        /// If the Planet is situated in a watery sign, is aspected by an enemy Planet and
        /// is without the aspect of benefic Planets he is called Trashita.
        ///
        /// --------
        /// "A planet in a Water Sign and aspected by an enemy planet,
        /// with no auspiscious Graha aspecting is said to be Trishita Avastha/Thirsty State".
        /// 
        /// This state is in effect whenever a planet is in a Water Sign and it gets
        /// aspected by an enemy planet. But if, a Gentle Planet (Mercury/Venus/Moon) aspects here,
        /// it strengthens the planet in Water Sign. This Avastha is only for the aspecting enemy
        /// planet that will cause Trishita/Thirst. This state shows that a planet in a watery
        /// Rasi can still be productive even when aspected by enemies, though it will not be happy.
        /// As the name “Thirsty State” implies, it indicates the lack of emotional fulfillment that a planet experiences.
        /// </summary>
        public static bool IsPlanetInTrashitaAvasta(PlanetName planetName, Time time)
        {
            //Planet in a watery sign
            var planetInWater = IsPlanetInWaterySign(planetName, time);

            //aspected by an enemy
            var aspectedByEnemy = IsPlanetAspectedByEnemyPlanets(planetName, time);

            //no benefic planet aspect
            var noBeneficAspect = false == IsPlanetAspectedByBeneficPlanets(planetName, time);

            //check if all conditions are met for Trashita
            var isTrashita = planetInWater && aspectedByEnemy && noBeneficAspect;

            return isTrashita;
        }

        /// <summary>
        /// The Planet who is in his friend’s sign, is in conjunction with Jupiter,
        /// and is together with or is aspected by a friendly Planet is called Mudita
        /// 
        /// Mudita, sated, happy – Planet in a friend’s sign or aspected by a friend and conjoined with Jupiter, Gains
        ///
        /// If a planet is in a friend’s sign or joined with a friend or aspected by a friend,
        /// or that joined with Jupiter is called Mudita Avastha/Delighted State
        ///
        /// It is clear from explanation itself that a planet will feel delighted when it
        /// is in friendly sign or friendly planet conjuncts/aspects or it is joined by the
        /// biggest benefic planet Jupiter. We can understand planet’s delight in such cases. 
        /// 
        /// Planet in friendly sign - A planet in a friendly sign is productive,
        /// and the stronger that friend planet, the more productive it will be. 
        /// </summary>
        public static bool IsPlanetInMuditaAvasta(PlanetName planetName, Time time)
        {
            //Planet who is in his friend’s sign
            var isInFriendly = IsPlanetInFriendHouse(planetName, time);

            //is in conjunction with Jupiter
            var isConjunctJupiter = IsPlanetConjunctWithPlanet(planetName, Jupiter, time);

            //is together with or is aspected by a friendly (conjunct or aspect)
            var isConjunctWithFriendly = IsPlanetConjunctWithFriendPlanets(planetName, time);
            var isAspectedByFriendly = IsPlanetAspectedByFriendPlanets(planetName, time);
            var accosiatedWithFriendly = isConjunctWithFriendly || isAspectedByFriendly;

            //check if all conditions are met for Mudita
            var isMudita = isInFriendly || isConjunctJupiter || accosiatedWithFriendly;

            return isMudita;
        }

        /// <summary>
        /// If a planet is conjunct by Sun or it is aspected by Enemy Malefic Planets then
        /// it should always be known as Kshobhita Avastha/Agitated State
        /// 
        /// Kshobhita, guilty, repentant – Planet in conjunction with sun and aspected by malefics and an enemy. Penury
        /// </summary>
        public static bool IsPlanetInKshobhitaAvasta(PlanetName planetName, Time time)
        {
            //Planet in conjunction with sun 
            var conjunctWithSun = IsPlanetConjunctWithPlanet(planetName, Sun, time);

            //aspected by an enemy or malefic
            var isAspectedByEnemy = false == IsPlanetAspectedByEnemyPlanets(planetName, time);
            var isAspectedByMalefics = false == IsPlanetAspectedByMaleficPlanets(planetName, time);
            var accosiatedWithBadPlanets = isAspectedByEnemy || isAspectedByMalefics;

            //check if all conditions are met for Kshobhita
            var isKshobhita = conjunctWithSun && accosiatedWithBadPlanets;

            return isKshobhita;
        }

        #endregion

        #region PLANET TRANSITS

        public static List<Tuple<Time, Time, ZodiacName, PlanetName>> PlanetSignTransit(Time startTime, Time endTime, PlanetName planetName)
        {
            //make slices to scan
            var accuracyInHours = 0.05; // 3 minute
            var timeSlices = Time.GetTimeListFromRange(startTime, endTime, accuracyInHours);

            //prepare place to store data
            var returnList = new List<Tuple<Time, Time, ZodiacName, PlanetName>>();

            //get the start sign
            var startZodiacSign = Calculate.PlanetRasiD1Sign(planetName, startTime);
            var previousZodiacName = startZodiacSign.GetSignName();
            var startTimeSlice = timeSlices[0]; //set start slice for 1st change
            foreach (var timeSlice in timeSlices)
            {
                var tempZodiacName = Calculate.PlanetRasiD1Sign(planetName, timeSlice).GetSignName();

                //if constellation changes mark the time as start for one and end for another
                if (tempZodiacName != previousZodiacName)
                {
                    //add previous, with current slice as end time
                    returnList.Add(new Tuple<Time, Time, ZodiacName, PlanetName>(startTimeSlice, timeSlice, previousZodiacName, planetName));

                    //save current slice as start for next
                    startTimeSlice = timeSlice;
                }

                //update value for next check
                previousZodiacName = tempZodiacName;
            }

            return returnList;

        }

        /// <summary>
        /// Gets all the constellation start time for a given planet
        /// Set to an accuracy of 1 minute
        /// </summary>
        public static List<Tuple<Time, ConstellationName, ZodiacSign>> GetConstellationTransitStartTime(Time startTime, Time endTime, PlanetName planetName)
        {
            //make slices to scan
            var accuracyInHours = 0.05; // 3 minute
            var timeSlices = Time.GetTimeListFromRange(startTime, endTime, accuracyInHours);

            var returnList = new List<Tuple<Time, ConstellationName, ZodiacSign>>();

            var startConstellation = Calculate.PlanetConstellation(planetName, startTime);
            var previousConstellation = startConstellation.GetConstellationName();

            foreach (var timeSlice in timeSlices)
            {
                //if constellation changes mark the time
                var tempConstellationName = Calculate.PlanetConstellation(planetName, timeSlice).GetConstellationName();

                //CPJ Added for Planet's Zodiac Sign
                var planetLongitude = Calculate.PlanetNirayanaLongitude(planetName, timeSlice);
                var planetZodiacSign = Calculate.ZodiacSignAtLongitude(planetLongitude);
                //-------

                if (tempConstellationName != previousConstellation)
                {
                    returnList.Add(new Tuple<Time, ConstellationName, ZodiacSign>(timeSlice, tempConstellationName, planetZodiacSign));
                }

                //update value for next check
                previousConstellation = tempConstellationName;
            }

            return returnList;
        }

        #endregion

        #region ALL DATA

        /// <summary>
        /// Niryana Constellation of all 9 planets
        /// </summary>
        public static Dictionary<PlanetName, Constellation> AllPlanetConstellation(Time time) => All9Planets.ToDictionary(planet => planet, planet => PlanetConstellation(planet, time));

        /// <summary>
        /// Gets all possible calculations for a given Time
        /// </summary>
        /// <param name="time">can be birth or query time</param>
        public static List<APIFunctionResult> AllTimeData(Time time)
        {
            //exclude this method from getting included in "Find" and Execute below
            MethodBase method = MethodBase.GetCurrentMethod();
            MethodInfo methodToExclude = method as MethodInfo;

            //do calculation
            var raw = AutoCalculator.FindAndExecuteFunctions(methodToExclude, time);

            return raw;
        }

        /// <summary>
        /// Gets all possible calculations for a Planet at a given Time
        /// </summary>
        /// <param name="time">can be birth or query time</param>
        public static List<APIFunctionResult> AllPlanetData(PlanetName planetName, Time time)
        {
            //exclude this method from getting included in "Find" and Execute below
            MethodBase method = MethodBase.GetCurrentMethod();
            MethodInfo methodToExclude = method as MethodInfo;

            //do calculation
            var raw = AutoCalculator.FindAndExecuteFunctions(methodToExclude, planetName, time);

            return raw;
        }

        /// <summary>
        /// All possible calculations for a House at a given Time
        /// </summary>
        /// <param name="time">can be birth or query time</param>
        public static List<APIFunctionResult> AllHouseData(HouseName houseName, Time time)
        {
            //exclude this method from getting included in "Find" and Execute below
            MethodBase method = MethodBase.GetCurrentMethod();
            MethodInfo methodToExclude = method as MethodInfo;

            //do calculation
            var raw = AutoCalculator.FindAndExecuteFunctions(methodToExclude, houseName, time);

            return raw;
        }

        /// <summary>
        /// All possible calculations for a Planet and House at a given Time
        /// </summary>
        /// <param name="time">can be birth or query time</param>
        public static List<APIFunctionResult> AllPlanetHouseData(PlanetName planetName, HouseName houseName, Time time)
        {
            //exclude this method from getting included in "Find" and Execute below
            MethodBase method = MethodBase.GetCurrentMethod();
            MethodInfo methodToExclude = method as MethodInfo;

            //do calculation
            var raw = AutoCalculator.FindAndExecuteFunctions(methodToExclude, planetName, houseName, time);

            return raw;
        }

        /// <summary>
        /// All possible calculations for a Zodiac Sign at a given Time
        /// </summary>
        /// <param name="time">can be birth or query time</param>
        public static List<APIFunctionResult> AllZodiacSignData(ZodiacName zodiacName, Time time)
        {
            //exclude this method from getting included in "Find" and Execute below
            MethodBase method = MethodBase.GetCurrentMethod();
            MethodInfo methodToExclude = method as MethodInfo;

            //do calculation
            var raw = AutoCalculator.FindAndExecuteFunctions(methodToExclude, zodiacName, time);

            return raw;
        }

        #endregion

        #region TIME

        /// <summary>
        /// Converts time back to longitude, it is the reverse of LongitudeToLMTOffset
        /// Exp :  5h. 10m. 20s. E. Long. to 77° 35' E. Long
        /// </summary>
        public static Angle TimeOffsetToLongitude(TimeSpan time)
        {
            //TODO function is a candidate for caching
            //degrees is equivalent to hours
            var totalDegrees = time.TotalHours * 15;

            return Angle.FromDegrees(totalDegrees);
        }

        /// <summary>
        /// Gets the ephemris time that is consumed by Swiss Ephemeris
        /// Converts normal time to Ephemeris time shown as a number
        /// </summary>
        public static double TimeToJulianEphemerisTime(Time time)
        {

            //CACHE MECHANISM
            return CacheManager.GetCache(new CacheKey(nameof(TimeToJulianEphemerisTime), time, Ayanamsa), _timeToJulianEphemerisTime);


            //UNDERLYING FUNCTION
            double _timeToJulianEphemerisTime()
            {
                SwissEph ephemeris = new();

                //set GREGORIAN CALENDAR
                int gregflag = SwissEph.SE_GREG_CAL;

                //get LMT at UTC (+0:00)
                DateTimeOffset utcDate = LmtToUtc(time);

                //extract details of time
                int year = utcDate.Year;
                int month = utcDate.Month;
                int day = utcDate.Day;
                int hour = utcDate.Hour;
                int minute = utcDate.Minute;
                int second = utcDate.Second;

                double jul_day_UT;
                double jul_day_ET;

                //results[0] = Julian day in ET (TT)
                //results[1] = Julian day in UT (UT1)
                double[] results = new double[2];
                string err_msg = "";

                //do conversion to ephemris time
                ephemeris.swe_utc_to_jd(year, month, day, hour, minute, second, gregflag, results, ref err_msg); //time to Julian Day

                //Julian day in ET (TT)
                return results[0];
            }

        }

        public static double TimeToJulianUniversalTime(Time time)
        {

            //CACHE MECHANISM
            return CacheManager.GetCache(new CacheKey(nameof(TimeToJulianUniversalTime), time, Ayanamsa), _timeToJulianUniversalTime);


            //UNDERLYING FUNCTION
            double _timeToJulianUniversalTime()
            {
                SwissEph ephemeris = new();

                //set GREGORIAN CALENDAR
                int gregflag = SwissEph.SE_GREG_CAL;

                //get LMT at UTC (+0:00)
                DateTimeOffset utcDate = LmtToUtc(time);

                //extract details of time
                int year = utcDate.Year;
                int month = utcDate.Month;
                int day = utcDate.Day;
                int hour = utcDate.Hour;
                int minute = utcDate.Minute;
                int second = utcDate.Second;

                double jul_day_UT;
                double jul_day_ET;

                //results[0] = Julian day in ET (TT)
                //results[1] = Julian day in UT (UT1)
                double[] results = new double[2];
                string err_msg = "";

                //do conversion to ephemris time
                ephemeris.swe_utc_to_jd(year, month, day, hour, minute, second, gregflag, results, ref err_msg); //time to Julian Day

                //Julian day in UT (UT1)
                return results[1];
            }

        }

        /// <summary>
        /// Convert Local Mean Time (LMT) to Standard Time (STD)
        /// API URL : ../LmtToStd/Time/05:45/03/05/1932/Longitude/75/STDOffset/+05:30
        /// </summary>
        public static DateTimeOffset LmtToStd(LocalMeanTime lmtDateTime, TimeSpan stdOffset)
        {
            //get lmt time
            var lmtTime = new DateTimeOffset(lmtDateTime.Date, LongitudeToLMTOffset(lmtDateTime.Longitude));

            //convert lmt to std & store it
            var stdTime = lmtTime.ToOffset(stdOffset);

            return stdTime;
        }

        /// <summary>
        /// Convert longitude to LMT offset
        /// input longitude range : -180 to 180 
        /// </summary>
        public static TimeSpan LongitudeToLMTOffset(double longitudeDeg)
        {
            var failCount = 0;
            var failTryLimit = 3;


            try
            {
            TryAgain:
                //raise alarm if longitude is out of range
                var outOfRange = !(longitudeDeg >= -180 && longitudeDeg <= 180);
                if (outOfRange)
                {
                    if (failCount < failTryLimit)
                    {
                        var oldLongitude = longitudeDeg; //back up for logging

                        //instead of giving up, lets take a go at correcting it
                        //assume input is 48401 but should be 48.401, so divide 1000
                        longitudeDeg = longitudeDeg / 1000;

                        failCount++; //keep track so not fall into rabbit hole

                        LibLogger.Debug($"Longitude out of range : {oldLongitude} > Auto correct to : {longitudeDeg}"); //log it for debug research

                        goto TryAgain;
                    }

                    //if control reaches here than raise exception,
                    //control should not reach here under any good call condition
                    throw new Exception($"Longitude out of range : {longitudeDeg} > Auto correct failed!");
                }

                //calculate offset based on longitude
                var offsetToReturn = TimeSpan.FromHours(longitudeDeg / 15.0);

                //round off offset to full minutes (because datetime doesnt accept fractional minutes in offsets)
                var offsetMinutes = Math.Round(offsetToReturn.TotalMinutes);

                //get new offset from rounded minutes
                offsetToReturn = TimeSpan.FromMinutes(offsetMinutes);

                //return offset to caller
                return offsetToReturn;

            }
            catch (Exception e)
            {
                //let caller know failure silently
                LibLogger.Debug(e);

                //return empty LMT for controlled failure
                return TimeSpan.Zero;
            }

        }

        /// <summary>
        /// Given a standard time (LMT) and location will get Local mean time
        /// </summary>
        public static string LocalMeanTime(Time time) => time.GetLmtDateTimeOffsetText();

        /// <summary>
        /// Given a standard time (STD) and location will get local standard time based on location
        /// Offset auto set by Google Offset API 
        /// </summary>
        public static string LocalStandardTime(Time time) => time.GetStdDateTimeOffsetText();

        /// <summary>
        /// supports dynamic 3 types of preset
        /// - age1to10
        /// - 3weeks, 3months, 3years, fulllife
        /// - 1990-2000
        /// given a nice human time range will generate start and end times
        /// input user's current timezone, could be different from birth
        /// </summary>
        /// <param name="outputTimezone">output timezone can be different from birth timezone "+08:00"</param>
        public static TimeRange AutoCalculateTimeRange(Time inputBirthTime, string timePreset, TimeSpan outputTimezone)
        {
            var birthLocation = inputBirthTime.GetGeoLocation();

            Time start, end;
            //use the inputed user's timezone
            DateTimeOffset now = DateTimeOffset.Now.ToOffset(outputTimezone);
            var today = now.ToString("dd/MM/yyyy zzz");

            var yesterday = now.AddDays(-1).ToString("dd/MM/yyyy zzz");
            var timePresetString = timePreset.ToLower(); //so that all cases are accepted

            //PRESET A = 3days,
            //PRESET B = 1990 - 2000
            //get type of preset
            var isPresetB = timePresetString.Contains("-"); //if has hyphen than must be time range
            var isPresetC = timePresetString.Contains("to"); //age5to10

            TimeRange returnValue;

            //process 1990-2000
            if (isPresetB) { returnValue = ProcessPresetTypeB(); }

            //when preset is age1to50
            else if (isPresetC) { returnValue = ProcessPresetTypeC(); }

            //A type is default processing, if not B or C must A then
            //3days, 2years, this week, full life
            else { returnValue = ProcessPresetTypeA(); }


            return returnValue;


            //process 3days, 2years, this week, full life
            TimeRange ProcessPresetTypeA()
            {

                //NOTE:
                //two possible name types for 6months and "thismonth"
                //so if got number infront then different handle
                //assume input is "3days", number + date type
                //so split by number
                var split = Tools.SplitAlpha(timePresetString);
                var result = int.TryParse(split[0], out int number);
                number = number < 1 ? 1 : number; //min 1, so user can in put just, "year" and except 1 year
                                                  //if no number, than data type in 1st place
                var dateType = result ? split[1] : split[0];


                //process accordingly
                int days;
                double hoursToAdd;
                string _1WeekAgo = now.AddDays(-7).ToString("dd/MM/yyyy zzz");
                string _2MonthsAgo = now.AddDays(-60).ToString("dd/MM/yyyy zzz");
                string _3MonthsAgo = now.AddDays(-90).ToString("dd/MM/yyyy zzz");
                string _6MonthsAgo = now.AddDays(-182).ToString("dd/MM/yyyy zzz");
                string _1YearAgo = now.AddDays(-365).ToString("dd/MM/yyyy zzz");
                var timeNow = Time.NowSystem(birthLocation);
                switch (dateType.ToLower())
                {
                    case "hour":
                    case "hours":
                        var startHour = now.AddHours(-1); //back 1 hour
                        var endHour = now.AddHours(number); //front by input
                        start = new Time(startHour, birthLocation);
                        end = new Time(endHour, birthLocation);
                        return new TimeRange(start, end);
                    case "today":
                    case "day":
                    case "days":
                        hoursToAdd = Tools.DaysToHours(number); //convert DAYS to HOURS
                        var startDays = now.RemoveHours(hoursToAdd); //back by input
                        var endDays = now.AddHours(hoursToAdd); //front by input
                        start = new Time(startDays, birthLocation);
                        end = new Time(endDays, birthLocation);
                        return new TimeRange(start, end);
                    case "week":
                    case "weeks":
                        hoursToAdd = Tools.WeeksToHours(number);
                        start = timeNow.RemoveHours(hoursToAdd);
                        end = timeNow.AddHours(hoursToAdd); //+the days
                        return new TimeRange(start, end);
                    case "month":
                    case "months":
                        hoursToAdd = Tools.MonthsToHours(number);
                        start = timeNow.RemoveHours(hoursToAdd);
                        end = timeNow.AddHours(hoursToAdd);
                        return new TimeRange(start, end);
                    case "year":
                    case "years":
                        hoursToAdd = Tools.YearsToHours(number);
                        start = timeNow.RemoveHours(hoursToAdd);
                        end = timeNow.AddHours(hoursToAdd);
                        return new TimeRange(start, end);
                    case "decades":
                    case "decade":
                        hoursToAdd = Tools.DecadesToHours(number);
                        start = timeNow.RemoveHours(hoursToAdd);
                        end = timeNow.AddHours(hoursToAdd);
                        return new TimeRange(start, end);
                    case "fulllife":
                        start = inputBirthTime;
                        end = inputBirthTime.AddYears(75);
                        return new TimeRange(start, end);
                    default:
                        return new TimeRange(Time.Empty, Time.Empty);

                }
            }

            //process age1to50
            TimeRange ProcessPresetTypeC()
            {

                //age 1 to 50
                var split = Tools.SplitAlpha(timePresetString);

                var startAge = int.Parse(split[1]);
                var endAge = int.Parse(split[3]);

                //if age 1 set to 0, because in common talk age 1 is same as birth year, nobody says age 0
                startAge = startAge == 1 ? 0 : startAge;

                //add to birth time to get final time range
                start = inputBirthTime.AddYears(startAge);
                end = inputBirthTime.AddYears(endAge);
                return new TimeRange(start, end);

            }

            //process 1990-2000
            TimeRange ProcessPresetTypeB()
            {
                //break into start & end year
                var splited = timePresetString.Split('-');

                //get year
                var startYear = splited[0];
                var endYear = splited[1];

                //timezone to construct new time for client time
                var timeZone = now.ToString("zzz");

                //create time at start and end of year
                var startTime = new Time($"00:00 01/01/{startYear} {timeZone}", birthLocation);
                var endTime = new Time($"00:00 31/12/{endYear} {timeZone}", birthLocation);

                return new TimeRange(startTime, endTime);

            }
        }

        #endregion

        #region GENERAL

        /// <summary>
        /// Give a time preset (3 types), will return days between them
        /// NOTE: used by web UI via API for chart precision calculation
        /// </summary>
        public static double DaysBetweenTimeRangePreset(Time inputBirthTime, string timePreset, TimeSpan outputTimezone)
        {
            //get time range from given data
            var timeRange = Calculate.AutoCalculateTimeRange(inputBirthTime, timePreset, outputTimezone);

            //calculate days between
            var daysBetween = Math.Round(timeRange.DaysBetween, 2);

            return daysBetween;
        }



        /// <summary>
        /// Easyly import Jaganath Hora (.jhd) files into VedAstro.
        /// Yeah! Competition drives growth!
        /// </summary>
        public static Person ParseJHDFiles(string personName, string rawTextData)
        {
            // Split the raw text data into an array of strings
            var lines = rawTextData.Trim().Split('\n');

            // Extract the date and time parts
            var hoursTotalDecimal = double.Parse(lines[3]);
            // Extract the whole number part for hours
            var hours = (int)hoursTotalDecimal;
            // Get the fractional part and convert it to minutes
            double fractionalPart = hoursTotalDecimal - hours;
            var minutes = (int)Math.Round(fractionalPart * 100);

            //extract out the date parts
            var month = int.Parse(lines[0]);
            var day = int.Parse(lines[1]);
            var year = int.Parse(lines[2]);
            var timeZoneSpan = ConvertRawTimezoneToTimeSpan(lines[4]);

            // Format the date and time text
            DateTimeOffset parsedStdTime = new DateTimeOffset(year, month, day, hours, minutes, 0, 0, timeZoneSpan);
            var dateTimeText = parsedStdTime.ToString(Time.DateTimeFormat);

            // Extract the location and coordinates
            var locationRaw = $"{lines[12].Trim()}, {lines[13].Trim()}";//remove trailing white spaces
            var locationName = Regex.Replace(locationRaw, "[^a-zA-Z0-9 ,]", "");
            var rawLongitude = lines[5];
            var longitude = ConvertRawLongitude(rawLongitude);
            var latitude = double.Parse(lines[6]);
            var parsedLocation = new GeoLocation(locationName, longitude, latitude);

            var birthTime = new Time(parsedStdTime, parsedLocation);

            //extract gender
            var genderRaw = int.Parse(lines[17].Trim());
            var parsedGender = genderRaw == 1 ? Gender.Male : Gender.Female; //female is 2

            //combine all into 1 person
            var person = new Person(personName, birthTime, parsedGender);

            return person;

            // Converts input to a TimeSpan representing UTC offset.
            // If input is “-5.300000/r” it converts to "+05:30"
            // but if “5.300000/r” it converts to "-05:30"
            static TimeSpan ConvertRawTimezoneToTimeSpan(string input)
            {
                // Remove the "/r" from the end of the string
                string cleanedInput = input.Replace("/r", "");

                // Split the string into hours and minutes
                string[] timeParts = cleanedInput.Split('.');

                // Convert the string parts to integers
                int hours = int.Parse(timeParts[0]);
                int minutes = int.Parse(timeParts[1].Substring(0, 2)); // Get the first two digits of the decimal part

                // Reverse the sign of the hours
                hours = -hours;

                // Convert the hours and minutes to a TimeSpan
                TimeSpan timeSpan = new TimeSpan(hours, minutes, 0);

                return timeSpan;
            }

            //Converts a raw longitude string to a double and changes its sign.
            //EXP: "-77.350000\r" to 77.35, "108.350000\r" to -108.35
            static double ConvertRawLongitude(string rawLongitude)
            {
                // Trim the string to remove leading and trailing white spaces
                string trimmedLongitude = rawLongitude.Trim();

                // Try to parse the string to a double
                if (double.TryParse(trimmedLongitude, out double longitude))
                {
                    // If the longitude is negative, make it positive. If it's positive, make it negative.
                    longitude = longitude < 0 ? Math.Abs(longitude) : -Math.Abs(longitude);
                    return longitude;
                }
                else
                {
                    throw new FormatException("Invalid format for longitude.");
                }
            }

        }


        /// <summary>
        /// All horoscope predictions as Alpaca Template ready for LoRA training in JSON
        /// </summary>
        public static async Task<List<JObject>> HoroscopePredictionAlpacaTemplateLoRA(Time birthTime)
        {
            var returnList = new List<JObject>();
            foreach (var horoscopeData in HoroscopeDataListStatic.Rows)
            {
                JObject jObject = new JObject
                {
                    ["instruction"] = horoscopeData.Name.ToString(),
                    ["input"] = "",
                    ["output"] = horoscopeData.Description
                };

                returnList.Add(jObject);
            }

            return returnList;
        }

        /// <summary>
        /// Given a birth time will calculate all predictions that match for given birth time.
        /// Default includes all predictions, ie: Yoga, Planets in Sign, AshtakavargaYoga
        /// Can be filtered.
        /// </summary>
        /// <param name="filterTag">Set to only show certain types of predictions</param>
        public static List<HoroscopePrediction> HoroscopePredictions(Time birthTime, EventTag filterTag = EventTag.Empty)
        {
            //calculate predictions for current person
            var predictionList = Tools.GetHoroscopePrediction(birthTime, filterTag);

            return predictionList;
        }

        /// <summary>
        /// Given a birth time will calculate all prediction name's that match for given birth time
        /// example : "Moon House 8", "10th Lord in 8th House"
        /// note : used by AI Chat, when talking to Astro tuned LLM server
        /// </summary>
        public static List<string> HoroscopePredictionNames(Time birthTime)
        {
            //calculate predictions for current person
            var predictionList = Tools.GetHoroscopePrediction(birthTime);

            //take out only name
            var namesOnly = predictionList.Select(x => x.Name.ToString()).ToList();

            return namesOnly;
        }


        /// <summary>
        /// Calculate Fortuna Point for a given birth time & place. Returns Sign Number from Lagna
        /// for KP system a fast-moving point which can differentiate between two early births as twins.
        /// </summary>
        public static int FortunaPoint(ZodiacName ascZodiacSignName, Time time)
        {
            //Fortune Point is calculated as Asc Degrees + Moon Degrees - Sun Degrees
            var a1 = Calculate.AllHouseLongitudes(time)[0].GetBeginLongitude().TotalDegrees;

            //Find Lagna, Moon and Sun longitude degree
            var _asc_Degrees = Calculate.AllHouseLongitudes(time)[0].GetMiddleLongitude().TotalDegrees;
            var _moonDegrees = Calculate.PlanetNirayanaLongitude(PlanetName.Moon, time).TotalDegrees;
            var _sunDegrees = Calculate.PlanetNirayanaLongitude(PlanetName.Sun, time).TotalDegrees;

            //fortuna point is the point that is same distance from Ascendant
            //as Moon is from Sun
            var _fortunaPointDegrees = 0.00;

            /*
            //if its a day chart


            if (_sunDegrees >= 180.000 && _sunDegrees < 360.000) 
            {
                _fortunaPointDegrees = _asc_Degrees + _moonDegrees - _sunDegrees;
            }

            else
            {
                if (_sunDegrees >= 0.000 && _sunDegrees < 180.000)
                {
                    _fortunaPointDegrees = _asc_Degrees + _sunDegrees - _moonDegrees;
                }
            }
            */

            //first let's compute how far the Moon is from Sun
            var _moon_sun_distance = _moonDegrees - _sunDegrees;

            if (_moon_sun_distance < 0) //moon is behind sun
            {
                _moon_sun_distance = _moon_sun_distance + 360;
            }

            //now lets compute the Fortuna point 
            _fortunaPointDegrees = _asc_Degrees + _moon_sun_distance;

            if (_fortunaPointDegrees >= 360)
            {
                _fortunaPointDegrees = _fortunaPointDegrees - 360;
            }


            //convert Degrees to Angle
            var _angleAtFortunaPointDegrees = VedAstro.Library.Angle.FromDegrees(_fortunaPointDegrees);

            //find zodiacSignAtFP Longitude
            var _zodiacSignAtFP = Calculate.ZodiacSignAtLongitude(_angleAtFortunaPointDegrees).GetSignName();

            // var houseNo = Calculate.HouseFromSignName(_zodiacSignAtFP, time);


            //find how many signs the FP is from Lagna
            var _signCount = Calculate.CountFromSignToSign(ascZodiacSignName, _zodiacSignAtFP);
            return _signCount;
        }

        /// <summary>
        /// Calculate Destiny Point for a given birth time & place. Returns Sign Number from Lagna
        /// </summary>
        public static int DestinyPoint(Time time, ZodiacName ascZodiacSignName)
        {
            //destiny point is calculated as follows
            //Difference between Moon and Rahu longitude, Difference divided by 2, the result added to Rahu longitude

            var rahuDegrees = Calculate.PlanetNirayanaLongitude(PlanetName.Rahu, time).TotalDegrees;
            var moonDegrees = Calculate.PlanetNirayanaLongitude(PlanetName.Moon, time).TotalDegrees;

            var diff = moonDegrees - rahuDegrees;

            // if diff is negative, that means Moon is ahead of Rahu, then add 360 to the number. 
            if (diff < 0)
            {
                diff = diff + 360;
            }

            var mid_point = diff / 2;

            // Add mid_point to Rahu degrees
            var destinyPointDegrees = rahuDegrees + mid_point;

            if (destinyPointDegrees >= 360)
            {
                destinyPointDegrees = destinyPointDegrees - 360;
            }

            var angleAtDestinyPointDegrees = VedAstro.Library.Angle.FromDegrees(destinyPointDegrees);
            var zodiacSignAtDP = Calculate.ZodiacSignAtLongitude(angleAtDestinyPointDegrees).GetSignName();
            var signCount = Calculate.CountFromSignToSign(ascZodiacSignName, zodiacSignAtDP);

            return signCount;
        }

        /// <summary>
        /// Given a person will give yoni kuta animal with sex
        /// </summary>
        public static string YoniKutaAnimal(Time birthTime)
        {
            var finalPrediction = "";

            var birthConst = Calculate.MoonConstellation(birthTime);
            var animal = Calculate.YoniKutaAnimalFromConstellation(birthConst.GetConstellationName());

            finalPrediction += animal.ToString();

            return finalPrediction;
        }

        /// <summary>
        /// Given a constellation will give animal with sex, used for yoni kuta calculations
        /// and body appearance prediction
        /// </summary>
        public static ConstellationAnimal YoniKutaAnimalFromConstellation(ConstellationName sign)
        {
            switch (sign)
            {
                //Horse
                case ConstellationName.Aswini:
                    return new ConstellationAnimal("Male", AnimalName.Horse);
                case ConstellationName.Satabhisha:
                    return new ConstellationAnimal("Female", AnimalName.Horse);

                //Elephant
                case ConstellationName.Bharani:
                    return new ConstellationAnimal("Male", AnimalName.Elephant);
                case ConstellationName.Revathi:
                    return new ConstellationAnimal("Female", AnimalName.Elephant);

                //Sheep
                case ConstellationName.Pushyami:
                    return new ConstellationAnimal("Male", AnimalName.Sheep);
                case ConstellationName.Krithika:
                    return new ConstellationAnimal("Female", AnimalName.Sheep);

                //Serpent
                case ConstellationName.Rohini:
                    return new ConstellationAnimal("Male", AnimalName.Serpent);
                case ConstellationName.Mrigasira:
                    return new ConstellationAnimal("Female", AnimalName.Serpent);

                //Dog
                case ConstellationName.Moola:
                    return new ConstellationAnimal("Male", AnimalName.Dog);
                case ConstellationName.Aridra:
                    return new ConstellationAnimal("Female", AnimalName.Dog);

                //Cat
                case ConstellationName.Aslesha:
                    return new ConstellationAnimal("Male", AnimalName.Cat);
                case ConstellationName.Punarvasu:
                    return new ConstellationAnimal("Female", AnimalName.Cat);

                //Rat
                case ConstellationName.Makha:
                    return new ConstellationAnimal("Male", AnimalName.Rat);
                case ConstellationName.Pubba:
                    return new ConstellationAnimal("Female", AnimalName.Rat);

                //Cow
                case ConstellationName.Uttara:
                    return new ConstellationAnimal("Male", AnimalName.Cow);
                case ConstellationName.Uttarabhadra:
                    return new ConstellationAnimal("Female", AnimalName.Cow);

                //Buffalo
                case ConstellationName.Swathi:
                    return new ConstellationAnimal("Male", AnimalName.Buffalo);
                case ConstellationName.Hasta:
                    return new ConstellationAnimal("Female", AnimalName.Buffalo);

                //Tiger
                case ConstellationName.Vishhaka:
                    return new ConstellationAnimal("Male", AnimalName.Tiger);
                case ConstellationName.Chitta:
                    return new ConstellationAnimal("Female", AnimalName.Tiger);

                //Hare
                case ConstellationName.Jyesta:
                    return new ConstellationAnimal("Male", AnimalName.Hare);
                case ConstellationName.Anuradha:
                    return new ConstellationAnimal("Female", AnimalName.Hare);

                //Monkey
                case ConstellationName.Poorvashada:
                    return new ConstellationAnimal("Male", AnimalName.Monkey);
                case ConstellationName.Sravana:
                    return new ConstellationAnimal("Female", AnimalName.Monkey);

                //Lion
                case ConstellationName.Poorvabhadra:
                    return new ConstellationAnimal("Male", AnimalName.Lion);
                case ConstellationName.Dhanishta:
                    return new ConstellationAnimal("Female", AnimalName.Lion);

                //Mongoose
                case ConstellationName.Uttarashada:
                    return new ConstellationAnimal("Male", AnimalName.Mongoose);

                default: throw new Exception("Yoni Kuta Animal Not Found!");
            }
        }

        /// <summary>
        /// Get sky chart as animated GIF. URL can be used like a image source link
        /// </summary>
        public static async Task<byte[]> SkyChartGIF(Time time) => await SkyChartFactory.GenerateChartGif(time, 750, 230);

        /// <summary>
        /// Get sky chart at a given time. SVG image file. URL can be used like a image source link
        /// </summary>
        public static async Task<string> SkyChart(Time time) => await SkyChartFactory.GenerateChart(time, 750, 230);

        /// <summary>
        /// Creates a kundali chart from D1 to D20. In south indian style. URL can be used like a SVG image source link
        /// </summary>
        public static string SouthIndianChart(Time time, ChartType chartType = ChartType.RasiD1)
        {
            var svgString = (new SouthChartFactory(time, chartType)).SVGChart;

            return svgString;
        }

        /// <summary>
        /// Creates a kundali chart from D1 to D20. In north indian style. URL can be used like a SVG image source link
        /// </summary>
        public static string NorthIndianChart(Time time, ChartType chartType = ChartType.RasiD1)
        {
            var svgString = (new NorthChartFactory(time, chartType)).SVGChart;

            return svgString;
        }

        /// <summary>
        /// special function localized to allow caching
        /// note: there is another version that does caching
        /// </summary>
        public static double TimeToJulianDay(Time time)
        {
            //get lmt time
            var lmtDateTime = time.GetLmtDateTimeOffset();

            //Converts LMT to UTC (GMT)
            DateTimeOffset utcDateTime = lmtDateTime.ToUniversalTime();

            SwissEph swissEph = new SwissEph();

            double jul_day_UT;
            jul_day_UT = swissEph.swe_julday(utcDateTime.Year, utcDateTime.Month, utcDateTime.Day,
                utcDateTime.TimeOfDay.TotalHours, SwissEph.SE_GREG_CAL);
            return jul_day_UT;

        }

        /// <summary>
        /// Convert LMT to Julian Days used in Swiss Ephemeris
        /// </summary>
        public static double ConvertLmtToJulian(Time time)
        {

            //CACHE MECHANISM
            return CacheManager.GetCache(new CacheKey(nameof(ConvertLmtToJulian), time, Ayanamsa), _convertLmtToJulian);


            //UNDERLYING FUNCTION
            double _convertLmtToJulian()
            {
                //get lmt time
                DateTimeOffset lmtDateTime = time.GetLmtDateTimeOffset();

                //split lmt time to pieces
                int year = lmtDateTime.Year;
                int month = lmtDateTime.Month;
                int day = lmtDateTime.Day;
                double hour = (lmtDateTime.TimeOfDay).TotalHours;

                //set calender type
                int gregflag = SwissEph.SE_GREG_CAL; //GREGORIAN CALENDAR

                //declare output variables
                double localMeanTimeInJulian_UT;

                //initialize ephemeris
                SwissEph ephemeris = new SwissEph();

                //get lmt in julian day in Universal Time (UT)
                localMeanTimeInJulian_UT = ephemeris.swe_julday(year, month, day, hour, gregflag);//time to Julian Day

                return localMeanTimeInJulian_UT;

            }

        }

        /// <summary>
        /// Gets longitudinal space between 2 planets
        /// Note :
        /// - Longitude of planet after 360 is 0 degrees,
        ///   when calculating difference this needs to be accounted for.
        /// - Calculation in Nirayana longitudes
        /// - Calculates longitudes for you
        /// </summary>
        public static Angle DistanceBetweenPlanets(PlanetName planet1, PlanetName planet2, Time time)
        {
            var planet1Longitude = PlanetNirayanaLongitude(planet1, time);
            var planet2Longitude = PlanetNirayanaLongitude(planet2, time);

            var distanceBetweenPlanets = planetDistance(planet1Longitude.TotalDegrees, planet2Longitude.TotalDegrees);

            return Angle.FromDegrees(distanceBetweenPlanets);



            //---------------FUNCTION---------------


            double planetDistance(double len1, double len2)
            {
                double d = red_deg(Math.Abs(len2 - len1));

                if (d > 180) return (360 - d);

                return d;
            }

            //Reduces a given double value modulo 360.
            //The return value is between 0 and 360.
            double red_deg(double input) => a_red(input, 360);

            //Reduces a given double value x modulo the double a(should be positive).
            //The return value is between 0 and a.
            double a_red(double x, double a) => (x - Math.Floor(x / a) * a);

        }

        /// <summary>
        /// Gets longitudinal space between 2 planets
        /// Note :
        /// - Longitude of planet after 360 is 0 degrees,
        ///   when calculating difference this needs to be accounted for
        /// - Expects you to calculate longitude
        /// </summary>
        public static Angle DistanceBetweenPlanets(Angle planet1, Angle planet2)
        {

            var distanceBetweenPlanets = planetDistance(planet1.TotalDegrees, planet2.TotalDegrees);

            return Angle.FromDegrees(distanceBetweenPlanets);



            //---------------FUNCTION---------------


            double planetDistance(double len1, double len2)
            {
                double d = red_deg(Math.Abs(len2 - len1));

                if (d > 180) return (360 - d);

                return d;
            }

            //Reduces a given double value modulo 360.
            //The return value is between 0 and 360.
            double red_deg(double input) => a_red(input, 360);

            //Reduces a given double value x modulo the double a(should be positive).
            //The return value is between 0 and a.
            double a_red(double x, double a) => (x - Math.Floor(x / a) * a);

        }

        /// <summary>
        /// Greenwich Apparent In Julian Days
        /// </summary>
        public static double GreenwichApparentInJulianDays(Time time)
        {
            //convert lmt to julian days, in universal time (UT)
            var localMeanTimeInJulian_UT = GreenwichLmtInJulianDays(time);

            //get longitude of location
            double longitude = time.GetGeoLocation().Longitude();

            //delcare output variables
            double localApparentTimeInJulian;
            string errorString = "";

            //convert lmt to local apparent time (LAT)
            using SwissEph ephemeris = new();
            ephemeris.swe_lmt_to_lat(localMeanTimeInJulian_UT, longitude, out localApparentTimeInJulian, ref errorString);


            return localApparentTimeInJulian;
        }

        /// <summary>
        /// Shows local apparent time from Swiss Eph
        /// </summary>
        public static DateTime LocalApparentTime(Time time)
        {
            //CACHE MECHANISM
            return CacheManager.GetCache(new CacheKey(nameof(LocalApparentTime), time, Ayanamsa), _getLocalApparentTime);

            //UNDERLYING FUNCTION
            DateTime _getLocalApparentTime()
            {
                //convert lmt to julian days, in universal time (UT)
                var localMeanTimeInJulian_UT = ConvertLmtToJulian(time);

                //get longitude of location
                double longitude = time.GetGeoLocation().Longitude();

                //delcare output variables
                double localApparentTimeInJulian;
                string errorString = null;

                //initialize ephemeris
                SwissEph ephemeris = new SwissEph();

                //convert lmt to local apparent time (LAT)
                ephemeris.swe_lmt_to_lat(localMeanTimeInJulian_UT, longitude, out localApparentTimeInJulian, ref errorString);

                var localApparentTime = ConvertJulianTimeToNormalTime(localApparentTimeInJulian);

                return localApparentTime;

            }

        }

        /// <summary>
        /// WARNING! MARKED FOR DELETION : ERONEOUS RESULTS NOT SUITED FOR INTENDED PURPOSE
        /// METHOD NOT VERIFIED
        /// This methods perpose is to define the final good or bad
        /// nature of planet in antaram.
        ///
        /// For now only data from chapter "Key-planets for Each Sign"
        /// If this proves to be inacurate, add more checks in this method.
        /// - bindu points
        /// 
        /// Similar to method GetDasaInfoForAscendant
        /// Data from pg 80 of Key-planets for Each Sign in Hindu Predictive Astrology
        /// TODO meant to determine nature of antram
        /// </summary>
        public static EventNature PlanetDasaNature(Time birthTime, PlanetName planet)
        {
            //todo account for rahu & ketu
            //rahu & ketu not sure for now, just return neutral
            if (planet == Rahu || planet == Ketu) { return EventNature.Neutral; }

            //get nature from person's lagna
            var planetNature = GetNatureFromLagna();

            //if nature is neutral then use nature of relation to current house
            //assumed that bad relation to sign is bad planet (todo upgrade to bindu points)
            //note: generaly speaking a neutral planet shloud not exist, either good or bad
            if (planetNature == EventNature.Neutral)
            {
                var _planetCurrentHouse = HousePlanetOccupiesBasedOnLongitudes(planet, birthTime);

                var _currentHouseRelation = PlanetRelationshipWithHouse(_planetCurrentHouse, planet, birthTime);

                switch (_currentHouseRelation)
                {
                    case PlanetToSignRelationship.BestFriendVarga:
                    case PlanetToSignRelationship.FriendVarga:
                    case PlanetToSignRelationship.OwnVarga:
                    case PlanetToSignRelationship.Moolatrikona:
                        return EventNature.Good;
                    case PlanetToSignRelationship.NeutralVarga:
                        return EventNature.Neutral;
                    case PlanetToSignRelationship.EnemyVarga:
                    case PlanetToSignRelationship.BitterEnemyVarga:
                        return EventNature.Bad;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            //else return nature from lagna
            return planetNature;


            //LOCAL FUNCTIONS

            EventNature GetNatureFromLagna()
            {
                var personLagna = HouseSignName(HouseName.House1, birthTime);

                //get list of good and bad planets for a lagna
                dynamic planetData = GetPlanetData(personLagna);
                List<PlanetName> goodPlanets = planetData.Good;
                List<PlanetName> badPlanets = planetData.Bad;

                //check good planets first
                if (goodPlanets.Contains(planet))
                {
                    return EventNature.Good;
                }

                //check bad planets next
                if (badPlanets.Contains(planet))
                {
                    return EventNature.Bad;
                }

                //if control reaches here, then planet not
                //listed as good or bad, so just say neutral
                return EventNature.Neutral;
            }

            // data from chapter "Key-planets for Each Sign"
            object GetPlanetData(ZodiacName lagna)
            {
                List<PlanetName> good = null;
                List<PlanetName> bad = null;

                switch (lagna)
                {
                    //Aries - Saturn, Mercury and Venus are ill-disposed.
                    // Jupiter and the Sun are auspicious. The mere combination
                    // of Jupiler and Saturn produces no beneficial results. Jupiter
                    // is the Yogakaraka or the planet producing success. If Venus
                    // becomes a maraka, he will not kill the native but planets like
                    // Saturn will bring about death to the person.
                    case ZodiacName.Aries:
                        good = new List<PlanetName>() { Jupiter, Sun };
                        bad = new List<PlanetName>() { Saturn, Mercury, Venus };
                        break;
                    //Taurus - Saturn is the most auspicious and powerful
                    // planet. Jupiter, Venus and the Moon are evil planets. Saturn
                    // alone produces Rajayoga. The native will be killed in the
                    // periods and sub-periods of Jupiter, Venus and the Moon if
                    // they get death-inflicting powers.
                    case ZodiacName.Taurus:
                        good = new List<PlanetName>() { Saturn };
                        bad = new List<PlanetName>() { Jupiter, Venus, Moon };
                        break;
                    //Gemini - Mars, Jupiter and the Sun are evil. Venus alone
                    // is most beneficial and in conjunction with Saturn in good signs
                    // produces and excellent career of much fame. Combination
                    // of Saturn and Jupiter produces similar results as in Aries.
                    // Venus and Mercury, when well associated, cause Rajayoga.
                    // The Moon will not kill the person even though possessed of
                    // death-inflicting powers.
                    case ZodiacName.Gemini:
                        good = new List<PlanetName>() { Venus };
                        bad = new List<PlanetName>() { Mars, Jupiter, Sun };
                        break;
                    //Cancer - Venus and Mercury are evil. Jupiter and Mars
                    // give beneficial results. Mars is the Rajayogakaraka
                    // (conferor of name and fame). The combination of Mars and Jupiter
                    // also causes Rajayoga (combination for political success). The
                    // Sun does not kill the person although possessed of maraka
                    // powers. Venus and other inauspicious planets kill the native.
                    // Mars in combination with the Moon or Jupiter in favourable
                    // houses especially the 1st, the 5th, the 9th and the 10th
                    // produces much reputation.
                    case ZodiacName.Cancer:
                        good = new List<PlanetName>() { Jupiter, Mars };
                        bad = new List<PlanetName>() { Venus, Mercury };
                        break;
                    //Leo - Mars is the most auspicious and favourable planet.
                    // The combination of Venus and Jupiter does not cause Rajayoga
                    // but the conjunction of Jupiter and Mars in favourable
                    // houses produce Rajayoga. Saturn, Venus and Mercury are
                    // evil. Saturn does not kill the native when he has the maraka
                    // power but Mercury and other evil planets inflict death when
                    // they get maraka powers.
                    case ZodiacName.Leo:
                        good = new List<PlanetName>() { Mars };
                        bad = new List<PlanetName>() { Saturn, Venus, Mercury };
                        break;
                    //Virgo - Venus alone is the most powerful. Mercury and
                    // Venus when combined together cause Rajayoga. Mars and
                    // the Moon are evil. The Sun does not kill the native even if
                    // be becomes a maraka but Venus, the Moon and Jupiter will
                    // inflict death when they are possessed of death-infticting power.
                    case ZodiacName.Virgo:
                        good = new List<PlanetName>() { Venus };
                        bad = new List<PlanetName>() { Mars, Moon };
                        break;
                    // Libra - Saturn alone causes Rajayoga. Jupiter, the Sun
                    // and Mars are inauspicious. Mercury and Saturn produce good.
                    // The conjunction of the Moon and Mercury produces Rajayoga.
                    // Mars himself will not kill the person. Jupiter, Venus
                    // and Mars when possessed of maraka powers certainly kill the
                    // nalive.
                    case ZodiacName.Libra:
                        good = new List<PlanetName>() { Saturn, Mercury };
                        bad = new List<PlanetName>() { Jupiter, Sun, Mars };
                        break;
                    //Scorpio - Jupiter is beneficial. The Sun and the Moon
                    // produce Rajayoga. Mercury and Venus are evil. Jupiter,
                    // even if be becomes a maraka, does not inflict death. Mercury
                    // and other evil planets, when they get death-inlflicting powers,
                    // do not certainly spare the native.
                    case ZodiacName.Scorpio:
                        good = new List<PlanetName>() { Jupiter };
                        bad = new List<PlanetName>() { Mercury, Venus };
                        break;
                    //Sagittarius - Mars is the best planet and in conjunction
                    // with Jupiter, produces much good. The Sun and Mars also
                    // produce good. Venus is evil. When the Sun and Mars
                    // combine together they produce Rajayoga. Saturn does not
                    // bring about death even when he is a maraka. But Venus
                    // causes death when be gets jurisdiction as a maraka planet.
                    case ZodiacName.Sagittarius:
                        good = new List<PlanetName>() { Mars };
                        bad = new List<PlanetName>() { Venus };
                        break;
                    //Capricorn - Venus is the most powerful planet and in
                    // conjunction with Mercury produces Rajayoga. Mars, Jupiter
                    // and the Moon are evil.
                    case ZodiacName.Capricorn:
                        good = new List<PlanetName>() { Venus };
                        bad = new List<PlanetName>() { Mars, Jupiter, Moon };
                        break;
                    //Aquarius - Venus alone is auspicious. The combination of
                    // Venus and Mars causes Rajayoga. Jupiter and the Moon are
                    // evil.
                    case ZodiacName.Aquarius:
                        good = new List<PlanetName>() { Venus };
                        bad = new List<PlanetName>() { Jupiter, Moon };
                        break;
                    //Pisces - The Moon and Mars are auspicious. Mars is
                    // most powerful. Mars with the Moon or Jupiter causes Rajayoga.
                    // Saturn, Venus, the Sun and Mercury are evil. Mars
                    // himself does not kill the person even if he is a maraka.
                    case ZodiacName.Pisces:
                        good = new List<PlanetName>() { Moon, Mars };
                        bad = new List<PlanetName>() { Saturn, Venus, Sun, Mercury };
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }


                return new { Good = good, Bad = bad };

            }
        }

        /// <summary>
        /// Get planet's Longitude, Latitude, DistanceAU, SpeedLongitude, SpeedLatitude...
        /// Swiss Ephemeris "swe_calc" wrapper for open API 
        /// </summary>
        public static dynamic SwissEphemeris(PlanetName planetName, Time time)
        {
            //convert planet name, compatible with Swiss Eph
            int swissPlanet = Tools.VedAstroToSwissEph(planetName);

            //do the calculation
            var sweCalcResults = Tools.ephemeris_swe_calc(time, swissPlanet);

            return sweCalcResults;
        }

        /// <summary>
        /// For all planets including Pluto, Neptune, Uranus
        /// Get planet's Longitude, Latitude, DistanceAU, SpeedLongitude, SpeedLatitude...
        /// Uses Swiss Ephemeris directly to get values
        /// </summary>
        public static List<dynamic> SwissEphemerisAll(Time time)
        {
            //for all planets
            var _12Planets = new List<int>
            {
                SwissEph.SE_SUN, SwissEph.SE_MOON, SwissEph.SE_MERCURY, SwissEph.SE_MARS,
                SwissEph.SE_VENUS, SwissEph.SE_JUPITER, SwissEph.SE_SATURN,
                SwissEph.SE_URANUS, SwissEph.SE_NEPTUNE, SwissEph.SE_PLUTO,
                //rahu & ketu
                SwissEph.SE_TRUE_NODE, SwissEph.SE_OSCU_APOG,
            };

            //put all data for all planets in 1 big list
            var bigList = new List<dynamic>();
            foreach (var planet in _12Planets)
            {
                var temp = Tools.ephemeris_swe_calc(time, planet);
                bigList.Add(temp);
            }

            return bigList;
        }

        /// <summary>
        /// Checks if a planet is same house (not nessarly conjunct) with the lord of a certain house
        /// Example : Is Sun joined with lord of 9th?
        /// </summary>
        public static bool IsPlanetSameHouseWithHouseLord(int houseNumber, PlanetName planet, Time birthTime)
        {
            //get house of the lord in question
            var houseLord = LordOfHouse((HouseName)houseNumber, birthTime);
            var houseLordHouse = HousePlanetOccupiesBasedOnLongitudes(houseLord, birthTime);

            //get house of input planet
            var inputPlanetHouse = HousePlanetOccupiesBasedOnLongitudes(planet, birthTime);

            //check if both are in same house
            if (inputPlanetHouse == houseLordHouse)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// Based on Shadvarga get nature of house for a person,
        /// nature in number form to for easy calculation into summary
        /// good = 1, bad = -1, neutral = 0
        /// specially made method for life chart summary
        /// Experimental Code
        /// </summary>
        public static double HouseNatureScore(Time birthTime, HouseName inputHouse)
        {
            //if no house then no score
            if (inputHouse == HouseName.Empty)
            {
                return 0;
            }

            //get house score
            var houseStrength = HouseStrength(inputHouse, birthTime).ToDouble();

            //weakest planet gives lowest score -2
            //strongest planet gives highest score 2
            //get range
            var highestHouseScore = HouseStrength(AllHousesOrderedByStrength(birthTime)[0], birthTime).ToDouble();
            var lowestHouseScore = HouseStrength(AllHousesOrderedByStrength(birthTime)[11], birthTime).ToDouble();

            var rangeBasedScore = houseStrength.Remap(lowestHouseScore, highestHouseScore, -3, 3);


            return rangeBasedScore;
        }

        /// <summary>
        /// Based on Shadvarga get nature of planet for a person,
        /// nature in number form to for easy calculation into summary
        /// good = 1, bad = -1, neutral = 0
        /// specially made method for life chart summary
        /// </summary>
        public static int PlanetNatureScore(Time personBirthTime, PlanetName inputPlanet)
        {
            //get house score
            var planetStrength = PlanetShadbalaPinda(inputPlanet, personBirthTime).ToDouble();


            //based on score determine final nature
            switch (planetStrength)
            {
                //positive
                case > 550: return 2; //extra for power
                case >= 450: return 1;

                //negative
                case < 250: return -3; //if below is even worse
                case < 350: return -2; //if below is even worse
                case < 450: return -1;
                default:
                    throw new Exception("No Strength Power defined!");
            }
        }


        /// <summary>
        /// Used for judging dasa good or bad, Bala book pg 110
        /// output range -5 to 5
        /// </summary>
        public static double PlanetIshtaKashtaScoreDegree(PlanetName planet, Time birthTime)
        {
            //get both scores of good and bad
            var ishtaScore = PlanetIshtaScore(planet: planet, birthTime: birthTime);
            var kashtaScore = PlanetKashtaScore(planet: planet, birthTime: birthTime);

            //final nature of event
            var ishtaMore = ishtaScore > kashtaScore;

            //NOTE: ASTRO THEORY
            //caculate the difference between Good and Bad scores in percentage
            //so the more difference there is the greater the Good or Bad
            //if the difference is very small, than it makes sense that they
            //should cancel each other.
            var baseVal = ishtaMore ? ishtaScore : kashtaScore;
            var difference = Math.Abs(value: ishtaScore - kashtaScore);
            var ratio = difference / baseVal;
            var percentage = ratio * 100;

            var finalVal = 0.0;
            if (ishtaMore)
            {
                //remap the
                finalVal = percentage.Remap(fromMin: 0, fromMax: 100, toMin: 0, toMax: 4);
            }
            else
            {
                //remap the
                finalVal = percentage.Remap(fromMin: 0, fromMax: 100, toMin: -4, toMax: 0);
            }

            return Math.Round(finalVal, 3);
        }

        /// <summary>
        /// Kashta Phala (Bad Strength) of a Planet
        /// </summary>
        public static double PlanetKashtaScore(PlanetName planet, Time birthTime)
        {
            //The Ochcha Bala (exaltation strength) of a planet
            //is multiplied by its Chesta Bala(motional strength)
            //and then the square root of the product extracted.
            var ochchaBala = PlanetOchchaBala(planet, birthTime).ToDouble();
            var chestaBala = PlanetChestaBala(planet, birthTime, true).ToDouble();
            var product = (60 - ochchaBala) * (60 - chestaBala);

            //Square root of the product extracted.
            //the result would represent the Kashta Phala.
            var ishtaScore = Math.Sqrt(product);

            return ishtaScore;

        }

        /// <summary>
        /// Ishta Phala (Good Strength) of a Planet
        /// </summary>
        public static double PlanetIshtaScore(PlanetName planet, Time birthTime)
        {
            //The Ochcha Bala (exaltation strength) of a planet
            //is multiplied by its Chesta Bala(motional strength)
            //and then the square root of the product extracted.
            var ochchaBala = PlanetOchchaBala(planet, birthTime).ToDouble();
            var chestaBala = PlanetChestaBala(planet, birthTime, true).ToDouble();
            var product = ochchaBala * chestaBala;

            //Square root of the product extracted.
            //the result would represent the Ishta Phala.
            var ishtaScore = Math.Sqrt(product);

            return ishtaScore;
        }


        #endregion

        #region UPAGRAHA

        /// <summary>
        /// Dhuma Sun' s longitude + 133°20’
        /// </summary>
        public static Angle DhumaLongitude(Time time)
        {
            //get sun long
            var sunLong = Calculate.PlanetNirayanaLongitude(Sun, time);

            //add 133°20’
            var _133 = new Angle(133, 20, 0);
            var total = _133 + sunLong;

            return total.Normalize360();
        }

        /// <summary>
        /// 360°-Dhuma's longitude
        /// </summary>
        public static Angle VyatipaataLongitude(Time time)
        {
            //get needed longitude
            var dhumaLong = Calculate.DhumaLongitude(time);

            //calculate final
            var total = Angle.Degrees360 - dhumaLong;

            return total.Normalize360();
        }

        /// <summary>
        /// Vyatipaata's longitude + 180°
        /// </summary>
        public static Angle PariveshaLongitude(Time time)
        {
            //get needed longitude
            var longitude = Calculate.VyatipaataLongitude(time);

            //calculate final
            var total = longitude + Angle.Degrees180;

            return total.Normalize360();
        }

        /// <summary>
        /// 360° - Parivesha's longitude
        /// </summary>
        public static Angle IndrachaapaLongitude(Time time)
        {
            //get needed longitude
            var longitude = Calculate.PariveshaLongitude(time);

            //calculate final
            var total = Angle.Degrees360 - longitude;

            return total.Normalize360();
        }

        /// <summary>
        /// Indrachaapa's longitude + 16°40'
        /// </summary>
        public static Angle UpaketuLongitude(Time time)
        {
            //get needed longitude
            var longitude = Calculate.IndrachaapaLongitude(time);

            //calculate final
            var _1640 = new Angle(16, 40, 0);
            var total = longitude + _1640;

            return total.Normalize360();
        }

        /// <summary>
        /// Kaala rises at the middle of Sun's part. In other words,
        /// we find the time at the middle of Sun's part
        /// and find lagna rising then. That gives Kaala's longitude.
        /// </summary>
        public static Angle KaalaLongitude(Time time) => UpagrahaLongitude(time, PlanetNameEnum.Sun, "middle");

        /// <summary>
        /// Mrityu rises at the middle of Mars's part.
        /// </summary>
        public static Angle MrityuLongitude(Time time) => UpagrahaLongitude(time, PlanetNameEnum.Mars, "middle");

        /// <summary>
        /// Artha Praharaka rises at the middle of Mercury's part. 
        /// </summary>
        public static Angle ArthaprahaaraLongitude(Time time) => UpagrahaLongitude(time, PlanetNameEnum.Mercury, "middle");

        /// <summary>
        /// Yama ghantaka rises at the middle of Jupiter's part
        /// </summary>
        public static Angle YamaghantakaLongitude(Time time) => UpagrahaLongitude(time, PlanetNameEnum.Jupiter, "middle");

        /// <summary>
        /// Gulika rises at the middle of Saturn's part. 
        /// </summary>
        public static Angle GulikaLongitude(Time time) => UpagrahaLongitude(time, PlanetNameEnum.Saturn, "begin");

        /// <summary>
        /// Maandi rises at the beginning of Saturn's part.
        /// </summary>
        public static Angle MaandiLongitude(Time time) => UpagrahaLongitude(time, PlanetNameEnum.Saturn, "middle");

        /// <summary>
        /// Calculates longitudes for the non sun based Upagrahas (sub-planets)
        /// </summary>
        public static Angle UpagrahaLongitude(Time time, PlanetNameEnum relatedPlanet, string upagrahaPart)
        {
            // Once we divide the day/night of birth into 8 equal parts and identify the
            // ruling planets of the 8 parts, we can find the longitudes of Kaala etc upagrahas
            var partNumber = UpagrahaPartNumber(time, relatedPlanet); //since Kaala->Sun

            //ascertain if day birth or night birth
            var isDayBirth = Calculate.IsDayBirth(time);

            var adjustedPartNumber = partNumber - 1; //decrement part number to calculate start time of part interested in

            //calculated duration of day based on sunrise and sunset
            var dayDuration = Calculate.DayDurationHours(time);
            //since there are 8 parts, hours per part is roughly ~1.5
            var hoursPerPart = dayDuration / 8.0;

            //place to store all longitudes for house 1 (lagna)
            House lagnaLongitudes;

            //# Based on night or day birth calculate the
            //longitude based on lagna position at given part number

            //day birth
            if (isDayBirth)
            {
                //get time the part starts after sunrise before sunset
                var hoursAfterSunrise = adjustedPartNumber * hoursPerPart;

                //calculate start time based on sunrise
                var sunrise = Calculate.SunriseTime(time);
                var partStartTime = sunrise.AddHours(hoursAfterSunrise);

                //calculate middle point in time of part (~1.5/2 = ~0.75 hours)
                var hoursPerHalfPart = hoursPerPart / 2;
                var partMiddleTime = partStartTime.AddHours(hoursPerHalfPart);

                //get lagna longitude at this middle time, which is the sub planet's long
                //NOTE ASSUMPITION: only possible values "middle" or "begin"
                var selectedPart = upagrahaPart == "middle" ? partMiddleTime :
                    upagrahaPart == "begin" ? partStartTime : throw new Exception("END OF LINE!");
                var allHouseMiddleLongitudes = Calculate.AllHouseLongitudes(selectedPart);
                lagnaLongitudes = allHouseMiddleLongitudes.Where(x => x.GetHouseName() == HouseName.House1).First();
            }
            //nigth birth
            else
            {
                //get time the part starts after sunset before sunrise next day
                var hoursAfterSunset = adjustedPartNumber * hoursPerPart;

                //calculate start time based on sunrise
                var sunset = Calculate.SunsetTime(time);
                var partStartTime = sunset.AddHours(hoursAfterSunset);

                //calculate middle point in time of part (~1.5/2 = ~0.75 hours)
                var hoursPerHalfPart = hoursPerPart / 2;
                var partMiddleTime = partStartTime.AddHours(hoursPerHalfPart);

                //get lagna longitude at this middle time, which is the sub planet's long
                //NOTE ASSUMPITION: only possible values "middle" or "begin"
                var selectedPart = upagrahaPart == "middle" ? partMiddleTime :
                    upagrahaPart == "begin" ? partStartTime : throw new Exception("END OF LINE!");
                var allHouseMiddleLongitudes = Calculate.AllHouseLongitudes(selectedPart);
                lagnaLongitudes = allHouseMiddleLongitudes.Where(x => x.GetHouseName() == HouseName.House1).First();
            }

            return lagnaLongitudes.GetMiddleLongitude();

        }

        /// <summary>
        /// Depending on whether one is born during the day or the night, we divide the
        /// length of the day/night into 8 equal parts. Each part is assigned a planet.
        /// Given a planet and time the part number will be returned.
        /// Each part is 12/8 = 1.5 hours.
        /// </summary>
        public static int UpagrahaPartNumber(Time inputTime, PlanetNameEnum inputPlanet)
        {
            //based on night or day birth get the number accoridngly
            var isDayBirth = Calculate.IsDayBirth(inputTime);

            if (isDayBirth)
            {
                return UpagrahaPartNumberDayBirth(inputTime, inputPlanet);
            }
            else
            {
                return UpagrahaPartNumberNightBirth(inputTime, inputPlanet);
            }


            //------------------LOCAL FUNCS-------------------------

            int UpagrahaPartNumberNightBirth(Time inputTime, PlanetNameEnum inputPlanet)
            {
                //get weekday
                var weekday = Calculate.DayOfWeek(inputTime);

                //based on weekday and planet name return part number
                //NOTE: table data from 
                Dictionary<DayOfWeek, Dictionary<PlanetNameEnum, int>> nightRulers = new Dictionary<DayOfWeek, Dictionary<PlanetNameEnum, int>>
            {
                { Library.DayOfWeek.Sunday, new Dictionary<PlanetNameEnum, int>
                    { { PlanetNameEnum.Jupiter, 1 }, { PlanetNameEnum.Venus, 2 }, { PlanetNameEnum.Saturn, 3 }, { PlanetNameEnum.Empty, 4 }, { PlanetNameEnum.Sun, 5 }, { PlanetNameEnum.Moon, 6 }, { PlanetNameEnum.Mars ,7 }, { PlanetNameEnum.Mercury ,8 } }
                },
                { Library.DayOfWeek.Monday, new Dictionary<PlanetNameEnum, int>
                    { { PlanetNameEnum.Venus, 1 }, { PlanetNameEnum.Saturn, 2 }, { PlanetNameEnum.Empty, 3 }, { PlanetNameEnum.Sun, 4 }, { PlanetNameEnum.Moon, 5 }, { PlanetNameEnum.Mars, 6 }, { PlanetNameEnum.Mercury, 7 }, { PlanetNameEnum.Jupiter ,8 } }
                },
                { Library.DayOfWeek.Tuesday, new Dictionary<PlanetNameEnum, int>
                    { { PlanetNameEnum.Saturn, 1 }, { PlanetNameEnum.Empty, 2 }, { PlanetNameEnum.Sun, 3 }, { PlanetNameEnum.Moon, 4 }, { PlanetNameEnum.Mars, 5 }, { PlanetNameEnum.Mercury, 6 }, { PlanetNameEnum.Jupiter, 7 }, { PlanetNameEnum.Venus ,8 } }
                },
                { Library.DayOfWeek.Wednesday, new Dictionary<PlanetNameEnum, int>
                    { { PlanetNameEnum.Sun, 1 }, { PlanetNameEnum.Moon, 2 }, { PlanetNameEnum.Mars, 3 }, { PlanetNameEnum.Mercury, 4 }, { PlanetNameEnum.Jupiter, 5 }, { PlanetNameEnum.Venus, 6 }, { PlanetNameEnum.Saturn, 7 }, { PlanetNameEnum.Empty ,8} }
                },
                { Library.DayOfWeek.Thursday, new Dictionary<PlanetNameEnum, int>
                    { { PlanetNameEnum.Moon, 1 }, { PlanetNameEnum.Mars, 2 }, { PlanetNameEnum.Mercury, 3 }, { PlanetNameEnum.Jupiter, 4 }, { PlanetNameEnum.Venus, 5 }, { PlanetNameEnum.Saturn, 6 }, { PlanetNameEnum.Empty, 7 }, { PlanetNameEnum.Sun ,8 } }
                },
                { Library.DayOfWeek.Friday, new Dictionary<PlanetNameEnum, int>
                    { { PlanetNameEnum.Mars, 1 }, { PlanetNameEnum.Mercury, 2 }, { PlanetNameEnum.Jupiter, 3 }, { PlanetNameEnum.Venus, 4 }, { PlanetNameEnum.Saturn, 5 }, { PlanetNameEnum.Empty, 6 }, { PlanetNameEnum.Sun, 7 }, { PlanetNameEnum.Moon ,8 } }
                },
                { Library.DayOfWeek.Saturday, new Dictionary<PlanetNameEnum, int>
                    { { PlanetNameEnum.Mercury, 1 }, { PlanetNameEnum.Jupiter, 2 }, { PlanetNameEnum.Venus, 3 }, { PlanetNameEnum.Saturn, 4 }, { PlanetNameEnum.Empty, 5 }, { PlanetNameEnum.Sun, 6 }, { PlanetNameEnum.Moon, 7 }, { PlanetNameEnum.Mars ,8 } }
                },

            };

                if (nightRulers.TryGetValue(weekday, out var planetParts))
                {
                    if (planetParts.TryGetValue(inputPlanet, out var partNumber))
                    {
                        return partNumber;
                    }
                    throw new Exception("Invalid planet name");
                }

                throw new Exception("Invalid day of week");
            }


            int UpagrahaPartNumberDayBirth(Time inputTime, PlanetNameEnum inputPlanet)
            {
                //get weekday
                var weekday = Calculate.DayOfWeek(inputTime);

                //based on weekday and planet name return part number
                //NOTE: table data from 
                Dictionary<DayOfWeek, Dictionary<PlanetNameEnum, int>> dayRulers = new Dictionary<DayOfWeek, Dictionary<PlanetNameEnum, int>>
            {
                { Library.DayOfWeek.Sunday, new Dictionary<PlanetNameEnum, int>
                    { { PlanetNameEnum.Sun, 1 }, { PlanetNameEnum.Moon, 2 }, { PlanetNameEnum.Mars ,3 }, { PlanetNameEnum.Mercury ,4 }, { PlanetNameEnum.Jupiter ,5 }, { PlanetNameEnum.Venus ,6 }, { PlanetNameEnum.Saturn ,7 }, { PlanetNameEnum.Empty ,8 } }
                },
                { Library.DayOfWeek.Monday, new Dictionary<PlanetNameEnum, int>
                    { { PlanetNameEnum.Moon, 1 }, { PlanetNameEnum.Mars ,2 }, { PlanetNameEnum.Mercury ,3 }, { PlanetNameEnum.Jupiter ,4},  {PlanetNameEnum.Venus ,5},  {PlanetNameEnum.Saturn ,6},  {PlanetNameEnum.Empty ,7}, { PlanetNameEnum.Sun ,8 } }
                },
                { Library.DayOfWeek.Tuesday, new Dictionary<PlanetNameEnum, int>
                    { { PlanetNameEnum.Mars, 1 }, { PlanetNameEnum.Mercury ,2 }, { PlanetNameEnum.Jupiter ,3 }, { PlanetNameEnum.Venus ,4},  {PlanetNameEnum.Saturn ,5},  {PlanetNameEnum.Empty ,6},  {PlanetNameEnum.Sun ,7}, { PlanetNameEnum.Moon ,8 } }
                },
                { Library.DayOfWeek.Wednesday, new Dictionary<PlanetNameEnum, int>
                    { { PlanetNameEnum.Mercury, 1 }, { PlanetNameEnum.Jupiter ,2 }, { PlanetNameEnum.Venus ,3 }, { PlanetNameEnum.Saturn ,4},  {PlanetNameEnum.Empty ,5},  {PlanetNameEnum.Sun ,6},  {PlanetNameEnum.Moon ,7}, { PlanetNameEnum.Mars ,8 } }
                },
                { Library.DayOfWeek.Thursday, new Dictionary<PlanetNameEnum, int>
                    { { PlanetNameEnum.Jupiter, 1 }, { PlanetNameEnum.Venus ,2 }, { PlanetNameEnum.Saturn ,3 }, { PlanetNameEnum.Empty ,4},  {PlanetNameEnum.Sun ,5},  {PlanetNameEnum.Moon ,6},  {PlanetNameEnum.Mars ,7}, { PlanetNameEnum.Mercury ,8 } }
                },
                { Library.DayOfWeek.Friday, new Dictionary<PlanetNameEnum, int>
                    { { PlanetNameEnum.Venus, 1 }, { PlanetNameEnum.Saturn ,2 }, { PlanetNameEnum.Empty ,3 }, { PlanetNameEnum.Sun ,4},  {PlanetNameEnum.Moon ,5},  {PlanetNameEnum.Mars ,6},  {PlanetNameEnum.Mercury ,7}, { PlanetNameEnum.Jupiter ,8 } }
                },
                { Library.DayOfWeek.Saturday, new Dictionary<PlanetNameEnum, int>
                    { { PlanetNameEnum.Saturn, 1 }, { PlanetNameEnum.Empty ,2 }, { PlanetNameEnum.Sun ,3 }, { PlanetNameEnum.Moon ,4},  {PlanetNameEnum.Mars ,5},  {PlanetNameEnum.Mercury ,6},  {PlanetNameEnum.Jupiter ,7}, { PlanetNameEnum.Venus ,8 } }
                },
            };


                if (dayRulers.TryGetValue(weekday, out var planetParts))
                {
                    if (planetParts.TryGetValue(inputPlanet, out var partNumber))
                    {
                        return partNumber;
                    }
                    throw new Exception("Invalid planet name");
                }

                throw new Exception("Invalid day of week");
            }

        }

        /// <summary>
        /// Given a planet name will tell if it is an Upagraha planet
        /// </summary>
        public static bool IsUpagraha(PlanetName planet)
        {
            var planetName = planet.Name;
            switch (planetName)
            {
                case PlanetNameEnum.Dhuma:
                case PlanetNameEnum.Vyatipaata:
                case PlanetNameEnum.Parivesha:
                case PlanetNameEnum.Indrachaapa:
                case PlanetNameEnum.Upaketu:
                case PlanetNameEnum.Kaala:
                case PlanetNameEnum.Mrityu:
                case PlanetNameEnum.Arthaprahaara:
                case PlanetNameEnum.Yamaghantaka:
                case PlanetNameEnum.Gulika:
                case PlanetNameEnum.Maandi:
                    return true;
            }

            //if control reaches here than must be normal planet
            return false;
        }

        #endregion

        #region CACHED FUNCTIONS
        //NOTE : These are functions that don't call other functions from this class
        //       Only functions that don't call other cached functions are allowed to be cached
        //       otherwise, it's erroneous in parallel

        /// <summary>
        /// Gets nutation from Swiss Ephemeris
        public static double Nutation(Time time)
        {
            SwissEph swissEph = new SwissEph();
            double[] x = new double[6];
            string serr = "";

            var julDayUt = Calculate.TimeToJulianDay(time);

            swissEph.swe_calc(julDayUt, SwissEph.SE_ECL_NUT, 0, x, ref serr);
            return x[2]; //See SWISS EPH docs and confirm array location - is it 1 or 2??
        }

        /// <summary>
        /// This method is used to convert the tropical ascendant to the ARMC (Ascendant Right Meridian Circle).
        /// It first calculates the right ascension and declination using the provided tropical ascendant and
        /// obliquity of the ecliptic. Then, it calculates the oblique ascension by subtracting a value derived
        /// from the declination and geographic latitude from the right ascension. Finally, it calculates the ARMC
        /// based on the value of the tropical ascendant and the oblique ascension.
        /// </summary>
        public static double AscendantDegreesToARMC(double ascendant, double obliquityOfEcliptic, double geographicLatitude, Time time)
        {
            //NEEDS UPDATE CP

            // The main method is taken from a post by K S Upendra on Group.IO in 2019
            // Calculate the right ascension using the formula:
            // atan(cos(obliquityOfEcliptic) * tan(tropicalAscendant))
            double rightAscension = 4.98;

            // Calculate the declination using the formula:
            // asin(sin(obliquityOfEcliptic) * sin(tropicalAscendant))
            double declination = 6.64;

            // Calculate the oblique ascension by subtracting the result of the following formula from the right ascension:
            // asin(tan(declination) * tan(geographicLatitude))
            double obliqueAscension = rightAscension -
                                      (Math.Asin(Math.Tan(declination * Math.PI / 180) *
                                                 Math.Tan(geographicLatitude * Math.PI / 180)) * 180 / Math.PI);
            // Initialize the armc variable
            double armc = 0;
            // Depending on the value of the tropical ascendant, calculate the armc using the formula:
            // armc = 270 + obliqueAscension or armc = 90 + obliqueAscension
            if (ascendant >= 0 && ascendant < 90)
            {
                armc = 270 + obliqueAscension;
            }
            else if (ascendant >= 90 && ascendant < 180)
            {
                armc = 90 + obliqueAscension;
            }
            else if (ascendant >= 180 && ascendant < 270)
            {
                armc = 90 + obliqueAscension;
            }
            else if (ascendant >= 270 && ascendant < 360)
            {
                armc = 270 + obliqueAscension;
            }
            // Return the calculated armc value
            return armc;
        }

        /// <summary>
        /// The distance between the Hindu First Point and the Vernal Equinox, measured at an epoch, is known as the Ayanamsa
        /// in Varahamihira's time, the summer solistice coincided with the first degree of Cancer,
        /// and the winter solistice with the first degree of Capricorn, whereas at one time the summer solistice coincided with the
        /// middle of the Aslesha
        /// </summary>
        public static Angle AyanamsaDegree(Time time)
        {

            //it has been observed and proved mathematically, that each year at the time when the Sun reaches his
            //equinoctial point of Aries 0° when throughout the earth, the day and night are equal in length,
            //the position of the earth in reference to some fixed star is nearly 50.333 of space farther west
            //than the earth was at the same equinoctial moment of the previous year.


            //CACHE MECHANISM
            return CacheManager.GetCache(new CacheKey(nameof(AyanamsaDegree), time, Ayanamsa), _getAyanamsaDegree);


            //UNDERLYING FUNCTION
            Angle _getAyanamsaDegree()
            {
                //get ayanamsa from swiss for all except Raman,
                //becasue swiss does not match with Raman's book
                if (Calculate.Ayanamsa == (int)Library.Ayanamsa.RAMAN)
                {
                    return calculateRamanAyanamsa(time);
                }
                else
                {
                    return getAyanamsaFromSwissEphemeris(time);
                }

            }

            //gets ayanamsa from swiss eph
            Angle getAyanamsaFromSwissEphemeris(Time time)
            {
                //This would request sidereal positions calculated using the Swiss Ephemeris.
                int iflag = SwissEph.SEFLG_SIDEREAL;
                //int iflag = SwissEph.SEFLG_NONUT;
                double jul_day_ET;
                SwissEph ephemeris = new SwissEph();

                // Convert DOB to ET
                jul_day_ET = TimeToJulianEphemerisTime(time);

                //set ayanamsa
                ephemeris.swe_set_sid_mode(Ayanamsa, 0, 0);

                //USE this newer method in Swiss Eph introduced in Ver 2.0. See Swiss Eph for Documentation
                //CPJ Add/Change Nov 22 2023 because Ayanamsa not precise compared to other software products
                //this provides higher precision Ayanamsa
                string serr = ""; //buffer to capture error messages
                double daya;
                var ayanamsaDegree = ephemeris.swe_get_ayanamsa_ex(jul_day_ET, iflag, out daya, ref serr);

                return Angle.FromDegrees(daya);

            }

            //manually calculates Raman ayanamsa to match with : Article 49 - Manual Of Hindu Astrology - pg 22
            Angle calculateRamanAyanamsa(Time time)
            {
                int year = Calculate.LmtToUtc(time).Year;

                //it has been observed and proved mathematically, that each year at the time when the Sun reaches his
                //equinoctial point of Aries 0° when throughout the earth, the day and night are equal in length,
                //the position of the earth in reference to some fixed star is nearly 50.333 of space farther west
                //than the earth was at the same equinoctial moment of the previous year.
                const double precessionRate = 50.3333333333;

                // B.V.Raman accepted 397 AD as the Zero Ayanamsa Year 
                const int yearOfCoincidence = 397;

                var ayanamsaSecondsRaw = (year - yearOfCoincidence) * precessionRate;
                var returnValue = new Angle(seconds: (long)(Math.Round(ayanamsaSecondsRaw)));

                return returnValue;
            }


        }

        /// <summary>
        /// Get fixed longitude used in western systems, connects SwissEph Library with VedAstro
        /// NOTE This method connects SwissEph Library with VedAstro Library
        /// </summary>
        public static Angle PlanetSayanaLongitude(PlanetName planetName, Time time)
        {

            //CACHE MECHANISM
            return CacheManager.GetCache(new CacheKey(nameof(PlanetSayanaLongitude), time, planetName, Ayanamsa), _getPlanetSayanaLongitude);


            //UNDERLYING FUNCTION

            Angle _getPlanetSayanaLongitude()
            {
                int iflag = SwissEph.SEFLG_SWIEPH;

                double[] results = new double[6];
                string err_msg = "";
                SwissEph ephemeris = new SwissEph();

                // Convert DOB to ET
                double jul_day_ET = TimeToJulianEphemerisTime(time);

                //convert planet name, compatible with Swiss Eph
                int swissPlanet = Tools.VedAstroToSwissEph(planetName);

                //Get planet long
                int ret_flag = ephemeris.swe_calc(jul_day_ET, swissPlanet, iflag, results, ref err_msg);

                //data in results at index 0 is longitude
                var planetSayanaLongitude = Angle.FromDegrees(results[0]);

                //if ketu add 180 to rahu
                if (planetName == Ketu)
                {
                    var x = planetSayanaLongitude + Angle.Degrees180;
                    planetSayanaLongitude = x.Normalize360();
                }

                return planetSayanaLongitude;

            }


        }

        /// <summary>
        /// Planet longitude that has been corrected with Ayanamsa
        /// Gets planet longitude used vedic astrology
        /// Nirayana Longitude = Sayana Longitude corrected to Ayanamsa
        /// Number from 0 to 360, represent the degrees in the zodiac as viewed from earth
        /// Note: Since Nirayana is corrected, in actuality 0 degrees will start at Taurus not Aries
        /// </summary>
        public static Angle PlanetNirayanaLongitude(PlanetName planetName, Time time)
        {

            //CACHE MECHANISM
            return CacheManager.GetCache(new CacheKey(nameof(PlanetNirayanaLongitude), time, planetName, Ayanamsa), _getPlanetNirayanaLongitude);


            //UNDERLYING FUNCTION

            Angle _getPlanetNirayanaLongitude()
            {
                //if Upagrahas hadle seperately
                if (Calculate.IsUpagraha(planetName))
                {
                    //calculate upagraha
                    switch (planetName.Name)
                    {
                        case PlanetNameEnum.Dhuma: return Calculate.DhumaLongitude(time);
                        case PlanetNameEnum.Vyatipaata: return Calculate.VyatipaataLongitude(time);
                        case PlanetNameEnum.Parivesha: return Calculate.PariveshaLongitude(time);
                        case PlanetNameEnum.Indrachaapa: return Calculate.IndrachaapaLongitude(time);
                        case PlanetNameEnum.Upaketu: return Calculate.UpaketuLongitude(time);
                        case PlanetNameEnum.Kaala: return Calculate.KaalaLongitude(time);
                        case PlanetNameEnum.Mrityu: return Calculate.MrityuLongitude(time);
                        case PlanetNameEnum.Arthaprahaara: return Calculate.ArthaprahaaraLongitude(time);
                        case PlanetNameEnum.Yamaghantaka: return Calculate.YamaghantakaLongitude(time);
                        case PlanetNameEnum.Gulika: return Calculate.GulikaLongitude(time);
                        case PlanetNameEnum.Maandi: return Calculate.MaandiLongitude(time);
                    }
                }

                //get ayanamsa from swiss for all except Raman,
                //becasue swiss ayanamsa does not match with Raman's book
                if (Calculate.Ayanamsa == (int)Library.Ayanamsa.RAMAN)
                {
                    return _getPlanetNirayanaLongitudeForRaman();
                }
                else
                {
                    return _getPlanetNirayanaLongitudeSwissEph();
                }



            }

            //for all other ayanamsa uses swiss
            Angle _getPlanetNirayanaLongitudeSwissEph()
            {
                //This would request sidereal (ayanamsa) positions calculated using the Swiss Ephemeris.
                int iflag = SwissEph.SEFLG_SIDEREAL | SwissEph.SEFLG_SWIEPH;
                double[] results = new double[6];
                string err_msg = "";
                double jul_day_ET;
                SwissEph ephemeris = new SwissEph();

                // Convert DOB to ET
                jul_day_ET = TimeToJulianEphemerisTime(time);

                //convert planet name, compatible with Swiss Eph
                int swissPlanet = Tools.VedAstroToSwissEph(planetName);

                //NOTE Ayanamsa needs to be set before caling calc
                ephemeris.swe_set_sid_mode(Ayanamsa, 0, 0);

                //do calculation
                int ret_flag = ephemeris.swe_calc(jul_day_ET, swissPlanet, iflag, results, ref err_msg);

                //data in results at index 0 is longitude
                var planetSayanaLongitude = Angle.FromDegrees(results[0]);

                //if ketu add 180 to rahu
                if (planetName == Ketu)
                {
                    var x = planetSayanaLongitude + Angle.Degrees180;
                    planetSayanaLongitude = x.Normalize360();
                }

                return planetSayanaLongitude;

            }

            //specialized to use calculated Raman ayanamsa
            Angle _getPlanetNirayanaLongitudeForRaman()
            {
                //declare return value
                Angle returnValue;

                //Get sayana longitude on day 
                Angle longitude = PlanetSayanaLongitude(planetName, time);

                //3 - Hindu Nirayana Long = Sayana Long — Ayanamsa.
                Angle birthAyanamsa = Calculate.AyanamsaDegree(time);

                //if below ayanamsa add 360 before minus
                returnValue = longitude.TotalDegrees < birthAyanamsa.TotalDegrees
                    ? (longitude + Angle.Degrees360) - birthAyanamsa
                    : longitude - birthAyanamsa;

                return returnValue;
            }

        }

        /// <summary>
        /// find time of next lunar eclipse UTC time
        /// </summary>
        public static DateTime NextLunarEclipse(Time time)
        {

            //CACHE MECHANISM
            return CacheManager.GetCache(new CacheKey(nameof(NextLunarEclipse), time, Ayanamsa), _getNextLunarEclipse);


            //UNDERLYING FUNCTION

            DateTime _getNextLunarEclipse()
            {
                int iflag = SwissEph.SEFLG_SWIEPH;  //+ SwissEph.SEFLG_SPEED;
                double[] results = new double[10];
                string err_msg = "";
                SwissEph ephemeris = new SwissEph();

                // Convert DOB to ET
                var jul_day_ET = Calculate.ConvertLmtToJulian(time);

                //Get planet long
                var eclipseType = 0; /* eclipse type wanted: SE_ECL_TOTAL etc. or 0, if any eclipse type */
                var backward = false; /* TRUE, if backward search */
                int ret_flag = ephemeris.swe_lun_eclipse_when(jul_day_ET, iflag, eclipseType, results, backward, ref err_msg);

                //get raw results out
                var eclipseMaxTime = results[0]; //time of maximum eclipse (Julian day number)

                //convert to UTC Time
                var utcTime = Calculate.ConvertJulianTimeToNormalTime(eclipseMaxTime);

                return utcTime;

            }


        }

        /// <summary>
        /// finds the next solar eclipse globally UTC time
        /// </summary>
        public static DateTime NextSolarEclipse(Time time)
        {

            //CACHE MECHANISM
            return CacheManager.GetCache(new CacheKey(nameof(NextSolarEclipse), time, Ayanamsa), _getNextSolarEclipse);


            //UNDERLYING FUNCTION

            DateTime _getNextSolarEclipse()
            {
                int iflag = SwissEph.SEFLG_SWIEPH;  //+ SwissEph.SEFLG_SPEED;
                double[] results = new double[10];
                string err_msg = "";
                double jul_day_ET;
                SwissEph ephemeris = new SwissEph();

                // Convert DOB to ET
                jul_day_ET = Calculate.ConvertLmtToJulian(time);

                //Get planet long
                var eclipseType = 0; /* eclipse type wanted: SE_ECL_TOTAL etc. or 0, if any eclipse type */
                var backward = false; /* TRUE, if backward search */
                int ret_flag = ephemeris.swe_sol_eclipse_when_glob(jul_day_ET, iflag, eclipseType, results, backward, ref err_msg);

                //get raw results out
                var eclipseMaxTime = results[0]; //time of maximum eclipse (Julian day number)

                //convert to UTC Time
                var utcTime = Calculate.ConvertJulianTimeToNormalTime(eclipseMaxTime);

                return utcTime;

            }


        }

        /// <summary>
        /// Get fixed longitude used in western systems aka Sayana longitude
        /// NOTE This method connects SwissEph Library with VedAstro Library
        /// </summary>
        public static Angle PlanetEphemerisLongitude(PlanetName planetName, Time time)
        {

            //CACHE MECHANISM
            return CacheManager.GetCache(new CacheKey(nameof(PlanetEphemerisLongitude), time, planetName, Ayanamsa), _getPlanetSayanaLongitude);


            //UNDERLYING FUNCTION

            Angle _getPlanetSayanaLongitude()
            {
                //Converts LMT to UTC (GMT)
                //DateTimeOffset utcDate = lmtDateTime.ToUniversalTime();

                int iflag = SwissEph.SEFLG_SWIEPH;  //+ SwissEph.SEFLG_SPEED;
                double[] results = new double[6];
                string err_msg = "";
                double jul_day_ET;
                SwissEph ephemeris = new SwissEph();

                // Convert DOB to ET
                jul_day_ET = TimeToJulianEphemerisTime(time);

                //convert planet name, compatible with Swiss Eph
                int swissPlanet = Tools.VedAstroToSwissEph(planetName);

                //Get planet long
                int ret_flag = ephemeris.swe_calc(jul_day_ET, swissPlanet, iflag, results, ref err_msg);

                //data in results at index 0 is longitude
                var planetSayanaLongitude = Angle.FromDegrees(results[0]);

                //if ketu add 180 to rahu
                if (planetName == Library.PlanetName.Ketu)
                {
                    var x = planetSayanaLongitude + Angle.Degrees180;
                    planetSayanaLongitude = x.Normalize360();
                }

                return planetSayanaLongitude;
            }


        }

        /// <summary>
        /// Gets Swiss Ephemeris longitude for a planet
        /// </summary>
        public static Angle PlanetSayanaLatitude(PlanetName planetName, Time time)
        {

            //CACHE MECHANISM
            return CacheManager.GetCache(new CacheKey(nameof(PlanetSayanaLatitude), time, planetName, Ayanamsa), _getPlanetSayanaLatitude);


            //UNDERLYING FUNCTION

            Angle _getPlanetSayanaLatitude()
            {
                //Converts LMT to UTC (GMT)
                //DateTimeOffset utcDate = lmtDateTime.ToUniversalTime();

                int planet = 0;
                int iflag = SwissEph.SEFLG_SWIEPH;  //+ SwissEph.SEFLG_SPEED;
                double[] results = new double[6];
                string err_msg = "";
                double jul_day_ET;
                SwissEph ephemeris = new SwissEph();

                // Convert DOB to ET
                jul_day_ET = TimeToJulianEphemerisTime(time);


                //Convert PlanetName to SE_PLANET type
                if (planetName == Library.PlanetName.Sun)
                    planet = SwissEph.SE_SUN;
                else if (planetName == Library.PlanetName.Moon)
                {
                    planet = SwissEph.SE_MOON;
                }
                else if (planetName == Library.PlanetName.Mars)
                {
                    planet = SwissEph.SE_MARS;
                }
                else if (planetName == Library.PlanetName.Mercury)
                {
                    planet = SwissEph.SE_MERCURY;
                }
                else if (planetName == Library.PlanetName.Jupiter)
                {
                    planet = SwissEph.SE_JUPITER;
                }
                else if (planetName == Library.PlanetName.Venus)
                {
                    planet = SwissEph.SE_VENUS;
                }
                else if (planetName == Library.PlanetName.Saturn)
                {
                    planet = SwissEph.SE_SATURN;
                }
                else if (planetName == Library.PlanetName.Rahu)
                {
                    planet = SwissEph.SE_MEAN_NODE;
                }
                else if (planetName == Library.PlanetName.Ketu)
                {
                    planet = SwissEph.SE_MEAN_NODE;
                }

                //Get planet long
                int ret_flag = ephemeris.swe_calc(jul_day_ET, planet, iflag, results, ref err_msg);

                //data in results at index 1 is latitude
                return Angle.FromDegrees(results[1]);

            }


        }

        /// <summary>
        /// Speed of planet from Swiss eph
        /// </summary>
        public static double PlanetSpeed(PlanetName planetName, Time time)
        {
            //Converts LMT to UTC (GMT)
            //DateTimeOffset utcDate = lmtDateTime.ToUniversalTime();

            int planet = 0;
            int iflag = SwissEph.SEFLG_SWIEPH | SwissEph.SEFLG_SPEED;
            double[] results = new double[6];
            string err_msg = "";
            double jul_day_ET;
            SwissEph ephemeris = new SwissEph();

            // Convert DOB to ET
            jul_day_ET = TimeToJulianEphemerisTime(time);


            //Convert PlanetName to SE_PLANET type
            if (planetName == Library.PlanetName.Sun)
                planet = SwissEph.SE_SUN;
            else if (planetName == Library.PlanetName.Moon)
            {
                planet = SwissEph.SE_MOON;
            }
            else if (planetName == Library.PlanetName.Mars)
            {
                planet = SwissEph.SE_MARS;
            }
            else if (planetName == Library.PlanetName.Mercury)
            {
                planet = SwissEph.SE_MERCURY;
            }
            else if (planetName == Library.PlanetName.Jupiter)
            {
                planet = SwissEph.SE_JUPITER;
            }
            else if (planetName == Library.PlanetName.Venus)
            {
                planet = SwissEph.SE_VENUS;
            }
            else if (planetName == Library.PlanetName.Saturn)
            {
                planet = SwissEph.SE_SATURN;
            }
            else if (planetName == Library.PlanetName.Rahu)
            {
                planet = SwissEph.SE_MEAN_NODE;
            }
            else if (planetName == Library.PlanetName.Ketu)
            {
                planet = SwissEph.SE_MEAN_NODE;
            }

            //Get planet long
            int ret_flag = ephemeris.swe_calc(jul_day_ET, planet, iflag, results, ref err_msg);

            //data in results at index 3 is speed in right ascension (deg/day)
            return results[3];
        }

        /// <summary>
        /// Converts Planet Longitude to Constellation equivelant
        /// Gets info about the constellation at a given longitude, ie. Constellation Name,
        /// Quarter, Degrees in constellation, etc.
        /// </summary>
        public static Constellation ConstellationAtLongitude(Angle planetLongitude)
        {

            //CACHE MECHANISM
            return CacheManager.GetCache(new CacheKey(nameof(ConstellationAtLongitude), planetLongitude, Ayanamsa), _constellationAtLongitude);


            //UNDERLYING FUNCTION
            Constellation _constellationAtLongitude()
            {
                if (planetLongitude == null) { return Library.Constellation.Empty; }

                //if planet longitude is negative means, it before aries at 0, starts back at 360 pieces
                if (planetLongitude.TotalDegrees < 0)
                {
                    planetLongitude = Angle.FromDegrees(360.0 + planetLongitude.TotalDegrees); //use plus because number is already negative
                }

                //get planet's longitude in minutes
                var planetLongitudeInMinutes = planetLongitude.TotalMinutes;

                //The ecliptic is divided into 27 constellations
                //of 13° 20' (800') each. Hence divide 800
                var roughConstellationNumber = planetLongitudeInMinutes / 800.0;

                //get constellation number (rounds up)
                var constellationNumber = (int)Math.Ceiling(roughConstellationNumber);

                //if constellation number = 0, then its 1 - CPJ Added to handle 0 degree longitude items
                if (constellationNumber == 0) { constellationNumber = 1; }

                //calculate quarter from remainder
                int quarter;

                var remainder = roughConstellationNumber - Math.Floor(roughConstellationNumber);

                //CPJ Amnded Code - March 13, 2024 - changed the upper limit not to be <= 0.25 but only < 0.25. 
                //This returns the Pada correctly. Try edge case Long 270 degrees. It is the start of U-Ashada Pada 2.
                //The equal to value is the lower limit of each case below
                if (remainder >= 0 && remainder < 0.25) quarter = 1;
                else if (remainder >= 0.25 && remainder < 0.5) quarter = 2;
                else if (remainder >= 0.5 && remainder < 0.75) quarter = 3;
                else if (remainder >= 0.75 && remainder <= 1) quarter = 4;
                else quarter = 0;

                //calculate "degrees in constellation" from the remainder
                var minutesInConstellation = remainder * 800.0;
                var degreesInConstellation = new Angle(0, minutesInConstellation, 0);

                var constellation = new Constellation();
                //put together all the info of this point in the constellation
                //CPJ Added Code Change - March 13, 2024 - to fix an error with edge cases - example 266.666667Long results in remainder = 0.
                //CPJ - When remainder = 0, new Constellation should return next Constellation Pada 1. Hence the if-else code change
                if (minutesInConstellation == 0)
                {
                    constellation = new Constellation((constellationNumber + 1), quarter, degreesInConstellation);
                }
                else
                {

                    constellation = new Constellation(constellationNumber, quarter, degreesInConstellation);
                }

                //return constellation value
                return constellation;
            }

        }


        /// <summary>
        /// Converts Planet Longitude to Zodiac Sign equivalent
        /// </summary>
        public static ZodiacSign ZodiacSignAtLongitude(Angle longitude)
        {

            //CACHE MECHANISM
            return CacheManager.GetCache(new CacheKey(nameof(ZodiacSignAtLongitude), longitude, Ayanamsa), _zodiacSignAtLongitude);


            //UNDERLYING FUNCTION
            ZodiacSign _zodiacSignAtLongitude()
            {
                //max degrees of each sign
                const double maxDegreesInSign = 30.0;

                // Adjust longitude to be within 0-360 range
                double adjustedLongitude = longitude.TotalDegrees;
                while (adjustedLongitude < 0)
                {
                    adjustedLongitude += 360.0;
                }
                //get rough zodiac number
                double roughZodiacNumber = (adjustedLongitude % 360.0) / maxDegreesInSign;

                //Calculate degrees in zodiac sign
                //get remainder from rough zodiac number
                var roughZodiacNumberRemainder = roughZodiacNumber - Math.Truncate(roughZodiacNumber);

                //convert remainder to degrees in current sign
                var degreesInSignRaw = roughZodiacNumberRemainder * maxDegreesInSign;

                //round number (too high accuracy causes equality mismtach because of minute difference)
                var degreesInSign = Math.Round(degreesInSignRaw, 7);

                //Get name of zodiac sign
                //round to ceiling to get integer zodiac number
                var zodiacNumber = (int)Math.Ceiling(roughZodiacNumber);
                if (adjustedLongitude == 0.00) { zodiacNumber = 1; }

                //convert zodiac number to zodiac name
                var calculatedZodiac = (ZodiacName)zodiacNumber;

                //return new instance of planet sign
                var degreesAngle = Angle.FromDegrees(Math.Abs(degreesInSign)); //make always positive

                var zodiacSignAtLongitude = new ZodiacSign(calculatedZodiac, degreesAngle);
                return zodiacSignAtLongitude;
            }


        }

        /// <summary>
        /// Converts Zodiac Sign to Planet Longitude equivalent
        /// </summary>
        public static Angle LongitudeAtZodiacSign(ZodiacSign zodiacSign)
        {

            //CACHE MECHANISM
            return CacheManager.GetCache(new CacheKey(nameof(LongitudeAtZodiacSign), zodiacSign, Ayanamsa), _getLongitudeAtZodiacSign);


            //UNDERLYING FUNCTION
            Angle _getLongitudeAtZodiacSign()
            {
                //convert zodic name to its number equivelant in order
                var zodiacNumber = (int)zodiacSign.GetSignName();

                //calculate planet longitude to sign just before
                var zodiacBefore = zodiacNumber - 1;
                var maxDegreesInSign = 30.0;
                var longtiudeToBefore = Angle.FromDegrees(maxDegreesInSign * zodiacBefore);

                //add planet longitude from sign just before with
                //degrees already traversed in current sign
                var totalLongitude = longtiudeToBefore + zodiacSign.GetDegreesInSign();

                return totalLongitude;
            }


        }

        /// <summary>
        /// Get Vedic Day Of Week
        /// The Hindu day begins with sunrise and continues till
        /// next sunrise.The first hora on any day will be the
        /// first hour after sunrise and the last hora, the hour
        /// before sunrise the next day.
        /// </summary>
        public static DayOfWeek DayOfWeek(Time time)
        {
            //CACHE MECHANISM
            return CacheManager.GetCache(new CacheKey(nameof(DayOfWeek), time, Ayanamsa), _getDayOfWeek);


            //UNDERLYING FUNCTION
            DayOfWeek _getDayOfWeek()
            {
                // The Hindu day begins with sunrise and continues till
                // next sunrise. The first hora on any day will be the
                // first hour after sunrise and the last hora, the hour
                // before sunrise the next day.

                //TODO NEEDS VERIFICATION

                var sunRise = Calculate.SunriseTime(time);

                // If the current time is after today's sunrise and before tomorrow's sunrise,
                // then it is still considered today.
                var tomorrowSunrise = Calculate.SunriseTime(time.AddHours(24)).GetLmtDateTimeOffset();
                var todaySunrise = sunRise.GetLmtDateTimeOffset();
                var lmtTime = time.GetLmtDateTimeOffset();

                if (lmtTime >= todaySunrise && lmtTime < tomorrowSunrise)
                {
                    //get week day name in string
                    var dayOfWeekNameInString = lmtTime.DayOfWeek.ToString();

                    //convert string to day of week type
                    Enum.TryParse(dayOfWeekNameInString, out DayOfWeek dayOfWeek);

                    return dayOfWeek;
                }
                else
                {
                    //get week day name in string
                    var dayOfWeekNameInString = lmtTime.AddDays(-1).DayOfWeek.ToString();

                    //convert string to day of week type
                    Enum.TryParse(dayOfWeekNameInString, out DayOfWeek dayOfWeek);

                    // If the current time is before today's sunrise, then it is considered the previous day.
                    return dayOfWeek;
                }
            }


        }

        /// <summary>
        /// Gets hora lord based on hora number & week day
        /// </summary>
        public static PlanetName LordOfHoraFromWeekday(int hora, DayOfWeek day)
        {
            switch (day)
            {
                case Library.DayOfWeek.Sunday:
                    switch (hora)
                    {
                        case 1: return Library.PlanetName.Sun;
                        case 2: return Library.PlanetName.Venus;
                        case 3: return Library.PlanetName.Mercury;
                        case 4: return Library.PlanetName.Moon;
                        case 5: return Library.PlanetName.Saturn;
                        case 6: return Library.PlanetName.Jupiter;
                        case 7: return Library.PlanetName.Mars;
                        case 8: return Library.PlanetName.Sun;
                        case 9: return Library.PlanetName.Venus;
                        case 10: return Library.PlanetName.Mercury;
                        case 11: return Library.PlanetName.Moon;
                        case 12: return Library.PlanetName.Saturn;
                        case 13: return Library.PlanetName.Jupiter;
                        case 14: return Library.PlanetName.Mars;
                        case 15: return Library.PlanetName.Sun;
                        case 16: return Library.PlanetName.Venus;
                        case 17: return Library.PlanetName.Mercury;
                        case 18: return Library.PlanetName.Moon;
                        case 19: return Library.PlanetName.Saturn;
                        case 20: return Library.PlanetName.Jupiter;
                        case 21: return Library.PlanetName.Mars;
                        case 22: return Library.PlanetName.Sun;
                        case 23: return Library.PlanetName.Venus;
                        case 24: return Library.PlanetName.Mercury;
                    }
                    break;
                case Library.DayOfWeek.Monday:
                    switch (hora)
                    {
                        case 1: return Library.PlanetName.Moon;
                        case 2: return Library.PlanetName.Saturn;
                        case 3: return Library.PlanetName.Jupiter;
                        case 4: return Library.PlanetName.Mars;
                        case 5: return Library.PlanetName.Sun;
                        case 6: return Library.PlanetName.Venus;
                        case 7: return Library.PlanetName.Mercury;
                        case 8: return Library.PlanetName.Moon;
                        case 9: return Library.PlanetName.Saturn;
                        case 10: return Library.PlanetName.Jupiter;
                        case 11: return Library.PlanetName.Mars;
                        case 12: return Library.PlanetName.Sun;
                        case 13: return Library.PlanetName.Venus;
                        case 14: return Library.PlanetName.Mercury;
                        case 15: return Library.PlanetName.Moon;
                        case 16: return Library.PlanetName.Saturn;
                        case 17: return Library.PlanetName.Jupiter;
                        case 18: return Library.PlanetName.Mars;
                        case 19: return Library.PlanetName.Sun;
                        case 20: return Library.PlanetName.Venus;
                        case 21: return Library.PlanetName.Mercury;
                        case 22: return Library.PlanetName.Moon;
                        case 23: return Library.PlanetName.Saturn;
                        case 24: return Library.PlanetName.Jupiter;
                    }
                    break;
                case Library.DayOfWeek.Tuesday:
                    switch (hora)
                    {
                        case 1: return Library.PlanetName.Mars;
                        case 2: return Library.PlanetName.Sun;
                        case 3: return Library.PlanetName.Venus;
                        case 4: return Library.PlanetName.Mercury;
                        case 5: return Library.PlanetName.Moon;
                        case 6: return Library.PlanetName.Saturn;
                        case 7: return Library.PlanetName.Jupiter;
                        case 8: return Library.PlanetName.Mars;
                        case 9: return Library.PlanetName.Sun;
                        case 10: return Library.PlanetName.Venus;
                        case 11: return Library.PlanetName.Mercury;
                        case 12: return Library.PlanetName.Moon;
                        case 13: return Library.PlanetName.Saturn;
                        case 14: return Library.PlanetName.Jupiter;
                        case 15: return Library.PlanetName.Mars;
                        case 16: return Library.PlanetName.Sun;
                        case 17: return Library.PlanetName.Venus;
                        case 18: return Library.PlanetName.Mercury;
                        case 19: return Library.PlanetName.Moon;
                        case 20: return Library.PlanetName.Saturn;
                        case 21: return Library.PlanetName.Jupiter;
                        case 22: return Library.PlanetName.Mars;
                        case 23: return Library.PlanetName.Sun;
                        case 24: return Library.PlanetName.Venus;
                    }
                    break;
                case Library.DayOfWeek.Wednesday:
                    switch (hora)
                    {
                        case 1: return Library.PlanetName.Mercury;
                        case 2: return Library.PlanetName.Moon;
                        case 3: return Library.PlanetName.Saturn;
                        case 4: return Library.PlanetName.Jupiter;
                        case 5: return Library.PlanetName.Mars;
                        case 6: return Library.PlanetName.Sun;
                        case 7: return Library.PlanetName.Venus;
                        case 8: return Library.PlanetName.Mercury;
                        case 9: return Library.PlanetName.Moon;
                        case 10: return Library.PlanetName.Saturn;
                        case 11: return Library.PlanetName.Jupiter;
                        case 12: return Library.PlanetName.Mars;
                        case 13: return Library.PlanetName.Sun;
                        case 14: return Library.PlanetName.Venus;
                        case 15: return Library.PlanetName.Mercury;
                        case 16: return Library.PlanetName.Moon;
                        case 17: return Library.PlanetName.Saturn;
                        case 18: return Library.PlanetName.Jupiter;
                        case 19: return Library.PlanetName.Mars;
                        case 20: return Library.PlanetName.Sun;
                        case 21: return Library.PlanetName.Venus;
                        case 22: return Library.PlanetName.Mercury;
                        case 23: return Library.PlanetName.Moon;
                        case 24: return Library.PlanetName.Saturn;
                    }
                    break;
                case Library.DayOfWeek.Thursday:
                    switch (hora)
                    {
                        case 1: return Library.PlanetName.Jupiter;
                        case 2: return Library.PlanetName.Mars;
                        case 3: return Library.PlanetName.Sun;
                        case 4: return Library.PlanetName.Venus;
                        case 5: return Library.PlanetName.Mercury;
                        case 6: return Library.PlanetName.Moon;
                        case 7: return Library.PlanetName.Saturn;
                        case 8: return Library.PlanetName.Jupiter;
                        case 9: return Library.PlanetName.Mars;
                        case 10: return Library.PlanetName.Sun;
                        case 11: return Library.PlanetName.Venus;
                        case 12: return Library.PlanetName.Mercury;
                        case 13: return Library.PlanetName.Moon;
                        case 14: return Library.PlanetName.Saturn;
                        case 15: return Library.PlanetName.Jupiter;
                        case 16: return Library.PlanetName.Mars;
                        case 17: return Library.PlanetName.Sun;
                        case 18: return Library.PlanetName.Venus;
                        case 19: return Library.PlanetName.Mercury;
                        case 20: return Library.PlanetName.Moon;
                        case 21: return Library.PlanetName.Saturn;
                        case 22: return Library.PlanetName.Jupiter;
                        case 23: return Library.PlanetName.Mars;
                        case 24: return Library.PlanetName.Sun;
                    }
                    break;
                case Library.DayOfWeek.Friday:
                    switch (hora)
                    {
                        case 1: return Library.PlanetName.Venus;
                        case 2: return Library.PlanetName.Mercury;
                        case 3: return Library.PlanetName.Moon;
                        case 4: return Library.PlanetName.Saturn;
                        case 5: return Library.PlanetName.Jupiter;
                        case 6: return Library.PlanetName.Mars;
                        case 7: return Library.PlanetName.Sun;
                        case 8: return Library.PlanetName.Venus;
                        case 9: return Library.PlanetName.Mercury;
                        case 10: return Library.PlanetName.Moon;
                        case 11: return Library.PlanetName.Saturn;
                        case 12: return Library.PlanetName.Jupiter;
                        case 13: return Library.PlanetName.Mars;
                        case 14: return Library.PlanetName.Sun;
                        case 15: return Library.PlanetName.Venus;
                        case 16: return Library.PlanetName.Mercury;
                        case 17: return Library.PlanetName.Moon;
                        case 18: return Library.PlanetName.Saturn;
                        case 19: return Library.PlanetName.Jupiter;
                        case 20: return Library.PlanetName.Mars;
                        case 21: return Library.PlanetName.Sun;
                        case 22: return Library.PlanetName.Venus;
                        case 23: return Library.PlanetName.Mercury;
                        case 24: return Library.PlanetName.Moon;
                    }
                    break;
                case Library.DayOfWeek.Saturday:
                    switch (hora)
                    {
                        case 1: return Library.PlanetName.Saturn;
                        case 2: return Library.PlanetName.Jupiter;
                        case 3: return Library.PlanetName.Mars;
                        case 4: return Library.PlanetName.Sun;
                        case 5: return Library.PlanetName.Venus;
                        case 6: return Library.PlanetName.Mercury;
                        case 7: return Library.PlanetName.Moon;
                        case 8: return Library.PlanetName.Saturn;
                        case 9: return Library.PlanetName.Jupiter;
                        case 10: return Library.PlanetName.Mars;
                        case 11: return Library.PlanetName.Sun;
                        case 12: return Library.PlanetName.Venus;
                        case 13: return Library.PlanetName.Mercury;
                        case 14: return Library.PlanetName.Moon;
                        case 15: return Library.PlanetName.Saturn;
                        case 16: return Library.PlanetName.Jupiter;
                        case 17: return Library.PlanetName.Mars;
                        case 18: return Library.PlanetName.Sun;
                        case 19: return Library.PlanetName.Venus;
                        case 20: return Library.PlanetName.Mercury;
                        case 21: return Library.PlanetName.Moon;
                        case 22: return Library.PlanetName.Saturn;
                        case 23: return Library.PlanetName.Jupiter;
                        case 24: return Library.PlanetName.Mars;
                    }
                    break;
            }

            throw new Exception("Did not find hora, something wrong!");

        }


        /// <summary>
        /// Each day starts at sunrise and ends at next day's sunrise. This period is
        /// divided into 24 equal parts and they are called horas. A hora is almost equal
        /// to an hour. These horas are ruled by different planets. The lords of hora
        /// come in the order of decreasing speed with respect to earth: Saturn, Jupiter,
        /// Mars, Sun, Venus, Mercury and Moon. After Moon, we go back to Saturn
        /// and repeat the 7 planets.
        /// </summary>
        public static PlanetName LordOfHoraFromTime(Time time)
        {
            //first ascertain the weekday of birth
            var birthWeekday = Calculate.DayOfWeek(time);

            //ascertain the number of hours elapsed from sunrise to birth
            //This shows the number of horas passed.
            var hora = Calculate.HoraAtBirth(time);

            //get lord of hora (hour)
            var lord = Calculate.LordOfHoraFromWeekday(hora, birthWeekday);

            return lord;
        }

        /// <summary>
        /// Gets the junction point (sandhi) between 2 consecutive
        /// houses, where one house begins and the other ends.
        /// </summary>
        public static Angle HouseJunctionPoint(Angle previousHouse, Angle nextHouse)
        {
            //Add the longitudes of two consecutive Bhavas (house)
            //and divide the sum by 2. The result represents sandhi (junction point of houses).

            // Normalize the house longitudes to ensure they are within 0-360°
            previousHouse = previousHouse.Normalize360();
            nextHouse = nextHouse.Normalize360();

            // Check if the houses cross the 360° boundary
            if (nextHouse < previousHouse)
            {
                // Add 360° to the next house longitude for correct wrapping
                nextHouse += Angle.Degrees360;
            }

            // Calculate the junction point
            var longitudeSum = previousHouse + nextHouse;
            var junctionPoint = longitudeSum.Divide(2);

            // Normalize the junction point back to 0-360° range
            return junctionPoint.Normalize360();

        }

        /// <summary>
        /// Gets planet which is the lord of a given sign
        /// </summary>
        public static PlanetName LordOfZodiacSign(ZodiacName signName)
        {
            //handle empty
            if (signName == Library.ZodiacName.Empty) { return Library.PlanetName.Empty; }

            switch (signName)
            {
                //Aries and Scorpio are ruled by Mars;
                case ZodiacName.Aries:
                case ZodiacName.Scorpio:
                    return Library.PlanetName.Mars;

                //Taurus and Libra by Venus;
                case ZodiacName.Taurus:
                case ZodiacName.Libra:
                    return Library.PlanetName.Venus;

                //Gemini and Virgo by Mercury;
                case ZodiacName.Gemini:
                case ZodiacName.Virgo:
                    return Library.PlanetName.Mercury;

                //Cancer by the Moon;
                case ZodiacName.Cancer:
                    return Library.PlanetName.Moon;

                //Leo by the Sun ;
                case ZodiacName.Leo:
                    return Library.PlanetName.Sun;

                //Sagittarius and Pisces by Jupiter
                case ZodiacName.Sagittarius:
                case ZodiacName.Pisces:
                    return Library.PlanetName.Jupiter;

                //Capricorn and Aquarius by Saturn.
                case ZodiacName.Capricorn:
                case ZodiacName.Aquarius:
                    return Library.PlanetName.Saturn;
                default:
                    throw new Exception("Lord of sign not found, error!");
            }
        }

        /// <summary>
        /// Given a planet name will return list of signs that the planet rules
        /// </summary>
        public static List<ZodiacName> ZodiacSignsOwnedByPlanet(PlanetName planetName)
        {
            List<ZodiacName> zodiacNames = new List<ZodiacName>();
            switch (planetName.Name)
            {
                case PlanetNameEnum.Mars:
                    zodiacNames.Add(ZodiacName.Aries);
                    zodiacNames.Add(ZodiacName.Scorpio);
                    break;
                case PlanetNameEnum.Venus:
                    zodiacNames.Add(ZodiacName.Taurus);
                    zodiacNames.Add(ZodiacName.Libra);
                    break;
                case PlanetNameEnum.Mercury:
                    zodiacNames.Add(ZodiacName.Gemini);
                    zodiacNames.Add(ZodiacName.Virgo);
                    break;
                case PlanetNameEnum.Moon:
                    zodiacNames.Add(ZodiacName.Cancer);
                    break;
                case PlanetNameEnum.Sun:
                    zodiacNames.Add(ZodiacName.Leo);
                    break;
                case PlanetNameEnum.Jupiter:
                    zodiacNames.Add(ZodiacName.Sagittarius);
                    zodiacNames.Add(ZodiacName.Pisces);
                    break;
                case PlanetNameEnum.Saturn:
                    zodiacNames.Add(ZodiacName.Capricorn);
                    zodiacNames.Add(ZodiacName.Aquarius);
                    break;
                case PlanetNameEnum.Dhuma:
                    zodiacNames.Add(ZodiacName.Capricorn);
                    break;
                case PlanetNameEnum.Vyatipaata:
                    zodiacNames.Add(ZodiacName.Gemini);
                    break;
                case PlanetNameEnum.Parivesha:
                    zodiacNames.Add(ZodiacName.Sagittarius);
                    break;
                case PlanetNameEnum.Indrachaapa:
                    zodiacNames.Add(ZodiacName.Cancer);
                    break;
                case PlanetNameEnum.Upaketu:
                    zodiacNames.Add(ZodiacName.Cancer);
                    break;
                case PlanetNameEnum.Gulika:
                    zodiacNames.Add(ZodiacName.Aquarius);
                    break;
                case PlanetNameEnum.Yamaghantaka:
                    zodiacNames.Add(ZodiacName.Sagittarius);
                    break;
                case PlanetNameEnum.Arthaprahaara:
                    zodiacNames.Add(ZodiacName.Gemini);
                    break;
                case PlanetNameEnum.Kaala:
                    zodiacNames.Add(ZodiacName.Capricorn);
                    break;
                case PlanetNameEnum.Mrityu:
                    zodiacNames.Add(ZodiacName.Scorpio);
                    break;
                default:
                    zodiacNames.Add(ZodiacName.Empty);
                    break;
            }
            return zodiacNames;
        }

        /// <summary>
        /// Gets next zodiac sign after input sign
        /// </summary>
        public static ZodiacName NextZodiacSign(ZodiacName inputSign)
        {

            //CACHE MECHANISM
            return CacheManager.GetCache(new CacheKey(nameof(NextZodiacSign), inputSign, Ayanamsa), _getNextZodiacSign);


            //UNDERLYING FUNCTION
            ZodiacName _getNextZodiacSign()
            {
                //get number of of input zodiac
                int inputSignNumber = (int)inputSign;

                int nextSignNumber;

                //after pieces (12) is Aries (1)
                if (inputSignNumber == 12)
                {
                    nextSignNumber = 1;
                }
                else
                {
                    //else next sign is input sign plus 1
                    nextSignNumber = inputSignNumber + 1;
                }

                //convert next sign number to its zodiac name
                var nextSignName = (ZodiacName)nextSignNumber;

                return nextSignName;

            }
        }

        /// <summary>
        /// Gets next house number after input house number, goes to  1 after 12
        /// </summary>
        public static int NextHouseNumber(int inputHouseNumber)
        {

            //CACHE MECHANISM
            return CacheManager.GetCache(new CacheKey(nameof(NextHouseNumber), inputHouseNumber, Ayanamsa), _getNextHouseNumber);


            //UNDERLYING FUNCTION
            int _getNextHouseNumber()
            {
                int nextHouseNumber;

                //if input house number is 12
                if (inputHouseNumber == 12)
                {
                    //next house number is 1
                    nextHouseNumber = 1;

                }
                else
                {
                    //else next house number is input number + 1
                    nextHouseNumber = inputHouseNumber + 1;
                }


                return nextHouseNumber;

            }

        }

        /// <summary>
        /// Gets the exact longitude where planet is Exalted/Exaltation
        /// Exaltation
        /// Each planet is held to be exalted when it is
        /// in a particular sign. The power to do good when in
        /// exaltation is greater than when in its own sign.
        /// Throughout the sign ascribed, the planet is exalted
        /// but in a particular degree its exaltation is at the maximum level.
        /// 
        /// NOTE:
        /// - For Upagrahas no exact degree for exaltation the whole
        /// sign is counted as such exalatiotn set at degree 1
        /// 
        /// - Rahu & ketu have exaltation points ref : Astroloy for Beginners pg. 12
        /// </summary>
        public static ZodiacSign PlanetExaltationPoint(PlanetName planetName)
        {

            //CACHE MECHANISM
            return CacheManager.GetCache(new CacheKey(nameof(PlanetExaltationPoint), planetName, Ayanamsa), _getPlanetExaltationPoint);


            //UNDERLYING FUNCTION
            ZodiacSign _getPlanetExaltationPoint()
            {
                //Sun in the 10th degree of Aries;
                if (planetName == Library.PlanetName.Sun)
                {
                    return new ZodiacSign(ZodiacName.Aries, Angle.FromDegrees(10));
                }

                // Moon 3rd of Taurus;
                else if (planetName == Library.PlanetName.Moon)
                {
                    return new ZodiacSign(ZodiacName.Taurus, Angle.FromDegrees(3));
                }

                // Mars 28th of Capricorn ;
                else if (planetName == Library.PlanetName.Mars)
                {
                    return new ZodiacSign(ZodiacName.Capricorn, Angle.FromDegrees(28));
                }

                // Mercury 15th of Virgo;
                else if (planetName == Library.PlanetName.Mercury)
                {
                    return new ZodiacSign(ZodiacName.Virgo, Angle.FromDegrees(15));
                }

                // Jupiter 5th of Cancer;
                else if (planetName == Library.PlanetName.Jupiter)
                {
                    return new ZodiacSign(ZodiacName.Cancer, Angle.FromDegrees(5));
                }

                // Venus 27th of Pisces and
                else if (planetName == Library.PlanetName.Venus)
                {
                    return new ZodiacSign(ZodiacName.Pisces, Angle.FromDegrees(27));
                }

                // Saturn 20th of Libra.
                else if (planetName == Library.PlanetName.Saturn)
                {
                    return new ZodiacSign(ZodiacName.Libra, Angle.FromDegrees(20));
                }

                // Rahu 20th of Taurus.
                else if (planetName == Library.PlanetName.Rahu)
                {
                    return new ZodiacSign(ZodiacName.Taurus, Angle.FromDegrees(20));
                }

                // Ketu 20th of Scorpio.
                else if (planetName == Library.PlanetName.Ketu)
                {
                    return new ZodiacSign(ZodiacName.Scorpio, Angle.FromDegrees(20));
                }

                //NOTE: Upagrahas exalatation whole sign, artificial set degree 1
                else if (planetName == Library.PlanetName.Dhuma)
                {
                    return new ZodiacSign(ZodiacName.Leo, Angle.FromDegrees(1));
                }
                else if (planetName == Library.PlanetName.Vyatipaata)
                {
                    return new ZodiacSign(ZodiacName.Scorpio, Angle.FromDegrees(1));
                }
                else if (planetName == Library.PlanetName.Parivesha)
                {
                    return new ZodiacSign(ZodiacName.Gemini, Angle.FromDegrees(1));
                }
                else if (planetName == Library.PlanetName.Indrachaapa)
                {
                    return new ZodiacSign(ZodiacName.Sagittarius, Angle.FromDegrees(1));
                }
                else if (planetName == Library.PlanetName.Upaketu)
                {
                    return new ZodiacSign(ZodiacName.Aquarius, Angle.FromDegrees(1));
                }

                throw new Exception("Planet exaltation point not found, error!");

            }

        }

        /// <summary>
        /// Gets the exact sign longitude where planet is Debilitated/Debility
        /// TODO method needs testing!
        /// Note:
        /// - Rahu & ketu have debilitation points ref : Astroloy for Beginners pg. 12
        /// - "planet to sign relationship" is the whole sign, this is just a point
        /// - The 7th house or the 180th degree from the place of exaltation is the
        ///   place of debilitation or fall. The Sun is debilitated-
        ///   in the 10th degree of Libra, the Moon 3rd
        ///   of Scorpio and so on.
        /// - For Upagrahas no exact degree for exaltation the whole
        ///   sign is counted as such exalatiotn set at degree 1
        /// - The debilitation or depression points are found
        ///   by adding 180° to the maximum points given above.
        ///   While in a state of fall, planets give results contrary
        ///   to those when in exaltation. ref : Astroloy for Beginners pg. 11
        /// </summary>
        public static ZodiacSign PlanetDebilitationPoint(PlanetName planetName)
        {

            //CACHE MECHANISM
            return CacheManager.GetCache(new CacheKey(nameof(PlanetDebilitationPoint), planetName, Ayanamsa), _getPlanetDebilitationPoint);


            //UNDERLYING FUNCTION
            ZodiacSign _getPlanetDebilitationPoint()
            {
                //The 7th house or the
                // 180th degree from the place of exaltation is the
                // place of debilitation or fall. The Sun is debilitated-
                // in the 10th degree of Libra, the Moon 3rd
                // of Scorpio and so on.

                //Sun in the 10th degree of Libra;
                if (planetName == Library.PlanetName.Sun)
                {
                    return new ZodiacSign(ZodiacName.Libra, Angle.FromDegrees(10));
                }

                // Moon 0 of Scorpio
                else if (planetName == Library.PlanetName.Moon)
                {
                    //TODO check if 0 degrees exist
                    return new ZodiacSign(ZodiacName.Scorpio, Angle.FromDegrees(0));
                }

                // Mars 28th of Cancer ;
                else if (planetName == Library.PlanetName.Mars)
                {
                    return new ZodiacSign(ZodiacName.Cancer, Angle.FromDegrees(28));
                }

                // Mercury 15th of Pisces;
                else if (planetName == Library.PlanetName.Mercury)
                {
                    return new ZodiacSign(ZodiacName.Pisces, Angle.FromDegrees(15));
                }

                // Jupiter 5th of Capricorn;
                else if (planetName == Library.PlanetName.Jupiter)
                {
                    return new ZodiacSign(ZodiacName.Capricorn, Angle.FromDegrees(5));
                }

                // Venus 27th of Virgo and
                else if (planetName == Library.PlanetName.Venus)
                {
                    return new ZodiacSign(ZodiacName.Virgo, Angle.FromDegrees(27));
                }

                // Saturn 20th of Aries.
                else if (planetName == Library.PlanetName.Saturn)
                {
                    return new ZodiacSign(ZodiacName.Aries, Angle.FromDegrees(20));
                }

                // Rahu 20th of Scorpio.
                else if (planetName == Library.PlanetName.Rahu)
                {
                    return new ZodiacSign(ZodiacName.Scorpio, Angle.FromDegrees(20));
                }

                // Ketu 20th of Taurus.
                else if (planetName == Library.PlanetName.Ketu)
                {
                    return new ZodiacSign(ZodiacName.Taurus, Angle.FromDegrees(20));
                }

                //NOTE: Upagrahas Debilitation whole sign, artificial set degree 1
                else if (planetName == Library.PlanetName.Dhuma)
                {
                    return new ZodiacSign(ZodiacName.Aquarius, Angle.FromDegrees(1));
                }
                else if (planetName == Library.PlanetName.Vyatipaata)
                {
                    return new ZodiacSign(ZodiacName.Taurus, Angle.FromDegrees(1));
                }
                else if (planetName == Library.PlanetName.Parivesha)
                {
                    return new ZodiacSign(ZodiacName.Sagittarius, Angle.FromDegrees(1));
                }
                else if (planetName == Library.PlanetName.Indrachaapa)
                {
                    return new ZodiacSign(ZodiacName.Gemini, Angle.FromDegrees(1));
                }
                else if (planetName == Library.PlanetName.Upaketu)
                {
                    return new ZodiacSign(ZodiacName.Leo, Angle.FromDegrees(1));
                }


                throw new Exception("Planet debilitation point not found, error!");

            }


        }


        #region SIGN GROUP CALULATORS

        /// <summary>
        /// Returns true if zodiac sign is an Even sign,  Yugma Rasis
        /// </summary>
        public static bool IsEvenSign(ZodiacName planetSignName)
        {

            //CACHE MECHANISM
            return CacheManager.GetCache(new CacheKey(nameof(IsEvenSign), planetSignName, Ayanamsa), _isEvenSign);


            //UNDERLYING FUNCTION
            bool _isEvenSign()
            {
                if (planetSignName == ZodiacName.Taurus || planetSignName == ZodiacName.Cancer || planetSignName == ZodiacName.Virgo ||
                    planetSignName == ZodiacName.Scorpio || planetSignName == ZodiacName.Capricorn || planetSignName == ZodiacName.Pisces)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }

        }

        /// <summary>
        /// Returns true if zodiac sign is an Odd sign, Oja Rasis
        /// </summary>
        public static bool IsOddSign(ZodiacName planetSignName)
        {
            //CACHE MECHANISM
            return CacheManager.GetCache(new CacheKey(nameof(IsOddSign), planetSignName, Ayanamsa), _isOddSign);


            //UNDERLYING FUNCTION
            bool _isOddSign()
            {
                if (planetSignName == ZodiacName.Aries || planetSignName == ZodiacName.Gemini || planetSignName == ZodiacName.Leo ||
                    planetSignName == ZodiacName.Libra || planetSignName == ZodiacName.Sagittarius || planetSignName == ZodiacName.Aquarius)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }


        }

        /// <summary>
        /// Fixed signs- Taurus, Leo, Scropio, Aquarius.
        /// </summary>
        public static bool IsFixedSign(ZodiacName sunSign)
        {
            switch (sunSign)
            {
                case ZodiacName.Taurus:
                case ZodiacName.Leo:
                case ZodiacName.Scorpio:
                case ZodiacName.Aquarius:
                    return true;
                default:
                    return false;
            }

        }

        /// <summary>
        /// Movable signs- Aries, Cancer, Libra, Capricorn.
        /// </summary>
        public static bool IsMovableSign(ZodiacName sunSign)
        {
            switch (sunSign)
            {
                case ZodiacName.Aries:
                case ZodiacName.Cancer:
                case ZodiacName.Libra:
                case ZodiacName.Capricorn:
                    return true;
                default:
                    return false;
            }

        }

        /// <summary>
        /// Common signs- Gemini, Virgo, Sagitarius, Pisces.
        /// </summary>
        public static bool IsCommonSign(ZodiacName sunSign)
        {
            switch (sunSign)
            {
                case ZodiacName.Gemini:
                case ZodiacName.Virgo:
                case ZodiacName.Sagittarius:
                case ZodiacName.Pisces:
                    return true;
                default:
                    return false;
            }

        }


        #endregion

        /// <summary>
        /// Gets a planets permenant relationship.
        /// Based on : Hindu Predictive Astrology, pg. 21
        /// Note:
        /// - Rahu & Ketu are not mentioned in any permenant relatioship by Raman.
        ///   But some websites do mention this. As such Raman's take is taken as final.
        ///   Since there's so far no explanation by Raman on Rahu & Ketu permenant relation it
        ///   is assumed that such relationship is not needed and to make them up for conveniece sake
        ///   could result in wrong prediction down the line.
        ///   But temporary relationship are mentioned by Raman for Rahu & Ketu, so explicitly use
        ///   Temperary relationship where needed.
        /// </summary>
        public static PlanetToPlanetRelationship PlanetPermanentRelationshipWithPlanet(PlanetName mainPlanet, PlanetName secondaryPlanet)
        {

            //no calculation for rahu and ketu here
            var isRahu = mainPlanet.Name == Library.PlanetName.PlanetNameEnum.Rahu;
            var isKetu = mainPlanet.Name == Library.PlanetName.PlanetNameEnum.Ketu;
            var isRahu2 = secondaryPlanet.Name == Library.PlanetName.PlanetNameEnum.Rahu;
            var isKetu2 = secondaryPlanet.Name == Library.PlanetName.PlanetNameEnum.Ketu;
            var isRahuKetu = isRahu || isKetu || isRahu2 || isKetu2;
            if (isRahuKetu) { return PlanetToPlanetRelationship.Empty; }



            //CACHE MECHANISM
            return CacheManager.GetCache(new CacheKey(nameof(PlanetPermanentRelationshipWithPlanet), mainPlanet, secondaryPlanet, Ayanamsa), _getPlanetPermanentRelationshipWithPlanet);


            //UNDERLYING FUNCTION
            PlanetToPlanetRelationship _getPlanetPermanentRelationshipWithPlanet()
            {
                //if main planet & secondary planet is same, then it is own plant (same planet), end here
                if (mainPlanet == secondaryPlanet) { return PlanetToPlanetRelationship.SamePlanet; }


                bool planetInEnemies = false;
                bool planetInNeutrals = false;
                bool planetInFriends = false;


                //if main planet is sun
                if (mainPlanet == Library.PlanetName.Sun)
                {
                    //List planets friends, neutrals & enemies
                    var sunFriends = new List<PlanetName>() { Library.PlanetName.Moon, Library.PlanetName.Mars, Library.PlanetName.Jupiter };
                    var sunNeutrals = new List<PlanetName>() { Library.PlanetName.Mercury };
                    var sunEnemies = new List<PlanetName>() { Library.PlanetName.Venus, Library.PlanetName.Saturn };

                    //check if planet is found in any of the lists
                    planetInFriends = sunFriends.Contains(secondaryPlanet);
                    planetInNeutrals = sunNeutrals.Contains(secondaryPlanet);
                    planetInEnemies = sunEnemies.Contains(secondaryPlanet);
                }

                //if main planet is moon
                if (mainPlanet == Library.PlanetName.Moon)
                {
                    //List planets friends, neutrals & enemies
                    var moonFriends = new List<PlanetName>() { Library.PlanetName.Sun, Library.PlanetName.Mercury };
                    var moonNeutrals = new List<PlanetName>() { Library.PlanetName.Mars, Library.PlanetName.Jupiter, Library.PlanetName.Venus, Library.PlanetName.Saturn };
                    var moonEnemies = new List<PlanetName>() { };

                    //check if planet is found in any of the lists
                    planetInFriends = moonFriends.Contains(secondaryPlanet);
                    planetInNeutrals = moonNeutrals.Contains(secondaryPlanet);
                    planetInEnemies = moonEnemies.Contains(secondaryPlanet);

                }

                //if main planet is mars
                if (mainPlanet == Library.PlanetName.Mars)
                {
                    //List planets friends, neutrals & enemies
                    var marsFriends = new List<PlanetName>() { Library.PlanetName.Sun, Library.PlanetName.Moon, Library.PlanetName.Jupiter };
                    var marsNeutrals = new List<PlanetName>() { Library.PlanetName.Venus, Library.PlanetName.Saturn };
                    var marsEnemies = new List<PlanetName>() { Library.PlanetName.Mercury };

                    //check if planet is found in any of the lists
                    planetInFriends = marsFriends.Contains(secondaryPlanet);
                    planetInNeutrals = marsNeutrals.Contains(secondaryPlanet);
                    planetInEnemies = marsEnemies.Contains(secondaryPlanet);

                }

                //if main planet is mercury
                if (mainPlanet == Library.PlanetName.Mercury)
                {
                    //List planets friends, neutrals & enemies
                    var mercuryFriends = new List<PlanetName>() { Library.PlanetName.Sun, Library.PlanetName.Venus };
                    var mercuryNeutrals = new List<PlanetName>() { Library.PlanetName.Mars, Library.PlanetName.Jupiter, Library.PlanetName.Saturn };
                    var mercuryEnemies = new List<PlanetName>() { Library.PlanetName.Moon };

                    //check if planet is found in any of the lists
                    planetInFriends = mercuryFriends.Contains(secondaryPlanet);
                    planetInNeutrals = mercuryNeutrals.Contains(secondaryPlanet);
                    planetInEnemies = mercuryEnemies.Contains(secondaryPlanet);

                }

                //if main planet is jupiter
                if (mainPlanet == Library.PlanetName.Jupiter)
                {
                    //List planets friends, neutrals & enemies
                    var jupiterFriends = new List<PlanetName>() { Library.PlanetName.Sun, Library.PlanetName.Moon, Library.PlanetName.Mars };
                    var jupiterNeutrals = new List<PlanetName>() { Library.PlanetName.Saturn };
                    var jupiterEnemies = new List<PlanetName>() { Library.PlanetName.Mercury, Library.PlanetName.Venus };

                    //check if planet is found in any of the lists
                    planetInFriends = jupiterFriends.Contains(secondaryPlanet);
                    planetInNeutrals = jupiterNeutrals.Contains(secondaryPlanet);
                    planetInEnemies = jupiterEnemies.Contains(secondaryPlanet);

                }

                //if main planet is venus
                if (mainPlanet == Library.PlanetName.Venus)
                {
                    //List planets friends, neutrals & enemies
                    var venusFriends = new List<PlanetName>() { Library.PlanetName.Mercury, Library.PlanetName.Saturn };
                    var venusNeutrals = new List<PlanetName>() { Library.PlanetName.Mars, Library.PlanetName.Jupiter };
                    var venusEnemies = new List<PlanetName>() { Library.PlanetName.Sun, Library.PlanetName.Moon };

                    //check if planet is found in any of the lists
                    planetInFriends = venusFriends.Contains(secondaryPlanet);
                    planetInNeutrals = venusNeutrals.Contains(secondaryPlanet);
                    planetInEnemies = venusEnemies.Contains(secondaryPlanet);

                }

                //if main planet is saturn
                if (mainPlanet == Library.PlanetName.Saturn)
                {
                    //List planets friends, neutrals & enemies
                    var saturnFriends = new List<PlanetName>() { Library.PlanetName.Mercury, Library.PlanetName.Venus };
                    var saturnNeutrals = new List<PlanetName>() { Library.PlanetName.Jupiter };
                    var saturnEnemies = new List<PlanetName>() { Library.PlanetName.Sun, Library.PlanetName.Moon, Library.PlanetName.Mars };

                    //check if planet is found in any of the lists
                    planetInFriends = saturnFriends.Contains(secondaryPlanet);
                    planetInNeutrals = saturnNeutrals.Contains(secondaryPlanet);
                    planetInEnemies = saturnEnemies.Contains(secondaryPlanet);

                }

                //for Rahu & Ketu special exception
                if (mainPlanet == Library.PlanetName.Rahu || mainPlanet == Library.PlanetName.Ketu)
                {
                    throw new Exception("No Permenant Relation for Rahu and Ketu, use Temporary Relation!");
                }




                //return planet relationship based on where planet is found
                if (planetInFriends)
                {
                    return PlanetToPlanetRelationship.Friend;
                }
                if (planetInNeutrals)
                {
                    return PlanetToPlanetRelationship.Neutral;
                }
                if (planetInEnemies)
                {
                    return PlanetToPlanetRelationship.Enemy;
                }

                return PlanetToPlanetRelationship.Empty;
                throw new Exception("planet permanent relationship not found, error!");

            }

        }

        /// <summary>
        /// Converts julian time to normal time, normal time can be lmt, lat, utc
        /// </summary>
        public static DateTime ConvertJulianTimeToNormalTime(double julianTime)
        {

            //CACHE MECHANISM
            return CacheManager.GetCache(new CacheKey(nameof(ConvertJulianTimeToNormalTime), julianTime, Ayanamsa), _convertJulianTimeToNormalTime);


            //UNDERLYING FUNCTION
            DateTime _convertJulianTimeToNormalTime()
            {
                //initialize ephemeris
                SwissEph ephemeris = new SwissEph();

                //set calender type
                int gregflag = SwissEph.SE_GREG_CAL; //GREGORIAN CALENDAR

                //julian time to normal time
                int year = 0, month = 0, day = 0, hour = 0, min = 0;
                double sec = 0;

                // convert julian time to normal time
                ephemeris.swe_jdut1_to_utc(julianTime, gregflag, ref year, ref month, ref day, ref hour, ref min, ref sec);

                //put pieces of time into one type
                var normalUtcTime = new DateTime(year, month, day, hour, min, (int)sec);

                return normalUtcTime;

            }


        }

        /// <summary>
        /// Gets Greenwich time in normal format from Julian days at Greenwich
        /// Note : Inputed time is Julian days at greenwich, callers reponsibility to make sure 
        /// </summary>
        public static DateTimeOffset GreenwichTimeFromJulianDays(double julianTime)
        {

            //CACHE MECHANISM
            return CacheManager.GetCache(new CacheKey(nameof(GreenwichTimeFromJulianDays), julianTime, Ayanamsa), _convertJulianTimeToNormalTime);


            //UNDERLYING FUNCTION
            DateTimeOffset _convertJulianTimeToNormalTime()
            {
                //initialize ephemeris
                SwissEph ephemeris = new();

                //set calender type
                int gregflag = SwissEph.SE_GREG_CAL; //GREGORIAN CALENDAR

                //prepare a place to receive the time in normal format 
                int year = 0, month = 0, day = 0, hour = 0, min = 0;
                double sec = 0;

                //convert julian time to normal time
                ephemeris.swe_jdut1_to_utc(julianTime, gregflag, ref year, ref month, ref day, ref hour, ref min, ref sec);

                //put pieces of time into one type
                var normalUtcTime = new DateTime(year, month, day, hour, min, (int)sec);

                //set the correct offset (Greenwich = UTC = +0:00)
                var offsetTime = new DateTimeOffset(normalUtcTime, new TimeSpan(0, 0, 0));

                return offsetTime;
            }


        }

        /// <summary>
        /// Gets Local mean time (LMT) at Greenwich (UTC) in Julian days based on the inputed time
        /// </summary>
        public static double GreenwichLmtInJulianDays(Time time)
        {

            //CACHE MECHANISM
            return CacheManager.GetCache(new CacheKey(nameof(GreenwichLmtInJulianDays), time, Ayanamsa), _getGreenwichLmtInJulianDays);


            //UNDERLYING FUNCTION
            double _getGreenwichLmtInJulianDays()
            {
                //get LMT time at Greenwich (UTC)
                DateTimeOffset lmtDateTime = time.GetLmtDateTimeOffset().ToUniversalTime();

                //split lmt time to pieces
                int year = lmtDateTime.Year;
                int month = lmtDateTime.Month;
                int day = lmtDateTime.Day;
                double hour = (lmtDateTime.TimeOfDay).TotalHours;

                //set calender type
                int gregflag = SwissEph.SE_GREG_CAL; //GREGORIAN CALENDAR

                //declare output variables
                double localMeanTimeInJulian_UT;

                //initialize ephemeris
                SwissEph ephemeris = new();

                //get lmt in julian day in Universal Time (UT)
                localMeanTimeInJulian_UT = ephemeris.swe_julday(year, month, day, hour, gregflag);//time to Julian Day

                return localMeanTimeInJulian_UT;

            }

        }


        /// <summary>
        /// Converts Local Mean Time (LMT) to Universal Time (UTC)
        /// </summary>
        public static DateTimeOffset LmtToUtc(Time time)
        {
            //CACHE MECHANISM
            return CacheManager.GetCache(new CacheKey(nameof(LmtToUtc), time, Ayanamsa), _lmtToUtc);


            //UNDERLYING FUNCTION
            DateTimeOffset _lmtToUtc()
            {
                return time.GetLmtDateTimeOffset().ToUniversalTime();
            }
        }

        #endregion

        #region ASHTAKVARGA

        /// <summary>
        /// When the benefic points contributed by each planet in Bhinnashtakavargas
        /// different signs are added, we get a Sarvashtakavarga.
        /// A total of 337 benefic points are contributed, by the seven planets,
        /// to various houses in relation to seven planets and the lagna.
        /// </summary>
        public static Sarvashtakavarga SarvashtakavargaChart(Time birthTime)
        {
            return Ashtakavarga.SarvashtakavargaChart2(birthTime);
        }

        /// <summary>
        /// Seven different charts are thus possible for the seven different
        /// planets. These are called as Bhinnashtakavargas. The position
        /// of each planet in the natal chart is of primary consideration. 
        /// </summary>
        public static Bhinnashtakavarga BhinnashtakavargaChart(Time birthTime)
        {
            var returnVal = new Bhinnashtakavarga();

            foreach (var planet in PlanetName.All7Planets)
            {
                //use prastaraka to calculate bhinnashtaka and add it compiled list
                var planetPrastaraka = Ashtakavarga.PlanetPrastaraka(planet, birthTime);
                returnVal[planet] = planetPrastaraka.BhinnashtakaRow();
            }

            return returnVal;
        }

        /// <summary>
        /// Give a planet and sign and ashtakvarga bindu can be calculated
        /// (uses Bhinnashtakavarga)
        /// EXP : In the Sun's own Ashtakvarga, there are 5 bindus in Aries
        /// 
        /// NOTE ON USE: Ashtakvarga System pg.128 
        /// For example, in the Standard Horoscope,
        /// the Sun's transit of Aries (3rd from Moon) should
        /// prove favorable. In the Sun's own Ashtakvarga,
        /// there are 5 bindus in Aries. Therefore the
        /// good effects produced should be to the extent
        /// of 62%. The Sun's transit of Capricorn
        /// (12th from the Moon) should prove adverse.
        /// Capricorn, has no bindus.Therefore the evil results
        /// to be produced by this transit are to the brim.
        /// </summary>
        public static int PlanetAshtakvargaBindu(PlanetName planet, ZodiacName signToCheck, Time time)
        {
            //calculates ashtakvarga for given planet 
            var bhinnashtakavargaChart = Ashtakavarga.BhinnashtakavargaChartForPlanet(planet, time);

            //get bindu score for given sign
            var bindu = bhinnashtakavargaChart[signToCheck];

            return bindu;
        }

        /// <summary>
        /// Example : Get Venus bindu in Mercury's Ashtakvarga (main planet)
        /// </summary>
        /// <param name="mainAshtakvargaPlanet">planet's who Bhinnashtakavarga Chart is checked</param>
        /// <param name="planetToCheck">planet to get bindu based on sign</param>
        public static int PlanetAshtakvargaBinduByPlanet(PlanetName mainAshtakvargaPlanet, PlanetName planetToCheck, Time time)
        {
            var planetToCheckZodiac = Calculate.PlanetRasiD1Sign(planetToCheck, time).GetSignName();
            int bindus = Calculate.PlanetAshtakvargaBindu(mainAshtakvargaPlanet, planetToCheckZodiac, time);

            return bindus;
        }

        /// <summary>
        /// Gets bindus for planet in it's own Ashtakavarga in the sign it is in
        /// </summary>
        public static int PlanetOwnAshtakvargaBindu(PlanetName planet, Time time)
        {
            //rahu & ketu does not have Ashtakvarga Bindu
            if (planet == Rahu || planet == Ketu) { return 0; }

            //get planet's sign
            var planetRasi = Calculate.PlanetRasiD1Sign(planet, time);

            //get bindus for the sign which planet is on
            var bindus = Calculate.PlanetAshtakvargaBindu(planet, planetRasi.GetSignName(), time);

            return bindus;
        }

        /// <summary>
        /// Kakshyas for daily use : The concept of Kakshyas can be
        /// employed for daily use. The method of this application is simple.
        /// Prepare the Prastaraka charts for the seven planets. Then find
        /// out the longitudes of each of the seven planets on a given day.
        /// In the Prastaraka of the Sun, see if the transiting Sun is passing
        /// through a Kakshya with a benefic point. For the Moon's transit,
        /// consider the Prastaraka of the Moon. See for all the planets.
        /// When several planets are transiting the Kakshyas where the natal
        /// planets have contributed benefic points, that day is auspicious.
        /// When several planets transit the Kakshyas where there are no
        /// benefic points, it is adverse time for the native
        /// 
        /// The Concept of Kakshya
        /// The Prastaraka charts for different planets can be represented
        /// in a different manner to make use of the concept of Kakshyas.
        /// Each rashi or sign is divided into eight equal parts or Kakshyas
        /// The Prastaraka chart for each planet can thus be readjusted
        /// to bring in the concept of the Kakshyas.
        /// A planet is considered to be productive of benefic
        /// results when it transits a Kakshya where there is a benefic point
        /// </summary>
        public static GocharaKakshas GocharaKakshas(Time checkTime, Time birthTime)
        {
            //first is column of name planets
            var column1 = PlanetName.All7Planets;

            //2nd column is signs the planet is currently in
            var column2 = new Dictionary<PlanetName, ZodiacSign>();
            //add in each planet and the sign it is in at check time
            foreach (var planetName in column1) { column2.Add(planetName, Calculate.PlanetRasiD1Sign(planetName, checkTime)); }


            //3rd column is planet which is ruling the current planet
            //based on current zodiac sign determine the ruling planet
            var column3 = new Dictionary<PlanetName, string>();
            foreach (var currentZodiacSign in column2) { column3.Add(currentZodiacSign.Key, GetKakshyaLord(currentZodiacSign.Value)); }

            //NOTE : where current time is linked to birth time
            //4th column, score of 1 or 0 from Prastaraka 
            var column4 = new Dictionary<PlanetName, int>();
            foreach (var mainPlanet in column1) //can be acendant
            {
                var planetPrastaraka = Ashtakavarga.PlanetPrastaraka(mainPlanet, birthTime);
                //narrow down to planet which ruling current planet
                var rullingPlanet = column3[mainPlanet]; //includes Ascendant
                var allSigns = planetPrastaraka[rullingPlanet];

                //get specific score at current transiting sign
                var checkTimeSign = column2[mainPlanet];
                var score = allSigns[checkTimeSign.GetSignName()];
                column4.Add(mainPlanet, score);
            }

            //5th column add points from Prastaraka
            var column5 = new Dictionary<PlanetName, int>();
            foreach (var mainPlanet in column1) //can be acendant
            {
                var planetPrastaraka = Ashtakavarga.PlanetPrastaraka(mainPlanet, birthTime);
                //get score of compiled Prastaraka for all signs
                var bhinnashtakaRow = planetPrastaraka.BhinnashtakaRow();

                //get specific score at current transiting sign
                var checkTimeSign = column2[mainPlanet];
                var score = bhinnashtakaRow[checkTimeSign.GetSignName()];
                column5.Add(mainPlanet, score);
            }

            //6th column add points from Sarvashtaka 
            var column6 = new Dictionary<PlanetName, int>();
            foreach (var mainPlanet in column1) //can be acendant
            {
                //get Sarvashtaka for all signs
                var allSigns = SarvashtakavargaChart(birthTime);

                //get specific score at current transiting sign
                var checkTimeSign = column2[mainPlanet];

                //get column with added points
                var score = allSigns.SarvashtakavargaRow[checkTimeSign.GetSignName()];

                column6.Add(mainPlanet, score);
            }

            //pack the data to be oupted various formats even JPEG! yeah!
            var finalData = new GocharaKakshas(column1, column2, column3, column4, column5, column6);
            return finalData;

            //based on table data
            string GetKakshyaLord(ZodiacSign inputZodiacSign)
            {
                var degreeInSign = inputZodiacSign.GetDegreesInSign().TotalDegrees;

                if (degreeInSign >= 0 && degreeInSign <= 3.75) { return "Saturn"; }
                else if (degreeInSign > 3.75 && degreeInSign <= 7.5) { return "Jupiter"; }
                else if (degreeInSign > 7.5 && degreeInSign <= 11.25) { return "Mars"; }
                else if (degreeInSign > 11.25 && degreeInSign <= 15.00) { return "Sun"; }
                else if (degreeInSign > 15.00 && degreeInSign <= 18.75) { return "Venus"; }
                else if (degreeInSign > 18.75 && degreeInSign <= 22.5) { return "Mercury"; }
                else if (degreeInSign > 22.5 && degreeInSign <= 26.25) { return "Moon"; }
                else if (degreeInSign > 26.25 && degreeInSign <= 30.0) { return "Ascendant"; }

                throw new Exception("END OF LINE");
            }
        }


        #endregion

        #region GOCHARA

        /// <summary>
        /// Gets the Gochara sign number which is the count from birth Moon sign (janma rasi)
        /// to the sign the planet is at the current time. Gochara == Transits
        /// </summary>
        public static int GocharaZodiacSignCountFromMoon(Time birthTime, Time currentTime, PlanetName planet)
        {
            //get moon sign at birth (janma rasi)
            var janmaSign = Calculate.MoonSignName(birthTime);

            //get planet sign at input time
            var planetSign = Calculate.PlanetRasiD1Sign(planet, currentTime).GetSignName();

            //count from janma to sign planet is in
            var count = Calculate.CountFromSignToSign(janmaSign, planetSign);

            return count;
        }

        /// <summary>
        /// Check if there is an obstruction to a given Gochara, obstructing house/point (Vedhanka)
        /// </summary>
        public static bool IsGocharaObstructed(PlanetName planet, int gocharaHouse, Time birthTime, Time currentTime)
        {
            //get the obstructing house/point (Vedhanka) for the inputed Gochara house
            var vedhanka = Vedhanka(planet, gocharaHouse);

            //if vedhanka is 0, then end here as no obstruction
            if (vedhanka == 0) { return false; }

            //get all the planets transiting (gochara) in this obstruction point/house (vedhanka)
            var planetList = PlanetsInGocharaHouse(birthTime, currentTime, gocharaHouse);

            //remove the exception planets
            //No Vedha occurs between the Sun and Saturn, and the Moon and Mercury.
            if (planet == Library.PlanetName.Sun || planet == Library.PlanetName.Saturn)
            {
                planetList.Remove(Library.PlanetName.Sun);
                planetList.Remove(Library.PlanetName.Saturn);
            }
            if (planet == Library.PlanetName.Moon || planet == Mercury)
            {
                planetList.Remove(Library.PlanetName.Moon);
                planetList.Remove(Library.PlanetName.Mercury);
            }

            //now if any planet is found in the list, than obstruction is present
            return planetList.Any();

        }

        /// <summary>
        /// Gets all the planets in a given Gochara House
        /// 
        /// Note : Gochara House number is the count from birth Moon sign (janma rasi)
        /// to the sign the planet is at the current time. Gochara == Transits
        /// </summary>
        public static List<PlanetName> PlanetsInGocharaHouse(Time birthTime, Time currentTime, int gocharaHouse)
        {
            //get the gochara house for every planet at current time
            var gocharaSun = GocharaZodiacSignCountFromMoon(birthTime, currentTime, Library.PlanetName.Sun);
            var gocharaMoon = GocharaZodiacSignCountFromMoon(birthTime, currentTime, Library.PlanetName.Moon);
            var gocharaMars = GocharaZodiacSignCountFromMoon(birthTime, currentTime, Library.PlanetName.Mars);
            var gocharaMercury = GocharaZodiacSignCountFromMoon(birthTime, currentTime, Library.PlanetName.Mercury);
            var gocharaJupiter = GocharaZodiacSignCountFromMoon(birthTime, currentTime, Library.PlanetName.Jupiter);
            var gocharaVenus = GocharaZodiacSignCountFromMoon(birthTime, currentTime, Library.PlanetName.Venus);
            var gocharaSaturn = GocharaZodiacSignCountFromMoon(birthTime, currentTime, Library.PlanetName.Saturn);

            //add every planet name to return list that matches input Gochara house number
            var planetList = new List<PlanetName>();
            if (gocharaSun == gocharaHouse) { planetList.Add(Library.PlanetName.Sun); }
            if (gocharaMoon == gocharaHouse) { planetList.Add(Library.PlanetName.Moon); }
            if (gocharaMars == gocharaHouse) { planetList.Add(Library.PlanetName.Mars); }
            if (gocharaMercury == gocharaHouse) { planetList.Add(Library.PlanetName.Mercury); }
            if (gocharaJupiter == gocharaHouse) { planetList.Add(Library.PlanetName.Jupiter); }
            if (gocharaVenus == gocharaHouse) { planetList.Add(Library.PlanetName.Venus); }
            if (gocharaSaturn == gocharaHouse) { planetList.Add(Library.PlanetName.Saturn); }

            return planetList;
        }

        /// <summary>
        /// Gets the Vedhanka (point of obstruction), used for Gohchara calculations.
        /// The data returned comes from a fixed table. 
        /// NOTE: - Planet exceptions are not accounted for here.
        ///       - Return 0 when no obstruction point exists 
        /// Reference : Hindu Predictive Astrology pg. 257
        /// </summary>
        public static int Vedhanka(PlanetName planet, int house)
        {
            //filter based on planet
            if (planet == Library.PlanetName.Sun)
            {
                //good
                if (house == 11) { return 5; }
                if (house == 3) { return 9; }
                if (house == 10) { return 4; }
                if (house == 6) { return 12; }
                //bad
                if (house == 5) { return 11; }
                if (house == 9) { return 3; }
                if (house == 4) { return 10; }
                if (house == 12) { return 6; }
            }

            if (planet == Library.PlanetName.Moon)
            {
                //good
                if (house == 7) { return 2; }
                if (house == 1) { return 5; }
                if (house == 6) { return 12; }
                if (house == 11) { return 8; }
                if (house == 10) { return 4; }
                if (house == 3) { return 9; }
                //bad
                if (house == 2) { return 7; }
                if (house == 5) { return 1; }
                if (house == 12) { return 6; }
                if (house == 8) { return 11; }
                if (house == 4) { return 10; }
                if (house == 9) { return 3; }

            }

            if (planet == Library.PlanetName.Mars)
            {
                //good
                if (house == 3) { return 12; }
                if (house == 11) { return 5; }
                if (house == 6) { return 9; }
                //bad
                if (house == 12) { return 3; }
                if (house == 5) { return 11; }
                if (house == 9) { return 6; }
            }

            if (planet == Library.PlanetName.Mercury)
            {
                //good
                if (house == 2) { return 5; }
                if (house == 4) { return 3; }
                if (house == 6) { return 9; }
                if (house == 8) { return 1; }
                if (house == 10) { return 7; }
                if (house == 11) { return 12; }

                //bad
                if (house == 5) { return 2; }
                if (house == 3) { return 4; }
                if (house == 9) { return 6; }
                if (house == 1) { return 8; }
                if (house == 7) { return 10; }
                if (house == 12) { return 11; }
            }

            if (planet == Library.PlanetName.Jupiter)
            {
                //good
                if (house == 2) { return 12; }
                if (house == 11) { return 8; }
                if (house == 9) { return 10; }
                if (house == 5) { return 4; }
                if (house == 7) { return 3; }

                //bad
                if (house == 12) { return 2; }
                if (house == 8) { return 11; }
                if (house == 10) { return 9; }
                if (house == 4) { return 5; }
                if (house == 3) { return 7; }

            }

            if (planet == Library.PlanetName.Venus)
            {
                //good
                if (house == 1) { return 8; }
                if (house == 2) { return 7; }
                if (house == 3) { return 1; }
                if (house == 4) { return 10; }
                if (house == 5) { return 9; }
                if (house == 8) { return 5; }
                if (house == 9) { return 11; }
                if (house == 11) { return 6; }
                if (house == 12) { return 3; }

                //bad
                if (house == 8) { return 1; }
                if (house == 7) { return 2; }
                if (house == 1) { return 3; }
                if (house == 10) { return 4; }
                if (house == 9) { return 5; }
                if (house == 5) { return 8; }
                if (house == 11) { return 9; }
                if (house == 6) { return 11; }
                if (house == 3) { return 12; }

            }

            if (planet == Library.PlanetName.Saturn)
            {
                //good
                if (house == 3) { return 12; }
                if (house == 11) { return 5; }
                if (house == 6) { return 9; }

                //bad
                if (house == 12) { return 3; }
                if (house == 5) { return 11; }
                if (house == 9) { return 6; }

            }
            //copy of saturn & mars
            if (planet == Library.PlanetName.Rahu)
            {
                //good
                if (house == 3) { return 12; }
                if (house == 11) { return 5; }
                if (house == 6) { return 9; }

                //bad
                if (house == 12) { return 3; }
                if (house == 5) { return 11; }
                if (house == 9) { return 6; }

            }
            if (planet == Library.PlanetName.Ketu)
            {
                //good
                if (house == 3) { return 12; }
                if (house == 11) { return 5; }
                if (house == 6) { return 9; }

                //bad
                if (house == 12) { return 3; }
                if (house == 5) { return 11; }
                if (house == 9) { return 6; }

            }





            //if no condition above met, then there is no obstruction point
            return 0;
        }

        /// <summary>
        /// Is SunGocharaInHouse1
        /// Checks if a Gochara is occuring for a planet in a given house without any obstructions at a given time
        /// Note : Basically a wrapper method for Gochra event calculations
        /// </summary>
        public static bool IsGocharaOccurring(Time birthTime, Time time, PlanetName planet, int gocharaHouse)
        {
            //check if planet is in the specified gochara house
            var planetGocharaMatch = Calculate.GocharaZodiacSignCountFromMoon(birthTime, time, planet) == gocharaHouse;

            //NOTE: only use Vedha point by default, but allow disable if needed (LONG LEVER DESIGN)
            bool obstructionNotFound = true; //default to true, so if disabled Vedha point will still work
            if (Calculate.UseVedhankaInGochara)
            {
                //check if there is any planet obstructing this transit prediction via Vedhasthana
                obstructionNotFound = !Calculate.IsGocharaObstructed(planet, gocharaHouse, birthTime, time);
            }

            //occuring if all conditions met
            var occuring = planetGocharaMatch && obstructionNotFound;

            return occuring;
        }

        /// <summary>
        /// Checks if a given planet's with given number of bindu is transiting now (Gochara)
        /// </summary>
        public static bool IsPlanetGocharaBindu(Time birthTime, Time nowTime, PlanetName planet, int bindu)
        {
            //sign the planet is transiting now counted from moon
            var signCountFromMoon = Calculate.GocharaZodiacSignCountFromMoon(birthTime, nowTime, planet);

            //check if there is any planet obstructing this transit prediction via Vedhasthana
            var obstructionFound = Calculate.IsGocharaObstructed(planet, signCountFromMoon, birthTime, nowTime);

            //if obstructed end here
            if (obstructionFound) { return false; }

            //gochara ongoing, get sign of house to get planet's bindu score for said transit
            var gocharaSign = Calculate.SignCountedFromPlanetSign(signCountFromMoon, Moon, birthTime);

            //get planet's current bindu
            var planetBindu = Calculate.PlanetAshtakvargaBindu(planet, gocharaSign, birthTime);

            //occuring if bindu is match
            var occuring = planetBindu == bindu;

            return occuring;
        }


        #endregion

        #region CHARA DASA

        /// <summary>
        /// Represents a Chara Dasha period with sub-periods.
        /// </summary>
        public class DashaPeriod
        {
            public ZodiacName SignName { get; set; }
            public DateTimeOffset StartTime { get; set; }
            public DateTimeOffset EndTime { get; set; }
            public List<SubPeriod> SubPeriods { get; set; }
        }

        /// <summary>
        /// Represents a sub-period within a Chara Dasha period.
        /// </summary>
        public class SubPeriod
        {
            public ZodiacName SignName { get; set; }
            public DateTimeOffset StartTime { get; set; }
            public DateTimeOffset EndTime { get; set; }
        }

        /// <summary>
        /// Calculates the Chara Dasa sign at the specified checkTime based on the birthTime.
        /// </summary>
        /// <param name="birthTime">The birth time of the person.</param>
        /// <param name="checkTime">The time at which to check the Chara Dasa.</param>
        /// <returns>The name of the Chara Dasha sign active at the checkTime with sub-periods.</returns>
        public static DashaPeriod GetCharaDasaAtTime(Time birthTime, Time checkTime)
        {
            // Step 1: Determine the Lagna (Ascendant) sign at birth
            var lagnaSign = Calculate.LagnaSignName(birthTime);
            int lagnaIndex = (int)lagnaSign;

            // Step 2: Calculate the Chara Dasha order starting from the Lagna
            List<ZodiacName> dashaOrder = CalculateCharaDashaOrder(lagnaSign);

            // Step 3: Calculate the duration of each Dasha period in years
            Dictionary<ZodiacName, double> dashaYears = CalculateCharaDashaYears(birthTime, dashaOrder);

            // Step 4: Generate the Dasha periods with start and end times
            List<DashaPeriod> dashaPeriods = GenerateCharaDashaPeriods(birthTime, dashaOrder, dashaYears);

            // Step 5: Find the Dasha period active at checkTime
            var activeDasha = dashaPeriods.FirstOrDefault(dp => dp.StartTime <= checkTime.GetLmtDateTimeOffset() && checkTime.GetLmtDateTimeOffset() < dp.EndTime);

            // Step 6: Calculate the sub-periods for the active Dasha period
            if (activeDasha != null)
            {
                var subPeriods = GenerateSubPeriods(activeDasha, birthTime);
                activeDasha.SubPeriods = subPeriods;
            }

            return activeDasha;
        }

        /// <summary>
        /// Generates the sub-periods for a given Chara Dasha period.
        /// </summary>
        private static List<SubPeriod> GenerateSubPeriods(DashaPeriod dashaPeriod, Time birthTime)
        {
            List<SubPeriod> subPeriods = new List<SubPeriod>();
            var planetDegrees = GetPlanetDegrees(birthTime);

            // Ninth house from the current Dasha sign
            var ninthHouse = Calculate.SignCountedFromInputSign(dashaPeriod.SignName, 9);

            // Order type (direct or indirect)
            var orderType = GetOrderType(ninthHouse);

            // Calculate the sub-periods
            var totalMonths = (dashaPeriod.EndTime - dashaPeriod.StartTime).TotalDays / 30;
            var totalDegrees = 360;
            var subPeriodMonths = (totalMonths * 12) / totalDegrees;

            if (orderType == "direct")
            {
                for (int i = 1; i <= 12; i++)
                {
                    var subPeriodSign = (ZodiacName)(((int)dashaPeriod.SignName + i - 1) % 12 + 1);
                    var subPeriodStartTime = dashaPeriod.StartTime.AddMonths((int)((i - 1) * subPeriodMonths));
                    var subPeriodEndTime = dashaPeriod.StartTime.AddMonths((int)(i * subPeriodMonths));

                    subPeriods.Add(new SubPeriod
                    {
                        SignName = subPeriodSign,
                        StartTime = subPeriodStartTime,
                        EndTime = subPeriodEndTime
                    });
                }
            }
            else
            {
                for (int i = 1; i <= 12; i++)
                {
                    var subPeriodSign = (ZodiacName)(((int)dashaPeriod.SignName - i + 12) % 12 + 1);
                    var subPeriodStartTime = dashaPeriod.StartTime.AddMonths((int)((i - 1) * subPeriodMonths));
                    var subPeriodEndTime = dashaPeriod.StartTime.AddMonths((int)(i * subPeriodMonths));

                    subPeriods.Add(new SubPeriod
                    {
                        SignName = subPeriodSign,
                        StartTime = subPeriodStartTime,
                        EndTime = subPeriodEndTime
                    });
                }
            }

            return subPeriods;
        }

        /// <summary>
        /// Determines the order type (direct or indirect) for the sub-periods.
        /// </summary>
        private static string GetOrderType(ZodiacName ninthHouse)
        {
            // The order type depends on the nature of the 9th house from the current Dasha sign
            if (ninthHouse == ZodiacName.Aries || ninthHouse == ZodiacName.Taurus || ninthHouse == ZodiacName.Gemini ||
                ninthHouse == ZodiacName.Libra || ninthHouse == ZodiacName.Scorpio || ninthHouse == ZodiacName.Sagittarius)
            {
                return "direct";
            }
            else
            {
                return "indirect";
            }
        }

        /// <summary>
        /// Gets the degrees of the planets at birth time.
        /// </summary>
        private static Dictionary<PlanetName, double> GetPlanetDegrees(Time birthTime)
        {
            // Get the degrees of the planets at birth time
            var planetDegrees = new Dictionary<PlanetName, double>();
            foreach (var planet in PlanetName.All7Planets)
            {
                var planetZodiac = Calculate.PlanetRasiD1Sign(planet, birthTime);
                planetDegrees[planet] = planetZodiac.GetDegreesInSign().TotalDegrees;
            }
            return planetDegrees;
        }


        /// <summary>
        /// Calculates the Chara Dasha order starting from the Lagna sign.
        /// </summary>
        private static List<ZodiacName> CalculateCharaDashaOrder(ZodiacName lagnaSign)
        {
            // Determine the order type based on Lagna sign
            var orderType = GetCharaDashaOrderType(lagnaSign);

            // Generate the Dasha order
            List<ZodiacName> dashaOrder = new List<ZodiacName>();
            if (orderType == "direct")
            {
                // Direct motion through zodiac signs
                for (int i = 0; i < 12; i++)
                {
                    var sign = (ZodiacName)(((int)lagnaSign + i - 1) % 12 + 1);
                    dashaOrder.Add(sign);
                }
            }
            else
            {
                // Reverse motion through zodiac signs
                for (int i = 0; i < 12; i++)
                {
                    var sign = (ZodiacName)(((int)lagnaSign - i - 1 + 12) % 12 + 1);
                    dashaOrder.Add(sign);
                }
            }
            return dashaOrder;
        }

        /// <summary>
        /// Determines the order type (direct or indirect) for Chara Dasha based on Lagna sign.
        /// </summary>
        private static string GetCharaDashaOrderType(ZodiacName lagnaSign)
        {
            // The order type depends on the nature of the 9th house from Lagna
            var ninthHouseSign = Calculate.SignCountedFromInputSign(lagnaSign, 9);
            if (ninthHouseSign == ZodiacName.Aries || ninthHouseSign == ZodiacName.Taurus || ninthHouseSign == ZodiacName.Gemini ||
                ninthHouseSign == ZodiacName.Libra || ninthHouseSign == ZodiacName.Scorpio || ninthHouseSign == ZodiacName.Sagittarius)
            {
                return "direct";
            }
            else
            {
                return "indirect";
            }
        }

        /// <summary>
        /// Calculates the duration of each Chara Dasha period in years.
        /// </summary>
        private static Dictionary<ZodiacName, double> CalculateCharaDashaYears(Time birthTime, List<ZodiacName> dashaOrder)
        {
            Dictionary<ZodiacName, double> dashaYears = new Dictionary<ZodiacName, double>();

            foreach (var sign in dashaOrder)
            {
                // Calculate the duration for each sign
                double years = CalculateCharaDashaDurationForSign(birthTime, sign);
                dashaYears[sign] = years;
            }

            return dashaYears;
        }

        /// <summary>
        /// Calculates the duration of the Chara Dasha period for a particular sign.
        /// </summary>
        private static double CalculateCharaDashaDurationForSign(Time birthTime, ZodiacName sign)
        {
            // Get the lord of the sign
            var lord = Calculate.LordOfZodiacSign(sign);

            // Get the position of the lord
            var lordSign = Calculate.PlanetRasiD1Sign(lord, birthTime).GetSignName();

            // Calculate the distance between the signs
            int distance = Calculate.GetSignDistance(sign, lordSign);
            if (distance == 0)
            {
                distance = 12;
            }

            return distance;
        }

        /// <summary>
        /// Generates the Chara Dasha periods with start and end times.
        /// </summary>
        private static List<DashaPeriod> GenerateCharaDashaPeriods(Time birthTime, List<ZodiacName> dashaOrder, Dictionary<ZodiacName, double> dashaYears)
        {
            List<DashaPeriod> dashaPeriods = new List<DashaPeriod>();
            var currentTime = birthTime.GetLmtDateTimeOffset();

            foreach (var sign in dashaOrder)
            {
                double years = dashaYears[sign];
                var endTime = currentTime.AddYears((int)years).AddMonths((int)((years - (int)years) * 12));

                dashaPeriods.Add(new DashaPeriod
                {
                    SignName = sign,
                    StartTime = currentTime,
                    EndTime = endTime
                });

                currentTime = endTime;
            }

            return dashaPeriods;
        }


        /// <summary>
        /// Calculates the distance between two zodiac signs.
        /// </summary>
        private static int GetSignDistance(ZodiacName fromSign, ZodiacName toSign)
        {
            int distance = ((int)toSign - (int)fromSign + 12) % 12;
            return distance == 0 ? 12 : distance;
        }

        #endregion

        #region DASA

        /// <summary>
        /// Given a start time and end time and birth time will calculate all dasa periods
        /// in nice JSON table format
        /// You can also set how many levels of dasa you want to calculate, default is 4
        /// 7 Levels : Dasa > Bhukti > Antaram > Sukshma > Prana > Avi Prana > Viprana
        /// </summary>
        /// <param name="levels">range 1 to 7, coresponds to Dasa, Bhukti, Antaram, Sukshma etc..., lower is faster</param>
        /// <param name="scanYears">time span to calculate, defaults 100 years, average life</param>
        /// <param name="precisionHours">defaults to 24 hours, since any number above that causes dasa end & start time to not allign</param>
        public static JObject DasaForLife(Time birthTime, int levels = 3, int precisionHours = 24, int scanYears = 100)
        {
            //TODO NOTE:
            //precisionHours limits the levels that can be calculated (because of 0 duration filtering)

            //based on scan years, set start & end time
            Time startTime = birthTime;
            Time endTime = birthTime.AddYears(scanYears);

            //set what dasa levels to include based on input level
            var tagList = new List<EventTag>();
            for (int i = 1; i <= levels; i++)
            {
                tagList.Add((EventTag)Enum.Parse(typeof(EventTag), $"PD{i}"));
            }

            // TEMP hack to place time in Person (wrapped) 
            var johnDoe = new Person("", birthTime, Gender.Empty);

            //do calculation (heavy computation)
            List<Event> eventList = EventManager.CalculateEvents(precisionHours,
                                                                        startTime,
                                                                        endTime,
                                                                        johnDoe,
                                                                        tagList);

            //convert to Dasa Events for special Dasa related formating
            var dasaEventList = eventList.Select(e => new DasaEvent(e)).ToList();

            //format raw events into nested JSON format
            var dasaEvents1 = VimshottariDasa.GetDasaJson(dasaEventList, 1);

            return dasaEvents1;

        }

        /// <summary>
        /// Calculates dasa for a specific time frame
        /// </summary>
        /// <param name="startTime">start of time range to show dasa</param>
        /// <param name="endTime">end of time range to show dasa</param>
        /// <param name="levels">range 1 to 7,coresponds to bhukti, antaram, etc..., lower is faster</param>
        /// <param name="precisionHours"> defaults to 21 days, higher is faster
        /// set how accurately the start & end time of each event is calculated
        /// exp: setting 1 hour, means given in a time range of 100 years, it will be checked 876600 times 
        /// </param>
        /// <returns></returns>
        public static JObject DasaAtRange(Time birthTime, Time startTime, Time endTime, int levels = 3, int precisionHours = 100)
        {
            //TODO NOTE:
            //precisionHours limits the levels that can be calculated (because of 0 filtering)

            //based on scan years, set start & end time

            //set what dasa levels to include based on input level
            var tagList = new List<EventTag>();
            for (int i = 1; i <= levels; i++)
            {
                tagList.Add((EventTag)Enum.Parse(typeof(EventTag), $"PD{i}"));
            }

            // TEMP hack to place time in Person (wrapped) 
            var johnDoe = new Person("", birthTime, Gender.Empty);

            //do calculation (heavy computation)
            List<Event> eventList = EventManager.CalculateEvents(precisionHours,
                                                                        startTime,
                                                                        endTime,
                                                                        johnDoe,
                                                                        tagList);

            //convert to Dasa Events for special Dasa related formating
            var dasaEventList = eventList.Select(e => new DasaEvent(e)).ToList();

            //format raw events into nested JSON format
            var dasaEvents1 = VimshottariDasa.GetDasaJson(dasaEventList, 1);

            return dasaEvents1;
        }

        public static JObject DasaAtTime(Time birthTime, Time checkTime, int levels = 3)
        {
            //TODO NOTE:
            //precisionHours limits the levels that can be calculated (because of 0 filtering)
            //based on scan years, set start & end time

            //set what dasa levels to include based on input level
            var tagList = new List<EventTag>();
            for (int i = 1; i <= levels; i++)
            {
                tagList.Add((EventTag)Enum.Parse(typeof(EventTag), $"PD{i}"));
            }

            // TEMP hack to place time in Person (wrapped) 
            var johnDoe = new Person("", birthTime, Gender.Empty);

            //do calculation (heavy computation)
            List<Event> eventList = EventManager.CalculateEvents(1,
                                                                        checkTime,
                                                                        checkTime,
                                                                        johnDoe,
                                                                        tagList, false);

            //convert to Dasa Events for special Dasa related formating
            var dasaEventList = eventList.Select(e => new DasaEvent(e)).ToList();

            //format raw events into nested JSON format
            var dasaEvents1 = VimshottariDasa.GetDasaJson(dasaEventList, 1);

            return dasaEvents1;

        }

        public static JObject DasaForNow(Time birthTime, int levels = 3)
        {
            //TODO NOTE:
            //precisionHours limits the levels that can be calculated (because of 0 filtering)
            //based on scan years, set start & end time

            //set what dasa levels to include based on input level
            var tagList = new List<EventTag>();
            for (int i = 1; i <= levels; i++)
            {
                tagList.Add((EventTag)Enum.Parse(typeof(EventTag), $"PD{i}"));
            }

            // TEMP hack to place time in Person (wrapped) 
            var johnDoe = new Person("", birthTime, Gender.Empty);

            //get now time using birth location
            var nowTime = Time.NowSystem(birthTime.GetGeoLocation());

            //do calculation (heavy computation)
            List<Event> eventList = EventManager.CalculateEvents(1,
                                                                        nowTime,
                                                                        nowTime,
                                                                        johnDoe,
                                                                        tagList, false);

            //convert to Dasa Events for special Dasa related formating
            var dasaEventList = eventList.Select(e => new DasaEvent(e)).ToList();

            //format raw events into nested JSON format
            var dasaEvents1 = VimshottariDasa.GetDasaJson(dasaEventList, 1);

            return dasaEvents1;

        }

        #endregion

        #region PLANET BENEFIC & MALEFIC

        /// <summary>
        /// Whenever an affiiction by way of a malefic occupying
        /// a certain house or joining with a certain planet
        /// is suggested, by implication an aspect is also meant,
        /// though an affliction caused by aspect.is comparatively less malevolent
        ///
        /// Note:
        /// TODO presently not 100% sure, if what is meant by "affliction" is solely only limited to
        /// aspects & conjunction with bad planets. Or
        /// -Located in enemy sign an affliction?
        /// -Low shadbala an affliction?
        /// -Low drikbala an affliction?
        ///
        /// 
        /// At present, malefic aspects & conjunctions are used
        /// becasue it seems based on texts that this is correct.
        /// 
        /// But it seems mercury in enemny sign or position in a house should also play a role.
        /// 
        /// There must be a corelation between shadbala or drikbala to aspects & conjucntion
        /// A more precise way of mesurement it could be via the bala method.
        /// Needs testing for sure, to find out what bala values determine an afflicted mercury
        ///
        /// </summary>
        /// TODO POSSIBLE RENAME TO is IsMercuryMalefic
        public static bool IsMercuryAfflicted(Time time)
        {
            //for now only malefic conjunctions are considered
            return IsMercuryMalefic(time);

        }

        /// <summary>
        /// Check if Mercury is malefic (true), returns false if benefic 
        /// 
        ///
        /// References:
        /// 
        /// "Mercury, by nature, is called sournya or good. And if he is in
        /// conjunction with the Sun, Saturn, Mars, Rahu or Ketu, he will
        /// be a malefic. His conjunction with beneficial planets like Full
        /// Moon, Jupiter or Venus will classify him as a benefic. Benefic
        /// means a good and malefic means an evil planet."
        /// -TODO Does malefic moon make it malefic? (atm malefic moon makes it malefic)
        ///
        /// "Though in the earlier pages Mercury is defined either as a subba
        /// (benefic) or papa (malefic) according to its association is with a benefic or
        /// malefic, Mercury for purposes of calculating Drisbtibala of Bbavas is to
        /// be deemed as a full benefic. This is in accord with the injunctions of
        /// classical writers (Gurugnabbyam tu yuktasya poomamekam tu yojayet).
        /// "
        ///11. Benefics and Malefics. Among these, Sūrya, Śani, Mangal, decreasing Candr, Rahu and
        /// Ketu (the ascending and the descending nodes of Candr) are malefics, while the rest are
        /// benefics. Budh, however, is a malefic, if he joins a malefic. 
        /// 
        /// Note:
        /// ATM malefic planets override benefic
        /// TODO not sure if malefic planet overrides benefic if both are conjunct
        /// </summary>
        public static bool IsMercuryMalefic(Time time)
        {
            //CACHE MECHANISM
            return CacheManager.GetCache(new CacheKey(nameof(IsMercuryMalefic), time, Ayanamsa), _isMercuryMalefic);


            //UNDERLYING FUNCTION
            bool _isMercuryMalefic()
            {
                //if mercury is already with malefics,then not checking if conjunct with benefic (not 100% sure)
                if (conjunctWithMalefic()) { return true; }

                //if conjunct with benefic, then it is benefic
                if (conjunctWithBenefic()) { return false; }

                //if not conjunct with any planet, should be benefic
                //NOTE : Mercury, by nature, is called sournya or good.
                return false; // false means not malefic


                //------------FUNCTIONS-------------

                bool conjunctWithMalefic()
                {
                    //list the planets that will make mercury malefic
                    var evilPlanetNameList = new List<PlanetName>() { Library.PlanetName.Sun, Library.PlanetName.Saturn, Library.PlanetName.Mars, Library.PlanetName.Rahu, Library.PlanetName.Ketu };

                    //if moon is malefic, add to malefic list
                    if (!IsMoonBenefic(time)) { evilPlanetNameList.Add(Library.PlanetName.Moon); }

                    //get all planets in conjunction with mercury
                    var planetsConjunct = Calculate.PlanetsInConjunction(Library.PlanetName.Mercury, time);

                    //mark evil planet not in conjunct at first
                    bool evilPlanetFoundInConjunct = false;

                    //check if evil planets are in conjunct
                    foreach (var planetName in evilPlanetNameList)
                    {
                        evilPlanetFoundInConjunct = planetsConjunct.Contains(planetName);

                        //if one evil planet is found, break loop, stop looking
                        if (evilPlanetFoundInConjunct) { break; }
                    }

                    //return flag of evil planets found in conjunct
                    return evilPlanetFoundInConjunct;

                }

                bool conjunctWithBenefic()
                {
                    var beneficPlanetNameList = new List<PlanetName>() { Library.PlanetName.Jupiter, Library.PlanetName.Venus };

                    //if moon is benefic, add to benefic list
                    if (IsMoonBenefic(time)) { beneficPlanetNameList.Add(Library.PlanetName.Moon); }

                    //get all planets in conjunction with mercury
                    var planetsConjunct = Calculate.PlanetsInConjunction(Library.PlanetName.Mercury, time);

                    //mark benefic planet not in conjunct at first
                    bool beneficPlanetFoundInConjunct = false;

                    //check if benefic planets are in conjunct
                    foreach (var planetName in beneficPlanetNameList)
                    {
                        beneficPlanetFoundInConjunct = planetsConjunct.Contains(planetName);

                        //if one good planet is found, break loop, stop looking
                        if (beneficPlanetFoundInConjunct) { break; }
                    }

                    //return flag of benefic planets found in conjunct
                    return beneficPlanetFoundInConjunct;

                }

            }


        }

        /// <summary>
        /// Moon is a benefic from the 8th day of the bright half of the lunar month
        /// to the 8th day of the dark half of the lunar month
        /// and a malefic in the rest of the days.
        /// 
        /// Returns true if benefic & false if malefic
        /// </summary>
        public static bool IsMoonBenefic(Time time)
        {
            //get the lunar date number
            int lunarDateNumber = Calculate.LunarDay(time).GetLunarDateNumber();

            //8th day of the bright half = 8th lunar date
            //8th day of the dark half  = 23rd lunar date
            if (lunarDateNumber >= 8 && lunarDateNumber <= 23)
            {
                return true;
            }
            else
            {
                return false;
            }

        }


        /// <summary>
        /// Checks if a given planet is benefic
        /// </summary>
        public static bool IsPlanetBenefic(PlanetName planetName, Time time)
        {
            //get list of benefic planets
            var beneficPlanetList = BeneficPlanetList(time);

            //check if input planet is in the list
            var planetIsBenefic = beneficPlanetList.Contains(planetName);

            return planetIsBenefic;
        }

        /// <summary>
        /// Gets all planets that are benefics at a given time, since moon & mercury changes
        /// Benefics, on the other hand, tend to do good ; but
        /// sometimes they also become capable of doing harm.
        /// </summary>
        public static List<PlanetName> BeneficPlanetList(Time time)
        {
            //Add permanent good planets to list first
            var listOfGoodPlanets = new List<PlanetName>() { PlanetName.Jupiter, PlanetName.Venus };

            //check if moon is benefic
            var moonIsBenefic = IsMoonBenefic(time);

            //if moon is benefic add to benefic list
            if (moonIsBenefic) { listOfGoodPlanets.Add(PlanetName.Moon); }

            //check if mercury is good
            var mercuryIsBenefic = IsMercuryMalefic(time) == false;

            //if mercury is benefic add to benefic list
            if (mercuryIsBenefic) { listOfGoodPlanets.Add(PlanetName.Mercury); }

            return listOfGoodPlanets;
        }

        /// <summary>
        /// Checks if a given planet is Malefic
        /// </summary>
        public static bool IsPlanetMalefic(PlanetName planetName, Time time)
        {
            //get list of malefic planets
            var maleficPlanetList = MaleficPlanetList(time);

            //check if input planet is in the list
            var planetIsMalefic = maleficPlanetList.Contains(planetName);

            return planetIsMalefic;
        }

        /// <summary>
        /// Gets list of permanent malefic planets,
        /// for moon & mercury it is based on changing factors
        ///
        /// Malefics are always inclined to do harm, but under certain
        /// conditions, the intensity of the mischief is tempered.
        /// </summary>
        public static List<PlanetName> MaleficPlanetList(Time time)
        {
            //Add permanent evil planets to list first
            var listOfEvilPlanets = new List<PlanetName>() { Library.PlanetName.Sun, Library.PlanetName.Saturn, Library.PlanetName.Mars, Library.PlanetName.Rahu, Library.PlanetName.Ketu };

            //check if moon is evil
            var moonIsEvil = IsMoonBenefic(time) == false;

            //if moon is evil add to evil list
            if (moonIsEvil)
            {
                listOfEvilPlanets.Add(Library.PlanetName.Moon);
            }

            //check if mercury is evil
            var mercuryIsEvil = IsMercuryMalefic(time);
            //if mercury is evil add to evil list
            if (mercuryIsEvil)
            {
                listOfEvilPlanets.Add(Library.PlanetName.Mercury);
            }

            return listOfEvilPlanets;
        }

        /// <summary>
        /// Gets all planets the inputed planet is transmitting aspect to
        /// </summary>
        public static List<PlanetName> PlanetsInAspect(PlanetName inputPlanet, Time time)
        {
            //get signs planet is aspecting
            var signAspecting = Calculate.SignsPlanetIsAspecting(inputPlanet, time);

            //get all the planets located in these signs
            var planetsAspected = new List<PlanetName>();
            foreach (var zodiacSign in signAspecting)
            {
                var planetInSign = Calculate.PlanetInSign(zodiacSign, time);

                //add to list
                planetsAspected.AddRange(planetInSign);
            }

            //return these planets as aspected by input planet
            return planetsAspected;
        }

        /// <summary>
        /// Calculate aspect angle between 2 planets
        /// </summary>
        public static double PlanetAspectDegree(PlanetName receiver, PlanetName trasmitter, Time time)
        {
            //Finding Drishti Kendra or Aspect Angle
            var planetNirayanaLongitude = Calculate.PlanetNirayanaLongitude(receiver, time).TotalDegrees;
            var nirayanaLongitude = Calculate.PlanetNirayanaLongitude(trasmitter, time).TotalDegrees;
            var dk = planetNirayanaLongitude - nirayanaLongitude;

            if (dk < 0) { dk += 360; }

            //get special aspect if any
            var vdrishti = FindViseshaDrishti(dk, trasmitter);

            var final = FindDrishtiValue(dk) + vdrishti;

            return final;

        }

        /// <summary>
        /// Gets all planets the transmitting aspect to inputed planet
        /// </summary>
        public static List<PlanetName> PlanetsAspectingPlanet(PlanetName receivingAspect, Time time)
        {
            //check if all planets is aspecting inputed planet
            var aspectFound = Library.PlanetName.All9Planets.FindAll(transmitPlanet => IsPlanetAspectedByPlanet(receivingAspect, transmitPlanet, time));

            return aspectFound;
        }

        /// <summary>
        /// Gets houses aspected by the inputed planet
        /// </summary>
        public static List<HouseName> HousesInAspect(PlanetName planet, Time time)
        {
            //get signs planet is aspecting
            var signAspecting = Calculate.SignsPlanetIsAspecting(planet, time);

            //get all the houses located in these signs
            var housesAspected = new List<HouseName>();
            foreach (var house in Library.House.AllHouses)
            {
                //get sign of house
                var houseSign = Calculate.HouseSignName(house, time);

                //add house to list if sign is aspected by planet
                if (signAspecting.Contains(houseSign)) { housesAspected.Add(house); }
            }

            //return the houses aspected by input planet
            return housesAspected;

        }

        /// <summary>
        /// Gets all planets aspecting inputed house
        /// </summary>
        public static List<PlanetName> PlanetsAspectingHouse(HouseName inputHouse, Time time)
        {
            //create empty list
            var returnPlanetList = new List<PlanetName>();

            //check each planet if aspecting house
            foreach (var planet in Library.PlanetName.All9Planets)
            {
                //get houses
                var housesInAspect = HousesInAspect(planet, time);

                //check if any house is a match
                var houseMatch = housesInAspect.FindAll(house => house == inputHouse).Any();
                if (houseMatch)
                {
                    returnPlanetList.Add(planet);
                }
            }


            return returnPlanetList;
        }

        /// <summary>
        /// Checks if the a planet is aspected by another planet
        /// </summary>
        public static bool IsPlanetAspectedByPlanet(PlanetName receiveingAspect, PlanetName transmitingAspect, Time time)
        {
            //get planets aspected by transmiting planet
            var planetsInAspect = Calculate.PlanetsInAspect(transmitingAspect, time);

            //if receiving planet is in list of currently aspected
            return planetsInAspect.Contains(receiveingAspect);

        }

        /// <summary>
        /// Checks if a house is aspected by a planet
        /// </summary>
        public static bool IsHouseAspectedByPlanet(HouseName receiveingAspect, PlanetName transmitingAspect, Time time)
        {
            //get houses aspected by transmiting planet
            var houseInAspect = Calculate.HousesInAspect(transmitingAspect, time);

            //if receiving house is in list of currently aspected
            return houseInAspect.Contains(receiveingAspect);

        }

        /// <summary>
        /// Checks if the a planet is conjunct with another planet
        /// (Based on longitudes)
        /// Note:
        /// Both planets A & B are checked if they are in conjunct with each other,
        /// performance might be effected mildly, but errors in conjunction calculation would be caught here.
        /// Can be removed once, conjunction calculator is confirmed accurate.
        /// </summary>
        public static bool IsPlanetConjunctWithPlanet(PlanetName planetA, PlanetName planetB, Time time)
        {
            //get planets in conjunt for A & B
            var planetAConjunct = Calculate.PlanetsInConjunction(planetA, time);
            var planetBConjunct = Calculate.PlanetsInConjunction(planetB, time);

            //check that A conjuncts B and B conjuncts A 
            var conjunctFound = planetAConjunct.Contains(planetB) && planetBConjunct.Contains(planetA);

            return conjunctFound;
        }

        /// <summary>
        /// Check if benefic planets are conjunct with specified planet
        /// </summary>
        public static bool IsPlanetConjunctWithBeneficPlanets(PlanetName inputPlanet, Time time)
        {
            var beneficPlanets = Calculate.BeneficPlanetList(time);

            // Check if planet is conjunct with any benefic planet
            var isPlanetConjunctBenefic = beneficPlanets.Any(benefic => Calculate.IsPlanetConjunctWithPlanet(inputPlanet, benefic, time));

            return isPlanetConjunctBenefic;
        }

        #endregion

        #region PLANET AND HOUSE STRENGHT

        /// <summary>
        /// convert the planets strength into a value over hundred with max & min set by strongest & weakest planet
        /// </summary>
        public static double PlanetPowerPercentage(PlanetName inputPlanet, Time time)
        {
            //get all planet strength for given time (horoscope)
            var allPlanets = Calculate.AllPlanetStrength(time);

            //get the power of the planet inputed
            var planetPwr = allPlanets.FirstOrDefault(x => x.Item2 == inputPlanet).Item1;

            //get min & max
            var min = allPlanets.Min(x => x.Item1); //weakest planet
            var max = allPlanets.Max(x => x.Item1); //strongest planet

            //convert the planets strength into a value over hundred with max & min set by strongest & weakest planet
            //returns as percentage over 100%
            var factor = planetPwr.Remap(min, max, 0, 100);

            //planet power below 60% filtered out
            //factor = factor < 60 ? 0 : factor;

            return factor;
        }

        /// <summary>
        /// Given a list of planets will pick out the strongest planet based on Shadbala
        /// </summary>
        public static PlanetName PickOutStrongestPlanet(List<PlanetName> relatedPlanets, Time birthTime)
        {
            //if only 1 planet than no need to check
            if (relatedPlanets.Count == 1) { return relatedPlanets[0]; }

            //calculate strength for all given planets
            var powerList = new Dictionary<PlanetName, double>();
            foreach (var relatedPlanet in relatedPlanets)
            {
                var strength = Calculate.PlanetStrength(relatedPlanet, birthTime);
                powerList.Add(relatedPlanet, strength.ToDouble());
            }

            //pickout highest value
            var strongest = powerList.Aggregate((l, r) => l.Value > r.Value ? l : r);

            //return strongest planet name
            return strongest.Key;
        }

        /// <summary>
        /// Returns an array of all planets sorted by strenght,
        /// 0 index being strongest to 8 index being weakest
        ///
        /// Note:
        /// Significance of being Powerful.-Among
        /// the several planets associated with a bhava, that,
        /// which has the greatest Sbadbala, influences the
        /// bhava most.
        /// </summary>
        public static List<PlanetName> AllPlanetOrderedByStrength(Time time)
        {

            //CACHE MECHANISM
            return CacheManager.GetCache(new CacheKey(nameof(AllPlanetOrderedByStrength), time, Ayanamsa), _getAllPlanetOrderedByStrength);


            //UNDERLYING FUNCTION
            List<PlanetName> _getAllPlanetOrderedByStrength()
            {
                var planetStrenghtList = new Dictionary<PlanetName, double>();

                //create a list with planet names & its strength (unsorted)
                foreach (var planet in Library.PlanetName.All9Planets)
                {
                    //get planet strength in rupas
                    var strength = PlanetShadbalaPinda(planet, time).ToDouble();

                    //place in list with planet name
                    planetStrenghtList.Add(planet, strength);
                }


                //sort that list from strongest planet to weakest planet
                var sortedList = planetStrenghtList.OrderByDescending(item => item.Value);
                var nameOnlyList = sortedList.Select(x => x.Key).ToList();

                return nameOnlyList;

                /*--------------FUNCTIONS----------------*/
            }
        }

        /// <summary>
        /// Significance of being Powerful.-Among
        /// the several planets associated with a bhava, that,
        /// which has the greatest Sbadbala, influences the
        /// bhava most.
        /// Powerful Planets.-Ravi is befd to be
        /// powerful when his Shadbala Pinda is 5 or more
        /// rupas. Chandra becomes strong when his Shadbala
        /// Pinda is 6 or more rupas. Kuja becomes powerful
        /// when bis Shadbala Pinda does not fall short of
        /// 5 rupas.Budha becomes potent by having his
        /// Sbadbala Pinda as 7 rupas; Guru, Sukra and Sani
        /// become thoroughly powerful if their Shadbala
        /// Pindas are 6.5, 5.5 and 5 rupas or more respectively.
        /// </summary>
        public static bool IsPlanetStrongInShadbala(PlanetName planet, Time time)
        {

            var limit = 0.0;
            if (planet == Sun) { limit = 5; }
            else if (planet == Library.PlanetName.Moon) { limit = 6; }
            else if (planet == Library.PlanetName.Mars) { limit = 5; }
            else if (planet == Library.PlanetName.Mercury) { limit = 7; }
            else if (planet == Library.PlanetName.Jupiter) { limit = 6.5; }
            else if (planet == Library.PlanetName.Venus) { limit = 5.5; }
            else if (planet == Library.PlanetName.Saturn) { limit = 5; }
            //todo rahu and ketu added later on based on saturn and mars
            else if (planet == Library.PlanetName.Rahu) { limit = 5; }
            else if (planet == Library.PlanetName.Ketu) { limit = 5; }

            //divide strength by minimum limit of power (based on planet)
            //if above limit than benefic, else return false
            var shadbalaRupa = Calculate.PlanetShadbalaPinda(planet, time);
            var rupa = Math.Round(shadbalaRupa.ToRupa(), 1);
            var strengthAfterLimit = rupa / limit;

            //if 1 or above is positive, below 1 is below limit
            var isBenefic = strengthAfterLimit >= 1.1;

            return isBenefic;
        }

        /// <summary>
        /// sets benefic if above 450 score
        /// </summary>
        public static bool IsHouseBeneficInShadbala(HouseName house, Time birthTime, double threshold)
        {
            //get house strength
            var strength = HouseStrength(house, birthTime).ToDouble();

            //if above 450 then good
            var isBenefic = strength > threshold;
            return isBenefic;
        }

        /// <summary>
        /// Gets  strength (shadbala) of all 9 planets
        /// </summary>
        public static List<(double, PlanetName)> AllPlanetStrength(Time time)
        {
            var planetStrenghtList = new List<(double, PlanetName)>();

            //create a list with planet names & its strength (unsorted)
            foreach (var planet in Library.PlanetName.All9Planets)
            {
                //get planet strength in rupas
                var strength = PlanetShadbalaPinda(planet, time).ToDouble();

                //place in list with planet name
                planetStrenghtList.Add((strength, planet));

            }

            return planetStrenghtList;
        }

        /// <summary>
        /// Returns an array of all houses sorted by strength,
        /// 0 index being strongest to 11 index being weakest
        /// </summary>
        public static HouseName[] AllHousesOrderedByStrength(Time time)
        {

            //CACHE MECHANISM
            return CacheManager.GetCache(new CacheKey(nameof(AllHousesOrderedByStrength), time, Ayanamsa), _getAllHousesOrderedByStrength);


            //UNDERLYING FUNCTION
            HouseName[] _getAllHousesOrderedByStrength()
            {
                var houseStrengthList = new Dictionary<double, HouseName>();

                //create a list with planet names & its strength (unsorted)
                foreach (var house in Library.House.AllHouses)
                {
                    //get house strength
                    var strength = HouseStrength(house, time).ToRupa();

                    //place in list with house number
                    houseStrengthList[strength] = house;

                }


                //sort that list from strongest house to weakest house
                var keysSorted = houseStrengthList.Keys.ToList();
                keysSorted.Sort();

                var sortedArray = new HouseName[12];
                var count = 11;
                foreach (var key in keysSorted)
                {
                    sortedArray[count] = houseStrengthList[key];
                    count--;
                }

                return sortedArray;
            }

        }

        /// <summary>
        /// THE FINAL TOTAL STRENGTH
        /// Shadbala :the six sources of strength and weakness the planets
        /// The importance of and the part played by the Shadbalas,
        /// in the science of horoscopy, are manifold
        ///
        /// In order to obtain the total strength of
        /// the Shadbala Pinda of each planet, we have to add
        /// together its Sthana Bala, Dik Bala, Kala Bala.
        /// 'Chesta Bala and Naisargika Bala. And the Graha's
        /// Drik Bala must be added to or subtracted from the
        /// above sum according as it is positive or negative.
        /// The result obtained is the Shadbala Pinda of the
        /// planet in Shashtiamsas.
        ///
        /// Note: Rahu & Ketu supported, via house lord
        /// </summary>
        public static Shashtiamsa PlanetShadbalaPinda(PlanetName planetName, Time time)
        {
            try
            {
                //return 0 if null planet
                if (planetName == null) { return Shashtiamsa.Zero; }

                //CACHE MECHANISM
                return CacheManager.GetCache(new CacheKey(nameof(PlanetShadbalaPinda), planetName, time, Ayanamsa), _getPlanetShadbalaPinda);


                //UNDERLYING FUNCTION
                Shashtiamsa _getPlanetShadbalaPinda()
                {

                    //if planet name is rahu or ketu then replace with house lord's strength
                    if (planetName == Rahu || planetName == Ketu)
                    {
                        var houseLord = LordOfHousePlanetIsIn(planetName, time);
                        planetName = houseLord;
                    }

                    //Sthana bala (Positional Strength)
                    var sthanaBala = PlanetSthanaBala(planetName, time);

                    //Get dik bala (Directional Strength)
                    var digBala = PlanetDigBala(planetName, time);

                    //Get kala bala (Temporal Strength)
                    var kalaBala = PlanetKalaBala(planetName, time);

                    //Get Chesta bala (Motional Strength)
                    var chestaBala = PlanetChestaBala(planetName, time);

                    //Get Naisargika bala (Natural Strength)
                    var naisargikaBala = PlanetNaisargikaBala(planetName, time);

                    //Get Drik/drug bala (Aspect Strength)
                    var drikBala = PlanetDrikBala(planetName, time);


                    //Get total Shadbala Pinda
                    var total = sthanaBala + digBala + kalaBala + chestaBala + naisargikaBala + drikBala;

                    //round it 2 decimal places
                    var roundedTotal = new Shashtiamsa(Math.Round(total.ToDouble(), 2));

                    return roundedTotal;
                }
            }
            catch (Exception e)
            {
                //print the error and for server guys
                Console.WriteLine(e);

                //continue without a word
                return Shashtiamsa.Zero;
            }

        }

        /// <summary>
        /// get total combined strength of the inputed planet
        /// input birth time to get strength in horoscope
        /// note: an alias method to GetPlanetShadbalaPinda ("strength" is easier to remember)
        /// </summary>
        public static Shashtiamsa PlanetStrength(PlanetName planetName, Time time) => PlanetShadbalaPinda(planetName, time);

        /// <summary>
        /// Gets the lord of the house the inputed planet is in
        /// </summary>
        private static PlanetName LordOfHousePlanetIsIn(PlanetName planetName, Time time)
        {
            var currentHouse = Calculate.HousePlanetOccupiesBasedOnLongitudes(planetName, time);
            var houseLord = Calculate.LordOfHouse((HouseName)currentHouse, time);

            return houseLord;
        }

        /// <summary>
        /// Aspect strength
        ///
        /// This strength is gained by the virtue of the aspect
        /// (Graha Dristi) of different planets on other planet.
        /// The aspect of benefics is considered to be strength and
        /// the aspect of malefics is considered to be weaknesses.
        ///
        /// 
        /// Drik Bala.-This means aspect strength.
        /// The Drik Bala of a Gqaha is one-fourth of the
        /// Drishti Pinda on it. It is positive or negative
        /// according as the Drishti Pinda is positive or
        /// negative.
        ///
        /// 
        /// See the formula given on page 85. There is
        /// special aspect for Jupiter, ,Mars and Saturn on the
        /// 5th and 9th, 4th and 8th and 3rd and 10th signs
        /// respectively.
        /// </summary>
        public static Shashtiamsa PlanetDrikBala(PlanetName planetName, Time time)
        {
            //no calculation for rahu and ketu here
            var isRahu = planetName.Name == Library.PlanetName.PlanetNameEnum.Rahu;
            var isKetu = planetName.Name == Library.PlanetName.PlanetNameEnum.Ketu;
            var isRahuKetu = isRahu || isKetu;
            if (isRahuKetu) { return Shashtiamsa.Zero; }

            double dk;
            var drishti = new Dictionary<string, double>();
            double vdrishti;
            var sp = new Dictionary<PlanetName, int>();


            foreach (var p in Library.PlanetName.All7Planets)
            {
                if (Calculate.IsPlanetBenefic(p, time))
                {
                    sp[p] = 1;
                }
                else
                {
                    sp[p] = -1;
                }

            }


            foreach (var i in Library.PlanetName.All7Planets)
            {
                foreach (var j in Library.PlanetName.All7Planets)
                {
                    //Finding Drishti Kendra or Aspect Angle
                    var planetNirayanaLongitude = Calculate.PlanetNirayanaLongitude(j, time).TotalDegrees;
                    var nirayanaLongitude = Calculate.PlanetNirayanaLongitude(i, time).TotalDegrees;
                    dk = planetNirayanaLongitude - nirayanaLongitude;

                    if (dk < 0) { dk += 360; }

                    //get special aspect if any
                    vdrishti = FindViseshaDrishti(dk, i);

                    drishti[i.ToString() + j.ToString()] = FindDrishtiValue(dk) + vdrishti;

                }
            }

            double bala = 0;

            var DrikBala = new Dictionary<PlanetName, double>();

            foreach (var i in Library.PlanetName.All7Planets)
            {
                bala = 0;

                foreach (var j in All7Planets)
                {
                    bala = bala + (sp[j] * drishti[j.ToString() + i.ToString()]);

                }

                DrikBala[i] = bala / 4;
            }



            return new Shashtiamsa(DrikBala[planetName]);



        }

        /// <summary>
        /// Get special aspect if any of Kuja, Guru and Sani
        /// </summary>
        public static double FindViseshaDrishti(double dk, PlanetName p)
        {
            double vdrishti = 0;

            if (p == Library.PlanetName.Saturn)
            {
                if (((dk >= 60) && (dk <= 90)) || ((dk >= 270) && (dk <= 300)))
                {
                    vdrishti = 45;
                }

            }
            else if (p == Library.PlanetName.Jupiter)
            {

                if (((dk >= 120) && (dk <= 150))
                    || ((dk >= 240) && (dk <= 270)))
                {
                    vdrishti = 30;
                }

            }
            else if (p == Library.PlanetName.Mars)
            {
                if (((dk >= 90) && (dk <= 120)) || ((dk >= 210) && (dk <= 240)))
                {
                    vdrishti = 15;
                }

            }
            else
            {
                vdrishti = 0;
            }


            return vdrishti;

        }

        public static double FindDrishtiValue(double dk)
        {

            double drishti = 0;

            if ((dk >= 30.0) && (dk <= 60))
            {
                drishti = (dk - 30) / 2;
            }
            else if ((dk > 60.0) && (dk <= 90))
            {
                drishti = (dk - 60) + 15;
            }
            else if ((dk > 90.0) && (dk <= 120))
            {
                drishti = ((120 - dk) / 2) + 30;
            }
            else if ((dk > 120.0) && (dk <= 150))
            {
                drishti = (150 - dk);
            }
            else if ((dk > 150.0) && (dk <= 180))
            {
                drishti = (dk - 150) * 2;
            }
            else if ((dk > 180.0) && (dk <= 300))
            {
                drishti = (300 - dk) / 2;
            }

            return drishti;

        }

        /// <summary>
        /// Nalsargika Bala.-This is the natural
        /// strength that each Graha possesses. The value
        /// assigned to each depends upon its luminosity.
        /// Ravi, the brightest of all planets, has the greatest
        /// Naisargika strength while Sani, the darkest, has
        /// the least Naisargika Bala.
        ///
        /// This is the natural or inherent strength of a planet.
        /// </summary>
        public static Shashtiamsa PlanetNaisargikaBala(PlanetName planetName, Time time)
        {
            //no calculation for rahu and ketu here
            var isRahu = planetName.Name == Library.PlanetName.PlanetNameEnum.Rahu;
            var isKetu = planetName.Name == Library.PlanetName.PlanetNameEnum.Ketu;
            var isRahuKetu = isRahu || isKetu;
            if (isRahuKetu) { return Shashtiamsa.Zero; }


            if (planetName == Library.PlanetName.Sun) { return new Shashtiamsa(60); }
            else if (planetName == Library.PlanetName.Moon) { return new Shashtiamsa(51.43); }
            else if (planetName == Library.PlanetName.Venus) { return new Shashtiamsa(42.85); }
            else if (planetName == Library.PlanetName.Jupiter) { return new Shashtiamsa(34.28); }
            else if (planetName == Library.PlanetName.Mercury) { return new Shashtiamsa(25.70); }
            else if (planetName == Library.PlanetName.Mars) { return new Shashtiamsa(17.14); }
            else if (planetName == Library.PlanetName.Saturn) { return new Shashtiamsa(8.57); }

            throw new Exception("Planet not specified!");
        }

        /// <summary>
        /// NOTE: sun, moon get score for ISHTA/KESHA calculation only when specified for IshataKashta
        /// MOTIONAL STRENGTH
        /// Chesta here means Vakra Chesta or act of retrogression. Each planet, except the Sun and the Moon,
        /// and shadowy planets get into the state of Vakra or retrogression
        /// when its distance from the Sun exceeds a particular limit.
        /// And the strength or potency due to the planet on account of the arc of the retrogression is
        /// termed as Chesta Bala
        ///
        /// Deduct from the Seeghrocbcha, half the sum
        /// of the True and Mean Longitudes of planets and
        /// divide the difference by 3. The quotient is the
        /// Chestabala.
        /// Max 60, meaning Retrograde/Vakra
        /// When the distance of any one planet from
        /// the Sun exceeds a particular limit, it becomes
        /// retrograde, i.e., when the planet goes from
        /// perihelion (the point in a planet's orbit nearest
        /// to the Sun) to aphelion (the part of a planet's
        /// oroit most distant from the Sun) as it recedes
        /// from the Sun, it gradually loses the power
        /// of the Sun's gravitation and consequently, 
        /// </summary>
        public static Shashtiamsa PlanetChestaBala(PlanetName planetName, Time time, bool useSpecialSunMoon = false)
        {
            //if include Sun/Moon specified, then use special function (used for Ishta/Kashta score)
            if (planetName == Sun && useSpecialSunMoon) { return SunChestaBala(time); }
            if (planetName == Moon && useSpecialSunMoon) { return MoonChestaBala(time); }

            //the Sun,Moon,Rahu and Ketu does not not retrograde, so 0 chesta bala
            if (planetName == Sun || planetName == Moon || planetName == Rahu || planetName == Ketu) { return Shashtiamsa.Zero; }

            //get the interval between birth date and the date of the epoch (1900)
            //verified standard horoscope = 6862.579
            //NOTE: dates before 1900 give negative values
            var interval = EpochInterval(time);

            //get the mean/average longitudes of all planets
            var madhya = Madhya(interval, time);

            //get the apogee of all planets (apogee=earth, aphelion=sun)
            //aphelion (the part of a planet's orbit most distant from the Sun) 
            var seegh = GetSeeghrochcha(madhya, interval, time);


            //calculate chesta kendra, also called Seeghra kendra
            var planetLongitude = Calculate.PlanetNirayanaLongitude(planetName, time).TotalDegrees;
            //This is the Arc of retrogression.
            var planetAphelion = seegh[planetName]; //fixed most distant point from sun
            var planetMeanCircle = madhya[planetName]; //planet average distant point from sun (CIRCLE ORBIT)
                                                       //Chesta kendra = 180 degrees = Retrograde
                                                       //Because the orbits are elliptical
                                                       //and not circular, equations are applied to the mean positions to get the true longitudes.
            var trueLongitude = ((planetMeanCircle + planetLongitude) / 2.0);
            //distance from stationary point, if less than 0 than add 360 
            var chestaKendra = (planetAphelion - trueLongitude);


            //If the Chesta kendra exceeds 180° (maximum retrograde), subtract it from 360, otherwise
            //keep it as it is. The remainder represents the reduced Chesta kendra
            //NOTE: done to reduce value of direct motion, only value relative to retro motion
            if (chestaKendra < 360.00)
            {
                chestaKendra = chestaKendra + 360;
            }
            chestaKendra = chestaKendra % 360;
            if (chestaKendra > 180.00) { chestaKendra = 360 - chestaKendra; }


            //The Chesta Bala is zero when the Chesta kendra is also zero. When it is
            //180° the Chesta Bala is 60 Shashtiamsas. In intermediate position, the
            //Bala is found by proportion (devide by 3)
            var chestaBala = (chestaKendra / 3.00);

            return new Shashtiamsa(chestaBala);



            //------------------------FUNCTIONS--------------


            //Seeghrochcha is the aphelion of the planet
            //It is required to find the Chesta kendra.
            //NOTE:aphelion (the part of a planet's orbit most distant from the Sun)
            Dictionary<PlanetName, double> GetSeeghrochcha(Dictionary<PlanetName, double> mean, double epochToBirthDays, Time time1)
            {
                int _birthYear = time1.GetLmtDateTimeOffset().Year;
                var seegh = new Dictionary<PlanetName, double>();
                double correction;

                //The mean longitude of the Sun will be the Seeghrochcha of Kuja, Guru and Sani.
                seegh[Library.PlanetName.Mars] = seegh[Library.PlanetName.Jupiter] = seegh[Library.PlanetName.Saturn] = mean[Library.PlanetName.Sun];

                correction = 6.670 + (0.00133 * (_birthYear - 1900));
                double changeDuringIntervalMercury = (epochToBirthDays * 4.092385);
                const double aphelionAtEpochMercury = 164.00; // The Seeghrochcha of Budha at the epoch
                double mercuryAphelion = changeDuringIntervalMercury < 0 ? aphelionAtEpochMercury - changeDuringIntervalMercury : aphelionAtEpochMercury + changeDuringIntervalMercury;
                mercuryAphelion -= correction; //further correction of +6.670-0133
                seegh[Library.PlanetName.Mercury] = (mercuryAphelion + correction) % 360;

                correction = 5 + (0.0001 * (_birthYear - 1900));
                double changeDuringIntervalVenus = (epochToBirthDays * 1.602159);
                const double aphelionAtEpochVenus = 328.51; // The Seeghrochcha of Budha at the epoch
                double venusAphelion = changeDuringIntervalVenus < 0 ? aphelionAtEpochVenus - changeDuringIntervalVenus : aphelionAtEpochVenus + changeDuringIntervalVenus;
                venusAphelion -= correction; //diminish the sum by 5 + 0.001*t (where t = birth year - 1900)
                seegh[Library.PlanetName.Venus] = venusAphelion % 360;

                return seegh;
            }

        }

        /// <summary>
        /// special function to get chesta score for Ishta/Kashta score
        /// Bala book pg. 108
        ///
        /// Sun has no Chesta kendra or Chesta bala
        /// as he never gets into retrogression. But still a
        /// method is prescribed to find his Chesla Bala which
        /// is necessary to ascertain the lshta and Kashta·
        /// Phalas. 
        /// </summary>
        public static Shashtiamsa SunChestaBala(Time inputTime)
        {
            //Add 90° to Sun's Sayana longitude.
            var sunSayana = Calculate.PlanetSayanaLatitude(Sun, inputTime);

            //The result is Sun's Chesta kendra
            var chestaKendra = (sunSayana + Angle.Degrees90).TotalDegrees;

            //if chesta kendra execeeds 180 substract from 360
            if (chestaKendra > 180) { chestaKendra = 360 - chestaKendra; }

            //dividing this by 3 we get his Chesta bala in Shashtiamsa
            var chestaBala = chestaKendra / 3.0;

            return new Shashtiamsa(chestaBala);
        }

        /// <summary>
        /// special function to get chesta score for Ishta/Kashta score
        /// Bala book pg. 108
        /// </summary>
        public static Shashtiamsa MoonChestaBala(Time inputTime)
        {

            //Subtract the Sun's longitude from that of the Moon and the 
            //latter's Chesta Kendra is obtained.
            var sunNirayana = Calculate.PlanetNirayanaLongitude(Sun, inputTime);
            var moonNirayana = Calculate.PlanetNirayanaLongitude(Moon, inputTime);
            var chestaKendra = moonNirayana.GetDifference(sunNirayana).TotalDegrees;

            //if chesta kendra execeeds 180 substract from 360
            if (chestaKendra > 180) { chestaKendra = 360 - chestaKendra; }

            //dividing this by 3 we get his Chesta bala. in Shashtiamsa
            var chestaBala = chestaKendra / 3.0;

            return new Shashtiamsa(chestaBala);
        }

        /// <summary>
        /// The mean position of a planet is the position which it would have attained at a uniform
        /// rate of motion and the corrections to be applied in respect of the eccentricity of the orbit are not considered
        /// </summary>
        public static Dictionary<PlanetName, double> Madhya(double epochToBirthDays, Time time1)
        {
            int _birthYear = time1.GetLmtDateTimeOffset().Year;

            var madhya = new Dictionary<PlanetName, double>();

            //calculate chesta kendra, also called Seeghra kendra

            //SUN 
            //Start from the epoch. Calculate the time of interval from the epoch to the day of birth
            //and multiply the same by the daily motion of the planet, and the change during the interval is obtained.
            var sunEpochMean = 257.4568; //epoch the mean position
            double changeDuringIntervalSun = (epochToBirthDays * 0.9855931);

            //This change being added to or subtracted from the mean position at the
            //time of epoch as the date is posterior or anterior to the epoch day, the mean position is arrived at.
            double meanPositionSun = changeDuringIntervalSun < 0 ? sunEpochMean - changeDuringIntervalSun : sunEpochMean + changeDuringIntervalSun;
            meanPositionSun = meanPositionSun % 360; //expunge
            madhya[Library.PlanetName.Sun] = meanPositionSun;

            //Mean Longitudes of -Inferior Planets.-The mean longitudes of Budba and Sukra are the same as that of the Sun.
            //same for venus & mercury because closer to sun than earth it self
            madhya[Library.PlanetName.Mercury] = madhya[Library.PlanetName.Venus] = madhya[Library.PlanetName.Sun];

            //MARS
            var marsEpochMean = 270.22;
            double changeDuringIntervalMars = (epochToBirthDays * 0.5240218);
            double meanPositionMars = changeDuringIntervalMars < 0 ? marsEpochMean - changeDuringIntervalMars : marsEpochMean + changeDuringIntervalMars;
            meanPositionMars = meanPositionMars % 360; //expunge
            madhya[Library.PlanetName.Mars] = meanPositionMars;

            //JUPITER
            var jupiterEpochMean = 220.04;
            double changeDuringIntervalJupiter = (epochToBirthDays * 0.08310024);
            double meanPositionJupiter = changeDuringIntervalJupiter < 0 ? jupiterEpochMean - changeDuringIntervalJupiter : jupiterEpochMean + changeDuringIntervalJupiter;
            var correction1 = 3.33 + (0.0067 * (_birthYear - 1900));
            meanPositionJupiter -= correction1; //deduct from the total 3.33 + 0.0067*t (where t=birth year-1900).
            meanPositionJupiter %= 360; //expunge
            madhya[Library.PlanetName.Jupiter] = meanPositionJupiter;

            //SATURN
            var saturnEpochMean = 220.04;
            double changeDuringIntervalSaturn = (epochToBirthDays * 0.03333857);
            double meanPositionSaturn = changeDuringIntervalSaturn < 0 ? saturnEpochMean - changeDuringIntervalSaturn : saturnEpochMean + changeDuringIntervalSaturn;
            var correction2 = 5 + (0.001 * (_birthYear - 1900));
            meanPositionSaturn += correction2; //add 5°+0.001*t (where t = birth year - 1900)
            meanPositionSaturn %= 360; //expunge
            madhya[Library.PlanetName.Saturn] = meanPositionSaturn;

            //raise alarm if negative, since that is clearly an error, no negative mean longitude
            if (madhya.Any(x => x.Value < 0)) { throw new Exception("Madya/Mean can't be negative!"); }

            return madhya;
        }

        /// <summary>
        /// Get interval from the epoch to the birth date in days
        /// The result represents the interval from the epoch to the birth date.
        /// </summary>
        public static double EpochInterval(Time time1)
        {
            //Determine the interval between birth date and the date of the epoch thus.

            int birthYear = time1.GetLmtDateTimeOffset().Year;
            int birthMonth = time1.GetLmtDateTimeOffset().Month;
            int birthDate = time1.GetLmtDateTimeOffset().Day;

            //month ends in days
            int[] monthEnds = { 0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334, 365 };

            //Deduct 1900 from the Christian Era. The difference will be past
            //years when positive and coming years when negative.
            int yrdiff = birthYear - 1900;

            //Multiply the same by 365 and to the product add the intervening bi-sextile days.
            var epochDays = ((yrdiff * 365) + (yrdiff / 4) + monthEnds[birthMonth - 1]) - 1 + birthDate;


            int hour = time1.GetLmtDateTimeOffset().Hour;
            int minute = time1.GetLmtDateTimeOffset().Minute;
            double offsetHours = time1.GetLmtDateTimeOffset().Offset.TotalHours;
            double utime = new TimeSpan(hour, minute, 0).TotalHours + ((5 + (double)(4.00 / 60.00)) - offsetHours);

            //The result represents the interval from the epoch to the birth date.
            double interval = epochDays + (double)(utime / 24.00);
            interval = Math.Round(interval, 3);//round to 3 places decimal

            return interval;
        }

        /// <summary>
        /// Gets the planets motion name, can be Retrograde, Direct, Stationary
        /// a name version of Chesta Bala
        /// </summary>
        public static PlanetMotion PlanetMotionName(PlanetName planetName, Time time)
        {
            return PlanetMotion.Direct; //RETURN DUMMY DATA

            ////sun, moon, rahu & ketu don' have retrograde so always direct
            //if (planetName == Library.PlanetName.Sun || planetName == Library.PlanetName.Moon || planetName == Library.PlanetName.Rahu || planetName == Library.PlanetName.Ketu) { return PlanetMotion.Direct; }

            ////get chestaBala
            //var chestaBala = Calculate.PlanetChestaBala(planetName, time).ToDouble();

            ////based on chesta bala assign name to it
            ////Chesta kendra = 180 degrees = Retrograde
            //switch (chestaBala)
            //{
            //    case <= 60 and > 45: return PlanetMotion.Retrograde;
            //    case <= 45 and > 15: return PlanetMotion.Direct;
            //    case <= 15 and >= 0: return PlanetMotion.Stationary;
            //    default:
            //        throw new Exception($"Error in GetPlanetMotionName : {chestaBala}");
            //}

            throw new NotImplementedException();
        }

        ///// <summary>
        ///// </summary>
        //public static PlanetMotion PlanetMotionName2(PlanetName planetName, Time time)
        //{
        //    //Brihat Parashara Hora Shatra > Ch.27 Shl.21-23
        //    //
        //    //Motions of Grahas (Mangal to Shani): Eight kinds of motions are attributed to grahas.
        //    //These are Vakra (retrogression),
        //    //Anuvakr (entering the previous rashi in retrograde motion),
        //    //Vikal (devoid of motion or in stationary position),
        //    //Mand (somewhat slower motion than usual),
        //    //Mandatar (slower than the previous mentioned motion),
        //    //Sama (somewhat increasing in motion as against Mand),
        //    //Chara (faster than Sama) and
        //    //Atichara (entering next rashi in accelerated motion).
        //    //The strengths allotted due to such 8 motions are: 60, 30, 15, 30, 15, 7.5, 45, and 30.

        //    //There is an easy method to find out Gati or speed of Mars, Jupiter & Saturn which are beyond Earth with respect to Sun (outer planets).
        //    // Whenever these planets are transmitting 2nd or 1st or 12th sign from Sun, these planets will be in - Atichara
        //    // In 3rd - Sama
        //    // In 4th - Chara
        //    // In 5th - Manda & Mandatara
        //    // In 6th - Vikala
        //    // In 7th & 8th - Vakra
        //    // In 9th - Vikala & Forward (Manda)
        //    // In 10th - Sama
        //    // In 11 th -Chara
        //    // (Source - Bhaavartha Ratnakara-Last Chapters-Translated by B. V. Raman ji)
        //    //

        //    //Ancient Siddhaanta and Phalit classics mention eight types of speeds (Gati) of planets. All these eight types apply to Pancha-taaraa planets only : Mercury, Venus, Mars, Jupiter and Saturn. Rahu and Ketu are always retrograde. Sun and Moon are never retrograde.
        //    // 
        //    // The eight types of speeds are as follows :-
        //    // 
        //    // Vakra (Faster Retrograde)
        //    // Anuvakra (Slower Retrograde)
        //    // Kutila (complicated and very slow retrograde, sometimes relapsing into non-retro)
        //    // Mandatara (slowest forward motion)
        //    // Manda (slow forward motion)
        //    // Sama (normal forward motion)
        //    // Sheeghra (fast forward)
        //    // Sheeghratara (very fast forward)
        //    // These eight speeds according to their numbers are shown in the picture below, which is GEOCENTRIC epicycloidal motion of any Pancha-taara planet. In heliocentric model, there is no such differentiation and speed is always "sama". In Geocentric system too, speed of Sun or Moon is always "sama".

        //    //sun, moon, rahu & ketu don't have retrograde so always direct
        //    if (planetName == Library.PlanetName.Sun || planetName == Library.PlanetName.Moon || planetName == Library.PlanetName.Rahu || planetName == Library.PlanetName.Ketu) { return PlanetMotion.Direct; }

        //    //get chestaBala
        //    var chestaBala = Calculate.PlanetChestaBala(planetName, time).ToDouble();

        //    //based on chesta bala assign name to it
        //    //Chesta kendra = 180 degrees = Retrograde
        //    switch (chestaBala)
        //    {
        //        case <= 60 and > 30: return PlanetMotion.Retrograde;
        //        case <= 30 and > 15: return PlanetMotion.Direct;
        //        default:
        //            throw new Exception($"Error in GetPlanetMotionName : {chestaBala}");
        //    }

        //}


        /// <summary>
        /// A retrograde planet moves in the reverse direction and, instead of
        /// increasing, its longitude decreases as the time elapses. Rahu and Ketu often
        /// move in retrograde direction only. Other planets, except the Sun and the
        /// Moon, are subject to retrogression from time to time.
        /// </summary>
        public static bool IsPlanetRetrograde(PlanetName planetName, Time time)
        {
            bool retro = false;
            //if planet is Sun or Moon than default retrograde is off
            if (planetName.Name == PlanetNameEnum.Sun || planetName.Name == PlanetNameEnum.Moon) { return false; }

            //if planet is Rahu or Ketu than default retrograde is always on
            if (planetName.Name == PlanetNameEnum.Rahu || planetName.Name == PlanetNameEnum.Ketu) { return false; } //RahuKetu never go retro. Thier retro is direct.

            //get longitude of planet at given time
            var checkTimeLong = PlanetNirayanaLongitude(planetName, time);

            //get longitude of planet next day
            var nextDay = time.AddHours(24);
            var nextDayLong = PlanetNirayanaLongitude(planetName, nextDay);

            var dayplus2 = time.AddHours(48);
            var dayplus2Long = PlanetNirayanaLongitude(planetName, dayplus2);

            var dayplus3 = time.AddHours(72);
            var dayplus3Long = PlanetNirayanaLongitude(planetName, dayplus3);
            //Console.WriteLine("Long: {0} {1} {2} {3}", dayplus3Long.TotalDegrees, dayplus2Long.TotalDegrees, nextDayLong.TotalDegrees, checkTimeLong.TotalDegrees);

            if (nextDayLong <= checkTimeLong)
            {
                //check if the next day long is less than checktimelong because its crossing over 0.00 - this is not a retro condition
                if ((checkTimeLong.TotalDegrees >= 355.00 && checkTimeLong.TotalDegrees <= 360.00) && (nextDayLong.TotalDegrees >= 0.00 && nextDayLong.TotalDegrees <= 5.00))
                {
                    retro = false;
                }
                else
                {
                    retro = true;
                }
            }
            if (nextDayLong >= checkTimeLong)
            {
                //check if the next day long is more than checktimelong because its reverse crossing over 0.00 - this is a retro condition
                if ((checkTimeLong.TotalDegrees >= 0.00 && checkTimeLong.TotalDegrees <= 5.00) && (nextDayLong.TotalDegrees >= 355.00 && nextDayLong.TotalDegrees <= 0.00))
                {
                    retro = true;

                }
                else
                {
                    retro = false;

                }
            }
            return retro;
        }


        /// <summary>
        /// Determines if a given planet is combust at a specific time.
        /// Combustion of planets: Planets when too close to the Sun become
        /// invisible and are labelled as combust. A combust planet loses its strength
        /// and tends to behave adversely according to predictive astrology. Aryabhata
        /// has the following to say about combustion:
        /// ‘When the Moon has no latitude (i.e., when it is at zero degree of
        /// latitude) it is visible when situated at a distance of 12 degrees from the Sun.
        /// Venus is visible when 9 degrees distant from the Sun. The other planets
        /// taken in the order of decreasing sizes (viz., Jupiter, Mercury, Saturn, and
        /// Mars) are visible when they are 9 degrees increased by twos (i.e., when they
        /// are 11, 13, 15, and 17 degrees) distant from the Sun.’
        /// The degrees as mentioned above are generally taken as the limits within
        /// which the respective planets are said to be combust.
        /// </summary>
        /// <param name="planetName">The planet to check for combustion.</param>
        /// <param name="time">The time at which to check combustion.</param>
        /// <returns>True if the planet is combust at the given time; otherwise, false.</returns>
        public static bool IsPlanetCombust(PlanetName planetName, Time time)
        {

            // Check if the planet is eligible for combustion
            if (!IsPlanetCombustable(planetName))
            {
                // Planets that cannot become combust (Sun, Moon, Rahu, Ketu) return false
                return false;
            }

            // Get the sidereal (nirayana) longitude of the planet and the Sun at the given time
            double planetLongitude = Calculate.PlanetNirayanaLongitude(planetName, time).TotalDegrees;
            double sunLongitude = Calculate.PlanetNirayanaLongitude(PlanetName.Sun, time).TotalDegrees;

            // Calculate the absolute angular difference between the planet and the Sun (0 to 360 degrees)
            double angularDifference = Math.Abs(planetLongitude - sunLongitude) % 360;

            // Adjust the angular difference to be within 0 to 180 degrees
            if (angularDifference > 180)
            {
                angularDifference = 360 - angularDifference;
            }

            // Get the combustion limit for the planet
            double combustionLimit = GetPlanetCombustionLimit(planetName);

            // The planet is combust if the angular difference is less than or equal to the combustion limit
            return angularDifference <= combustionLimit;

            //--------------LOCALS---------
            // Local function to check if a planet is eligible for combustion
            bool IsPlanetCombustable(PlanetName pName)
            {
                if (pName == PlanetName.Mars || pName == PlanetName.Mercury || pName == PlanetName.Jupiter || pName == PlanetName.Venus || pName == PlanetName.Saturn)
                { return true; }

                return false;
            }

            // Local function to get the combustion limit for a planet
            double GetPlanetCombustionLimit(PlanetName pName)
            {
                if (pName == PlanetName.Venus)
                {
                    return 9;
                }
                if (pName == PlanetName.Jupiter)
                {
                    return 11;
                }
                if (pName == PlanetName.Mercury)
                {
                    return 13;
                }
                if (pName == PlanetName.Saturn)
                {
                    return 15;
                }
                if (pName == PlanetName.Mars)
                {
                    return 17;
                }
                return 0;
            }

        }

        /// <summary>
        /// circulation time of the objects in years, used by cheshta bala calculation
        /// </summary>
        public static double PlanetCirculationTime(PlanetName planetName)
        {

            if (planetName == Library.PlanetName.Sun) { return 1.0; }
            if (planetName == Library.PlanetName.Moon) { return .082; }
            if (planetName == Library.PlanetName.Mars) { return 1.88; }
            if (planetName == Library.PlanetName.Mercury) { return .24; }
            if (planetName == Library.PlanetName.Jupiter) { return 11.86; }
            if (planetName == Library.PlanetName.Venus) { return .62; }
            if (planetName == Library.PlanetName.Saturn) { return 29.46; }

            throw new Exception("Planet circulation time not defined!");

        }

        /// <summary>
        /// Sapthavargajabala: This is the strength of a
        /// planet due to its residence in the seven sub-divisions
        /// according to its relation with the dispositor.
        ///
        /// Saptavargaja bala means the strength a
        /// planet gets by virtue of its disposition in a friendly,
        /// neutral or inimical Rasi, Hora, Drekkana, Sapthamsa,
        /// Navamsa, Dwadasamsa and Thrimsamsa.
        /// </summary>
        public static Shashtiamsa PlanetSaptavargajaBala(PlanetName planetName, Time time)
        {
            //CACHE MECHANISM
            return CacheManager.GetCache(new CacheKey(nameof(PlanetSaptavargajaBala), planetName, time, Ayanamsa), _getPlanetSaptavargajaBala);


            //UNDERLYING FUNCTION
            Shashtiamsa _getPlanetSaptavargajaBala()
            {
                //declare total value
                double totalSaptavargajaBalaInShashtiamsa = 0;

                //declare empty list of planet sign relationships
                var planetSignRelationshipList = new List<PlanetToSignRelationship>();


                //get planet rasi Moolatrikona.
                var planetInMoolatrikona = Calculate.IsPlanetInMoolatrikona(planetName, time);

                //if planet is in moolatrikona
                if (planetInMoolatrikona)
                {
                    //add the relationship to the list
                    planetSignRelationshipList.Add(PlanetToSignRelationship.Moolatrikona);
                }
                else
                //else get planet's normal relationship with rasi
                {
                    //get planet rasi
                    var planetRasi = Calculate.PlanetRasiD1Sign(planetName, time).GetSignName();
                    var rasiSignRelationship = Calculate.PlanetRelationshipWithSign(planetName, planetRasi, time);

                    //add planet rasi relationship to list
                    planetSignRelationshipList.Add(rasiSignRelationship);
                }

                //get planet hora (D2)
                var planetHora = Calculate.PlanetHoraD2Signs(planetName, time).GetSignName();
                var horaSignRelationship = Calculate.PlanetRelationshipWithSign(planetName, planetHora, time);
                //add planet hora relationship to list
                planetSignRelationshipList.Add(horaSignRelationship);


                //get planet drekkana (D3)
                var planetDrekkana = Calculate.PlanetDrekkanaD3Sign(planetName, time).GetSignName();
                var drekkanaSignRelationship = Calculate.PlanetRelationshipWithSign(planetName, planetDrekkana, time);
                //add planet drekkana relationship to list
                planetSignRelationshipList.Add(drekkanaSignRelationship);


                //get planet saptamsa (D7)
                var planetSaptamsa = Calculate.PlanetSaptamshaD7Sign(planetName, time).GetSignName();
                var saptamsaSignRelationship = Calculate.PlanetRelationshipWithSign(planetName, planetSaptamsa, time);
                //add planet saptamsa relationship to list
                planetSignRelationshipList.Add(saptamsaSignRelationship);


                //get planet navamsa (D9)
                var planetNavamsa = Calculate.PlanetNavamshaD9Sign(planetName, time).GetSignName();
                var navamsaSignRelationship = Calculate.PlanetRelationshipWithSign(planetName, planetNavamsa, time);
                //add planet navamsa relationship to list
                planetSignRelationshipList.Add(navamsaSignRelationship);


                //get planet dwadasamsa (D12)
                var planetDwadasamsa = Calculate.PlanetDwadashamshaD12Sign(planetName, time).GetSignName();
                var dwadasamsaSignRelationship = Calculate.PlanetRelationshipWithSign(planetName, planetDwadasamsa, time);
                //add planet dwadasamsa relationship to list
                planetSignRelationshipList.Add(dwadasamsaSignRelationship);


                //get planet thrimsamsa (D30)
                var planetThrimsamsa = Calculate.PlanetTrimshamshaD30Sign(planetName, time).GetSignName();
                var thrimsamsaSignRelationship = Calculate.PlanetRelationshipWithSign(planetName, planetThrimsamsa, time);
                //add planet thrimsamsa relationship to list
                planetSignRelationshipList.Add(thrimsamsaSignRelationship);


                //calculate total Saptavargaja Bala

                //loop through all the relationship
                foreach (var planetToSignRelationship in planetSignRelationshipList)
                {
                    //add relationship point accordingly

                    //A planet in its Moolatrikona is assigned a value of 45 Shashtiamsas;
                    if (planetToSignRelationship == PlanetToSignRelationship.Moolatrikona)
                    {
                        totalSaptavargajaBalaInShashtiamsa = totalSaptavargajaBalaInShashtiamsa + 45;
                    }

                    //in Swavarga 30 Shashtiamsas;
                    if (planetToSignRelationship == PlanetToSignRelationship.OwnVarga)
                    {
                        totalSaptavargajaBalaInShashtiamsa = totalSaptavargajaBalaInShashtiamsa + 30;
                    }

                    //in Adhi Mitravarga 22.5 Shashtiamsas;
                    if (planetToSignRelationship == PlanetToSignRelationship.BestFriendVarga)
                    {
                        totalSaptavargajaBalaInShashtiamsa = totalSaptavargajaBalaInShashtiamsa + 22.5;
                    }

                    //in Mitravarga 15 · Shashtiamsas;
                    if (planetToSignRelationship == PlanetToSignRelationship.FriendVarga)
                    {
                        totalSaptavargajaBalaInShashtiamsa = totalSaptavargajaBalaInShashtiamsa + 15;
                    }

                    //in Samavarga 7.5 Shashtiamsas ~
                    if (planetToSignRelationship == PlanetToSignRelationship.NeutralVarga)
                    {
                        totalSaptavargajaBalaInShashtiamsa = totalSaptavargajaBalaInShashtiamsa + 7.5;
                    }

                    //in Satruvarga 3.75 Shashtiamsas;
                    if (planetToSignRelationship == PlanetToSignRelationship.EnemyVarga)
                    {
                        totalSaptavargajaBalaInShashtiamsa = totalSaptavargajaBalaInShashtiamsa + 3.75;
                    }

                    //in Adhi Satruvarga 1.875 Shashtiamsas.
                    if (planetToSignRelationship == PlanetToSignRelationship.BitterEnemyVarga)
                    {
                        totalSaptavargajaBalaInShashtiamsa = totalSaptavargajaBalaInShashtiamsa + 1.875;
                    }

                }


                return new Shashtiamsa(totalSaptavargajaBalaInShashtiamsa);

            }

        }

        /// <summary>
        /// residence of the planet and as such a certain degree of strength or weakness attends on it
        /// 
        /// Positonal strength
        /// 
        /// A planet occupies a
        /// certain sign in a Rasi and friendly, neutrai or
        /// inimical varga~. It is either exalted or debilitated·
        /// lt ocupies its Moolathrikona or it has its own
        /// varga. All these states refer to the position or
        /// residence of the planet and as such a certain degree
        /// of strength or weakness attends on it. This strength
        /// or potency is known as the Sthanabala.
        /// 
        /// 
        /// 1.Uccha Bala:
        /// Uccha means exaltation. When a planet is placed in its highest exaltation point,
        /// it is of full strength and when it is in its deepest debilitation point, it is devoid of any strength.
        /// When in between the strength is calculated proportionately dependent on the distance these planets are
        /// placed from the highest exaltation or deepest debilitation point.
        ///
        /// 2.Sapta Vargiya Bala:
        /// Rashi, Hora, Drekkana, Saptamsha, Navamsha, Dwadasamsha and Trimsamsha constitute the Sapta Varga.
        /// The strength of the planets in these seven divisional charts based on their placements in Mulatrikona,
        /// own sign, friendly sign etc. constitute the Sapta vargiya bala.
        /// 
        /// 3.Oja-Yugma Rashi-Amsha Bala:
        /// Oja means odd signs and Yugma means even signs. Thus, as the name imply, this strength is derived from
        /// a planet’s placement in the odd or even signs in the Rashi and Navamsha.
        /// 
        /// 4.Kendradi Bala:
        /// The name itself implies how to compute this strength. A planet in a Kendra (1-4-7-10) gets full strength,
        /// while one in Panapara (2-5-8-11) gets half and the one in Apoklimas (12-3-6-9) gets quarter strength.
        ///
        /// 5.Drekkana Bala:
        /// Due to placement in first, second, or third Drekkana of a sign, male, female and hermaphrodite planets respectively,
        /// get a quarter strength according to placements in the first, second and third Drekkana.
        /// </summary>
        public static Shashtiamsa PlanetSthanaBala(PlanetName planetName, Time time)
        {
            //no calculation for rahu and ketu here
            var isRahu = planetName.Name == Library.PlanetName.PlanetNameEnum.Rahu;
            var isKetu = planetName.Name == Library.PlanetName.PlanetNameEnum.Ketu;
            var isRahuKetu = isRahu || isKetu;
            if (isRahuKetu) { return Shashtiamsa.Zero; }

            //CACHE MECHANISM
            return CacheManager.GetCache(new CacheKey(nameof(PlanetSthanaBala), planetName, time, Ayanamsa), _getPlanetSthanaBala);


            //UNDERLYING FUNCTION
            Shashtiamsa _getPlanetSthanaBala()
            {
                //Get Ochcha Bala (exaltation strength)
                var ochchaBala = PlanetOchchaBala(planetName, time);

                //Get Saptavargaja Bala
                var saptavargajaBala = PlanetSaptavargajaBala(planetName, time);

                //Get Ojayugmarasyamsa Bala
                var ojayugmarasymsaBala = PlanetOjayugmarasyamsaBala(planetName, time);

                //Get Kendra Bala
                var kendraBala = PlanetKendraBala(planetName, time);

                //Drekkana Bala
                var drekkanaBala = PlanetDrekkanaBala(planetName, time);

                //Total Sthana Bala
                var totalSthanaBala = ochchaBala + saptavargajaBala + ojayugmarasymsaBala + kendraBala + drekkanaBala;

                return totalSthanaBala;

            }

        }

        /// <summary>
        /// Drekkanabala: The Sun, Jupiter and Mars
        /// in the lst ; Saturn and Mercury in the 2nd ; and
        /// the Moon and Venus in the last Drekkana, get full
        /// strength of 60 shashtiamsas.
        /// </summary>
        public static Shashtiamsa PlanetDrekkanaBala(PlanetName planetName, Time time)
        {
            //get sign planet is in
            var planetSign = Calculate.PlanetRasiD1Sign(planetName, time);

            //get degrees in sign 
            var degreesInSign = planetSign.GetDegreesInSign().TotalDegrees;

            //if male planet -Ravi, Guru and Kuja.
            if (planetName == Library.PlanetName.Sun || planetName == Library.PlanetName.Jupiter || planetName == Library.PlanetName.Mars)
            {
                //if planet is in 1st drekkana
                if (degreesInSign >= 0 && degreesInSign <= 10.0)
                {
                    //return 15 bala
                    return new Shashtiamsa(15);
                }

            }

            //if Hermaphrodite Planets.-Sani and Budba
            if (planetName == Library.PlanetName.Saturn || planetName == Library.PlanetName.Mercury)
            {
                //if planet is in 2nd drekkana
                if (degreesInSign > 10 && degreesInSign <= 20.0)
                {
                    //return 15 bala
                    return new Shashtiamsa(15);
                }

            }

            //if Female Planets.-Chandra and Sukra
            if (planetName == Library.PlanetName.Moon || planetName == Library.PlanetName.Venus)
            {
                //if planet is in 3rd drekkana
                if (degreesInSign > 20 && degreesInSign <= 30.0)
                {
                    //return 15 bala
                    return new Shashtiamsa(15);
                }
            }

            //if none above conditions met return 0 bala
            return new Shashtiamsa(0);
        }

        /// <summary>
        /// Kendrtzbala: Planets in Kendras get 60
        /// shashtiamsas; in Panapara 30, and in Apoklima 15.
        /// </summary>
        public static Shashtiamsa PlanetKendraBala(PlanetName planetName, Time time)
        {
            //get number of the sign planet is in
            var planetSignNumber = (int)Calculate.PlanetRasiD1Sign(planetName, time).GetSignName();

            //A planet in a kendra sign  gets 60 Shashtiamsas as its strength ;
            //Quadrants.-Kendras-1 (Ar, 4, 7 and 10.
            if (planetSignNumber == 1 || planetSignNumber == 4 || planetSignNumber == 7 || planetSignNumber == 10)
            {
                return new Shashtiamsa(60);
            }

            //in a Panapara sign 30 Shashtiamsas;
            //-Panaparas-2, 5, 8 and 11.
            if (planetSignNumber == 2 || planetSignNumber == 5 || planetSignNumber == 8 || planetSignNumber == 11)
            {
                return new Shashtiamsa(30);
            }


            //and in an Apoklima sign 15 Shashtiamsas.
            //Apoklimas-3, 6, 9 and 12 {9th being a trikona must be omitted).
            if (planetSignNumber == 3 || planetSignNumber == 6 || planetSignNumber == 9 || planetSignNumber == 12)
            {
                return new Shashtiamsa(15);
            }


            throw new Exception("Kendra Bala not found, error");
        }

        /// <summary>
        /// Ojayugmarasyamsa: In odd Rasi and Navamsa,
        /// the Sun, Mars, Jupiter, Mercury and Saturn
        /// get strength and the rest in even signs
        /// </summary>
        public static Shashtiamsa PlanetOjayugmarasyamsaBala(PlanetName planetName, Time time)
        {
            //get planet rasi sign
            var planetRasiSign = Calculate.PlanetRasiD1Sign(planetName, time).GetSignName();

            //get planet navamsa sign
            var planetNavamsaSign = Calculate.PlanetNavamshaD9Sign(planetName, time).GetSignName();

            //declare total Ojayugmarasyamsa Bala as 0 first
            double totalOjayugmarasyamsaBalaInShashtiamsas = 0;

            //if planet is the moon or venus
            if (planetName == Library.PlanetName.Moon || planetName == Library.PlanetName.Venus)
            {
                //if rasi sign is an even sign
                if (Calculate.IsEvenSign(planetRasiSign))
                {
                    //add 15 Shashtiamsas
                    totalOjayugmarasyamsaBalaInShashtiamsas += 15;
                }

                //if navamsa sign is an even sign
                if (Calculate.IsEvenSign(planetNavamsaSign))
                {
                    //add 15 Shashtiamsas
                    totalOjayugmarasyamsaBalaInShashtiamsas += 15;
                }

            }
            //if planet is Sun, Mars, Jupiter, Mercury and Saturn
            else if (planetName == Library.PlanetName.Sun || planetName == Library.PlanetName.Mars ||
                     planetName == Library.PlanetName.Jupiter || planetName == Library.PlanetName.Mercury || planetName == Library.PlanetName.Saturn)
            {
                //if rasi sign is an odd sign
                if (Calculate.IsOddSign(planetRasiSign))
                {
                    //add 15 Shashtiamsas
                    totalOjayugmarasyamsaBalaInShashtiamsas += 15;
                }

                //if navamsa sign is an odd sign
                if (Calculate.IsOddSign(planetNavamsaSign))
                {
                    //add 15 Shashtiamsas
                    totalOjayugmarasyamsaBalaInShashtiamsas += 15;
                }

            }

            return new Shashtiamsa(totalOjayugmarasyamsaBalaInShashtiamsas);
        }

        /// <summary>
        /// Gets a planet's Kala Bala or Temporal strength
        /// </summary>
        public static Shashtiamsa PlanetKalaBala(PlanetName planetName, Time time)
        {
            //no calculation for rahu and ketu here
            var isRahu = planetName.Name == Library.PlanetName.PlanetNameEnum.Rahu;
            var isKetu = planetName.Name == Library.PlanetName.PlanetNameEnum.Ketu;
            var isRahuKetu = isRahu || isKetu;
            if (isRahuKetu) { return Shashtiamsa.Zero; }



            //CACHE MECHANISM
            return CacheManager.GetCache(new CacheKey(nameof(PlanetKalaBala), planetName, time, Ayanamsa), _getPlanetKalaBala);


            //UNDERLYING FUNCTION
            Shashtiamsa _getPlanetKalaBala()
            {
                //place to store pre kala bala values
                var kalaBalaList = new Dictionary<PlanetName, Shashtiamsa>();

                //Yuddha Bala requires all planet's pre kala bala
                //so calculate pre kala bala for all planets first
                foreach (var planet in Library.PlanetName.All7Planets)
                {
                    //calculate pre kala bala
                    var preKalaBala = GetPreKalaBala(planet, time);

                    //store in a list sorted by planet name, to be accessed later
                    kalaBalaList.Add(planet, preKalaBala);
                }

                //calculate Yuddha Bala
                var yuddhaBala = PlanetYuddhaBala(planetName, kalaBalaList, time);

                //Total Kala Bala
                var total = kalaBalaList[planetName] + yuddhaBala;

                return total;

                //---------------FUNCTIONS--------------
                Shashtiamsa GetPreKalaBala(PlanetName planetName, Time time)
                {
                    //Nathonnatha Bala
                    var nathonnathaBala = PlanetNathonnathaBala(planetName, time);

                    //Paksha Bala
                    var pakshaBala = PlanetPakshaBala(planetName, time);

                    //Tribhaga Bala
                    var tribhagaBala = PlanetTribhagaBala(planetName, time);

                    //Abda Bala
                    var abdaBala = PlanetAbdaBala(planetName, time);

                    //Masa Bala
                    var masaBala = PlanetMasaBala(planetName, time);

                    //Vara Bala
                    var varaBala = PlanetVaraBala(planetName, time);

                    //Hora Bala
                    var horaBala = PlanetHoraBala(planetName, time);

                    //Ayana Bala
                    var ayanaBala = PlanetAyanaBala(planetName, time);

                    //combine all the kala bala calculated so far, and return the value
                    var preKalaBala = nathonnathaBala + pakshaBala + tribhagaBala + abdaBala + masaBala + varaBala + horaBala +
                                      ayanaBala;

                    return preKalaBala;
                }
            }

        }

        /// <summary>
        /// Two planets are said to be in Yuddha or fight when they are in
        /// conjunction and the distance between them is less than one degree.
        /// TODO Not fully tested
        ///
        /// Yuddhabala : All planets excepting the Sun
        /// and the Moon enter into war when two planets are
        /// in the same degree. The pJanet having the lesser
        /// longitude is the winner. Find out the sum total of
        /// the SthanabaJa, Kalabala and Digbala of these two'
        /// planets. Difference between the two, divided by
        /// the difference of their diameters of its disc, gives
        /// the Yuddhabala. Add this to the victorious planet
        /// and dedu_ct it from the vanquished.
        /// </summary>
        public static Shashtiamsa PlanetYuddhaBala(PlanetName inputedPlanet, Dictionary<PlanetName, Shashtiamsa> preKalaBalaValues, Time time)
        {
            //All the planets excepting Sun and Moon may enter into war (Yuddha)
            if (inputedPlanet == Library.PlanetName.Moon || inputedPlanet == Library.PlanetName.Sun) { return Shashtiamsa.Zero; }


            //place to store winner & loser balas
            var yudhdhabala = new Dictionary<PlanetName, Shashtiamsa>();


            //get all planets that are conjunct with inputed planet
            var conjunctPlanetList = Calculate.PlanetsInConjunction(inputedPlanet, time);

            //remove rahu & kethu if present, they are not included in Yuddha Bala calculations
            conjunctPlanetList.RemoveAll(pl => pl == Library.PlanetName.Rahu || pl == Library.PlanetName.Ketu);


            foreach (var checkingPlanet in conjunctPlanetList)
            {

                //All the planets excepting Sun and Moon may enter into war (Yuddha)
                //no need to calculate Yuddha, move to next planet, if sun or moon
                if (checkingPlanet == Library.PlanetName.Moon || checkingPlanet == Library.PlanetName.Sun) { continue; }


                //get distance between conjunct planet & inputed planet
                var inputedPlanetLong = Calculate.PlanetNirayanaLongitude(inputedPlanet, time);
                var checkingPlanetLong = Calculate.PlanetNirayanaLongitude(checkingPlanet, time);
                var distance = Calculate.DistanceBetweenPlanets(inputedPlanetLong, checkingPlanetLong);


                //if the distance between them is less than one degree
                if (distance < Angle.FromDegrees(1))
                {
                    PlanetName winnerPlanet = null;
                    PlanetName losserPlanet = null;

                    //The conquering planet is the one whose longitude is less.
                    if (inputedPlanetLong < checkingPlanetLong) { winnerPlanet = inputedPlanet; losserPlanet = checkingPlanet; } //inputed planet won
                    else if (inputedPlanetLong > checkingPlanetLong) { winnerPlanet = checkingPlanet; losserPlanet = inputedPlanet; } //checking planet won
                    else if (inputedPlanetLong == checkingPlanetLong)
                    {
                        //unlikely chance, log error & set inputed planet as winner (random)
                        LogManager.Error($"Planets same longitude! Not expected, random result used!");
                        winnerPlanet = inputedPlanet; losserPlanet = checkingPlanet;
                    }

                    //When two planets are in war, get the sum of the various Balas, viv., Sthanabala, the
                    // Dikbala and the Kalabala (up to Horabala) described hitherto of the fighting planets. Find out the
                    // difference between these two sums.
                    var shadbaladiff = Math.Abs(preKalaBalaValues[inputedPlanet].ToDouble() - preKalaBalaValues[checkingPlanet].ToDouble());


                    //Divide shadbala difference by the difference between the diameters of the discs of the two fighting planets
                    var diameterDifference = PlanetDiscDiameter(inputedPlanet).GetDifference(PlanetDiscDiameter(checkingPlanet));


                    //And the resulting quotient which is the Yuddhabala (Shashtiamsa) must be added to the total of the Kalabala (detailed
                    // hitherto) of the victorious planet and must be subtracted from the total Kalabala of the vanquished planet.
                    var shadbala = diameterDifference.TotalDegrees / shadbaladiff;

                    yudhdhabala[winnerPlanet] = new Shashtiamsa(shadbala);
                    yudhdhabala[losserPlanet] = new Shashtiamsa(-shadbala);

                }


            }


            //return yudhdhabala if it was calculated else, return 0 
            var found = yudhdhabala.TryGetValue(inputedPlanet, out var bala);
            return found ? bala : Shashtiamsa.Zero;




            //-----------FUNCTIONS----------------


        }

        /// <summary>
        /// Bimba Parimanas -This means the diameters of the discs of the planets.
        /// </summary>
        static Angle PlanetDiscDiameter(PlanetName planet)
        {
            if (planet == Library.PlanetName.Mars) { return new Angle(0, 9, 4); }
            if (planet == Library.PlanetName.Mercury) { return new Angle(0, 6, 6); }
            if (planet == Library.PlanetName.Jupiter) { return new Angle(0, 190, 4); }
            if (planet == Library.PlanetName.Venus) { return new Angle(0, 16, 6); }
            if (planet == Library.PlanetName.Saturn) { return new Angle(0, 158, 0); }

            //control should not come here, report error
            throw new Exception("Disc diameter now found!");
        }

        /// <summary>
        /// Ayanabala : All planets get 30 shasbtiamsas
        /// at the equator. For the Sun, Jupiter, Mars
        /// and Venus add proportionately when they are in
        /// northern course and for the Moon and Saturn when
        /// in southern course. Deduct proportionately when
        /// they are in the opposite direction. Unit of strength
        /// is 60 shashtiamsas.
        ///
        /// 
        /// TODO some values for calculation with standard hooscope out of whack,
        /// it seems small differences in longitude seem magnified at final value,
        /// not 100% sure, need further testing for confirmation, but final values seem ok so far
        /// </summary>
        public static Shashtiamsa PlanetAyanaBala(PlanetName planetName, Time time)
        {

            //CACHE MECHANISM
            return CacheManager.GetCache(new CacheKey(nameof(PlanetAyanaBala), planetName, time, Ayanamsa), _getPlanetAyanaBala);


            //UNDERLYING FUNCTION
            Shashtiamsa _getPlanetAyanaBala()
            {
                double ayanaBala = 0;

                //get plant kranti (negative south, positive north)
                var kranti = PlanetDeclination(planetName, time);

                //prepare values for calculation of ayanabala
                var x = Angle.FromDegrees(24);
                var isNorthDeclination = kranti < 0 ? false : true;

                //get declination without negative (absolute value), easier for calculation
                var absKranti = Math.Abs((double)kranti);

                //In case of Sukra, Ravi, Kuja and Guru their north declinations are
                //additive and south declinations are subtractive
                if (planetName == Library.PlanetName.Venus || planetName == Library.PlanetName.Sun || planetName == Library.PlanetName.Mars || planetName == Library.PlanetName.Jupiter)
                {
                    //additive
                    if (isNorthDeclination) { ayanaBala = ((24 + absKranti) / 48) * 60; }

                    //subtractive
                    else { ayanaBala = ((24 - absKranti) / 48) * 60; }

                    //And double the Ayanabala in the case of the Sun
                    if (planetName == Library.PlanetName.Sun) { ayanaBala = ayanaBala * 2; }

                }
                //In case of Sani and Chandra, their south declinations are additive while their
                //north declinations are subtractive.
                else if (planetName == Library.PlanetName.Saturn || planetName == Library.PlanetName.Moon)
                {
                    //additive
                    if (!isNorthDeclination) { ayanaBala = ((24 + absKranti) / 48) * 60; }

                    //subtractive
                    else { ayanaBala = ((24 - absKranti) / 48) * 60; }
                }
                //For Budha the declination, north or south, is always additive.
                else if (planetName == Library.PlanetName.Mercury)
                {
                    ayanaBala = ((24 + absKranti) / 48) * 60;
                }


                return new Shashtiamsa(ayanaBala);

            }


        }

        /// <summary>
        /// A heavenly body moves northwards the equator for sometime and
        /// then gets southwards. This angular distance from
        /// the equinoctial or celestial equator is Kranti or the
        /// declination.
        ///
        /// Declinations are reckoned plus or minus according as the planet
        /// is situated in the northern or southern celestial hemisphere
        /// </summary>
        public static double PlanetDeclination(PlanetName planetName, Time time)
        {

            //CACHE MECHANISM
            return CacheManager.GetCache(new CacheKey(nameof(PlanetDeclination), planetName, time, Ayanamsa), _getPlanetDeclination);


            //UNDERLYING FUNCTION
            double _getPlanetDeclination()
            {
                //for degree to radian conversion
                const double DEG2RAD = 0.0174532925199433;

                var eps = EclipticObliquity(time);

                var tlen = Calculate.PlanetSayanaLongitude(planetName, time);
                var lat = Calculate.PlanetSayanaLatitude(planetName, time);

                //if kranti (declination), is a negative number, it means south, else north of equator
                var kranti = lat.TotalDegrees + eps * Math.Sin(DEG2RAD * tlen.TotalDegrees);

                return kranti;
            }

        }

        /// <summary>
        /// true obliquity of the Ecliptic (includes nutation)
        /// </summary>
        public static double EclipticObliquity(Time time)
        {

            //CACHE MECHANISM
            return CacheManager.GetCache(new CacheKey(nameof(EclipticObliquity), time, Ayanamsa), _getPlanetEps);


            //UNDERLYING FUNCTION
            double _getPlanetEps()
            {
                double eps;

                string err = "";
                double[] x = new double[6];

                SwissEph ephemeris = new SwissEph();

                // Convert DOB to ET
                var jul_day_ET = Calculate.TimeToJulianEphemerisTime(time);

                //ephemeris.swe_calc(jul_day_ET, SwissEph.SE_ECL_NUT, 0, x, ref err);

                ephemeris.swe_calc(jul_day_ET, SwissEph.SE_ECL_NUT, 0, x, ref err);

                eps = x[0];

                return eps;
            }

        }

        /// <summary>
        /// Hora Bala AKA Horadhipathi Bala
        /// </summary>
        public static Shashtiamsa PlanetHoraBala(PlanetName planetName, Time time)
        {

            //CACHE MECHANISM
            return CacheManager.GetCache(new CacheKey(nameof(PlanetHoraBala), planetName, time, Ayanamsa), _getPlanetHoraBala);


            //UNDERLYING FUNCTION
            Shashtiamsa _getPlanetHoraBala()
            {
                //first ascertain the weekday of birth
                var birthWeekday = Calculate.DayOfWeek(time);

                //ascertain the number of hours elapsed from sunrise to birth
                //This shows the number of horas passed.
                var hora = Calculate.HoraAtBirth(time);

                //get lord of hora (hour)
                var lord = Calculate.LordOfHoraFromWeekday(hora, birthWeekday);

                //planet inputed is lord of hora, then 60 shashtiamsas
                if (lord == planetName)
                {
                    return new Shashtiamsa(60);
                }
                else
                {
                    return Shashtiamsa.Zero;
                }

            }



        }

        /// <summary>
        /// The planet who is the king of
        /// the year of birth is assigned a value of 15 Shashtiamsas
        /// as his Abdabala.
        /// </summary>
        public static Shashtiamsa PlanetAbdaBala(PlanetName planetName, Time time)
        {
            //CACHE MECHANISM
            return CacheManager.GetCache(new CacheKey(nameof(PlanetAbdaBala), planetName, time, Ayanamsa), _getPlanetAbdaBala);


            //UNDERLYING FUNCTION
            Shashtiamsa _getPlanetAbdaBala()
            {
                //calculate year lord
                dynamic yearAndMonthLord = YearAndMonthLord(time);
                PlanetName yearLord = yearAndMonthLord.YearLord;

                //if inputed planet is year lord than 15 Shashtiamsas
                if (yearLord == planetName) { return new Shashtiamsa(15); }

                //not year lord, 0 Shashtiamsas
                return Shashtiamsa.Zero;
            }


        }

        /// <summary>
        /// Gets a planet's masa bala
        /// the lord of the month of birth is assigned a value of 30 Shashtiamsas as his Masabala
        /// </summary>
        public static Shashtiamsa PlanetMasaBala(PlanetName planetName, Time time)
        {
            //The planet who is the lord of
            //the month of birth is assigned a value of 30 Shashtiamsas
            //as his Masabala.

            //calculate month lord
            dynamic yearAndMonthLord = YearAndMonthLord(time);
            PlanetName monthLord = yearAndMonthLord.MonthLord;

            //if inputed planet is month lord than 30 Shashtiamsas
            if (monthLord == planetName) { return new Shashtiamsa(30); }

            //not month lord, 0 Shashtiamsas
            return Shashtiamsa.Zero;
        }

        public static Shashtiamsa PlanetVaraBala(PlanetName planetName, Time time)
        {
            //The planet who is the lord of
            //the day of birth is assigned a value of 45 Shashtiamsas
            //as his Varabala.

            //calculate day lord
            PlanetName dayLord = Calculate.LordOfWeekday(time);

            //if inputed planet is day lord than 45 Shashtiamsas
            if (dayLord == planetName) { return new Shashtiamsa(45); }

            //not day lord, 0 Shashtiamsas
            return Shashtiamsa.Zero;

        }

        /// <summary>
        /// Gets year & month lord at inputed time
        /// </summary>
        public static object YearAndMonthLord(Time time)
        {
            //CACHE MECHANISM
            return CacheManager.GetCache(new CacheKey(nameof(YearAndMonthLord), time, Ayanamsa), _getYearAndMonthLord);


            //UNDERLYING FUNCTION
            object _getYearAndMonthLord()
            {
                //set default
                var yearLord = Library.PlanetName.Sun;
                var monthLord = Library.PlanetName.Sun;

                //initialize ephemeris
                using SwissEph ephemeris = new SwissEph();

                double ut_arghana = ephemeris.swe_julday(1827, 5, 2, 0, SwissEph.SE_GREG_CAL);
                double ut_noon = Calculate.GreenwichLmtInJulianDays(time);

                double diff = ut_noon - ut_arghana;
                if (diff >= 0)
                {
                    double quo = Math.Floor(diff / 360.0);
                    diff -= quo * 360.0;
                }
                else
                {
                    double pdiff = -diff;
                    double quo = Math.Ceiling(pdiff / 360.0);
                    diff += quo * 360.0;
                }

                double diff_year = diff;
                while (diff > 30.0) diff -= 30.0;
                double diff_month = diff;
                while (diff > 7) diff -= 7.0;

                var yearLordRaw = ephemeris.swe_day_of_week(ut_noon - diff_year);
                var monthLordRaw = ephemeris.swe_day_of_week(ut_noon - diff_month);

                //parse raw weekday
                var yearWeekday = swissEphWeekDayToMuhurthaDay(yearLordRaw);
                var monthWeekday = swissEphWeekDayToMuhurthaDay(monthLordRaw);


                //Abdadbipat : the planet that rules over the weekday on which the year begins (hindu year)
                yearLord = Calculate.LordOfWeekday(yearWeekday);

                //Masadhipath : The planet that rules the weekday of the commencement of the month of the birth
                monthLord = Calculate.LordOfWeekday(monthWeekday);

                //package year & month lord together & return
                return new { YearLord = yearLord, MonthLord = monthLord };


                //---------------------FUNCTION--------------------

                //converts swiss epehmeris weekday numbering to muhurtha weekday numbering
                DayOfWeek swissEphWeekDayToMuhurthaDay(int dayNumber)
                {
                    switch (dayNumber)
                    {
                        case 0: return Library.DayOfWeek.Monday;
                        case 1: return Library.DayOfWeek.Tuesday;
                        case 2: return Library.DayOfWeek.Wednesday;
                        case 3: return Library.DayOfWeek.Thursday;
                        case 4: return Library.DayOfWeek.Friday;
                        case 5: return Library.DayOfWeek.Saturday;
                        case 6: return Library.DayOfWeek.Sunday;
                        default: throw new Exception("Invalid day number!");
                    }
                }

            }

        }

        /// <summary>
        /// Thribhagabala : Mercury, the Sun and
        /// Saturn get 60 shashtiamsas each, during the lst,
        /// 2nd and 3rd one-third positions of the day, respectively.
        /// The Moon, Venus and Mars govern the lst, 2nd and 3rd one-third portion of the night
        /// respectively. Jupiter is always strong and gets 60
        /// shashtiamsas of strength.
        /// </summary>
        public static Shashtiamsa PlanetTribhagaBala(PlanetName planetName, Time time)
        {
            PlanetName ret = Library.PlanetName.Jupiter;

            var sunsetTime = Calculate.SunsetTime(time);

            if (IsDayBirth(time))
            {
                //find out which part of the day birth took place

                var sunriseTime = Calculate.SunriseTime(time);

                //substraction should always return a positive number, since sunset is always after sunrise
                double lengthHours = (sunsetTime.Subtract(sunriseTime).TotalHours) / 3;
                double offset = time.Subtract(sunriseTime).TotalHours;
                int part = (int)(Math.Floor(offset / lengthHours));
                switch (part)
                {
                    case 0: ret = Library.PlanetName.Mercury; break;
                    case 1: ret = Library.PlanetName.Sun; break;
                    case 2: ret = Library.PlanetName.Saturn; break;
                }
            }
            else
            {
                //get sunrise time at on next day to get duration of the night
                var nextDayTime = time.AddHours(24);
                var nextDaySunrise = Calculate.SunriseTime(nextDayTime);

                double lengthHours = (nextDaySunrise.Subtract(sunsetTime).TotalHours) / 3;
                double offset = time.Subtract(sunsetTime).TotalHours;
                int part = (int)(Math.Floor(offset / lengthHours));
                switch (part)
                {
                    case 0: ret = Library.PlanetName.Moon; break;
                    case 1: ret = Library.PlanetName.Venus; break;
                    case 2: ret = Library.PlanetName.Mars; break;
                }
            }

            //Always assign a value of 60 Shashtiamsas
            //to Guru irrespective of whether birth is during
            //night or during day.
            if (planetName == Library.PlanetName.Jupiter || planetName == ret) { return new Shashtiamsa(60); }

            return new Shashtiamsa(0);
        }

        /// <summary>
        /// Oochchabala : The distance between the
        /// planet's longitude and its debilitation point, divided
        /// by 3, gives its exaltation strength or oochchabaJa.
        /// </summary>
        public static Shashtiamsa PlanetOchchaBala(PlanetName planetName, Time time)
        {
            //1.0 Get Planet longitude
            var planetLongitude = Calculate.PlanetNirayanaLongitude(planetName, time);

            //2.0 Get planet debilitation point
            var planetDebilitationPoint = Calculate.PlanetDebilitationPoint(planetName);
            //convert to planet longitude
            var debilitationLongitude = LongitudeAtZodiacSign(planetDebilitationPoint);

            //3.0 Get difference between planet longitude & debilitation point
            //var difference = planetLongitude.GetDifference(planetDebilitationPoint); //todo need checking
            var difference = DistanceBetweenPlanets(planetLongitude, debilitationLongitude);

            //4.0 If difference is more than 180 degrees
            if (difference.TotalDegrees > 180)
            {
                //get the difference of it with 360 degrees
                //difference = difference.GetDifference(Angle.Degrees360);
                difference = Calculate.DistanceBetweenPlanets(difference, Angle.Degrees360);

            }

            //5.0 Divide difference with 3 to get ochchabala
            var ochchabalaInShashtiamsa = difference.TotalDegrees / 3;

            //return value in shashtiamsa type
            return new Shashtiamsa(ochchabalaInShashtiamsa);
        }

        /// <summary>
        /// Pakshabala : When the Moon is waxing,
        /// take the distance from the Sun to the Moon and
        /// divide it by 3. The quotient is the Pakshabala.
        /// When the Moon is waning, take the distance from
        /// the Moon to the· Sun, and divide it by 3 for assessing
        /// Pakshabala. Moon, Jupiter, Venus and
        /// Mercury are strong in Sukla Paksha and the others
        /// in Krishna Paksha.
        ///
        /// Note: Mercury is benefic or malefic based on planets conjunct with it
        /// </summary>
        public static Shashtiamsa PlanetPakshaBala(PlanetName planetName, Time time)
        {
            double pakshaBala = 0;

            //get moon phase
            var moonPhase = Calculate.LunarDay(time).GetMoonPhase();

            var sunLongitude = Calculate.PlanetNirayanaLongitude(Library.PlanetName.Sun, time);
            var moonLongitude = Calculate.PlanetNirayanaLongitude(Library.PlanetName.Moon, time);

            //var differenceBetweenMoonSun = moonLongitude.GetDifference(sunLongitude);
            var differenceBetweenMoonSun = Calculate.DistanceBetweenPlanets(moonLongitude, sunLongitude);

            //When Moon's Long.-Sun's Long. exceeds 180, deduct it from 360°
            if (differenceBetweenMoonSun.TotalDegrees > 180)
            {
                differenceBetweenMoonSun = Calculate.DistanceBetweenPlanets(differenceBetweenMoonSun, Angle.Degrees360);
            }

            double pakshaBalaOfSubhas = 0;

            //get paksha Bala Of Subhas
            if (moonPhase == MoonPhase.BrightHalf)
            {
                //If birth has occurred during Sukla Paksha subtract the Sun's longitude from that of the Moon· Divide the
                // balance by 3. The result represents the Paksha Bala of Subhas.
                pakshaBalaOfSubhas = differenceBetweenMoonSun.TotalDegrees / 3.0;
            }
            else if (moonPhase == MoonPhase.DarkHalf)
            {
                //Subtract the remainder again from 360 degrees and divide the balance divide 3
                var totalDegrees = Calculate.DistanceBetweenPlanets(differenceBetweenMoonSun, Angle.Degrees360).TotalDegrees;
                pakshaBalaOfSubhas = totalDegrees / 3.0;
            }

            //60 Shashtiamsas diminished by paksha Bala Of Subhas gives the Paksha Bala of Papas
            var pakshaBalaOfPapas = 60.0 - pakshaBalaOfSubhas;

            //if planet is malefic
            var planetIsMalefic = Calculate.IsPlanetMalefic(planetName, time);
            var planesIsBenefic = Calculate.IsPlanetBenefic(planetName, time);

            if (planesIsBenefic == true && planetIsMalefic == false)
            {
                pakshaBala = pakshaBalaOfSubhas;
            }

            if (planesIsBenefic == false && planetIsMalefic == true)
            {
                pakshaBala = pakshaBalaOfPapas;
            }

            //Chandra's Paksha Bala is always to be doubled
            if (planetName == Library.PlanetName.Moon)
            {
                pakshaBala = pakshaBala * 2.0;
            }

            //if paksha bala is 0
            if (pakshaBala == 0)
            {
                //raise error
                //throw new Exception("Paksha bala not found, error!");
                //TODO better error handling, possibly error in logic
                Console.WriteLine("Paksha bala not found, error!!!!");
            }

            return new Shashtiamsa(pakshaBala);
        }

        /// <summary>
        /// Nathonnathabala: Midnight to midday,
        /// the Sun, Jupiter and Venus gain strength proportionately
        /// till they get maximum at zenith. The other
        /// planets, except Mercury. a,re gaining strength from
        /// midday to midnight proportionately. In the same
        /// way, Mercury is always strong and gets 60 shashtiamsas.
        /// </summary>
        public static Shashtiamsa PlanetNathonnathaBala(PlanetName planetName, Time time)
        {

            //no calculation for rahu and ketu here
            var isRahu = planetName.Name == Library.PlanetName.PlanetNameEnum.Rahu;
            var isKetu = planetName.Name == Library.PlanetName.PlanetNameEnum.Ketu;
            var isRahuKetu = isRahu || isKetu;
            if (isRahuKetu) { return Shashtiamsa.Zero; }


            //get local apparent time
            var localApparentTime = Calculate.LocalApparentTime(time);

            //convert birth time (reckoned from midnight) into degrees at 15 degrees per hour
            var hour = localApparentTime.Hour;
            var minuteInHours = localApparentTime.Minute / 60.0;
            var secondInHours = localApparentTime.Second / 3600.0;
            //total hours
            var totalTimeInHours = hour + minuteInHours + secondInHours;

            //convert hours to degrees
            const double degreesPerHour = 15;
            var birthTimeInDegrees = totalTimeInHours * degreesPerHour;

            //if birth time in degrees exceeds 180 degrees subtract it from 360
            if (birthTimeInDegrees > 180)
            {
                birthTimeInDegrees = 360 - birthTimeInDegrees;
            }

            if (planetName == Library.PlanetName.Sun || planetName == Library.PlanetName.Jupiter || planetName == Library.PlanetName.Venus)
            {
                var divBala = birthTimeInDegrees / 3;

                return new Shashtiamsa(divBala);
            }

            if (planetName == Library.PlanetName.Saturn || planetName == Library.PlanetName.Moon || planetName == Library.PlanetName.Mars)
            {
                var ratriBala = (180 - birthTimeInDegrees) / 3;

                return new Shashtiamsa(ratriBala);
            }

            if (planetName == Library.PlanetName.Mercury)
            {
                //Budha has always a Divaratri Bala of 60 Shashtiamsas
                return new Shashtiamsa(60);

            }

            throw new Exception("Planet Nathonnatha Bala not found, error!");
        }

        /// <summary>
        /// Gets Dig Bala of a planet.
        /// Jupiter and Mercury are strong in Lagna (Ascendant),
        /// the Sun and Mars in the 10th, Saturn in
        /// the 7th and the Moon and Venus in the 4th. The
        /// opposite houses are weak , points. Divide the
        /// distance between the longitude of the planet and
        /// its depression point by 3. Quotient is the strength.
        /// </summary>
        public static Shashtiamsa PlanetDigBala(PlanetName planetName, Time time)
        {
            try
            {
                //no calculation for rahu and ketu here
                var isRahu = planetName.Name == PlanetNameEnum.Rahu;
                var isKetu = planetName.Name == PlanetNameEnum.Ketu;
                var isRahuKetu = isRahu || isKetu;
                if (isRahuKetu) { return Shashtiamsa.Zero; }


                //get planet longitude
                var planetLongitude = PlanetNirayanaLongitude(planetName, time);

                //
                Angle powerlessPointLongitude = null;
                House powerlessHouse;


                //subtract the longitude of the 4th house from the longitudes of the Sun and Mars.
                if (planetName == Sun || planetName == Mars)
                {
                    powerlessHouse = HouseLongitude(HouseName.House4, time);
                    powerlessPointLongitude = powerlessHouse.GetMiddleLongitude();
                }

                //Subtract the 7th house, from Jupiter and Mercury.
                if (planetName == Jupiter || planetName == Mercury)
                {
                    powerlessHouse = HouseLongitude(HouseName.House7, time);
                    powerlessPointLongitude = powerlessHouse.GetMiddleLongitude();
                }

                //Subtracc the 10th house from Venus and the Moon
                if (planetName == Venus || planetName == Moon)
                {
                    powerlessHouse = HouseLongitude(HouseName.House10, time);
                    powerlessPointLongitude = powerlessHouse.GetMiddleLongitude();
                }

                //from Saturn, the ascendant.
                if (planetName == Saturn)
                {
                    powerlessHouse = HouseLongitude(HouseName.House1, time);
                    powerlessPointLongitude = powerlessHouse.GetMiddleLongitude();
                }

                //get Digbala arc
                //Digbala arc= planet's long. - its powerless cardinal point.
                //var digBalaArc = planetLongitude.GetDifference(powerlessPointLongitude);
                var xxx = powerlessPointLongitude.TotalDegrees == null ? Angle.Zero : powerlessPointLongitude;
                var digBalaArc = DistanceBetweenPlanets(planetLongitude, xxx);

                //If difference is more than 180° 
                if (digBalaArc > Angle.Degrees180)
                {
                    //subtract it from 360 degrees.
                    //digBalaArc = digBalaArc.GetDifference(Angle.Degrees360);
                    digBalaArc = DistanceBetweenPlanets(digBalaArc, Angle.Degrees360);
                }

                //The Digbala arc of a ptanet, divided by 3, gives the Digbala
                var digBala = digBalaArc.TotalDegrees / 3;



                return new Shashtiamsa(digBala);
            }
            catch (Exception e)
            {
                //print the error and for server guys
                Console.WriteLine(e);

                //continue without a word
                return Shashtiamsa.Zero;
            }

        }

        /// <summary>
        /// Bhava Bala.-Bhava means house and
        /// Bala means strength. Bhava Bala is the potency or
        /// strength of the house or bhava or signification. We
        /// have already seen that there are 12 bhavas which
        /// comprehend all human events. Each bhava signifies
        /// or indicates certain events or functions. For
        /// instance, the first bhava represents Thanu or body,
        /// the appearance of the individual, his complexion,
        /// his disposition, his stature, etc.
        ///
        /// If it attains certain strength, the native will enjoy the indications of
        /// the bhava fully, otherwise he will not sufficiently
        /// enjoy them. The strength of a bhava is composed
        /// of three factors, viz., (1) Bhavadhipathi Bala,
        /// (2) Bhava Digbala, (3) Bhava Drishti Bala.
        /// </summary>
        public static Shashtiamsa HouseStrength(HouseName inputHouse, Time time)
        {

            //CACHE MECHANISM
            return CacheManager.GetCache(new CacheKey(nameof(HouseStrength), inputHouse, time, Ayanamsa), _getBhavabala);


            //UNDERLYING FUNCTION
            Shashtiamsa _getBhavabala()
            {
                //get all the sub-strengths into a list 
                var subStrengthList = new List<HouseSubStrength>();

                subStrengthList.Add(BhavaAdhipathiBala(time));
                subStrengthList.Add(BhavaDigBala(time));
                subStrengthList.Add(BhavaDrishtiBala(time));

                var totalBhavaBala = new Dictionary<HouseName, double>();

                foreach (var houseToTotal in Library.House.AllHouses)
                {
                    //to get the total strength of the a house, we combine 3 types sub-strengths
                    double total = 0;
                    foreach (var subStrength in subStrengthList) { total += subStrength.Power[houseToTotal]; }
                    totalBhavaBala[houseToTotal] = total;
                }

                return new Shashtiamsa(totalBhavaBala[inputHouse]);

            }

        }

        /// <summary>
        /// House received aspect strength
        /// 
        /// Bhavadrishti Bala.-Each bhava in a
        /// horoscope remains aspected by certain planets.
        /// Sometimes the aspect cast on a bhava will be positive
        /// and sometimes it will be negative according
        /// as it is aspected by benefics or malefics.
        /// For all 12 houses
        /// </summary>
        public static HouseSubStrength BhavaDrishtiBala(Time time)
        {

            //CACHE MECHANISM
            return CacheManager.GetCache(new CacheKey(nameof(BhavaDrishtiBala), time, Ayanamsa), _calcBhavaDrishtiBala);


            //UNDERLYING FUNCTION
            HouseSubStrength _calcBhavaDrishtiBala()
            {
                double vdrishti;

                //assign initial negative or positive value based on benefic or malefic planet
                var sp = goodAndBad();


                var drishti = GetDrishtiKendra(time);


                double bala = 0;

                var BhavaDrishtiBala = new Dictionary<HouseName, double>();

                foreach (var house in Library.House.AllHouses)
                {

                    bala = 0;

                    foreach (var planet in Library.PlanetName.All7Planets)
                    {

                        bala += (sp[planet] * drishti[planet.ToString() + house.ToString()]);

                    }

                    BhavaDrishtiBala[house] = bala;
                }


                var newHouseResult = new HouseSubStrength(BhavaDrishtiBala, "BhavaDrishtiBala");

                return newHouseResult;



                //------------------LOCAL FUNCTIONS---------------------

                Dictionary<PlanetName, int> goodAndBad()
                {

                    var _sp = new Dictionary<PlanetName, int>();

                    //assign initial negative or positive value based on benefic or malefic planet
                    foreach (var p in Library.PlanetName.All7Planets)
                    {
                        //Though in the earlier pages Mercury is defined either as a subba
                        //(benefic) or papa (malefic) according to its association is with a benefic or
                        //malefic, Mercury for purposes of calculating Drisbtibala of Bbavas is to
                        //be deemed as a full benefic. This is in accord with the injunctions of
                        //classical writers (Gurugnabbyam tu yuktasya poomamekam tu yojayet).

                        if (p == Library.PlanetName.Mercury)
                        {
                            _sp[p] = 1;
                            continue;
                        }

                        if (Calculate.IsPlanetBenefic(p, time))
                        {
                            _sp[p] = 1;
                        }
                        else
                        {
                            _sp[p] = -1;
                        }
                    }

                    return _sp;
                }

                Dictionary<string, double> GetDrishtiKendra(Time time1)
                {

                    //planet & house no. is used key
                    var _drishti = new Dictionary<string, double>();

                    double drishtiKendra;

                    foreach (var planet in Library.PlanetName.All7Planets)
                    {
                        foreach (var houseNo in Library.House.AllHouses)
                        {
                            //house is considered as a Drusya Graha (aspected body)
                            var houseMid = Calculate.HouseLongitude(houseNo, time1).GetMiddleLongitude();
                            var plantLong = Calculate.PlanetNirayanaLongitude(planet, time1);

                            //Subtract the longitude of the Drishti (aspecting)
                            // planet from that of the Drusya (aspected) Bhava
                            // Madhya. The Drishti Kendra is obtained.
                            drishtiKendra = (houseMid - plantLong).TotalDegrees;

                            //In finding the Drishti Kendra always add 360° to the longitude of the
                            //Drusya (Bhava Madhya) when it is less than the longitude of the
                            //Drishta (aspecting Graha).
                            if (drishtiKendra < 0) { drishtiKendra += 360; }

                            //get special aspect if any
                            vdrishti = FindViseshaDrishti(drishtiKendra, planet);

                            if ((planet == Library.PlanetName.Mercury) || (planet == Library.PlanetName.Jupiter))
                            {
                                //take the Drishti values of Guru and Budha on the Bhava Madhya as they are
                                _drishti[planet.ToString() + (houseNo).ToString()] = FindDrishtiValue(drishtiKendra) + vdrishti;
                            }
                            else
                            {
                                //take a fourth of the aspect value of other Grahas over the Bhava Madhya
                                _drishti[planet.ToString() + (houseNo).ToString()] = (FindDrishtiValue(drishtiKendra) + vdrishti) / 4.00;
                            }
                        }
                    }


                    return _drishti;
                }
            }

        }

        /// <summary>
        /// House strength from different types of signs
        /// 
        /// Bhava Digbala.-This is the strength
        /// acquired by the different bhavas falling in the
        /// different groups or types of signs.
        /// For all 12 houses
        /// </summary>
        public static HouseSubStrength BhavaDigBala(Time time)
        {

            var BhavaDigBala = new Dictionary<HouseName, double>();

            int dig = 0;

            //for every house
            foreach (var houseNumber in Library.House.AllHouses)
            {
                //a particular bhava acquires strength by its mid-point
                //falling in a particular kind of sign.

                //so get mid point of house
                var mid = Calculate.HouseLongitude(houseNumber, time).GetMiddleLongitude().TotalDegrees;
                var houseSign = Calculate.HouseSignName(houseNumber, time);

                //Therefore first find the number of a given Bhava Madhya and subtract
                // it from 1, if the given Bhava Madhya is situated
                // in Vrischika
                if ((mid >= 210.00)
                    && (mid <= 240.00))
                {
                    dig = 1 - (int)houseNumber;
                }
                //Subtract it from 4, if the given Bhava
                // Madhya is situated in Mesha, Vrishabha, Simha,
                // first half of Makara or last half of Dhanus.
                else if (((mid >= 0.00) && (mid <= 60.00))
                         || ((mid >= 120.00) && (mid <= 150.00))
                         || ((mid >= 255.00) && (mid <= 285.00)))
                {
                    dig = 4 - (int)houseNumber;
                }
                //Subtract it from 7 if in Mithuna, Thula, Kumbha, Kanya or
                // first half of Dhanus
                else if (((mid >= 60.00) && (mid <= 90.00))
                         || ((mid >= 150.00) && (mid <= 210.00))
                         || ((mid >= 300.00) && (mid <= 330.00))
                         || ((mid >= 240.00) && (mid <= 255.00)))
                {
                    dig = 7 - (int)houseNumber;
                }
                //and lastly from 1O if in Kataka, Meena and last half of Makara.
                else if (((mid >= 90.00) && (mid <= 120.00))
                         || ((mid >= 330.00) && (mid <= 360.00))
                         || ((mid >= 285.00) && (mid <= 300.00)))
                {
                    dig = 10 - (int)houseNumber;
                }


                //If the difference exceeds 6, subtract it from 12, otherwise
                //take it as it is and multiply this difference by 1O.
                //And you will get Bhava digbala of the particular bhava.

                if (dig < 0)
                {
                    dig = dig + 12;
                }

                if (dig > 6)
                {
                    dig = 12 - dig;
                }

                //store digbala value in return list with house number
                BhavaDigBala[houseNumber] = (double)dig * 10;

            }


            var newHouseResult = new HouseSubStrength(BhavaDigBala, "BhavaDigBala");

            return newHouseResult;

        }

        /// <summary>
        /// Bhavadhipatbi Bala: This is the potency
        /// of the lord of the bhava.
        /// For all 12 houses
        /// </summary>
        public static HouseSubStrength BhavaAdhipathiBala(Time time)
        {
            //CACHE MECHANISM
            return CacheManager.GetCache(new CacheKey(nameof(BhavaAdhipathiBala), time, Ayanamsa), _calcBhavaAdhipathiBala);


            //UNDERLYING FUNCTION
            HouseSubStrength _calcBhavaAdhipathiBala()
            {
                PlanetName houseLord;

                var BhavaAdhipathiBala = new Dictionary<HouseName, double>();

                foreach (var house in Library.House.AllHouses)
                {

                    //get current house lord
                    houseLord = Calculate.LordOfHouse(house, time);

                    //The Shadbala Pinda (aggregate of the Shadbalas) of the lord of the
                    //bhava constitutes its Bhavadhipathi Bala.
                    //get Shadbala Pinda of lord (total strength) in shashtiamsas
                    BhavaAdhipathiBala[house] = PlanetShadbalaPinda(houseLord, time).ToDouble();

                }

                var newHouseResult = new HouseSubStrength(BhavaAdhipathiBala, "BhavaAdhipathiBala");

                return newHouseResult;

            }

        }

        /// <summary>
        /// 0 index is strongest
        /// </summary>
        public static List<PlanetName> BeneficPlanetListByShadbala(Time personBirthTime, int threshold)
        {

            //get all planets
            //var allPlanetByStrenght = AstronomicalCalculator.GetAllPlanetOrderedByStrength(personBirthTime);

            //take top 3 as needed planets
            var returnList = new List<PlanetName>();
            var yyy = Calculate.AllPlanetStrength(personBirthTime);
            foreach (var planet in yyy)
            {
                if (planet.Item1 > threshold)
                {
                    returnList.Add(planet.Item2);
                }
            }
            return returnList;
        }

        public static List<PlanetName> BeneficPlanetListByShadbala(Time personBirthTime)
        {

            //get all planets
            var allPlanetByStrenght = Calculate.AllPlanetOrderedByStrength(personBirthTime);

            //take top 3 as needed planets
            var returnList = new List<PlanetName>();
            returnList.Add(allPlanetByStrenght[0]);
            //returnList.Add(allPlanetByStrenght[1]);
            //returnList.Add(allPlanetByStrenght[2]);

            return returnList;
        }

        /// <summary>
        /// 0 index is strongest
        /// </summary>
        public static List<HouseName> BeneficHouseListByShadbala(Time personBirthTime, int threshold)
        {
            var returnList = new List<HouseName>();

            //create a list with planet names & its strength (unsorted)
            foreach (var house in Library.House.AllHouses)
            {
                //get house strength
                var strength = HouseStrength(house, personBirthTime).ToDouble();

                if (strength > threshold)
                {
                    returnList.Add(house);
                }


            }

            return returnList;


        }

        public static List<HouseName> BeneficHouseListByShadbala(Time personBirthTime)
        {
            //get all planets
            var allPlanetByStrenght = Calculate.AllHousesOrderedByStrength(personBirthTime);

            //take top 3 as needed planets
            var returnList = new List<HouseName>();
            returnList.Add(allPlanetByStrenght[0]);
            //returnList.Add(allPlanetByStrenght[1]);
            //returnList.Add(allPlanetByStrenght[2]);

            return returnList;


        }

        public static List<PlanetName> MaleficPlanetListByShadbala(Time personBirthTime, int threshold)
        {

            var returnList = new List<PlanetName>();
            var yyy = Calculate.AllPlanetStrength(personBirthTime);
            foreach (var planet in yyy)
            {
                if (planet.Item1 < threshold)
                {
                    returnList.Add(planet.Item2);
                }
            }
            return returnList;
        }

        /// <summary>
        /// 0 index is most malefic
        /// </summary>
        public static List<PlanetName> MaleficPlanetListByShadbala(Time personBirthTime)
        {

            //get all planets
            var allPlanetByStrenght = Calculate.AllPlanetOrderedByStrength(personBirthTime);

            //take last 3 as needed planets
            var returnList = new List<PlanetName>();
            returnList.Add(allPlanetByStrenght[^1]);
            //returnList.Add(allPlanetByStrenght[^2]);
            //returnList.Add(allPlanetByStrenght[^3]);

            return returnList;

        }

        /// <summary>
        /// 0 index is most malefic
        /// </summary>
        public static List<HouseName> MaleficHouseListByShadbala(Time personBirthTime, int threshold)
        {
            var returnList = new List<HouseName>();

            //create a list with planet names & its strength (unsorted)
            foreach (var house in Library.House.AllHouses)
            {
                //get house strength
                var strength = HouseStrength(house, personBirthTime).ToDouble();

                if (strength < threshold)
                {
                    returnList.Add(house);
                }


            }

            return returnList;
        }

        public static List<HouseName> MaleficHouseListByShadbala(Time personBirthTime)
        {

            //get all planets
            var allPlanetByStrenght = Calculate.AllHousesOrderedByStrength(personBirthTime);

            //take last 3 as needed planets
            var returnList = new List<HouseName>();
            returnList.Add(allPlanetByStrenght[^1]);
            //returnList.Add(allPlanetByStrenght[^2]);
            //returnList.Add(allPlanetByStrenght[^3]);

            return returnList;

        }

        #endregion

        #region TAGS STATIC

        /// <summary>
        /// Gets all events names grouped by tags, for printing on website for user selection when generating events chart.
        /// </summary>
        public static JObject GetAllEventDataGroupedByTag()
        {

            JObject result = new JObject();

            foreach (EventTag eventTag in Enum.GetValues(typeof(EventTag)))
            {
                var eventDataList = EventManager.GetEventDataListByTag(eventTag);
                if (eventDataList.Any())
                {
                    JArray eventDataArray = new JArray();
                    foreach (var eventData in eventDataList)
                    {
                        eventDataArray.Add(eventData.ToJson());
                    }
                    result[eventTag.ToString()] = eventDataArray;
                }
                else
                {
                    result[eventTag.ToString()] = new JArray();
                }
            }

            return result;

        }

        /// <summary>
        /// Gets all possible algorithm functions, for printing on website for user selection when generating events chart.
        /// </summary>
        public static JArray GetAllEventsChartAlgorithms() => Algorithm.All;

        /// <summary>
        /// keywords or tag related to a house
        /// </summary>
        public static string GetHouseTags(HouseName house)
        {
            switch (house)
            {
                case HouseName.House1: return "beginning of life, childhood, health, environment, personality, the physical body and character";
                case HouseName.House2: return "family, face, right eye, food, wealth, literary gift, and manner and source of death, self-acquisition and optimism";
                case HouseName.House3: return "brothers and sisters, intelligence, cousins and other immediate relations";
                case HouseName.House4: return "peace of mind, home life, mother, conveyances, house property, landed and ancestral properties, education and neck and shoulders";
                case HouseName.House5: return "children, grandfather, intelligence, emotions and fame";
                case HouseName.House6: return "debts, diseases, enemies, miseries, sorrows, illness and disappointments";
                case HouseName.House7: return "wife, husband, marriage, urinary organs, marital happiness, sexual diseases, business partner, diplomacy, talent, energies and general happiness";
                case HouseName.House8: return "longevity, legacies and gifts and unearned wealth, cause of death, disgrace, degradation and details pertaining to death";
                case HouseName.House9: return "father, righteousness, preceptor, grandchildren, intuition, religion, sympathy, fame, charities, leadership, journeys and communications with spirits";
                case HouseName.House10: return "occupation, profession, temporal honours, foreign travels, self-respect, knowledge and dignity and means of livelihood";
                case HouseName.House11: return "means of gains, elder brother and freedom from misery";
                case HouseName.House12: return "losses, expenditure, waste, extravagance, sympathy, divine knowledge, Moksha and the state after death";
                default: throw new Exception("House details not found!");
            }
        }

        /// <summary>
        /// Given a zodiac sign, will return astro keywords related to sign
        /// These details would be highly useful in the delineation of
        /// character and mental disposition
        /// Source:Hindu Predictive Astrology pg.16
        /// </summary>
        public static string GetSignTags(ZodiacName zodiacName)
        {
            switch (zodiacName)
            {
                case ZodiacName.Aries:
                    return @"movable, odd, masculine, cruel, fiery, of short ascension, rising by hinder part, powerful during the night";
                case ZodiacName.Taurus:
                    return @"fixed, even, feminine, mild,earthy, fruitful, of short ascension, rising by hinder part";
                case ZodiacName.Gemini:
                    return @"common, odd, masculine, cruel, airy, barren, of short ascension, rising by the head.";
                case ZodiacName.Cancer:
                    return @"even, movable, feminine, mild, watery, of long ascension, rising by the hinder part and fruitful.";
                case ZodiacName.Leo:
                    return @"fixed, odd, masculine, cruel, fiery, of long ascension, barren, rising by the head.";
                case ZodiacName.Virgo:
                    return @"common, even, feminine, mild, earthy, of long ascension, rising by the head.";
                case ZodiacName.Libra:
                    return @"movable, odd, masculine, cruel, airy, of long ascension, rising by the head.";
                case ZodiacName.Scorpio:
                    return @"fixed, even, feminine, mild, watery, of long ascension, rising by the head.";
                case ZodiacName.Sagittarius:
                    return @"common, odd, masculine, cruel, fiery, of long ascension, rising by the hinder part.";
                case ZodiacName.Capricorn:
                    return @"movable, even, feminine, mild, earthy, of long ascension, rising by hinder part";
                case ZodiacName.Aquarius:
                    return @"fixed, odd, masculine, cruel, fruitful, airy, of short ascension, rising by the head.";
                case ZodiacName.Pisces:
                    return @"common, feminine, water, even, mild, of short ascension, rising by head and hinder part.";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static string GetPlanetTags(List<PlanetName> planetList) => planetList.Aggregate("", (current, planet) => current + GetPlanetTags(planet));

        /// <summary>
        /// Get keywords related to a planet.
        /// </summary>
        public static string GetPlanetTags(PlanetName lordOfHouse)
        {
            switch (lordOfHouse.Name)
            {
                case PlanetName.PlanetNameEnum.Sun:
                    return "Father, masculine, malefic, copper colour, philosophical tendency, royal, ego, sons, patrimony, self reliance, political power, windy and bilious temperament, month, places of worship, money-lenders, goldsmith, bones, fires, coronation chambers, doctoring capacity";
                case PlanetName.PlanetNameEnum.Moon:
                    return "Mother, feminine, mind, benefic when waxing, malefic when waning, white colour, women, sea-men, pearls, gems, water, fishermen, stubbornness, romances, bath-rooms, blood, popularity, human responsibilities";
                case PlanetName.PlanetNameEnum.Mars:
                    return "Brothers, masculine, blood-red colour, malefic, refined taste, base metals, vegetation, rotten things, orators, ambassadors, military activities, commerce, aerial journeys, weaving, public speakers.";
                case PlanetName.PlanetNameEnum.Mercury:
                    return "Profession, benefic if well associated, hermaphrodite, green colour, mercantile activity, public speakers, cold nervous, intelligence";
                case PlanetName.PlanetNameEnum.Jupiter:
                    return "Children, masculine, benefic, bright yellow colour, devotion, truthfulness, religious fervour, philosophical wisdom, corpulence";
                case PlanetName.PlanetNameEnum.Venus:
                    return "Wife, feminine, benefic, mixture of all colours, love affairs, sensual pleasure, family bliss, harems of ill-fame, vitality";
                case PlanetName.PlanetNameEnum.Saturn:
                    return "Longevity, malefic, hermaphrodite, dark colour, stubbornness, impetuosity, demoralisation, windy diseases, despondency, gambling";
                case PlanetName.PlanetNameEnum.Rahu:
                    return "Maternal relations, malefic, feminine, renunciation, corruption, epidemics";
                case PlanetName.PlanetNameEnum.Ketu:
                    return "Paternal relations, Hermaphrodite, malefic, religious, sectarian principles, pride, selfishness, occultism, mendicancy";
                default:
                    return "";
            }
        }

        /// <summary>
        /// Source: Hindu Predictive Astrology pg.17
        /// </summary>
        public static string GetHouseType(HouseName houseNumber)
        {
            //Quadrants (kendras) are l, 4, 7 and 10.
            //Trines(Trikonas) are 5 and 9.
            //Cadent houses (Panaparas) are 2, 5, 8 and 11
            //Succeedent houses (Apoklimas) are 3, 6, 9 and 12 (9th being a trikona must be omitted)
            //Upachayas are 3, 6, 10 and 11.

            var returnString = "";

            switch (houseNumber)
            {
                //Quadrants (kendras) are l, 4, 7 and 10.
                case HouseName.House1:
                case HouseName.House4:
                case HouseName.House7:
                case HouseName.House10:
                    returnString += @"Quadrants (kendras)";
                    break;
                //Trines(Trikonas) are 5 and 9.
                case HouseName.House5:
                case HouseName.House9:
                    returnString += @"Trines (Trikonas)";
                    break;
            }

            switch (houseNumber)
            {
                //Cadent (Panaparas) are 2, 5, 8 and 11
                case HouseName.House2:
                case HouseName.House5:
                case HouseName.House8:
                case HouseName.House11:
                    returnString += @"Cadent (Panaparas)";
                    break;
                //Succeedent (Apoklimas) are 3, 6, 9 and 12 (9th being a trikona must be omitted)
                case HouseName.House3:
                case HouseName.House6:
                case HouseName.House9:
                case HouseName.House12:
                    returnString += @"Succeedent (Apoklimas)";
                    break;
            }

            switch (houseNumber)
            {
                //Upachayas are 3, 6, 10 and 11.
                case HouseName.House3:
                case HouseName.House6:
                case HouseName.House10:
                case HouseName.House11:
                    returnString += @"Upachayas";
                    break;

            }

            return returnString;
        }

        /// <summary>
        /// Get general planetary info for person's dasa (hardcoded table)
        /// It is intended to be used to interpret dasa predictions
        /// as such should be displayed next to dasa chart.
        /// This method is direct translation from the book.
        /// Similar to method GetPlanetDasaNature
        /// Data from pg 80 of Key-planets for Each Sign in Hindu Predictive Astrology
        /// </summary>
        public static string GetDasaInfoForAscendant(ZodiacName ascendantName)
        {
            //As soon as tbc Dasas and Bhuktis are determined, the next
            //step would be to find out the good and evil planets for each
            //ascendant so that in applying the principles to decipher the
            //future history of man, the student may be able to carefully
            //analyse the intensilty or good or evil combinations and proceed
            //further with his predictions when applying the results of
            //Dasas and other combinations.

            switch (ascendantName)
            {
                case ZodiacName.Aries:
                    return @"
                        Aries - Saturn, Mercury and Venus are ill-disposed.
                        Jupiter and the Sun are auspicious. The mere combination
                        of Jupiler and Saturn produces no beneficial results. Jupiter
                        is the Yogakaraka or the planet producing success. If Venus
                        becomes a maraka, he will not kill the native but planets like
                        Saturn will bring about death to the person.
                        ";
                case ZodiacName.Taurus:
                    return @"
                        Taurus - Saturn is the most auspicious and powerful
                        planet. Jupiter, Venus and the Moon are evil planets. Saturn
                        alone produces Rajayoga. The native will be killed in the
                        periods and sub-periods of Jupiter, Venus and the Moon if
                        they get death-inflicting powers.
                        ";
                case ZodiacName.Gemini:
                    return @"
                        Gemini - Mars, Jupiter and the Sun are evil. Venus alone
                        is most beneficial and in conjunction with Saturn in good signs
                        produces and excellent career of much fame. Combination
                        of Saturn and Jupiter produces similar results as in Aries.
                        Venus and Mercury, when well associated, cause Rajayoga.
                        The Moon will not kill the person even though possessed of
                        death-inflicting powers.
                        ";
                case ZodiacName.Cancer:
                    return @"
                        Cancer - Venus and Mercury are evil. Jupiter and Mars
                        give beneficial results. Mars is the Rajayogakaraka
                        (conferor of name and fame). The combination of Mars and Jupiter
                        also causes Rajayoga (combination for political success). The
                        Sun does not kill the person although possessed of maraka
                        powers. Venus and other inauspicious planets kill the native.
                        Mars in combination with the Moon or Jupiter in favourable
                        houses especially the 1st, the 5th, the 9th and the 10th
                        produces much reputation.
                        ";
                case ZodiacName.Leo:
                    return @"
                        Leo - Mars is the most auspicious and favourable planet.
                        The combination of Venus and Jupiter does not cause Rajayoga
                        but the conjunction of Jupiter and Mars in favourable
                        houses produce Rajayoga. Saturn, Venus and Mercury are
                        evil. Saturn does not kill the native when he has the maraka
                        power but Mercury and other evil planets inflict death when
                        they get maraka powers.
                        ";
                case ZodiacName.Virgo:
                    return @"
                        Virgo - Venus alone is the most powerful. Mercury and
                        Venus when combined together cause Rajayoga. Mars and
                        the Moon are evil. The Sun does not kill the native even if
                        be becomes a maraka but Venus, the Moon and Jupiter will
                        inflict death when they are possessed of death-infticting power.
                        ";
                case ZodiacName.Libra:
                    return @"
                        Libra - Saturn alone causes Rajayoga. Jupiter, the Sun
                        and Mars are inauspicious. Mercury and Saturn produce good.
                        The conjunction of the Moon and Mercury produces Rajayoga.
                        Mars himself will not kill the person. Jupiter, Venus
                        and Mars when possessed of maraka powers certainly kill the
                        nalive.
                        ";
                case ZodiacName.Scorpio:
                    return @"
                        Scorpio - Jupiter is beneficial. The Sun and the Moon
                        produce Rajayoga. Mercury and Venus are evil. Jupiter,
                        even if be becomes a maraka, does not inflict death. Mercury
                        and other evil planets, when they get death-inlflicting powers,
                        do not certainly spare the native.
                        ";
                case ZodiacName.Sagittarius:
                    return @"
                        Sagittarius - Mars is the best planet and in conjunction
                        with Jupiter, produces much good. The Sun and Mars also
                        produce good. Venus is evil. When the Sun and Mars
                        combine together they produce Rajayoga. Saturn does not
                        bring about death even when he is a maraka. But Venus
                        causes death when be gets jurisdiction as a maraka planet.
                        ";
                case ZodiacName.Capricorn:
                    return @"
                        Capricorn - Venus is the most powerful planet and in
                        conjunction with Mercury produces Rajayoga. Mars, Jupiter
                        and the Moon are evil.
                        ";
                case ZodiacName.Aquarius:
                    return @"
                        Aquarius - Venus alone is auspicious. The combination of
                        Venus and Mars causes Rajayoga. Jupiter and the Moon are
                        evil.
                        ";
                case ZodiacName.Pisces:
                    return @"
                        Pisces - The Moon and Mars are auspicious. Mars is
                        most powerful. Mars with the Moon or Jupiter causes Rajayoga.
                        Saturn, Venus, the Sun and Mercury are evil. Mars
                        himself does not kill the person even if he is a maraka.
                        ";
                default:
                    throw new ArgumentOutOfRangeException(nameof(ascendantName), ascendantName, null);
            }

        }

        #endregion

        //--------------------------------------------------------------------------------------------


        /// <summary>
        /// Gets the characteristic of signs
        /// </summary>
        public static SignProperties SignProperties(ZodiacName inputSign) => new SignProperties(inputSign);


    }
}

