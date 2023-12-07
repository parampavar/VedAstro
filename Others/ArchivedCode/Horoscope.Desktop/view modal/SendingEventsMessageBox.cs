﻿using System;
using System.Windows;


namespace Horoscope.Desktop
{

    public class SendingEventsMessageBox : ViewModal
    {
        /** BACKING FIELDS **/
        private string _messageText;

        /** EVENTS **/
        public event EventHandler CancelSendingEventsButtonClicked;


        /** CTOR **/
        //defaults are set here
        public SendingEventsMessageBox()
        {
            _messageText = "Sending events...";
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
        public void CancelSendingEventsButton_Click(object sender, RoutedEventArgs routedEventArgs) => CancelSendingEventsButtonClicked?.Invoke(sender, routedEventArgs);
    }


}
