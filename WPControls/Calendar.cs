using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Data;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace WPControls
{
    /// <summary>
    /// Calendar control for Windows Phone 7
    /// </summary>
    public class Calendar : Control
    {
        #region Constructor

        /// <summary>
        /// Create new instance of a calendar
        /// </summary>
        public Calendar()
        {
            DefaultStyleKey = typeof(Calendar);
            DatesAssigned = new List<DateTime>();
            var binding = new Binding();
            Loaded += CalendarLoaded;
            SetBinding(PrivateDataContextPropertyProperty, binding);
            WireUpDataSource(DataContext, DataContext);

        }

        void CalendarLoaded(object sender, RoutedEventArgs e)
        {
            if (EnableGestures)
            {
                EnableGesturesSupport();
            }
        }

        private void EnableGesturesSupport()
        {
            DisableGesturesSupport();
            TouchPanel.EnabledGestures = GestureType.Flick;
            ManipulationCompleted += CalendarManipulationCompleted;
        }

        private void DisableGesturesSupport()
        {
            TouchPanel.EnabledGestures = GestureType.None;
            ManipulationCompleted -= CalendarManipulationCompleted;
        }

        void CalendarManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            while (TouchPanel.IsGestureAvailable)
            {
                var gesture = TouchPanel.ReadGesture();
                if (gesture.GestureType == GestureType.Flick)
                {
                    double horizontal = gesture.Delta.X / Factor;
                    double vertical = gesture.Delta.Y / Factor;
                    if (Math.Abs(horizontal) > Math.Abs(vertical))
                    {
                        if ((int)horizontal > 0)
                        {
                            DecrementMonth();
                        }
                        else
                        {
                            IncrementMonth();

                        }
                    }
                    else
                    {
                        if ((int)vertical > 0)
                        {
                            DecrementYear();
                        }
                        else
                        {
                            IncrementYear();

                        }
                    }
                }
            }
        }



        #endregion

        #region Gestures

        #endregion

        #region Memebers

        private Grid _itemsGrid;
        CalendarItem _lastItem;
        private bool _addedItems;
        private int _month = DateTime.Today.Month;
        private int _year = DateTime.Today.Year;
        internal List<DateTime> DatesAssigned;

        #endregion

        #region Events

        /// <summary>
        /// Event that occurs before month/year combination is changed
        /// </summary>
        public event EventHandler<MonthChangedEventArgs> MonthChanging;

        /// <summary>
        /// Event that occurs after month/year combination is changed
        /// </summary>
        public event EventHandler<MonthChangedEventArgs> MonthChanged;

        /// <summary>
        /// Event that occurs after a date is selected on the calendar
        /// </summary>
        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;

        /// <summary>
        /// Raises MonthChanging event
        /// </summary>
        /// <param name="year">Year for event arguments</param>
        /// <param name="month">Month for event arguments</param>
        protected void OnMonthChanging(int year, int month)
        {
            if (MonthChanging != null)
            {
                MonthChanging(this, new MonthChangedEventArgs(year, month));
            }
        }

        /// <summary>
        /// Raises MonthChanged event
        /// </summary>
        /// <param name="year">Year for event arguments</param>
        /// <param name="month">Month for event arguments</param>
        protected void OnMonthChanged(int year, int month)
        {
            if (MonthChanged != null)
            {
                MonthChanged(this, new MonthChangedEventArgs(year, month));
            }
        }

        ///// <summary>
        ///// Raises SelectedChanged event
        ///// </summary>
        ///// <param name="dateTime">Selected date</param>
        //protected void OnSelectionChanged(DateTime dateTime)
        //{
        //    if (SelectionChanged != null)
        //    {
        //        SelectionChanged(this, new SelectionChangedEventArgs(dateTime));
        //    }
        //}

        #endregion

        #region Constants

        private const short RowCount = 6;
        private const short ColumnCount = 8;

        #endregion

        #region Properties



        internal object PrivateDataContextProperty
        {
            get { return GetValue(PrivateDataContextPropertyProperty); }
            set { SetValue(PrivateDataContextPropertyProperty, value); }
        }

        internal static readonly DependencyProperty PrivateDataContextPropertyProperty =
            DependencyProperty.Register("PrivateDataContextProperty", typeof(object), typeof(Calendar), new PropertyMetadata(null, OnPrivateDataContextChanged));

        private static void OnPrivateDataContextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var calendar = sender as Calendar;
            if (calendar != null)
            {
                calendar.WireUpDataSource(e.OldValue, e.NewValue);
                calendar.Refresh();
            }
        }

        private void WireUpDataSource(object oldValue, object newValue)
        {
            if (newValue != null)
            {
                var source = newValue as INotifyPropertyChanged;
                if (source != null)
                {
                    source.PropertyChanged += SourcePropertyChanged;
                }
            }
            if (oldValue != null)
            {
                var source = newValue as INotifyPropertyChanged;
                if (source != null)
                {
                    source.PropertyChanged -= SourcePropertyChanged;
                }
            }
        }

        private const short Factor = 1000;

        /// <summary>
        /// Explicitly refresh the calendar
        /// </summary>
        public void Refresh()
        {
            BuildDates();
            BuildItems();
        }

        private void SourcePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var expression = GetBindingExpression(DatesSourceProperty);
            if (expression != null)
            {
                if (expression.ParentBinding.Path.Path.EndsWith(e.PropertyName))
                {
                    Refresh();
                }
            }
        }


        /// <summary>
        /// Collection of objects containing dates
        /// </summary>
        public IEnumerable<ISupportCalendarItem> DatesSource
        {
            get { return (IEnumerable<ISupportCalendarItem>)GetValue(DatesSourceProperty); }
            set { SetValue(DatesSourceProperty, value); }
        }

        /// <summary>
        /// Collection of objects containing dates
        /// </summary>
        public static readonly DependencyProperty DatesSourceProperty =
            DependencyProperty.Register("DatesSource", typeof(IEnumerable<ISupportCalendarItem>), typeof(Calendar), new PropertyMetadata(null, OnDatesSourceChanged));

        private static void OnDatesSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var calendar = sender as Calendar;
            Debug.Assert(calendar != null, "calendar != null");
            calendar.BuildDates();
            calendar.BuildItems();
            if (e.OldValue is INotifyCollectionChanged)
            {
                ((INotifyCollectionChanged)e.NewValue).CollectionChanged -= calendar.DatesSourceChanged;
            }
            if (e.NewValue is INotifyCollectionChanged)
            {
                (e.NewValue as INotifyCollectionChanged).CollectionChanged += calendar.DatesSourceChanged;
            }
        }

        private void DatesSourceChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Refresh();
        }

        /// <summary>
        /// Property name for each object in DatesSource that contains the date to be evaluating 
        /// when building a calendar
        /// </summary>
        public static readonly DependencyProperty DatePropertyNameForDatesSourceProperty =
            DependencyProperty.Register("DatePropertyNameForDatesSource", typeof(string), typeof(Calendar), new PropertyMetadata(string.Empty, OnDatesSourceChanged));



        /// <summary>
        /// Style for the calendar item
        /// </summary>
        public Style CalendarItemStyle
        {
            get { return (Style)GetValue(CalendarItemStyleProperty); }
            set { SetValue(CalendarItemStyleProperty, value); }
        }

        /// <summary>
        /// Style for the calendar item
        /// </summary>
        public static readonly DependencyProperty CalendarItemStyleProperty =
            DependencyProperty.Register("CalendarItemStyle", typeof(Style), typeof(Calendar), new PropertyMetadata(null));

        /// <summary>
        /// Style for the calendar item
        /// </summary>
        public Style CalendarWeekItemStyle
        {
            get { return (Style)GetValue(CalendarWeekItemStyleStyleProperty); }
            set { SetValue(CalendarWeekItemStyleStyleProperty, value); }
        }

        /// <summary>
        /// Style for the calendar item
        /// </summary>
        public static readonly DependencyProperty CalendarWeekItemStyleStyleProperty =
            DependencyProperty.Register("CalendarWeekItemStyle", typeof(Style), typeof(Calendar), new PropertyMetadata(null));

        /// <summary>
        /// This value is shown in calendar header and includes month and year
        /// </summary>
        public string YearMonthLabel
        {
            get { return (string)GetValue(YearMonthLabelProperty); }
            internal set { SetValue(YearMonthLabelProperty, value); }
        }

        /// <summary>
        /// This value is shown in calendar header and includes month and year
        /// </summary>
        public static readonly DependencyProperty YearMonthLabelProperty =
            DependencyProperty.Register("YearMonthLabel", typeof(string), typeof(Calendar), new PropertyMetadata(""));

        /// <summary>
        /// This value currently selected date on the calendar
        /// This property can be bound to
        /// </summary>
        public DateTime SelectedDate
        {
            get { return (DateTime)GetValue(SelectedDateProperty); }
            set { SetValue(SelectedDateProperty, value); }
        }

        /// <summary>
        /// This value currently selected date on the calendar
        /// This property can be bound to
        /// </summary>
        public static readonly DependencyProperty SelectedDateProperty =
            DependencyProperty.Register("SelectedDate", typeof(DateTime), typeof(Calendar), new PropertyMetadata(DateTime.MinValue, OnSelectedDateChanged));

        private static void OnSelectedDateChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var calendar = sender as Calendar;
            Debug.Assert(calendar != null, "calendar != null");
            //calendar.OnSelectionChanged((DateTime)e.NewValue);
        }


        /// <summary>
        /// This converter is used to dynamically color the background or day number of a calendar cell
        /// based on date and the fact that a date is selected and type of conversion
        /// </summary>
        public IDateToBrushConverter ColorConverter
        {
            get { return (IDateToBrushConverter)GetValue(ColorConverterProperty); }
            set { SetValue(ColorConverterProperty, value); }
        }

        /// <summary>
        /// This converter is used to dynamically color the background of a calendar cell
        /// based on date and the fact that a date is selected
        /// </summary>
        public static readonly DependencyProperty ColorConverterProperty =
            DependencyProperty.Register("ColorConverter", typeof(IDateToBrushConverter), typeof(Calendar), new PropertyMetadata(null));



        /// <summary>
        /// Currently selected year
        /// </summary>
        public int SelectedYear
        {
            get { return (int)GetValue(SelectedYearProperty); }
            set { SetValue(SelectedYearProperty, value); }
        }

        /// <summary>
        /// Currently selected year
        /// </summary>
        public static readonly DependencyProperty SelectedYearProperty =
            DependencyProperty.Register("SelectedYear", typeof(int), typeof(Calendar), new PropertyMetadata(DateTime.Today.Year, OnSelectedYearMonthChanged));

        private static void OnSelectedYearMonthChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var calendar = sender as Calendar;
            if (calendar != null && (calendar._year != calendar.SelectedYear || calendar._month != calendar.SelectedMonth))
            {
                if (!calendar._ignoreMonthChange)
                {
                    calendar._year = calendar.SelectedYear;
                    calendar._month = calendar.SelectedMonth;
                    calendar.SetYearMonthLabel();
                }
            }
        }


        /// <summary>
        /// Currently selected month
        /// </summary>
        public int SelectedMonth
        {
            get { return (int)GetValue(SelectedMonthProperty); }
            set { SetValue(SelectedMonthProperty, value); }
        }

        /// <summary>
        /// Currently selected month
        /// </summary>
        public static readonly DependencyProperty SelectedMonthProperty =
            DependencyProperty.Register("SelectedMonth", typeof(int), typeof(Calendar), new PropertyMetadata(DateTime.Today.Month, OnSelectedYearMonthChanged));


        /// <summary>
        /// If true, previous and next month buttons are shown
        /// </summary>
        public bool ShowNavigationButtons
        {
            get { return (bool)GetValue(ShowNavigationButtonsProperty); }
            set { SetValue(ShowNavigationButtonsProperty, value); }
        }

        /// <summary>
        /// If true, previous and next month buttons are shown
        /// </summary>
        public static readonly DependencyProperty ShowNavigationButtonsProperty =
            DependencyProperty.Register("ShowNavigationButtons", typeof(bool), typeof(Calendar), new PropertyMetadata(true));

        /// <summary>
        /// If true, gesture support is enabled
        /// </summary>
        public bool EnableGestures
        {
            get { return (bool)GetValue(EnableGesturesProperty); }
            set { SetValue(EnableGesturesProperty, value); }
        }

        /// <summary>
        /// If true, gesture support is enabled
        /// </summary>
        public static readonly DependencyProperty EnableGesturesProperty =
            DependencyProperty.Register("EnableGestures", typeof(bool), typeof(Calendar), new PropertyMetadata(false, OnEnableGesturesChanged));

        /// <summary>
        /// Handle changes to gesture support
        /// </summary>
        /// <param name="sender">Calendar control</param>
        /// <param name="e">Event arguments</param>
        public static void OnEnableGesturesChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var target = (Calendar)sender;
            if (target.EnableGestures)
            {
                target.EnableGesturesSupport();
            }
            else
            {
                target.DisableGesturesSupport();
            }
        }

        /// <summary>
        /// If set to false, selected date is not highlighted
        /// </summary>
        public bool ShowSelectedDate
        {
            get { return (bool)GetValue(ShowSelectedDateProperty); }
            set { SetValue(ShowSelectedDateProperty, value); }
        }

        /// <summary>
        /// If set to false, selected date is not highlighted
        /// </summary>
        public static readonly DependencyProperty ShowSelectedDateProperty =
            DependencyProperty.Register("ShowSelectedDate", typeof(bool), typeof(Calendar), new PropertyMetadata(true));


        /// <summary>
        /// Sets an option of how to display week number
        /// </summary>
        public WeekNumberDisplayOption WeekNumberDisplay
        {
            get { return (WeekNumberDisplayOption)GetValue(WeekNumberDisplayProperty); }
            set { SetValue(WeekNumberDisplayProperty, value); }
        }

        /// <summary>
        /// If set to false, selected date is not highlighted
        /// </summary>
        public static readonly DependencyProperty WeekNumberDisplayProperty =
            DependencyProperty.Register("WeekNumberDisplay", typeof(WeekNumberDisplayOption), typeof(Calendar),
            new PropertyMetadata(WeekNumberDisplayOption.None, OnWeekNumberDisplayChanged));

        /// <summary>
        /// Update calendar display when display option changes
        /// </summary>
        /// <param name="sender">Calendar control</param>
        /// <param name="e">Event arguments</param>
        public static void OnWeekNumberDisplayChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((Calendar)sender).BuildItems();
        }

        #endregion

        #region Template

        /// <summary>
        /// Apply default template and perform initialization
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var previousButton = GetTemplateChild("PreviousMonthButton") as Button;
            if (previousButton != null) previousButton.Click += PreviousButtonClick;
            var nextButton = GetTemplateChild("NextMonthButton") as Button;
            if (nextButton != null) nextButton.Click += NextButtonClick;
            _itemsGrid = GetTemplateChild("ItemsGrid") as Grid;
            BuildDates();
            SetYearMonthLabel();
        }

        #endregion

        #region Event handling

        void NextButtonClick(object sender, RoutedEventArgs e)
        {
            IncrementMonth();
        }

        private void IncrementMonth()
        {
            if (_year != 2499 || _month != 12)
            {
                _month += 1;
                if (_month == 13)
                {
                    _month = 1;
                    _year += 1;
                }
                SetYearMonthLabel();
            }
        }

        void PreviousButtonClick(object sender, RoutedEventArgs e)
        {
            DecrementMonth();
        }

        private void DecrementMonth()
        {
            if (_year != 1753 || _month != 1)
            {
                _month -= 1;
                if (_month == 0)
                {
                    _month = 12;
                    _year -= 1;
                }
                SetYearMonthLabel();
            }
        }

        private void IncrementYear()
        {
            if (_year != 2499)
            {
                _year += 1;
                SetYearMonthLabel();
            }
        }

        private void DecrementYear()
        {
            if (_year != 1753)
            {
                _year -= 1;
                SetYearMonthLabel();
            }
        }

        private void ItemClick(object sender, RoutedEventArgs e)
        {
            if (_lastItem != null)
            {
                _lastItem.IsSelected = false;
            }
            _lastItem = (sender as CalendarItem);
            if (_lastItem != null)
            {
                if (ShowSelectedDate)
                    _lastItem.IsSelected = true;
                SelectedDate = _lastItem.ItemDate;
            }
        }

        #endregion

        #region Methods
        private bool _ignoreMonthChange;
        private void SetYearMonthLabel()
        {
            OnMonthChanging(_year, _month);
            YearMonthLabel = string.Concat(GetMonthName(), " ", _year.ToString());
            _ignoreMonthChange = true;
            SelectedMonth = _month;
            SelectedYear = _year;
            _ignoreMonthChange = false;
            BuildItems();
            OnMonthChanged(_year, _month);
        }

        private string GetMonthName()
        {
            string returnValue = "";
            switch (_month)
            {
                case 1:
                    returnValue = "January";
                    break;
                case 2:
                    returnValue = "February";
                    break;
                case 3:
                    returnValue = "March";
                    break;
                case 4:
                    returnValue = "April";
                    break;
                case 5:
                    returnValue = "May";
                    break;
                case 6:
                    returnValue = "June";
                    break;
                case 7:
                    returnValue = "July";
                    break;
                case 8:
                    returnValue = "August";
                    break;
                case 9:
                    returnValue = "September";
                    break;
                case 10:
                    returnValue = "October";
                    break;
                case 11:
                    returnValue = "November";
                    break;
                case 12:
                    returnValue = "December";
                    break;
            }
            return returnValue;
        }

        private void BuildItems()
        {
            if (_itemsGrid != null)
            {
                AddDefaultItems();
                var startOfMonth = new DateTime(_year, _month, 1);
                DayOfWeek dayOfWeek = startOfMonth.DayOfWeek;
                var daysInMonth = (int)Math.Floor(startOfMonth.AddMonths(1).Subtract(startOfMonth).TotalDays);
                var addedDays = 0;
                int lastWeekNumber = 0;
                for (int rowCount = 1; rowCount <= RowCount; rowCount++)
                {
                    for (var columnCount = 1; columnCount < ColumnCount; columnCount++)
                    {
                        var item = (CalendarItem)(from oneChild in _itemsGrid.Children
                                                  where oneChild is CalendarItem &&
                                                  ((CalendarItem)oneChild).Tag.ToString() == string.Concat(rowCount.ToString(), ":", columnCount.ToString())
                                                  select oneChild).First();
                        if (rowCount == 1 && columnCount < (int)dayOfWeek + 1)
                        {
                            item.Visibility = Visibility.Collapsed;
                        }
                        else if (addedDays < daysInMonth)
                        {
                            item.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            item.Visibility = Visibility.Collapsed;
                        }

                        var weekItem = (CalendarWeekItem)(from oneChild in _itemsGrid.Children
                                                          where oneChild is CalendarWeekItem &&
                                                          ((CalendarWeekItem)oneChild).Tag.ToString() == string.Concat(rowCount.ToString(), ":0")
                                                          select oneChild).FirstOrDefault();

                        if (item.Visibility == Visibility.Visible)
                        {
                            item.ItemDate = startOfMonth.AddDays(addedDays);
                            if (SelectedDate == DateTime.MinValue && item.ItemDate == DateTime.Today)
                            {
                                SelectedDate = item.ItemDate;
                                if (ShowSelectedDate)
                                    item.IsSelected = true;
                                _lastItem = item;
                            }
                            else
                            {
                                if (item.ItemDate == SelectedDate)
                                {
                                    if (ShowSelectedDate)
                                        item.IsSelected = true;
                                }
                                else
                                {
                                    item.IsSelected = false;
                                }
                            }
                            addedDays += 1;
                            item.DayNumber = addedDays;
                            item.SetBackcolor();

                            if (WeekNumberDisplay != WeekNumberDisplayOption.None)
                            {
                                int weekNumber;

                                if (WeekNumberDisplay == WeekNumberDisplayOption.WeekOfYear)
                                {
                                    var systemCalendar = System.Threading.Thread.CurrentThread.CurrentCulture.Calendar;
                                    weekNumber = systemCalendar.GetWeekOfYear(
                                        item.ItemDate,
                                        System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.CalendarWeekRule,
                                        System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.FirstDayOfWeek);
                                }
                                else
                                {
                                    weekNumber = rowCount;
                                }
                                weekItem.WeekNumber = weekNumber;
                                lastWeekNumber = weekNumber;
                                weekItem.Visibility = Visibility.Visible;
                            }
                        }
                        else
                        {
                            if (WeekNumberDisplay != WeekNumberDisplayOption.None && weekItem.WeekNumber != lastWeekNumber)
                            {
                                weekItem.Visibility = Visibility.Collapsed;
                            }
                        }
                    }
                }
            }
        }

        private void AddDefaultItems()
        {
            if (!_addedItems && _itemsGrid != null)
            {
                for (int rowCount = 1; rowCount <= RowCount; rowCount++)
                {
                    for (int columnCount = 1; columnCount < ColumnCount; columnCount++)
                    {
                        var item = new CalendarItem(this);
                        item.SetValue(Grid.RowProperty, rowCount);
                        item.SetValue(Grid.ColumnProperty, columnCount);
                        item.Visibility = Visibility.Collapsed;
                        item.Tag = string.Concat(rowCount.ToString(), ":", columnCount.ToString());
                        item.Click += ItemClick;
                        if (CalendarItemStyle != null)
                        {
                            item.Style = CalendarItemStyle;
                        }
                        _itemsGrid.Children.Add(item);
                    }
                    if (WeekNumberDisplay != WeekNumberDisplayOption.None)
                    {
                        const int columnCount = 0;
                        var item = new CalendarWeekItem();
                        item.SetValue(Grid.RowProperty, rowCount);
                        item.SetValue(Grid.ColumnProperty, columnCount);
                        item.Visibility = Visibility.Collapsed;
                        item.Tag = string.Concat(rowCount.ToString(), ":", columnCount.ToString());
                        if (CalendarWeekItemStyle != null)
                        {
                            item.Style = CalendarWeekItemStyle;
                        }
                        _itemsGrid.Children.Add(item);
                    }
                }
                _addedItems = true;
            }
        }

        private void BuildDates()
        {
            if (DatesSource != null)
            {
                DatesAssigned.Clear();
                DatesSource.ToList().ForEach(one => DatesAssigned.Add(one.CalendarItemDate));
            }
        }

        #endregion

    }
}
