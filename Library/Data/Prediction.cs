﻿using System.Text.RegularExpressions;
using System.Web;

namespace VedAstro.Library
{

    /// <summary>
    /// Simple data wrapper to hold an instance of an Event Prediction (Muhurtha)
    /// </summary>
    public struct Prediction 
    {
        //FIELDS
        private readonly EventName _name;
        private string _description;
        private readonly string _strength;
        private readonly EventNature _nature;
        private readonly Time _startTime;
        private readonly Time _endTime;


        //CTOR
        public Prediction(EventName name, EventNature nature, string description, string strength, Time startTime, Time endTime)
        {
            //initialize fields
            _name = name;
            _nature = nature;
            Description = description; //HTML character safe
            _strength = strength;
            _startTime = startTime;
            _endTime = endTime;
        }



        //PROPERTIES
        //Note: Created mainly for ease of use with WPF binding
        public EventName Name => _name;
        public string Description
        {
            get => HttpUtility.HtmlDecode(_description);
            set => _description = HttpUtility.HtmlEncode(value);
        }
        public string Strength => _strength;
        public EventNature Nature => _nature;
        public Time StartTime => _startTime;
        public Time EndTime => _endTime;
        public int Duration => GetDurationMinutes();



        //PUBLIC METHODS
        public EventName GetName() => _name;

        public EventNature GetNature() => _nature;

        public Time GetStartTime() => _startTime;

        public string GetDescription() => _description;

        public Time GetEndTime() => _endTime;


        //PRIVATE METHODS
        private static string SplitCamelCase(string str)
        {
            return Regex.Replace(
                Regex.Replace(
                    str,
                    @"(\P{Ll})(\P{Ll}\p{Ll})",
                    "$1 $2"
                ),
                @"(\p{Ll})(\P{Ll})",
                "$1 $2"
            );
        }


        //METHOD OVERRIDES
        public override bool Equals(object value)
        {

            if (value.GetType() == typeof(Prediction))
            {
                //cast to type
                var parsedValue = (Prediction)value;

                //check equality
                bool returnValue = (this.GetHashCode() == parsedValue.GetHashCode());

                return returnValue;
            }
            else
            {
                //Return false if value is null
                return false;
            }


        }

        public override int GetHashCode()
        {
            //get hash of all the fields & combine them
            var hash1 = _name.GetHashCode();
            var hash2 = Tools.GetStringHashCode(_description);
            var hash3 = _nature.GetHashCode();
            var hash4 = _startTime.GetHashCode();
            var hash5 = _endTime.GetHashCode();

            return hash1 + hash2 + hash3 + hash4 + hash5;
        }

        public override string ToString()
        {
            return $"{GetName()} - {_nature} - {_startTime} - {_endTime} - {GetDurationMinutes()}";
        }


        /// <summary>
        /// Gets the duration of the event from start to end time
        /// </summary>
        public int GetDurationMinutes()
        {
            var difference = GetEndTime().GetStdDateTimeOffset() - GetStartTime().GetStdDateTimeOffset();

            return (int)difference.TotalMinutes;
        }
    }
}