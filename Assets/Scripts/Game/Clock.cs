using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using KingdomsSharedCode.Generic;

public class Clock
{
    public interface IClockReceiver { void RegisterClockReceiver(); }
    public interface IDaily : IClockReceiver { void OnNewDay(); }
    public interface IMonthly : IClockReceiver { void OnNewMonth(); }
    public interface IYearly : IClockReceiver { void OnNewYear(); }

    #region Calendar-related classes
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
    #endregion

    public event Action onCalculationEnd;

    int minBeatDuration = 200; // Duration of a BEAT in mili seconds
    int beatsPerDay = 5; // How many beats in a day
    bool isPaused = false;
    Thread beatThread;
    Dictionary<byte, List<Action>> beatActions = new Dictionary<byte, List<Action>>();
    ushort currentBeat = 0;

    int currentBeatsInCurrentDay = 0;
    
    int daysInAYear = 365;
    int monthsInAYear = 12;

    List<Day> days = new List<Day>();
    List<Month> months = new List<Month>();
    Date currentDate;
    List<Thread> threads = new List<Thread>();
    List<IClockReceiver> clockEventsReceivers = new List<IClockReceiver>();

    public Clock()
    {
        beatThread = new Thread((ThreadStart)async delegate {
            
            while (true)
            {
                if (isPaused) continue;
                await Task.Delay(minBeatDuration);

                if (isPaused) continue;
                
                await AdvanceBeat();
                ExecuteBeat();
            }
        });
    }

    public void Start(ushort beat = 0)
    {
        currentBeat = beat;
        beatThread.Start();
        Play();
    }

    public void Pause()
    {
        isPaused = true;
    }

    public void Play()
    {
        isPaused = false;
    }

    async Task AdvanceBeat()
    {
        currentBeatsInCurrentDay++;
        await BlockUntilCalculationsFinished();

        if (currentBeatsInCurrentDay >= beatsPerDay)
        {
            currentBeatsInCurrentDay = 0;
            AdvanceDate();
        }
        currentBeat++;
    }

    void ExecuteBeat()
    {
        if (beatActions.ContainsKey(GetByteBeat()) && beatActions[GetByteBeat()] != null)
        {
            foreach (var act in beatActions[GetByteBeat()])
                act.Invoke();

            beatActions[GetByteBeat()].Clear();
        }
    }

    public byte GetByteBeat()
    {
        return currentBeat.ToByte();
    }

    public ushort GetBeat()
    {
        return currentBeat;
    }

    public void SetBeatsPerDay(int beatsPerDay)
    {
        this.beatsPerDay = beatsPerDay;
    }

    public void SetCalendar(int daysInAYear, int monthsInAYear)
    {
        this.daysInAYear = daysInAYear;
        this.monthsInAYear = monthsInAYear;

        MakeCalendar();
    }
    
    public void RegisterClockReceiver(IClockReceiver eventReceiver)
    {
        clockEventsReceivers.Add(eventReceiver);
    }

    void MakeCalendar()
    {
        months.Clear();
        days.Clear();
        for (int i = 0; i < daysInAYear; i++) {
            var currentMonthIndex = KMaths.FloorToInt((i / ((float)daysInAYear)) * monthsInAYear);
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

    async Task BlockUntilCalculationsFinished()
    {
        while (AreCalculationsStillRunning())
        {
            await Task.Delay(10);
        }
    }

    public void AdvanceDate()
    {
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
                currentDate.day = currentDate.month.GetDay(currentDay);
                ExecuteInThread(ExecuteAllMonthlyEvents);
            }
        }
        else {
            currentDate.day = currentDate.month.GetDay(currentDay);
            ExecuteInThread(ExecuteAllDailyEvents);
        }
    }

    void CheckThreads()
    {
        foreach(var thread in threads.ToArray()) {
            if (!thread.IsAlive) {
                threads.RemoveAll(o => o == thread);
            }
        }
    }

    public bool AreCalculationsStillRunning()
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

    void ExecuteInThread(Action function)
    {
        var dailyThread = new Thread(new ThreadStart(function));
        dailyThread.IsBackground = true;
        dailyThread.Start();
        threads.Add(dailyThread);
    }

    void ExecuteAllDailyEvents()
    {
        SanitizeClockReceivers();
        try
        {
            foreach (var receiver in clockEventsReceivers) {
                if (receiver is IDaily) {
                    (receiver as IDaily).OnNewDay();
                }
            }
        }
        catch (MoonSharp.Interpreter.InterpreterException e)
        {
            GameLogger.logger.Error(e.DecoratedMessage);
            UnityEngine.Debug.LogError(e.DecoratedMessage);
        }
        if (onCalculationEnd!=null) onCalculationEnd.Invoke();
    }

    void ExecuteAllMonthlyEvents()
    {
        SanitizeClockReceivers();
        try { 
            foreach (var receiver in clockEventsReceivers) {
                if (receiver is IMonthly) {
                    (receiver as IMonthly).OnNewMonth();
                }
            }
        }
        catch (MoonSharp.Interpreter.InterpreterException e)
        {
            GameLogger.logger.Error(e.DecoratedMessage);
            UnityEngine.Debug.LogError(e.DecoratedMessage);
        }
        ExecuteAllDailyEvents();
    }

    void ExecuteAllYearlyEvents()
    {
        SanitizeClockReceivers();
        try
        {
            foreach (var receiver in clockEventsReceivers) {
                if (receiver is IYearly) {
                    (receiver as IYearly).OnNewYear();
                }
            }
        }
        catch (MoonSharp.Interpreter.InterpreterException e)
        {
            GameLogger.logger.Error(e.DecoratedMessage);
            UnityEngine.Debug.LogError(e.DecoratedMessage);
        }
        ExecuteAllMonthlyEvents();
    }

    public Date GetDate()
    {
        return currentDate;
    }

    public ushort GetNextPlannableBeat()
    {
        return (ushort)(currentBeat + Game.networkClient.BEAT_FUTURE_GAP);
    }

    public void Plan(ushort beat, Action action)
    {
        var bb = beat.ToByte();
        if (!beatActions.ContainsKey(bb) || beatActions[bb] == null) beatActions[bb] = new List<Action>();
        beatActions[bb].Add(action);
    }

    public void Kill()
    {
        beatThread.Abort();
    }
}
