﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Calendar.Plugin.Shared
{
    public partial class Calendar : ContentView
    {
        private readonly List<CalendarButton> _buttons;
        private readonly List<Grid> _mainCalendars;
        private List<Label> _titleLabels;
        private readonly StackLayout _mainView;

        public readonly StackLayout _contentView;
        public static double GridSpace = 0;
        public event EventHandler<EventArgs> OnStartRenderCalendar, OnEndRenderCalendar;

        public Calendar()
        {
            TitleLeftArrow = new CalendarButton
            {
                FontAttributes = FontAttributes.Bold,
                TintColor = Color.Transparent,
                FontSize = 24,
                Text = "❰",
                TextColor = Color.FromHex("#c82727")
            };
            TitleLabel = new Label
            {
                FontSize = 24,
                VerticalTextAlignment = TextAlignment.Center,
                HorizontalTextAlignment = TextAlignment.Center,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.Black,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Text = ""
            };
            TitleRightArrow = new CalendarButton
            {
                FontAttributes = FontAttributes.Bold,
                TintColor = Color.Transparent,
                FontSize = 24,
                Text = "❱",
                TextColor = Color.FromHex("#c82727")
            };
            MonthNavigationLayout = new StackLayout
            {
                Padding = 0,
                VerticalOptions = LayoutOptions.Start,
                Orientation = StackOrientation.Horizontal,
                HeightRequest = Device.RuntimePlatform == Device.UWP ? 50 : 32,
                Children = { TitleLeftArrow, TitleLabel, TitleRightArrow }
            };
            _contentView = new StackLayout
            {
                Padding = 0,
                Orientation = StackOrientation.Vertical
            };
            _mainView = new StackLayout
            {
                Padding = 0,
                Orientation = StackOrientation.Vertical,
                Children = { MonthNavigationLayout, _contentView }
            };


            TitleLeftArrow.Clicked += LeftArrowClickedEvent;
            TitleRightArrow.Clicked += RightArrowClickedEvent;
            _dayLabels = new List<Label>(7);
            _weekNumberLabels = new List<Label>(6);
            _buttons = new List<CalendarButton>(42);
            _mainCalendars = new List<Grid>(1);
            _weekNumbers = new List<Grid>(1);

            CalendarViewType = DateTypeEnum.Normal;
            YearsRow = 4;
            YearsColumn = 4;
        }

        public bool IsRendering => Content == null;

        #region MinDate

        public static readonly BindableProperty MinDateProperty =
            BindableProperty.Create(nameof(MinDate), typeof(DateTime?), typeof(Calendar), null,
                                    propertyChanged: (bindable, oldValue, newValue) => (bindable as Calendar).ChangeCalendar(CalandarChanges.MaxMin));

        /// <summary>
        /// Gets or sets the minimum date.
        /// </summary>
        /// <value>The minimum date.</value>
        public DateTime? MinDate
        {
            get => (DateTime?)GetValue(MinDateProperty);
            set { SetValue(MinDateProperty, value); ChangeCalendar(CalandarChanges.MaxMin); }
        }

        #endregion

        #region MaxDate

        public static readonly BindableProperty MaxDateProperty =
            BindableProperty.Create(nameof(MaxDate), typeof(DateTime?), typeof(Calendar), null,
                                    propertyChanged: (bindable, oldValue, newValue) => (bindable as Calendar).ChangeCalendar(CalandarChanges.MaxMin));

        /// <summary>
        /// Gets or sets the max date.
        /// </summary>
        /// <value>The max date.</value>
        public DateTime? MaxDate
        {
            get => (DateTime?)GetValue(MaxDateProperty);
            set => SetValue(MaxDateProperty, value);
        }

        #endregion

        #region StartDate

        public static readonly BindableProperty StartDateProperty =
            BindableProperty.Create(nameof(StartDate), typeof(DateTime), typeof(Calendar), DateTime.Now,
                                    propertyChanged: (bindable, oldValue, newValue) => (bindable as Calendar).ChangeCalendar(CalandarChanges.StartDate));

        /// <summary>
        /// Gets or sets a date, to pick the month, the calendar is focused on
        /// </summary>
        /// <value>The start date.</value>
        public DateTime StartDate
        {
            get => (DateTime)GetValue(StartDateProperty);
            set => SetValue(StartDateProperty, value);
        }

        #endregion

        #region StartDay

        public static readonly BindableProperty StartDayProperty =
            BindableProperty.Create(nameof(StartDate), typeof(DayOfWeek), typeof(Calendar), DayOfWeek.Sunday,
                                    propertyChanged: (bindable, oldValue, newValue) => (bindable as Calendar).ChangeCalendar(CalandarChanges.StartDay));

        /// <summary>
        /// Gets or sets the day the calendar starts the week with.
        /// </summary>
        /// <value>The start day.</value>
        public DayOfWeek StartDay
        {
            get => (DayOfWeek)GetValue(StartDayProperty);
            set => SetValue(StartDayProperty, value);
        }

        #endregion

        #region BorderWidth

        public static readonly BindableProperty BorderWidthProperty =
            BindableProperty.Create(nameof(BorderWidth), typeof(int), typeof(Calendar), Device.RuntimePlatform == Device.iOS ? 1 : 3,
                                    propertyChanged: (bindable, oldValue, newValue) => (bindable as Calendar).ChangeBorderWidth((int)newValue, (int)oldValue));

        protected void ChangeBorderWidth(int newValue, int oldValue)
        {
            if (newValue == oldValue) return;
            _buttons.FindAll(b => !b.IsSelected && b.IsEnabled).ForEach(b => b.BorderWidth = newValue);
        }

        /// <summary>
        /// Gets or sets the border width of the calendar.
        /// </summary>
        /// <value>The width of the border.</value>
        public int BorderWidth
        {
            get => (int)GetValue(BorderWidthProperty);
            set => SetValue(BorderWidthProperty, value);
        }

        #endregion

        #region OuterBorderWidth

        public static readonly BindableProperty OuterBorderWidthProperty =
            BindableProperty.Create(nameof(OuterBorderWidth), typeof(int), typeof(Calendar), Device.RuntimePlatform == Device.iOS ? 1 : 3,
                                    propertyChanged: (bindable, oldValue, newValue) => (bindable as Calendar)._mainCalendars.ForEach(obj => obj.Padding = (int)newValue));

        /// <summary>
        /// Gets or sets the width of the whole calandar border.
        /// </summary>
        /// <value>The width of the outer border.</value>
        public int OuterBorderWidth
        {
            get => (int)GetValue(OuterBorderWidthProperty);
            set => SetValue(OuterBorderWidthProperty, value);
        }

        #endregion

        #region BorderColor

        public static readonly BindableProperty BorderColorProperty =
            BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(Calendar), Color.FromHex("#dddddd"),
                                    propertyChanged: (bindable, oldValue, newValue) => (bindable as Calendar).ChangeBorderColor((Color)newValue, (Color)oldValue));

        protected void ChangeBorderColor(Color newValue, Color oldValue)
        {
            if (newValue == oldValue) return;
            _mainCalendars.ForEach(obj => obj.BackgroundColor = newValue);
            _buttons.FindAll(b => b.IsEnabled && !b.IsSelected).ForEach(b => b.TintBorderColor = newValue);
        }

        /// <summary>
        /// Gets or sets the border color of the calendar.
        /// </summary>
        /// <value>The color of the border.</value>
        public Color BorderColor
        {
            get => (Color)GetValue(BorderColorProperty);
            set => SetValue(BorderColorProperty, value);
        }

        #endregion

        #region DatesBackgroundColor

        public static readonly BindableProperty DatesBackgroundColorProperty =
            BindableProperty.Create(nameof(DatesBackgroundColor), typeof(Color), typeof(Calendar), Color.White,
                                    propertyChanged: (bindable, oldValue, newValue) => (bindable as Calendar).ChangeDatesBackgroundColor((Color)newValue, (Color)oldValue));

        protected void ChangeDatesBackgroundColor(Color newValue, Color oldValue)
        {
            if (newValue == oldValue) return;
            _buttons.FindAll(b => b.IsEnabled && (!b.IsSelected || SelectedBackgroundColor != Color.Default)).ForEach(b => b.TintColor = newValue);
        }

        /// <summary>
        /// Gets or sets the background color of the normal dates.
        /// </summary>
        /// <value>The color of the dates background.</value>
        public Color DatesBackgroundColor
        {
            get => (Color)GetValue(DatesBackgroundColorProperty);
            set => SetValue(DatesBackgroundColorProperty, value);
        }

        #endregion

        #region DatesTextColor

        public static readonly BindableProperty DatesTextColorProperty =
            BindableProperty.Create(nameof(DatesTextColor), typeof(Color), typeof(Calendar), Color.Black,
                                    propertyChanged: (bindable, oldValue, newValue) => (bindable as Calendar).ChangeDatesTextColor((Color)newValue, (Color)oldValue));

        protected void ChangeDatesTextColor(Color newValue, Color oldValue)
        {
            if (newValue == oldValue) return;
            _buttons.FindAll(b => b.IsEnabled && (!b.IsSelected || SelectedTextColor != Color.Default) && !b.IsOutOfMonth).ForEach(b => b.TextColor = newValue);
        }

        /// <summary>
        /// Gets or sets the text color of the normal dates.
        /// </summary>
        /// <value>The color of the dates text.</value>
        public Color DatesTextColor
        {
            get => (Color)GetValue(DatesTextColorProperty);
            set => SetValue(DatesTextColorProperty, value);
        }

        #endregion

        #region DatesFontAttributes

        public static readonly BindableProperty DatesFontAttributesProperty =
            BindableProperty.Create(nameof(DatesFontAttributes), typeof(FontAttributes), typeof(Calendar), FontAttributes.None,
                                    propertyChanged: (bindable, oldValue, newValue) => (bindable as Calendar).ChangeDatesFontAttributes((FontAttributes)newValue, (FontAttributes)oldValue));

        protected void ChangeDatesFontAttributes(FontAttributes newValue, FontAttributes oldValue)
        {
            if (newValue == oldValue) return;
            _buttons.FindAll(b => b.IsEnabled && (!b.IsSelected || SelectedTextColor != Color.Default) && !b.IsOutOfMonth).ForEach(b => b.FontAttributes = newValue);
        }

        /// <summary>
        /// Gets or sets the dates font attributes.
        /// </summary>
        /// <value>The dates font attributes.</value>
        public FontAttributes DatesFontAttributes
        {
            get => (FontAttributes)GetValue(DatesFontAttributesProperty);
            set => SetValue(DatesFontAttributesProperty, value);
        }

        #endregion

        #region DatesFontSize

        public static readonly BindableProperty DatesFontSizeProperty =
            BindableProperty.Create(nameof(DatesFontSize), typeof(double), typeof(Calendar), 20.0,
                                    propertyChanged: (bindable, oldValue, newValue) => (bindable as Calendar).ChangeDatesFontSize((double)newValue, (double)oldValue));

        protected void ChangeDatesFontSize(double newValue, double oldValue)
        {
            if (Math.Abs(newValue - oldValue) < 0.01) return;
            _buttons?.FindAll(b => !b.IsSelected && b.IsEnabled).ForEach(b => b.FontSize = newValue);
        }

        /// <summary>
        /// Gets or sets the font size of the normal dates.
        /// </summary>
        /// <value>The size of the dates font.</value>
        public double DatesFontSize
        {
            get => (double)GetValue(DatesFontSizeProperty);
            set => SetValue(DatesFontSizeProperty, value);
        }

        #endregion

        #region DatesFontFamily

        public static readonly BindableProperty DatesFontFamilyProperty =
                    BindableProperty.Create(nameof(DatesFontFamily), typeof(string), typeof(Calendar), default(string),
                                    propertyChanged: (bindable, oldValue, newValue) => (bindable as Calendar).ChangeDatesFontFamily((string)newValue, (string)oldValue));

        protected void ChangeDatesFontFamily(string newValue, string oldValue)
        {
            if (newValue == oldValue) return;
            _buttons?.FindAll(b => !b.IsSelected && b.IsEnabled).ForEach(b => b.FontFamily = newValue);
        }

        /// <summary>
        /// Gets or sets the font family of dates.
        /// </summary>
        public string DatesFontFamily
        {
            get => GetValue(DatesFontFamilyProperty) as string;
            set => SetValue(DatesFontFamilyProperty, value);
        }

        #endregion

        #region ShowNumOfMonths

        public static readonly BindableProperty ShowNumOfMonthsProperty =
            BindableProperty.Create(nameof(ShowNumOfMonths), typeof(int), typeof(Calendar), 1,
                                    propertyChanged: (bindable, oldValue, newValue) => (bindable as Calendar).ChangeCalendar(CalandarChanges.All));

        /// <summary>
        /// Gets or sets a the number of months to show
        /// </summary>
        /// <value>The start date.</value>
        public int ShowNumOfMonths
        {
            get => (int)GetValue(ShowNumOfMonthsProperty);
            set => SetValue(ShowNumOfMonthsProperty, value);
        }

        #endregion

        #region ShowInBetweenMonthLabels

        public static readonly BindableProperty ShowInBetweenMonthLabelsProperty =
            BindableProperty.Create(nameof(ShowInBetweenMonthLabels), typeof(bool), typeof(Calendar), true,
                                    propertyChanged: (bindable, oldValue, newValue) => (bindable as Calendar).ChangeCalendar(CalandarChanges.All));

        /// <summary>
        /// Gets or sets a the number of months to show
        /// </summary>
        /// <value>The start date.</value>
        public bool ShowInBetweenMonthLabels
        {
            get => (bool)GetValue(ShowInBetweenMonthLabelsProperty);
            set => SetValue(ShowInBetweenMonthLabelsProperty, value);
        }

        #endregion

        #region DateCommand

        public static readonly BindableProperty DateCommandProperty =
            BindableProperty.Create(nameof(DateCommand), typeof(ICommand), typeof(Calendar), null);

        /// <summary>
        /// Gets or sets the selected date command.
        /// </summary>
        /// <value>The date command.</value>
        public ICommand DateCommand
        {
            get => (ICommand)GetValue(DateCommandProperty);
            set => SetValue(DateCommandProperty, value);
        }

        #endregion

        #region CalendarSwipeProperties

        public static readonly BindableProperty EnableSwipingProperty =
            BindableProperty.Create(nameof(EnableSwipingProperty), typeof(bool), typeof(Calendar), false);

        /// <summary>
        /// Gets or sets if the calendar swiping is enabled.
        /// </summary>
        /// <value>The swiping enabled property</value>
        public bool EnableSwiping
        {
            get => (bool)GetValue(EnableSwipingProperty);
            set => SetValue(EnableSwipingProperty, value);
        }

        public static readonly BindableProperty IsSwipingAnimatedProperty =
            BindableProperty.Create(nameof(IsSwipingAnimated), typeof(bool), typeof(Calendar), false);

        /// <summary>
        /// Gets or sets if the calendar swiping is enabled.
        /// </summary>
        /// <value>The swiping enabled property</value>
        public bool IsSwipingAnimated
        {
            get => (bool)GetValue(IsSwipingAnimatedProperty);
            set => SetValue(IsSwipingAnimatedProperty, value);
        }

        public static readonly BindableProperty LeftSwipeCommandProperty =
            BindableProperty.Create(nameof(LeftSwipeCommand), typeof(ICommand), typeof(Calendar), null);

        /// <summary>
        /// The right swipe command for the calander navigation.
        /// </summary>
        /// <value>The Left swipe command</value>
        public ICommand LeftSwipeCommand
        {
            get => (ICommand)GetValue(LeftSwipeCommandProperty);
            set => SetValue(LeftSwipeCommandProperty, value);
        }

        public static readonly BindableProperty RightSwipeCommandProperty =
            BindableProperty.Create(nameof(RightSwipeCommand), typeof(ICommand), typeof(Calendar), null);

        /// <summary>
        /// The right swipe command for the calendar navigation.
        /// </summary>
        /// <value>The Right swipe command</value>
        public ICommand RightSwipeCommand
        {
            get => (ICommand)GetValue(RightSwipeCommandProperty);
            set => SetValue(RightSwipeCommandProperty, value);
        }

        #endregion

        public DateTime CalendarStartDate(DateTime date)
        {
            var start = date;
            var beginOfMonth = start.Day == 1;
            while (!beginOfMonth || start.DayOfWeek != StartDay)
            {
                start = start.AddDays(-1);
                beginOfMonth |= start.Day == 1;
            }
            return start;
        }

        #region Functions

        protected override void OnParentSet()
        {
            FillCalendarWindows();
            base.OnParentSet();
            ChangeCalendar(CalandarChanges.All);
        }

        protected Task FillCalendar()
        {
            return Task.Factory.StartNew(FillCalendarWindows);
        }

        protected void FillCalendarWindows()
        {
            CreateWeeknumbers();
            CreateButtons();
            ShowHideElements();
        }

        protected void CreateWeeknumbers()
        {
            _weekNumberLabels.Clear();
            _weekNumbers.Clear();
            if (!ShowNumberOfWeek) return;

            for (var i = 0; i < ShowNumOfMonths; i++)
            {
                var columDef = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
                var rowDef = new RowDefinition { Height = new GridLength(1, GridUnitType.Star) };
                var weekNumbers = new Grid { VerticalOptions = LayoutOptions.FillAndExpand, HorizontalOptions = LayoutOptions.Start, RowSpacing = 0, ColumnSpacing = 0, Padding = new Thickness(0, 0, 0, 0) };
                weekNumbers.ColumnDefinitions = new ColumnDefinitionCollection { columDef };
                weekNumbers.RowDefinitions = new RowDefinitionCollection { rowDef, rowDef, rowDef, rowDef, rowDef, rowDef };
                weekNumbers.WidthRequest = NumberOfWeekFontSize * (Device.RuntimePlatform == Device.iOS ? 1.5 : 2.5);


                for (int r = 0; r < 6; r++)
                {

                    _weekNumberLabels.Add(new Label
                    {
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        VerticalOptions = LayoutOptions.FillAndExpand,
                        TextColor = NumberOfWeekTextColor,
                        BackgroundColor = NumberOfWeekBackgroundColor,
                        VerticalTextAlignment = TextAlignment.Center,
                        HorizontalTextAlignment = TextAlignment.Center,
                        FontSize = NumberOfWeekFontSize,
                        FontAttributes = NumberOfWeekFontAttributes,
                        FontFamily = NumberOfWeekFontFamily
                    });
                    weekNumbers.Children.Add(_weekNumberLabels.Last(), 0, r);
                }
                _weekNumbers.Add(weekNumbers);
            }
        }

        protected void CreateButtons()
        {
            var columDef = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
            var rowDef = new RowDefinition { Height = new GridLength(1, GridUnitType.Star) };
            _buttons.Clear();
            _mainCalendars.Clear();
            for (var i = 0; i < ShowNumOfMonths; i++)
            {
                var mainCalendar = new Grid { VerticalOptions = LayoutOptions.FillAndExpand, HorizontalOptions = LayoutOptions.FillAndExpand, RowSpacing = GridSpace, ColumnSpacing = GridSpace, Padding = 1, BackgroundColor = BorderColor };
                mainCalendar.ColumnDefinitions = new ColumnDefinitionCollection { columDef, columDef, columDef, columDef, columDef, columDef, columDef };
                mainCalendar.RowDefinitions = new RowDefinitionCollection { rowDef, rowDef, rowDef, rowDef, rowDef, rowDef };

                for (int r = 0; r < 6; r++)
                {
                    for (int c = 0; c < 7; c++)
                    {
                        _buttons.Add(new CalendarButton
                        {
                            BorderRadius = 0,
                            BorderWidth = BorderWidth,
                            TintBorderColor = BorderColor,
                            FontSize = DatesFontSize,
                            TintColor = DatesBackgroundColor,
                            TextColor = DatesTextColor,
                            FontAttributes = DatesFontAttributes,
                            FontFamily = DatesFontFamily,
                            HorizontalOptions = LayoutOptions.FillAndExpand,
                            VerticalOptions = LayoutOptions.FillAndExpand
                        });
                        var b = _buttons.Last();
                        b.Clicked += DateClickedEvent;
                        mainCalendar.Children.Add(b, c, r);
                    }
                }
                _mainCalendars.Add(mainCalendar);
            }
        }

        public void ForceRedraw()
        {
            ChangeCalendar(CalandarChanges.All);
        }

        protected void ChangeCalendar(CalandarChanges changes)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                OnStartRenderCalendar?.Invoke(this, EventArgs.Empty);
                Content = null;
                if (changes.HasFlag(CalandarChanges.StartDate))
                {
                    TitleLabel.Text = StartDate.ToString(TitleLabelFormat);
                    if (_titleLabels != null)
                    {
                        var tls = StartDate.AddMonths(1);
                        foreach (var tl in _titleLabels)
                        {
                            tl.Text = tls.ToString(TitleLabelFormat);
                            tls = tls.AddMonths(1);
                        }
                    }
                }

                var start = CalendarStartDate(StartDate).Date;
                var beginOfMonth = false;
                var endOfMonth = false;
                for (int i = 0; i < _buttons.Count; i++)
                {
                    endOfMonth |= beginOfMonth && start.Day == 1;
                    beginOfMonth |= start.Day == 1;

                    if (i < _dayLabels.Count && WeekdaysShow && changes.HasFlag(CalandarChanges.StartDay))
                    {
                        _dayLabels[i].Text = start.ToString(WeekdaysFormat);
                    }

                    ChangeWeekNumbers(start, i);

                    if (changes.HasFlag(CalandarChanges.All))
                    {
                        _buttons[i].Text = $"{start.Day}";
                    }
                    else
                    {
                        _buttons[i].TextWithoutMeasure = $"{start.Day}";
                    }
                    _buttons[i].Date = start;

                    _buttons[i].IsOutOfMonth = !(beginOfMonth && !endOfMonth);
                    _buttons[i].IsEnabled = ShowNumOfMonths == 1 || !_buttons[i].IsOutOfMonth;

                    SpecialDate sd = null;
                    if (SpecialDates != null)
                    {
                        sd = SpecialDates.FirstOrDefault(s => s.Date.Date == start.Date);
                    }

                    SetButtonNormal(_buttons[i]);

                    if ((MinDate.HasValue && start < MinDate) || (MaxDate.HasValue && start > MaxDate) || (DisableAllDates && sd == null))
                    {
                        SetButtonDisabled(_buttons[i]);
                    }
                    else if (_buttons[i].IsEnabled && SelectedDates.Select(d => d.Date).Contains(start.Date))
                    {
                        SetButtonSelected(_buttons[i], sd);
                    }
                    else if (sd != null)
                    {
                        SetButtonSpecial(_buttons[i], sd);
                    }

                    start = start.AddDays(1);
                    if (i != 0 && (i + 1) % 42 == 0)
                    {
                        beginOfMonth = false;
                        endOfMonth = false;
                        start = CalendarStartDate(start);
                    }

                }
                if (DisableDatesLimitToMaxMinRange)
                {
                    TitleLeftArrow.IsEnabled = !(MinDate.HasValue && CalendarStartDate(StartDate).Date < MinDate);
                    TitleRightArrow.IsEnabled = !(MaxDate.HasValue && start > MaxDate);
                }
                Content = _mainView;
                OnEndRenderCalendar?.Invoke(this, EventArgs.Empty);
            });
        }

        protected void SetButtonNormal(CalendarButton button)
        {
            button.BackgroundPattern = null;
            button.BackgroundImage = null;

            Device.BeginInvokeOnMainThread(() =>
            {
                button.IsEnabled = true;
                button.IsSelected = false;
                button.FontSize = DatesFontSize;
                button.BorderWidth = BorderWidth;
                button.TintBorderColor = BorderColor;
                button.FontFamily = button.IsOutOfMonth ? DatesFontFamilyOutsideMonth : DatesFontFamily;
                button.TintColor = button.IsOutOfMonth ? DatesBackgroundColorOutsideMonth : DatesBackgroundColor;

                button.TextColor = button.IsOutOfMonth ? DatesTextColorOutsideMonth : DatesTextColor;
                button.FontAttributes = button.IsOutOfMonth ? DatesFontAttributesOutsideMonth : DatesFontAttributes;
                button.IsEnabled = ShowNumOfMonths == 1 || !button.IsOutOfMonth;
            });
        }

        protected void DateClickedEvent(object s, EventArgs a)
        {
            var selectedDate = (s as CalendarButton).Date;
            if (SelectedDate.HasValue && selectedDate.HasValue && SelectedDate.Value == selectedDate.Value)
            {
                ChangeSelectedDate(selectedDate);
                SelectedDate = null;
            }
            else
            {
                SelectedDate = selectedDate;
            }
        }

        #endregion

        public event EventHandler<DateTimeEventArgs> DateClicked;
    }
}
