using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clock
{
    public interface IClockReceiver { void RegisterClockReceiver(); }
    public interface IDaily : IClockReceiver { void OnNewDay(); }
    public interface IMonthly : IClockReceiver { void OnNewMonth(); }
    public interface IYearly : IClockReceiver { void OnNewYear(); }

    public class Date
    {
        public int year;
        public Month month;
        public Day day;

        public override string ToString()
        {
            return day + "/" + month + "/" + year;
        }
    }

    public class Month {
        public readonly int index;
        List<Day> days = new List<Day>();
        
        public Month (int index)
        {
            this.index = index;
        }

        public void AddDay(Day day)
        {
            day.month = this;
            days.Add(day);
        }

        public int GetDayCount()
        {
            return days.Count;
        }

        public Day GetDay(int index)
        {
            return days[index];
        }

        public int GetDayLocalIndex(Day day)
        {
            return days.FindIndex(o=>o==day);
        }

        public override string ToString()
        {
            return (index+1).ToString();
        }
    }

    public class Day {
        public Month month;
        public readonly int index;
        public Day(int index) { this.index = index; }

        public override string ToString()
        {
            return (month.GetDayLocalIndex(this) + 1).ToString();
        }
    }


    float timeScale = 1f; // Duration of a day in real seconds
    int daysInAYear = 365;
    int monthsInAYear = 12;
    float yearDurationInSeconds = 100f;
    List<Day> days = new List<Day>();
    List<Month> months = new List<Month>();
    Date currentDate;
    bool isRunning = false;
    List<Thread> threads = new List<Thread>();
    List<IClockReceiver> clockEventsReceivers = new List<IClockReceiver>();

    public Clock(int daysInAYear, int monthsInAYear)
    {
        this.daysInAYear = daysInAYear;
        this.monthsInAYear = monthsInAYear;

        MakeCalendar();

        RecalculateScale();
    }
    
    public void RegisterClockReceiver(IClockReceiver eventReceiver)
    {
        clockEventsReceivers.Add(eventReceiver);
    }

    void RecalculateScale()
    {
        timeScale = yearDurationInSeconds / daysInAYear;
    }

    public float GetTimeScale()
    {
        return timeScale;
    }

    public void SetYearDurationSeconds(float yds)
    {
        yearDurationInSeconds = yds;
        RecalculateScale();
    }

    void MakeCalendar()
    {
        months.Clear();
        days.Clear();
        for (int i = 0; i < daysInAYear; i++) {
            var currentMonthIndex = Mathf.FloorToInt((i / ((float)daysInAYear)) * monthsInAYear);
            if (months.Count <= currentMonthIndex) months.Add(new Month(currentMonthIndex));
            var month = months[currentMonthIndex];
            var day = new Day(i);
            month.AddDay(day);
            days.Add(day);
        }

        currentDate = new Date() {
            year = 1,
            month = months[0],
            day = months[0].GetDay(0)
        };
    }

    public bool Advance()
    {
        if (AreCalculationsStillRunning()) {
            return false;
        }

        int currentDay = currentDate.month.GetDayLocalIndex(currentDate.day);
        currentDay++;

        if (currentDay >= currentDate.month.GetDayCount()) {
            // New month
            var monthIndex = months.FindIndex(o => o == currentDate.month);
            monthIndex++;
            currentDay = 0;

            if (monthIndex >= months.Count) {
                // New year
                currentDate.year++;
                MakeCalendar();

                ExecuteInThread(ExecuteAllYearlyEvents);
            }
            else {
                currentDate.month = months[monthIndex];
            }
            ExecuteInThread(ExecuteAllMonthlyEvents);
        }
        currentDate.day = currentDate.month.GetDay(currentDay);
        ExecuteInThread(ExecuteAllDailyEvents);

        return true;
    }

    void CheckThreads()
    {
        foreach(var thread in threads.ToArray()) {
            if (!thread.IsAlive) {
                threads.RemoveAll(o => o == thread);
            }
        }
    }

    bool AreCalculationsStillRunning()
    {
        CheckThreads();
        if (threads.Count > 0) return true;
        return false;
    }

    void SanitizeClockReceivers()
    {
        foreach(var receiver in clockEventsReceivers.ToArray()) {
            if (receiver == null) clockEventsReceivers.RemoveAll(o=>o==receiver);
        }
    }

    void ExecuteInThread(System.Action function)
    {
        var dailyThread = new Thread(new ThreadStart(function));
        dailyThread.IsBackground = true;
        dailyThread.Start();
        threads.Add(dailyThread);
    }

    void ExecuteAllDailyEvents()
    {
        SanitizeClockReceivers();
        foreach(var receiver in clockEventsReceivers) {
            if (receiver is IDaily) {
                (receiver as IDaily).OnNewDay();
            }
        }
    }

    void ExecuteAllMonthlyEvents()
    {
        SanitizeClockReceivers();
        foreach (var receiver in clockEventsReceivers) {
            if (receiver is IMonthly) {
                (receiver as IMonthly).OnNewMonth();
            }
        }
    }

    void ExecuteAllYearlyEvents()
    {
        SanitizeClockReceivers();
        foreach (var receiver in clockEventsReceivers) {
            if (receiver is IYearly) {
                (receiver as IYearly).OnNewYear();
            }
        }
    }

    public Date GetDate()
    {
        return currentDate;
    }
}
