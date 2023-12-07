﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Genso.Astrology.Library;

namespace Muhurtha.Desktop
{

    /// <summary>
    /// The main visible place where events are listed, located next to options panel
    /// </summary>
    public class EventView : ViewModal
    {
        /** BACKING FIELDS **/
        private List<Event> _eventList;
        private ComboBoxItem _selectedEvent;



        /** EVENTS **/
        public event EventHandler CalculateEventsButtonClicked;
        public event EventHandler CancelButtonClicked; //todo supposed to be in options panel



        /** PROPERTIES **/
        //Event that is selected (not yet used), maybe can be used to show more info
        public ComboBoxItem SelectedEvent
        {
            get => _selectedEvent;
            set
            {
                _selectedEvent = value;
                OnPropertyChanged(nameof(SelectedEvent));
            }
        }
        //List of events that have been calculated
        public List<Event> EventList
        {
            get => _eventList;
            set
            {
                _eventList = value;
                OnPropertyChanged(nameof(EventList));
            }
        }




        /** PUBLIC METHODS **/



        /** EVENT ROUTING **/
        public void CalculateEventsButton_Click(object sender, RoutedEventArgs routedEventArgs) => CalculateEventsButtonClicked?.Invoke(sender, routedEventArgs);
        public void CancelButton_OnClick(object sender, RoutedEventArgs routedEventArgs) => CancelButtonClicked?.Invoke(sender, routedEventArgs);

    }


}
