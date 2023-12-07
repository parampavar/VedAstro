﻿using System;
using System.Windows;

namespace Muhurtha.Desktop
{

    public class EventsCalculatingPopup : ViewModal
    {
        /** BACKING FIELDS **/
        private string _messageText;

        /** EVENTS **/
        public event EventHandler CancelCalculateEventsButtonClicked;


        /** CTOR **/
        //defaults are set here
        public EventsCalculatingPopup()
        {
            _messageText = "Calculating events...";
            this.Hide(); //default hidden
        }


        /** PROPERTIES **/
        public string MessageText
        {
            get => _messageText;
            set
            {
                _messageText = value;
                OnPropertyChanged(nameof(MessageText));
            }
        }



        /** PUBLIC METHODS **/


        /** EVENT ROUTING **/
        public void CancelCalculateEventsButton_Click(object sender, RoutedEventArgs routedEventArgs) => CancelCalculateEventsButtonClicked?.Invoke(sender, routedEventArgs);
    }


}
