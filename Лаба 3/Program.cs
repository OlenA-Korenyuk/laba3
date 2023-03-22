
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;

using Newtonsoft.Json;
using System.IO;

class Schedule
{
    private List<ScheduleItem> items;

    public Schedule()
    {
        items = new List<ScheduleItem>();
    }

    public void AddItem(ScheduleItem newItem)
    {
        if (CanAddItem(newItem))
        {
            items.Add(newItem);
        }
        else
        {
            throw new InvalidOperationException("Додати запис у розклад неможливо");
        }
    }

    public void RemoveItem(ScheduleItem itemToRemove)
    {
        items.Remove(itemToRemove);
    }

    public void ReplaceItem(ScheduleItem itemToReplace, ScheduleItem newItem)
    {
        int index = items.IndexOf(itemToReplace);
        if (index >= 0 && CanAddItem(newItem))
        {
            items[index] = newItem;
        }
        else
        {
            throw new InvalidOperationException("Замінити запис неможливо");
        }
    }

    public void InsertItem(ScheduleItem newItem, DateTime startTime)
    {
        int index = items.FindIndex(item => item.StartTime >= startTime);
        if (index >= 0 && CanAddItem(newItem))
        {
            items.Insert(index, newItem);
        }
        else
        {
            throw new InvalidOperationException("Вставити рзапис в розклад неможливо");
        }
    }

    public void Clear()
    {
        items.Clear();
    }

    public List<ScheduleItem> GetItemsByDate(DateTime date)
    {
        return items.FindAll(item => item.StartTime.Date == date.Date);
    }

    public List<ScheduleItem> GetItemsByLocation(string location)
    {
        return items.FindAll(item => item.Location == location);
    }

    public List<ScheduleItem> GetIntersectingItems(DateTime startTime, TimeSpan duration)
    {
        DateTime endTime = startTime.Add(duration);
        return items.FindAll(item => item.StartTime < endTime && item.EndTime > startTime);
    }

    public bool CanAddItem(ScheduleItem newItem)
    {
        DateTime startTime = newItem.StartTime;
        DateTime endTime = newItem.EndTime;
        foreach (ScheduleItem item in items)
        {
            if (item.StartTime < endTime && item.EndTime > startTime)
            {
                return false;
            }
        }
        return true;
    }
    

    
}

class ScheduleItem
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Location { get; set; }
    public string Text { get; set; }

    public ScheduleItem(DateTime startTime, DateTime endTime, string location, string text)
    {
        StartTime = startTime;
        EndTime = endTime;
        Location = location;
        Text = text;
    }
}

class Program
{
    static void Main(string[] args)
    {
        Schedule schedule = new Schedule();

        ScheduleItem item1 = new ScheduleItem(new DateTime(2023, 3, 21, 9, 0, 0), new DateTime(2023, 3, 21, 10, 0, 0), "Кабінет 112", "Конференція");
        ScheduleItem item2 = new ScheduleItem(new DateTime(2023, 3, 21, 10, 0, 0), new DateTime(2023, 3, 21, 11, 0, 0), "Кабінет 115", "Презентація проекту");
        ScheduleItem item3 = new ScheduleItem(new DateTime(2023, 3, 21, 11, 0, 0), new DateTime(2023, 3, 21, 12, 0, 0), "Кабінет 113", "Обід");
        
        schedule.AddItem(item1);
        schedule.AddItem(item2);
        schedule.AddItem(item3);
        

        // Виведення розкладу на екран
        Console.WriteLine("Розклад:");
        foreach (ScheduleItem item in schedule.GetItemsByDate(new DateTime(2023, 3, 21)))
        {
            Console.WriteLine($"{item.StartTime.ToString("HH:mm")} - {item.EndTime.ToString("HH:mm")}: {item.Text} ({item.Location})");
        }

        // Пошук записів за місцем
        List<ScheduleItem> itemsInRoom2 = schedule.GetItemsByLocation("Кабінет 112");
        Console.WriteLine("\nЗаписи у кабінеті 112:");
        foreach (ScheduleItem item in itemsInRoom2)
        {
            Console.WriteLine($"{item.StartTime.ToString("HH:mm")} - {item.EndTime.ToString("HH:mm")}: {item.Text}");
        }

        // Перевірка можливості вставки нового пункту
        ScheduleItem newItem = new ScheduleItem(new DateTime(2023, 3, 21, 12, 30, 0), new DateTime(2023, 3, 21, 13, 30, 0), "Room 1", "Break");
        bool canInsert = schedule.CanAddItem(newItem);
        Console.WriteLine($"\nЗапис можливо вставити?: {canInsert}");

        // Пошук перетинів
        List<ScheduleItem> intersectingItems = schedule.GetIntersectingItems(new DateTime(2023, 3, 21, 9, 30, 0), TimeSpan.FromHours(1));
        Console.WriteLine("\nЗаписи, що перетинаються:");
        foreach (ScheduleItem item in intersectingItems)
        {
            Console.WriteLine($"{item.StartTime.ToString("HH:mm")} - {item.EndTime.ToString("HH:mm")}: {item.Text}");
        }
        
        Console.WriteLine();
        
        
        ScheduleItem NewItem = new ScheduleItem(new DateTime(2023, 3, 21, 12, 30, 0), new DateTime(2023, 3, 21, 13, 30, 0), "Кабінет 114", "Перерва");
        schedule.ReplaceItem(item1, NewItem);
        
        Console.WriteLine("Заміна розкладу");
        foreach (ScheduleItem item in schedule.GetItemsByDate(new DateTime(2023, 3, 21)))
        {
            Console.WriteLine($" {item.StartTime.ToString("HH:mm")} - {item.EndTime.ToString("HH:mm")}: {item.Text} ({item.Location})");
        }

        string json = JsonConvert.SerializeObject(schedule.GetItemsByDate(new DateTime(2023, 3, 21)));
        File.WriteAllText("schedule.json", json);
        Console.WriteLine($"\nРезультат було записано у файл schedule.json.");
        Console.WriteLine();

        Schedule schedule2 = new Schedule();
        string jsonString = File.ReadAllText("schedule.json");
        List<ScheduleItem> items = System.Text.Json.JsonSerializer.Deserialize<List<ScheduleItem>>(jsonString);

        // Додавання елементів до розкладу
        foreach (ScheduleItem item in items)
        {
            schedule2.AddItem(item);
        }

        // Виведення розкладу на екран
        Console.WriteLine("Розклад:");
        foreach (ScheduleItem item in schedule2.GetItemsByDate(new DateTime(2023, 3, 21)))
        {
            Console.WriteLine($"{item.StartTime.ToString("HH:mm")} - {item.EndTime.ToString("HH:mm")}: {item.Text} ({item.Location})");
        }

    }
}